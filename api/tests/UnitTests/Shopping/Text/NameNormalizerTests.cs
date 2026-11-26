using Shopping.Domain.Parsing;

namespace UnitTests.Shopping.Text;

public sealed class NameNormalizerTests
{
    [Theory]
    [InlineData("Зелёный  Чай ", "зеленый чай")]
    [InlineData("  МЁД\t липовый", "мед липовый")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Names_AreLowerCased_WithYo_AsYe_AndSpacesCollapsed(string? input, string expected)
    {
        Assert.Equal(expected, NameNormalizer.Normalize(input));
    }
}
