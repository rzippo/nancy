using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class LowerPseudoInverse
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LowerPseudoInverse(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Sequence operand, Sequence expected)> ContinuousRevertibleKnownCases =
    [
        (
            operand: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 3),
                Point.Zero(3),
                new Segment(3, 5, 0, 2)
            ]),
            expected: new Sequence([
                Point.Origin(),
                new Segment(0, 4, 3, new Rational(1, 2))
            ])
        ),
        (
            operand: new Sequence([
                new Segment(0, 3, 1, 2),
                new Point(3, 7),
                new Segment(3, 5, 7, 3)
            ]),
            expected: new Sequence([
                new Segment(1, 7, 0, new Rational(1, 2)),
                new Point(7, 3),
                new Segment(7, 13, 3, new Rational(1, 3))
            ])
        ),
        (
            operand: new Sequence([
                new Segment(1, 2, 2, 3)
            ]),
            expected: new Sequence([
                new Segment(2, 5, 1, new Rational(1, 3))
            ])
        ),
        (
            operand: new Sequence([
                new Segment(1, 2, 2, 3),
                new Point(2, 5)
            ]),
            expected: new Sequence([
                new Segment(2, 5, 1, new Rational(1, 3)),
                new Point(5, 2)
            ])
        ),
    ];

    public static IEnumerable<object[]> ContinuousRevertibleTestCases()
        => ContinuousRevertibleKnownCases.ToXUnitTestCases();

    /// <summary>
    /// Examples which are continuous yet not revertible due to plateaus at the endpoints
    /// </summary>
    public static List<(Sequence operand, Sequence expected1, Sequence expected2)> ContinuousNonRevertibleKnownCases =
    [
        (
            operand: new Sequence([
                new Segment(1, 2, 1, 1),
                new Point(2, 2),
                Segment.Constant(2, 3, 2)
            ]),
            expected1: new Sequence([
                new Segment(1, 2, 1, 1),
                new Point(2, 2)
            ]),
            expected2: new Sequence([
                new Segment(1, 2, 1, 1),
                new Point(2, 2)
            ])
        ),
        (
            operand: new Sequence([
                Segment.Constant(1, 2, 2),
                new Point(2, 2),
                new Segment(2, 4, 4, 1),
                new Point(4, 6),
                Segment.Constant(4, 6, 6)
            ]),
            expected1: new Sequence([
                new Point(2, 1),
                Segment.Constant(2, 4, 2),
                new Point(4, 2),
                new Segment(4, 6, 2, 1),
                new Point(6, 4)
            ]),
            expected2: new Sequence([
                new Point(1, 2),
                Segment.Constant(1, 2, 2),
                new Point(2, 2),
                new Segment(2, 4, 4, 1),
                new Point(4, 6),
            ])
        ),
    ];
    
    public static IEnumerable<object[]> ContinuousNonRevertibleTestCases()
        => ContinuousNonRevertibleKnownCases.ToXUnitTestCases();
    
    /// <summary>
    /// Examples which are discontinuous yet revertible, as they are left-continuous
    /// </summary>
    public static List<(Sequence operand, Sequence expected)> DiscontinuousRevertibleKnownCases = 
    [
        (
            new Sequence( new List<Element>
            {
                Point.Origin(),
                new Segment(0, 1, 0, 1),
                new Point(1, 1),
                new Segment(1, 2, 2, 1)
            }),
            new Sequence( new List<Element>
            {
                Point.Origin(),
                new Segment(0, 1, 0, 1),
                new Point(1, 1),
                new Segment(1, 2, 1, 0),
                new Point(2, 1),
                new Segment(2, 3, 1, 1)
            })
        ),
        (
            operand: new Sequence([
                new Segment(0, 3, 1, 2),
                new Point(3, 7),
                new Segment(3, 5, 8, 2)
            ]),
            expected: new Sequence([
                new Segment(1, 7, 0, new Rational(1, 2)),
                new Point(7, 3),
                new Segment(7, 8, 3, 0),
                new Point(8, 3),
                new Segment(8, 12, 3, new Rational(1, 2))
            ])
        )
    ];
    
    public static IEnumerable<object[]> DiscontinuousRevertibleTestCases()
        => DiscontinuousRevertibleKnownCases.ToXUnitTestCases();

    public static List<(Sequence operand, Sequence expected1, Sequence expected2)>
        DiscontinuousNonRevertibleKnownCases =
        [
            (
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 2),
                    new Segment(1, 2, 2, 1)
                }),
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 1, 0),
                    new Point(2, 1),
                    new Segment(2, 3, 1, 1)
                }),
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 2, 1)
                })
            ),
            (
                operand: new Sequence([
                    Segment.Zero(0, 3),
                    Point.Zero(3),
                    new Segment(3, 5, 0, 2)
                ]),
                expected1: new Sequence([
                    Point.Origin(),
                    new Segment(0, 4, 3, new Rational(1, 2))
                ]),
                expected2: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 3),
                    Point.Zero(3),
                    new Segment(3, 5, 0, 2)
                ])
            ),
        ];

    public static IEnumerable<object[]> DiscontinuousNonRevertibleTestCases()
        => DiscontinuousNonRevertibleKnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ContinuousRevertibleTestCases))]
    [MemberData(nameof(DiscontinuousRevertibleTestCases))]
    public void RevertibleInverseTest(Sequence operand, Sequence expected)
    {
        _testOutputHelper.WriteLine(operand.ToCodeString());
        _testOutputHelper.WriteLine(expected.ToCodeString());

        var result = operand.LowerPseudoInverse();
        _testOutputHelper.WriteLine(result.ToCodeString());
        Assert.True(Sequence.Equivalent(expected, result));

        var result2 = result.LowerPseudoInverse();
        _testOutputHelper.WriteLine(result2.ToCodeString());
        Assert.True(Sequence.Equivalent(operand, result2));
    }

    [Theory]
    [MemberData(nameof(ContinuousNonRevertibleTestCases))]
    [MemberData(nameof(DiscontinuousNonRevertibleTestCases))]
    public void NonRevertibleInverseTest(Sequence operand, Sequence expected1, Sequence expected2)
    {
        _testOutputHelper.WriteLine(operand.ToCodeString());
        _testOutputHelper.WriteLine(expected1.ToCodeString());
        _testOutputHelper.WriteLine(expected2.ToCodeString());

        var result = operand.LowerPseudoInverse();
        _testOutputHelper.WriteLine(result.ToCodeString());
        Assert.True(Sequence.Equivalent(expected1, result));

        var result2 = result.LowerPseudoInverse();
        _testOutputHelper.WriteLine(result2.ToCodeString());
        Assert.True(Sequence.Equivalent(expected2, result2));
    }

    public static IEnumerable<object[]> PseudoInverseIntervalsTestCases()
    {
        var sequencesFromRevertible = ContinuousRevertibleKnownCases
            .Concat(DiscontinuousRevertibleKnownCases)
            .SelectMany(pair => new[] { pair.operand, pair.expected });
        var sequencesFromNonRevertible = ContinuousNonRevertibleKnownCases
            .Concat(DiscontinuousNonRevertibleKnownCases)
            .SelectMany(tuple => new []{ tuple.operand, tuple.expected1, tuple.expected2 });

        var sequences = sequencesFromRevertible.Concat(sequencesFromNonRevertible);
        return sequences.ToXUnitTestCases();
    }
    
    [Theory]
    [MemberData(nameof(PseudoInverseIntervalsTestCases))]
    public void PseudoInverseIntervals(Sequence sequence)
    {
        var lpi = sequence.LowerPseudoInverse();
        var expectedLpiSupport = sequence.Image;
        var expectedLpiImage = sequence.EndsWithPlateau
            ? sequence.Support
                .WithUpper(sequence.LastPlateauStart)
                .WithIsUpperIncluded(true)
            : sequence.Support;
        if (sequence.StartsWithPlateau)
            expectedLpiImage = expectedLpiImage
                .WithIsLowerIncluded(true);
        Assert.Equal(expectedLpiSupport, lpi.Support);
        Assert.Equal(expectedLpiImage, lpi.Image);
    }
}