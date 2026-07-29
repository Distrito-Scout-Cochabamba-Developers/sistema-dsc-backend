using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Domain.Entities;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Layer Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");

var app = builder.Build();

// 2. Global Exception Handling RFC 7807 Problem Details
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 3. Seed initial training module for testing/demo
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ScoutDbContext>();
    dbContext.Database.EnsureCreated();

    if (!dbContext.TrainingModules.Any())
    {
        var sampleModule = TrainingModule.Create(
            "Módulo de Formación Institucional 2026",
            "MOD-2026-CBBA",
            DateTime.UtcNow
        );
        dbContext.TrainingModules.Add(sampleModule);
        dbContext.SaveChanges();
    }
}

app.Run();
