using System;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceConvolution
{
    [Fact]
    public void Convolution1()
    {
        Sequence function1 = new Sequence(new Element[]
        {
            new Point
            (
                time: 0,
                value: 0
            ), 
            new Segment
            (
                startTime : 0,
                rightLimitAtStartTime : 0,
                slope : 0,
                endTime : 10
            ),
            new Point
            (
                time: 10,
                value: 10
            ), 
            new Segment
            (
                startTime : 10,
                rightLimitAtStartTime : 0,
                slope : 10,
                endTime : 50
            )
        });

        Sequence function2 = new Sequence(new Element[]
        {
            new Point(
                time: 0,
                value: 10
            ), 
            new Segment
            (
                startTime: 0,
                rightLimitAtStartTime: 10,
                slope: 5,
                endTime: 50
            )
        });

        Sequence convolution = function1.Convolution(function2);

        Assert.Equal(0, convolution.DefinedFrom);
        Assert.Equal(100, convolution.DefinedUntil);

        Assert.Equal(10, convolution.GetElementAt(8).ValueAt(8));
        Assert.Equal(0, convolution.GetSegmentAfter(8).Slope);
        Assert.Equal(5, convolution.GetSegmentAfter(30).Slope);
        Assert.Equal(10, convolution.GetSegmentAfter(80).Slope);
    }

    [Fact]
    public void Convolution2()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB;

        Sequence convolution = f1.Convolution(f2);

        Assert.Equal(0, convolution.DefinedFrom);
        Assert.Equal(14, convolution.DefinedUntil);

        Assert.Equal(25, convolution.ValueAt(0));
        Assert.Equal(20, convolution.ValueAt(4));
        Assert.Equal(25, convolution.ValueAt(6));
        Assert.Equal(20, convolution.ValueAt(7));
        Assert.Equal(15, convolution.ValueAt(11));
        Assert.Equal(21, convolution.GetSegmentAfter(12).LeftLimitAtEndTime);
    }

    /// <summary>
    /// Reproduces the steps of specialized staircase convolution through which the issue was discovered
    /// </summary>
    [Fact]
    public void CommutativityByPairs()
    {
        var a = new FlowControlCurve(4, 12, 4);
        var b = new FlowControlCurve(3, 11, 3);
        var min = Curve.Minimum(a, b, new (){UseRepresentationMinimization = false});
        var minCut = min.Cut(0, 31);

        foreach (var ea in minCut.Elements)
        {
            foreach (var eb in minCut.Elements)
            {
                Assert.Equal(
                    ea.Convolution(eb),
                    eb.Convolution(ea)
                );
            }   
        }
    }

    /// <summary>
    /// Reproduces the steps of specialized staircase convolution through which the issue was discovered
    /// </summary>
    [Fact]
    public void CommutativityOfSymmetricalPairs()
    {
        var a = new FlowControlCurve(4, 12, 4);
        var b = new FlowControlCurve(3, 11, 3);
        var min = Curve.Minimum(a, b, new (){UseRepresentationMinimization = false});
        var minCut = min.Cut(0, 31);

        var lowerFirstPairs = minCut.Elements
            .SelectMany(ea => minCut.Elements
                .Where(eb => ea.StartTime < eb.StartTime)
                .Select(eb => (ea: ea, eb: eb))
            );

        var higherFirstPairs = minCut.Elements
            .SelectMany(ea => minCut.Elements
                .Where(eb => ea.StartTime > eb.StartTime)
                .Select(eb => (ea: ea, eb: eb))
            );

        var lowerFirstLowerEnvelope =
                lowerFirstPairs
                    .AsParallel()
                    .SelectMany(tuple => tuple.ea.Convolution(tuple.eb))
                    .ToList()
                    .LowerEnvelope();

        var higherFirstLowerEnvelope =
                higherFirstPairs    
                    .AsParallel()
                    .SelectMany(tuple => tuple.ea.Convolution(tuple.eb))
                    .ToList()
                    .LowerEnvelope();

        Assert.Equal(lowerFirstLowerEnvelope, higherFirstLowerEnvelope);
    }

    /// <summary>
    /// Reproduces the steps of specialized staircase convolution through which the issue was discovered
    /// </summary>
    [Fact]
    public void CommutativityOfSymmetricalPairs_WithColoring()
    {
        var a = new FlowControlCurve(4, 12, 4);
        var b = new FlowControlCurve(3, 11, 3);
        var min = Curve.Minimum(a, b, new (){UseRepresentationMinimization = false});
        var minCut = min.Cut(0, 31);


        var colors = minCut.Elements
            .Select(element =>
                {
                    if(a.Match(element))
                        return Color.A;
                    else if (b.Match(element))
                        return Color.B;
                    else
                        return Color.Both;
                }
            )
            .ToList();

        var lowerFirstPairs = minCut.Elements
            .SelectMany((ea, ia) => minCut.Elements
                .Where((eb, ib) => colors[ia] != colors[ib] || colors[ib] == Color.Both)
                .Where(eb => ea.StartTime < eb.StartTime)
                .Select(eb => (ea: ea, eb: eb))
            );

        var higherFirstPairs = minCut.Elements
            .SelectMany((ea, ia) => minCut.Elements
                .Where((eb, ib) => colors[ia] != colors[ib])
                .Where(eb => ea.StartTime > eb.StartTime)
                .Select(eb => (ea: ea, eb: eb))
            );

        var lowerFirstLowerEnvelope =
                lowerFirstPairs
                    .AsParallel()
                    .SelectMany(tuple => tuple.ea.Convolution(tuple.eb))
                    .ToList()
                    .LowerEnvelope();

        var higherFirstLowerEnvelope =
                higherFirstPairs    
                    .AsParallel()
                    .SelectMany(tuple => tuple.ea.Convolution(tuple.eb))
                    .ToList()
                    .LowerEnvelope();

        Assert.Equal(lowerFirstLowerEnvelope, higherFirstLowerEnvelope);
    }

    private enum Color { A, B, Both }
}