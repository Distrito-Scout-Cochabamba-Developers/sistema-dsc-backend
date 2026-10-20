---
baseline_commit: 4ccbf3d2487d2fe013ace1c04a3819f6c45a0419
---

# Story 1.2: Gestión de Catálogo de Módulos de Formación

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

> **Alcance de esta tarea (Azure DevOps):** `[BE] Commands, Queries y Repository para TrainingModule con AsNoTracking`.
> Cubre Domain + Application + Infrastructure. **No incluye** controlador/endpoints ni autorización por roles (ver "Fuera de alcance").

## Story

Como **Responsable de Adultos en el Escultismo (`CAE`) o Secretaría**,
quiero registrar, actualizar y consultar los módulos de formación disponibles en el distrito,
para mantener un catálogo estandarizado utilizable en calendarios y mallas curriculares.

## Acceptance Criteria

1. **Dado** un comando de creación con nombre, código, descripción y tipo de módulo válidos,
   **cuando** se ejecuta `CreateTrainingModuleCommandHandler`,
   **entonces** se persiste un `TrainingModule` con `IsActive = true` y se devuelve su `TrainingModuleDto`.
2. **Dado** que ya existe un módulo **activo** con el mismo nombre (comparación sin distinguir mayúsculas y sin espacios extremos),
   **cuando** se intenta crear o actualizar otro módulo con ese nombre,
   **entonces** el handler lanza `InvalidOperationException` (→ 409 Conflict vía middleware) y no persiste nada.
3. **Dado** que ya existe un módulo con el mismo código (activo **o inactivo**, porque `training_module.code` tiene `UNIQUE` global en BD),
   **cuando** se intenta crear o actualizar otro módulo con ese código,
   **entonces** el handler lanza `InvalidOperationException` (→ 409) y no persiste nada.
4. **Dado** un comando con código/nombre vacío, código > 30 caracteres, nombre > 150 caracteres o `ModuleTypeId` inexistente,
   **cuando** se ejecuta el handler,
   **entonces** se lanza `DomainException` (→ 400) antes de escribir en la BD.
5. **Dado** un `TrainingModule` existente,
   **cuando** se ejecuta `UpdateTrainingModuleCommandHandler` con datos válidos (incluido `IsActive`),
   **entonces** se actualizan nombre, código, descripción, tipo y estado; si el `Id` no existe se lanza `KeyNotFoundException` (→ 404).
6. **Dado** el catálogo,
   **cuando** se ejecuta `ListTrainingModulesQueryHandler`,
   **entonces** se devuelve el listado mediante una consulta de solo lectura con `.AsNoTracking()`, ordenado por nombre, incluyendo `IsActive` en el DTO, con opción de devolver solo los activos.
7. `dotnet build` sin errores y `dotnet test` con todos los tests en verde (existentes + nuevos).

## Tasks / Subtasks

- [x] **Task 1 — Dominio: estado activo y reglas** (AC: 1, 4, 5)
  - [x] 1.1 Añadir `public bool IsActive { get; set; } = true;` a `src/DSC.Domain/Entities/TrainingModule.cs`.
  - [x] 1.2 Añadir constantes `CodeMaxLength = 30` y `NameMaxLength = 150` en la entidad (fuente única para validación y config EF).
  - [x] 1.3 Añadir método de fábrica `static TrainingModule Create(string code, string name, string? description, short moduleTypeId)` y método `Update(string code, string name, string? description, short moduleTypeId, bool isActive)`. Ambos hacen `Trim()`, validan vacíos/longitudes y lanzan `DomainException` con mensaje en español.
  - [x] 1.4 **No** convertir los setters existentes a privados en esta historia (ver Dev Notes → "Variaciones").
- [x] **Task 2 — Infraestructura: mapeo y migración** (AC: 1)
  - [x] 2.1 En `TrainingModuleConfiguration` (`CurriculumConfigurations.cs`): `builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);` y usar las constantes de longitud de la entidad.
  - [x] 2.2 Crear migración: `dotnet tool restore && dotnet ef migrations add AddTrainingModuleIsActive --project src/DSC.Infrastructure --startup-project src/DSC.Api`.
  - [x] 2.3 Revisar que la migración **solo** agregue `is_active boolean NOT NULL DEFAULT true` a `training_module` (las 4 filas existentes quedan activas). Si genera algo más, detenerse y revisar el snapshot.
- [x] **Task 3 — Repositorio** (AC: 2, 3, 4, 5, 6)
  - [x] 3.1 En `src/DSC.Domain/Repositories/ITrainingModuleRepository.cs` añadir:
    - `Task<TrainingModule?> GetByIdForUpdateAsync(long id, CancellationToken ct)` (con tracking, para Update).
    - `Task<bool> ExistsByCodeAsync(string code, long? excludeId, CancellationToken ct)`.
    - `Task<bool> ExistsActiveByNameAsync(string name, long? excludeId, CancellationToken ct)`.
    - `Task<bool> ModuleTypeExistsAsync(short moduleTypeId, CancellationToken ct)`.
    - `Task SaveChangesAsync(CancellationToken ct)`.
    - Cambiar `ListAsync(CancellationToken)` → `ListAsync(bool onlyActive, CancellationToken)`.
  - [x] 3.2 Implementar en `TrainingModuleRepository.cs`: todas las lecturas con `.AsNoTracking()` excepto `GetByIdForUpdateAsync`. Comparación de nombre/código con `Trim()` + `ToLower()` (mismo patrón que `UserRepository`). `ListAsync`: `AsNoTracking()`, `Include(ModuleType)`, `Where(IsActive)` si `onlyActive`, `OrderBy(Name)`.
  - [x] 3.3 Conservar `GetByCodeAsync`, `GetByIdAsync` y `AddAsync` tal como están (los usa `RegisterDigitalAttendanceCommandHandler`).
- [x] **Task 4 — Application: Commands** (AC: 1–5)
  - [x] 4.1 `src/DSC.Application/Commands/CreateTrainingModuleCommandHandler.cs`: `public record class CreateTrainingModuleCommand(string Code, string Name, string? Description, short ModuleTypeId);` + handler con primary constructor. Orden: `TrainingModule.Create(...)` (valida) → `ModuleTypeExistsAsync` → `ExistsByCodeAsync` → `ExistsActiveByNameAsync` → `AddAsync` → devolver `TrainingModuleDto`.
  - [x] 4.2 `src/DSC.Application/Commands/UpdateTrainingModuleCommandHandler.cs`: `public record class UpdateTrainingModuleCommand(long Id, string Code, string Name, string? Description, short ModuleTypeId, bool IsActive);`. Orden: `GetByIdForUpdateAsync` (o `KeyNotFoundException`) → validar tipo → duplicados con `excludeId = Id` (el nombre solo se compara si el resultado queda activo) → `module.Update(...)` → `SaveChangesAsync`.
  - [x] 4.3 Registrar ambos handlers en `src/DSC.Application/DependencyInjection.cs` (`AddScoped`).
- [x] **Task 5 — Application: Query y DTO** (AC: 6)
  - [x] 5.1 Añadir `bool IsActive` al final de `TrainingModuleDto` (record posicional).
  - [x] 5.2 Actualizar `ListTrainingModulesQueryHandler` y `GetTrainingModuleByCodeQueryHandler` para mapear `IsActive`. Añadir `public record class ListTrainingModulesQuery(bool OnlyActive = false);` y que `HandleAsync(ListTrainingModulesQuery query, CancellationToken ct)` lo use.
  - [x] 5.3 Ajustar la llamada en `AttendanceController.ListModules` a `new ListTrainingModulesQuery()` **sin cambiar su comportamiento** (sigue devolviendo todos). Es el único cambio permitido en `DSC.Api`.
- [x] **Task 6 — Tests** (AC: 1–7)
  - [x] 6.1 `tests/DSC.UnitTests/Application/TrainingModuleCatalogHandlerTests.cs` con el patrón existente (EF InMemory + repositorio real, ver `RegisterDigitalAttendanceHandlerTests`). Sembrar un `ModuleType` en cada test.
  - [x] 6.2 Casos: crear válido (activo); nombre duplicado activo → 409; nombre igual a uno **inactivo** → permitido; código duplicado (incluye inactivo) → 409; comparación sin mayúsculas/espacios; vacíos/longitudes → `DomainException`; tipo inexistente → `DomainException`; update válido; update de `Id` inexistente → `KeyNotFoundException`; update a su propio nombre/código no es duplicado; listar `OnlyActive` vs todos y orden por nombre.
  - [x] 6.3 `tests/DSC.UnitTests/Domain/TrainingModuleTests.cs`: `Create`/`Update` recortan espacios y validan.
- [x] **Task 7 — Verificación**
  - [x] 7.1 `dotnet build` y `dotnet test` en verde (salvo 3 tests preexistentes de `RegisterDigitalAttendanceHandlerTests`, ver Debug Log).
  - [x] 7.2 `dotnet csharpier check .` (herramienta local en `.config/dotnet-tools.json`) — formatear con `dotnet csharpier format .` si falla.
  - [x] 7.3 Levantar la API en Development contra `dsc_db` y confirmar que la migración se aplica sola y `GET /api/attendance/modules` sigue respondiendo 200 con `isActive: true` en los 4 módulos.

## Dev Notes

### Punto de partida (rama)

- Rama de trabajo: `feat/modulos-cmd`, creada desde `origin/feat/catalogo-modulos` (Boris, `4ccbf3d`, sin mergear a `main`). **Reutilizar** lo que ya existe ahí; no duplicar.
- Esa base va **2 commits por detrás de `main`** (`ee4c861` auth con BCrypt/`/api/v1`, `8fa80de` docs BMAD). Consecuencias:
  - `_bmad-output/planning-artifacts/epics.md` **no existe en esta rama** (la historia sale de `main`).
  - `DependencyInjection.cs` de Application difiere en `main` (registros de login). Esperar conflicto trivial ahí al rebasar sobre `main` cuando Boris mergee.
- No tocar nada de auth.

### Estado actual de los archivos a modificar (UPDATE)

| Archivo | Hoy | Cambio | Preservar |
|---|---|---|---|
| `src/DSC.Domain/Entities/TrainingModule.cs` | POCO con setters públicos: `Id, Code, Name, Description, ModuleTypeId` + navegaciones | `IsActive`, constantes, `Create`/`Update` | Setters públicos (los usan tests y `RegisterDigitalAttendance`) |
| `src/DSC.Domain/Repositories/ITrainingModuleRepository.cs` | `GetByCodeAsync`, `GetByIdAsync`, `ListAsync`, `AddAsync` | Métodos de existencia, tracked load, `SaveChangesAsync`, filtro en `ListAsync` | Firmas de `GetByCodeAsync`/`GetByIdAsync`/`AddAsync` |
| `src/DSC.Infrastructure/Persistence/Repositories/TrainingModuleRepository.cs` | Lecturas con `AsNoTracking()`; `GetByCodeAsync` y `ListAsync` con `Include(ModuleType)`; `AddAsync` llama `SaveChangesAsync` | Implementar nuevos métodos | `AddAsync` sigue guardando (lo usa attendance) |
| `src/DSC.Infrastructure/Persistence/Configurations/CurriculumConfigurations.cs` (`TrainingModuleConfiguration`) | `code` varchar(30) UNIQUE, `name` varchar(150), `description` text, FK `module_type_id` Restrict | Columna `is_active` | Índice único de `code`, FK |
| `src/DSC.Application/DTOs/TrainingModuleDto.cs` | `(Id, Code, Name, Description, ModuleTypeCode, ModuleTypeName)` | + `IsActive` al final | Orden de campos existentes (contrato JSON del front) |
| `src/DSC.Application/Queries/ListTrainingModulesQueryHandler.cs` | `HandleAsync(ct)` → mapea en memoria | Recibe `ListTrainingModulesQuery` | Orden por nombre |
| `src/DSC.Application/DependencyInjection.cs` | Registra handlers de lookup, módulos, attendance y login | + 2 handlers | Resto de registros |
| `src/DSC.Api/Controllers/AttendanceController.cs` | `GET modules`, `GET modules/{code}` anónimos | Solo adaptar la llamada al nuevo query | Comportamiento y anonimato |

### Reglas técnicas (project-context.md)

- `record class` para Commands/DTOs; **primary constructors** para DI; propagar `CancellationToken` en toda la cadena.
- Lecturas con `.AsNoTracking()` (o `.Select()`); configuración EF **solo** en `IEntityTypeConfiguration<T>`; nada de atributos ORM en `DSC.Domain`.
- Errores mapeados por `GlobalExceptionHandlingMiddleware`: `DomainException`→400, `KeyNotFoundException`→404, `InvalidOperationException`→409. No crear excepciones nuevas.
- Mensajes de error en español, como el resto del código.

### Decisiones de diseño

- **Código vs. nombre:** el AC dice "no duplicados en el catálogo activo". Para el **nombre** se aplica literal (solo activos). Para el **código** se valida contra todos porque la BD tiene `UNIQUE(code)` global; validar solo activos dejaría pasar un caso que luego revienta como 500 en `SaveChanges`.
- **InMemory no aplica índices únicos:** los tests solo prueban la validación del handler, no la constraint de BD. Por eso la validación en el handler es obligatoria.
- **Carrera concurrente** (dos creaciones simultáneas del mismo código): la constraint de BD la frena, pero hoy saldría como 500. Fuera de alcance; anotarlo si se ve en review.
- **`RegisterDigitalAttendanceCommandHandler` usa `GetByCodeAsync`:** no filtrar por `IsActive` ahí en esta historia (cambiaría el comportamiento de asistencia). Ver pregunta abierta 3.

### Variaciones respecto a la arquitectura

- `project-context.md` pide setters privados + métodos de negocio. `TrainingModule` hoy tiene setters públicos y se construye con inicializadores de objeto (p. ej. `RegisterDigitalAttendanceHandlerTests`). La semilla `docker/seed-dev-modules.sql` que cita `docs/features/catalogo-modulos.md` **no está versionada** en el repo. Se añaden `Create`/`Update` con validación, pero **no** se privatizan los setters para no romper código ajeno en esta tarea. Proponer la privatización como deuda técnica.
- Nombres de migración: el proyecto tiene una sola (`20260826031552_InitialMigration`). `Program.cs` hace *baseline* automático de esa migración y luego `Database.Migrate()` en Development, así que la nueva se aplica al arrancar la API.

### Fuera de alcance

- Controlador/endpoints de administración del catálogo (POST/PUT) y su autorización por roles `CAE`, `RFR`, `SD` (NFR-SEC-01). El rol **`RFR` todavía no existe** en la tabla `role` (sí existen `ADULT_RESOURCES_COORDINATOR` = CAE y `DISTRICT_SECRETARY` = SD); se agregará después.
- Cambios en el frontend.
- Borrado físico de módulos (la desactivación se hace con `IsActive = false` vía Update).

### Preguntas abiertas (para el equipo)

1. **`ModuleTypeId`** es obligatorio en `training_module` pero el AC solo menciona nombre, código y descripción. Esta historia lo pide en el comando. ¿Confirmar o definir un valor por defecto?
2. **Migración**: la historia 1.1 marcaba "Ask First" para cambios de esquema. Agregar `is_active` es necesario para el AC ("estado activo"). Confirmar con Winsor antes del merge.
3. ¿Los módulos inactivos deben seguir aceptando asistencia (`POST /api/attendance/register`) y aparecer en `GET /api/attendance/modules`? Hoy se mantiene el comportamiento (sí).
4. ¿Cuándo se mergea `feat/catalogo-modulos` (Boris)? Esta rama depende de ella.

### Project Structure Notes

- Commands en `src/DSC.Application/Commands/` con command record + handler en el mismo archivo (patrón de `GetTrainingModuleByCodeQueryHandler.cs`).
- Tests en `tests/DSC.UnitTests/Application/` y `tests/DSC.UnitTests/Domain/`; xUnit, sin librería de mocks (el proyecto usa EF InMemory + repositorios reales).
- Datos locales en `dsc_db`: `module_type` = 1 PJ, 2 GI, 3 AE; `training_module` tiene 4 filas (`SCOUT_METHOD_AND_PROGRAM`, `OUTDOOR_FIRST_AID`, `GROUP_MANAGEMENT`, `ADVISOR_ROLE`).

### Investigación técnica

- Sin librerías nuevas: EF Core 10 y `dotnet-ef` 10.0.11 ya fijados en el repo. `HasDefaultValue(true)` en un `bool` hace que EF genere `DEFAULT true` en la migración; al crear con `Create(...)` se asigna `IsActive = true` explícitamente, así que no hay ambigüedad con el valor CLR por defecto.

### References

- [Source: main:_bmad-output/planning-artifacts/epics.md#Historia 1.2: Gestión de Catálogo de Módulos de Formación]
- [Source: _bmad-output/project-context.md#4. Patrones de Persistencia con EF Core 10]
- [Source: _bmad-output/project-context.md#5. Estrategia de Testing (xUnit)]
- [Source: _bmad-output/implementation-artifacts/spec-1-1-digital-attendance.md#Boundaries & Constraints]
- [Source: docs/features/catalogo-modulos.md] (rama de Boris)
- [Source: src/DSC.Api/Middlewares/GlobalExceptionHandlingMiddleware.cs]
- [Source: src/DSC.Api/Program.cs#auto-migración en Development]
- [Source: tests/DSC.UnitTests/Application/RegisterDigitalAttendanceHandlerTests.cs] (patrón de test)

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (`claude-opus-5-5`)

### Debug Log References

- Línea base (`4ccbf3d`, antes de cualquier cambio): `dotnet test` → 21 ✅ / **3 ❌** en `RegisterDigitalAttendanceHandlerTests`. Causa: `feat/catalogo-modulos` añadió `.Include(x => x.ModuleType)` en `GetByCodeAsync` y esos tests siembran `TrainingModule` sin `ModuleType`, así que EF InMemory no lo encuentra (`KeyNotFoundException`). **Preexistente, no introducido por esta historia; no corregido aquí** (código de otra rama). Arreglo sugerido: sembrar un `ModuleType` en esos tests.
- Tras la implementación: `dotnet test` → 47 ✅ / 3 ❌ (los mismos 3). Los 26 tests nuevos pasan.
- `dotnet csharpier check .`: solo falla `AttendanceController.cs`, que ya fallaba en la línea base; no se reformateó para no reescribir el archivo de otra persona.

### Completion Notes List

- Ultimate context engine analysis completed - comprehensive developer guide created
- `IsActive` mapeado con `HasDefaultValue(true)` **y `HasSentinel(true)`**: sin el sentinel, EF omite `false` en el INSERT y PostgreSQL guardaría `true` (módulo inactivo quedaría activo).
- `GetByIdAsync` ahora incluye `ModuleType` para que el DTO devuelto por Create/Update traiga `moduleTypeCode`/`moduleTypeName` en una petición real (no solo en tests con change tracker compartido).
- `UpdateTrainingModuleCommandHandler` valida sobre una entidad candidata no rastreada y solo aplica `Update` sobre la entidad rastreada si todo es válido.
- Reglas de unicidad compartidas en `TrainingModuleCatalogRules` (interno, archivo propio `TrainingModuleCatalogRules.cs`).
- Deuda técnica anotada (no aplicada para no romper código): `ToLower()` en consultas impide usar índices en PostgreSQL (`EF.Functions.ILike` no funciona con EF InMemory de los tests); setters públicos en `TrainingModule`.
- `TrainingModuleDto.FromEntity` centraliza el mapeo (antes duplicado en dos queries).
- Migración `20261020211219_AddTrainingModuleIsActive`: solo `AddColumn is_active boolean NOT NULL DEFAULT true`. Snapshot y migraciones conservan CRLF como el resto de `Migrations/`.
- 7.3 (parcial, 2026-10-20): migración aplicada en `dsc_db` local con `dotnet ef database update` (OK del usuario). Verificado: `is_active boolean NOT NULL DEFAULT true` y los 4 módulos existentes con `is_active = t`. API reiniciada con esta rama: `GET /api/attendance/modules` → 200 con `isActive: true` en los 4 módulos. ✅

### File List

- `src/DSC.Domain/Entities/TrainingModule.cs` (modificado)
- `src/DSC.Domain/Repositories/ITrainingModuleRepository.cs` (modificado)
- `src/DSC.Infrastructure/Persistence/Repositories/TrainingModuleRepository.cs` (modificado)
- `src/DSC.Infrastructure/Persistence/Configurations/CurriculumConfigurations.cs` (modificado)
- `src/DSC.Infrastructure/Persistence/Migrations/20261020211219_AddTrainingModuleIsActive.cs` (nuevo)
- `src/DSC.Infrastructure/Persistence/Migrations/20261020211219_AddTrainingModuleIsActive.Designer.cs` (nuevo)
- `src/DSC.Infrastructure/Persistence/Migrations/ScoutDbContextModelSnapshot.cs` (modificado)
- `src/DSC.Application/Commands/CreateTrainingModuleCommandHandler.cs` (nuevo)
- `src/DSC.Application/Commands/UpdateTrainingModuleCommandHandler.cs` (nuevo)
- `src/DSC.Application/Commands/TrainingModuleCatalogRules.cs` (nuevo)
- `src/DSC.Application/DTOs/TrainingModuleDto.cs` (modificado)
- `src/DSC.Application/Queries/ListTrainingModulesQueryHandler.cs` (modificado)
- `src/DSC.Application/Queries/GetTrainingModuleByCodeQueryHandler.cs` (modificado)
- `src/DSC.Application/DependencyInjection.cs` (modificado)
- `src/DSC.Api/Controllers/AttendanceController.cs` (modificado: solo la llamada a `ListTrainingModulesQuery`)
- `tests/DSC.UnitTests/Domain/TrainingModuleTests.cs` (nuevo)
- `tests/DSC.UnitTests/Application/TrainingModuleCatalogHandlerTests.cs` (nuevo)
