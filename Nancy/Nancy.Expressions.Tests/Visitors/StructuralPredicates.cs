using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

/// <summary>
/// Every predicate a curve expression answers from its structure agrees with the same predicate of its computed curve.
/// </summary>
/// <remarks>
/// The operands reach the edges of the shortcuts: infinities, values at the origin alone, constants with and without the origin, jumps of either continuity, oscillations.
/// An expression with a documented domain failure is left out, and so is a predicate the computed curve reports undetermined: neither has an answer to agree with.
/// Where the curve's predicate throws otherwise, the expression's must throw the same.
/// The smoke test checks that every operation has a computable zero-operand case.
/// </remarks>
public class StructuralPredicates
{
    private static readonly Curve NegativeOriginOnly = new(new Sequence([
        new Point(0, -1), new Segment(0, 1, 1, 0),
        new Point(1, 1), new Segment(1, 2, 1, 0)
    ]), 1, 1, 0);

    private static readonly Curve InfiniteOriginOnly = new(new Sequence([
        Point.PlusInfinite(0), new Segment(0, 1, 3, 0),
        new Point(1, 3), new Segment(1, 2, 3, 0)
    ]), 1, 1, 0);

    private static readonly List<(string Name, Curve Curve)> Operands =
    [
        ("zero", Curve.Zero()),
        ("3 from the origin", new Curve(new Sequence([new Point(0, 3), new Segment(0, 1, 3, 0)]), 0, 1, 0)),
        ("-2 from the origin", new Curve(new Sequence([new Point(0, -2), new Segment(0, 1, -2, 0)]), 0, 1, 0)),
        ("ConstantCurve(3)", new ConstantCurve(3)),
        ("ConstantCurve(-2)", new ConstantCurve(-2)),
        ("rate-latency", new RateLatencyServiceCurve(2, 3)),
        ("token bucket", new SigmaRhoArrivalCurve(2, 1)),
        ("t", new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 1)),
        ("t - 1", new Curve(new Sequence([new Point(0, -1), new Segment(0, 1, -1, 1)]), 0, 1, 1)),
        ("1 - t", new Curve(new Sequence([new Point(0, 1), new Segment(0, 1, 1, -1)]), 0, 1, -1)),
        ("-t", new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, -1)),
        ("stair", new StairCurve(2, 3)),
        ("step", new StepCurve(4, 2)),
        ("delay", new DelayServiceCurve(2)),
        ("+inf", Curve.PlusInfinite()),
        ("-inf", Curve.MinusInfinite()),
        ("-1 at the origin only", NegativeOriginOnly),
        ("+inf at the origin only", InfiniteOriginOnly),
        // 1/2 on ]0, 1[, 0 at the integers: its floor is zero
        ("half saw", new Curve(new Sequence([Point.Origin(), new Segment(0, 1, new Rational(1, 2), 0)]), 0, 1, 0)),
        // 2 + 2t on ]0, 1[, 0 on [1, 2[, repeating
        ("saw", new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 2), new Point(1, 0), new Segment(1, 2, 0, 0)]), 0, 2, 0)),
        ("left-continuous jump", new Curve(new Sequence([Point.Origin(), new Segment(0, 2, 0, 0), new Point(2, 0), new Segment(2, 3, 2, 0)]), 2, 1, 0)),
        ("right-continuous jump", new Curve(new Sequence([Point.Origin(), new Segment(0, 2, 0, 0), new Point(2, 2), new Segment(2, 3, 2, 0)]), 2, 1, 0)),
        ("t, then +inf", new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)]), 1, 1, 0)),
        ("-1, then 1", new Curve(new Sequence([new Point(0, -1), new Segment(0, 2, -1, 0), new Point(2, 1), new Segment(2, 3, 1, 0)]), 2, 1, 0)),
    ];

    private static readonly List<(string Name, Curve Curve)> TernaryOperands =
    [
        ("zero", Curve.Zero()),
        ("3 from the origin", new Curve(new Sequence([new Point(0, 3), new Segment(0, 1, 3, 0)]), 0, 1, 0)),
        ("ConstantCurve(3)", new ConstantCurve(3)),
        ("rate-latency", new RateLatencyServiceCurve(2, 3)),
        ("t - 1", new Curve(new Sequence([new Point(0, -1), new Segment(0, 1, -1, 1)]), 0, 1, 1)),
        ("1 - t", new Curve(new Sequence([new Point(0, 1), new Segment(0, 1, 1, -1)]), 0, 1, -1)),
        ("-1 at the origin only", NegativeOriginOnly),
        ("saw", new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 2), new Point(1, 0), new Segment(1, 2, 0, 0)]), 0, 2, 0)),
        ("right-continuous jump", new Curve(new Sequence([Point.Origin(), new Segment(0, 2, 0, 0), new Point(2, 2), new Segment(2, 3, 2, 0)]), 2, 1, 0)),
        ("-1, then 1", new Curve(new Sequence([new Point(0, -1), new Segment(0, 2, -1, 0), new Point(2, 1), new Segment(2, 3, 1, 0)]), 2, 1, 0)),
    ];

    private static IReadOnlyList<CurveExpression> UnaryExpressions(CurveExpression operand)
        =>
        [
            operand.Negate(),
            operand.ToNonNegative(),
            operand.SubAdditiveClosure(),
            operand.SuperAdditiveClosure(),
            operand.ToUpperNonDecreasing(),
            operand.ToLowerNonDecreasing(),
            operand.ToUpperNonIncreasing(),
            operand.ToLowerNonIncreasing(),
            operand.ToLeftContinuous(),
            operand.ToRightContinuous(),
            operand.WithZeroOrigin(),
            operand.LowerPseudoInverse(),
            operand.UpperPseudoInverse(),
            operand.Floor(),
            operand.Ceil(),
            operand.WithOriginAt(2),
            operand.WithOriginAt(-1),
            operand.WithOriginAt(Rational.PlusInfinity),
            operand.DelayBy(2),
            operand.DelayBy(0),
            operand.ForwardBy(1),
            operand.ForwardBy(new Rational(5, 2)),
            operand.HorizontalShift(1),
            operand.HorizontalShift(-1),
            operand.VerticalShift(2),
            operand.VerticalShift(new Rational(-1, 2)),
            operand.VerticalShift(Rational.PlusInfinity),
            operand.VerticalShift(Rational.MinusInfinity),
            operand.Scale(2),
            operand.Scale(new Rational(1, 2)),
            operand.Scale(-2),
            operand.Scale(0),
            operand.Scale(Rational.PlusInfinity),
            operand.Scale(Rational.MinusInfinity),
        ];

    private static IReadOnlyList<CurveExpression> BinaryExpressions(CurveExpression left, CurveExpression right)
        =>
        [
            left.Addition(right),
            left.Subtraction(right),
            left.Minimum(right),
            left.Maximum(right),
            left.Convolution(right),
            left.Deconvolution(right),
            left.MaxPlusConvolution(right),
            left.MaxPlusDeconvolution(right),
            left.Composition(right),
        ];

    // Each predicate receives a fresh curve-expression tree, including fresh descendants.
    // Scalar operands in this corpus are literals.
    private static CurveExpression CopyWithEmptyCaches(CurveExpression expression)
        => expression switch
        {
            ConcreteCurveExpression leaf => new ConcreteCurveExpression(leaf.Value, leaf.Name, leaf.Settings),
            CurveUnaryExpression<Curve> unary => (CurveExpression)unary.WithOperand(
                CopyWithEmptyCaches((CurveExpression)unary.Operand)),
            CurveBinaryExpression<Curve, Curve> binary => (CurveExpression)binary.WithOperands(
                CopyWithEmptyCaches((CurveExpression)binary.LeftOperand),
                CopyWithEmptyCaches((CurveExpression)binary.RightOperand)),
            CurveBinaryExpression<Curve, Rational> binary => (CurveExpression)binary.WithOperands(
                CopyWithEmptyCaches((CurveExpression)binary.LeftOperand), binary.RightOperand),
            CurveNAryExpression nary => (CurveExpression)nary.WithOperands(nary.Operands
                .Select(operand => (IGenericExpression<Curve>)CopyWithEmptyCaches((CurveExpression)operand))
                .ToArray()),
            _ => throw new NotSupportedException($"Unexpected corpus node: {expression.GetType()}")
        };

    private static readonly Lazy<List<(string Label, CurveExpression Expression, Curve Value)>> Cases = new(() =>
    {
        var expressions = new List<CurveExpression>();
        foreach (var (name, curve) in Operands)
            expressions.AddRange(UnaryExpressions(curve.ToExpression(name)));
        foreach (var (leftName, left) in Operands)
            foreach (var (rightName, right) in Operands)
                expressions.AddRange(BinaryExpressions(left.ToExpression(leftName), right.ToExpression(rightName)));

        // Three operands exercise the n-ary visits.
        var some = TernaryOperands;
        foreach (var (a, f) in some)
            foreach (var (b, g) in some)
                foreach (var (c, h) in some)
                {
                    expressions.Add(f.ToExpression(a).Addition(g.ToExpression(b)).Addition(h.ToExpression(c)));
                    expressions.Add(f.ToExpression(a).Minimum(g.ToExpression(b)).Minimum(h.ToExpression(c)));
                    expressions.Add(f.ToExpression(a).Maximum(g.ToExpression(b)).Maximum(h.ToExpression(c)));
                    expressions.Add(f.ToExpression(a).Convolution(g.ToExpression(b)).Convolution(h.ToExpression(c)));
                }

        // Deeper expression trees are inputs to the predicate tests.
        var random = new Random(20261125);
        var zero = Curve.Zero().ToExpression();
        var unaryCount = UnaryExpressions(zero).Count;
        var binaryCount = BinaryExpressions(zero, zero).Count;
        var finite = Operands.Where(operand => operand.Curve.IsFinite || operand.Curve.WithOriginAt(0).IsFinite).ToList();
        for (var i = 0; i < 1000; i++)
            expressions.Add(Tree(3));

        CurveExpression Tree(int depth)
        {
            if (depth == 0 || random.Next(4) == 0)
            {
                var (name, curve) = finite[random.Next(finite.Count)];
                return curve.ToExpression(name);
            }
            if (random.Next(2) == 0)
            {
                var index = random.Next(unaryCount);
                var choices = UnaryExpressions(Tree(depth - 1));
                return choices[index];
            }
            else
            {
                var index = random.Next(binaryCount);
                var choices = BinaryExpressions(Tree(depth - 1), Tree(depth - 1));
                return choices[index];
            }
        }

        var cases = new List<(string Label, CurveExpression Expression, Curve Value)>();
        foreach (var expression in expressions)
        {
            try
            {
                cases.Add((expression.ToUnicodeString(), expression, CopyWithEmptyCaches(expression).Compute()));
            }
            catch (UndeterminedResultException)
            {
                // Opposite infinities and rounding infinite values have no result.
            }
            catch (ArgumentException error) when (error.Message is
                "g must be non-negative" or "g must be non-decreasing" or "g must be finite" or
                "The upper pseudo-inverse is defined only for non-decreasing functions" or
                "The lower pseudo-inverse is defined only for non-decreasing functions" or
                "Cannot have opposite infinites as rightLimitAtStartTime and slope" or
                "The pseudo-period cannot take both +infinity and -infinity." or
                "The pseudo-period height cannot be the infinity opposite to an infinite value in the pseudo-period, as their sum is undefined.")
            {
                // These inputs violate the operation's documented domain.
            }
            catch (InvalidOperationException error) when (error.Message is
                "This method cannot compute the superadditive closure of curves with f(0) > 0. See GeneralSuperAdditiveClosure instead." or
                "This method cannot compute the subadditive closure of curves with f(0) < 0. See GeneralSubAdditiveClosure instead.")
            {
                // The ordinary closures do not support these origin values.
            }
            catch (Exception error)
            {
                throw new Xunit.Sdk.XunitException($"Unexpected computation failure for {expression.ToUnicodeString()}: {error}");
            }
        }
        Assert.NotEmpty(cases);
        Assert.Equal(expressions.Select(expression => expression.GetType()).ToHashSet(),
            cases.Select(testCase => testCase.Expression.GetType()).ToHashSet());
        return cases;
    });

    [Fact]
    public void OriginExceptionsHaveConstantFiniteTails()
    {
        Assert.Equal((Rational)(-1), NegativeOriginOnly.ValueAt(0));
        Assert.Equal((Rational)1, NegativeOriginOnly.ValueAt(1));
        Assert.True(NegativeOriginOnly.ForwardBy(1).IsConstant);
        Assert.True(InfiniteOriginOnly.ValueAt(0).IsPlusInfinite);
        Assert.Equal((Rational)3, InfiniteOriginOnly.ValueAt(1));
        Assert.True(InfiniteOriginOnly.IsUltimatelyFinite);
        Assert.True(InfiniteOriginOnly.ForwardBy(1).IsConstant);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public void ZeroOperandsGiveAComputableCaseForEveryOperation()
    {
        var zero = Curve.Zero().ToExpression();
        foreach (var expression in UnaryExpressions(zero).Concat(BinaryExpressions(zero, zero)))
            Assert.NotNull(expression.Compute());
    }

    [Fact]
    public void IsSubAdditiveAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsSubAdditive.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsSubAdditive.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsSuperAdditiveAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsSuperAdditive.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsSuperAdditive.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsLeftContinuousAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsLeftContinuous.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsLeftContinuous.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsRightContinuousAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsRightContinuous.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsRightContinuous.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsNonNegativeAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsNonNegative.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsNonNegative.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsNonPositiveAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsNonPositive.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsNonPositive.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsPositiveAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsPositive.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsPositive.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsNegativeAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsNegative.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsNegative.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsNonDecreasingAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsNonDecreasing.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsNonDecreasing.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsIncreasingAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsIncreasing.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsIncreasing.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsConcaveAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsConcave.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsConcave.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsConvexAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsConvex.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsConvex.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsPassingThroughOriginAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsPassingThroughOrigin.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsPassingThroughOrigin.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsUltimatelyFiniteAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsUltimatelyFinite.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsUltimatelyFinite.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsPlainAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsPlain.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsPlain.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsUltimatelyPlainAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsUltimatelyPlain.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsUltimatelyPlain.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsUltimatelyAffineAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsUltimatelyAffine.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsUltimatelyAffine.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

    [Fact]
    public void IsUltimatelyConstantAgreesWithTheComputedCurve()
    {
        var disagreements = new List<string>();
        var checkedCases = 0;
        foreach (var (label, expression, value) in Cases.Value)
        {
            string expected;
            try
            {
                expected = value.IsUltimatelyConstant.ToString();
            }
            catch (UndeterminedResultException)
            {
                continue;
            }
            catch (Exception error)
            {
                expected = error.GetType().FullName;
            }
            string actual;
            try
            {
                actual = CopyWithEmptyCaches(expression).IsUltimatelyConstant.ToString();
            }
            catch (Exception error)
            {
                actual = error.GetType().FullName;
            }
            checkedCases++;
            if (actual != expected)
                disagreements.Add($"{label}: curve {expected}, expression {actual}");
        }
        Assert.True(checkedCases > 0);
        Assert.True(disagreements.Count == 0, string.Join("\n", disagreements));
    }

}
