using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// The predicates that curve and sequence expressions compute on their value agree with the library's, on a leaf and on an operation.
/// </summary>
public class ValuePredicates
{
    // f(t) = -t: affine and decreasing, zero at the origin.
    private static readonly Curve MinusT =
        new(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, -1);

    // f(t) = 3 from the origin included, where a ConstantCurve is 0.
    private static readonly Curve Three =
        new(new Sequence([new Point(0, 3), new Segment(0, 1, 3, 0)]), 0, 1, 0);

    private static readonly List<(string Name, Curve Curve)> Curves =
    [
        ("zero", new ConstantCurve(0)),
        ("three", Three),
        ("constant after the origin", new ConstantCurve(-2)),
        ("rate-latency", new RateLatencyServiceCurve(2, 3)),
        ("token bucket", new SigmaRhoArrivalCurve(2, 1)),
        ("minus t", MinusT),
    ];

    private static readonly Interval Window = new(0, 4, true, false);

    public static IEnumerable<object[]> CurveCases()
        => Curves.Select(item => new object[] { item.Name, item.Curve });

    public static IEnumerable<object[]> SequenceCases()
        => Curves.Select(item => new object[] { item.Name, item.Curve.Cut(Window) });

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsNonIncreasingAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsNonIncreasing, curve.ToExpression().IsNonIncreasing);
        Assert.Equal(curve.Negate().IsNonIncreasing, curve.ToExpression().Negate().IsNonIncreasing);
    }

    [Fact]
    public void CurveIsNonIncreasingTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsNonIncreasing);
        Assert.Contains(values, value => !value.IsNonIncreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsDecreasingAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsDecreasing, curve.ToExpression().IsDecreasing);
        Assert.Equal(curve.Negate().IsDecreasing, curve.ToExpression().Negate().IsDecreasing);
    }

    [Fact]
    public void CurveIsDecreasingTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsDecreasing);
        Assert.Contains(values, value => !value.IsDecreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsNonPositiveAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsNonPositive, curve.ToExpression().IsNonPositive);
        Assert.Equal(curve.Negate().IsNonPositive, curve.ToExpression().Negate().IsNonPositive);
    }

    [Fact]
    public void CurveIsNonPositiveTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsNonPositive);
        Assert.Contains(values, value => !value.IsNonPositive);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsPositiveAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsPositive, curve.ToExpression().IsPositive);
        Assert.Equal(curve.Negate().IsPositive, curve.ToExpression().Negate().IsPositive);
    }

    [Fact]
    public void CurveIsPositiveTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsPositive);
        Assert.Contains(values, value => !value.IsPositive);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsNegativeAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsNegative, curve.ToExpression().IsNegative);
        Assert.Equal(curve.Negate().IsNegative, curve.ToExpression().Negate().IsNegative);
    }

    [Fact]
    public void CurveIsNegativeTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsNegative);
        Assert.Contains(values, value => !value.IsNegative);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsConstantAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsConstant, curve.ToExpression().IsConstant);
        Assert.Equal(curve.Negate().IsConstant, curve.ToExpression().Negate().IsConstant);
    }

    [Fact]
    public void CurveIsConstantTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsConstant);
        Assert.Contains(values, value => !value.IsConstant);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsAffineAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsAffine, curve.ToExpression().IsAffine);
        Assert.Equal(curve.Negate().IsAffine, curve.ToExpression().Negate().IsAffine);
    }

    [Fact]
    public void CurveIsAffineTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsAffine);
        Assert.Contains(values, value => !value.IsAffine);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CurveIsZeroAgreesWithTheLibrary(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.IsZero, curve.ToExpression().IsZero);
        Assert.Equal(curve.Negate().IsZero, curve.ToExpression().Negate().IsZero);
    }

    [Fact]
    public void CurveIsZeroTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve)
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsZero);
        Assert.Contains(values, value => !value.IsZero);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsNonIncreasingAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsNonIncreasing, sequence.ToExpression().IsNonIncreasing);
        Assert.Equal(sequence.Negate().IsNonIncreasing, sequence.ToExpression().Negate().IsNonIncreasing);
    }

    [Fact]
    public void SequenceIsNonIncreasingTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsNonIncreasing);
        Assert.Contains(values, value => !value.IsNonIncreasing);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsDecreasingAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsDecreasing, sequence.ToExpression().IsDecreasing);
        Assert.Equal(sequence.Negate().IsDecreasing, sequence.ToExpression().Negate().IsDecreasing);
    }

    [Fact]
    public void SequenceIsDecreasingTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsDecreasing);
        Assert.Contains(values, value => !value.IsDecreasing);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsNonPositiveAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsNonPositive, sequence.ToExpression().IsNonPositive);
        Assert.Equal(sequence.Negate().IsNonPositive, sequence.ToExpression().Negate().IsNonPositive);
    }

    [Fact]
    public void SequenceIsNonPositiveTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsNonPositive);
        Assert.Contains(values, value => !value.IsNonPositive);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsPositiveAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsPositive, sequence.ToExpression().IsPositive);
        Assert.Equal(sequence.Negate().IsPositive, sequence.ToExpression().Negate().IsPositive);
    }

    [Fact]
    public void SequenceIsPositiveTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsPositive);
        Assert.Contains(values, value => !value.IsPositive);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsNegativeAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsNegative, sequence.ToExpression().IsNegative);
        Assert.Equal(sequence.Negate().IsNegative, sequence.ToExpression().Negate().IsNegative);
    }

    [Fact]
    public void SequenceIsNegativeTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsNegative);
        Assert.Contains(values, value => !value.IsNegative);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsConstantAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsConstant, sequence.ToExpression().IsConstant);
        Assert.Equal(sequence.Negate().IsConstant, sequence.ToExpression().Negate().IsConstant);
    }

    [Fact]
    public void SequenceIsConstantTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsConstant);
        Assert.Contains(values, value => !value.IsConstant);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsAffineAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsAffine, sequence.ToExpression().IsAffine);
        Assert.Equal(sequence.Negate().IsAffine, sequence.ToExpression().Negate().IsAffine);
    }

    [Fact]
    public void SequenceIsAffineTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsAffine);
        Assert.Contains(values, value => !value.IsAffine);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsZeroAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsZero, sequence.ToExpression().IsZero);
        Assert.Equal(sequence.Negate().IsZero, sequence.ToExpression().Negate().IsZero);
    }

    [Fact]
    public void SequenceIsZeroTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsZero);
        Assert.Contains(values, value => !value.IsZero);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceIsContinuousAgreesWithTheLibrary(string name, Sequence sequence)
    {
        _ = name;
        Assert.Equal(sequence.IsContinuous, sequence.ToExpression().IsContinuous);
        Assert.Equal(sequence.Negate().IsContinuous, sequence.ToExpression().Negate().IsContinuous);
    }

    [Fact]
    public void SequenceIsContinuousTakesBothAnswersOnTheOperands()
    {
        var values = Curves.Select(item => item.Curve.Cut(Window))
            .SelectMany(value => new[] { value, value.Negate() })
            .ToArray();
        Assert.Contains(values, value => value.IsContinuous);
        Assert.Contains(values, value => !value.IsContinuous);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsNonIncreasingAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsNonIncreasing, curve.ToExpression().Cut(Window).IsNonIncreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsDecreasingAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsDecreasing, curve.ToExpression().Cut(Window).IsDecreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsNonPositiveAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsNonPositive, curve.ToExpression().Cut(Window).IsNonPositive);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsPositiveAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsPositive, curve.ToExpression().Cut(Window).IsPositive);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsNegativeAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsNegative, curve.ToExpression().Cut(Window).IsNegative);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsConstantAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsConstant, curve.ToExpression().Cut(Window).IsConstant);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsAffineAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsAffine, curve.ToExpression().Cut(Window).IsAffine);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsZeroAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsZero, curve.ToExpression().Cut(Window).IsZero);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void CutIsContinuousAgreesWithTheSequence(string name, Curve curve)
    {
        _ = name;
        Assert.Equal(curve.Cut(Window).IsContinuous, curve.ToExpression().Cut(Window).IsContinuous);
    }

}
