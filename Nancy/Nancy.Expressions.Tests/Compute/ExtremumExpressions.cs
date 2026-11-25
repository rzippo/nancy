using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// The extremum locations, the extrema over an interval and the extrema of sequences compute what the library computes,
/// and throw where the library finds no attained extremum.
/// </summary>
public class ExtremumExpressions
{
    // f(t) = 3 - t: decreasing, its maximum attained at the origin.
    private static readonly Curve Descending =
        new(new Sequence([new Point(0, 3), new Segment(0, 1, 3, -1)]), 0, 1, -1);

    private static readonly List<(string Name, Curve Curve)> Curves =
    [
        ("rate-latency", new RateLatencyServiceCurve(2, 3)),
        ("token bucket", new SigmaRhoArrivalCurve(2, 1)),
        ("constant after the origin", new ConstantCurve(7)),
        ("descending", Descending),
    ];

    private static readonly Interval Window = new(1, 4, true, false);

    private static readonly Interval SequenceSupport = new(0, 4, true, false);

    public static IEnumerable<object[]> CurveCases()
        => Curves.Select(item => new object[] { item.Name, item.Curve });

    public static IEnumerable<object[]> SequenceCases()
        => Curves.Select(item => new object[] { item.Name, item.Curve.Cut(SequenceSupport) });

    private static void AssertComputesAsTheLibrary(Rational? expected, RationalExpression expression)
    {
        if (expected is { } value)
            Assert.Equal(value, expression.Compute());
        else
            Assert.Throws<InvalidOperationException>(() => expression.Compute());
    }

    [Fact]
    public void DescendingIsAffineWithItsMaximumAtTheOrigin()
    {
        Assert.True(Descending.IsAffine);
        Assert.True(Descending.IsDecreasing);
        Assert.Equal(new Rational(5, 2), Descending.ValueAt(new Rational(1, 2)));
        Assert.Equal((Rational)3, Descending.MaxValue());
        Assert.Equal(Rational.Zero, Descending.MaxArg());
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveSupArgComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.SupArg(), curve.ToExpression().SupArg());
        AssertComputesAsTheLibrary(curve.Negate().SupArg(), curve.ToExpression().Negate().SupArg());
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveInfArgComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.InfArg(), curve.ToExpression().InfArg());
        AssertComputesAsTheLibrary(curve.Negate().InfArg(), curve.ToExpression().Negate().InfArg());
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveMaxArgComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.MaxArg(), curve.ToExpression().MaxArg());
        AssertComputesAsTheLibrary(curve.Negate().MaxArg(), curve.ToExpression().Negate().MaxArg());
    }

    [Fact]
    public void CurveMaxArgReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MaxArg())
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveMinArgComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.MinArg(), curve.ToExpression().MinArg());
        AssertComputesAsTheLibrary(curve.Negate().MinArg(), curve.ToExpression().Negate().MinArg());
    }

    [Fact]
    public void CurveMinArgReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MinArg())
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveSupValueOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.SupValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().SupValueOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().SupValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().SupValueOverInterval(Window));
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveInfValueOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.InfValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().InfValueOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().InfValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().InfValueOverInterval(Window));
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveMaxValueOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.MaxValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().MaxValueOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().MaxValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().MaxValueOverInterval(Window));
    }

    [Fact]
    public void CurveMaxValueOverIntervalReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MaxValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded))
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveMinValueOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.MinValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().MinValueOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().MinValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().MinValueOverInterval(Window));
    }

    [Fact]
    public void CurveMinValueOverIntervalReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MinValueOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded))
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveSupArgOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.SupArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().SupArgOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().SupArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().SupArgOverInterval(Window));
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveInfArgOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.InfArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().InfArgOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().InfArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().InfArgOverInterval(Window));
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveMaxArgOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.MaxArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().MaxArgOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().MaxArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().MaxArgOverInterval(Window));
    }

    [Fact]
    public void CurveMaxArgOverIntervalReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MaxArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded))
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveMinArgOverIntervalComputesAsTheLibrary(string name, Curve curve)
    {
        _ = name;
        AssertComputesAsTheLibrary(curve.MinArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().MinArgOverInterval(Window));
        AssertComputesAsTheLibrary(curve.Negate().MinArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded), curve.ToExpression().Negate().MinArgOverInterval(Window));
    }

    [Fact]
    public void CurveMinArgOverIntervalReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MinArgOverInterval(Window.Lower, Window.Upper, Window.IsLowerIncluded, Window.IsUpperIncluded))
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceSupValueComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.SupValue(), sequence.ToExpression().SupValue());
        AssertComputesAsTheLibrary(sequence.Negate().SupValue(), sequence.ToExpression().Negate().SupValue());
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceInfValueComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.InfValue(), sequence.ToExpression().InfValue());
        AssertComputesAsTheLibrary(sequence.Negate().InfValue(), sequence.ToExpression().Negate().InfValue());
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceMaxValueComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.MaxValue(), sequence.ToExpression().MaxValue());
        AssertComputesAsTheLibrary(sequence.Negate().MaxValue(), sequence.ToExpression().Negate().MaxValue());
    }

    [Fact]
    public void SequenceMaxValueReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve.Cut(SequenceSupport))
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MaxValue())
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceMinValueComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.MinValue(), sequence.ToExpression().MinValue());
        AssertComputesAsTheLibrary(sequence.Negate().MinValue(), sequence.ToExpression().Negate().MinValue());
    }

    [Fact]
    public void SequenceMinValueReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve.Cut(SequenceSupport))
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MinValue())
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceSupArgComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.SupArg(), sequence.ToExpression().SupArg());
        AssertComputesAsTheLibrary(sequence.Negate().SupArg(), sequence.ToExpression().Negate().SupArg());
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceInfArgComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.InfArg(), sequence.ToExpression().InfArg());
        AssertComputesAsTheLibrary(sequence.Negate().InfArg(), sequence.ToExpression().Negate().InfArg());
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceMaxArgComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.MaxArg(), sequence.ToExpression().MaxArg());
        AssertComputesAsTheLibrary(sequence.Negate().MaxArg(), sequence.ToExpression().Negate().MaxArg());
    }

    [Fact]
    public void SequenceMaxArgReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve.Cut(SequenceSupport))
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MaxArg())
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceMinArgComputesAsTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        AssertComputesAsTheLibrary(sequence.MinArg(), sequence.ToExpression().MinArg());
        AssertComputesAsTheLibrary(sequence.Negate().MinArg(), sequence.ToExpression().Negate().MinArg());
    }

    [Fact]
    public void SequenceMinArgReachesAttainedAndUnattainedExtrema()
    {
        var answers = Curves.Select(item => item.Curve.Cut(SequenceSupport))
            .SelectMany(value => new[] { value, value.Negate() })
            .Select(value => value.MinArg())
            .ToArray();
        Assert.Contains(answers, answer => answer is null);
        Assert.Contains(answers, answer => answer is not null);
    }

    [Fact]
    public void TheStaticFactoriesBuildWhatTheInstanceMethodsBuild()
    {
        var curve = new RateLatencyServiceCurve(2, 3);
        var expression = curve.ToExpression("f");
        Assert.Equal(expression.SupArg(), Expressions.SupArg(expression));
        Assert.Equal(expression.SupArg(), Expressions.SupArg(curve, "f"));
        Assert.Equal(expression.MinValueOverInterval(Window), Expressions.MinValueOverInterval(expression, Window));
        Assert.Equal(expression.MinValueOverInterval(Window), Expressions.MinValueOverInterval(curve, Window, "f"));

        var sequence = curve.Cut(SequenceSupport);
        var sequenceExpression = sequence.ToExpression("s");
        Assert.Equal(sequenceExpression.MaxValue(), Expressions.MaxValue(sequenceExpression));
        Assert.Equal(sequenceExpression.MaxValue(), Expressions.MaxValue(sequence, "s"));
    }

    private static IEnumerable<RationalExpression> CurveNodes()
    {
        var expression = new RateLatencyServiceCurve(2, 3).ToExpression("f");
        yield return expression.SupArg();
        yield return expression.InfArg();
        yield return expression.MaxArg();
        yield return expression.MinArg();
        yield return expression.SupValueOverInterval(Window);
        yield return expression.InfValueOverInterval(Window);
        yield return expression.MaxValueOverInterval(Window);
        yield return expression.MinValueOverInterval(Window);
        yield return expression.SupArgOverInterval(Window);
        yield return expression.InfArgOverInterval(Window);
        yield return expression.MaxArgOverInterval(Window);
        yield return expression.MinArgOverInterval(Window);
    }

    private static IEnumerable<RationalExpression> SequenceNodes()
    {
        var expression = new RateLatencyServiceCurve(2, 3).Cut(SequenceSupport).ToExpression("s");
        yield return expression.SupValue();
        yield return expression.InfValue();
        yield return expression.MaxValue();
        yield return expression.MinValue();
        yield return expression.SupArg();
        yield return expression.InfArg();
        yield return expression.MaxArg();
        yield return expression.MinArg();
    }

    public static IEnumerable<object[]> AllNodes()
        => CurveNodes().Concat(SequenceNodes())
            .Select(expression => new object[] { expression });

    public static IEnumerable<object[]> UnsupportedMppgNodes()
        => CurveNodes().Select(expression => new object[] { expression });

    [Theory]
    [MemberData(nameof(AllNodes))]
    public void ANodeRendersAndKeepsItsTypeAndValueThroughARename(RationalExpression expression)
    {
        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToLatexString());

        var renamed = expression.WithName("x");
        Assert.Equal("x", renamed.Name);
        Assert.Equal(expression.GetType(), renamed.GetType());
        Assert.Equal(expression with { Name = "x" }, renamed);
    }

    [Fact]
    public void AnIntervalNodeRendersItsInterval()
    {
        var expression = new RateLatencyServiceCurve(2, 3).ToExpression("f").SupValueOverInterval(Window);

        Assert.Equal($"sup(f, {Window})", expression.ToUnicodeString());
        Assert.Equal($@"\sup\left( f, {Window} \right)", expression.ToLatexString());
    }

    [Theory]
    [MemberData(nameof(UnsupportedMppgNodes))]
    public void MppgRefusesTheCurveNode(RationalExpression expression)
        => Assert.Throws<MppgFormattingException>(() => expression.ToMppgString());

    private static readonly Curve EndpointExtrema = new(new Sequence([
        Point.Origin(), new Segment(0, 1, 0, 0),
        new Point(1, 10), new Segment(1, 2, -5, 10),
        new Point(2, 5), new Segment(2, 3, 5, -10),
        new Point(3, -10), new Segment(3, 4, 0, 0)
    ]), 3, 1, 0);

    private static IEnumerable<(Curve Curve, Interval Interval, Rational SupValue, Rational InfValue,
        Rational SupArg, Rational InfArg, Rational? MaxValue, Rational? MinValue,
        Rational? MaxArg, Rational? MinArg)> EndpointCases()
    {
        foreach (var includeStart in new[] { false, true })
            foreach (var includeEnd in new[] { false, true })
            {
                var interval = new Interval(1, 3, includeStart, includeEnd);
                Rational sup = includeStart ? 10 : 5;
                Rational inf = includeEnd ? -10 : -5;
                Rational supArg = includeStart ? 1 : 2;
                Rational infArg = includeEnd ? 3 : 1;
                Rational? min = includeEnd ? -10 : null;
                Rational? minArg = includeEnd ? 3 : null;
                yield return (EndpointExtrema, interval, sup, inf, supArg, infArg, sup, min, supArg, minArg);
                yield return (EndpointExtrema.Negate(), interval, -inf, -sup, infArg, supArg, -min, -sup, minArg, supArg);
            }
        var singleton = new Interval(2, 2, true, true);
        yield return (EndpointExtrema, singleton, 5, 5, 2, 2, 5, 5, 2, 2);
        yield return (EndpointExtrema.Negate(), singleton, -5, -5, 2, 2, -5, -5, 2, 2);
    }

    public static IEnumerable<object[]> SupValueEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.SupValue });

    [Theory]
    [MemberData(nameof(SupValueEndpointCases))]
    public void SupValueOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.SupValueOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().SupValueOverInterval(interval));
    }

    public static IEnumerable<object[]> InfValueEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.InfValue });

    [Theory]
    [MemberData(nameof(InfValueEndpointCases))]
    public void InfValueOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.InfValueOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().InfValueOverInterval(interval));
    }

    public static IEnumerable<object[]> MaxValueEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.MaxValue });

    [Theory]
    [MemberData(nameof(MaxValueEndpointCases))]
    public void MaxValueOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.MaxValueOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().MaxValueOverInterval(interval));
    }

    public static IEnumerable<object[]> MinValueEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.MinValue });

    [Theory]
    [MemberData(nameof(MinValueEndpointCases))]
    public void MinValueOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.MinValueOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().MinValueOverInterval(interval));
    }

    public static IEnumerable<object[]> SupArgEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.SupArg });

    [Theory]
    [MemberData(nameof(SupArgEndpointCases))]
    public void SupArgOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.SupArgOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().SupArgOverInterval(interval));
    }

    public static IEnumerable<object[]> InfArgEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.InfArg });

    [Theory]
    [MemberData(nameof(InfArgEndpointCases))]
    public void InfArgOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.InfArgOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().InfArgOverInterval(interval));
    }

    public static IEnumerable<object[]> MaxArgEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.MaxArg });

    [Theory]
    [MemberData(nameof(MaxArgEndpointCases))]
    public void MaxArgOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.MaxArgOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().MaxArgOverInterval(interval));
    }

    public static IEnumerable<object[]> MinArgEndpointCases()
        => EndpointCases().Select(item => new object[] { item.Curve, item.Interval, item.MinArg });

    [Theory]
    [MemberData(nameof(MinArgEndpointCases))]
    public void MinArgOverIntervalRespectsEndpointInclusion(Curve curve, Interval interval, Rational? expected)
    {
        Assert.Equal(expected, curve.MinArgOverInterval(interval.Lower, interval.Upper,
            interval.IsLowerIncluded, interval.IsUpperIncluded));
        AssertComputesAsTheLibrary(expected, curve.ToExpression().MinArgOverInterval(interval));
    }

}
