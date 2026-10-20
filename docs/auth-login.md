# Autenticación: `POST /api/v1/auth/login` y `GET /api/v1/auth/me`

La especificación OpenAPI formal y su interfaz interactiva están disponibles en tiempo de ejecución en:
- **Interfaz visual interactiva (Scalar)**: `http://localhost:5090/scalar/v1`
- **Especificación OpenAPI v3.0.1 (JSON)**: `http://localhost:5090/openapi/v1.json`

---

## Casos de uso documentados

### 1. Login por Username (caso exitoso)

Autentica utilizando el nombre de usuario registrado.

**Solicitud:**
```http
POST /api/v1/auth/login
Content-Type: application/json
```
```json
{
  "identifier": "pablo.rocha",
  "password": "<contraseña>"
}
```

**Respuesta (`200 OK`):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2026-10-21T03:00:00+00:00",
  "user": {
    "id": 2,
    "personId": 2,
    "username": "pablo.rocha",
    "email": "pablo.rocha@example.test",
    "roles": [
      "ADMIN",
      "DISTRICT_DIRECTOR"
    ]
  }
}
```

---

### 2. Login por Email (caso exitoso)

El campo `identifier` acepta indistintamente el correo electrónico o el username, sin distinguir mayúsculas ni minúsculas.

**Solicitud:**
```http
POST /api/v1/auth/login
Content-Type: application/json
```
```json
{
  "identifier": "pablo.rocha@example.test",
  "password": "<contraseña>"
}
```

**Respuesta (`200 OK`):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2026-10-21T03:00:00+00:00",
  "user": {
    "id": 2,
    "personId": 2,
    "username": "pablo.rocha",
    "email": "pablo.rocha@example.test",
    "roles": [
      "ADMIN",
      "DISTRICT_DIRECTOR"
    ]
  }
}
```

---

### 3. Casos negativos y validaciones

#### 3.1. Solicitud incompleta (`400 Bad Request`)

Si `identifier` o `password` están ausentes, vacíos o contienen únicamente espacios en blanco.

**Solicitud:**
```http
POST /api/v1/auth/login
Content-Type: application/json
```
```json
{
  "identifier": "",
  "password": "<contraseña>"
}
```

**Respuesta (`400 Bad Request` - RFC 7807 Problem Details):**
```json
{
  "title": "Solicitud Inválida",
  "status": 400,
  "detail": "El identificador y la contraseña son obligatorios.",
  "instance": "/api/v1/auth/login"
}
```

#### 3.2. Credenciales inválidas o cuenta no activa (`401 Unauthorized`)

Si el usuario no existe, la contraseña no coincide con el hash BCrypt, el hash almacenado es inválido o la cuenta no tiene estado `ACTIVE`. El mensaje es deliberadamente genérico para no filtrar si el usuario existe.

**Solicitud:**
```http
POST /api/v1/auth/login
Content-Type: application/json
```
```json
{
  "identifier": "pablo.rocha",
  "password": "contraseña_incorrecta"
}
```

**Respuesta (`401 Unauthorized` - RFC 7807 Problem Details):**
```json
{
  "title": "Credenciales Inválidas",
  "status": 401,
  "detail": "Las credenciales son inválidas.",
  "instance": "/api/v1/auth/login"
}
```

#### 3.3. Contraseña antigua en texto plano (`401 Unauthorized`)

Las contraseñas heredadas en texto plano son rechazadas automáticamente por el validador BCrypt sin exponer stack trace.

**Solicitud:**
```http
POST /api/v1/auth/login
Content-Type: application/json
```
```json
{
  "identifier": "wimpi",
  "password": "<contraseña_antigua>"
}
```

**Respuesta (`401 Unauthorized`):**
```json
{
  "title": "Credenciales Inválidas",
  "status": 401,
  "detail": "Las credenciales son inválidas.",
  "instance": "/api/v1/auth/login"
}
```

#### 3.4. Ruta antigua retirada (`404 Not Found`)

Las rutas legacy `/api/auth/login`, `/api/auth/login/email` y `/api/auth/login/username` fueron retiradas del controlador.

**Solicitud:**
```http
POST /api/auth/login
Content-Type: application/json
```

**Respuesta (`404 Not Found`):**
```http
Status: 404 Not Found
```

---

### 4. Sesión autenticada: `GET /api/v1/auth/me`

#### 4.1. Con token Bearer válido (`200 OK`)

Devuelve los datos del perfil y roles decodificados desde el JWT.

**Solicitud:**
```http
GET /api/v1/auth/me
Authorization: Bearer <token_jwt>
Accept: application/json
```

**Respuesta (`200 OK`):**
```json
{
  "id": 2,
  "personId": 2,
  "username": "pablo.rocha",
  "email": "pablo.rocha@example.test",
  "roles": [
    "ADMIN",
    "DISTRICT_DIRECTOR"
  ]
}
```

#### 4.2. Sin cabecera de autorización o token inválido (`401 Unauthorized`)

**Solicitud:**
```http
GET /api/v1/auth/me
Accept: application/json
```

**Respuesta (`401 Unauthorized`):**
```http
Status: 401 Unauthorized
```

---

## Contrato y modelo de datos

## Claims del JWT

| Claim | Contenido |
|---|---|
| `sub` | ID de la cuenta (`user.id`). |
| `email` | Email de la cuenta. |
| `personId` | ID de la persona (`user.person_id`). |
| `username` | Nombre de usuario. |
| `roles` | Uno por cada rol activo y vigente. |
| `exp`, `iss`, `aud` | Expiración, emisor y audiencia configurados en `Jwt`. |

La API valida el token con `MapInboundClaims = false`, por lo que los nombres de claim se conservan tal cual. `RoleClaimType` es `roles`.

### Reglas de roles

Solo se emiten roles que cumplen las tres condiciones:

1. `role.is_active = true`.
2. `user_role.valid_from <= hoy`.
3. `user_role.valid_until` es `NULL` o `>= hoy`.

Los códigos son los de la tabla `role` (por ejemplo `ADMIN`, `DISTRICT_DIRECTOR`, `BRANCH_COMMISSIONER`). No se usan abreviaturas.

## Contraseñas

Las contraseñas se almacenan con **BCrypt** (`BCrypt.Net-Next`, coste 12). Los hashes heredados en texto plano se rechazan; la cuenta existente debe resetear su contraseña antes de volver a iniciar sesión.

## Endpoints retirados

Se eliminaron las rutas anteriores:

- `POST /api/auth/login`
- `POST /api/auth/login/email`
- `POST /api/auth/login/username`

La ruta vigente es `POST /api/v1/auth/login`. `GET /api/v1/auth/me` requiere `Authorization: Bearer <token>`.

## Carga de datos de desarrollo

El manifiesto está en `tools/DSC.Seeder/dev-users.json`. El seeder es idempotente: reutiliza personas y cuentas existentes, no modifica contraseñas y solo agrega asignaciones de rol faltantes.

```powershell
$env:DSC_SEED_PASSWORD = "<contraseña-local>"
dotnet run --project tools/DSC.Seeder
```

Opcionalmente se puede fijar la fecha de inicio de las asignaciones:

```powershell
dotnet run --project tools/DSC.Seeder -- --date=2026-10-19
```

Para resetear la contraseña de **todas** las cuentas con un hash BCrypt nuevo (incluida `wimpi`), usar el flag explícito:

```powershell
$env:DSC_SEED_PASSWORD = "<contraseña-local>"
dotnet run --project tools/DSC.Seeder -- --reset-all-passwords
```

Este flag es destructivo: reemplaza la contraseña de todas las cuentas existentes. Sin el flag, una ejecución normal no toca contraseñas.

La cadena de conexión se toma de `DATABASE_URL` o `ConnectionStrings__DefaultConnection`, incluyendo el archivo `.env`.

### Casos de prueba en Postman

La colección de Postman se mantiene en el workspace del equipo y no se versiona en este repositorio. Contiene los casos de login por username y email, roles múltiples, respuestas `400`/`401`/`404`, `/me` con y sin Bearer, y validación de claims del JWT.

### Usuarios de desarrollo

| Username | Email | Roles |
|---|---|---|
| `pablo.rocha` | `pablo.rocha@example.test` | `ADMIN`, `DISTRICT_DIRECTOR` |
| `teresa.aillon` | `teresa.aillon@example.test` | `COMMUNICATIONS_OFFICER` |
| `maricel.rocha` | `maricel.rocha@example.test` | `DISTRICT_SECRETARY`, `FINANCE_COMMISSIONER` |
| `german.rocha` | `german.rocha@example.test` | `YOUTH_PROGRAM_COORDINATOR` |
| `daniel.vargas` | `daniel.vargas@example.test` | `ADULT_RESOURCES_COORDINATOR` |
| `jorge.revollo` | `jorge.revollo@example.test` | `QUARTERMASTER` |
| `apu` | `apu@example.test` | `OPERATIONS_OFFICER` |
| `ana.laura` | `ana.laura@example.test` | `BRANCH_COMMISSIONER` |
| `fabiola` | `fabiola@example.test` | `BRANCH_COMMISSIONER` |
| `erick` | `erick@example.test` | `BRANCH_COMMISSIONER` |
| `mariel` | `mariel@example.test` | `BRANCH_COMMISSIONER` |

La cuenta existente `wimpi` no tiene roles asignados. Su contraseña también quedó con hash BCrypt tras ejecutar `--reset-all-passwords`.

Todos tienen `user_status = ACTIVE`, `is_email_verified = false`, `person_status = ACTIVE` y sin fecha de fin de rol.
