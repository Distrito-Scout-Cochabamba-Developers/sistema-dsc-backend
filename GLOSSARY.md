# Formación DSC

Vocabulario del esquema de formación de dirigentes.

## Language

**TrainingLine (línea de formación)**:
Área de formación que agrupa niveles y comparte módulos con otras líneas. Existen tres líneas oficiales:
- **PJ (Programa de Jóvenes)**: Orientada a capacitar a los Dirigentes de Unidad.
- **GI (Gestión Institucional)**: Dirigida a los dirigentes que ocupan cargos de dirección, administración y gestión organizativa (como Consejos de Grupo, Comités Distritales, Directores de Grupo o Comisionados).
- **AE (Adultos en el Escultismo)**: Orientada a la capacitación de los dirigentes encargados de formar, acompañar y evaluar a otros adultos dentro del sistema educativo de la ASB.
_Avoid_: ModuleType, tipo de módulo, Adjuntos/Asesores de Formación como nombre de línea.

**TrainingLevel (nivel de formación)**:
Calificación de una línea que puede obtener un dirigente; un dirigente puede obtener varios niveles. Cada nivel pertenece estrictamente a una línea:
- Línea **PJ**:
  - `IM I-II` (Insignia de Madera I-II): Nivel único de la línea Programa de Jóvenes (PJ), no dos niveles separados.
  - `IM III` (Insignia de Madera III).
- Línea **GI**:
  - `NIVEL_GRUPO` (Nivel Grupo).
  - `NIVEL_DISTRITO` (Nivel Distrito).
- Línea **AE**:
  - `ASESOR_PERSONAL_FORMACION` (Asesor Personal de Formación).
  - `ADJUNTO_FORMACION` (Adjunto de Formación).
  - `DIRECTOR_FORMACION` (Director de Formación).

**IM I-II (Insignia de Madera I-II)**:
Nivel único de la línea Programa de Jóvenes (PJ), no dos niveles separados.
_Avoid_: IM I e IM II como niveles independientes.

**TrainingModule (módulo de formación)**:
Contenido formativo del catálogo de módulos curricular, perteneciente a una o más líneas de formación, que se imparte en una o varias sesiones.

**Programación de un módulo de formación**:
Oferta de un módulo para fechas concretas, a la que las personas pueden inscribirse. Una nueva programación del mismo módulo permite una nueva inscripción; el cupo corresponde a la programación.
_Avoid_: Convocatoria.

**Sesión**:
Encuentro presencial o virtual que forma parte de un módulo de formación programado. Sus fechas, horarios y modalidades pueden diferir; la inscripción comprende todas sus sesiones.

**Disertante**:
Persona que imparte una sesión de formación. Cada sesión tiene un solo disertante; distintas sesiones de una programación pueden tener disertantes diferentes.

**MandatoryModule (módulo obligatorio)**:
Módulo de la lista reglamentaria elegible de un nivel; pertenecer a esa lista no implica que deban aprobarse todos sus módulos. Una aprobación acreditada en el historial puede reutilizarse para satisfacer requisitos de otros niveles sin repetir el módulo.

**VoluntaryModule (módulo voluntario)**:
Módulo de formación electivo que el dirigente cursa para completar, junto con los obligatorios aprobados, el total exigido por el nivel.
_Avoid_: Módulo ordinario.

**LevelRequirement (requisitos de nivel)**:
Reglas y condiciones estándar necesarias para la aprobación de un nivel de formación. Es un modelo extensible para diversos tipos de criterios. Para los módulos curriculares, define:
1. La lista de módulos obligatorios elegibles.
2. El mínimo de obligatorios a aprobar.
3. El total de módulos formativos requeridos para el nivel.
No posee periodos de vigencia por fechas.
_Avoid_: CurriculumPlan con DateRange, cuota fija de voluntarios.

**Rover**:
Joven de la modalidad Rover que se inscribe a una programación. La modalidad "Rover sin cuenta" pertenece a la inscripción, no a la persona: se registra con Carnet de Identidad, nombres, apellidos y celular opcional, sin credenciales, afiliación, rama ni rol, y se rechaza si la persona ya tiene cuenta.
_Avoid_: tratar al Rover sin cuenta como dirigente o como usuario.

**Secretaría (Secretaría Distrital)**:
Rol `DISTRICT_SECRETARY` que busca personas por Carnet de Identidad e inscribe de forma asistida (dirigente existente, dirigente nuevo o Rover sin cuenta) en una programación. Es el único rol autorizado para los endpoints de Secretaría; ADMIN no los usa.
_Avoid_: Administración, ADMIN.

**Invitación**:
Correo con un enlace de un solo uso (vence en 24 horas) para que una persona creada por la inscripción asistida elija su contraseña y active su cuenta. Queda PENDING si el envío falla y solo se reintenta manualmente; nunca duplica la inscripción.
_Avoid_: Recuperación de contraseña, invitación a la programación.
