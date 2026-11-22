using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DSC.UnitTests.Application;

public class CalendarSessionHandlerTests
{
    private static ScoutDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ScoutDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new ScoutDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static async Task<TrainingModule> SeedModuleAsync(ScoutDbContext db)
    {
        db.TrainingLines.Add(new TrainingLine(1, "PJ", "Programa de Jóvenes"));
        db.TrainingModuleSessionStatuses.AddRange(
            new TrainingModuleSessionStatus
            {
                Id = 1,
                Code = "OPEN",
                Name = "Abierta",
            },
            new TrainingModuleSessionStatus
            {
                Id = 2,
                Code = "CANCELLED",
                Name = "Cancelada",
            },
            new TrainingModuleSessionStatus
            {
                Id = 3,
                Code = "CLOSED",
                Name = "Cerrada",
            }
        );
        db.EnrollmentSources.Add(
            new EnrollmentSource
            {
                Id = 1,
                Code = "AUTHENTICATED_USER",
                Name = "Usuario autenticado",
            }
        );
        var module = new TrainingModule
        {
            Name = "Primeros auxilios",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }],
        };
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();
        return module;
    }

    [Fact]
    public async Task Create_ShouldPersistScheduleAndExposeCalendarDto()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        var handler = new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        );

        var result = await handler.HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 11, 28),
                            new TimeOnly(8, 30),
                            new TimeOnly(12, 30)
                        ),
                    ]
                )
            )
        );

        Assert.True(result.Id > 0);
        Assert.Equal(module.Id.ToString(), result.ModuleCode);
        Assert.Equal("Primeros auxilios", result.ModuleName);
        Assert.Equal("PJ", result.ModuleTypeCode);
        Assert.Equal("OPEN", result.Status);
        Assert.Equal(new TimeSpan(-4, 0, 0), result.StartsAt.Offset);
        Assert.Equal(8, result.StartsAt.Hour);
        Assert.Equal(30, result.StartsAt.Minute);
        Assert.Null(result.Capacity);
        Assert.Equal(0, result.EnrolledCount);
        Assert.False(result.IsEnrolled);
    }

    [Fact]
    public async Task Create_WhenOpenStatusMissing_ShouldInsertCatalogRow()
    {
        using var db = CreateDb();
        db.TrainingLines.Add(new TrainingLine(1, "PJ", "Programa de Jóvenes"));
        var module = new TrainingModule
        {
            Name = "Primeros auxilios",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }],
        };
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();

        var handler = new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        );

        var result = await handler.HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 12, 1),
                            new TimeOnly(8, 30),
                            new TimeOnly(12, 30)
                        ),
                    ]
                )
            )
        );

        Assert.Equal("OPEN", result.Status);
        Assert.Equal("OPEN", db.TrainingModuleSessionStatuses.Single().Code);
    }

    [Fact]
    public async Task Create_WithoutSchedules_ShouldValidate()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        var handler = new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        );

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.HandleAsync(
                new CreateTrainingModuleSessionCommand(
                    module.Id,
                    new WriteTrainingModuleSessionDto([])
                )
            )
        );
    }

    [Fact]
    public async Task List_ShouldFilterByScheduleDate()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        var create = new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        );
        await create.HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 11, 10),
                            new TimeOnly(9, 0),
                            new TimeOnly(11, 0)
                        ),
                    ]
                )
            )
        );
        await create.HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 12, 1),
                            new TimeOnly(9, 0),
                            new TimeOnly(11, 0)
                        ),
                    ]
                )
            )
        );

        var list = new ListCalendarSessionsQueryHandler(
            new TrainingModuleSessionRepository(db),
            new FakeCurrentUser()
        );

        var december = await list.HandleAsync(
            new ListCalendarSessionsQuery(new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 31))
        );

        Assert.Equal(new DateOnly(2026, 12, 1), DateOnly.FromDateTime(december.Single().StartsAt.DateTime));
    }

    [Fact]
    public async Task Enroll_ShouldMarkPersonAndCount()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        var created = await new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        ).HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 11, 28),
                            new TimeOnly(8, 30),
                            new TimeOnly(12, 30)
                        ),
                    ]
                )
            )
        );

        var user = new FakeCurrentUser { Id = 9, PersonId = 4 };
        var enroll = new EnrollInCalendarSessionCommandHandler(
            new TrainingModuleSessionRepository(db),
            user
        );

        var result = await enroll.HandleAsync(new EnrollInCalendarSessionCommand(created.Id));

        Assert.True(result.EnrollmentId > 0);
        Assert.True(result.Session.IsEnrolled);
        Assert.Equal(1, result.Session.EnrolledCount);
    }

    [Fact]
    public async Task Enroll_Duplicate_ShouldConflict()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        var created = await new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        ).HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 11, 28),
                            new TimeOnly(8, 30),
                            new TimeOnly(12, 30)
                        ),
                    ]
                )
            )
        );

        var enroll = new EnrollInCalendarSessionCommandHandler(
            new TrainingModuleSessionRepository(db),
            new FakeCurrentUser { Id = 9, PersonId = 4 }
        );
        await enroll.HandleAsync(new EnrollInCalendarSessionCommand(created.Id));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            enroll.HandleAsync(new EnrollInCalendarSessionCommand(created.Id))
        );
        Assert.Contains("inscrito", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Enroll_Cancelled_ShouldConflict()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        var created = await new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        ).HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 11, 28),
                            new TimeOnly(8, 30),
                            new TimeOnly(12, 30)
                        ),
                    ],
                    StatusCode: "CANCELLED"
                )
            )
        );

        var enroll = new EnrollInCalendarSessionCommandHandler(
            new TrainingModuleSessionRepository(db),
            new FakeCurrentUser { Id = 9, PersonId = 4 }
        );

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            enroll.HandleAsync(new EnrollInCalendarSessionCommand(created.Id))
        );
        Assert.Contains("cancelada", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Enroll_FullFacility_ShouldConflict()
    {
        using var db = CreateDb();
        var module = await SeedModuleAsync(db);
        db.FacilityTypes.Add(
            new FacilityType
            {
                Id = 1,
                Code = "HALL",
                Name = "Salón",
            }
        );
        db.Facilities.Add(
            new Facility
            {
                Id = 10,
                FacilityTypeId = 1,
                Name = "Sala 1",
                Capacity = 1,
                IsActive = true,
            }
        );
        await db.SaveChangesAsync();

        var created = await new CreateTrainingModuleSessionCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingModuleSessionRepository(db)
        ).HandleAsync(
            new CreateTrainingModuleSessionCommand(
                module.Id,
                new WriteTrainingModuleSessionDto(
                    [
                        new WriteTrainingModuleScheduleDto(
                            new DateOnly(2026, 11, 28),
                            new TimeOnly(8, 30),
                            new TimeOnly(12, 30),
                            FacilityId: 10
                        ),
                    ]
                )
            )
        );

        var first = new EnrollInCalendarSessionCommandHandler(
            new TrainingModuleSessionRepository(db),
            new FakeCurrentUser { Id = 1, PersonId = 1 }
        );
        await first.HandleAsync(new EnrollInCalendarSessionCommand(created.Id));

        var second = new EnrollInCalendarSessionCommandHandler(
            new TrainingModuleSessionRepository(db),
            new FakeCurrentUser { Id = 2, PersonId = 2 }
        );
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            second.HandleAsync(new EnrollInCalendarSessionCommand(created.Id))
        );
        Assert.Contains("cupo", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public long? Id { get; init; }
        public long? PersonId { get; init; }
        public string? Username => "tester";
        public string? Email => "tester@example.test";
        public IEnumerable<string> Roles => [];
        public bool IsAuthenticated => PersonId is not null;
        public bool IsInRole(string role) => false;
    }
}
