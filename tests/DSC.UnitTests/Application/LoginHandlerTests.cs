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
using Xunit;

namespace DSC.UnitTests.Application;

public class LoginHandlerTests
{
    private ScoutDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ScoutDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ScoutDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    private class SimpleTestConfiguration(Dictionary<string, string?> data) : IConfiguration
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

    private IConfiguration CreateTestConfiguration()
    {
        return new SimpleTestConfiguration(
            new Dictionary<string, string?>
            {
                { "Jwt:Key", "this-is-a-valid-test-secret-key-32bytes-long" },
                { "Jwt:Issuer", "DSC.Api" },
                { "Jwt:Audience", "DSC.Client" },
                { "Jwt:ExpiresMinutes", "60" },
            }
        );
    }

    private async Task<User> SeedActiveUserAsync(
        ScoutDbContext db,
        string username = "scoutmaster",
        string email = "master@scout.org",
        string rawPassword = "SecurePassword123!"
    )
    {
        var activeStatus = new UserStatus
        {
            Id = 1,
            Code = "ACTIVE",
            Name = "Activo",
        };
        db.UserStatuses.Add(activeStatus);

        var person = new Person
        {
            Id = 1,
            FirstName = "Baden",
            FirstLastName = "Powell",
            IdCard = "1234567",
            Gender = GenderType.MALE,
        };
        db.Persons.Add(person);

        var passwordHasher = new NormalPasswordValidation();
        var user = new User
        {
            Id = 1,
            PersonId = 1,
            Username = username,
            Email = email,
            Password = passwordHasher.Hash(rawPassword),
            UserStatusId = 1,
            UserStatus = activeStatus,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task LoginWithUsername_WithValidCredentials_ShouldReturnTokenAndProfile()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedActiveUserAsync(db, username: "badenpowell", rawPassword: "MySecretPassword123");

        var userRepo = new UserRepository(db);
        var passwordHasher = new NormalPasswordValidation();
        var jwtGenerator = new JwtTokenGenerator(CreateTestConfiguration());
        var handler = new LoginWithUsernameCommandHandler(userRepo, passwordHasher, jwtGenerator);

        var request = new LoginWithUsernameRequestDto(
            Username: "badenpowell",
            Password: "MySecretPassword123"
        );

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("badenpowell", result.User.Username);
        Assert.Equal(1, result.User.Id);
    }

    [Fact]
    public async Task LoginWithUsername_WithWrongPassword_ShouldThrowDomainException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedActiveUserAsync(db, username: "badenpowell", rawPassword: "MySecretPassword123");

        var userRepo = new UserRepository(db);
        var passwordHasher = new NormalPasswordValidation();
        var jwtGenerator = new JwtTokenGenerator(CreateTestConfiguration());
        var handler = new LoginWithUsernameCommandHandler(userRepo, passwordHasher, jwtGenerator);

        var request = new LoginWithUsernameRequestDto(
            Username: "badenpowell",
            Password: "WrongPassword"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request, CancellationToken.None)
        );

        Assert.Contains("inválidas", exception.Message);
    }

    [Fact]
    public async Task LoginWithUsername_WithEmptyCredentials_ShouldThrowDomainException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var userRepo = new UserRepository(db);
        var passwordHasher = new NormalPasswordValidation();
        var jwtGenerator = new JwtTokenGenerator(CreateTestConfiguration());
        var handler = new LoginWithUsernameCommandHandler(userRepo, passwordHasher, jwtGenerator);

        var request = new LoginWithUsernameRequestDto(Username: "", Password: "");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task LoginWithEmail_WithValidCredentials_ShouldReturnTokenAndProfileWithUsername()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedActiveUserAsync(
            db,
            username: "bp_leader",
            email: "bp@scout.org",
            rawPassword: "Password123!"
        );

        var userRepo = new UserRepository(db);
        var passwordHasher = new NormalPasswordValidation();
        var jwtGenerator = new JwtTokenGenerator(CreateTestConfiguration());
        var handler = new LoginCommandHandler(userRepo, passwordHasher, jwtGenerator);

        var request = new LoginRequestDto(Email: "bp@scout.org", Password: "Password123!");

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("bp_leader", result.User.Username);
        Assert.Equal("bp@scout.org", result.User.Email);
    }
}
