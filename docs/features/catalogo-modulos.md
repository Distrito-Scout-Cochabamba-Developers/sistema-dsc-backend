# Catálogo de módulos de formación

- **Estado:** Entregado (2026-10-16)
- **Motivo:** El frontend lista módulos desde PostgreSQL y solo abre el formulario de asistencia al elegir uno.

Los endpoints son anónimos, igual que lookup y register. En Scalar (http://localhost:5090/scalar/v1) viven en el grupo **Attendance**. El CRUD admin está en **Módulos**.

## Contrato HTTP

Prefijo global `api` (`GlobalRoutePrefixConvention`). JSON camelCase.

### `GET /api/attendance/modules`

Lista `training_module` ordenado por nombre, con sus líneas de formación incluidas.

| | |
| - | - |
| 200 | `TrainingModuleDto[]` (puede ser `[]`) |

### `GET /api/attendance/modules/{code}`

`code` = `TrainingModule.Code` = `sessionToken` de `POST /api/attendance/register`.

| | |
| - | - |
| 200 | `TrainingModuleDto` |
| 404 | Problem Details: módulo no encontrado |

### `TrainingModuleDto`

| Campo JSON | Origen |
| ---------- | ------ |
| `id` | `training_module.id` |
| `code` | `training_module.code` (máx. 30) |
| `name` | `training_module.name` |
| `description` | `training_module.description` (nullable) |
| `trainingLines` | Lista de líneas (`id`, `code`, `name`, `description`), ordenada por código; unión `training_module_line` con `training_line` |

## Archivos tocados

| Capa | Archivo | Cambio |
| ---- | ------- | ------ |
| Api | `src/DSC.Api/Controllers/AttendanceController.cs` | `ListModules`, `GetModuleByCode` |
| Application | `src/DSC.Application/DTOs/TrainingModuleDto.cs` | DTO nuevo |
| Application | `src/DSC.Application/Queries/ListTrainingModulesQueryHandler.cs` | Listado |
| Application | `src/DSC.Application/Queries/GetTrainingModuleByCodeQueryHandler.cs` | Detalle por código |
| Application | `src/DSC.Application/DependencyInjection.cs` | Registro de ambos handlers |
| Domain | `src/DSC.Domain/Repositories/ITrainingModuleRepository.cs` | `ListAsync` |
| Infrastructure | `src/DSC.Infrastructure/Persistence/Repositories/TrainingModuleRepository.cs` | Incluye `ModuleLines.TrainingLine` en listado y detalle |

Migración `IntroduceTrainingLines`: renombra la línea existente y transforma pertenencia única en múltiple. Cambia el contrato JSON anterior; frontend pendiente de adaptación.

## Datos locales

Sin filas en `training_module` el GET devuelve `[]` y el POST de asistencia responde 404. Semilla del workspace: `docker/seed-dev-modules.sql` (tipos `AE` / `PJ` y cuatro módulos, entre ellos `mod-liderazgo-20241024`).

## Fuera de alcance

- Convocatoria (`TrainingModuleSession`: fecha, horario, QR admin). El `code` del módulo curricular hace de token de sesión.

Administración (POST/PUT, líneas, roles): [admin-catalogo-modulos.md](./admin-catalogo-modulos.md).
