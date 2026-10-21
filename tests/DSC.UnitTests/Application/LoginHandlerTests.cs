using System.IdentityModel.Tokens.Jwt;
using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Enums;
using DSC.Domain.Exceptions;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace DSC.UnitTests.Application;

public class LoginHandlerTests
{
    private const string RawPassword = "SecurePassword123!";
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static ScoutDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ScoutDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ScoutDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    private sealed class SimpleTestConfiguration(Dictionary<string, string?> data) : IConfiguration
    {
        public string? this[string key]
        {
            get => data.TryGetValue(key, out var v) ? v : null;
            set => data[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() =>
            Enumerable.Empty<IConfigurationSection>();

        public IChangeToken GetReloadToken() => throw new NotImplementedException();

        public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
    }

    private static IConfiguration CreateTestConfiguration(int expiresMinutes = 60) =>
        new SimpleTestConfiguration(
            new Dictionary<string, string?>
            {
                { "Jwt:Key", "this-is-a-valid-test-secret-key-32bytes-long" },
                { "Jwt:Issuer", "DSC.Api" },
                { "Jwt:Audience", "DSC.Client" },
                { "Jwt:ExpiresMinutes", expiresMinutes.ToString() },
            }
        );

    private static async Task<User> SeedUserAsync(
        ScoutDbContext db,
        string username = "badenpowell",
        string email = "bp@scout.org",
        string? storedPassword = null,
        string statusCode = "ACTIVE",
        params RoleSeed[] roles
    )
    {
        var activeStatus = new UserStatus
        {
            Id = 1,
            Code = "ACTIVE",
            Name = "Activo",
        };
        var blockedStatus = new UserStatus
        {
            Id = 2,
            Code = "BLOCKED",
            Name = "Bloqueado",
        };
        db.UserStatuses.AddRange(activeStatus, blockedStatus);

        var personStatus = new PersonStatus
        {
            Id = 1,
            Code = "ACTIVE",
            Name = "Activo",
        };
        db.PersonStatuses.Add(personStatus);

        var person = new Person
        {
            Id = 1,
            FirstName = "Baden",
            FirstLastName = "Powell",
            IdCard = "1234567",
            Gender = GenderType.MALE,
            PersonStatusId = personStatus.Id,
        };
        db.Persons.Add(person);

        var roleEntities = roles
            .Select(
                (seed, index) =>
                    new Role
                    {
                        Id = (short)(index + 1),
                        Code = seed.Code,
                        Name = seed.Code,
                        IsActive = seed.IsActive,
                    }
            )
            .ToList();
        db.Roles.AddRange(roleEntities);

        var hasher = new BCryptPasswordHasher();
        var user = new User
        {
            Id = 1,
            Person = person,
            Username = username,
            Email = email,
            Password = storedPassword ?? hasher.Hash(RawPassword),
            UserStatusId = statusCode == "ACTIVE" ? activeStatus.Id : blockedStatus.Id,
            UserStatus = statusCode == "ACTIVE" ? activeStatus : blockedStatus,
            UserRoles = roleEntities
                .Select(
                    (role, index) =>
                        new UserRole
                        {
                            UserId = 1,
                            RoleId = role.Id,
                            Role = role,
                            ValidFrom = roles[index].ValidFromValue,
                            ValidUntil = roles[index].ValidUntil,
                        }
                )
                .ToList(),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    private sealed record RoleSeed(
        string Code,
        bool IsActive = true,
        DateOnly? ValidFrom = null,
        DateOnly? ValidUntil = null
    )
    {
        public DateOnly ValidFromValue => ValidFrom ?? Today.AddDays(-30);
    }

    private static LoginCommandHandler CreateHandler(
        ScoutDbContext db,
        IConfiguration? configuration = null
    ) =>
        new(
            new UserRepository(db),
            new BCryptPasswordHasher(),
            new JwtTokenGenerator(configuration ?? CreateTestConfiguration())
        );

    [Fact]
    public async Task Login_WithUsername_ShouldReturnTokenAndProfile()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            new LoginRequestDto("badenpowell", RawPassword),
            CancellationToken.None
        );

        Assert.NotEmpty(result.Token);
        Assert.Equal("badenpowell", result.User.Username);
        Assert.Equal(1, result.User.Id);
        Assert.Equal(1, result.User.PersonId);
    }

    [Fact]
    public async Task Login_WithEmail_ShouldReturnTokenAndProfile()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            new LoginRequestDto("BP@SCOUT.ORG", RawPassword),
            CancellationToken.None
        );

        Assert.NotEmpty(result.Token);
        Assert.Equal("badenpowell", result.User.Username);
        Assert.Equal("bp@scout.org", result.User.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrowInvalidCredentials()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db);
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(
                new LoginRequestDto("badenpowell", "WrongPassword"),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Login_WithUnknownIdentifier_ShouldThrowInvalidCredentials()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db);
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(new LoginRequestDto("nobody", RawPassword), CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ShouldThrowValidationError()
    {
        using var db = CreateInMemoryDbContext();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.HandleAsync(new LoginRequestDto("", ""), CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_WithBlockedUser_ShouldThrowInvalidCredentials()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db, statusCode: "BLOCKED");
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(
                new LoginRequestDto("badenpowell", RawPassword),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Login_WithLegacyPlaintextPassword_ShouldFail()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db, storedPassword: RawPassword);
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(
                new LoginRequestDto("badenpowell", RawPassword),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Login_ShouldEmitIdentityClaimsAndRoles()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db, roles: [new RoleSeed("ADMIN"), new RoleSeed("DISTRICT_DIRECTOR")]);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            new LoginRequestDto("badenpowell", RawPassword),
            CancellationToken.None
        );

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("1", jwt.Claims.Single(c => c.Type == "sub").Value);
        Assert.Equal("bp@scout.org", jwt.Claims.Single(c => c.Type == "email").Value);
        Assert.Equal("1", jwt.Claims.Single(c => c.Type == "personId").Value);
        Assert.Equal("badenpowell", jwt.Claims.Single(c => c.Type == "username").Value);

        var roles = jwt
            .Claims.Where(c => c.Type == "roles")
            .Select(c => c.Value)
            .OrderBy(value => value)
            .ToArray();
        Assert.Equal(new[] { "ADMIN", "DISTRICT_DIRECTOR" }, roles);
    }

    [Fact]
    public async Task Login_ShouldIncludeOnlyActiveAndCurrentlyValidRoles()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(
            db,
            roles:
            [
                new RoleSeed("ADMIN"),
                new RoleSeed("DISTRICT_DIRECTOR", ValidFrom: Today.AddDays(5)),
                new RoleSeed("DISTRICT_SECRETARY", ValidUntil: Today.AddDays(-1)),
                new RoleSeed("FINANCE_COMMISSIONER", IsActive: false),
                new RoleSeed("QUARTERMASTER", ValidUntil: Today),
            ]
        );
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            new LoginRequestDto("badenpowell", RawPassword),
            CancellationToken.None
        );

        var roles = new JwtSecurityTokenHandler()
            .ReadJwtToken(result.Token)
            .Claims.Where(c => c.Type == "roles")
            .Select(c => c.Value)
            .OrderBy(value => value)
            .ToArray();

        Assert.Equal(new[] { "ADMIN", "QUARTERMASTER" }, roles);
        Assert.Equal(roles, result.User.Roles.OrderBy(value => value).ToArray());
    }

    [Fact]
    public async Task Login_ShouldReturnExpirationMatchingConfiguration()
    {
        using var db = CreateInMemoryDbContext();
        await SeedUserAsync(db);
        var handler = CreateHandler(db, CreateTestConfiguration(expiresMinutes: 30));

        var before = DateTimeOffset.UtcNow.AddMinutes(29);
        var result = await handler.HandleAsync(
            new LoginRequestDto("badenpowell", RawPassword),
            CancellationToken.None
        );
        var after = DateTimeOffset.UtcNow.AddMinutes(31);

        Assert.InRange(result.ExpiresAt, before, after);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.InRange(jwt.ValidTo, before.UtcDateTime, after.UtcDateTime);
    }

    [Fact]
    public void Hasher_ShouldNotStorePlaintextAndShouldRejectWrongPassword()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash(RawPassword);

        Assert.NotEqual(RawPassword, hash);
        Assert.StartsWith("$2", hash);
        Assert.True(hasher.Verify(RawPassword, hash));
        Assert.False(hasher.Verify("WrongPassword", hash));
        Assert.False(hasher.Verify(RawPassword, RawPassword));
    }
}
