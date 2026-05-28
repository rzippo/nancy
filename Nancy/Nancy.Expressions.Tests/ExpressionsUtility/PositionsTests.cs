using Unipi.Nancy.Expressions.ExpressionsUtility;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

public class PositionsTests
{
    [Fact]
    public void Constants_HaveExpectedValues()
    {
        Assert.Equal("LeftOperand", Positions.LeftOperand);
        Assert.Equal("RightOperand", Positions.RightOperand);
        Assert.Equal("Operand", Positions.InnerOperand);
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(42, "42")]
    public void IndexedOperand_ReturnsIndexString(int index, string expected)
    {
        Assert.Equal(expected, Positions.IndexedOperand(index));
    }
}
