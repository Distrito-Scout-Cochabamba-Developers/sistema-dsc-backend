---
title: 'PRD - Sistema de Gestión de Formación, Kardex y Administración Scout (DSC Backend)'
status: 'draft'
created: '2026-07-28'
updated: '2026-07-28'
author: 'DSC Software Team & BMad PM (John)'
version: '1.1.0'
---

# 📋 Product Requirement Document (PRD)
## Sistema de Gestión Integral y Kardex - Distrito Scout Cochabamba (DSC)

---

## 1. Visión General del Producto

El **Sistema de Gestión Integral y Kardex Scout** para el Distrito Scout Cochabamba (DSC) es una plataforma digital centralizada diseñada para automatizar la formación de adultos, la gestión de asistencia, la evaluación docente cruzada, el control de propuestas de sesión, el inventario, las reservas de la sede y el Kardex académico/administrativo de todos los dirigentes del distrito.

El sistema garantiza una experiencia móvil ágil en eventos de formación (mediante códigos QR y autocompletado inteligente), elimina la carga manual y transcripción errónea de datos, y asegura la **trazabilidad, validez de certificados y la privacidad de la información** según la taxonomía estricta de roles del distrito.

---

## 2. Taxonomía Completa de Usuarios y Roles de Sistema

| Sigla | Rol de Usuario | Descripción y Responsabilidades Principales |
| :---: | :--- | :--- |
| **DD** | **Director Distrital** | Coordina y supervisa todas las carteras (ej. Pablo). Asigna tareas, aprueba egresos mayores (> 500 Bs), firma circulares y evalúa informes distritales. |
| **RCD** | **Responsable de Comunicaciones** | Planifica publicaciones, recolecta/selecciona fotos de eventos y mantiene la línea gráfica institucional. |
| **SD** | **Secretaría del Distrito** | Registra asistencia a módulos, gestiona reservas de sede, controla correspondencia entrante/saliente, numera circulares y archiva documentos. |
| **CF** | **Comisionada de Finanzas** | Controla la caja mayor y caja chica del distrito, registra ingresos/egresos y elabora informes económicos. |
| **CPJ** | **Coordinador de Programa de Jóvenes** | Supervisa el programa educativo, vigila la aplicación del Ciclo de Programa, registra permisos de salidas y valida documentos de reconocimientos especiales. |
| **CAE** | **Coordinador de Adultos en el Escultismo** | Planifica la malla curricular de adultos, supervisa avance de capacitación, asigna evaluadores de cuadernillos equitativamente, aprueba diseños de sesiones y busca disertantes cualificados (Responsable de Formación). |
| **EF** | **Evaluador de Formación** | Dirigente experimentado asignado para revisar cuadernillos y ciclos utilizando el checklist del sistema y emitir dictámenes. |
| **DIS** | **Disertante (Expositor)** | Carga su plan/diseño de sesión para revisión, dicta módulos y es evaluado por cursantes y dirección del evento. |
| **CUR** | **Cursante (Dirigente en Capacitación)** | Asiste a módulos de formación distritales, registra asistencia por QR y evalúa al disertante al concluir. |
| **ID** | **Intendente del Distrito** | Encargado del resguardo, orden, despacho y recepción del inventario y activos fijos del distrito. |
| **RO** | **Responsable de Operaciones** | Mantenimiento físico de la casa distrital y adquisición de materiales de infraestructura. |
| **CR** | **Comisionado de Rama** | Programa y organiza actividades de una rama específica a nivel distrital (Lobatos, Scouts, Scouters, Rovers). |
| **DS** | **Dirigente Scout** | Dirigente de grupo que asiste a módulos, solicita certificados, reserva espacios de la sede, solicita préstamo de materiales y gestiona sus cuadernillos/ciclos para aprobación de nivel. |
| **RES** | **Residente del DSC** | Encargado de la logística en sitio, limpieza y preparación de la sede para las actividades reservadas (ej. Jorge Revollo). |
| **PUB** | **Visitante General / Público / Padres** | Personas que acceden a la web pública para informarse, ver el organigrama, buscar puntos de reunión de grupos y verificar certificados por CI. |

---

## 3. Matriz de Control de Acceso y Privacidad (RBAC)

| Módulo / Funcionalidad | PUB | DS / CUR | DIS | EF | CAE / SD / DD | CF / ID / RO / RES |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Consulta Pública por CI & Verificación QR Certificados** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Inscripción y Asistencia por QR en Sala** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Ver Kardex Propio & Exportar Ficha PDF** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Ver Kardex General & Perfiles Ajenos** | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ |
| **Subir Diseño de Sesión** | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| **Aprobar Diseño de Sesión & Invitar Disertante** | ❌ | ❌ | ❌ | ❌ | ✅ (CAE) | ❌ |
| **Registro Manual y Corrección de Asistencia** | ❌ | ❌ | ❌ | ❌ | ✅ (SD) | ❌ |
| **Evaluación Cruzada a Disertantes** | ❌ | ✅ (Cursante) | ❌ | ❌ | ✅ (Dirección) | ❌ |
| **Asignar Evaluador & Revisar Cuadernillos** | ❌ | ❌ | ❌ | ✅ (EF) | ✅ (CAE) | ❌ |
| **Reservas de Sede & Préstamo de Materiales** | ❌ | ✅ (Solicita) | ✅ | ✅ | ✅ (SD Aprueba) | ✅ (ID/RES Prepara) |
| **Aprobación de Egresos Mayores (> 500 Bs)** | ❌ | ❌ | ❌ | ❌ | ✅ (DD / CF) | ❌ |

---

## 4. Requisitos Funcionales por Módulos (FRs)

### Módulo A: Registro Digital de Asistencia e Inscripción a Módulos (`FR-REG`)

*   **`FR-REG-01` Acceso por QR/Enlace Único**: Generación de QR y enlace único por sesión de módulo. Acceso mobile-first sin instalación de aplicaciones.
*   **`FR-REG-02` Formulario con Validación Estricta de CI**: Campos obligatorios: Nombre Completo, Carnet de Identidad (CI - *validación exclusivamente numérica*), Extensión (departamento) y Teléfono de contacto.
*   **`FR-REG-03` Autocompletado por CI**: Si el CI ingresado ya existe en la base de datos de adultos del distrito, autocompletar automáticamente Nombre y Apellidos.
*   **`FR-REG-04` Auto-detección de Sesión Autenticada**: Si el dirigente (`DS`/`CUR`) está autenticado en la plataforma, recuperar y pre-llenar automáticamente sus datos de perfil.
*   **`FR-REG-05` Registro Parcial**: Permite la inscripción de dirigentes aún no registrados en el sistema bajo estado "Registro Parcial" para su posterior regularización por Secretaría (`SD`).
*   **`FR-REG-06` Resumen y Confirmación**: Pantalla de éxito inmediata con el resumen del módulo registrado.

---

### Módulo B: Secretaría y Gestión Administrativa de Asistencia (`FR-SEC`)

*   **`FR-SEC-01` Panel de Gestión de Módulos**: Secretaría (`SD`) busca, filtra y selecciona cualquier evento de módulo específico.
*   **`FR-SEC-02` Registro Manual en Sitio**: Permite a `SD` añadir participantes por CI creando el perfil básico (CI, Nombre, Celular) si el usuario no existe.
*   **`FR-SEC-03` Edición Extemporánea**: Permite a `SD` editar registros de asistencia pasados para corregir errores de digitación en CIs o nombres.

---

### Módulo C: Consulta Pública, Verificación y Certificación Digital (`FR-CER`)

*   **`FR-CER-01` Buscador Público por CI**: Barra de consulta abierta para todo público (`PUB`) mediante CI.
*   **`FR-CER-02` Listado Histórico**: Muestra Nombre del Módulo, Fecha en que se cursó, Nombre del Disertante y enlace de Descarga de Certificado PDF.
*   **`FR-CER-03` Trazabilidad y Verificación QR**: Todo certificado incluye un código único (Hash/UUID) y un QR imprimible verificable públicamente en el sistema.
*   **`FR-CER-04` Exportación de Ficha PDF**:
    *   `SD` y `CAE`: Exportan e imprimen la ficha histórica de cualquier dirigente en PDF.
    *   `DS` / `CUR`: Exportan su propia ficha histórica a PDF desde su cuenta personal.

---

### Módulo D: Diseños de Sesión por Disertantes (`FR-SES`)

*   **`FR-SES-01` Formulario de Diseño de Sesión**: Disertantes (`DIS`) completan la propuesta pedagógica (objetivos, metodología, tiempos, recursos).
*   **`FR-SES-02` Estados del Diseño**: `Borrador`, `Pendiente de Revisión`, `Aprobado`, `Con Observaciones`.
*   **`FR-SES-03` Aprobación por CAE/RF**: Notificación automática al Coordinador de Adultos en el Escultismo (`CAE`) para aprobar, observar o solicitar modificaciones con comentarios.

---

### Módulo E: Evaluaciones Cruzadas del Disertante (`FR-EVA`)

*   **`FR-EVA-01` Evaluación por QR en Sala**: Al finalizar el módulo, los cursantes (`CUR`) acceden a un formulario anónimo via QR.
*   **`FR-EVA-02` Criterios Cursante**: Escala de 1 a 5 (claridad, material, dominio, empatía) + campo libre de retroalimentación.
*   **`FR-EVA-03` Reporte de Dirección**: La dirección del evento (`CAE` / `CR`) ingresa un reporte técnico docente por separado.
*   **`FR-EVA-04` Consolidación Anónima**: Promedio histórico automático asignado al perfil Kardex del disertante, resguardando la anonimidad individual.

---

### Módulo F: Kardex Digital y Privacidad Estricta (`FR-KAR`)

*   **`FR-KAR-01` Directorio Administrativo**: Filtros por Grupo Scout, Rama (Lobatos, Scouts, Scouters, Rovers) y Nivel de Formación.
*   **`FR-KAR-02` Perfil Detallado**: Despliega datos personales (CI, nombre, cargo), bitácora cronológica de módulos y eventos distritales.
*   **`FR-KAR-03` Privacidad Estricta**: La lista general de Kardex y perfiles ajenos está **estrictamente restringida** a `CAE`, `SD` y `DD`. Un dirigente regular (`DS`) **solo puede ver su propia ficha**.

---

### Módulo G: Selección y Convocatoria de Disertantes (`FR-DIS`)

*   **`FR-DIS-01` Búsqueda Cruzada de Disertantes**: Permite a `CAE` filtrar por *"Haber cursado Módulo X"* Y *"Haber dictado Módulo Y"*.
*   **`FR-DIS-02` Promedio Docente**: Despliega la calificación docente promedio del dirigente candidato.
*   **`FR-DIS-03` Invitación Automática**: Botón *"Invitar como disertante"* con pre-relleno de datos del módulo programado.

---

## 5. Requisitos No Funcionales (NFRs)

*   **`NFR-SEC-01` Autenticación y RBAC**: JWT Bearer tokens con reclamos de roles estrictos (`DD`, `SD`, `CAE`, `CF`, `DS`, etc.).
*   **`NFR-PER-01` Rendimiento Mobile**: Tiempos de respuesta < 200ms en autocompletado de CIs y soporte de 200+ asistencias/evaluaciones concurrentes en sala.
*   **`NFR-DOC-01` Trazabilidad PDF**: Generación sub-segundo de certificados PDF con códigos de verificación criptográficos y QR.

---

## 6. Métrica de Éxito & Contra-Métricas

*   **Reducción de errores en CIs**: **0% errores de transcripción** gracias a validaciones numéricas y autocompletado.
*   **Tiempo de Inscripción**: **< 30 segundos** por dirigente mediante QR.
*   **Contra-Métrica (Privacidad)**: **0 filtraciones de datos privados**: La consulta pública NUNCA expone información personal, teléfonos o Kardex completo fuera del certificado verificado.
