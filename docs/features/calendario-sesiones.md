# Calendario de programaciones e inscripción

Actualmente `TrainingModuleSession` representa la programación inscribible y `training_module_schedule` almacena bloques de horario. Estos bloques no implementan todavía sesiones como encuentros independientes con disertante y modalidad propios.

- **Estado:** Entregado (2026-11-21)
- **Scalar:** grupo **Calendario**. Alta de programaciones en **Módulos**.
- **Frontend:** `/calendario` con `trainingCalendarApiEnabled: true`

Fecha y hora **no** viven en el catálogo de módulos ni en la sesión. El listado proyecta `training_module` + `training_module_session` + `training_module_schedule`.

## Contrato HTTP

Prefijo global `api`. JSON camelCase. Offset de horas: **America/La Paz (−04:00)**.

### `GET /api/v1/calendar/sessions?from&to`

Público. `from` / `to` opcionales (`YYYY-MM-DD`) filtran por `session_date` de cualquier bloque. Con `Authorization: Bearer`, `isEnrolled` usa el `personId` del JWT.

| | |
| - | - |
| 200 | `CalendarSessionDto[]` (puede ser `[]`) |

### `POST /api/v1/calendar/sessions/{id}/enrollments`

Cuerpo vacío. Cualquier usuario autenticado. Origen `enrollment_source.code = AUTHENTICATED_USER`.

| | |
| - | - |
| 201 | `{ enrollmentId, session }` |
| 401 | sin JWT |
| 400 | el JWT no trae `personId` |
| 404 | programación inexistente o sin horarios |
| 409 | ya inscrito, cupo agotado, inscripción cerrada, cancelada o finalizada |

Una persona no puede tener dos filas en `training_module_enrollment` para la misma sesión (índice único).

La modalidad `rover_without_account` solo se conoce para inscripciones nuevas: `true` para Rover sin cuenta y `false` para las demás. Las históricas conservan `NULL`; no se infiere su clasificación. Al confirmarse como Rover, se verifica la ausencia de cuenta en la misma transacción; una cuenta puede crearse después.

### `POST /api/v1/admin/modules/{id}/sessions`

Roles `ADMIN` o `DISTRICT_SECRETARY`.

```json
{
  "statusCode": "OPEN",
  "isRegistrationOpen": true,
  "schedules": [
    {
      "sessionDate": "2026-11-28",
      "startTime": "08:30:00",
      "endTime": "12:30:00",
      "facilityId": null,
      "partNumber": 1,
      "partTitle": null
    }
  ]
}
```

`statusCode` por defecto `OPEN`. Hace falta al menos un bloque. `facilityId` es opcional.

- Si se omite `capacity`, se guarda el cupo del primer bloque con sede (orden por parte, fecha, hora e ID/desempate por orden de alta); si esa sede no indica cupo o no hay sede, se guarda `null` (sin límite). No se recalcula si cambia la sede.
- `capacity` explícito acepta enteros no negativos (`0` = sin plazas). Secretaría puede fijar un cupo igual o menor al predeterminado. Solo `ADMIN` puede superar ese valor cuando es finito o enviar explícitamente `capacity: null`; otros reciben `403`. Con un predeterminado sin límite, un entero fija un límite.

### `PUT /api/v1/admin/sessions/{id}/capacity`

Solo `ADMIN`. Cuerpo `{"capacity": 35}` para ampliar, o `{"capacity": null}` para quitar el límite. `capacity` es obligatorio, aunque puede ser `null`.

Devuelve `204` al ampliar o repetir el valor actual; `400` por cuerpo inválido, `401` sin JWT, `403` sin el rol, `404` si no existe y `409` si reduciría el cupo, incluso de sin límite a finito. No modifica inscripciones ni reabre la ventana de inscripción.

## `CalendarSessionDto`

| Campo | Origen |
| ----- | ------ |
| `id` | `training_module_session.id` |
| `moduleCode` | `training_module.id` en texto (el catálogo de módulos ya no tiene `code`) |
| `moduleName` | `training_module.name` |
| `moduleTypeCode` / `moduleTypeName` | primera línea de formación del módulo (orden por código) |
| `startsAt` / `endsAt` | primer y último bloque de `training_module_schedule` |
| `location` | `facility.name` del primer bloque con sede |
| `status` | ver tabla de estados |
| `capacity` | `training_module_session.capacity` o `null` (sin límite) |
| `enrolledCount` | filas de inscripción |
| `remainingSeats` | `max(0, capacity - enrolledCount)`, o `null` |
| `isEnrolled` | la persona del JWT ya está inscrita |

### Estados

Catálogo de módulos `training_module_session_status` → JSON del calendario:

| BD | API |
| -- | --- |
| `PLANNED` | `SCHEDULED` |
| `OPEN` | `OPEN` |
| `IN_PROGRESS` | `IN_PROGRESS` |
| `CLOSED` | `FINISHED` |
| `CANCELLED` | `CANCELLED` |
| cupo 0 (si no está cerrada/cancelada) | `FULL` |

## Datos locales

El seeder (`tools/DSC.Seeder`) crea estados, orígenes de inscripción y tres programaciones demo (Primeros auxilios, Liderazgo, Trabajo en Equipo) ancladas a hoy, si esos módulos existen.

Sin sesiones, el GET devuelve `[]` y el calendario queda vacío (ya no usa fixtures).

## Cómo probar

1. Sembrar catálogo de módulos + usuarios.
2. `GET /api/v1/calendar/sessions`.
3. Login `pablo.rocha` / `1234`.
4. `POST /api/v1/calendar/sessions/{id}/enrollments` con Bearer.
5. Frontend: http://localhost:4200/calendario

## Fuera de alcance

- UI admin de programaciones (solo API).
- Inscripción por Secretaría (Carnet de Identidad).
- Una tarjeta de calendario por cada día de un dictado de varios bloques: la grilla usa `startsAt` (primer bloque).
