# Administración del catálogo de módulos

- **Estado:** Entregado (2026-10-22)
- **Roles:** `ADMIN`, `DISTRICT_SECRETARY` (claim JWT `roles`)
- **Scalar:** http://localhost:5090/scalar/v1 (solo Development; grupo **Módulos**. Asistencia va en **Attendance**)
- **OpenAPI JSON:** http://localhost:5090/openapi/v1.json

La UI admin del frontend vive en `/admin/modulos`. El GET anónimo de asistencia (`/api/attendance/modules`) no cambia.

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
  "moduleTypeCode": "AE"
}
```

| | |
| - | - |
| 201 | `TrainingModuleDto` + `Location` |
| 400 | código/nombre/tipo vacíos o demasiado largos |
| 404 | `moduleTypeCode` inexistente |
| 409 | código duplicado |

`code` máx. 30, `name` máx. 150.

### `PUT /api/v1/admin/modules/{code}`

El `{code}` de la ruta identifica el módulo y **no se modifica**. Body: `name`, `description`, `moduleTypeCode` (`code` del body se ignora).

| | |
| - | - |
| 200 | `TrainingModuleDto` |
| 400 | validación |
| 404 | módulo o tipo inexistente |

### `DELETE /api/v1/admin/modules/{code}`

| | |
| - | - |
| 204 | sin cuerpo |
| 404 | módulo inexistente |
| 409 | hay asistencia, sesiones o plantillas asociadas (no se borra) |

### `GET /api/v1/admin/module-types`

| | |
| - | - |
| 200 | `ModuleTypeDto[]` (`id`, `code`, `name`, `description`) |

Tipos locales de Docker: `AE`, `PJ`.

### `TrainingModuleDto`

Igual que asistencia: `id`, `code`, `name`, `description`, `moduleTypeCode`, `moduleTypeName`.

## Cómo probar en Scalar

1. `POST /api/v1/auth/login` con `{ "identifier": "pablo.rocha", "password": "1234" }`.
2. Copiar `token`.
3. Authorize → `Bearer <token>`.
4. `GET /api/v1/admin/module-types` y `GET /api/v1/admin/modules`.
5. `POST /api/v1/admin/modules`, `PUT /api/v1/admin/modules/{code}` y `DELETE /api/v1/admin/modules/{code}`.

`maricel.rocha` (`DISTRICT_SECRETARY`) también puede. `teresa.aillon` recibe `403`.

## Archivos

| Capa | Archivo |
| ---- | ------- |
| Api | `Controllers/AdminCatalogController.cs` (`EndpointSummary` / `EndpointDescription` para Scalar) |
| Application | handlers create/update/delete + `ListModuleTypesQueryHandler` |
| Domain | `IModuleTypeRepository`, `GetTrackedByCodeAsync` |
| Infrastructure | `ModuleTypeRepository` |
