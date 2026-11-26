# Documentación — DSC Backend

Índice de cambios y contratos de features. La guía de capas sigue en [ARCHITECTURE.md](../ARCHITECTURE.md).

| Documento | Contenido |
| --------- | --------- |
| [features/catalogo-modulos.md](./features/catalogo-modulos.md) | `GET /api/attendance/modules` y `GET /api/attendance/modules/{code}` |
| [features/calendario-sesiones.md](./features/calendario-sesiones.md) | Calendario de programaciones e inscripción digital |
| [features/admin-catalogo-modulos.md](./features/admin-catalogo-modulos.md) | CRUD admin `/api/v1/admin/modules` (Scalar) |
| [features/catalogos-asistencia.md](./features/catalogos-asistencia.md) | Filas de catálogo de módulos que exige `POST /api/attendance/register` |
| [features/busqueda-secretaria.md](./features/busqueda-secretaria.md) | Búsqueda de participantes por Carnet de Identidad para Secretaría |
| [features/configurar-correo.md](./features/configurar-correo.md) | Conectar el envío de correo de invitaciones (SMTP) |
| [database-migrations.md](./database-migrations.md) | Inicialización limpia para bases vacías; migraciones incrementales para bases existentes |

Semillas locales del workspace (no viven en este repo): `../docker/seed-dev-modules.sql` y `../docker/seed-dev-user.sql`.
