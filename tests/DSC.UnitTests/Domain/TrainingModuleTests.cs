using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using Xunit;

namespace DSC.UnitTests.Domain;

public class TrainingModuleTests
{
    [Fact]
    public void Create_WithValidInputs_ShouldTrimAndBeActive()
    {
        // Act
        var module = TrainingModule.Create("  MOD-01 ", "  Liderazgo  ", "  Descripción ", 1);

        // Assert
        Assert.Equal("MOD-01", module.Code);
        Assert.Equal("Liderazgo", module.Name);
        Assert.Equal("Descripción", module.Description);
        Assert.Equal(1, module.ModuleTypeId);
        Assert.True(module.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankDescription_ShouldStoreNull(string? description)
    {
        // Act
        var module = TrainingModule.Create("MOD-01", "Liderazgo", description, 1);

        // Assert
        Assert.Null(module.Description);
    }

    [Theory]
    [InlineData("", "Liderazgo")]
    [InlineData("   ", "Liderazgo")]
    [InlineData("MOD-01", "")]
    [InlineData("MOD-01", "   ")]
    public void Create_WithEmptyCodeOrName_ShouldThrowDomainException(string code, string name)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => TrainingModule.Create(code, name, null, 1));
    }

    [Fact]
    public void Create_WithCodeTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var code = new string('C', TrainingModule.CodeMaxLength + 1);

        // Act & Assert
        Assert.Throws<DomainException>(() => TrainingModule.Create(code, "Liderazgo", null, 1));
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var name = new string('N', TrainingModule.NameMaxLength + 1);

        // Act & Assert
        Assert.Throws<DomainException>(() => TrainingModule.Create("MOD-01", name, null, 1));
    }

    [Fact]
    public void Update_WithValidInputs_ShouldReplaceValuesAndState()
    {
        // Arrange
        var module = TrainingModule.Create("MOD-01", "Liderazgo", null, 1);

        // Act
        module.Update(" MOD-02 ", " Gestión ", " Nueva ", 2, isActive: false);

        // Assert
        Assert.Equal("MOD-02", module.Code);
        Assert.Equal("Gestión", module.Name);
        Assert.Equal("Nueva", module.Description);
        Assert.Equal(2, module.ModuleTypeId);
        Assert.False(module.IsActive);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowAndKeepPreviousValues()
    {
        // Arrange
        var module = TrainingModule.Create("MOD-01", "Liderazgo", null, 1);

        // Act & Assert
        Assert.Throws<DomainException>(() => module.Update("MOD-02", " ", null, 2, true));
        Assert.Equal("MOD-01", module.Code);
        Assert.Equal("Liderazgo", module.Name);
    }
}
