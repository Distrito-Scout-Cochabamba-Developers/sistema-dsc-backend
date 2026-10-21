using System.Text.Json;
using DotNetEnv;
using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Env.TraversePath().Load();

var seedDate = ResolveSeedDate(args);
var resetAllPasswords = args.Any(argument =>
    string.Equals(argument, "--reset-all-passwords", StringComparison.OrdinalIgnoreCase)
);
var password = Environment.GetEnvironmentVariable("DSC_SEED_PASSWORD") ?? ReadPassword();
var manifestPath = Path.Combine(AppContext.BaseDirectory, "dev-users.json");

if (!File.Exists(manifestPath))
{
    Console.Error.WriteLine($"No se encontró el manifiesto: {manifestPath}");
    return 1;
}

var manifest =
    JsonSerializer.Deserialize<SeedManifest>(
        await File.ReadAllTextAsync(manifestPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    ) ?? throw new InvalidOperationException("El manifiesto de usuarios está vacío.");

ValidateManifest(manifest);

var connectionString = ResolveConnectionString();
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine(
        "No se encontró cadena de conexión. Defina DATABASE_URL o ConnectionStrings__DefaultConnection."
    );
    return 1;
}

var services = new ServiceCollection();
services.AddInfrastructure(connectionString);
await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ScoutDbContext>();
var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

var activePersonStatus = await db.PersonStatuses.SingleOrDefaultAsync(x => x.Code == "ACTIVE");
var activeUserStatus = await db.UserStatuses.SingleOrDefaultAsync(x => x.Code == "ACTIVE");

if (activePersonStatus is null || activeUserStatus is null)
{
    Console.Error.WriteLine("Faltan los catálogos ACTIVE de person_status o user_status.");
    return 1;
}

var rolesByCode = await db.Roles.ToDictionaryAsync(
    role => role.Code,
    role => role,
    StringComparer.OrdinalIgnoreCase
);
var missingRoles = manifest
    .Users.SelectMany(user => user.Roles)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .Where(code => !rolesByCode.ContainsKey(code))
    .ToArray();

if (missingRoles.Length > 0)
{
    Console.Error.WriteLine($"Roles inexistentes en la BD: {string.Join(", ", missingRoles)}");
    return 1;
}

var createdPeople = 0;
var createdUsers = 0;
var createdAssignments = 0;
var reusedPeople = 0;
var reusedUsers = 0;
var resetPasswords = 0;

var strategy = db.Database.CreateExecutionStrategy();

try
{
    await strategy.ExecuteAsync(async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            if (resetAllPasswords)
            {
                var users = await db.Users.ToListAsync();
                foreach (var existingUser in users)
                {
                    existingUser.Password = hasher.Hash(password);
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    resetPasswords++;
                }

                await db.SaveChangesAsync();
                Console.WriteLine(
                    $"Contraseñas reseteadas con BCrypt: {resetPasswords} cuenta(s)."
                );
            }

            foreach (var seed in manifest.Users)
            {
                var idCard = seed.IdCard.Trim();
                var idCardExtension = string.IsNullOrWhiteSpace(seed.IdCardExtension)
                    ? null
                    : seed.IdCardExtension.Trim();
                var username = seed.Username.Trim();
                var email = seed.Email.Trim().ToLowerInvariant();

                var person = idCardExtension is null
                    ? await db.Persons.FirstOrDefaultAsync(p =>
                        p.IdCard == idCard && p.IdCardExtension == null
                    )
                    : await db.Persons.FirstOrDefaultAsync(p =>
                        p.IdCard == idCard && p.IdCardExtension == idCardExtension
                    );

                var usernameOwner = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
                var emailOwner = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (
                    usernameOwner is not null
                    && emailOwner is not null
                    && usernameOwner.Id != emailOwner.Id
                )
                    throw new InvalidOperationException(
                        $"El username '{username}' y el email '{email}' pertenecen a cuentas distintas."
                    );

                var user = usernameOwner ?? emailOwner;

                if (person is not null && user is null)
                {
                    user = await db.Users.FirstOrDefaultAsync(u => u.PersonId == person.Id);
                    if (
                        user is not null
                        && (
                            !string.Equals(
                                user.Username,
                                username,
                                StringComparison.OrdinalIgnoreCase
                            )
                            || !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                        throw new InvalidOperationException(
                            $"La persona con CI {idCard} ya tiene la cuenta '{user.Username}'."
                        );
                }

                if (user is not null && person is null)
                {
                    person = await db.Persons.FindAsync(user.PersonId);
                    if (person is null)
                        throw new InvalidOperationException(
                            $"La cuenta '{user.Username}' apunta a una persona inexistente."
                        );
                }

                if (user is not null && person is not null && user.PersonId != person.Id)
                    throw new InvalidOperationException(
                        $"El usuario '{username}' y la persona con CI {idCard} no coinciden."
                    );

                if (person is null)
                {
                    person = new Person
                    {
                        IdCard = idCard,
                        IdCardExtension = idCardExtension,
                        FirstName = seed.FirstName.Trim(),
                        MiddleName = NormalizeOptional(seed.MiddleName),
                        FirstLastName = seed.FirstLastName.Trim(),
                        SecondLastName = NormalizeOptional(seed.SecondLastName),
                        PersonStatusId = activePersonStatus.Id,
                        IsProfileComplete = false,
                    };
                    db.Persons.Add(person);
                    await db.SaveChangesAsync();
                    createdPeople++;
                    Console.WriteLine($"Persona creada: {person.FullName} (CI {idCard})");
                }
                else
                {
                    reusedPeople++;
                    Console.WriteLine($"Persona existente: {person.FullName} (CI {idCard})");
                }

                if (user is null)
                {
                    user = new User
                    {
                        PersonId = person.Id,
                        Username = username,
                        Email = email,
                        Password = hasher.Hash(password),
                        UserStatusId = activeUserStatus.Id,
                        IsEmailVerified = false,
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    createdUsers++;
                    Console.WriteLine($"Cuenta creada: {username} <{email}>");
                }
                else
                {
                    reusedUsers++;
                    Console.WriteLine($"Cuenta existente: {user.Username} <{user.Email}>");
                }

                foreach (var roleCode in seed.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var role = rolesByCode[roleCode];
                    var alreadyAssigned = await db.UserRoles.AnyAsync(userRole =>
                        userRole.UserId == user.Id && userRole.RoleId == role.Id
                    );

                    if (alreadyAssigned)
                        continue;

                    db.UserRoles.Add(
                        new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            ValidFrom = seedDate,
                            ValidUntil = null,
                            CreatedAt = DateTime.UtcNow,
                        }
                    );
                    createdAssignments++;
                    Console.WriteLine($"  Rol asignado: {role.Code}");
                }

                await db.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    });
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Carga abortada: {ex.Message}");
    return 1;
}

Console.WriteLine();
Console.WriteLine("Resumen de la carga");
Console.WriteLine($"  Personas creadas:       {createdPeople}");
Console.WriteLine($"  Personas reutilizadas:  {reusedPeople}");
Console.WriteLine($"  Cuentas creadas:        {createdUsers}");
Console.WriteLine($"  Cuentas reutilizadas:   {reusedUsers}");
Console.WriteLine($"  Roles asignados:        {createdAssignments}");
Console.WriteLine($"  Contraseñas reseteadas: {resetPasswords}");
Console.WriteLine($"  Fecha de inicio roles:  {seedDate:yyyy-MM-dd}");
return 0;

static DateOnly ResolveSeedDate(string[] args)
{
    var dateArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--date=", StringComparison.OrdinalIgnoreCase)
    );

    if (
        dateArgument is not null
        && DateOnly.TryParse(dateArgument["--date=".Length..], out var parsedDate)
    )
        return parsedDate;

    return DateOnly.FromDateTime(DateTime.Now);
}

static string ReadPassword()
{
    if (Console.IsInputRedirected)
        throw new InvalidOperationException(
            "Defina DSC_SEED_PASSWORD o ejecute el comando en una consola interactiva."
        );

    Console.Write("Contraseña inicial para las cuentas nuevas: ");
    var password = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
                password.Length--;
            continue;
        }

        if (!char.IsControl(key.KeyChar))
            password.Append(key.KeyChar);
    }

    if (password.Length == 0)
        throw new InvalidOperationException("La contraseña no puede estar vacía.");

    return password.ToString();
}

static string? NormalizeOptional(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : value.Trim();

static void ValidateManifest(SeedManifest manifest)
{
    if (manifest.Users.Count == 0)
        throw new InvalidOperationException("El manifiesto no contiene usuarios.");

    foreach (var user in manifest.Users)
    {
        if (
            string.IsNullOrWhiteSpace(user.FirstName)
            || string.IsNullOrWhiteSpace(user.FirstLastName)
            || string.IsNullOrWhiteSpace(user.IdCard)
            || string.IsNullOrWhiteSpace(user.Username)
            || string.IsNullOrWhiteSpace(user.Email)
        )
            throw new InvalidOperationException(
                "Todos los usuarios requieren firstName, firstLastName, idCard, username y email."
            );

        if (user.Roles.Count == 0)
            throw new InvalidOperationException($"El usuario '{user.Username}' no tiene roles.");
    }

    var duplicateUsernames = manifest
        .Users.GroupBy(user => user.Username, StringComparer.OrdinalIgnoreCase)
        .Where(group => group.Count() > 1)
        .Select(group => group.Key)
        .ToArray();
    var duplicateEmails = manifest
        .Users.GroupBy(user => user.Email, StringComparer.OrdinalIgnoreCase)
        .Where(group => group.Count() > 1)
        .Select(group => group.Key)
        .ToArray();
    var duplicateIdCards = manifest
        .Users.GroupBy(
            user => $"{user.IdCard}|{user.IdCardExtension}",
            StringComparer.OrdinalIgnoreCase
        )
        .Where(group => group.Count() > 1)
        .Select(group => group.Key)
        .ToArray();

    if (duplicateUsernames.Length > 0)
        throw new InvalidOperationException(
            $"Usernames duplicados en el manifiesto: {string.Join(", ", duplicateUsernames)}"
        );
    if (duplicateEmails.Length > 0)
        throw new InvalidOperationException(
            $"Emails duplicados en el manifiesto: {string.Join(", ", duplicateEmails)}"
        );
    if (duplicateIdCards.Length > 0)
        throw new InvalidOperationException(
            $"CI duplicados en el manifiesto: {string.Join(", ", duplicateIdCards)}"
        );
}

static string? ResolveConnectionString()
{
    var connectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    if (string.IsNullOrWhiteSpace(connectionString))
        return null;

    if (
        connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
    )
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var user = userInfo.Length > 0 ? userInfo[0] : "postgres";
        var pass = userInfo.Length > 1 ? userInfo[1] : string.Empty;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};Port={port};Database={database};Username={user};Password={pass};";
    }

    return connectionString;
}

internal sealed class SeedManifest
{
    public List<SeedUser> Users { get; set; } = [];
}

internal sealed class SeedUser
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FirstLastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public string IdCard { get; set; } = string.Empty;
    public string? IdCardExtension { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
