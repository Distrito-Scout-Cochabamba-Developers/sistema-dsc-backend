# Guía de despliegue del sistema DSC

Actualizada el 22 de noviembre de 2026.

La publicación inicial se realizó el 26 de octubre. El pipeline del backend se verificó el 13 de noviembre. Esta edición documenta esos resultados y los pasos para repetir la subida. La disponibilidad actual de los sitios requiere una nueva comprobación.

## 1. Qué publicamos

| Parte | Servicio utilizado | Dirección |
| --- | --- | --- |
| Frontend Angular | Cloudflare Pages Free | [Abrir frontend](https://sistema-dsc-pruebas.pages.dev) |
| Backend ASP.NET Core .NET 10 | MonsterASP.NET Free | [Abrir API](https://dsc-api-pruebas.runasp.net) |
| Base de datos | PostgreSQL de MonsterASP | Acceso interno desde la API |

```text
Usuario
  └─ HTTPS → Angular en Cloudflare Pages
                └─ HTTPS + JWT → API en MonsterASP.NET
                                      └─ Red interna → PostgreSQL
```

Angular muestra las pantallas y envía solicitudes HTTP. La API autentica al usuario, ejecuta las operaciones y consulta la base.

| Repositorio local | Rama actual del trabajo |
| --- | --- |
| `E:/DSC DEV/sistema-dsc-frontend` | `front-pablo` |
| `E:/DSC DEV/sistema-dsc-backend` | `back-pablo` |

Durante el despliegue inicial el backend estaba en `pablo`. Después se publicaron los cambios en `back-pablo`.

La subida inicial se hizo con archivos compilados y los paneles de ambos servicios. Cada nueva versión del hosting requiere publicar su paquete.

## 2. Preparar las herramientas y los servicios

En la computadora se utilizaron Git, Node.js, pnpm, el SDK .NET 10 y las herramientas .NET del repositorio.

Para repetir los comandos del frontend se puede usar Node.js 24, como hace su workflow. `package.json` fija pnpm 11.10.0. El backend utiliza `net10.0`. La última comprobación local empleó el SDK 10.0.403.

### Sitio del backend

1. Entramos a MonsterASP.NET y creamos un sitio Free en la región EU.
2. El sitio quedó identificado como `site102105`, con el dominio `dsc-api-pruebas.runasp.net`.
3. Comprobamos que el servidor admitiera .NET 10. En ese despliegue el panel indicaba 256 MB de RAM y proceso x86.
4. Activamos el certificado HTTPS y la redirección a HTTPS desde el panel.
5. Comprobamos el acceso con un certificado válido.

HTTPS pudo activarse directamente en nuestra cuenta. El procedimiento ejecutado quedó resuelto desde el panel.

### Base de datos

1. En MonsterASP abrimos **Databases → Add database**.
2. Seleccionamos el plan Free y PostgreSQL, en la misma región del sitio.
3. La base creada fue `db75981`.
4. Verificamos con SQL el motor real, porque el resumen del asistente mostraba una referencia inconsistente a MySQL.
5. Conservamos el acceso remoto deshabilitado y copiamos la conexión de **Local access**.

```sql
SELECT current_database() AS database_name, version() AS engine;
```

El resultado del despliegue inicial identificó PostgreSQL 18.6. Su host interno fue `db75981.databaseasp.net`, puerto `5432`. La aplicación alojada utiliza ese acceso interno. [Crear PostgreSQL](https://help.monsterasp.net/databases/postgresql/create-database), [conectarse a PostgreSQL](https://help.monsterasp.net/databases/postgresql/connect).

## 3. Configurar autenticación, conexión y CORS

En **MonsterASP → sitio → Scripting → Environment variables** configuramos estas variables.

| Nombre exacto | Valor o contenido |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Jwt__Issuer` | `DSC.Api` |
| `Jwt__Audience` | `DSC.Client` |
| `Jwt__Secret` | Clave aleatoria privada, con al menos 32 bytes UTF-8 |
| `Cors__AllowedOrigins__0` | `https://sistema-dsc-pruebas.pages.dev` |
| `ConnectionStrings__DefaultConnection` | Cadena completa de PostgreSQL obtenida de Local access |

En .NET, el doble guion bajo representa una sección de configuración. Por ejemplo, `Jwt__Secret` se lee como `Jwt:Secret`.

Las variables se guardan para ese sitio y la aplicación las toma al arrancar. Después de modificarlas se reinicia la aplicación desde el panel. [Variables de entorno de MonsterASP](https://help.monsterasp.net/websites/configuration/environment-variables).

La conexión tiene este formato. Los marcadores representan valores que se obtienen del panel.

```text
Server=<HOST_INTERNO>;Port=5432;Database=<BASE>;User Id=<USUARIO>;Password=<CONTRASEÑA>;
```

### Particularidad de TLS en nuestra base

Durante la prueba, `SHOW ssl;` devolvió `off`. La conexión con `SSL Mode=VerifyFull` fallaba.

Con tu autorización se utilizó `SSL Mode=Disable;` exclusivamente en la conexión interna del entorno con datos ficticios. El frontend y la API mantuvieron HTTPS, y el acceso remoto de PostgreSQL permaneció deshabilitado. El tramo API → PostgreSQL quedó sin TLS. Para utilizar datos personales reales queda pendiente disponer de cifrado validado en ese tramo.

### Configuración de GitHub

En **repositorio backend → Settings → Secrets and variables → Actions** se guardaron estos nombres.

| Tipo | Nombre | Uso |
| --- | --- | --- |
| Secret | `JWT_SECRET` | Clave privada de firma |
| Secret | `JWT_ISSUER` | Emisor JWT, `DSC.Api` |
| Secret | `JWT_AUDIENCE` | Audiencia JWT, `DSC.Client` |
| Variable | `FRONTEND_ORIGIN` | Origen público del frontend |

El workflow debe leer estos valores explícitamente. La configuración de MonsterASP se guardó también en su propio panel.

El workflow `validate-deployment.yml` permite comprobar JWT y CORS mediante ejecución manual en la rama predeterminada. El workflow `ci.yml` compila y prueba el proyecto, y produce el paquete de la API.

### Cómo se permitió al frontend llamar a la API

En el backend configuramos la política CORS desde `Program.cs` y `CorsConfiguration.cs`. Se permite el origen exacto de Pages, sin barra final.

```text
https://sistema-dsc-pruebas.pages.dev
```

La política permite los métodos HTTP utilizados por la aplicación y las cabeceras necesarias, incluida `Authorization`. La sesión utiliza un JWT Bearer. Angular envía el token en las solicitudes autenticadas y la API valida su firma, emisor y audiencia.

La clave JWT y la contraseña de PostgreSQL se guardan en el servidor. Angular contiene únicamente la URL pública de la API.

## 4. Crear las tablas de PostgreSQL

La base nueva estaba vacía. Generamos un script a partir de la migración existente `20260826031552_InitialMigration` y lo ejecutamos desde **Run SQL** del panel.

Para repetir la generación desde PowerShell, ejecuta los comandos en la carpeta backend. La configuración local requerida por el proyecto de arranque debe estar disponible, con los nombres de variables indicados en el apartado anterior.

```powershell
Set-Location 'E:/DSC DEV/sistema-dsc-backend'

dotnet --version
dotnet tool restore

$dscSqlFolder = Join-Path $env:USERPROFILE ('DSC-artefactos/schema-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $dscSqlFolder | Out-Null

dotnet ef migrations script --idempotent `
  --project src/DSC.Infrastructure/DSC.Infrastructure.csproj `
  --startup-project src/DSC.Api/DSC.Api.csproj `
  --configuration Release `
  --output (Join-Path $dscSqlFolder 'schema.sql')
```

La generación del SQL se hizo localmente. Para aplicarlo al servidor usamos el panel de la base.

1. Revisamos el script generado.
2. Abrimos **Databases → db75981 → Run SQL**.
3. Pegamos el SQL y lo ejecutamos.
4. El primer intento detectó un carácter BOM inicial. Retiramos ese carácter y repetimos el script.
5. Verificamos las tablas y el historial de migraciones.

```sql
SELECT count(*) AS public_tables
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_type = 'BASE TABLE';

SELECT "MigrationId"
FROM "__EFMigrationsHistory";
```

La primera instalación registró 41 tablas públicas y una migración aplicada.

El script idempotente consulta el historial de migraciones. Las actualizaciones posteriores requieren revisar las migraciones nuevas y respaldar los datos antes de aplicarlas. En `Production`, el código inspeccionado aplica las migraciones mediante este procedimiento manual.

Para permitir el login se cargaron los catálogos mínimos y una sola cuenta ficticia, `dsc.pruebas`, con rol `ADMIN` y contraseña aleatoria almacenada como hash BCrypt. Las 11 cuentas creadas durante las pruebas locales quedaron en la base local. Las contraseñas se entregaron por separado.

## 5. Compilar y subir el backend

### Generar el paquete

Desde PowerShell, ejecuta cada comprobación y continúa cuando termine correctamente.

```powershell
Set-Location 'E:/DSC DEV/sistema-dsc-backend'

git branch --show-current
git status --short

dotnet --version
dotnet tool restore
dotnet csharpier check src tests tools
dotnet restore DSC.slnx
dotnet build DSC.slnx --configuration Release --no-restore
dotnet test DSC.slnx --configuration Release --no-build --logger trx
```

La rama esperada es `back-pablo`. `dotnet --version` debe mostrar un SDK 10 compatible.

Después de aprobar las comprobaciones, genera una publicación completa y comprímela.

```powershell
$dscBackArtifacts = Join-Path $env:USERPROFILE ('DSC-artefactos/backend-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $dscBackArtifacts | Out-Null

dotnet publish src/DSC.Api/DSC.Api.csproj `
  --configuration Release `
  --no-build `
  --output (Join-Path $dscBackArtifacts 'publish')

# Ejecutar después de que publish termine correctamente.
Compress-Archive `
  -Path (Join-Path $dscBackArtifacts 'publish/*') `
  -DestinationPath (Join-Path $dscBackArtifacts 'monsterasp-api.zip')
```

El ZIP debe contener `DSC.Api.dll`, `web.config`, sus dependencias y los demás archivos de publicación directamente en la raíz. El proyecto incluye exclusiones de archivos sensibles. Revisamos el paquete para comprobar que `.env`, certificados privados y perfiles de despliegue quedaran fuera.

### Publicarlo en MonsterASP

1. Abre **Websites → sitio → Files / File manager**.
2. Sube el ZIP a `/private`.
3. En la fila del ZIP, pulsa **Unzip**.
4. Selecciona `/wwwroot` como destino.
5. Activa la sobrescritura de los archivos del paquete.
6. Si actualizas una API que está ejecutándose, utiliza la opción de reinicio del application pool para liberar las DLL bloqueadas.
7. Espera que el trabajo de extracción termine.
8. Comprueba que `DSC.Api.dll` y `web.config` estén directamente en `/wwwroot`.
9. Arranca la aplicación y verifica su funcionamiento.

La opción de reinicio del pool facilita reemplazar archivos en uso. Conserva los archivos propios del hosting, como `.well-known`, y cualquier dato persistente ajeno al paquete. [Publicar un ZIP en MonsterASP](https://help.monsterasp.net/websites/deploy/zip-file).

Durante el incidente de conexión mantuvimos el sitio pausado y el reinicio automático desmarcado hasta completar la corrección y la configuración.

## 6. Compilar y subir el frontend

### Configurar la API pública

El archivo `src/environments/environment.ts` del frontend quedó con esta URL.

```typescript
export const environment = {
  apiBaseUrl: 'https://dsc-api-pruebas.runasp.net',
};
```

El archivo de Development conserva la configuración local. El proxy utilizado con `ng serve` pertenece al desarrollo local. La versión publicada utiliza la URL HTTPS de la API.

### Generar el ZIP

En una terminal con Node.js 24 y pnpm 11.10.0 disponibles, ejecuta cada paso y comprueba que termine correctamente.

```powershell
Set-Location 'E:/DSC DEV/sistema-dsc-frontend'

git branch --show-current
git status --short

pnpm.cmd install --frozen-lockfile
pnpm.cmd test --watch=false
pnpm.cmd build:prod
```

La rama esperada es `front-pablo`. El contenido publicable se genera en `dist/sistema-dsc-frontend/browser`.

```powershell
$dscFrontArtifacts = Join-Path $env:USERPROFILE ('DSC-artefactos/frontend-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $dscFrontArtifacts | Out-Null

Compress-Archive `
  -Path './dist/sistema-dsc-frontend/browser/*' `
  -DestinationPath (Join-Path $dscFrontArtifacts 'cloudflare-frontend.zip')
```

`index.html` debe quedar directamente en la raíz del ZIP, junto con los assets.

### Publicarlo en Cloudflare Pages

1. Entramos a **Workers & Pages**.
2. Creamos el proyecto Pages mediante **Direct Upload**.
3. Elegimos `sistema-dsc-pruebas` como nombre.
4. Subimos el ZIP del frontend compilado y pulsamos **Deploy site**.
5. Al terminar abrimos el dominio y la ruta directa `/auth/login`.

Para una actualización, abre el proyecto existente, elige **Create a new deployment**, selecciona el entorno de producción y sube el nuevo ZIP. Cloudflare permite cargar el ZIP o la carpeta compilada mediante el panel. [Direct Upload de Cloudflare Pages](https://developers.cloudflare.com/pages/get-started/direct-upload/).

## 7. Comprobar la conexión completa

Estas fueron las verificaciones reales de la publicación inicial.

| Comprobación | Resultado obtenido |
| --- | --- |
| Frontend y API por HTTPS | Certificados válidos |
| `GET /api/health` | HTTP 200 y conexión a PostgreSQL confirmada |
| `GET /api/health/db-check` | HTTP 200 |
| `GET /api/v1/auth/me` sin iniciar sesión | HTTP 401 |
| Preflight desde el origen de Pages | HTTP 204 y origen permitido exacto |
| Login con la cuenta ficticia | HTTP 200 y JWT emitido |
| `/me` con ese JWT | HTTP 200 y rol ADMIN |
| Login desde Angular publicado | Inicio de sesión y navegación a la aplicación |

Para repetir las comprobaciones básicas en Windows puedes consultar únicamente los códigos HTTP.

```powershell
curl.exe --silent --show-error --output NUL --write-out "HTTP %{http_code}\n" https://dsc-api-pruebas.runasp.net/api/health
curl.exe --silent --show-error --output NUL --write-out "HTTP %{http_code}\n" https://dsc-api-pruebas.runasp.net/api/v1/auth/me
```

Los resultados esperados son 200 para salud y 401 para `/me` sin token.

Para revisar CORS puedes consultar las cabeceras del preflight.

```powershell
curl.exe --silent --show-error --dump-header - --output NUL `
  --request OPTIONS https://dsc-api-pruebas.runasp.net/api/v1/auth/login `
  --header "Origin: https://sistema-dsc-pruebas.pages.dev" `
  --header "Access-Control-Request-Method: POST" `
  --header "Access-Control-Request-Headers: content-type,authorization"
```

Debe aparecer `Access-Control-Allow-Origin: https://sistema-dsc-pruebas.pages.dev`. Después abre el frontend e inicia sesión con las credenciales de prueba entregadas por separado. Verifica también la función que acabas de modificar.

## 8. Cómo actualizar después de subir cambios a GitHub

El trabajo posterior añadió pipelines a ambos repositorios.

| Acción | Resultado |
| --- | --- |
| `git pull` | Actualiza tu copia local |
| `git push` | Publica tus commits en GitHub y dispara CI |
| CI del frontend | Escanea secretos, ejecuta pruebas, compila y guarda `frontend-browser` |
| CI del backend | Escanea secretos, revisa formato, compila, ejecuta xUnit y guarda `api-publish` |
| Subida a los paneles de hosting | Publica la versión que usarán los visitantes |

### Resultado del pipeline backend

El 13 de noviembre quedó aprobado [Backend CI #3](https://github.com/Distrito-Scout-Cochabamba-Developers/sistema-dsc-backend/actions/runs/43505468116), correspondiente al commit `21a2f32` de `back-pablo`.

- Gitleaks aprobó el escaneo del historial.
- CSharpier aprobó el formato C#.
- La compilación Release terminó correctamente.
- Las 62 pruebas xUnit pasaron localmente y la etapa xUnit también aprobó en GitHub.
- Se generaron los artefactos `xunit-test-results` y `api-publish`.

Para obtener el paquete del backend desde CI, abre **GitHub → Actions → Backend CI → ejecución correcta → Artifacts → api-publish**. Revisa la raíz del ZIP descargado y publica su contenido mediante el procedimiento de MonsterASP.

El frontend genera `frontend-browser` para el mismo propósito. Los workflows configuran una retención de siete días. Si el artefacto caducó, genera una nueva ejecución o compila localmente.

El despliegue del hosting sigue siendo manual. La automatización de esa última subida requiere un workflow de publicación y credenciales del proveedor.

## 9. Incidentes resueltos y límites de lo comprobado

| Situación encontrada | Resolución aplicada |
| --- | --- |
| SDK 9 al compilar un proyecto `net10.0` | Se utilizó el SDK .NET 10 |
| Imports standalone incompletos en el shell Angular | Se corrigieron los imports y el frontend compiló |
| Carácter BOM al pegar el SQL | Se retiró el carácter inicial y se verificó la migración |
| Cadena de conexión incorrecta y diagnóstico sensible | Se protegieron los diagnósticos de salud, se rotó la contraseña y se reemplazó la conexión |
| PostgreSQL interno con TLS deshabilitado | Se aplicó la excepción interna autorizada para datos ficticios |
| Dos alertas Gitleaks sobre JWT truncados de documentación | Se verificaron y excluyeron exclusivamente sus dos huellas históricas |
| Tres pruebas xUnit de asistencia fallidas | Se completaron los datos de prueba con el tipo obligatorio de cada módulo |

Durante la validación inicial del hosting, `/modulos` falló por un registro de inyección de dependencias faltante. También se documentaron endpoints de Kardex y administración pendientes y limitaciones en el manejo global de errores. El pipeline aprobado valida la compilación y las pruebas del código de su commit. La disponibilidad funcional de esas pantallas en Internet requiere su comprobación específica y la publicación de los cambios correspondientes.

Las claves JWT y las contraseñas se conservaron fuera de esta guía y de los paquetes publicados. Esta guía se versiona en `docs/guia-despliegue.md` del repositorio backend.
