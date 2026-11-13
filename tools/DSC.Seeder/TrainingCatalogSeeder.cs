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
        string[] MandatoryNames
    );

    private sealed record ModuleSeed(string Name, string? Description, string[] LineCodes);

    private static readonly LineSeed[] Lines =
    [
        new("PJ", "Programa de Jóvenes", "Está orientada a capacitar a los Dirigentes de Unidad"),
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
            MandatoryNames:
            [
                "Responsabilidad legal del dirigente",
                "Impacto ambiental en las actividades scouts",
                "Primeros auxilios",
                "Seguimiento del Ciclo de Programa",
            ]
        ),
        new(
            "IM III",
            TotalModules: 16,
            MinMandatory: 4,
            MandatoryNames:
            [
                "Responsabilidad legal del dirigente",
                "Impacto ambiental en las actividades scouts",
                "Primeros auxilios",
                "Seguimiento del Ciclo de Programa",
            ]
        ),
        new(
            "NIVEL_GRUPO",
            TotalModules: 12,
            MinMandatory: 2,
            MandatoryNames:
            [
                "Liderazgo",
                "ResoluciÃ³n de Conflictos",
                "Comunicaciones",
                "Trabajo en Equipo",
            ]
        ),
        new(
            "NIVEL_DISTRITO",
            TotalModules: 18,
            MinMandatory: 4,
            MandatoryNames:
            [
                "Liderazgo",
                "ResoluciÃ³n de Conflictos",
                "Comunicaciones",
                "Trabajo en Equipo",
            ]
        ),
    ];

    private static readonly ModuleSeed[] Modules =
    [
        // Módulos obligatorios para niveles IM I-II e IM III (PJ)
        new("Responsabilidad legal del dirigente", "Obligatorio para Insignia de Madera", ["PJ"]),
        new(
            "Impacto ambiental en las actividades scouts",
            "Obligatorio para Insignia de Madera",
            ["PJ"]
        ),
        new("Primeros auxilios", null, ["PJ"]),
        new("Seguimiento del Ciclo de Programa", "Obligatorio para Insignia de Madera", ["PJ"]),
        // Módulos obligatorios para Nivel Grupo y Nivel Distrito (GI)
        new("Liderazgo", "Obligatorio para Dirigentes Institucionales", ["GI", "PJ", "AE"]),
        new(
            "Resolución de Conflictos",
            "Obligatorio para Dirigentes Institucionales",
            ["GI", "PJ", "AE"]
        ),
        new("Comunicaciones", "Obligatorio para Dirigentes Institucionales", ["GI", "PJ", "AE"]),
        new("Trabajo en Equipo", "Obligatorio para Dirigentes Institucionales", ["GI", "PJ", "AE"]),
        // Área: Valores
        new("Los valores personales en el Movimiento Scout", "Área: Valores", ["PJ", "GI", "AE"]),
        new("Educación en valores durante la infancia", "Área: Valores", ["PJ"]),
        new(
            "La Ley Scout y la formación de una escala personal de valores",
            "Área: Valores",
            ["PJ", "AE"]
        ),
        new("Los valores sociales en el Movimiento Scout", "Área: Valores", ["PJ", "GI", "AE"]),
        new("La educación para los derechos humanos en la adolescencia", "Área: Valores", ["PJ"]),
        new("Respeto y comprensión intercultural", "Área: Valores", ["PJ", "GI", "AE"]),
        new("Los derechos del niño", "Área: Valores", ["PJ", "GI"]),
        new("Aprendizaje de la solidaridad en la juventud", "Área: Valores", ["PJ"]),
        new(
            "Movimiento Scout y participación política de los jóvenes",
            "Área: Valores",
            ["PJ", "GI"]
        ),
        new("Los valores espirituales en el Movimiento Scout", "Área: Valores", ["PJ", "AE"]),
        new("La educación de la fe en los jóvenes", "Área: Valores", ["PJ"]),
        // Área: Conocimiento del joven
        new(
            "La autoestima de niños, jóvenes y adolescentes",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "Desarrollo de niños y jóvenes en hogares uniparentales",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new("Prevención del abuso infantil", "Área: Conocimiento del joven", ["PJ", "GI"]),
        new(
            "Prevención del abuso sexual en niños, niñas y adolescentes",
            "Área: Conocimiento del joven",
            ["PJ", "GI"]
        ),
        new("La sexualidad en la pubertad", "Área: Conocimiento del joven", ["PJ"]),
        new(
            "La influencia de la televisión en los jóvenes",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "La estimulación de la creatividad en los jóvenes de 15 a 17 años",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new("La crisis de originalidad en la adolescencia", null, ["PJ"]),
        new("La comunicación en la familia", "Área: Conocimiento del joven", ["PJ", "GI"]),
        new(
            "Autoridad, libertad y participación en jóvenes de 17 a 20 años",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        new(
            "Factores que influyen en la opción vocacional",
            "Área: Conocimiento del joven",
            ["PJ"]
        ),
        // Área: Programa de jóvenes
        new("Presentación general de MACPRO", "Área: Programa de jóvenes", ["PJ"]),
        new("Fichas de actividades REME", "Área: Programa de jóvenes", ["PJ"]),
        new("Los nuevos paradigmas del programa scout", "Área: Programa de jóvenes", ["PJ", "AE"]),
        new("Planificación de actividades", "Área: Programa de jóvenes", ["PJ"]),
        new("Planificación de Reuniones", "Área: Programa de jóvenes", ["PJ"]),
        new("Manejo de cartillas de progreso personal", null, ["PJ"]),
        new("Escultismo para todos", "Área: Programa de jóvenes", ["PJ", "GI"]),
        new("JOTA/JOTI, A Limpiar El Mundo", "Área: Programa de jóvenes", ["PJ"]),
        new("Actividades de desarrollo de la comunidad local", null, ["PJ", "GI"]),
        new(
            "Actividades de educación para la paz y la comprensión intercultural",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "Integración de los padres al programa de jóvenes",
            "Área: Programa de jóvenes",
            ["PJ", "GI"]
        ),
        new(
            "Prevención del alcoholismo y del consumo de drogas",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new(
            "La dimensión internacional en el programa de jóvenes",
            "Área: Programa de jóvenes",
            ["PJ"]
        ),
        new("Proyectos de desarrollo comunitario", "Área: Programa de jóvenes", ["PJ", "GI"]),
        new("Educación en el desarrollo", "Área: Programa de jóvenes", ["PJ"]),
        new(
            "Proyectos prioritarios de la Asociación",
            "Área: Programa de jóvenes",
            ["PJ", "GI", "AE"]
        ),
        // Área: Método Scout
        new("Juegos y canciones", null, ["PJ"]),
        new("La cacería en la rama lobatos", "Área: Método Scout", ["PJ"]),
        new("La aventura en la rama scout", "Área: Método Scout", ["PJ"]),
        new("La empresa en la rama pioneros", null, ["PJ"]),
        new("La sabiduría del libro de la selva", "Área: Método Scout", ["PJ"]),
        new("Fiestas, celebraciones y ceremonias en la Unidad", "Área: Método Scout", ["PJ"]),
        new("La coeducación", "Área: Método Scout", ["PJ"]),
        new("Orden y disciplina en la Unidad", "Área: Método Scout", ["PJ"]),
        new("Evaluación de participantes", "Área: Método Scout", ["PJ", "AE"]),
        // Área: Vida al Aire libre
        new("Organización de campamentos", "Área: Vida al Aire libre", ["PJ"]),
        new("Juegos de medio ambiente", "Área: Vida al Aire libre", ["PJ"]),
        new("Primeros auxilios en las Ramas Intermedias", null, ["PJ"]),
        new("Técnicas de orientación", "Área: Vida al Aire libre", ["PJ"]),
        new("Amarres y ensambles", null, ["PJ"]),
        new("Seguridad en las excursiones scouts", "Área: Vida al Aire libre", ["PJ"]),
        new("Menú de un campamento de Tropa", "Área: Vida al Aire libre", ["PJ"]),
        new("Vivencia de una excursión en las Ramas Mayores", "Área: Vida al Aire libre", ["PJ"]),
        new(
            "Construcciones de cocina y comedor, etc. en campamentos",
            "Área: Vida al Aire libre",
            ["PJ"]
        ),
        // Área: Recursos educativos
        new("Publicaciones de apoyo al programa", "Área: Recursos educativos", ["PJ", "AE"]),
        new("Estudios de casos", null, ["AE"]),
        new("Técnicas de dirección del Curso Informativo", "Área: Recursos educativos", ["AE"]),
        new("Taller de aprendizaje de juegos", "Área: Recursos educativos", ["PJ", "AE"]),
        new("Taller de expresión artística", "Área: Recursos educativos", ["PJ"]),
        new("Promoción de la lectura", "Área: Recursos educativos", ["PJ"]),
        new("Organización de cursos del sistema de formación", "Área: Recursos educativos", ["AE"]),
        new("Conducción de un módulo de formación", "Área: Recursos educativos", ["AE"]),
        new("Uso de recursos audiovisuales", null, ["AE", "GI"]),
        new("Diseño de módulos de formación", "Área: Recursos educativos", ["AE"]),
        new("Técnicas de dinámica de grupos", "Área: Recursos educativos", ["AE", "PJ"]),
        // Área: Gestión
        new("El Grupo Scout", null, ["GI"]),
        new("El comité y el consejo de grupo", "Área: Gestión", ["GI"]),
        new("La obtención de recursos financieros en el Grupo", "Área: Gestión", ["GI"]),
        new("Actividades de financiamiento", "Área: Gestión", ["GI"]),
        new("Relaciones con la Institución Patrocinadora", "Área: Gestión", ["GI"]),
        new("Relaciones con otras instituciones", "Área: Gestión", ["GI"]),
        new("Relación con los padres de familia", "Área: Gestión", ["GI", "PJ"]),
        new("Captación de adultos para el Grupo scout", "Área: Gestión", ["GI", "AE"]),
        // Área: Desarrollo de proyectos
        new("Estrategia regional", "Área: Desarrollo de proyectos", ["GI"]),
        new("Marketing del Movimiento Scout", null, ["GI"]),
        new(
            "Escultismo para qué?, escultismo para quienes?",
            "Área: Desarrollo de proyectos",
            ["GI", "PJ", "AE"]
        ),
        new("Administración de una unidad scout", "Área: Desarrollo de proyectos", ["PJ", "GI"]),
        new(
            "Técnicas de formación de Grupos y crecimiento",
            "Área: Desarrollo de proyectos",
            ["GI"]
        ),
        new("Para llegar a ser 24", "Área: Desarrollo de proyectos", ["GI"]),
        // Área: Desarrollo personal
        new(
            "Asertividad y comunicación interpersonal",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new("Técnicas de solución de conflictos", "Área: Desarrollo personal", ["GI", "AE", "PJ"]),
        new("Estilos y habilidades de liderazgo", "Área: Desarrollo personal", ["GI", "AE", "PJ"]),
        new("Principios de la comunicación", "Área: Desarrollo personal", ["GI", "AE", "PJ"]),
        new("El abismo de las comunicaciones", "Área: Desarrollo personal", ["GI"]),
        new("Las relaciones interpersonales", null, ["GI", "AE", "PJ"]),
        new("Liderazgo situacional", "Área: Desarrollo personal", ["GI", "AE"]),
        new("Calidad total", null, ["GI"]),
        new(
            "Relaciones humanas (descripción de comportamientos)",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new(
            "Relaciones humanas (descripción de sentimientos)",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new("Motivación a los dirigentes", "Área: Desarrollo personal", ["GI", "AE"]),
        new("Las nueve \"S\"", null, ["GI"]),
        new("Técnicas de redacción", "Área: Desarrollo personal", ["GI", "AE"]),
        new(
            "Primero lo primero y los 7 hábitos de la gente altamente eficiente",
            "Área: Desarrollo personal",
            ["GI", "AE", "PJ"]
        ),
        new("Técnicas de conducción de sesiones", "Área: Desarrollo personal", ["GI", "AE"]),
        new("El proceso grupal", "Área: Desarrollo personal", ["GI", "AE", "PJ"]),
        new("El educador voluntario de tiempo libre", "Área: Desarrollo personal", ["AE", "PJ"]),
        new("Desarrollo de la creatividad", "Área: Desarrollo personal", ["PJ", "AE"]),
    ];

    public static async Task SeedAsync(
        ScoutDbContext db,
        CancellationToken cancellationToken = default
    )
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
                line.Update(seed.Name, seed.Description);
                linesUpdated++;
            }
            else
            {
                line = new TrainingLine(seed.Code, seed.Name, seed.Description);
                db.TrainingLines.Add(line);
                existingLines[seed.Code] = line;
                linesCreated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine(
            $"Líneas de formación: {linesCreated} creadas, {linesUpdated} actualizadas."
        );

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
                level.Update(seed.Name, line.Id, isActive: true);
                levelsUpdated++;
            }
            else
            {
                level = new TrainingLevel(seed.Code, seed.Name, line.Id, isActive: true);
                db.TrainingLevels.Add(level);
                existingLevels[seed.Code] = level;
                levelsCreated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine(
            $"Niveles de formación: {levelsCreated} creados, {levelsUpdated} actualizados."
        );

        var existingModules = await db
            .TrainingModules.Include(m => m.ModuleLines)
            .ToDictionaryAsync(
                x => x.Name,
                x => x,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken
            );

        var modulesCreated = 0;
        var modulesUpdated = 0;
        foreach (var seed in Modules)
        {
            if (existingModules.TryGetValue(seed.Name, out var module))
            {
                module.Name = seed.Name;
                module.Description = seed.Description;
                modulesUpdated++;
            }
            else
            {
                module = new TrainingModule { Name = seed.Name, Description = seed.Description };
                db.TrainingModules.Add(module);
                existingModules[seed.Name] = module;
                modulesCreated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine(
            $"Módulos de formación: {modulesCreated} creados, {modulesUpdated} actualizados."
        );

        var moduleLinesAdded = 0;
        foreach (var seed in Modules)
        {
            var module = existingModules[seed.Name];
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

        var trackedLevels = await db
            .TrainingLevels.Include(l =>
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
            var mandatoryModules = seed
                .MandatoryNames.Select(name => existingModules[name])
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
        Console.WriteLine(
            "Verificación del catálogo: OK (3 líneas, 7 niveles, 107 módulos, 4 requisitos válidos)."
        );
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

        var reqs = await db
            .TrainingLevelRequirements.Include(r => r.TrainingLevel)
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
