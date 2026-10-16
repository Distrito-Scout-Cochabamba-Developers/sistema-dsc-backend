# Catálogos que exige el registro de asistencia

- **Estado:** Semilla local (2026-10-16). El handler **no** se cambió: sigue usando IDs fijos.
- **Síntoma que se corrigió:** `POST /api/attendance/register` devolvía **500** (`DbUpdateException` / PostgreSQL `23503`) al insertar el teléfono.

## Causa

`RegisterDigitalAttendanceCommandHandler` crea `Person` + `PersonContactMechanism` con IDs mágicos:

| Campo | Id | Código esperado |
| ----- | -- | --------------- |
| `Person.PersonStatusId` | 1 | `ACTIVE` |
| `PersonContactMechanism.ContactPurposeTypeId` | 1 | `PERSONAL` |
| `PersonContactMechanism.ContactMechanismTypeId` | 2 | `CELLPHONE` |

`LookupLeaderByCiQueryHandler` también trata el tipo `2` como celular.

En Docker esas tablas de catálogo estaban vacías (`contact_mechanism_type`, `contact_purpose_type`). El insert a `person_contact_mechanism` violaba la FK.

`person_status` `ACTIVE` ya era `id = 1` por la semilla de usuario.

## Semilla (workspace Compose)

Archivo: `docker/seed-dev-user.sql` (idempotente, `OVERRIDING SYSTEM VALUE` para alinear ids).

| Tabla | Id | `code` |
| ----- | -- | ------ |
| `person_status` | 1 | `ACTIVE` |
| `contact_purpose_type` | 1 | `PERSONAL` |
| `contact_mechanism_type` | 1 | `EMAIL` |
| `contact_mechanism_type` | 2 | `CELLPHONE` |

Tras sembrar, el POST responde **201**. Un segundo POST del mismo CI al mismo módulo responde **409**.

## Deuda

Resolver los tipos por `code` (`CELLPHONE`, `PERSONAL`, `ACTIVE`) en el handler, en lugar de IDs fijos. Hasta entonces, cada entorno debe tener esas filas con esos ids.

## Relacionado

- Handler: `src/DSC.Application/Commands/RegisterDigitalAttendanceCommandHandler.cs`
- Lookup: `src/DSC.Application/Queries/LookupLeaderByCiQueryHandler.cs`
- Catálogo de módulos: [catalogo-modulos.md](./catalogo-modulos.md)
