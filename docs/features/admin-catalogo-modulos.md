# Administración del catálogo de módulos

- **Estado:** Entregado (2026-10-22)
- **Roles:** `ADMIN`, `DISTRICT_SECRETARY` (claim JWT `roles`)
- **Scalar:** http://localhost:5090/scalar/v1 (solo Development; grupo **Módulos**. Asistencia va en **Attendance**)
- **OpenAPI JSON:** http://localhost:5090/openapi/v1.json

La UI admin del frontend vive en `/admin/modulos`. Cambio incompatible de contrato: los módulos usan `trainingLines` y `trainingLineCodes`; el frontend debe adaptarse. La ruta anónima de asistencia se conserva.

## Autenticación

Todas las rutas de esta feature exigen:

```http
Authorization: Bearer <jwt>
```

Sin token → `401`. Con token pero sin esos roles → `403`.

En Scalar: botón **Authorize**, pegar `Bearer <token>` obtenido de `POST /api/v1/auth/login`.

## Contrato HTTP

Prefijo global `api`. JSON camelCase.

### `GET /api/v1/admin/modules`

| | |
| - | - |
| 200 | `TrainingModuleDto[]` |
| 401 / 403 | sin cuerpo de negocio |

### `GET /api/v1/admin/modules/{code}`

| | |
| - | - |
| 200 | `TrainingModuleDto` |
| 404 | Problem Details |

### `POST /api/v1/admin/modules`

```json
{
  "code": "mod-nuevo",
  "name": "Módulo nuevo",
  "description": "Opcional",
  "trainingLineCodes": ["AE", "PJ"]
}
```

| | |
| - | - |
| 201 | `TrainingModuleDto` + `Location` |
| 400 | código/nombre vacíos o demasiado largos; líneas ausentes, vacías, repetidas o con código vacío/mayor a 20 caracteres |
| 404 | algún código de `trainingLineCodes` inexistente |
| 409 | código duplicado |

`code` máx. 30, `name` máx. 150.

### `PUT /api/v1/admin/modules/{code}`

El `{code}` de la ruta identifica el módulo y **no se modifica**. Body: `name`, `description`, `trainingLineCodes` (`code` del body se ignora). Las líneas reemplazan el conjunto anterior; mínimo una, sin duplicados.

| | |
| - | - |
| 200 | `TrainingModuleDto` |
| 400 | validación |
| 404 | módulo o línea inexistente |

### `DELETE /api/v1/admin/modules/{code}`

| | |
| - | - |
| 204 | sin cuerpo |
| 404 | módulo inexistente |
| 409 | hay asistencia, sesiones o plantillas asociadas (no se borra) |

### `GET /api/v1/admin/training-lines`

| | |
| - | - |
| 200 | `TrainingLineDto[]` (`id`, `code`, `name`, `description`) |

Líneas de dominio: `PJ`, `GI`, `AE`. Este cambio no agrega semillas.

### `TrainingModuleDto`

Igual que asistencia: `id`, `code`, `name`, `description`, `trainingLines` (lista de `TrainingLineDto`, ordenada por código).

## Cómo probar en Scalar

1. `POST /api/v1/auth/login` con `{ "identifier": "pablo.rocha", "password": "1234" }`.
2. Copiar `token`.
3. Authorize → `Bearer <token>`.
4. `GET /api/v1/admin/training-lines` y `GET /api/v1/admin/modules`.
5. `POST /api/v1/admin/modules`, `PUT /api/v1/admin/modules/{code}` y `DELETE /api/v1/admin/modules/{code}`.

`maricel.rocha` (`DISTRICT_SECRETARY`) también puede. `teresa.aillon` recibe `403`.

## Archivos

| Capa | Archivo |
| ---- | ------- |
| Api | `Controllers/AdminCatalogController.cs` (`EndpointSummary` / `EndpointDescription` para Scalar) |
| Application | handlers create/update/delete + `ListTrainingLinesQueryHandler` |
| Domain | `ITrainingLineRepository`, `GetTrackedByCodeAsync` |
| Infrastructure | `TrainingLineRepository` |

## Persistencia

Hay dos rutas, con un único contexto e historial de migraciones. Comandos y requisitos: [Inicialización y migraciones](../database-migrations.md).

- **Base nueva y vacía:** generar SQL con `tools/New-DatabaseBaseline.ps1` e importarlo **antes del primer arranque de la aplicación**. Crea directamente `training_line`, `training_level` y `training_module_line`, sin crear `module_type` ni ejecutar la cadena histórica de cambios. Registra todas las migraciones actuales en `__EFMigrationsHistory`; luego `dotnet ef database update` y el arranque habitual solo aplican las futuras.
- **Base existente:** conservar datos e historial; aplicar migraciones incrementales. `IntroduceTrainingLines` renombra `module_type` a `training_line`, conserva sus filas y transfiere las pertenencias existentes a `training_module_line` (clave compuesta módulo/línea).

`TrainingLevel.TrainingLineId` es obligatorio; no hay pertenencia módulo–nivel.

Antes de aplicar la ruta incremental, clasificar cualquier nivel con `module_type_id` nulo; la migración rechaza esos datos sin asignar una línea arbitraria. El rollback rechaza módulos con cero o varias líneas para evitar pérdida de pertenencias. No se modifica la migración inicial ni la política de arranque (incluido el marcado de historial heredado); por eso la inicialización limpia debe importarse antes del primer arranque.

El mínimo de una línea se valida en los comandos de alta/edición; las FKs y la clave compuesta garantizan referencias válidas y pares únicos. Escrituras directas a la base deben respetar también ese mínimo.
