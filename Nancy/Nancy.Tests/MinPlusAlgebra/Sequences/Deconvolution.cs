using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Deconvolution
{
    [Fact]
    public void SimpleDeconvolution()
    {
        var a = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 5),
            Point.Zero(5),
            Segment.Zero(5, 10)
        });

        var b = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 3)
        });

        var result = Sequence.Deconvolution(a, b);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Elements);
    }

    [Fact]
    public void InstanceMethod_EquivalenceToStatic()
    {
        var a = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 5)
        });

        var b = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 3)
        });

        var staticResult = Sequence.Deconvolution(a, b);
        var instanceResult = a.Deconvolution(b);

        Assert.Equal(staticResult, instanceResult);
    }

    [Fact]
    public void InstanceMethod_WithCutParameters()
    {
        var a = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 10)
        });

        var b = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 5)
        });

        var result = a.Deconvolution(b, cutStart: Rational.Zero, cutEnd: new Rational(5));

        Assert.NotNull(result);
        Assert.True(result.DefinedFrom <= 0);
    }

    [Fact]
    public void MaxPlusDeconvolution_Static()
    {
        var a = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 5)
        });

        var b = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 3)
        });

        var result = Sequence.MaxPlusDeconvolution(a, b);

        Assert.NotNull(result);
    }

    [Fact]
    public void MaxPlusDeconvolution_Instance()
    {
        var a = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 5)
        });

        var b = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 3)
        });

        var result = a.MaxPlusDeconvolution(b);

        Assert.NotNull(result);
    }

    [Fact]
    public void MaxPlusDeconvolution_Equivalence()
    {
        var a = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 5)
        });

        var b = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Zero(0, 3)
        });

        var staticResult = Sequence.MaxPlusDeconvolution(a, b);
        var instanceResult = a.MaxPlusDeconvolution(b);

        Assert.Equal(staticResult, instanceResult);
    }
}