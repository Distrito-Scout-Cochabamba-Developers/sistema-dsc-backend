---
title: 'Spec - 1-1 Digital Module Attendance Registration'
type: 'feature'
created: '2026-07-28'
status: 'done'
baseline_commit: '9c709f6101b28bc1a9157c8e60893ac9d612d812'
review_loop_iteration: 0
context:
  - 'c:/Users/winso/OneDrive/Escritorio/PERSONAL/Projects/DSC/_bmad-output/project-context.md'
  - 'c:/Users/winso/OneDrive/Escritorio/PERSONAL/Projects/DSC/ARCHITECTURE.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Currently, module attendance is recorded manually on paper or unstructured spreadsheets, leading to transcription typos in CI numbers, lost records, and delays in tracking leader training progress.

**Approach:** Implement a digital attendance registration module in .NET 10 (Clean Architecture + DDD) that enables participants to scan a session QR code/link, validate their CI numerically, auto-complete their details if already registered, create a partial profile if unknown, or auto-fill their profile if authenticated, recording verified attendance efficiently.

## Boundaries & Constraints

**Always:**
- Enforce strict numeric validation for CI fields (reject alphabetic characters or phone numbers in CI).
- Encapsulate domain logic inside `DSC.Domain` entities with private setters and factory methods.
- Apply `.AsNoTracking()` on read-only queries in EF Core 10 repositories.
- Use `record class` for all DTOs and Commands, and Primary Constructors for Dependency Injection.
- Pass `CancellationToken` through all async call stacks.

**Ask First:**
- Modifying database migrations or changing table schema constraints beyond the `ScoutAdult`, `TrainingModule`, and `ModuleAttendance` entities.

**Never:**
- Allow direct HTTP/Framework references or ORM attributes inside `DSC.Domain`.
- Allow unhandled exceptions without RFC 7807 Problem Details response in API layer.
- Bypass CI numeric validation or allow duplicate attendance per module session.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Auto-complete Known Adult | CI: "7654321", Extension: "CB" (Exists in DB) | Return `ScoutAdultDto` with FirstName, LastName pre-filled | 200 OK |
| Unknown Adult Lookup | CI: "9999999" (Not in DB) | Return `null` / 404 Not Found, UI allows manual entry | 404 Not Found |
| Valid Digital Attendance Registration | `SessionToken`, CI: "7654321", FirstName, LastName, Extension, Phone | Create/Update `ScoutAdult` (or Partial), record `ModuleAttendance`, return `ModuleAttendanceResultDto` | 201 Created with summary |
| Invalid Numeric CI Input | CI: "7654321A" or "77123456" (Phone length) | Reject before DB query | 400 Bad Request (DomainException) |
| Duplicate Attendance Attempt | Same CI + Same Session Token | Detect existing attendance record | 409 Conflict / Error message |

</frozen-after-approval>

## Code Map

- `src/DSC.Domain/ValueObjects/CiNumber.cs` -- Value object enforcing numeric CI format and department extension validation.
- `src/DSC.Domain/Entities/ScoutAdult.cs` -- Aggregate Root representing a Scout Leader/Adult with full or partial registration status.
- `src/DSC.Domain/Entities/TrainingModule.cs` -- Entity representing a specific training module session with a unique session QR token.
- `src/DSC.Domain/Entities/ModuleAttendance.cs` -- Entity representing a verified attendance record linking a ScoutAdult to a TrainingModule.
- `src/DSC.Domain/Repositories/IScoutAdultRepository.cs` -- Domain repository interface for ScoutAdult persistence.
- `src/DSC.Domain/Repositories/ITrainingModuleRepository.cs` -- Domain repository interface for TrainingModule retrieval by session token.
- `src/DSC.Domain/Repositories/IModuleAttendanceRepository.cs` -- Domain repository interface for ModuleAttendance.
- `src/DSC.Application/DTOs/ScoutAdultDto.cs` -- DTO for adult lookup auto-completion.
- `src/DSC.Application/DTOs/RegisterDigitalAttendanceRequest.cs` -- DTO request for attendance submission.
- `src/DSC.Application/DTOs/ModuleAttendanceResultDto.cs` -- DTO response containing attendance confirmation summary.
- `src/DSC.Application/Queries/LookupAdultByCiQueryHandler.cs` -- Use case handler for CI auto-completion lookup.
- `src/DSC.Application/Commands/RegisterDigitalAttendanceCommandHandler.cs` -- Use case handler orchestrating QR validation, adult lookup/partial creation, and attendance logging.
- `src/DSC.Infrastructure/Persistence/ScoutDbContext.cs` -- EF Core 10 DbContext with DbSets for adults, modules, and attendances.
- `src/DSC.Infrastructure/Persistence/Configurations/ScoutAdultConfiguration.cs` -- EF Core entity type configuration for ScoutAdult.
- `src/DSC.Infrastructure/Persistence/Configurations/TrainingModuleConfiguration.cs` -- EF Core entity type configuration for TrainingModule.
- `src/DSC.Infrastructure/Persistence/Configurations/ModuleAttendanceConfiguration.cs` -- EF Core entity type configuration for ModuleAttendance.
- `src/DSC.Infrastructure/Persistence/Repositories/ScoutAdultRepository.cs` -- EF Core repository implementation with `.AsNoTracking()` optimization.
- `src/DSC.Infrastructure/Persistence/Repositories/TrainingModuleRepository.cs` -- EF Core repository implementation.
- `src/DSC.Infrastructure/Persistence/Repositories/ModuleAttendanceRepository.cs` -- EF Core repository implementation.
- `src/DSC.Api/Controllers/AttendanceController.cs` -- Web API controller with REST endpoints for CI lookup and QR attendance registration.
- `tests/DSC.UnitTests/Domain/CiNumberTests.cs` -- xUnit test verifying strict numeric validation rules of `CiNumber`.
- `tests/DSC.UnitTests/Application/RegisterDigitalAttendanceHandlerTests.cs` -- xUnit test verifying use case execution and double-registration prevention.

## Tasks & Acceptance

**Execution:**
- [x] `src/DSC.Domain/ValueObjects/CiNumber.cs` -- Create Value Object -- Enforce numeric format, non-empty, and valid extension code.
- [x] `src/DSC.Domain/Entities/ScoutAdult.cs` -- Create Aggregate Root -- Encapsulate leader personal data, CI, and partial registration state.
- [x] `src/DSC.Domain/Entities/TrainingModule.cs` -- Create Entity -- Represent training module session and QR session token.
- [x] `src/DSC.Domain/Entities/ModuleAttendance.cs` -- Create Entity -- Record attendance link between ScoutAdult and TrainingModule with timestamp.
- [x] `src/DSC.Domain/Repositories/IScoutAdultRepository.cs` -- Create Repository Interface -- Define `GetByCiAsync`, `AddAsync`, `UpdateAsync`.
- [x] `src/DSC.Domain/Repositories/ITrainingModuleRepository.cs` -- Create Repository Interface -- Define `GetBySessionTokenAsync`.
- [x] `src/DSC.Domain/Repositories/IModuleAttendanceRepository.cs` -- Create Repository Interface -- Define `AddAsync`, `ExistsAsync`.
- [x] `src/DSC.Application/DTOs/ScoutAdultDto.cs` -- Create DTO record -- Transfer adult details for frontend auto-complete.
- [x] `src/DSC.Application/DTOs/RegisterDigitalAttendanceRequest.cs` -- Create DTO record -- Input payload for QR attendance registration.
- [x] `src/DSC.Application/DTOs/ModuleAttendanceResultDto.cs` -- Create DTO record -- Output payload for attendance confirmation.
- [x] `src/DSC.Application/Queries/LookupAdultByCiQueryHandler.cs` -- Create Use Case Handler -- Handle CI auto-complete lookup using `.AsNoTracking()`.
- [x] `src/DSC.Application/Commands/RegisterDigitalAttendanceCommandHandler.cs` -- Create Use Case Handler -- Process QR attendance, create/update adult profile, prevent duplicate entries.
- [x] `src/DSC.Infrastructure/Persistence/ScoutDbContext.cs` -- Update DbContext -- Register DbSets and apply entity configurations.
- [x] `src/DSC.Infrastructure/Persistence/Configurations/ScoutAdultConfiguration.cs` -- Create Configuration -- Map `ScoutAdult` and `CiNumber` Value Object conversion.
- [x] `src/DSC.Infrastructure/Persistence/Configurations/TrainingModuleConfiguration.cs` -- Create Configuration -- Map `TrainingModule` table and indexes.
- [x] `src/DSC.Infrastructure/Persistence/Configurations/ModuleAttendanceConfiguration.cs` -- Create Configuration -- Map `ModuleAttendance` relationships and unique index.
- [x] `src/DSC.Infrastructure/Persistence/Repositories/ScoutAdultRepository.cs` -- Create Repository Implementation -- Implement EF Core queries with `.AsNoTracking()`.
- [x] `src/DSC.Infrastructure/Persistence/Repositories/TrainingModuleRepository.cs` -- Create Repository Implementation -- Implement session token lookup.
- [x] `src/DSC.Infrastructure/Persistence/Repositories/ModuleAttendanceRepository.cs` -- Create Repository Implementation -- Implement attendance check and save.
- [x] `src/DSC.Api/Controllers/AttendanceController.cs` -- Create REST Controller -- Expose `GET api/attendance/lookup-ci/{ci}` and `POST api/attendance/register`.
- [x] `tests/DSC.UnitTests/Domain/CiNumberTests.cs` -- Create Unit Tests -- Test valid and invalid CI numeric formats.
- [x] `tests/DSC.UnitTests/Application/RegisterDigitalAttendanceHandlerTests.cs` -- Create Unit Tests -- Test handler registration flow and duplicate rejection.

**Acceptance Criteria:**
- Given a participant entering a CI with letters or special characters, when submitted, then `CiNumber` throws a DomainException and API returns 400 Bad Request.
- Given an existing leader CI in database, when looked up via `GET api/attendance/lookup-ci/{ci}`, then the API returns 200 OK with pre-filled `FirstName` and `LastName`.
- Given a valid QR `SessionToken` and new leader CI, when submitted via `POST api/attendance/register`, then a new `ScoutAdult` with `IsPartialRegistration = true` and `ModuleAttendance` record are created and 201 Created is returned.
- Given an already registered participant for the same session, when attempting to register again, then the system returns 409 Conflict.

## Design Notes

Using DDD Aggregate Root `ScoutAdult` with Value Object `CiNumber` guarantees that no invalid CI can ever enter the Domain or Database. The `ModuleAttendance` entity enforces a unique constraint on `(TrainingModuleId, ScoutAdultId)` in EF Core configuration to prevent race condition duplicates.

## Verification

**Commands:**
- `dotnet build` -- expected: Build succeeded with 0 errors.
- `dotnet test` -- expected: All unit tests in `DSC.UnitTests` pass.

## Suggested Review Order

**Domain Model & Invariants**

- Objeto de Valor inmutable para validar CIs numéricos de 4-10 dígitos y extensión.
  [`CiNumber.cs:8`](../../src/DSC.Domain/ValueObjects/CiNumber.cs#L8)

- Raíz del Agregado con constructores privados y fábrica controlada de dirigentes.
  [`ScoutAdult.cs:5`](../../src/DSC.Domain/Entities/ScoutAdult.cs#L5)

- Entidad para representar las sesiones de capacitación con token QR único.
  [`TrainingModule.cs:5`](../../src/DSC.Domain/Entities/TrainingModule.cs#L5)

- Entidad de relación de asistencia con sello de tiempo.
  [`ModuleAttendance.cs:5`](../../src/DSC.Domain/Entities/ModuleAttendance.cs#L5)

**Use Cases & Application Logic**

- Orquestación de registro QR, creación/actualización de dirigente y prevención de duplicados.
  [`RegisterDigitalAttendanceCommandHandler.cs:9`](../../src/DSC.Application/Commands/RegisterDigitalAttendanceCommandHandler.cs#L9)

- Consulta de autocompletado por CI utilizando lecturas optimizadas `.AsNoTracking()`.
  [`LookupAdultByCiQueryHandler.cs:8`](../../src/DSC.Application/Queries/LookupAdultByCiQueryHandler.cs#L8)

**Infrastructure & Persistence**

- Configuración EF Core para `ScoutAdult` y mapeo del Value Object `CiNumber`.
  [`ScoutAdultConfiguration.cs:9`](../../src/DSC.Infrastructure/Persistence/Configurations/ScoutAdultConfiguration.cs#L9)

- Índice único `(TrainingModuleId, ScoutAdultId)` que previene duplicados concurrentes en BD.
  [`ModuleAttendanceConfiguration.cs:9`](../../src/DSC.Infrastructure/Persistence/Configurations/ModuleAttendanceConfiguration.cs#L9)

- Repositorio EF Core optimizado con desvinculación limpia para actualizaciones.
  [`ScoutAdultRepository.cs:8`](../../src/DSC.Infrastructure/Persistence/Repositories/ScoutAdultRepository.cs#L8)

**API & Presentation Layer**

- Middleware RFC 7807 Problem Details para mapeo de excepciones de dominio (400, 404, 409).
  [`GlobalExceptionHandlingMiddleware.cs:6`](../../src/DSC.Api/Middlewares/GlobalExceptionHandlingMiddleware.cs#L6)

- Controlador REST para búsqueda de CI y registro de asistencia digital via QR.
  [`AttendanceController.cs:9`](../../src/DSC.Api/Controllers/AttendanceController.cs#L9)

**Unit Tests**

- Pruebas unitarias de validación estricta de CIs numéricos y extensiones.
  [`CiNumberTests.cs:7`](../../tests/DSC.UnitTests/Domain/CiNumberTests.cs#L7)

- Pruebas unitarias del Handler registrando asistentes nuevos, duplicados y tokens no válidos.
  [`RegisterDigitalAttendanceHandlerTests.cs:12`](../../tests/DSC.UnitTests/Application/RegisterDigitalAttendanceHandlerTests.cs#L12)

