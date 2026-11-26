# Invitación por correo de la inscripción asistida (AB#161)

Alcance deducido de la spec de la Historia 2.3 (sin acceso al tablero): tras confirmar la inscripción asistida de un dirigente nuevo, enviar un correo con su nombre de usuario y un enlace de activación de un solo uso (AB#162), nunca una contraseña, sin alterar la inscripción.

## Flujo

1. `POST /api/v1/secretariat/participants/sessions/{id}/enrollments` crea en una sola transacción: perfil, inscripción, cuenta `PENDING_VERIFICATION` (sin contraseña, usuario aleatorio `dsc-xxxxxxxxxxxx`) y una fila `account_invitation` en `PENDING`. Si falla cualquier paso (cupo, ventana, duplicado, correo ya usado: `409 urn:dsc:secretariat:email-in-use`) no queda nada.
2. Ya confirmada la transacción se intenta la entrega una vez: se emite el token (`AccountActivation.IssueAsync`), se envía el correo y la fila pasa a `SENT`. La respuesta `201` incluye `invitation: "SENT" | "PENDING"`; un fallo de correo no cambia el `201` ni la inscripción.
3. Si falla (SMTP caído, configuración ausente, reinicio antes del envío) la fila permanece `PENDING` en PostgreSQL con `attempts` y `last_failure_code` (solo el tipo de la excepción). No hay reintento automático.
4. Reintento MANUAL, solo `DISTRICT_SECRETARY`: `POST /api/v1/secretariat/invitations/{enrollmentId}/retry` (`200` enviado; `502 delivery-failed` sigue pendiente; `409 already-sent`; `409 not-eligible` si la cuenta ya se activó, la invitación pasa a `CLOSED`; `404`). `GET /api/v1/secretariat/invitations/pending` lista `enrollmentId`, intentos, código de fallo y fecha, sin PII.

## Garantías y límites

- Entrega "al menos una vez", no exactamente una. Si el correo sale pero la confirmación en BD falla, o el servidor SMTP lo acepta y se pierde, un reintento envía otro enlace (el anterior queda invalidado por la reemisión). No se promete lo contrario.
- Un bloqueo de fila (`SELECT ... FOR UPDATE`) serializa emisión y envío por cuenta: reintentos simultáneos envían un solo correo.
- Enlace: `https://<Activation__FrontendOrigin>/auth/activar#token=<43 caracteres>`; el origen viene de configuración del servidor y debe ser HTTPS sin ruta, si no la entrega falla como `InvalidActivationOrigin`. El token va en el fragmento (no llega al servidor ni a logs de proxy). No contiene Carnet de Identidad, correo ni nombre.
- No se registra correo, enlace, token ni mensaje de excepción (puede contener correo); solo id de invitación y tipo de excepción.
- Fuera de alcance: invitar a dirigentes existentes sin cuenta; Rover (no tiene cuenta por diseño).

## Configuración (fuera de Git)

Variables en `.env` (ignorado) o entorno: `Smtp__Host`, `Smtp__Port` (587), `Smtp__EnableSsl` (true), `Smtp__Username`, `Smtp__Password`, `Smtp__From`, `Activation__FrontendOrigin`. `.env.example` solo contiene valores ficticios. No hay proveedor SMTP elegido: confirmar proveedor, SPF/DKIM del remitente y que el proveedor no reescriba enlaces (seguimiento de clics).

## Pruebas

`tests/DSC.UnitTests/Api/AssistedEnrollmentInvitationHttpTests.cs`: transporte ficticio `IAccountInvitationMailer`, PostgreSQL efímero, datos ficticios.
