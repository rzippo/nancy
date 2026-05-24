using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class Step
{
    public static List<(Rational value, Rational stepTime)> StepCtorCases =
    [
        (5, 4),
        (3, 7),
        (2, 0),
        (0, 4)
    ];

    public static IEnumerable<object[]> GetStepCtorCases()
        => StepCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetStepCtorCases))]
    public void StepCtor(Rational value, Rational stepTime)
    {
        StepCurve curve = new StepCurve(value: value, stepTime: stepTime);

        Assert.True(curve.IsFinite);
        Assert.Equal(value == 0, curve.IsZero);
        Assert.Equal(value == 0, curve.IsContinuous);
        Assert.Equal(stepTime == 0 || value == 0, curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.Equal(value == 0 ? Rational.PlusInfinity : stepTime, curve.FirstNonZeroTime);
        Assert.Equal(value, curve.Value);
        Assert.Equal(stepTime, curve.StepTime);

        Assert.Equal(0, curve.ValueAt(0));

        if (stepTime > 0 || value == 0)
        {
            Assert.Equal(0, curve.LeftLimitAt(stepTime));
            Assert.Equal(0, curve.ValueAt(stepTime));
        }

        Assert.Equal(value, curve.RightLimitAt(stepTime));

        Assert.Equal(value, curve.RightLimitAt(stepTime));
        Assert.Equal(value, curve.RightLimitAt(stepTime + 2));
        Assert.Equal(value, curve.RightLimitAt(stepTime + new Rational(53, 10)));
        Assert.Equal(value, curve.RightLimitAt(stepTime + 17));
        Assert.Equal(value, curve.RightLimitAt(stepTime + new Rational(1283, 10)));
    }

    public static List<(StepCurve curve, string code, string formattedCode, string mppg)> FormattingCases =
    [
        (
            new StepCurve(5, 4),
            "new StepCurve(5, 4)",
            "\tnew StepCurve(\n\t\t5,\n\t\t4\n\t)",
            "step(4, 5)"
        )
    ];

    public static IEnumerable<object[]> GetFormattingCases()
        => FormattingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetFormattingCases))]
    public void FormattingMethodsReturnStableRepresentations(
        StepCurve curve,
        string code,
        string formattedCode,
        string mppg)
    {
        Assert.Equal(code, curve.ToCodeString());
        Assert.Equal(formattedCode, curve.ToCodeString(formatted: true, indentation: 1));
        Assert.Equal(mppg, curve.ToMppgString());
    }
}
