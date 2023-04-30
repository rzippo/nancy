using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.Tests.MinPlusAlgebra.LinqExtentions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceUpperEnvelope
{
    [Fact]
    public void UpperEnvelope1()
    {
        Element[] elements = new Element[]
        {
            new Point(
                time: 0,
                value: 0
            ), 
            new Segment(
                startTime: 0,
                endTime: 90,
                rightLimitAtStartTime: 0,
                slope: 1
            ),
            new Point(
                time: 15,
                value: 45
            ), 
            new Segment(
                startTime: 15,
                endTime: 60,
                rightLimitAtStartTime: 45,
                slope: 0),
            new Point(
                time: 60,
                value: 60
            ), 
            new Segment(
                startTime: 60,
                endTime: 90,
                rightLimitAtStartTime: 60,
                slope: 0),
        };

        Sequence fun = elements.UpperEnvelope().ToSequence();

        //Assert.Equal(3, fun.Count);
        Assert.Equal(1, fun.GetSegmentAfter(0).Slope);
        Assert.Equal(0, fun.GetSegmentAfter(40).Slope);
        Assert.Equal(45, fun.ValueAt(30));
        Assert.Equal(1, fun.GetSegmentAfter(80).Slope);
        Assert.Equal(70, fun.ValueAt(70));
    }

    public static IEnumerable<object[]> DeconvolutionCases()
    {
        var testCases = new (List<Element> elements, List<Element> expected)[]
        {
            (
                elements: JsonConvert.DeserializeObject<List<Element>>("[{\"time\":{\"num\":0,\"den\":1},\"value\":{\"num\":0,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":-2,\"den\":1},\"endTime\":{\"num\":0,\"den\":1},\"slope\":{\"num\":0,\"den\":1},\"rightLimit\":{\"num\":0,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":-2,\"den\":1},\"value\":{\"num\":0,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":-100,\"den\":1},\"endTime\":{\"num\":-2,\"den\":1},\"slope\":{\"num\":2,\"den\":1},\"rightLimit\":{\"num\":-196,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":0,\"den\":1},\"endTime\":{\"num\":20,\"den\":19},\"slope\":{\"num\":7,\"den\":4},\"rightLimit\":{\"num\":2,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":-2,\"den\":1},\"endTime\":{\"num\":-18,\"den\":19},\"slope\":{\"num\":7,\"den\":4},\"rightLimit\":{\"num\":2,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":-18,\"den\":19},\"value\":{\"num\":73,\"den\":19},\"type\":\"point\"},{\"startTime\":{\"num\":-18,\"den\":19},\"endTime\":{\"num\":20,\"den\":19},\"slope\":{\"num\":0,\"den\":1},\"rightLimit\":{\"num\":73,\"den\":19},\"type\":\"segment\"},{\"startTime\":{\"num\":-2,\"den\":1},\"endTime\":{\"num\":-18,\"den\":19},\"slope\":{\"num\":7,\"den\":4},\"rightLimit\":{\"num\":2,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":-100,\"den\":1},\"endTime\":{\"num\":-2,\"den\":1},\"slope\":{\"num\":2,\"den\":1},\"rightLimit\":{\"num\":-194,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":-2,\"den\":1},\"value\":{\"num\":2,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":-2,\"den\":1},\"endTime\":{\"num\":-18,\"den\":19},\"slope\":{\"num\":7,\"den\":4},\"rightLimit\":{\"num\":2,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":20,\"den\":19},\"value\":{\"num\":73,\"den\":19},\"type\":\"point\"},{\"startTime\":{\"num\":-18,\"den\":19},\"endTime\":{\"num\":20,\"den\":19},\"slope\":{\"num\":0,\"den\":1},\"rightLimit\":{\"num\":73,\"den\":19},\"type\":\"segment\"},{\"time\":{\"num\":-18,\"den\":19},\"value\":{\"num\":73,\"den\":19},\"type\":\"point\"},{\"startTime\":{\"num\":-1880,\"den\":19},\"endTime\":{\"num\":-18,\"den\":19},\"slope\":{\"num\":2,\"den\":1},\"rightLimit\":{\"num\":-3651,\"den\":19},\"type\":\"segment\"},{\"startTime\":{\"num\":20,\"den\":19},\"endTime\":{\"num\":5,\"den\":1},\"slope\":{\"num\":4,\"den\":5},\"rightLimit\":{\"num\":73,\"den\":19},\"type\":\"segment\"},{\"startTime\":{\"num\":-18,\"den\":19},\"endTime\":{\"num\":3,\"den\":1},\"slope\":{\"num\":4,\"den\":5},\"rightLimit\":{\"num\":73,\"den\":19},\"type\":\"segment\"},{\"time\":{\"num\":3,\"den\":1},\"value\":{\"num\":7,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":3,\"den\":1},\"endTime\":{\"num\":5,\"den\":1},\"slope\":{\"num\":0,\"den\":1},\"rightLimit\":{\"num\":7,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":-18,\"den\":19},\"endTime\":{\"num\":3,\"den\":1},\"slope\":{\"num\":4,\"den\":5},\"rightLimit\":{\"num\":73,\"den\":19},\"type\":\"segment\"},{\"startTime\":{\"num\":-1880,\"den\":19},\"endTime\":{\"num\":-18,\"den\":19},\"slope\":{\"num\":2,\"den\":1},\"rightLimit\":{\"num\":-3651,\"den\":19},\"type\":\"segment\"},{\"time\":{\"num\":-18,\"den\":19},\"value\":{\"num\":73,\"den\":19},\"type\":\"point\"},{\"startTime\":{\"num\":-18,\"den\":19},\"endTime\":{\"num\":3,\"den\":1},\"slope\":{\"num\":4,\"den\":5},\"rightLimit\":{\"num\":73,\"den\":19},\"type\":\"segment\"},{\"time\":{\"num\":5,\"den\":1},\"value\":{\"num\":7,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":3,\"den\":1},\"endTime\":{\"num\":5,\"den\":1},\"slope\":{\"num\":0,\"den\":1},\"rightLimit\":{\"num\":7,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":3,\"den\":1},\"value\":{\"num\":7,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":-95,\"den\":1},\"endTime\":{\"num\":3,\"den\":1},\"slope\":{\"num\":2,\"den\":1},\"rightLimit\":{\"num\":-189,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":5,\"den\":1},\"endTime\":{\"num\":200,\"den\":1},\"slope\":{\"num\":2,\"den\":5},\"rightLimit\":{\"num\":7,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":3,\"den\":1},\"endTime\":{\"num\":198,\"den\":1},\"slope\":{\"num\":2,\"den\":5},\"rightLimit\":{\"num\":7,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":198,\"den\":1},\"value\":{\"num\":85,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":198,\"den\":1},\"endTime\":{\"num\":200,\"den\":1},\"slope\":{\"num\":0,\"den\":1},\"rightLimit\":{\"num\":85,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":3,\"den\":1},\"endTime\":{\"num\":198,\"den\":1},\"slope\":{\"num\":2,\"den\":5},\"rightLimit\":{\"num\":7,\"den\":1},\"type\":\"segment\"},{\"startTime\":{\"num\":-95,\"den\":1},\"endTime\":{\"num\":3,\"den\":1},\"slope\":{\"num\":2,\"den\":1},\"rightLimit\":{\"num\":-189,\"den\":1},\"type\":\"segment\"},{\"time\":{\"num\":3,\"den\":1},\"value\":{\"num\":7,\"den\":1},\"type\":\"point\"},{\"startTime\":{\"num\":3,\"den\":1},\"endTime\":{\"num\":198,\"den\":1},\"slope\":{\"num\":2,\"den\":5},\"rightLimit\":{\"num\":7,\"den\":1},\"type\":\"segment\"}]",
                    new RationalConverter()
                )!,
                expected: new List<Element>()
                {
                    new Segment(
                        startTime: -100,
                        endTime: -2,
                        rightLimitAtStartTime: -194,
                        slope: 2
                    ),
                    new Point(time: -2, value: 2),
                    new Segment(
                        startTime: -2,
                        endTime: new Rational(-18, 19),
                        rightLimitAtStartTime: 2,
                        slope: new Rational(7, 4)
                    ),
                    new Point(time: new Rational(-18, 19), value: new Rational(73, 19)),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: 3,
                        rightLimitAtStartTime: new Rational(73, 19),
                        slope: new Rational(4, 5)
                    ),
                    new Point(time: 3, value: 7),
                    new Segment(
                        startTime: 3,
                        endTime: 198,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(2, 5)
                    ),
                    new Point(time: 198, value: 85),
                    new Segment(
                        startTime: 198,
                        endTime: 200,
                        rightLimitAtStartTime: 85,
                        slope: 0
                    )
                }
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.elements, testCase.expected };
    }

    [Theory]
    [MemberData(nameof(DeconvolutionCases))]
    public void DeconvolutionTests(List<Element> elements, List<Element> expected)
    {
        var ue = elements.UpperEnvelope();
        Assert.True(Sequence.Equivalent(expected, ue));
    }
}