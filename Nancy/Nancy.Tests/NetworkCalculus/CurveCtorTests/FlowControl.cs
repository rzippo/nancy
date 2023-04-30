using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class FlowControl
{
    public static IEnumerable<object[]> GetTestCases()
    {
        var testCases = new (Rational delay, Rational rate, Rational height)[]
        {
            (0, 5, 0),
            (0, 5, 5),
            (5, 10, 0),
            (5, 10, 8),
            (14.5m, new Rational(20, 3), 4)
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] { testCase.delay, testCase.rate, testCase.height };
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void FlowControlCtor(Rational delay, Rational rate, Rational height)
    {
        var curve = new FlowControlCurve(delay, rate, height);

        Assert.True(curve.IsFinite);
        Assert.Equal(height == 0 && delay != 0, curve.IsZero);
        Assert.Equal(height == 0, curve.IsContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.Equal(height == 0 || height >= rate * delay, curve.IsUltimatelyAffine);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(height, curve.RightLimitAt(delay));

        if (delay > 0)
        {
            Assert.Equal(height, curve.LeftLimitAt(delay));
            Assert.Equal(height, curve.ValueAt(delay));
        }

        if (height > 0 && delay > 0 && rate > 0)
        {
            var riseLength = height / rate;

            foreach (var i in new[] {1, 2, 10, 110})
            {
                Assert.Equal(height * (i + 1), curve.ValueAt(i * delay + riseLength));
                Assert.Equal(height * (i + 1), curve.ValueAt((i + 1) * delay));
                Assert.Equal(height * (i + 2), curve.ValueAt((i + 1) * delay + riseLength));

                foreach (var j in new Rational[] {0.2m, 0.33m, 0.6m, 0.8m})
                {
                    Assert.Equal(
                        height * (i + 1),
                        curve.ValueAt(i * delay + riseLength + j * (delay - riseLength))
                    );
                    Assert.Equal(
                        height * (i + 1) + j * height,
                        curve.ValueAt((i + 1) * delay + j * riseLength)
                    );
                }
            }
        }
    }
}