# Requisitos de nivel (HU 1.3)

Solo criterio `MODULES`, estándar por nivel, sin fechas. No evalúa historiales ni consume aprobaciones. No hay cuota fija de voluntarios ni motor genérico de requisitos.

Caso confirmado: **Insignia de Madera I-II (IM I-II)** es un nivel único de **Programa de Jóvenes (PJ)**. Exige **12 módulos totales**, con **mínimo 2** de estos **4 elegibles obligatorios**: Responsabilidad legal del dirigente, Impacto ambiental en actividades Scouts, Primeros auxilios y Ciclo de programa. Por tanto, 2+10, 3+9 y 4+8 obligatorios/voluntarios satisfacen las cantidades; 1+11 no. Estas combinaciones describen el dominio; HU 1.3 no implementa la evaluación de aprobaciones.

La [grilla aportada por el usuario](training-modules-reference.md) conserva los nombres de referencia. Los códigos descriptivos siguientes y los de las fixtures **no son oficiales**, ni semillas de producción.

## HTTP

Todas las rutas requieren JWT. PUT exige `ADMIN` o `DISTRICT_SECRETARY`, como el catálogo existente. No se introduce un rol CAE.

- `GET /api/v1/training-levels`: niveles ordenados por nombre; objetos con `code`, `name`, `trainingLineCode`, `isActive`.
- `GET /api/v1/training-levels/{code}/requirements`: `200` con `{ "trainingLevelCode": "IM-I-II", "modules": null }` si existe sin configuración; `404` si no existe.
- `PUT /api/v1/admin/training-levels/{code}/requirements`: reemplaza exclusivamente el criterio de módulos y su selección; crea el criterio si no existe. Repetir la petición conserva el resultado. Devuelve `200` con el mismo formato de GET.

Cuerpo PUT (valores ilustrativos, no semillas):

```json
{
  "totalModulesCount": 12,
  "minMandatoryModulesCount": 2,
  "mandatoryModuleCodes": ["RESPONSABILIDAD-LEGAL", "IMPACTO-AMBIENTAL", "PRIMEROS-AUXILIOS", "CICLO-PROGRAMA"]
}
```

Respuesta configurada:

```json
{
  "trainingLevelCode": "IM-I-II",
  "modules": {
    "totalModulesCount": 12,
    "minMandatoryModulesCount": 2,
    "mandatoryModuleCodes": ["CICLO-PROGRAMA", "IMPACTO-AMBIENTAL", "PRIMEROS-AUXILIOS", "RESPONSABILIDAD-LEGAL"]
  }
}
```

Los tres campos son obligatorios y no nulos. Cantidades enteras no negativas; mínimo no mayor que el total ni que la cantidad real de módulos seleccionados. Se permite total/mínimo cero y lista vacía. La lista puede superar el total: es un conjunto elegible, no un cupo que deba cursarse completo. Códigos de 1 a 30 caracteres tras recortar espacios, sensibles a mayúsculas como el catálogo; duplicados rechazados tras normalizar; respuesta ordenada por código.

Errores `ProblemDetails`: `400` cuerpo ausente/incompleto, cantidades inválidas, códigos vacíos/duplicados/inexistentes; `404` nivel inexistente; `401` sin JWT; `403` sin rol para PUT; `409` conflicto concurrente de persistencia (releer antes de reintentar).

Selección sobre el catálogo existente, sin restringir por línea. `TrainingModule` no tiene estado activo/inactivo: no se afirma filtrar módulos activos; añadir ese estado pertenece a una evolución del catálogo. Un módulo referenciado no puede eliminarse (`409`), incluso sin sesiones ni asistencia.

## Persistencia y comprobación

Unicidad por nivel/categoría; clave compuesta en la selección; FK restrictiva hacia módulos; cantidades protegidas también por CHECK SQL. La cardinalidad mínima se valida en el agregado sobre módulos reales. Un token interno de concurrencia cambia incluso si solo se cambia la selección: guardados simultáneos sobre una misma versión devuelven `409` en vez de mezclar selecciones y cantidades. No es un ETag: no detecta ediciones de clientes basadas en lecturas antiguas si sus escrituras no se solapan. Una actualización no borra otros criterios ni modifica asistencia, evaluaciones o niveles obtenidos.

La migración debe generarse con Infrastructure como startup de diseño; revisar SQL sin arrancar la API ni aplicar cambios a bases configuradas. `tools/New-DatabaseBaseline.ps1` y `tests/Test-DatabaseMigrationPaths.ps1`, mencionados en la documentación anterior, no están presentes en este árbol: no se recrean ni se relaja una protección inexistente.

`TrainingStage` se elimina mediante `20261113061752_RemoveTrainingStages`, conservando migraciones históricas. Eliminará el catálogo de etapas y sus asignaciones, no los niveles, requisitos ni historiales. `Down` recupera la estructura, **no los datos eliminados**. No renombra ni fusiona niveles existentes. No aplicar a una base compartida sin autorización y respaldo; el arranque de la API en Development puede aplicar migraciones automáticamente.

Pruebas focalizadas: `dotnet test tests/DSC.UnitTests -c Release --filter FullyQualifiedName~TrainingLevelRequirement`. Son pruebas C# con xUnit, sin lanzador PowerShell. Requieren PostgreSQL 17 local (`C:\Program Files\PostgreSQL\17\bin` por defecto; variable opcional `DSC_TEST_PG_BIN` para otra ubicación); usan una instancia efímera en loopback con `trust` exclusivamente local, una base por caso y migraciones reales, sin EF InMemory ni dependencias nuevas. La fixture C# inicia y detiene PostgreSQL; los artefactos quedan en `%LOCALAPPDATA%/Temp/opencode/dsc-requirements-*`. Las pruebas HTTP usan JWT y el controlador real, pero un host de pruebas: no verifican `Program` ni garantizan un conflicto HTTP concurrente reproducible. La concurrencia se comprueba con dos repositorios/contextos PostgreSQL.

Suite completa: `dotnet test tests/DSC.UnitTests/DSC.UnitTests.csproj -c Release`. Los tres fallos previos del contrato general del catálogo y EF Core 10 se mantienen fuera del alcance.
