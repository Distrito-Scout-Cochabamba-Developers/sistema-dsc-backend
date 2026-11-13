# Formación DSC

Vocabulario del esquema de formación de dirigentes.

## Language

**TrainingLine (línea de formación)**:
Área de formación que agrupa niveles y comparte módulos con otras líneas: Programa de Jóvenes (PJ), Gestión Institucional (GI) y Adjuntos/Asesores de Formación (AE).
_Avoid_: ModuleType, tipo de módulo.

**TrainingLevel (nivel de formación)**:
Calificación de una línea que puede obtener un dirigente; un dirigente puede obtener varios niveles.

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
