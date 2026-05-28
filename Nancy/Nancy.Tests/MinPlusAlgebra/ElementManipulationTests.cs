using System;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class ElementManipulationTests
{
    #region Point.Scale

    [Fact]
    public void Point_Scale_Positive()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.Scale(2);
        Assert.Equal(new Point(time: 3, value: 10), result);
    }

    [Fact]
    public void Point_Scale_Negative()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.Scale(-1);
        Assert.Equal(new Point(time: 3, value: -5), result);
    }

    [Fact]
    public void Point_Scale_Zero()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.Scale(Rational.Zero);
        Assert.Equal(new Point(time: 3, value: 0), result);
    }

    #endregion

    #region Segment.Scale

    [Fact]
    public void Segment_Scale_Positive()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Scale(2);
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 20, slope: 6),
            result
        );
    }

    [Fact]
    public void Segment_Scale_Negative()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Scale(-1);
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: -10, slope: -3),
            result
        );
    }

    [Fact]
    public void Segment_Scale_Zero()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Scale(Rational.Zero);
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 0, slope: 0),
            result
        );
    }

    #endregion

    #region Point.Delay

    [Fact]
    public void Point_Delay_Positive()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.Delay(4);
        Assert.Equal(new Point(time: 7, value: 5), result);
    }

    [Fact]
    public void Point_Delay_Zero_ReturnsSame()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Equal(p, p.Delay(0));
    }

    [Fact]
    public void Point_Delay_Negative_Throws()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Throws<ArgumentException>(() => p.Delay(-1));
    }

    [Fact]
    public void Point_Delay_Infinite_Throws()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Throws<ArgumentException>(() => p.Delay(Rational.PlusInfinity));
    }

    #endregion

    #region Segment.Delay

    [Fact]
    public void Segment_Delay_Positive()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Delay(4);
        Assert.Equal(
            new Segment(startTime: 6, endTime: 9, rightLimitAtStartTime: 10, slope: 3),
            result
        );
    }

    [Fact]
    public void Segment_Delay_Zero_ReturnsSame()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.Equal(s, s.Delay(0));
    }

    [Fact]
    public void Segment_Delay_Negative_Throws()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.Throws<ArgumentException>(() => s.Delay(-1));
    }

    #endregion

    #region Point.Forward

    [Fact]
    public void Point_Forward_Positive()
    {
        var p = new Point(time: 7, value: 5);
        var result = (Point)p.Forward(4);
        Assert.Equal(new Point(time: 3, value: 5), result);
    }

    [Fact]
    public void Point_Forward_Zero_ReturnsSame()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Equal(p, p.Forward(0));
    }

    [Fact]
    public void Point_Forward_Negative_Throws()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Throws<ArgumentException>(() => p.Forward(-1));
    }

    #endregion

    #region Segment.Forward

    [Fact]
    public void Segment_Forward_Positive()
    {
        var s = new Segment(startTime: 6, endTime: 9, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Forward(4);
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3),
            result
        );
    }

    [Fact]
    public void Segment_Forward_Zero_ReturnsSame()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.Equal(s, s.Forward(0));
    }

    [Fact]
    public void Segment_Forward_Negative_Throws()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.Throws<ArgumentException>(() => s.Forward(-1));
    }

    #endregion

    #region Point.VerticalShift

    [Fact]
    public void Point_VerticalShift_Positive()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.VerticalShift(7);
        Assert.Equal(new Point(time: 3, value: 12), result);
    }

    [Fact]
    public void Point_VerticalShift_Negative()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.VerticalShift(-5);
        Assert.Equal(new Point(time: 3, value: 0), result);
    }

    [Fact]
    public void Point_VerticalShift_Zero_ReturnsSame()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Equal(p, p.VerticalShift(0));
    }

    #endregion

    #region Segment.VerticalShift

    [Fact]
    public void Segment_VerticalShift_Positive()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.VerticalShift(5);
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 15, slope: 3),
            result
        );
    }

    [Fact]
    public void Segment_VerticalShift_Negative()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.VerticalShift(-3);
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 7, slope: 3),
            result
        );
    }

    [Fact]
    public void Segment_VerticalShift_Zero_ReturnsSame()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.Equal(s, s.VerticalShift(0));
    }

    #endregion

    #region Point.Negate

    [Fact]
    public void Point_Negate_Positive()
    {
        var p = new Point(time: 3, value: 5);
        var result = (Point)p.Negate();
        Assert.Equal(new Point(time: 3, value: -5), result);
    }

    [Fact]
    public void Point_Negate_Zero_ReturnsSame()
    {
        var p = new Point(time: 3, value: 0);
        Assert.Equal(p, p.Negate());
    }

    #endregion

    #region Segment.Negate

    [Fact]
    public void Segment_Negate_Positive()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Negate();
        Assert.Equal(
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: -10, slope: -3),
            result
        );
    }

    [Fact]
    public void Segment_Negate_Zero_ReturnsSame()
    {
        var s = Segment.Zero(2, 5);
        Assert.Equal(s, s.Negate());
    }

    #endregion

    #region Point.Inverse

    [Fact]
    public void Point_Inverse_SwapsTimeAndValue()
    {
        var p = new Point(time: 3, value: 7);
        var result = (Point)p.Inverse();
        Assert.Equal(new Point(time: 7, value: 3), result);
    }

    #endregion

    #region Segment.Inverse

    [Fact]
    public void Segment_Inverse_StrictlyIncreasing()
    {
        // f(t) = 10 + 3*(t-2), defined on (2, 5)
        // f(2+) = 10, f(5-) = 19
        // inverse: start time = 10, end time = 19, rightLimitAtStartTime = 2, slope = 1/3
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        var result = (Segment)s.Inverse();
        Assert.Equal(
            new Segment(startTime: 10, endTime: 19, rightLimitAtStartTime: 2, slope: new Rational(1, 3)),
            result
        );
    }

    [Fact]
    public void Segment_Inverse_Constant_Throws()
    {
        var s = Segment.Constant(2, 5, 7);
        Assert.Throws<InvalidOperationException>(() => s.Inverse());
    }

    #endregion

    #region Point.Floor / Ceil

    [Fact]
    public void Point_Floor_NonInteger_RoundsDown()
    {
        var p = new Point(time: 3, value: new Rational(7, 3));
        var result = p.Floor().Single();
        Assert.Equal(new Point(time: 3, value: 2), result);
    }

    [Fact]
    public void Point_Floor_Integer_ReturnsSame()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Single(p.Floor(), p);
    }

    [Fact]
    public void Point_Ceil_NonInteger_RoundsUp()
    {
        var p = new Point(time: 3, value: new Rational(7, 3));
        var result = p.Ceil().Single();
        Assert.Equal(new Point(time: 3, value: 3), result);
    }

    [Fact]
    public void Point_Ceil_Integer_ReturnsSame()
    {
        var p = new Point(time: 3, value: 5);
        Assert.Single(p.Ceil(), p);
    }

    #endregion
}
