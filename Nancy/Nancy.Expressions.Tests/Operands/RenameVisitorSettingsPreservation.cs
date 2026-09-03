using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class RenameVisitorSettingsPreservation
{
    [Fact]
    public void WithNamePreservesSettingsOnCurveExpressions()
    {
        var settings = new ExpressionSettings { CacheSettings = new CacheSettings { CheapCacheElementThreshold = 14 } };
        var expression = new ConcreteCurveExpression(new ConstantCurve(1), "a", settings);

        var renamed = expression.WithName("b");

        Assert.Same(settings, renamed.Settings);
    }

    [Fact]
    public void WithGenerationPreservesSettingsOnCurveExpressions()
    {
        var settings = new ExpressionSettings { CacheSettings = new CacheSettings { CheapCacheElementThreshold = 14 } };
        var expression = new ConcreteCurveExpression(new ConstantCurve(1), "a", settings);

        var withGeneration = expression.WithGeneration(1);

        Assert.Same(settings, withGeneration.Settings);
    }

    [Fact]
    public void WithNamePreservesSettingsOnRationalExpressions()
    {
        var settings = new ExpressionSettings { CacheSettings = new CacheSettings { CheapCacheElementThreshold = 14 } };
        var expression = new RationalNumberExpression(new Rational(1), "a", settings);

        var renamed = expression.WithName("b");

        Assert.Same(settings, renamed.Settings);
    }
}
