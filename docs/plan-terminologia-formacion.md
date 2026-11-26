# Plan de migración del modelo de formación

Este plan no está ejecutado. El proyecto es greenfield y permite regenerar datos mediante semillas; antes de cualquier regeneración debe confirmarse que no se necesitan los datos existentes.

## Cambios futuros

1. **Backend y API:** separar la programación inscribible de sus sesiones; renombrar entidades, servicios, DTO y rutas según el modelo acordado. Adaptar inscripción, cupo, estados, calendario y asistencia; coordinar cualquier cambio de contrato con sus consumidores.
2. **Esquema:** diseñar programación vinculada al módulo y sesiones vinculadas a aquella, con fecha, horario, modalidad y un disertante por sesión. Revisar claves, restricciones, índices, inscripciones y asistencia; generar migraciones o reconstruir el esquema según el entorno.
3. **Frontend:** adaptar modelos, servicios, rutas y formularios al contrato nuevo; gestionar programación y encuentros por separado, incluyendo disertante y modalidad. Actualizar calendario, Secretaría y asistencia.
4. **Semillas:** actualizar catálogos y datos de demostración para generar programaciones y sesiones coherentes; regenerar únicamente después de aprobar la pérdida de datos y verificar dependencias.
5. **Pruebas:** actualizar contratos y fixtures; cubrir múltiples sesiones, un disertante por sesión, inscripciones a programaciones distintas, cupo concurrente, calendario, asistencia y regeneración de datos.

## Vacíos funcionales, no simples renombres

- Hoy `TrainingModuleSession` es la unidad inscribible; sus bloques `training_module_schedule` no equivalen a encuentros independientes con modalidad y disertante propios.
- La inscripción comprende conceptualmente todas las sesiones, pero debe diseñarse y verificarse su aplicación a asistencia y cambios posteriores de programación.
- La asistencia y el QR actual usan el código del módulo curricular; falta definir su asociación segura con cada sesión y su generación oficial.
- Deben definirse las reglas de reprogramación, cancelación, asistencia y acreditación cuando existan múltiples sesiones.

## Inscripción asistida: alcance y decisiones propuestas

La pantalla de inscripción asistida es el alcance visual de este plan. La migración general de dominio y los vacíos anteriores siguen pendientes.

### Dominio y condiciones

- El catálogo contiene módulos; una programación ofrece un módulo en fechas concretas y la inscripción comprende todas sus sesiones. Cada sesión tiene horario, modalidad y un disertante. Identificar qué bloques existentes corresponden a sesiones antes de modificar el esquema.
- Separar la condición actual Rover/dirigente de la cuenta y sus permisos. Al inscribir, el servidor guardará la condición vigente como histórico; no se deducirá de la existencia de credenciales. Una persona existente sin condición verificable deberá completar su ficha, sin duplicarla.
- Garantizar unicidad por persona y programación. Al confirmar, el servidor volverá a validar permisos de Secretaría, estado y cupo, aunque hayan cambiado tras la búsqueda. Adaptar modelos, servicios y consumidores del calendario; mantener sincronizados los glosarios.

### Localizar y seleccionar una programación

- Proponer `/secretaria/inscripciones` como ruta canónica; actualizar navegación y enlaces internos, y redirigir la ruta anterior con los mismos controles de acceso.
- Mostrar próximas programaciones ordenadas por fecha sin exigir texto. Ofrecer nombre opcional, atajos de fecha y rango inclusivo Desde/Hasta en hora de Cochabamba; un solo día usa el mismo inicio y fin. Si cualquier sesión cae en el rango, listar la programación una sola vez. Validar rangos invertidos y ofrecer limpiar filtros sin resultados. Para catálogos grandes, filtrar y paginar antes de enviar resultados.
- Mostrar el nombre del módulo en primera línea y debajo fecha, hora, modalidad y número de sesiones. La selección única debe ser accesible con teclado. Junto al carnet, resumir «Módulo seleccionado», nombre completo, fechas y «Cambiar», sin depender del color ni de un ID. Conservar filtros al volver a elegir e invalidar resultados personales al cambiar de programación.

### Consultar e inscribir

- Buscar el carnet con Enter o Buscar mediante una consulta sin escritura que resuelva persona, condición y estado de inscripción para la programación concreta. Si hay homónimos de carnet, pedir complemento. Un error de red no significa que la persona no exista; descartar respuestas atrasadas al cambiar la búsqueda.
- Si ya está inscrita, mostrar «Ya está inscrita en este módulo programado» sin habilitar la inscripción. Otra programación del mismo módulo puede admitir una nueva inscripción.
- Para una persona registrada y habilitada, mostrar nombre y condición de solo lectura con una sola acción «Inscribir». Eliminar la casilla «Inscribir como Rover sin cuenta»; una persona conocida sin cuenta también usa su condición registrada.
- Solo tras confirmar la ausencia, ofrecer «Registrar Rover» o «Registrar dirigente», abrir un único formulario y conservar el carnet. Confirmar alta e inscripción juntas.
- Tras confirmar, mostrar en el mismo lugar «Inscripción confirmada», conservando programación, filtros y posición. «Inscribir a otra persona» limpia solo los datos personales y enfoca el carnet. Bloquear doble envío; ante resultado incierto, consultar el estado antes de reintentar. Si otro operador ya inscribió, mostrar el estado real.

### Ejecución y verificación pendientes

1. Acordar el contrato de consulta y preparar esquema, restricciones y semillas regenerables para desarrollo. Incluir dos programaciones del mismo módulo, sesiones en distintas fechas, personas con y sin cuenta y una inscripción previa. Regenerar solo el entorno previsto, previa confirmación de que no se necesitan sus datos y aprobación de su pérdida.
2. Implementar consulta y confirmación; después ruta, filtros, selección y resultado con componentes compartidos. Mantener la pantalla montada. Comprobar en navegador si la sensación de recarga procede de navegación, desmontaje del resultado, pérdida de foco o salto de scroll.
3. Probar con teclado, en móvil y escritorio: encontrar un módulo sin saber su nombre, reconocer la selección, detectar duplicado al buscar, inscribir a una persona conocida en un clic y registrar dos personas consecutivas sin volver a seleccionar módulo. Verificar ausencia de navegación del documento e inscripciones duplicadas concurrentes.
4. Continuar la migración general: adaptar asistencia y QR a la sesión correcta; decidir reprogramación, cancelación y acreditación de módulos multisesión antes de implementar esas reglas. Validar fixtures, calendario y regeneración de la base de desarrollo.
