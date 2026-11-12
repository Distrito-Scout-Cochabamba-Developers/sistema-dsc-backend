using System.Reflection;
using DSC.Domain.Entities;

namespace DSC.UnitTests.Domain;

public class TrainingCatalogContractTests
{
    [Fact]
    public void CatalogEntities_ShouldKeepExistingSettersPrivate()
    {
        var exposedSetters = new[] { typeof(TrainingLine), typeof(TrainingLevel) }
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(property => property.SetMethod is { IsPrivate: false })
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name}");

        Assert.True(
            !exposedSetters.Any(),
            $"Se requieren setters privados: {string.Join(", ", exposedSetters)}"
        );
    }

    [Fact]
    public void CatalogIds_ShouldBeDistinctImmutableDomainValueObjects()
    {
        // El contrato exige IDs de dominio, no nombres de tipos ni constructores específicos.
        var idTypes = new[] { typeof(TrainingLine), typeof(TrainingLevel) }
            .Select(entity => entity.GetProperty("Id")!.PropertyType)
            .ToArray();
        Assert.True(
            idTypes.All(type => type.Assembly == typeof(TrainingLine).Assembly),
            "TrainingLine.Id y TrainingLevel.Id deben usar Value Objects de DSC.Domain, no primitivos."
        );
        Assert.NotEqual(idTypes[0], idTypes[1]);
        foreach (var type in idTypes)
        {
            Assert.DoesNotContain(
                type.GetFields(BindingFlags.Public | BindingFlags.Instance),
                field => !field.IsInitOnly
            );
            Assert.DoesNotContain(
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance),
                property =>
                    property.SetMethod is { IsPublic: true }
                    && !property
                        .SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit))
            );
        }
    }

    [Fact]
    public void CatalogEntities_ShouldNotDeclarePersistenceDataAnnotations()
    {
        foreach (var type in new[] { typeof(TrainingLine), typeof(TrainingLevel) })
        {
            var attributes = type.GetCustomAttributesData()
                .Concat(
                    type.GetProperties().SelectMany(property => property.GetCustomAttributesData())
                );
            Assert.DoesNotContain(
                attributes,
                attribute =>
                    attribute.AttributeType.Namespace?.StartsWith(
                        "System.ComponentModel.DataAnnotations",
                        StringComparison.Ordinal
                    ) == true
            );
        }
    }
}
