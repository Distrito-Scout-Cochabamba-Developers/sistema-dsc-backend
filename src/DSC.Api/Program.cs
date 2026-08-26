using System.Text;
using DotNetEnv;
using DSC.Api;
using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// 0. Load environment variables from .env file if present
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// 1. Resolve PostgreSQL Connection String securely from environment / .env / config
var connectionString = ResolveConnectionString(builder.Configuration);

// 2. Register Layer Services
builder.Services.AddControllers();
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
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

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
