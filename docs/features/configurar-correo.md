# Configurar el correo de invitaciones

Tras el alta asistida de un dirigente nuevo, la API envía un correo con su nombre de usuario y un enlace de activación (ver [invitacion-correo.md](./invitacion-correo.md)). Esta guía explica cómo conectar el envío con una cuenta de correo.

El transporte usa SMTP mediante MailKit (`SmtpAccountInvitationMailer`).

## Variables

Se definen en `.env` (copia de `.env.example`) o como variables de entorno del servidor.

| Variable | Valor por defecto | Descripción |
| --- | --- | --- |
| `Smtp__Host` | — | Servidor SMTP. Obligatoria. |
| `Smtp__Port` | `587` | Puerto del servidor. |
| `Smtp__EnableSsl` | `true` | Con `true`, el puerto 465 usa TLS implícito y los demás usan STARTTLS. |
| `Smtp__Username` | — | Usuario de la cuenta. |
| `Smtp__Password` | — | Clave de la cuenta. |
| `Smtp__From` | — | Remitente. Obligatoria. |
| `Smtp__TimeoutSeconds` | `5` | Tiempo máximo de envío (1 a 30 s). |
| `Activation__FrontendOrigin` | — | Origen HTTPS del frontend, sin ruta. Se usa para armar el enlace. |
| `Activation__AllowLoopbackHttp` | `false` | Con `true` se acepta `http://localhost:<puerto>` como origen. Solo para pruebas locales. |

Sin `Smtp__Host` o `Smtp__From`, las invitaciones quedan en estado `PENDING` y no se pierde nada. Lo mismo ocurre si `Activation__FrontendOrigin` no es HTTPS.

## Conexión guiada

El script `scripts/setup-correo.sh` te lleva paso a paso con Gmail y escribe las variables en `.env`:

```bash
bash scripts/setup-correo.sh
```

Puedes volver a ejecutarlo cuando quieras. Conserva los valores ya guardados.

## Ejemplo con Gmail

```
Smtp__Host="smtp.gmail.com"
Smtp__Port=587
Smtp__EnableSsl=true
Smtp__Username="tu-correo@gmail.com"
Smtp__Password="clave-de-aplicacion"
Smtp__From="tu-correo@gmail.com"
Activation__FrontendOrigin="https://dsc.example.test"
```

En Gmail, `Smtp__Password` es una contraseña de aplicación (se crea en la cuenta de Google) y `Smtp__From` debe ser la misma cuenta. Gmail sirve para pruebas; tiene un tope aproximado de 500 envíos diarios.

## Probar el envío

1. Reinicia la API.
2. En el panel de Secretaría, inscribe a un dirigente nuevo con un correo al que tengas acceso.
3. La respuesta `201` trae `invitation`:
   - `SENT`: el correo salió.
   - `PENDING`: el envío falló. El panel muestra "Reintentar envío del correo".
4. Si queda `PENDING`, el campo `last_failure_code` de la invitación guarda solo el tipo de error:

| Código | Causa probable |
| --- | --- |
| `InvalidActivationOrigin` | `Activation__FrontendOrigin` falta o no es HTTPS sin ruta. |
| `InvalidOperationException` | Falta `Smtp__Host` o `Smtp__From`. |
| `TimeoutException` | El servidor no respondió dentro de `Smtp__TimeoutSeconds`. |
| `AuthenticationException` | Usuario o clave rechazados por el servidor. |

## Enlace de activación en local

Para probar sin publicar el frontend:

```
Activation__FrontendOrigin="http://localhost:4200"
Activation__AllowLoopbackHttp=true
```

El enlace del correo abrirá `http://localhost:4200/auth/activar#token=...`, así que debes abrirlo desde el mismo equipo donde corre el frontend. Con `AllowLoopbackHttp=true` solo se aceptan hosts de loopback; cualquier otro origen `http` sigue rechazado.
