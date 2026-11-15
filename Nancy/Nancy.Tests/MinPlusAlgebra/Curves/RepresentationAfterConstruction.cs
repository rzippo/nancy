using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// The constructors check and normalize the representation of a curve, so nothing can set it afterwards, as an object initializer could through an init setter, nancy#179.
/// </summary>
public class RepresentationAfterConstruction
{
    [Theory]
    [InlineData(nameof(Curve.BaseSequence))]
    [InlineData(nameof(Curve.PseudoPeriodStart))]
    [InlineData(nameof(Curve.PseudoPeriodLength))]
    [InlineData(nameof(Curve.PseudoPeriodHeight))]
    public void TheRepresentationHasNoSetter(string property)
    {
        var info = typeof(Curve).GetProperty(property);
        Assert.NotNull(info);
        Assert.Null(info.SetMethod);
    }
}
