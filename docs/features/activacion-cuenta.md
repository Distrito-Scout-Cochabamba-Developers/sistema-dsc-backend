# Activación de cuenta (AB#162)

## Emisión interna para AB#161

1. Crear la `User` con `UserStatusId` del código `PENDING_VERIFICATION`, `Password = string.Empty` e `IsEmailVerified = false`. Resolver el estado por código, no por ID fijo. Confirmar el alta e inscripción antes de enviar correo.
2. En un scope de DI, tras confirmar el alta, llamar a `DSC.Infrastructure.AccountActivation.IssueAsync(userId, cancellationToken)`. Devuelve un token opaco Base64URL (43 caracteres, 256 bits aleatorios) o `null` si la cuenta no existe, ya no está pendiente o tiene contraseña. No se publica un endpoint de emisión. Caduca en 24 horas; emitir de nuevo invalida el enlace anterior. Serializar emisión y envío por cuenta en el flujo de reintentos de AB#161.
3. Enviar el nombre de usuario y `https://<frontend-configurado>/auth/activar#token=<token>`. Usar el origen HTTPS configurado, no datos del cliente, y evitar redirecciones de seguimiento en el correo. No persistir ni registrar el token bruto, el enlace completo ni la contraseña. Ante fallo de correo, emitir otro token sin reescribir la inscripción.

## Canje público

`POST /api/v1/auth/activate`, JSON `{ "token": "…", "password": "…" }`, sin requerir JWT. Abrir el enlace no hace solicitudes de activación. La contraseña requiere al menos 12 caracteres, máximo 72 bytes UTF-8 y no solo espacios; no se recorta ni normaliza.

- `204 No Content`: contraseña BCrypt, estado `ACTIVE`, correo verificado y consumo del token en un único `UPDATE` condicional. Sin JWT; se usa el login existente.
- `400 Problem Details`: no distingue token desconocido, expirado, ya usado, cuenta no elegible o contraseña rechazada. Un fallo no consume el enlace.

El token vive como SHA-256 en `user.activation_token_hash`. Solo hay uno vigente por cuenta; el estado, hash y expiración se comprueban al escribir. Dos canjes concurrentes no pueden ganar. La migración agrega columnas anulables y asegura el catálogo `PENDING_VERIFICATION` / `ACTIVE`, sin cambiar cuentas existentes.

Mantener desactivada la captura de cuerpos HTTP de esta ruta (aplicación, proxy y telemetría), el registro sensible de EF y el registro de enlaces en el correo. El fragmento URL no viaja al servidor. El endpoint pide `Cache-Control: no-store`. El correo, alta y reintentos pertenecen a AB#161; no cambia Rover.
