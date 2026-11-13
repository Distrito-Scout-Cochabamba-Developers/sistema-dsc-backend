using DSC.Domain.Entities;
using DSC.Domain.Exceptions;

namespace DSC.UnitTests.Domain;

public class TrainingLevelRequirementTests
{
    [Fact]
    public void InsigniaDeMaderaI_II_RequiresTwelveModulesWithAtLeastTwoOfFourMandatoryModules()
    {
        var requirement = TrainingLevelRequirement.Create(
            1,
            12,
            2,
            ScoutTrainingCatalog.Mandatory()
        );

        Assert.Equal(12, requirement.TotalModulesCount);
        Assert.Equal(2, requirement.MinMandatoryModulesCount);
        Assert.Equal(
            new[]
            {
                "CICLO-PROGRAMA",
                "IMPACTO-AMBIENTAL",
                "PRIMEROS-AUXILIOS",
                "RESPONSABILIDAD-LEGAL",
            },
            requirement.MandatoryModules.Select(link => link.TrainingModule.Code).Order()
        );
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(12, -1)]
    [InlineData(1, 2)]
    [InlineData(12, 5)]
    public void InvalidCounts_LeaveInsigniaDeMaderaConfigurationUnchanged(int total, int minimum)
    {
        var requirement = TrainingLevelRequirement.Create(
            1,
            12,
            2,
            ScoutTrainingCatalog.Mandatory()
        );

        Assert.Throws<DomainException>(() =>
            requirement.ConfigureCurricularModules(total, minimum, ScoutTrainingCatalog.Mandatory())
        );

        Assert.Equal(12, requirement.TotalModulesCount);
        Assert.Equal(2, requirement.MinMandatoryModulesCount);
        Assert.Equal(
            new long[] { 1, 2, 3, 4 },
            requirement.MandatoryModules.Select(link => link.TrainingModuleId).Order()
        );
    }

    [Theory]
    [InlineData("null-selection")]
    [InlineData("null-module")]
    [InlineData("unpersisted-module")]
    [InlineData("duplicate-first-aid")]
    public void InvalidModuleSelection_DoesNotChangeExistingRequirements(string scenario)
    {
        var requirement = TrainingLevelRequirement.Create(
            1,
            12,
            2,
            ScoutTrainingCatalog.Mandatory()
        );
        TrainingModule[]? selection = scenario switch
        {
            "null-selection" => null,
            "null-module" => [null!],
            "unpersisted-module" => [new() { Code = "TRIBU-TIERRA", Name = "Tribu Tierra" }],
            "duplicate-first-aid" =>
            [
                ScoutTrainingCatalog.Mandatory()[2],
                ScoutTrainingCatalog.Mandatory()[2],
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        Assert.Throws<DomainException>(() =>
            requirement.ConfigureCurricularModules(12, 2, selection!)
        );
        Assert.Equal(12, requirement.TotalModulesCount);
        Assert.Equal(2, requirement.MinMandatoryModulesCount);
        Assert.Equal(
            new long[] { 1, 2, 3, 4 },
            requirement.MandatoryModules.Select(link => link.TrainingModuleId).Order()
        );
    }
}

internal static class ScoutTrainingCatalog
{
    internal const string LevelCode = "IM-I-II";

    // Códigos descriptivos de fixtures; no son códigos oficiales del catálogo DSC.
    internal static TrainingModule[] Mandatory() =>
        [
            new()
            {
                Id = 1,
                Code = "RESPONSABILIDAD-LEGAL",
                Name = "Responsabilidad legal del dirigente",
            },
            new()
            {
                Id = 2,
                Code = "IMPACTO-AMBIENTAL",
                Name = "Impacto ambiental en actividades Scouts",
            },
            new()
            {
                Id = 3,
                Code = "PRIMEROS-AUXILIOS",
                Name = "Primeros auxilios",
            },
            new()
            {
                Id = 4,
                Code = "CICLO-PROGRAMA",
                Name = "Ciclo de programa",
            },
        ];

    internal static TrainingModule[] Voluntary() =>
        [
            new()
            {
                Id = 5,
                Code = "TRIBU-TIERRA",
                Name = "Tribu Tierra",
            },
            new()
            {
                Id = 6,
                Code = "PARA-LLEGAR-A-SER-24",
                Name = "Para llegar a ser 24",
            },
            new()
            {
                Id = 7,
                Code = "SISTEMA-EQUIPOS",
                Name = "El sistema de equipos",
            },
        ];

    internal static string[] MandatoryCodes =>
        ["RESPONSABILIDAD-LEGAL", "IMPACTO-AMBIENTAL", "PRIMEROS-AUXILIOS", "CICLO-PROGRAMA"];
}
