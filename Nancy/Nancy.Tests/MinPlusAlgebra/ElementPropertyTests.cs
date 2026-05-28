using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class ElementPropertyTests
{
    #region IsDefinedFor

    [Fact]
    public void Point_IsDefinedFor_AtTime_ReturnsTrue()
    {
        var p = new Point(time: 5, value: 3);
        Assert.True(p.IsDefinedFor(5));
    }

    [Fact]
    public void Point_IsDefinedFor_NotAtTime_ReturnsFalse()
    {
        var p = new Point(time: 5, value: 3);
        Assert.False(p.IsDefinedFor(4));
        Assert.False(p.IsDefinedFor(6));
    }

    [Fact]
    public void Segment_IsDefinedFor_Inside()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.True(s.IsDefinedFor(3));
    }

    [Fact]
    public void Segment_IsDefinedFor_AtStartTime_ReturnsFalse()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.False(s.IsDefinedFor(2));
    }

    [Fact]
    public void Segment_IsDefinedFor_AtEndTime_ReturnsFalse()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.False(s.IsDefinedFor(5));
    }

    [Fact]
    public void Segment_IsDefinedFor_Outside()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.False(s.IsDefinedFor(1));
        Assert.False(s.IsDefinedFor(6));
    }

    #endregion

    #region IsInfinite / IsPlusInfinite / IsMinusInfinite / IsZero

    [Fact]
    public void Point_IsInfinite_Finite_ReturnsFalse()
    {
        var p = new Point(time: 3, value: 5);
        Assert.False(p.IsInfinite);
    }

    [Fact]
    public void Point_IsInfinite_PlusInfinite_ReturnsTrue()
    {
        var p = new Point(time: 3, value: Rational.PlusInfinity);
        Assert.True(p.IsInfinite);
        Assert.True(p.IsPlusInfinite);
        Assert.False(p.IsMinusInfinite);
    }

    [Fact]
    public void Point_IsZero_ZeroValue_ReturnsTrue()
    {
        var p = new Point(time: 3, value: 0);
        Assert.True(p.IsZero);
    }

    [Fact]
    public void Point_IsZero_NonZero_ReturnsFalse()
    {
        var p = new Point(time: 3, value: 5);
        Assert.False(p.IsZero);
    }

    [Fact]
    public void Segment_IsInfinite_Finite_ReturnsFalse()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.False(s.IsInfinite);
    }

    [Fact]
    public void Segment_IsPlusInfinite_ReturnsTrue()
    {
        var s = Segment.PlusInfinite(2, 5);
        Assert.True(s.IsInfinite);
        Assert.True(s.IsPlusInfinite);
        Assert.False(s.IsMinusInfinite);
    }

    [Fact]
    public void Segment_IsMinusInfinite_ReturnsTrue()
    {
        var s = Segment.MinusInfinite(2, 5);
        Assert.True(s.IsInfinite);
        Assert.False(s.IsPlusInfinite);
        Assert.True(s.IsMinusInfinite);
    }

    [Fact]
    public void Segment_IsZero_ZeroSegment_ReturnsTrue()
    {
        var s = Segment.Zero(2, 5);
        Assert.True(s.IsZero);
    }

    [Fact]
    public void Segment_IsZero_NonZero_ReturnsFalse()
    {
        var s = new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 10, slope: 3);
        Assert.False(s.IsZero);
    }

    #endregion

    #region Element operators (via Element-typed variable)

    [Fact]
    public void Operator_Multiply_ElementByRational()
    {
        Element e = new Point(time: 3, value: 5);
        var result = e * 2;
        Assert.Equal(new Point(time: 3, value: 10), result);
    }

    [Fact]
    public void Operator_Multiply_RationalByElement()
    {
        Element e = new Point(time: 3, value: 5);
        var result = (Rational)2 * e;
        Assert.Equal(new Point(time: 3, value: 10), result);
    }

    [Fact]
    public void Operator_Divide_ElementByRational()
    {
        Element e = new Point(time: 3, value: 6);
        var result = e / 3;
        Assert.Equal(new Point(time: 3, value: 2), result);
    }

    [Fact]
    public void Operator_Add_ElementPlusRational()
    {
        Element e = new Point(time: 3, value: 5);
        var result = e + (Rational)7;
        Assert.Equal(new Point(time: 3, value: 12), result);
    }

    [Fact]
    public void Operator_Add_RationalPlusElement()
    {
        Element e = new Point(time: 3, value: 5);
        var result = (Rational)7 + e;
        Assert.Equal(new Point(time: 3, value: 12), result);
    }

    [Fact]
    public void Operator_Subtract_ElementMinusRational()
    {
        Element e = new Point(time: 3, value: 5);
        var result = e - (Rational)7;
        Assert.Equal(new Point(time: 3, value: -2), result);
    }

    [Fact]
    public void Operator_Negate_Unary()
    {
        Element e = new Point(time: 3, value: 5);
        var result = -e;
        Assert.Equal(new Point(time: 3, value: -5), result);
    }

    #endregion

    #region Element.Addition / Subtraction (IEnumerable)

    [Fact]
    public void Addition_IEnumerable_SumOfPoints()
    {
        var elements = new Element[]
        {
            new Point(time: 3, value: 5),
            new Point(time: 3, value: 7),
            new Point(time: 3, value: 2)
        };
        var result = Element.Addition(elements);
        Assert.Equal(new Point(time: 3, value: 14), result);
    }

    [Fact]
    public void Addition_IEnumerable_Empty_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => Element.Addition(Array.Empty<Element>())
        );
    }

    [Fact]
    public void Addition_IEnumerable_NonOverlapping_Throws()
    {
        var elements = new Element[]
        {
            new Point(time: 3, value: 5),
            new Point(time: 5, value: 7)
        };
        Assert.Throws<ArgumentException>(
            () => Element.Addition(elements)
        );
    }

    [Fact]
    public void Subtraction_IEnumerable_OfPoints()
    {
        var elements = new Element[]
        {
            new Point(time: 3, value: 10),
            new Point(time: 3, value: 3),
            new Point(time: 3, value: 2)
        };
        var result = Element.Subtraction(elements);
        Assert.Equal(new Point(time: 3, value: 5), result);
    }

    [Fact]
    public void Subtraction_IEnumerable_Empty_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => Element.Subtraction(Array.Empty<Element>())
        );
    }

    #endregion
}
