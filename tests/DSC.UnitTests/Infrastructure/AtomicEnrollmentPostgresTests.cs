using System.Data;
using DSC.Application.Commands;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using DSC.UnitTests.Application;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DSC.UnitTests.Infrastructure;

/// <summary>
/// La operación compartida de inscripción se prueba contra PostgreSQL real:
/// bloqueo de la convocatoria, cupo propio, transacción ambiente y errores ajenos.
/// </summary>
[Collection("Training requirements PostgreSQL")]
public class AtomicEnrollmentPostgresTests(RequirementsPostgres postgres)
{
    private const string SourceCode = "AUTHENTICATED_USER";

    [Fact]
    public async Task SharedHandlerJoinsAmbientTransactionAndRollsBackPersonAndEnrollment()
    {
        await using var db = await postgres.CreateDatabase();
        var fixture = await SeedAsync(db, capacity: 1);
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted
        );
        var person = await AddPersonAsync(db, fixture.PersonStatusId, "9910001", "Celia");
        await CreateHandler(db).HandleAsync(
            new(fixture.SessionId, person.Id, SourceCode, "Usuario", null)
        );
        await transaction.RollbackAsync();
        db.ChangeTracker.Clear();
        Assert.False(await db.Persons.AnyAsync(item => item.Id == person.Id));
        Assert.Equal(0, await db.TrainingModuleEnrollments.CountAsync());
    }

    [Fact]
    public async Task EnrollmentWaitsForCapacityExpansionAndUsesNewCapacity()
    {
        await using var db = await postgres.CreateDatabase();
        var fixture = await SeedAsync(db, capacity: 0);
        var person = await AddPersonAsync(db, fixture.PersonStatusId, "9920001", "Dalia");
        await using var expanding = postgres.Open(db.Database.GetConnectionString()!);
        await using var transaction = await expanding.Database.BeginTransactionAsync();
        await new TrainingModuleSessionRepository(expanding).ExpandCapacityAsync(
            fixture.SessionId, 1
        );
        var attempt = CreateHandler(db).HandleAsync(
            new(fixture.SessionId, person.Id, SourceCode, "Usuario", null)
        );
        await Task.Delay(300);
        Assert.False(attempt.IsCompleted);
        await transaction.CommitAsync();
        await attempt;
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync());
    }

    [Fact]
    public async Task OldRepeatableReadSnapshotCannotEnrollPastCapacity()
    {
        await using var db = await postgres.CreateDatabase();
        var fixture = await SeedAsync(db, capacity: 1);
        var first = await AddPersonAsync(db, fixture.PersonStatusId, "9900001", "Alba");
        var second = await AddPersonAsync(db, fixture.PersonStatusId, "9900002", "Bruno");
        await using var stale = postgres.Open(db.Database.GetConnectionString()!);
        await using var transaction = await stale.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead
        );
        Assert.Equal(0, await stale.TrainingModuleEnrollments.CountAsync());
        await CreateHandler(db).HandleAsync(
            new(fixture.SessionId, first.Id, SourceCode, "Usuario", null)
        );

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(stale).HandleAsync(
                new(fixture.SessionId, second.Id, SourceCode, "Usuario", null)
            )
        );
        Assert.Contains("READ COMMITTED", error.Message);
        await transaction.RollbackAsync();
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync());
    }

    [Fact]
    public async Task ConcurrentEnrollmentWaitsForTheSessionLockAndLosesTheLastSeat()
    {
        await using var db = await postgres.CreateDatabase();
        var fixture = await SeedAsync(db, capacity: 1);
        var first = await AddPersonAsync(db, fixture.PersonStatusId, "9600001", "Iván");
        var second = await AddPersonAsync(db, fixture.PersonStatusId, "9600002", "Julia");
        var sessionId = fixture.SessionId;
        var sourceId = fixture.SourceId;

        await using var competitor = postgres.Open(db.Database.GetConnectionString()!);
        await using var competitorTransaction = await competitor.Database.BeginTransactionAsync();
        await competitor.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM training_module_session WHERE id = {sessionId} FOR UPDATE"
        );

        var handler = CreateHandler(db);
        var attempt = handler.HandleAsync(
            new EnrollParticipantInSessionCommand(
                sessionId,
                second.Id,
                SourceCode,
                "Usuario autenticado",
                null
            )
        );

        await Task.Delay(500);
        Assert.False(attempt.IsCompleted);

        competitor.TrainingModuleEnrollments.Add(
            new TrainingModuleEnrollment
            {
                TrainingModuleSessionId = sessionId,
                PersonId = first.Id,
                EnrollmentSourceId = sourceId,
            }
        );
        await competitor.SaveChangesAsync();
        await competitorTransaction.CommitAsync();

        await Assert.ThrowsAsync<SessionFullException>(() => attempt);

        db.ChangeTracker.Clear();
        var enrollments = await db
            .TrainingModuleEnrollments.AsNoTracking()
            .Where(enrollment => enrollment.TrainingModuleSessionId == sessionId)
            .ToListAsync();
        Assert.Single(enrollments);
        Assert.Equal(first.Id, enrollments.Single().PersonId);
    }

    [Fact]
    public async Task EnrollmentJoinsTheAmbientTransactionAndRollsBackWithIt()
    {
        await using var db = await postgres.CreateDatabase();
        var fixture = await SeedAsync(db, capacity: 3);
        var person = await AddPersonAsync(db, fixture.PersonStatusId, "9700001", "Karen");
        var unitOfWork = new UnitOfWork(db);
        var repository = new TrainingModuleSessionRepository(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            unitOfWork.ExecuteInTransactionAsync<object?>(
                async cancellationToken =>
                {
                    await repository.EnrollAsync(
                        new SessionEnrollmentRequest(
                            fixture.SessionId,
                            person.Id,
                            SourceCode,
                            "Usuario autenticado",
                            null
                        ),
                        cancellationToken
                    );
                    throw new InvalidOperationException("Falla posterior al alta.");
                }
            )
        );

        db.ChangeTracker.Clear();
        Assert.Empty(
            await db.TrainingModuleEnrollments
                .Where(enrollment => enrollment.TrainingModuleSessionId == fixture.SessionId)
                .ToListAsync()
        );
    }

    [Fact]
    public async Task ForeignPersistenceErrorsAreNotReportedAsDuplicates()
    {
        await using var db = await postgres.CreateDatabase();
        var fixture = await SeedAsync(db, capacity: 3);
        var unitOfWork = new UnitOfWork(db);
        var repository = new TrainingModuleSessionRepository(db);

        var error = await Assert.ThrowsAsync<DbUpdateException>(() =>
            unitOfWork.ExecuteInTransactionAsync(
                cancellationToken =>
                    repository.EnrollAsync(
                        new SessionEnrollmentRequest(
                            fixture.SessionId,
                            999_999,
                            SourceCode,
                            "Usuario autenticado",
                            null
                        ),
                        cancellationToken
                    )
            )
        );

        Assert.IsNotType<EnrollmentConflictException>(error);
        Assert.IsNotType<AlreadyEnrolledException>(error);
        db.ChangeTracker.Clear();
        Assert.Empty(
            await db.TrainingModuleEnrollments
                .Where(enrollment => enrollment.TrainingModuleSessionId == fixture.SessionId)
                .ToListAsync()
        );
    }

    [Fact]
    public async Task TwoIndependentConnectionsCompeteForTheLastSeat()
    {
        await using var firstDb = await postgres.CreateDatabase();
        var fixture = await SeedAsync(firstDb, capacity: 1);
        var first = await AddPersonAsync(firstDb, fixture.PersonStatusId, "9800001", "Luis");
        var second = await AddPersonAsync(firstDb, fixture.PersonStatusId, "9800002", "Marta");
        await using var secondDb = postgres.Open(firstDb.Database.GetConnectionString()!);

        var firstAttempt = CreateHandler(firstDb).HandleAsync(
            new EnrollParticipantInSessionCommand(
                fixture.SessionId,
                first.Id,
                SourceCode,
                "Usuario autenticado",
                null
            )
        );
        var secondAttempt = CreateHandler(secondDb).HandleAsync(
            new EnrollParticipantInSessionCommand(
                fixture.SessionId,
                second.Id,
                SourceCode,
                "Usuario autenticado",
                null
            )
        );

        var outcomes = await Task.WhenAll(
            CaptureAsync(firstAttempt),
            CaptureAsync(secondAttempt)
        );

        Assert.Equal(1, outcomes.Count(outcome => outcome is null));
        Assert.Equal(1, outcomes.Count(outcome => outcome is SessionFullException));
        firstDb.ChangeTracker.Clear();
        Assert.Equal(
            1,
            await firstDb.TrainingModuleEnrollments.CountAsync(enrollment =>
                enrollment.TrainingModuleSessionId == fixture.SessionId
            )
        );
    }

    private static EnrollParticipantInSessionCommandHandler CreateHandler(ScoutDbContext db) =>
        new(new TrainingModuleSessionRepository(db), new UnitOfWork(db));

    private static async Task<Exception?> CaptureAsync(
        Task<TrainingModuleEnrollment> attempt
    )
    {
        try
        {
            await attempt;
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static async Task<Fixture> SeedAsync(ScoutDbContext db, int capacity)
    {
        await db.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
        var status = new TrainingModuleSessionStatus { Code = "OPEN", Name = "Abierta" };
        var personStatus = await db.PersonStatuses.SingleAsync(x => x.Code == "ACTIVE");
        var source = new EnrollmentSource { Code = SourceCode, Name = "Usuario autenticado" };
        db.TrainingModuleSessionStatuses.Add(status);
        db.EnrollmentSources.Add(source);
        await db.SaveChangesAsync();
        var moduleId = await db.TrainingModules.Select(module => module.Id).FirstAsync();

        var session = new TrainingModuleSession
        {
            TrainingModuleId = moduleId,
            TrainingModuleSessionStatusId = status.Id,
            Capacity = capacity,
            QrToken = Guid.NewGuid(),
            Schedules =
            [
                new TrainingModuleSchedule
                {
                    PartNumber = 1,
                    SessionDate = new DateOnly(2026, 12, 5),
                    StartTime = new TimeOnly(8, 30),
                    EndTime = new TimeOnly(12, 30),
                },
            ],
        };
        db.TrainingModuleSessions.Add(session);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return new Fixture(session.Id, source.Id, personStatus.Id);
    }

    private static async Task<Person> AddPersonAsync(
        ScoutDbContext db,
        short personStatusId,
        string idCard,
        string firstName
    )
    {
        var person = new Person
        {
            IdCard = idCard,
            FirstName = firstName,
            FirstLastName = "Prueba",
            PersonStatusId = personStatusId,
        };
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        return person;
    }

    private sealed record Fixture(long SessionId, short SourceId, short PersonStatusId);
}
