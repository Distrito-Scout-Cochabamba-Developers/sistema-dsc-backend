using DSC.Domain.Exceptions;
using DSC.Domain.ValueObjects;
using Xunit;

namespace DSC.UnitTests.Domain;

public class CiNumberTests
{
    [Theory]
    [InlineData("7654321", "CB", "7654321-CB")]
    [InlineData("12345678", "LP", "12345678-LP")]
    [InlineData("555444", "", "555444")]
    public void CiNumber_WithValidInputs_ShouldCreateSuccessfully(string number, string extension, string expectedFullValue)
    {
        // Act
        var ci = new CiNumber(number, extension);

        // Assert
        Assert.Equal(number, ci.Number);
        Assert.Equal(expectedFullValue, ci.FullValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CiNumber_WithEmptyNumber_ShouldThrowDomainException(string emptyNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new CiNumber(emptyNumber));
        Assert.Contains("no puede estar vacío", exception.Message);
    }

    [Theory]
    [InlineData("7654321A")]
    [InlineData("ABC1234")]
    [InlineData("123")] // less than 4 digits
    [InlineData("123456789012")] // phone length > 10 digits
    public void CiNumber_WithInvalidNumericFormat_ShouldThrowDomainException(string invalidNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new CiNumber(invalidNumber));
        Assert.Contains("exclusivamente numérico", exception.Message);
    }

    [Fact]
    public void CiNumber_WithInvalidExtension_ShouldThrowDomainException()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new CiNumber("7654321", "INVALID"));
        Assert.Contains("extensión de departamento válida", exception.Message);
    }
}
