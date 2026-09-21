using System.Collections.Generic;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

/// <summary>
/// The identity invariants the other two trees are held to, for the sequence tree.
/// Name, generation and settings do not participate in identity; the value cache does not either, so that an expression's hash does not move when its value is read.
/// </summary>
public class SequenceExpressionIdentity
{
    private static readonly Sequence A = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence B = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);

    private static SequenceExpression Sum(string name = "", ExpressionSettings settings = null)
        => A.ToExpression("a").Addition(B.ToExpression("b"), name, settings);

    [Fact]
    public void ExpressionsDifferingOnlyInNameAreEqual()
        => Assert.Equal(Sum("one"), Sum("two"));

    [Fact]
    public void ExpressionsDifferingOnlyInGenerationAreEqual()
        => Assert.Equal(Sum(), Sum().WithGeneration(3));

    [Fact]
    public void ExpressionsDifferingOnlyInSettingsAreEqual()
        => Assert.Equal(Sum(), Sum(settings: new ExpressionSettings()));

    /// <summary>
    /// The value cache must stay out of identity.
    /// A record would compare it, which makes the hash change the moment the value is read and the expression unfindable as a key.
    /// </summary>
    [Fact]
    public void EvaluatingDoesNotMoveTheHash()
    {
        var expression = Sum();
        var before = expression.GetHashCode();

        _ = expression.Value;

        Assert.Equal(before, expression.GetHashCode());
        Assert.Equal(Sum(), expression);
    }

    [Fact]
    public void AnEvaluatedExpressionIsStillFoundAsAKey()
    {
        var key = Sum();
        var dictionary = new Dictionary<SequenceExpression, string> { [key] = "value" };

        _ = key.Value;

        Assert.True(dictionary.ContainsKey(Sum()));
        Assert.Equal("value", dictionary[Sum()]);
    }

    public static IEnumerable<object[]> DifferingOperands()
    {
        yield return ["unary", A.ToExpression("a").Negate(), B.ToExpression("b").Negate()];
        yield return ["binary", A.ToExpression("a").Deconvolution(B.ToExpression("b")), A.ToExpression("a").Deconvolution(A.ToExpression("a"))];
        yield return ["n-ary", A.ToExpression("a").Addition(B.ToExpression("b")), A.ToExpression("a").Addition(A.ToExpression("a"))];
    }

    /// <summary>
    /// Keeping the cache out of identity must not also take the operands out of it.
    /// </summary>
    [Theory]
    [MemberData(nameof(DifferingOperands))]
    public void ExpressionsOverDifferentOperandsAreNotEqual(string arity, SequenceExpression one, SequenceExpression other)
        => Assert.False(one.Equals(other), arity);

    [Fact]
    public void DifferentOperatorsOverTheSameOperandsAreNotEqual()
        => Assert.NotEqual(
            A.ToExpression("a").Addition(B.ToExpression("b")),
            A.ToExpression("a").Minimum(B.ToExpression("b")));

    /// <summary>
    /// Addition is commutative, so the order its operands were given in is not part of the expression.
    /// </summary>
    [Fact]
    public void SwappingTheOperandsOfACommutativeOperatorIsEqual()
        => Assert.Equal(
            A.ToExpression("a").Addition(B.ToExpression("b")),
            B.ToExpression("b").Addition(A.ToExpression("a")));

    [Fact]
    public void ConcreteSequenceExpressionHashDiffersFromTheBareSequencesHash()
        => Assert.NotEqual(A.GetHashCode(), A.ToExpression("a").GetHashCode());

    [Fact]
    public void SequencePlaceholdersWithDifferentNamesAreNotEqual()
        => Assert.NotEqual(new SequencePlaceholderExpression("x"), new SequencePlaceholderExpression("y"));

    /// <summary>
    /// Renaming is non-destructive: it keeps the settings, and the cached value, which is why it exists rather than a plain <c>with</c>.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RenamingKeepsSettingsAndTheCache(bool byGeneration)
    {
        var expression = Sum("s", new ExpressionSettings());
        _ = expression.Value;

        var renamed = byGeneration ? expression.WithGeneration(2) : expression.WithName("t");

        Assert.NotNull(renamed.Settings);
        Assert.True(renamed.IsComputed);
    }

    /// <summary>
    /// A cached value is only worth reclaiming when it is large, so the threshold is set low enough for these operands to exceed it.
    /// </summary>
    private static ExpressionSettings ClearsEagerly
        => new() { CacheSettings = new CacheSettings { CheapCacheElementThreshold = 1 } };

    /// <summary>
    /// Clearing the cache must reach the sequence tree, including through a rational expression whose operands are sequences.
    /// </summary>
    [Fact]
    public void ClearingTheCacheReachesTheSequenceTree()
    {
        var expression = Sum(settings: ClearsEagerly);
        _ = expression.Value;
        Assert.True(expression.IsComputed);

        expression.ClearValueCache();

        Assert.False(expression.IsComputed);
    }

    /// <summary>
    /// A deviation is a rational expression over sequence operands, so clearing its cache must descend into them.
    /// </summary>
    [Fact]
    public void ClearingARationalExpressionReachesItsSequenceOperands()
    {
        var inner = Sum(settings: ClearsEagerly);
        var deviation = Expressions.HorizontalDeviation(inner, A.ToExpression("a"));
        _ = deviation.Value;
        Assert.True(inner.IsComputed);

        deviation.ClearValueCache();

        Assert.False(inner.IsComputed);
    }

    /// <summary>
    /// The placeholder has a factory, as the other two trees do.
    /// </summary>
    [Fact]
    public void ThePlaceholderHasAFactory()
    {
        var placeholder = Expressions.SequencePlaceholder("x");

        Assert.Equal(new SequencePlaceholderExpression("x"), placeholder);
    }
}
