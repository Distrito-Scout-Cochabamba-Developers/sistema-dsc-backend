using System.Text;
using DotNetEnv;
using DSC.Api;
using DSC.Api.Conventions;
using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public partial class Program
{
    private static void Main(string[] args)
    {
        // 0. Load environment variables from .env file if present
        Env.TraversePath().Load();

        var builder = WebApplication.CreateBuilder(args);

        // 1. Resolve PostgreSQL Connection String securely from environment / .env / config
        var connectionString = ResolveConnectionString(builder.Configuration);

        // 2. Register Layer Services
        builder.Services.AddControllers(options =>
        {
            options.Conventions.Add(new GlobalRoutePrefixConvention(new RouteAttribute("api")));
        });
        builder.Services.AddOpenApi();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddApi();
        builder.Services.AddProblemDetails();
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
                    ),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token))
                            context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    },
                };
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // 3. Global Exception Handling RFC 7807 Problem Details
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();
        else
            app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // 4. Auto-apply migrations in Development when using a relational database
        if (app.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(connectionString))
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ScoutDbContext>();
            if (dbContext.Database.IsRelational())
            {
                try
                {
                    // Si la base de datos ya tenía tablas previas pero no el historial de migraciones, hacemos baseline automático
                    var hasExistingTables =
                        dbContext
                            .Database.SqlQueryRaw<int>(
                                @"SELECT COUNT(*)::int AS ""Value"" FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'branch'"
                            )
                            .AsEnumerable()
                            .FirstOrDefault() > 0;

                    if (hasExistingTables)
                    {
                        dbContext.Database.ExecuteSqlRaw(
                            @"
                            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                                ""MigrationId"" character varying(150) NOT NULL,
                                ""ProductVersion"" character varying(32) NOT NULL,
                                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                            );
                            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                            SELECT '20260826031552_InitialMigration', '9.0.2'
                            WHERE NOT EXISTS (
                                SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '20260826031552_InitialMigration'
                            );
                            -- Asegurar que la columna username y su índice existan si la tabla user fue creada previamente
                            ALTER TABLE ""user"" ADD COLUMN IF NOT EXISTS username VARCHAR(50);
                            DO $$
                            BEGIN
                                IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_user_username') THEN
                                    CREATE UNIQUE INDEX ""IX_user_username"" ON ""user"" (username);
                                END IF;
                            END
                            $$;
                        "
                        );
                    }

                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning(
                        ex,
                        "No se pudieron aplicar las migraciones automáticas en el arranque: {Message}",
                        ex.Message
                    );
                }
            }
        }

        app.Run();

        static string ResolveConnectionString(IConfiguration configuration)
        {
            // Option A: ConnectionStrings:DefaultConnection (from .env as ConnectionStrings__DefaultConnection or appsettings)
            var conn = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(conn))
                return conn;

            // Option B: DATABASE_URL or DB_CONNECTION_STRING
            conn = configuration["DATABASE_URL"] ?? configuration["DB_CONNECTION_STRING"];
            if (!string.IsNullOrWhiteSpace(conn))
            {
                // Handle postgres:// URI format (e.g., Supabase / Neon / Docker)
                if (
                    conn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                    || conn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
                )
                {
                    var uri = new Uri(conn);
                    var userInfo = uri.UserInfo.Split(':');
                    var user = userInfo.Length > 0 ? userInfo[0] : "postgres";
                    var pass = userInfo.Length > 1 ? userInfo[1] : "";
                    var port = uri.Port > 0 ? uri.Port : 5432;
                    var db = uri.AbsolutePath.TrimStart('/');
                    return $"Host={uri.Host};Port={port};Database={db};Username={user};Password={pass};";
                }
                return conn;
            }

            // Option C: Composed from discrete parameters
            var host = configuration["DB_HOST"];
            if (!string.IsNullOrWhiteSpace(host))
            {
                var port = configuration["DB_PORT"] ?? "5432";
                var db = configuration["DB_NAME"] ?? "dsc_db";
                var user = configuration["DB_USER"] ?? "postgres";
                var pass = configuration["DB_PASSWORD"] ?? "";
                return $"Host={host};Port={port};Database={db};Username={user};Password={pass};";
            }

            return string.Empty;
        }
    }
}
