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
Unidad del catálogo curricular perteneciente a una o más líneas de formación, no a niveles.

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
