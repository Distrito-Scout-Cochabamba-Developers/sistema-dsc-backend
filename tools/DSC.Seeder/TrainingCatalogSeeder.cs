// ============================================================================
// Catálogo de Formación Scout - Líneas, Niveles, Módulos y Requisitos
// ============================================================================

using DSC.Domain.Entities;
using DSC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DSC.Seeder;

public static class TrainingCatalogSeeder
{
    private sealed record LineSeed(string Code, string Name, string Description);

    private sealed record LevelSeed(string Code, string Name, string LineCode);

    private sealed record RequirementSeed(
        string LevelCode,
        int TotalModules,
        int MinMandatory,
        string[] MandatoryCodes
    );

    private sealed record ModuleSeed(
        string Code,
        string Name,
        string? Description,
        string[] LineCodes
    );

    private static readonly LineSeed[] Lines =
    [
        new(
            "PJ",
            "Programa de Jóvenes",
            "Está orientada a capacitar a los Dirigentes de Unidad"
        ),
        new(
            "GI",
            "Gestión Institucional",
            "Dirigida a los dirigentes que ocupan cargos de dirección, administración y gestión organizativa (como Consejos de Grupo, Comités Distritales, Directores de Grupo o Comisionados)"
        ),
        new(
            "AE",
            "Adultos en el Escultismo",
            "Orientada a la capacitación de los dirigentes encargados de formar, acompañar y evaluar a otros adultos dentro del sistema educativo de la ASB."
        ),
    ];

    private static readonly LevelSeed[] Levels =
    [
        new("IM I-II", "Insignia de Madera I-II", "PJ"),
        new("IM III", "Insignia de Madera III", "PJ"),
        new("NIVEL_GRUPO", "Nivel Grupo", "GI"),
        new("NIVEL_DISTRITO", "Nivel Distrito", "GI"),
        new("ASESOR_PERSONAL_FORMACION", "Asesor Personal de Formación", "AE"),
        new("ADJUNTO_FORMACION", "Adjunto de Formación", "AE"),
        new("DIRECTOR_FORMACION", "Director de Formación", "AE"),
    ];

    private static readonly RequirementSeed[] Requirements =
    [
        new(
            "IM I-II",
            TotalModules: 12,
            MinMandatory: 2,
            MandatoryCodes:
            [
                "MOD-RESP-LEGAL",
                "MOD-IMP-AMBIENTAL",
                "MOD-PRIMEROS-AUXIL",
                "MOD-SEG-CICLO-PROG",
            ]
        ),
        new(
            "IM III",
            TotalModules: 16,
            MinMandatory: 4,
            MandatoryCodes:
            [
                "MOD-RESP-LEGAL",
                "MOD-IMP-AMBIENTAL",
                "MOD-PRIMEROS-AUXIL",
                "MOD-SEG-CICLO-PROG",
            ]
        ),
        new(
            "NIVEL_GRUPO",
            TotalModules: 12,
            MinMandatory: 2,
            MandatoryCodes:
            [
                "MOD-LIDERAZGO",
                "MOD-RESOL-CONFLICTOS",
                "MOD-COMUNICACIONES",
                "MOD-TRABAJO-EQUIPO",
            ]
        ),
        new(
            "NIVEL_DISTRITO",
            TotalModules: 18,
            MinMandatory: 4,
            MandatoryCodes:
            [
                "MOD-LIDERAZGO",
                "MOD-RESOL-CONFLICTOS",
                "MOD-COMUNICACIONES",
                "MOD-TRABAJO-EQUIPO",
            ]
        ),
    ];

    private static readonly ModuleSeed[] Modules =
    [
        // Módulos obligatorios para niveles IM I-II e IM III (PJ)
        new(
            "MOD-RESP-LEGAL",
            "Responsabilidad legal del dirigente",
            "Obligatorio para Insignia de Madera",
            ["PJ"]
        ),
        new(
            "MOD-IMP-AMBIENTAL",
            "Impacto ambiental en las actividades scouts",
            "Obligatorio para Insignia de Madera",
            ["PJ"]
        ),
        new(
            "MOD-PRIMEROS-AUXIL",
            "Primeros auxilios",
            "Obligatorio para Insignia de Madera",
            ["PJ"]
        ),
        new(
            "MOD-SEG-CICLO-PROG",
            "Seguimiento del Ciclo de Programa",
            "Obligatorio para Insignia de Madera",
            ["PJ"]
        ),

        // Módulos obligatorios para Nivel Grupo y Nivel Distrito (GI)
        new(
            "MOD-LIDERAZGO",
            "Liderazgo",
            "Obligatorio para Dirigentes Institucionales",
            ["GI", "PJ", "AE"]
        ),
        new(
            "MOD-RESOL-CONFLICTOS",
            "Resolución de Conflictos",
            "Obligatorio para Dirigentes Institucionales",
            ["GI", "PJ", "AE"]
        ),
        new(
            "MOD-COMUNICACIONES",
            "Comunicaciones",
            "Obligatorio para Dirigentes Institucionales",
            ["GI", "PJ", "AE"]
        ),
        new(
            "MOD-TRABAJO-EQUIPO",
            "Trabajo en Equipo",
            "Obligatorio para Dirigentes Institucionales",
            ["GI", "PJ", "AE"]
        ),

        // Área: Valores
        new(
            "MOD-VAL-PERSONALES",
            "Los valores personales en el Movimiento Scout",
            "Área: Valores",
            ["PJ", "GI", "AE"]
        ),
        new(
            "MOD-VAL-INFANCIA",
            "Educación en valores durante la infancia",
            "Área: Valores",
            ["PJ"]
        ),
        new(
            "MOD-LEY-SCOUT-VAL",
            "La Ley Scout y la formación de una escala personal de valores",
            "Área: Valores",
            ["PJ", "AE"]
        ),
        new(
            "MOD-VAL-SOCIALES",
            "Los valores sociales en el Movimiento Scout",
            "Área: Valores",
            ["PJ", "GI", "AE"]
        ),
        new(
            "MOD-DDHH-ADOLESCENCIA",
            "La educación para los derechos humanos en la adolescencia",
            "Área: Valores",
            ["PJ"]
        ),
        new(
            "MOD-INTERCULTURALIDAD",
            "Respeto y comprensión intercultural",
            "Área: Valores",
            ["PJ", "GI", "AE"]
        ),
        new("MOD-DERECHOS-NINO", "Los derechos del niño", "Área: Valores", ["PJ", "GI"]),
        new(
            "MOD-SOLIDARIDAD-JUV",
            "Aprendizaje de la solidaridad en la juventud",
            "Área: Valores",
            ["PJ"]
        ),
        new(
            "MOD-PARTICIPACION-POL",
            "Movimiento Scout y participación política de los jóvenes",
            "Área: Valores",
            ["PJ", "GI"]
        ),
        new(
            "MOD-VAL-ESPIRITUALES",
            "Los valores espirituales en el Movimiento Scout",
            "Área: Valores",
            ["PJ", "AE"]
        ),
        new(
            "MOD-EDUCACION-FE-JUV",
            "La educación de la fe en los jóvenes",
            "Área: Valores",
            ["PJ"]
        ),

        // Área: Conocimiento del joven
        new(
            "MOD-AUTOESTIMA-JOVEN",
            "La autoestima de niños, jóvenes y adolescentes",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-HOGARES-UNIPARENT",
            "Desarrollo de niños y jóvenes en hogares uniparentales",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-PREV-ABUSO-INF",
            "Prevención del abuso infantil",
            "Área: Conocimiento del joven",
            ["PJ", "GI"]
        ),
        new(
            "MOD-PREV-ABUSO-SEXUAL",
            "Prevención del abuso sexual en niños, niñas y adolescentes",
            "Área: Conocimiento del joven",
            ["PJ", "GI"]
        ),
        new(
            "MOD-SEXUALIDAD-PUBERTAD",
            "La sexualidad en la pubertad",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-INFLUENCIA-TV",
            "La influencia de la televisión en los jóvenes",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-CREAT-15-17",
            "La estimulación de la creatividad en los jóvenes de 15 a 17 años",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-CRISIS-ORIG-ADOL",
            "La crisis de originalidad en la adolescencia",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-COMUN-FAMILIA",
            "La comunicación en la familia",
            "Área: Conocimiento del joven",
            ["PJ", "GI"]
        ),
        new(
            "MOD-AUT-LIB-17-20",
            "Autoridad, libertad y participación en jóvenes de 17 a 20 años",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "MOD-OPCION-VOCACIONAL",
            "Factores que influyen en la opción vocacional",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),

        // Área: Programa de jóvenes
        new(
            "MOD-MACPRO-GRAL",
            "Presentación general de MACPRO",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-FICHAS-REME",
            "Fichas de actividades REME",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-PARADIGMAS-PROG",
            "Los nuevos paradigmas del programa scout",
            "Área: Programa de jóvenes",
            ["PJ", "AE"]
        ),
        new(
            "MOD-PLANIF-ACTIVIDADES",
            "Planificación de actividades",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-PLANIF-REUNIONES",
            "Planificación de Reuniones",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-CARTILLAS-PROGRESO",
            "Manejo de cartillas de progreso personal",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-ESCULTISMO-TODOS",
            "Escultismo para todos",
            "Área: Programa de jóvenes",
            ["PJ", "GI"]
        ),
        new(
            "MOD-JOTA-JOTI-LIMPIAR",
            "JOTA/JOTI, A Limpiar El Mundo",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-ACT-DESARROLLO-COM",
            "Actividades de desarrollo de la comunidad local",
            "Área: Programa de jóvenes",
            ["PJ", "GI"]
        ),
        new(
            "MOD-ACT-EDUC-PAZ",
            "Actividades de educación para la paz y la comprensión intercultural",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-INTEG-PADRES-PROG",
            "Integración de los padres al programa de jóvenes",
            "Área: Programa de jóvenes",
            ["PJ", "GI"]
        ),
        new(
            "MOD-PREV-DROGAS-ALC",
            "Prevención del alcoholismo y del consumo de drogas",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-DIM-INTERNACIONAL",
            "La dimensión internacional en el programa de jóvenes",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-PROY-DES-COMUNIT",
            "Proyectos de desarrollo comunitario",
            "Área: Programa de jóvenes",
            ["PJ", "GI"]
        ),
        new(
            "MOD-EDUC-DESARROLLO",
            "Educación en el desarrollo",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "MOD-PROY-PRIORITARIOS",
            "Proyectos prioritarios de la Asociación",
            "Área: Programa de jóvenes",
            ["PJ", "GI", "AE"]
        ),

        // Área: Método Scout
        new("MOD-JUEGOS-CANCIONES", "Juegos y canciones", "Área: Método Scout", ["PJ"]),
        new(
            "MOD-CACERIA-LOBATOS",
            "La cacería en la rama lobatos",
            "Área: Método Scout",
            ["PJ"]
        ),
        new("MOD-AVENTURA-SCOUT", "La aventura en la rama scout", "Área: Método Scout", ["PJ"]),
        new(
            "MOD-EMPRESA-PIONEROS",
            "La empresa en la rama pioneros",
            "Área: Método Scout",
            ["PJ"]
        ),
        new(
            "MOD-LIBRO-SELVA",
            "La sabiduría del libro de la selva",
            "Área: Método Scout",
            ["PJ"]
        ),
        new(
            "MOD-CELEB-CEREMONIAS",
            "Fiestas, celebraciones y ceremonias en la Unidad",
            "Área: Método Scout",
            ["PJ"]
        ),
        new("MOD-COEDUCACION", "La coeducación", "Área: Método Scout", ["PJ"]),
        new(
            "MOD-ORDEN-DISCIPLINA",
            "Orden y disciplina en la Unidad",
            "Área: Método Scout",
            ["PJ"]
        ),
        new(
            "MOD-EVAL-PARTICIPANTES",
            "Evaluación de participantes",
            "Área: Método Scout",
            ["PJ", "AE"]
        ),

        // Área: Vida al Aire libre
        new(
            "MOD-ORG-CAMPAMENTOS",
            "Organización de campamentos",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-JUEGOS-MEDIO-AMB",
            "Juegos de medio ambiente",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-PA-RAMAS-INTERMED",
            "Primeros auxilios en las Ramas Intermedias",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-TECNICAS-ORIENTAC",
            "Técnicas de orientación",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-AMARRES-ENSAMBLES",
            "Amarres y ensambles",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-SEG-EXCURSIONES",
            "Seguridad en las excursiones scouts",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-MENU-CAMP-TROPA",
            "Menú de un campamento de Tropa",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-VIV-EXCURSION-MAY",
            "Vivencia de una excursión en las Ramas Mayores",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        new(
            "MOD-CONSTR-COCINA-COM",
            "Construcciones de cocina y comedor, etc. en campamentos",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),

        // Área: Recursos educativos
        new(
            "MOD-PUB-APOYO-PROG",
            "Publicaciones de apoyo al programa",
            "Área: Recursos educativos",
            ["PJ", "AE"]
        ),
        new("MOD-ESTUDIOS-CASOS", "Estudios de casos", "Área: Recursos educativos", ["AE"]),
        new(
            "MOD-DIR-CURSO-INFORMAT",
            "Técnicas de dirección del Curso Informativo",
            "Área: Recursos educativos",
            ["AE"]
        ),
        new(
            "MOD-TALLER-JUEGOS",
            "Taller de aprendizaje de juegos",
            "Área: Recursos educativos",
            ["PJ", "AE"]
        ),
        new(
            "MOD-TALLER-EXPR-ART",
            "Taller de expresión artística",
            "Área: Recursos educativos",
            ["PJ"]
        ),
        new(
            "MOD-PROMOCION-LECTURA",
            "Promoción de la lectura",
            "Área: Recursos educativos",
            ["PJ"]
        ),
        new(
            "MOD-ORG-CURSOS-FORM",
            "Organización de cursos del sistema de formación",
            "Área: Recursos educativos",
            ["AE"]
        ),
        new(
            "MOD-COND-MODULO-FORM",
            "Conducción de un módulo de formación",
            "Área: Recursos educativos",
            ["AE"]
        ),
        new(
            "MOD-RECURSOS-AUDIOVIS",
            "Uso de recursos audiovisuales",
            "Área: Recursos educativos",
            ["AE", "GI"]
        ),
        new(
            "MOD-DISENO-MOD-FORM",
            "Diseño de módulos de formación",
            "Área: Recursos educativos",
            ["AE"]
        ),
        new(
            "MOD-DINAMICA-GRUPOS",
            "Técnicas de dinámica de grupos",
            "Área: Recursos educativos",
            ["AE", "PJ"]
        ),

        // Área: Gestión
        new("MOD-EL-GRUPO-SCOUT", "El Grupo Scout", "Área: Gestión", ["GI"]),
        new(
            "MOD-COMITE-CONSEJO-GRP",
            "El comité y el consejo de grupo",
            "Área: Gestión",
            ["GI"]
        ),
        new(
            "MOD-REC-FINANCIEROS",
            "La obtención de recursos financieros en el Grupo",
            "Área: Gestión",
            ["GI"]
        ),
        new(
            "MOD-ACT-FINANCIAMIENTO",
            "Actividades de financiamiento",
            "Área: Gestión",
            ["GI"]
        ),
        new(
            "MOD-REL-INST-PATROCIN",
            "Relaciones con la Institución Patrocinadora",
            "Área: Gestión",
            ["GI"]
        ),
        new(
            "MOD-REL-OTRAS-INST",
            "Relaciones con otras instituciones",
            "Área: Gestión",
            ["GI"]
        ),
        new(
            "MOD-REL-PADRES-FAMILIA",
            "Relación con los padres de familia",
            "Área: Gestión",
            ["GI", "PJ"]
        ),
        new(
            "MOD-CAPTACION-ADULTOS",
            "Captación de adultos para el Grupo scout",
            "Área: Gestión",
            ["GI", "AE"]
        ),

        // Área: Desarrollo de proyectos
        new(
            "MOD-ESTRATEGIA-REGION",
            "Estrategia regional",
            "Área: Desarrollo de proyectos",
            ["GI"]
        ),
        new(
            "MOD-MKT-SCOUT",
            "Marketing del Movimiento Scout",
            "Área: Desarrollo de proyectos",
            ["GI"]
        ),
        new(
            "MOD-ESCULTISMO-PARA-QUE",
            "Escultismo para qué?, escultismo para quienes?",
            "Área: Desarrollo de proyectos",
            ["GI", "PJ", "AE"]
        ),
        new(
            "MOD-ADMIN-UNIDAD-SCOUT",
            "Administración de una unidad scout",
            "Área: Desarrollo de proyectos",
            ["PJ", "GI"]
        ),
        new(
            "MOD-FORM-GRUPOS-CREC",
            "Técnicas de formación de Grupos y crecimiento",
            "Área: Desarrollo de proyectos",
            ["GI"]
        ),
        new(
            "MOD-LLEGAR-A-SER-24",
            "Para llegar a ser 24",
            "Área: Desarrollo de proyectos",
            ["GI"]
        ),

        // Área: Desarrollo personal
        new(
            "MOD-ASERTIVIDAD-COM",
            "Asertividad y comunicación interpersonal",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-TEC-SOLUC-CONFLICT",
            "Técnicas de solución de conflictos",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-ESTILOS-LIDERAZGO",
            "Estilos y habilidades de liderazgo",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-PRINCIPIOS-COMUNIC",
            "Principios de la comunicación",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-ABISMO-COMUNICAC",
            "El abismo de las comunicaciones",
            "Área: Desarrollo personal",
            ["GI"]
        ),
        new(
            "MOD-REL-INTERPERSONAL",
            "Las relaciones interpersonales",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-LIDERAZGO-SITUAC",
            "Liderazgo situacional",
            "Área: Desarrollo personal",
            ["GI", "AE"]
        ),
        new("MOD-CALIDAD-TOTAL", "Calidad total", "Área: Desarrollo personal", ["GI"]),
        new(
            "MOD-REL-HUMANAS-COMP",
            "Relaciones humanas (descripción de comportamientos)",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-REL-HUMANAS-SENT",
            "Relaciones humanas (descripción de sentimientos)",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-MOTIVACION-DIRIG",
            "Motivación a los dirigentes",
            "Área: Desarrollo personal",
            ["GI", "AE"]
        ),
        new("MOD-NUEVE-S", "Las nueve \"S\"", "Área: Desarrollo personal", ["GI"]),
        new(
            "MOD-TECNICAS-REDACCION",
            "Técnicas de redacción",
            "Área: Desarrollo personal",
            ["GI", "AE"]
        ),
        new(
            "MOD-7-HABITOS",
            "Primero lo primero y los 7 hábitos de la gente altamente eficiente",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-COND-SESIONES",
            "Técnicas de conducción de sesiones",
            "Área: Desarrollo personal",
            ["GI", "AE"]
        ),
        new(
            "MOD-PROCESO-GRUPAL",
            "El proceso grupal",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "MOD-EDUCADOR-TIEMPO-LIB",
            "El educador voluntario de tiempo libre",
            "Área: Desarrollo personal",
            ["AE", "PJ"]
        ),
        new(
            "MOD-DESARROLLO-CREATIV",
            "Desarrollo de la creatividad",
            "Área: Desarrollo personal",
            ["PJ", "AE"]
        ),
    ];

    public static async Task SeedAsync(ScoutDbContext db, CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine("Cargando Catálogo de Formación Scout");
        Console.WriteLine("==================================================");

        var existingLines = await db.TrainingLines.ToDictionaryAsync(
            x => x.Code,
            x => x,
            StringComparer.OrdinalIgnoreCase,
            cancellationToken
        );

        var linesCreated = 0;
        var linesUpdated = 0;
        foreach (var seed in Lines)
        {
            if (existingLines.TryGetValue(seed.Code, out var line))
            {
                line.Name = seed.Name;
                line.Description = seed.Description;
                linesUpdated++;
            }
            else
            {
                line = new TrainingLine
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    Description = seed.Description,
                };
                db.TrainingLines.Add(line);
                existingLines[seed.Code] = line;
                linesCreated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Líneas de formación: {linesCreated} creadas, {linesUpdated} actualizadas.");

        var existingLevels = await db.TrainingLevels.ToDictionaryAsync(
            x => x.Code,
            x => x,
            StringComparer.OrdinalIgnoreCase,
            cancellationToken
        );

        var levelsCreated = 0;
        var levelsUpdated = 0;
        foreach (var seed in Levels)
        {
            var line = existingLines[seed.LineCode];
            if (existingLevels.TryGetValue(seed.Code, out var level))
            {
                level.Name = seed.Name;
                level.TrainingLineId = line.Id;
                level.IsActive = true;
                levelsUpdated++;
            }
            else
            {
                level = new TrainingLevel
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    TrainingLineId = line.Id,
                    IsActive = true,
                };
                db.TrainingLevels.Add(level);
                existingLevels[seed.Code] = level;
                levelsCreated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Niveles de formación: {levelsCreated} creados, {levelsUpdated} actualizados.");

        var existingModules = await db.TrainingModules
            .Include(m => m.ModuleLines)
            .ToDictionaryAsync(
                x => x.Code,
                x => x,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken
            );

        var modulesCreated = 0;
        var modulesUpdated = 0;
        foreach (var seed in Modules)
        {
            if (existingModules.TryGetValue(seed.Code, out var module))
            {
                module.Name = seed.Name;
                module.Description = seed.Description;
                modulesUpdated++;
            }
            else
            {
                module = new TrainingModule
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    Description = seed.Description,
                };
                db.TrainingModules.Add(module);
                existingModules[seed.Code] = module;
                modulesCreated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Módulos de formación: {modulesCreated} creados, {modulesUpdated} actualizados.");

        var moduleLinesAdded = 0;
        foreach (var seed in Modules)
        {
            var module = existingModules[seed.Code];
            foreach (var lineCode in seed.LineCodes)
            {
                var line = existingLines[lineCode];
                if (!module.ModuleLines.Any(ml => ml.TrainingLineId == line.Id))
                {
                    var link = new TrainingModuleLine
                    {
                        TrainingModuleId = module.Id,
                        TrainingLineId = line.Id,
                    };
                    db.TrainingModuleLines.Add(link);
                    module.ModuleLines.Add(link);
                    moduleLinesAdded++;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Asignaciones módulo-línea agregadas: {moduleLinesAdded}.");

        var trackedLevels = await db.TrainingLevels
            .Include(l =>
                l.Requirements.Where(r => r.RequirementType == TrainingLevelRequirement.ModulesType)
            )
                .ThenInclude(r => r.MandatoryModules)
            .ToDictionaryAsync(
                x => x.Code,
                x => x,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken
            );

        var requirementsConfigured = 0;
        foreach (var seed in Requirements)
        {
            var level = trackedLevels[seed.LevelCode];
            var mandatoryModules = seed.MandatoryCodes
                .Select(code => existingModules[code])
                .ToList();

            var requirement = level.Requirements.SingleOrDefault(r =>
                r.RequirementType == TrainingLevelRequirement.ModulesType
            );

            if (requirement is null)
            {
                level.Requirements.Add(
                    TrainingLevelRequirement.Create(
                        level.Id,
                        seed.TotalModules,
                        seed.MinMandatory,
                        mandatoryModules
                    )
                );
            }
            else
            {
                requirement.ConfigureCurricularModules(
                    seed.TotalModules,
                    seed.MinMandatory,
                    mandatoryModules
                );
            }

            requirementsConfigured++;
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Requisitos de nivel configurados: {requirementsConfigured}.");

        await AssertCatalogValidAsync(db, cancellationToken);
        Console.WriteLine("Verificación del catálogo: OK (3 líneas, 7 niveles, 107 módulos, 4 requisitos válidos).");
    }

    public static async Task AssertCatalogValidAsync(
        ScoutDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        var linesCount = await db.TrainingLines.CountAsync(cancellationToken);
        if (linesCount != 3)
            throw new InvalidOperationException(
                $"Se esperaban 3 líneas de formación, se encontraron {linesCount}."
            );

        var levelsCount = await db.TrainingLevels.CountAsync(cancellationToken);
        if (levelsCount != 7)
            throw new InvalidOperationException(
                $"Se esperaban 7 niveles de formación, se encontraron {levelsCount}."
            );

        var modulesCount = await db.TrainingModules.CountAsync(cancellationToken);
        if (modulesCount != 107)
            throw new InvalidOperationException(
                $"Se esperaban 107 módulos de formación, se encontraron {modulesCount}."
            );

        var reqs = await db.TrainingLevelRequirements
            .Include(r => r.TrainingLevel)
            .Include(r => r.MandatoryModules)
            .ToListAsync(cancellationToken);

        if (reqs.Count != 4)
            throw new InvalidOperationException(
                $"Se esperaban 4 requisitos de nivel, se encontraron {reqs.Count}."
            );

        var im2 = reqs.Single(r => r.TrainingLevel.Code == "IM I-II");
        if (
            im2.TotalModulesCount != 12
            || im2.MinMandatoryModulesCount != 2
            || im2.MandatoryModules.Count != 4
        )
            throw new InvalidOperationException("Requisitos de IM I-II incorrectos.");

        var im3 = reqs.Single(r => r.TrainingLevel.Code == "IM III");
        if (
            im3.TotalModulesCount != 16
            || im3.MinMandatoryModulesCount != 4
            || im3.MandatoryModules.Count != 4
        )
            throw new InvalidOperationException("Requisitos de IM III incorrectos.");

        var grupo = reqs.Single(r => r.TrainingLevel.Code == "NIVEL_GRUPO");
        if (
            grupo.TotalModulesCount != 12
            || grupo.MinMandatoryModulesCount != 2
            || grupo.MandatoryModules.Count != 4
        )
            throw new InvalidOperationException("Requisitos de NIVEL_GRUPO incorrectos.");

        var distrito = reqs.Single(r => r.TrainingLevel.Code == "NIVEL_DISTRITO");
        if (
            distrito.TotalModulesCount != 18
            || distrito.MinMandatoryModulesCount != 4
            || distrito.MandatoryModules.Count != 4
        )
            throw new InvalidOperationException("Requisitos de NIVEL_DISTRITO incorrectos.");
    }
}
