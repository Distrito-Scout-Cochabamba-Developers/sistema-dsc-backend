# Calendario de convocatorias e inscripción

- **Estado:** Entregado (2026-11-21)
- **Scalar:** grupo **Calendario**. Alta de convocatorias en **Módulos**.
- **Frontend:** `/calendario` con `trainingCalendarApiEnabled: true`

Fecha y hora **no** viven en el catálogo ni en la sesión. El listado proyecta `training_module` + `training_module_session` + `training_module_schedule`.

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
| 404 | convocatoria inexistente o sin horarios |
| 409 | ya inscrito, cupo agotado, inscripción cerrada, cancelada o finalizada |

Una persona no puede tener dos filas en `training_module_enrollment` para la misma sesión (índice único).

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

`statusCode` por defecto `OPEN`. Hace falta al menos un bloque. `facilityId` opcional (sede y cupo).

## `CalendarSessionDto`

| Campo | Origen |
| ----- | ------ |
| `id` | `training_module_session.id` |
| `moduleCode` | `training_module.id` en texto (el catálogo ya no tiene `code`) |
| `moduleName` | `training_module.name` |
| `moduleTypeCode` / `moduleTypeName` | primera línea de formación del módulo (orden por código) |
| `startsAt` / `endsAt` | primer y último bloque de `training_module_schedule` |
| `location` | `facility.name` del primer bloque con sede |
| `status` | ver tabla de estados |
| `capacity` | `facility.capacity` o `null` (sin límite) |
| `enrolledCount` | filas de inscripción |
| `remainingSeats` | `capacity - enrolledCount`, o `null` |
| `isEnrolled` | la persona del JWT ya está inscrita |

### Estados

Catálogo `training_module_session_status` → JSON del calendario:

| BD | API |
| -- | --- |
| `PLANNED` | `SCHEDULED` |
| `OPEN` | `OPEN` |
| `IN_PROGRESS` | `IN_PROGRESS` |
| `CLOSED` | `FINISHED` |
| `CANCELLED` | `CANCELLED` |
| cupo 0 (si no está cerrada/cancelada) | `FULL` |

## Datos locales

El seeder (`tools/DSC.Seeder`) crea estados, orígenes de inscripción y tres convocatorias demo (Primeros auxilios, Liderazgo, Trabajo en Equipo) ancladas a hoy, si esos módulos existen.

Sin sesiones, el GET devuelve `[]` y el calendario queda vacío (ya no usa fixtures).

## Cómo probar

1. Sembrar catálogo + usuarios.
2. `GET /api/v1/calendar/sessions`.
3. Login `pablo.rocha` / `1234`.
4. `POST /api/v1/calendar/sessions/{id}/enrollments` con Bearer.
5. Frontend: http://localhost:4200/calendario

## Fuera de alcance

- UI admin de convocatorias (solo Scalar / POST admin).
- Inscripción por Secretaría (CI).
- Una tarjeta de calendario por cada día de un dictado de varios bloques: la grilla usa `startsAt` (primer bloque).
