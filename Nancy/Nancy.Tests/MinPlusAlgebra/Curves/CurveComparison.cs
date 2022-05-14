using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveComparison
{
    [Fact]
    public void Equivalence()
    {
        Assert.True(a.Equivalent(b));
        Assert.False(a.Equivalent(c));
        Assert.False(a.Equivalent(d));
        Assert.False(c.Equivalent(d));
    }

    [Fact]
    public void LowerBound()
    {
        Assert.True(a <= b);
        Assert.False(a <= c);
        Assert.True(d <= a);
        Assert.True(d <= b);
    }

    [Fact]
    public void UpperBound()
    {
        Assert.True(a >= b);
        Assert.False(a >= c);
        Assert.True(a >= d);
        Assert.True(b >= d);
    }

    Curve a = new Curve(
        baseSequence: new Sequence(
            elements: new Element[]
            {
                Point.Origin(),
                Segment.Zero(0, 5),
                Point.Zero(5),
                new Segment(
                    startTime: 5,
                    endTime: 10,
                    rightLimitAtStartTime: 0,
                    slope: 2
                )
            }
        ),
        pseudoPeriodLength: 5,
        pseudoPeriodStart: 5,
        pseudoPeriodHeight: 10
    );

    Curve b = new Curve(
        baseSequence: new Sequence(
            elements: new Element[]
            {
                Point.Origin(),
                Segment.Zero(0, 5),
                Point.Zero(5),
                new Segment(
                    startTime: 5,
                    endTime: 10,
                    rightLimitAtStartTime: 0,
                    slope: 2
                ),
                new Point(
                    time: 10,
                    value: 10
                ),
                new Segment(
                    startTime: 10,
                    endTime: 20,
                    rightLimitAtStartTime: 10,
                    slope: 2
                )
            }
        ),
        pseudoPeriodLength: 10,
        pseudoPeriodStart: 10,
        pseudoPeriodHeight: 20
    );

    Curve c = new Curve(
        baseSequence: new Sequence(
            elements: new Element[]
            {
                Point.Origin(),
                Segment.Zero(0, 4),
                Point.Zero(4),
                new Segment(
                    startTime: 4,
                    endTime: 8,
                    rightLimitAtStartTime: 0,
                    slope: 1
                )
            }
        ),
        pseudoPeriodLength: 4,
        pseudoPeriodStart: 4,
        pseudoPeriodHeight: 4
    );

    Curve d = new Curve(
        baseSequence: new Sequence(
            elements: new Element[]
            {
                Point.Origin(),
                Segment.Zero(0, 6),
                Point.Zero(6),
                new Segment(
                    startTime: 6,
                    endTime: 11,
                    rightLimitAtStartTime: 0,
                    slope: 2
                )
            }
        ),
        pseudoPeriodLength: 5,
        pseudoPeriodStart: 6,
        pseudoPeriodHeight: 10
    );
}