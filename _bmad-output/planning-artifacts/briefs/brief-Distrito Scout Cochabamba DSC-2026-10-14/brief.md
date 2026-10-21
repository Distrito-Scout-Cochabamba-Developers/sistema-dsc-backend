---
title: "Product Brief — Sistema de Gestión del Distrito Scout Cochabamba"
status: "approved"
created: "2026-10-14"
updated: "2026-10-15"
author: "Winsor Orellana con BMad"
---

# Product Brief: Sistema de Gestión del Distrito Scout Cochabamba

## Resumen ejecutivo

El Sistema de Gestión del Distrito Scout Cochabamba es una plataforma web a medida para reducir el trabajo administrativo que recae sobre dirigentes voluntarios y hacer más simples, claros y trazables los procesos institucionales del distrito. Su norte es que **los dirigentes puedan concentrarse en el Programa de Jóvenes**, en lugar de perseguir registros, documentos, aprobaciones, materiales y tareas por planillas, papel y mensajes dispersos.

La visión reúne varios ámbitos —formación y trayectoria de dirigentes, gestión de programa, administración, inventario, comunicaciones e información pública—. Para el primer release, el usuario ha definido como alcance inicial dos capacidades obligatorias para diciembre: gestión de módulos de formación (inscripción y consulta de módulos cursados) y seguimiento digital de cuadernillos (redacción digital y asignación de revisores), con un catálogo de módulos compartido. La redacción de cuadernillos se guiará por plantillas y secciones que varían según nivel; el release cubre únicamente Insignia de Madera (IM) nivel II y III. Diciembre es la fecha objetivo declarada; el año, la definición de “terminado”, los criterios de aceptación y la viabilidad todavía deben precisarse.

## El problema

Las entrevistas describen información distribuida entre hojas manuscritas, archivos Excel, carpetas físicas o digitales, calendarios y conversaciones de WhatsApp. Esto crea retrabajo, errores de transcripción, pérdida de contexto y dependencia de que personas concretas recuerden o respondan a tiempo.

- En módulos de formación, la escritura ilegible o incorrecta de CI hace difícil acreditar asistencia y obliga a buscar registros antiguos; dirigentes consultan repetidamente qué módulos completaron.
- Reservas de sede y préstamos de materiales pueden coordinarse por canales separados, con riesgo de cruces, avisos omitidos, disponibilidad incierta y poca trazabilidad de estado o daños.
- Responsables de cartera y dirección dedican esfuerzo a reunir informes y recordar vencimientos; los formatos varían y el seguimiento queda en conversaciones y actas.
- Comunicaciones tiene dificultades para recolectar y ordenar fotos; el diseño, la selección final y la publicación siguen necesitando criterio humano.
- Procesos de programa, como permisos de salida, ciclo de programa y evaluación de cuadernillos, requieren mejor organización y seguimiento; faltan entrevistas completas para confirmar sus flujos.

## La solución

Una plataforma web del distrito que centraliza los flujos administrativos priorizados, hace visibles sus estados y conserva una fuente fiable de información para quienes estén autorizados. Para el primer release, el cursante podrá inscribirse y consultar sus módulos; el dirigente en formación podrá escribir y guardar el contenido de su cuadernillo dentro de la plataforma mediante la plantilla correspondiente a IM nivel II o III; los responsables podrán asignar revisores y dar seguimiento a la revisión. Ambos flujos se apoyarán en un catálogo común de módulos. Las plantillas son distintas: IM II contempla un ciclo de programa y sus entrevistas, planificación/evaluación; IM III contempla dos ciclos de programa, con el primer ciclo evaluado y el segundo planificado en el primer envío. La interacción de edición y revisión sigue por diseñarse.

Se diseña para los roles reales del distrito y para uso web accesible desde móvil cuando el flujo lo requiera. La plataforma complementará o integrará servicios existentes de la Asociación de Scouts de Bolivia (ASB) cuando corresponda, en vez de duplicar automáticamente funciones nacionales. El conjunto de módulos y el primer flujo de extremo a extremo aún están por decidir.

## A quién sirve

Usuarios internos incluyen dirección distrital, secretaría, finanzas, responsables de comunicaciones, coordinaciones de programa y de adultos, evaluadores, disertantes, cursantes, intendencia, operaciones, comisionados de rama, dirigentes y residentes encargados de la sede. Visitantes y familias necesitan información pública clara, como datos institucionales, organigrama y puntos de reunión. Los permisos y datos visibles por rol deben definirse con especial cuidado.

## Diferenciación y principios

- Hecho a la medida de los procesos del Distrito Scout Cochabamba y desarrollado con participación de sus responsables.
- Código abierto y posibilidad de reutilización por otros distritos, sujeto a validar privacidad, seguridad y datos que nunca deben publicarse.
- Mantenibilidad y continuidad como prioridades: código legible, probado y documentado para que futuros equipos puedan sostenerlo.
- IA como herramienta de apoyo al equipo, manteniendo comprensión y responsabilidad humana sobre los cambios.
- Experiencia simple para voluntarios y grupos, evitando añadir burocracia digital a la existente.

## Éxito

El release será útil cuando usuarios destinatarios completen en producción un flujo acordado sin depender de transcripciones paralelas y cuando responsables del distrito puedan verificar el estado y resultado con datos confiables. Las métricas, los criterios de aceptación, los usuarios piloto, el presupuesto y la ventana de entrega están por definir. Mantenibilidad, seguridad, privacidad y operación real son condiciones de calidad, no sustitutos de la validación con usuarios.

## Alcance inicial y límites

**Alcance inicial confirmado por el usuario para planificar el release de diciembre:**

1. **Módulos de formación:** el dirigente cursante puede inscribirse a módulos y consultar los módulos que ha cursado.
2. **Cuadernillos de formación:** el dirigente redacta digitalmente su cuadernillo; se asignan revisores y se da seguimiento a la evaluación.
3. **Catálogo común de módulos:** fuente compartida para estos dos flujos.

4. **Plantillas de cuadernillo:** formulario/estructura de secciones adaptado a IM nivel II y a IM nivel III, manteniendo sus diferencias de contenido y cantidad de ciclos de programa. Los cuadernillos de Gestión Institucional (Paxtú) nivel Grupo y Distrito compartidos como referencia quedan fuera de este release y se contemplan para releases futuros.

Los demás módulos pueden incluirse si hay capacidad, pero no forman parte del compromiso inicial. Para cuadernillos, el alcance inicial se limita a plantillas IM II y III; otros niveles, incluidos Paxtú Grupo y Distrito, quedan para futuros releases. El procedimiento recibido describe revisiones por la Comisión de Formación y tres envíos consolidados en PDF; se conservará como evidencia del proceso vigente a validar, no como definición de que el producto solo permita cargar PDFs. Debe definirse cómo la autoría estructurada dentro del sistema se relaciona con los envíos, subsanaciones y expediente final PDF. No se han acordado aún criterios verificables de terminado, los límites detallados del catálogo, roles y permisos, el alcance de evaluación/correcciones, ni si el hito significa demo, piloto o producción. La integración con ASB debe aclararse para no duplicar su plataforma de registros.

No se consideran decisiones confirmadas construir un nuevo sistema de registros nacionales, automatizar edición/publicación de contenido o desplegar todos los módulos identificados. La fecha objetivo declarada es diciembre, pero su año y el nivel de entrega requerido todavía requieren confirmación.

## Visión

Si funciona, el sistema se convierte en una plataforma institucional confiable y mantenible que disminuye fricción administrativa en Cochabamba, facilita la continuidad entre gestiones y permite que dirigentes, grupos y familias dediquen más atención a la experiencia educativa scout.

## Estado de las decisiones

- **Acordado:** producto a medida para el Distrito Scout Cochabamba; reducir carga administrativa; facilitar uso real; código abierto es una intención expresada. Alcance inicial previsto para diciembre: módulos de formación (inscripción y consulta), cuadernillos digitales con asignación y seguimiento de revisores, y catálogo común de módulos. El cuadernillo se compone de secciones según nivel y el primer release cubre IM II y III; otros niveles quedan para releases futuros.
- **Evidencia de entrevistas:** módulos/manualidad de registros, dificultad de inventario y reservas, seguimiento de informes/tareas, recolección de imágenes y necesidades de programa.
- **Por confirmar:** año y significado de la entrega de diciembre; criterios de terminado; edición, guardado, revisión y generación del expediente final de cuadernillo; vigencia y adaptación del procedimiento de tres envíos PDF al sistema; límites del catálogo y el historial; integración con registros de ASB; roles/permisos; presupuesto/hosting, operación y aceptación; arquitectura/repositorio fuente de requisitos transversales.
- **No constituye compromiso:** las fases y prioridades históricas expresadas en entrevistas ni los estados actuales de Azure Boards.

## Fuente relacionada

El PRD del backend en `../../prd.md` fue conciliado y aprobado con base en este brief, el procedimiento oficial de cuadernillos y las decisiones registradas en `.memlog.md`. El índice local de coordinación está en el workspace padre y no es fuente versionada del equipo.
