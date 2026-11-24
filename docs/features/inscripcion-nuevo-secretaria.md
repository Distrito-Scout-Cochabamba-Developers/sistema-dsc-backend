# Alta e inscripción asistida

La creación del perfil mínimo y la inscripción comparten una transacción PostgreSQL READ COMMITTED. El cupo se reserva mediante la operación común de inscripción. La cuenta pendiente y su invitación se confirman o revierten con ellas; el envío del correo ocurre después y nunca afecta la inscripción (AB#161).

## Regla de complemento del CI

Las escrituras asistidas (alta de dirigente, alta de Rover sin cuenta e inscripción de una persona existente) serializan por número de CI con un bloqueo advisory transaccional y consultan nuevamente la identidad. Ante ambigüedad nunca se elige ni se crea automáticamente: la API responde 409 con `type: urn:dsc:secretariat:extension-required` y el panel pide el complemento para desambiguar. Se aplica cuando:

- la búsqueda por CI tiene varias coincidencias, o se indica un complemento sin coincidencia pero el CI existe bajo otros;
- un alta (dirigente o Rover) se solicita para un CI que ya existe solo bajo otros complementos (o sin complemento cuando existe con uno);
- la inscripción de una persona existente envía un CI con varias personas y sin complemento.

Si el CI ya existe con el mismo complemento (el vacío equivale al ausente; se ignoran mayúsculas) no se crea nada: el alta de dirigente responde `urn:dsc:secretariat:identity-changed` y el alta de Rover `urn:dsc:enrollment:person-exists`. La respuesta nunca incluye nombres ni identificadores de persona. No se modifica la unicidad global (`ux_person_ci_extension`) ni se corrigen datos históricos ambiguos.

## Asistencia digital (QR): excepción a la especificación

La asistencia digital por QR conserva su cambio: ahora crea a la persona dentro de una transacción, toma el mismo bloqueo por CI y resuelve los catálogos por código. Es una excepción deliberada a la especificación original, adoptada para evitar carreras entre canales de alta (asistencia digital, inscripción asistida y alta de Rover), que antes podían duplicar personas o agotar el cupo de forma inconsistente. No debe revertirse sin reemplazar esa protección.

## Correo de invitación (SMTP)

El envío ocurre dentro de la solicitud de alta, por lo que está acotado: `Smtp__TimeoutSeconds` (por defecto 5, entre 1 y 30) limita conexión, saludo y entrega. Al vencer, la invitación queda PENDING y se reintenta manualmente; la inscripción confirmada no se ve afectada.

## Limitación de tasa

El repositorio aún no usa el limitador de tasa de ASP.NET Core (`AddRateLimiter`), por lo que `POST /v1/auth/activate` y `POST /v1/secretariat/invitations/{id}/retry` no tienen límite propio. Queda pendiente definir una política común y, mientras tanto, aplicar límites en el proxy inverso.
