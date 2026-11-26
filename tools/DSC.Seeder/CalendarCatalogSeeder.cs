using DSC.Domain.Entities;
using DSC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DSC.Seeder;

/// <summary>
/// Catálogos de convocatoria/inscripción y unas sesiones de demo ancladas a hoy.
/// Idempotente: no duplica códigos ni la misma fecha+módulo.
/// </summary>
public static class CalendarCatalogSeeder
{
    public static async Task SeedAsync(
        ScoutDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureStatusAsync(db, "PLANNED", "Planificada", cancellationToken);
        await EnsureStatusAsync(db, "OPEN", "Inscripción abierta", cancellationToken);
        await EnsureStatusAsync(db, "IN_PROGRESS", "En curso", cancellationToken);
        await EnsureStatusAsync(db, "CLOSED", "Cerrada", cancellationToken);
        await EnsureStatusAsync(db, "CANCELLED", "Cancelada", cancellationToken);

        await EnsureEnrollmentSourceAsync(
            db,
            "AUTHENTICATED_USER",
            "Usuario autenticado",
            cancellationToken
        );
        await EnsureEnrollmentSourceAsync(db, "QR_CODE", "Código QR", cancellationToken);
        await EnsureEnrollmentSourceAsync(db, "WEB_LINK", "Enlace web", cancellationToken);
        await EnsureEnrollmentSourceAsync(
            db,
            "MANUAL_SECRETARY",
            "Secretaría (manual)",
            cancellationToken
        );

        var open =
            await db.TrainingModuleSessionStatuses.SingleAsync(
                status => status.Code == "OPEN",
                cancellationToken
            );
        var today = DateOnly.FromDateTime(DateTime.Now);

        await EnsureDemoSessionAsync(db, "Primeros auxilios", open.Id, today.AddDays(6), cancellationToken);
        await EnsureDemoSessionAsync(db, "Liderazgo", open.Id, today.AddDays(2), cancellationToken);
        await EnsureDemoSessionAsync(
            db,
            "Trabajo en Equipo",
            open.Id,
            today.AddDays(12),
            cancellationToken
        );

        Console.WriteLine("Catálogo de módulos de calendario: estados, orígenes de inscripción y sesiones de demo.");
    }

    private static async Task EnsureStatusAsync(
        ScoutDbContext db,
        string code,
        string name,
        CancellationToken cancellationToken
    )
    {
        if (await db.TrainingModuleSessionStatuses.AnyAsync(status => status.Code == code, cancellationToken))
            return;

        db.TrainingModuleSessionStatuses.Add(
            new TrainingModuleSessionStatus
            {
                Code = code,
                Name = name,
                Description = name,
            }
        );
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureEnrollmentSourceAsync(
        ScoutDbContext db,
        string code,
        string name,
        CancellationToken cancellationToken
    )
    {
        if (await db.EnrollmentSources.AnyAsync(source => source.Code == code, cancellationToken))
            return;

        db.EnrollmentSources.Add(
            new EnrollmentSource
            {
                Code = code,
                Name = name,
                Description = name,
            }
        );
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDemoSessionAsync(
        ScoutDbContext db,
        string moduleName,
        short openStatusId,
        DateOnly sessionDate,
        CancellationToken cancellationToken
    )
    {
        var module = await db.TrainingModules.FirstOrDefaultAsync(
            item => item.Name == moduleName,
            cancellationToken
        );
        if (module is null)
            return;

        var exists = await db.TrainingModuleSessions.AnyAsync(
            session =>
                session.TrainingModuleId == module.Id
                && session.Schedules.Any(block => block.SessionDate == sessionDate),
            cancellationToken
        );
        if (exists)
            return;

        db.TrainingModuleSessions.Add(
            new TrainingModuleSession
            {
                TrainingModuleId = module.Id,
                TrainingModuleSessionStatusId = openStatusId,
                IsRegistrationOpen = true,
                QrToken = Guid.NewGuid(),
                Schedules =
                [
                    new TrainingModuleSchedule
                    {
                        SessionDate = sessionDate,
                        StartTime = new TimeOnly(8, 30),
                        EndTime = new TimeOnly(12, 30),
                        PartNumber = 1,
                    },
                ],
            }
        );
        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"  Convocatoria demo: {moduleName} el {sessionDate:yyyy-MM-dd}.");
    }
}
