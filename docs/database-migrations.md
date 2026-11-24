# Inicialización y migraciones

Un solo `ScoutDbContext` y un solo `__EFMigrationsHistory`. No se reemplaza la migración inicial histórica ni se modifica el arranque de la aplicación.

## Cupo de convocatoria (AB#155)

`20261124060541_AddTrainingModuleSessionCapacity` añade `training_module_session.capacity`, entero nullable no negativo. Conserva las inscripciones y congela el cupo efectivo anterior: usa la primera sede en orden de parte, fecha y hora (ID de bloque para desempatar). Si esa sede tiene cupo `NULL`, no busca otra; si no hay sede o bloques, queda `NULL`. Un cupo histórico negativo ya implicaba cero plazas y se normaliza a `0`. Si hay más inscritos que plazas, se conservan todos y quedan cero plazas libres. No se vuelve a copiar el valor al cambiar la sede.

**Producción (Run SQL):** respaldar la base y detener las escrituras de convocatorias, sedes e inscripciones. Generar y revisar desde la raíz el script completo idempotente, que cubre tanto una base vacía como una base con historial EF desde la migración inicial. Ejecutarlo antes de publicar la nueva API, sin dejar escritores de la versión anterior activos tras el backfill:

```powershell
dotnet ef migrations script --idempotent --project src/DSC.Infrastructure --startup-project src/DSC.Infrastructure --context ScoutDbContext --output .\schema.sql
```

En la ruta incremental local, `dotnet ef database update` aplica la misma migración; no hay un SQL de cupo independiente. Verificar el historial y una muestra de cupos comparados con los bloques originales. No importar un baseline en una base existente. Los scripts de baseline limpio descritos abajo no están versionados en esta rama: para base nueva usar la cadena completa de migraciones, no marcar manualmente AB#155 como aplicada.

El rollback elimina los cupos propios, incluidas las ampliaciones posteriores: respaldarlos antes de revertir. Para #156, leer `TrainingModuleSession.Capacity` en la operación común de inscripción y coordinarla con las escrituras de esa misma fila. La disponibilidad publicada por el calendario no garantiza el último cupo.

## Base nueva y vacía: SQL limpio

Requiere PowerShell 5.1+, SDK, herramientas locales y paquetes ya instalados/restaurados. Desde la raíz del repositorio:

```powershell
.\tools\New-DatabaseBaseline.ps1 -OutputPath .\baseline.sql
```

El directorio de salida debe existir. Una salida existente se rechaza; `-Force` permite reemplazarla atómicamente, únicamente después de generar todo correctamente. Cualquier fallo de compilación, EF o escritura deja la salida anterior intacta. SQL UTF-8 sin BOM; el script devuelve la ruta absoluta del archivo generado.

El generador **no conecta ni aplica SQL**: compila Infrastructure con `--no-restore`, usa la fábrica de diseño de Infrastructure (sin arrancar la API ni cargar `.env`), comprueba `migrations has-pending-model-changes`, enumera `migrations list --no-connect --json` y genera `dbcontext script`. EF usa `--project src/DSC.Infrastructure --startup-project src/DSC.Infrastructure --context ScoutDbContext --no-build`. Si modelo y snapshot divergen, se detiene sin publicar SQL. No instala dependencias ni maneja credenciales.

El SQL crea directamente el modelo actual (`training_line`, `training_level`, `training_module_line`), no reproduce `module_type` ni la cadena de ALTER históricos. Tras crear todo el esquema, crea el historial y registra **todos** los IDs enumerados; `ProductVersion` procede del snapshot, no de la versión de la herramienta `dotnet-ef`.

**Límite revisado:** la última migración debe ser `20261111050951_IntroduceTrainingLines`; cualquier migración posterior bloquea la generación antes de publicar SQL. `dbcontext script` no reproduce operaciones fuera del modelo, como `migrationBuilder.Sql`, `InsertData`, triggers o transformaciones de datos. Solo actualizar este límite tras revisar deliberadamente esas operaciones y su representación en la inicialización limpia. La cadena actual está revisada para una base vacía: el enum inicial está incluido en el modelo y la transferencia incremental no tiene filas que transferir. Un baseline ya importado admite futuras migraciones por la ruta incremental habitual, sin cambiar este límite.

1. Preparar una base PostgreSQL **dedicada, vacía**, sin aplicación ni otros escritores concurrentes. No usar esta ruta en una base existente, aunque no tenga historial. Solo se admite el esquema `public` vacío, además de los objetos del sistema.
2. Revisar el SQL generado. Importarlo **antes del primer arranque de la aplicación**, mediante una conexión elegida y autenticada fuera del generador:

   ```powershell
   psql -X -v ON_ERROR_STOP=1 --file .\baseline.sql
   ```

   Seleccionar previamente la base correcta mediante la configuración normal de `psql`. No guardar credenciales en comandos ni archivos versionados. `-X` evita personalizaciones de `psql`; `ON_ERROR_STOP=1` es obligatorio.
3. Solo tras una importación correcta, iniciar la aplicación o usar el comando incremental siguiente para futuras migraciones. Las migraciones registradas no vuelven a ejecutarse.

La importación es una transacción: fija `search_path` a `public, pg_catalog`, rechaza esquemas de usuario distintos de `public` y objetos no sistémicos (incluye tablas, vistas, secuencias y enums). Una base con solo un enum o un esquema de usuario vacío también se rechaza. Un error aborta la transacción; no queda esquema ni historial parcialmente aplicado. El generador no borra objetos ni convierte una base existente.

El arranque conserva su lógica heredada de marcado de historial y `Migrate`; **no sustituye esta inicialización limpia**. No arrancar primero para luego importar el baseline.

## Base existente: ruta incremental

Conservar el historial y los datos; no importar el baseline ni borrar/recrear la base. Respaldar y revisar el SQL de migración antes de aplicar. Antes de `IntroduceTrainingLines`, clasificar los niveles con `training_level.module_type_id IS NULL`; no se asigna una línea arbitraria y esos datos hacen fallar la migración.

```powershell
dotnet ef database update --project src/DSC.Infrastructure --startup-project src/DSC.Infrastructure --context ScoutDbContext --connection "<conexion-seleccionada-fuera-del-repo>"
```

El marcador no es una conexión real: proporcionar la conexión del entorno por un mecanismo seguro, sin versionar secretos. La fábrica de diseño no toma la conexión de producción automáticamente.

`IntroduceTrainingLines` conserva filas, renombra `module_type` y transfiere las pertenencias a la tabla de unión dentro de la transacción de EF. El rollback exige exactamente una línea por módulo: con cero o varias, falla sin perder pertenencias. No forzar la reversión descartando datos. Las migraciones futuras usan este mismo historial, tanto tras el baseline como tras la ruta incremental.

## Verificación independiente

Desde la raíz del repositorio, con los binarios nativos de PostgreSQL ya instalados y las herramientas/paquetes .NET restaurados:

```powershell
.\tests\Test-DatabaseMigrationPaths.ps1
```

Si los binarios están en otra ubicación, indicar opcionalmente `-PgBin`:

```powershell
.\tests\Test-DatabaseMigrationPaths.ps1 -PgBin "C:\Program Files\PostgreSQL\17\bin"
```

La comprobación crea exclusivamente un clúster PostgreSQL nativo, aislado y efímero. No acepta una cadena de conexión ni utiliza la base configurada de la aplicación; no instala PostgreSQL. Verifica las rutas de migración independientemente de la suite xUnit. Los fallos de tickets de esa suite se siguen por separado; no son resultados de esta comprobación de migraciones.
