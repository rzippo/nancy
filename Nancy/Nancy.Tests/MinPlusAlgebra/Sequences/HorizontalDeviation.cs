using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Segments;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class HorizontalDeviation
{
    private readonly ITestOutputHelper _testOutputHelper;

    public HorizontalDeviation(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Sequence a, Sequence b, Rational hdev)> KnownHDevs =
    [
        //same domain and codomain
        (
            new Sequence([
                    Point.Origin(),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 0,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 2),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 4)
                ]
            ),
            new Sequence([
                    Point.Origin(),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 2),
                        endTime: 4
                    ),
                    new Point(4, 2),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 2,
                        slope: 2,
                        endTime: 5
                    ),
                    new Point(5, 4)
                ]
            ),
            new Rational(3)
        ),
        //same codomain. disjointed domains with Xa < Xb
        (
            new Sequence([
                    Point.Origin(),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 0,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 2),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 4)
                ]
            ),
            new Sequence([
                    new Point(6, 0),
                    new Segment
                    (
                        6,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(2, 3),
                        endTime: 12
                    ),
                    new Point(12, 4)
                ]
            ),
            new Rational(8)
        ),
        //same codomain. disjointed domains with Xa > Xb
        (
            new Sequence([
                    new Point(6, 0),
                    new Segment
                    (
                        6,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(2, 3),
                        endTime: 12
                    ),
                    new Point(12, 4)
                ]
            ),
            new Sequence([
                    Point.Origin(),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 0,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 2),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 4)
                ]
            ),
            new Rational(0)
        ),
        //same codomain. mixed domains with Xa < Xb
        (
            new Sequence([
                    Point.Origin(),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 0,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 2),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 4)
                ]
            ),
            new Sequence([
                    new Point(3, 0),
                    new Segment
                    (
                        3,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 1),
                    new Segment
                    (
                        5,
                        rightLimitAtStartTime: 1,
                        slope: new Rational(1),
                        endTime: 8
                    ),
                    new Point(8, 4)
                ]
            ),
            new Rational(5)
        ),
        //different codomain. mixed domains with Xa < Xb
        (
            new Sequence([
                    new Point(0, 1),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 1,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 3),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 5)
                ]
            ),
            new Sequence([
                    new Point(3, 0),
                    new Segment
                    (
                        3,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 1),
                    new Segment
                    (
                        5,
                        rightLimitAtStartTime: 1,
                        slope: new Rational(1),
                        endTime: 8
                    ),
                    new Point(8, 4)
                ]
            ),
            new Rational(6)
        ),
        //different codomain. disjointed domains with Xa < Xb
        (
            new Sequence([
                    new Point(0, 1),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 1,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 3),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 5)
                ]
            ),
            new Sequence([
                    new Point(8, 0),
                    new Segment
                    (
                        8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 2),
                        endTime: 10
                    ),
                    new Point(10, 1),
                    new Segment
                    (
                        10,
                        rightLimitAtStartTime: 1,
                        slope: new Rational(1),
                        endTime: 13
                    ),
                    new Point(13, 4)
                ]
            ),
            new Rational(11)
        ),
        //different codomain. disjointed domains with Xa < Xb
        (
            new Sequence([
                    new Point(0, 1),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 1,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 3),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 5)
                ]
            ),
            new Sequence([
                    new Point(7, 3),
                    new Segment
                    (
                        7,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 9
                    ),
                    new Point(9, 4),
                    new Segment
                    (
                        9,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(4),
                        endTime: 10
                    ),
                    new Point(10, 8)
                ]
            ),
            new Rational(6)
        ),
        //different codomain. disjointed domains with Xa > Xb
        (
            new Sequence([
                    new Point(7, 3),
                    new Segment
                    (
                        7,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 9
                    ),
                    new Point(9, 4),
                    new Segment
                    (
                        9,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(4),
                        endTime: 10
                    ),
                    new Point(10, 8)
                ]
            ),
            new Sequence([
                    new Point(0, 1),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 1,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 3),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 5)
                ]
            ),
            new Rational(0)
        ),
        //different codomain. disjointed domains with Xa < Xb
        (
            new Sequence([
                    new Point(0, 1),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 1,
                        slope: 2,
                        endTime: 1
                    ),
                    new Point(1, 3),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 2),
                        endTime: 5
                    ),
                    new Point(5, 5)
                ]
            ),
            new Sequence([
                    new Point(2, 2),
                    new Segment
                    (
                        2,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1),
                        endTime: 4
                    ),
                    new Point(4, 4),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(3),
                        endTime: 5
                    ),
                    new Point(5, 7)
                ]
            ),
            new Rational(2)
        ),
        //broken sequence Xa < Xb
        (
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: 3,
                        endTime: 1
                    ),
                    new Point(1, 5),
                    new Segment
                    (
                        1,
                        rightLimitAtStartTime: 5,
                        slope: new Rational(1, 3),
                        endTime: 4
                    ),
                    new Point(4, 6)
                ]
            ),
            new Sequence([
                    new Point(8, 0),
                    new Segment
                    (
                        8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(11, 7),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(1),
                        endTime: 12
                    ),
                    new Point(12, 8)
                ]
            ),
            new Rational(11)
        ),
        //broken sequences Xa < Xb
        (
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 4),
                        endTime: 4
                    ),
                    new Point(4, 4),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(1, 2),
                        endTime: 6
                    ),
                    new Point(6, 5)
                ]
            ),
            new Sequence([
                    new Point(8, 0),
                    new Segment
                    (
                        8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(11, 7),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(1),
                        endTime: 12
                    ),
                    new Point(12, 8)
                ]
            ),
            new Rational(11)
        ),
        //broken Test Xa < Xb
        (
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 4
                    ),
                    new Point(4, 8),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 8,
                        slope: new Rational(1, 2),
                        endTime: 6
                    ),
                    new Point(6, 9)
                ]
            ),
            new Sequence([
                    new Point(8, 0),
                    new Segment
                    (
                        8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(11, 5),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 5,
                        slope: new Rational(1, 2),
                        endTime: 13
                    ),
                    new Point(13, 6)
                ]
            ),
            new Rational(11)
        ),
        //super broken Test Xa < Xb
        (
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 4
                    ),
                    new Point(4, 8),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 8,
                        slope: new Rational(1, 2),
                        endTime: 6
                    ),
                    new Point(6, 9)
                ]
            ),
            new Sequence([
                    new Point(11, 5),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 5,
                        slope: new Rational(1, 2),
                        endTime: 13
                    ),
                    new Point(13, 6)
                ]
            ),
            new Rational(9)
        ),
        //broken Test Xa > Xb
        (
            new Sequence([
                    new Point(8, 0),
                    new Segment
                    (
                        8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(11, 5),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 5,
                        slope: new Rational(1, 2),
                        endTime: 13
                    ),
                    new Point(13, 6)
                ]
            ),
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 4
                    ),
                    new Point(4, 8),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 8,
                        slope: new Rational(1, 2),
                        endTime: 6
                    ),
                    new Point(6, 9)
                ]
            ),
            new Rational(0)
        ),
        //super broken Test Xa > Xb
        (
            new Sequence([
                    new Point(11, 5),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 5,
                        slope: new Rational(1, 2),
                        endTime: 13
                    ),
                    new Point(13, 6)
                ]
            ),
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 2),
                        endTime: 4
                    ),
                    new Point(4, 8),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 8,
                        slope: new Rational(1, 2),
                        endTime: 6
                    ),
                    new Point(6, 9)
                ]
            ),
            new Rational(0)
        ),
        //broken sequences Xa < Xb
        (
            new Sequence([
                    new Point(8, 0),
                    new Segment
                    (
                        8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(11, 7),
                    new Segment
                    (
                        11,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(1),
                        endTime: 12
                    ),
                    new Point(12, 8)
                ]
            ),
            new Sequence([
                    new Point(0, 2),
                    new Segment
                    (
                        0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 4),
                        endTime: 4
                    ),
                    new Point(4, 4),
                    new Segment
                    (
                        4,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(1, 2),
                        endTime: 6
                    ),
                    new Point(6, 5)
                ]
            ),
            new Rational(0)
        ),
        // The three windows of the plateau example, as sequences and nothing more.
        // b starts at 2 in all three; they differ in whether the plateau at 2 is inside the window.
        (
            new Sequence([ new Point(1,1), new Segment(1,2,1,1), new Point(2,2), new Segment(2,3,2,0), new Point(3,2), new Segment(3,4,2,1), new Point(4,3) ]),
            new Sequence([ new Point(6,2), new Segment(6,10,2,0), new Point(10,2), new Segment(10,12,2,2), new Point(12,6) ]),
            new Rational(7)
        ),
        (
            new Sequence([ new Point(1,1), new Segment(1,2,1,1), new Point(2,2), new Segment(2,3,2,0), new Point(3,2), new Segment(3,4,2,1), new Point(4,3) ]),
            new Sequence([ new Point(8,2), new Segment(8,10,2,0), new Point(10,2), new Segment(10,12,2,2), new Point(12,6) ]),
            new Rational(7)
        ),
        (
            new Sequence([ new Point(1,1), new Segment(1,2,1,1), new Point(2,2), new Segment(2,3,2,0), new Point(3,2), new Segment(3,4,2,1), new Point(4,3) ]),
            new Sequence([ new Point(10,2), new Segment(10,12,2,2), new Point(12,6) ]),
            new Rational(8)
        ),
    ];

    public static IEnumerable<object[]> HorizontalDeviationValuesTestCases()
    {
        return KnownHDevs.ToXUnitTestCases();
    }

    [Theory]
    [MemberData(nameof(HorizontalDeviationValuesTestCases))]
    public void HorizontalDeviationValues(Sequence a, Sequence b, Rational expected)
    {
        var hdev = Sequence.HorizontalDeviation(a, b);
        Assert.Equal(expected, hdev);
    }
    
    /// <summary>
    /// Known combinations of curves and cuts, such that $hdev(f, g) = hdev(Cut(f, kF), Cut(g, kG))$. 
    /// </summary>
    public static List<(Curve f, Curve g, Rational kF, Rational kG, Rational hdev)> KnownHDevValidCuts =
    [
        //sigmaRho and RateLatency
        (
            new SigmaRhoArrivalCurve(7, 3),
            new RateLatencyServiceCurve(5, 10),
            new Rational(7, 2),
            new Rational(149, 10),
            new Rational(57, 5)
        ),
        //UA, UA
        (
            new Curve(baseSequence: new Sequence([new Point(0, 0), new Segment(0, 1, 22, 8), new Point(1, 30), new Segment(1, 2, 30, 8)]), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 8),
            new RateLatencyServiceCurve(10, 29),
            new Rational(11),
            new Rational(211, 5),
            new Rational(156, 5)
        ),
        //UPP, UA - 1
        (
            new Curve(baseSequence: new Sequence([
                new Point(0, 0), new Segment(0, 1, 22, 5), new Point(1, 27), new Segment(1, 3, 27, 5), new Point(3, 37),
                new Segment(3, 4, 44, 5)
            ]), pseudoPeriodStart: 1, pseudoPeriodLength: 3, pseudoPeriodHeight: 22),
            new RateLatencyServiceCurve(10, 29),
            new Rational(33, 4),
            new Rational(789, 20),
            new Rational(156, 5)
        ),
        //UPP, UA - 2
        (
            new Curve(baseSequence: new Sequence([
                new Point(0, 0), new Segment(0, 3, 33, 0), new Point(3, 33), new Segment(3, 5, 40, 0), new Point(5, 40),
                new Segment(5, 6, 47, 0), new Point(6, 47), new Segment(6, 9, 54, 0), new Point(9, 54),
                new Segment(9, 10, 61, 0), new Point(10, 61), new Segment(10, 12, 68, 0), new Point(12, 68),
                new Segment(12, 15, 75, 0), new Point(15, 75), new Segment(15, 18, 89, 0), new Point(18, 89),
                new Segment(18, 20, 96, 0), new Point(20, 96), new Segment(20, 21, 103, 0), new Point(21, 103),
                new Segment(21, 24, 110, 0), new Point(24, 110), new Segment(24, 25, 117, 0), new Point(25, 117),
                new Segment(25, 27, 124, 0), new Point(27, 124), new Segment(27, 30, 131, 0)
            ]), pseudoPeriodStart: 15, pseudoPeriodLength: 15, pseudoPeriodHeight: 56),
            new RateLatencyServiceCurve(10, 29),
            new Rational(495, 94),
            new Rational(8828, 94),
            new Rational(323, 10)
        ),
        //UA, UPP - 1
        (
            new Curve(baseSequence: new Sequence([new Point(0, 0), new Segment(0, 1, 26, 5), new Point(1, 31), new Segment(1, 2, 31, 5)]), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 5),
            new Curve(baseSequence: new Sequence([
                new Point(0, 0), new Segment(0, 19, 0, 0), new Point(19, 0), new Segment(19, 20, 8, 0),
                new Point(20, 8), new Segment(20, 21, 12, 0)
            ]), pseudoPeriodStart: 19, pseudoPeriodLength: 2, pseudoPeriodHeight: 12),
            new Rational(34),
            new Rational(172, 3),
            new Rational(23)
        ),
        //UA, UPP - 2
        (
            new Curve(baseSequence: new Sequence([new Point(0, 0), new Segment(0, 1, 26, 3), new Point(1, 29), new Segment(1, 2, 29, 3)]), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 3),
            new Curve(baseSequence: new Sequence([
                new Point(0, 0), new Segment(0, 24, 0, 0), new Point(24, 0), new Segment(24, 25, 10, 0),
                new Point(25, 10), new Segment(25, 26, 13, 0)
            ]), pseudoPeriodStart: 24, pseudoPeriodLength: 2, pseudoPeriodHeight: 13),
            new Rational(72, 7),
            new Rational(268, 7),
            new Rational(28)
        ),
        //UPP, UPP - 1
        (
            new Curve(baseSequence: new Sequence([
                    new Point(0, 0), new Segment(0, 1, 20, 0), new Point(1, 20), new Segment(1, 2, 22, 0), new Point(2, 22),
                    new Segment(2, 3, 27, 0), new Point(3, 27), new Segment(3, 4, 29, 0)
                ]), 
                2, 
                2, 
                7
            ),
            new Curve(baseSequence: new Sequence([
                new Point(0, 0), new Segment(0, 24, 0, 0), new Point(24, 0), new Segment(24, 25, 10, 0),
                new Point(25, 10), new Segment(25, 26, 13, 0)
            ]), pseudoPeriodStart: 24, pseudoPeriodLength: 2, pseudoPeriodHeight: 13),
            new Rational(10),
            new Rational(482, 13),
            new Rational(26)
        ),
        //UPP, UPP - 2
        (
            new Curve(baseSequence: new Sequence([
                new Point(0, 0), new Segment(0, 2, 12, 0), new Point(2, 12), new Segment(2, 3, 15, 0), new Point(3, 15),
                new Segment(3, 4, 19, 0), new Point(4, 19), new Segment(4, 6, 22, 0), new Point(6, 22),
                new Segment(6, 8, 29, 0), new Point(8, 29), new Segment(8, 9, 32, 0), new Point(9, 32),
                new Segment(9, 10, 36, 0), new Point(10, 36), new Segment(10, 12, 39, 0)
            ]), pseudoPeriodStart: 6, pseudoPeriodLength: 6, pseudoPeriodHeight: 17),
            new Curve(baseSequence: new Sequence([new Point(0, 0), new Segment(0, 14, 0, 0), new Point(14, 0), new Segment(14, 17, 12, 0)]), pseudoPeriodStart: 14, pseudoPeriodLength: 3, pseudoPeriodHeight: 12),
            new Rational(144, 7),
            new Rational(263, 7),
            new Rational(15)
        ),
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 6),
                    new Segment(2, 3, 6, 1)
                ]), 
                pseudoPeriodStart: 2, 
                pseudoPeriodLength: 1, 
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            4,
            8,
            6
        ),
        (
            new SigmaRhoArrivalCurve(0, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 5),
                    new Point(5, 0),
                    new Segment(5, 6, 0, 2),
                    new Point(6, 2),
                    Segment.Constant(6, 10, 2),
                    new Point(10, 2),
                    new Segment(10, 11, 2, 2)
                ]),
                10,
                1,
                2
            ),
            4,
            12,
            8
        )
    ];

    /// <summary>
    /// Known combinations of curves and cuts, such that $hdev(f, g) != hdev(Cut(f, kF), Cut(g, kG))$.
    /// </summary>
    /// <remarks>
    /// In both, f jumps at 2 from 2 to 6, across the top of the shared image $[0, 5]$, which g reaches at 7:
    /// the deviation between the cuts is measured from the jump, and is 5, against 6 between the curves.
    /// </remarks>
    public static List<(Curve f, Curve g, Rational kF, Rational kG, Rational hdev)> KnownHDevInvalidCuts =
    [
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 6),
                    new Segment(2, 3, 6, 1)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            4,
            7,
            5
        ),
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 4, 6, 1)
                ]), 
                pseudoPeriodStart: 3, 
                pseudoPeriodLength: 1, 
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            4,
            7,
            5
        )
    ];

    public static IEnumerable<object[]> KnownHDevValidCutsTestCases
        => KnownHDevValidCuts.ToXUnitTestCases();
    
    public static IEnumerable<object[]> KnownHDevInvalidCutsTestCases
        => KnownHDevInvalidCuts.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownHDevValidCutsTestCases))]
    [MemberData(nameof(KnownHDevInvalidCutsTestCases))]
    public void HorizontalDeviationFunctionCuts(Curve f, Curve g, Rational kF, Rational kG, Rational expected)
    {
        _testOutputHelper.WriteLine($"var f = {f.ToCodeString()};");
        _testOutputHelper.WriteLine($"var g = {g.ToCodeString()};");
        _testOutputHelper.WriteLine($"var kF = {kF.ToCodeString()};");
        _testOutputHelper.WriteLine($"var kG = {kG.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        var fCut = f.Cut(0, kF, true, true);
        var gCut = g.Cut(0, kG, true, true);
        var hdevFunctionSequence = Sequence.HorizontalDeviationFunction(fCut, gCut);
        _testOutputHelper.WriteLine($"var hdevFunctionSequence = {hdevFunctionSequence.ToCodeString()};");
        AssertMatchesTheCurvesPointwise(f, g, fCut, gCut, hdevFunctionSequence);
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 4.4, the pointwise equivalence, for cuts where its hypothesis holds, as prefixes always do:
    /// at each measured time, the deviation between the cuts is the curves' delay from the top, within the shared image, of the range of values the first operand covers around that time.
    /// Where the first operand is continuous, that is the deviation between the curves at that time;
    /// where it jumps, the curves' deviation reads its value alone, and the two may differ.
    /// </summary>
    private static void AssertMatchesTheCurvesPointwise(Curve f, Curve g, Sequence fCut, Sequence gCut, Sequence hdevFunctionSequence)
    {
        var imageOverlap = Interval.Intersection(fCut.Image, gCut.Image)!.Value;
        var hdevFunctionCurves = Curve.HorizontalDeviationFunction(f, g);
        var gLpi = g.LowerPseudoInverse();

        var times = SampleTimes(hdevFunctionSequence)
            .Concat(SampleTimes(fCut).Where(hdevFunctionSequence.IsDefinedAt))
            .Distinct();
        foreach (var t in times)
        {
            var left = t == fCut.DefinedFrom ? fCut.ValueAt(t) : fCut.LeftLimitAt(t);
            var right = t == fCut.DefinedUntil ? fCut.ValueAt(t) : fCut.RightLimitAt(t);
            Rational expected;
            if (left == right)
                expected = hdevFunctionCurves.ValueAt(t);
            else
            {
                var top = Rational.Min(right, imageOverlap.Upper);
                var reached = top == imageOverlap.Upper && !imageOverlap.IsUpperIncluded
                    ? gLpi.LeftLimitAt(top)
                    : gLpi.ValueAt(top);
                expected = Rational.Max(0, reached - t);
            }
            Assert.True(expected == hdevFunctionSequence.ValueAt(t), $"{hdevFunctionSequence.ValueAt(t)} at {t}, not {expected}");
        }
    }
    
    [Theory]
    [MemberData(nameof(KnownHDevValidCutsTestCases))]
    public void HorizontalDeviationValidCuts(Curve f, Curve g, Rational kF, Rational kG, Rational expected)
    {
        _testOutputHelper.WriteLine($"var f = {f.ToCodeString()};");
        _testOutputHelper.WriteLine($"var g = {g.ToCodeString()};");
        _testOutputHelper.WriteLine($"var kF = {kF.ToCodeString()};");
        _testOutputHelper.WriteLine($"var kG = {kG.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        var hdevCurves = Curve.HorizontalDeviation(f, g);
        var fCut = f.Cut(0, kF, true, true);
        var gCut = g.Cut(0, kG, true, true);
        var hdevSeq = Sequence.HorizontalDeviation(fCut, gCut);
        _testOutputHelper.WriteLine($"var hdevCurves = {hdevCurves.ToCodeString()};");
        _testOutputHelper.WriteLine($"var hdevSeq = {hdevSeq.ToCodeString()};");
        Assert.Equal(expected, hdevSeq);
        Assert.Equal(expected, hdevCurves);
    }
    
    [Theory]
    [MemberData(nameof(KnownHDevInvalidCutsTestCases))]
    public void HorizontalDeviationInvalidCuts(Curve f, Curve g, Rational kF, Rational kG, Rational expected)
    {
        _testOutputHelper.WriteLine($"var f = {f.ToCodeString()};");
        _testOutputHelper.WriteLine($"var g = {g.ToCodeString()};");
        _testOutputHelper.WriteLine($"var kF = {kF.ToCodeString()};");
        _testOutputHelper.WriteLine($"var kG = {kG.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        var hdevCurves = Curve.HorizontalDeviation(f, g);
        var fCut = f.Cut(0, kF, true, true);
        var gCut = g.Cut(0, kG, true, true);
        var hdevSeq = Sequence.HorizontalDeviation(fCut, gCut);
        _testOutputHelper.WriteLine($"var hdevFunctionCurves = {hdevCurves.ToCodeString()};");
        _testOutputHelper.WriteLine($"var hdevFunctionSequence = {hdevSeq.ToCodeString()};");
        Assert.Equal(expected, hdevSeq);
        Assert.NotEqual(expected, hdevCurves);
    }

    /// <summary>
    /// Curves and cuts for which the hypothesis of the equivalence, [TBP-SEQ-DEV] § 4.4, holds:
    /// the curve of g does not reach the lowest height of the overlap before the cut of g does.
    /// The last three cut at open ends, the last at an open end on a plateau of g at that height.
    /// </summary>
    public static List<(Curve f, Curve g, Interval xF, Interval xG)> HDevFunctionCutsWithHypothesis =
    [
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 6),
                    new Segment(2, 3, 6, 1)
                ]), 
                pseudoPeriodStart: 2, 
                pseudoPeriodLength: 1, 
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            new Interval(1, 4),
            new Interval(4, 9)
        ),
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 6),
                    new Segment(2, 3, 6, 1)
                ]), 
                pseudoPeriodStart: 2, 
                pseudoPeriodLength: 1, 
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            new Interval(1, 4),
            new Interval(1, 9)
        ),
        (
            new SigmaRhoArrivalCurve(0, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 5),
                    new Point(5, 0),
                    new Segment(5, 6, 0, 2),
                    new Point(6, 2),
                    Segment.Constant(6, 10, 2),
                    new Point(10, 2),
                    new Segment(10, 11, 2, 2)
                ]),
                10,
                1,
                2
            ),
            new Interval(1, 4),
            new Interval(6, 12)
        ),
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 6),
                    new Segment(2, 3, 6, 1)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            new Interval(1, 4, false, true),
            new Interval(4, 9, true, false)
        ),
        (
            new Curve(baseSequence:new Sequence([
                    new Point(0, 0),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 6),
                    new Segment(2, 3, 6, 1)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(1, 2),
            new Interval(1, 4, true, false),
            new Interval(1, 9, false, true)
        ),
        (
            new SigmaRhoArrivalCurve(0, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 5),
                    new Point(5, 0),
                    new Segment(5, 6, 0, 2),
                    new Point(6, 2),
                    Segment.Constant(6, 10, 2),
                    new Point(10, 2),
                    new Segment(10, 11, 2, 2)
                ]),
                10,
                1,
                2
            ),
            new Interval(1, 4, false, false),
            new Interval(6, 12, false, true)
        ),
    ];

    /// <summary>
    /// Curves and cuts for which the curve of g reaches the lowest height of the overlap before the cut of g does, so the hypothesis of the equivalence, [TBP-SEQ-DEV] § 4.4, does not hold.
    /// The two cases differ only in how far into the same plateau the cut opens.
    /// </summary>
    public static List<(Curve f, Curve g, Interval xF, Interval xG)> HDevFunctionCutsWithoutHypothesis =
    [
        (
            new SigmaRhoArrivalCurve(0, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 5),
                    new Point(5, 0),
                    new Segment(5, 6, 0, 2),
                    new Point(6, 2),
                    Segment.Constant(6, 10, 2),
                    new Point(10, 2),
                    new Segment(10, 11, 2, 2)
                ]),
                10,
                1,
                2
            ),
            new Interval(1, 4),
            new Interval(8, 12)
        ),
        (
            new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    Segment.Constant(2, 3, 2),
                    new Point(3, 2),
                    new Segment(3, 4, 2, 1)
                ]),
                3,
                1,
                1
            ),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 5),
                    new Point(5, 0),
                    new Segment(5, 6, 0, 2),
                    new Point(6, 2),
                    Segment.Constant(6, 10, 2),
                    new Point(10, 2),
                    new Segment(10, 11, 2, 2)
                ]),
                10,
                1,
                2
            ),
            new Interval(1, 4),
            new Interval(10, 12)
        ),
    ];

    public static IEnumerable<object[]> HDevFunctionCutsWithHypothesisTestCases
        => HDevFunctionCutsWithHypothesis.ToXUnitTestCases();

    public static IEnumerable<object[]> HDevFunctionCutsWithoutHypothesisTestCases
        => HDevFunctionCutsWithoutHypothesis.ToXUnitTestCases();

    /// <summary>
    /// The negation of the hypothesis of [TBP-SEQ-DEV] § 4.4 on the second operand:
    /// the lowest height of the overlap is shared, and the curve reaches it before the cut does.
    /// </summary>
    private static bool HasReachedTheLowestSharedHeightBeforeTheCut(Curve g, Sequence fCut, Sequence gCut)
    {
        var imageOverlap = Interval.Intersection(fCut.Image, gCut.Image)!.Value;
        if (!imageOverlap.IsLowerIncluded)
            return false;

        var lowest = imageOverlap.Lower;
        return g.LowerPseudoInverseAt(lowest) < gCut.LowerPseudoInverseAt(lowest);
    }

    [Theory]
    [MemberData(nameof(HDevFunctionCutsWithHypothesisTestCases))]
    public void HorizontalDeviationFunctionOfCutsMatchesTheCurvesUnderTheHypothesis(Curve f, Curve g, Interval xF, Interval xG)
    {
        _testOutputHelper.WriteLine($"var f = {f.ToCodeString()};");
        _testOutputHelper.WriteLine($"var g = {g.ToCodeString()};");
        _testOutputHelper.WriteLine($"var xF = {xF.ToCodeString()};");
        _testOutputHelper.WriteLine($"var xG = {xG.ToCodeString()};");
        var fCut = f.Cut(xF);
        var gCut = g.Cut(xG);
        var hdevFunctionSequence = Sequence.HorizontalDeviationFunction(fCut, gCut);
        _testOutputHelper.WriteLine($"var hdevFunctionSequence = {hdevFunctionSequence.ToCodeString()};");

        Assert.False(HasReachedTheLowestSharedHeightBeforeTheCut(g, fCut, gCut));
        AssertMatchesTheCurvesPointwise(f, g, fCut, gCut, hdevFunctionSequence);
    }

    [Theory]
    [MemberData(nameof(HDevFunctionCutsWithoutHypothesisTestCases))]
    public void HorizontalDeviationFunctionOfCutsDivergesFromTheCurvesWithoutTheHypothesis(Curve f, Curve g, Interval xF, Interval xG)
    {
        _testOutputHelper.WriteLine($"var f = {f.ToCodeString()};");
        _testOutputHelper.WriteLine($"var g = {g.ToCodeString()};");
        _testOutputHelper.WriteLine($"var xF = {xF.ToCodeString()};");
        _testOutputHelper.WriteLine($"var xG = {xG.ToCodeString()};");
        var hdevFunctionCurves = Curve.HorizontalDeviationFunction(f, g);
        var fCut = f.Cut(xF);
        var gCut = g.Cut(xG);
        var hdevFunctionSequence = Sequence.HorizontalDeviationFunction(fCut, gCut);
        _testOutputHelper.WriteLine($"var hdevFunctionCurves = {hdevFunctionCurves.ToCodeString()};");
        _testOutputHelper.WriteLine($"var hdevFunctionSequence = {hdevFunctionSequence.ToCodeString()};");

        Assert.True(HasReachedTheLowestSharedHeightBeforeTheCut(g, fCut, gCut));
        Assert.False(hdevFunctionCurves.Match(hdevFunctionSequence));
    }

    /// <summary>
    /// HT-fails-sup, [TBP-SEQ-DEV] § 6: without the hypothesis of § 4.4, the deviation between the cuts exceeds the one between the curves.
    /// The curve of g reaches the lowest shared height 2 at 1, its cut only at 5, while f is at that height from 0.
    /// </summary>
    [Fact]
    public void HorizontalDeviationOfCutsExceedsTheCurvesWithoutTheHypothesis()
    {
        var f = new Curve(
            new Sequence([ new Point(0, 2), new Segment(0, 10, 2, 0), new Point(10, 2), new Segment(10, 11, 2, 1) ]),
            10, 1, 1);
        var g = new Curve(
            new Sequence([
                new Point(0, 0), new Segment(0, 1, 0, 2), new Point(1, 2), new Segment(1, 5, 2, 0),
                new Point(5, 2), new Segment(5, 6, 2, 100)
            ]),
            5, 1, 100);
        var fCut = f.Cut(0, 12, true, true);
        var gCut = g.Cut(5, 6, true, true);

        Assert.True(HasReachedTheLowestSharedHeightBeforeTheCut(g, fCut, gCut));
        Assert.Equal(5, Sequence.HorizontalDeviation(fCut, gCut));
        Assert.Equal(1, Curve.HorizontalDeviation(f, g));
    }

    private static Interval I(Rational lower, Rational upper, bool isLowerIncluded, bool isUpperIncluded)
        => new(lower, upper, isLowerIncluded, isUpperIncluded);

    /// <summary>
    /// The sequence $a$ is $c$ on $[0, 1[$, then $v$ at 1, then the given segment on $]1, 2[$ and its end value at 2:
    /// a jump at 1, whose value there, the spot $v$, is any value within the jump.
    /// </summary>
    private static Sequence JumpAtOne(Rational c, Rational v, Rational afterValue, Rational afterSlope)
        => new([
            new Point(0, c), new Segment(0, 1, c, 0), new Point(1, v),
            new Segment(1, 2, afterValue, afterSlope), new Point(2, afterValue + afterSlope)
        ]);

    /// <summary>
    /// Pairs of sequences named by the cases of [TBP-SEQ-DEV] § 6, with the measured times of $a$ that the result is defined over,
    /// its value at some of them, and its supremum.
    /// The values are computed by hand from [TBP-SEQ-DEV] § 4, reading each jump of $a$ as covering the range between its two sides.
    /// Where a case has a jump in $a$, it is repeated with the spot moved within the jump, and the expected result does not change.
    /// </summary>
    public static IEnumerable<object[]> HDevCases()
    {
        object[] Case(string id, Sequence a, Sequence b, Interval support, (Rational t, Rational v)[] values, Rational sup)
            => [id, a, b, support, values, sup];
        var half = new Rational(1, 2);

        // HY-touch, HX-point-bound: Y = {1}, reached by a at the closed end of its domain
        yield return Case("HY-touch",
            new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1) ]),
            new Sequence([ new Point(2, 1), new Segment(2, 3, 1, 1), new Point(3, 2) ]),
            I(1, 1, true, true), [(1, 1)], 1);

        // HY-overlap: Y = [2, 3], and b reaches each height 2 after a does
        var tOn03 = new Sequence([ new Point(0, 0), new Segment(0, 3, 0, 1), new Point(3, 3) ]);
        var tMinus2On47 = new Sequence([ new Point(4, 2), new Segment(4, 7, 2, 1), new Point(7, 5) ]);
        yield return Case("HY-overlap", tOn03, tMinus2On47,
            I(2, 3, true, true), [(2, 2), (new Rational(5, 2), 2), (3, 2)], 2);
        // ... with a right-open, so that u_y = 3 is not attained: Y = [2, 3[
        yield return Case("HY-overlap, u_y excluded",
            new Sequence([ new Point(0, 0), new Segment(0, 3, 0, 1) ]), tMinus2On47,
            I(2, 3, true, false), [(2, 2), (new Rational(5, 2), 2)], 2);
        // ... with b left-open, so that l_y = 2 is not attained: Y = ]2, 3], and a is at 2 at a time that is not measured
        yield return Case("HY-overlap, l_y excluded", tOn03,
            new Sequence([ new Segment(4, 7, 2, 1), new Point(7, 5) ]),
            I(2, 3, false, true), [(new Rational(5, 2), 2), (3, 2)], 2);

        // HY-nested: Y = Y_b = [2, 3]
        yield return Case("HY-nested",
            new Sequence([ new Point(0, 0), new Segment(0, 5, 0, 1), new Point(5, 5) ]),
            new Sequence([ new Point(3, 2), new Segment(3, 4, 2, 1), new Point(4, 3) ]),
            I(2, 3, true, true), [(2, 1), (3, 1)], 1);

        // HY-shared: Y = [0, 3], each end included where both operands attain it
        var tMinus1On14 = new Sequence([ new Point(1, 0), new Segment(1, 4, 0, 1), new Point(4, 3) ]);
        yield return Case("HY-shared", tOn03, tMinus1On14,
            I(0, 3, true, true), [(0, 1), (3, 1)], 1);
        // HX-dom-open: a is left-open, and the measured times with it
        yield return Case("HY-shared, HX-dom-open",
            new Sequence([ new Segment(0, 3, 0, 1), new Point(3, 3) ]), tMinus1On14,
            I(0, 3, false, true), [(half, 1), (3, 1)], 1);
        // HX-bound-open: b is left-open, so a at 0 is at a height that is not shared
        yield return Case("HY-shared, HX-bound-open", tOn03,
            new Sequence([ new Segment(1, 4, 0, 1), new Point(4, 3) ]),
            I(0, 3, false, true), [(half, 1), (3, 1)], 1);

        // HX-plateau: a is at l_y = 1 on ]0, 1], so the deviation tends to 1 at the open end, without attaining it
        yield return Case("HX-plateau",
            new Sequence([ new Segment(0, 1, 1, 0), new Point(1, 1), new Segment(1, 3, 1, 1), new Point(3, 3) ]),
            new Sequence([ new Point(1, 1), new Segment(1, 4, 1, 1), new Point(4, 4) ]),
            I(0, 3, false, true), [(half, half), (1, 0), (2, 0)], 1);

        // HX-jump-across, at the right end of the measured times (P1): a jumps from 0 to 3 at 1, across u_y = 2.
        // The jump covers [0, 2] of Y, and b reaches 2 at 4.
        var tHalfOn04 = new Sequence([ new Point(0, 0), new Segment(0, 4, 0, half), new Point(4, 2) ]);
        foreach (var spot in new Rational[] { 0, 1, 2, 3 })
            yield return Case($"HX-jump-across, right end, spot {spot}", JumpAtOne(0, spot, 3, 0), tHalfOn04,
                I(0, 1, true, true), [(half, 0), (1, 3)], 3);

        // HX-jump-across, at the left end of the measured times: a jumps from 0 to 2 at 1, across l_y = 1.
        // The jump covers [1, 2] of Y, and b reaches 2 at 3.
        var tMinus1On25 = new Sequence([ new Point(2, 1), new Segment(2, 5, 1, 1), new Point(5, 4) ]);
        foreach (var spot in new Rational[] { 0, half, 1, 2 })
            yield return Case($"HX-jump-across, left end, spot {spot}", JumpAtOne(0, spot, 2, 1), tMinus1On25,
                I(1, 2, true, true), [(1, 2), (new Rational(3, 2), 2), (2, 2)], 2);

        // HX-jump-excluded-bound: a jumps from 2 to 5 at 2, and u_y = 2 is excluded, so the jump covers no shared height
        var tHalfOn04Open = new Sequence([ new Point(0, 0), new Segment(0, 4, 0, half) ]);
        foreach (var spot in new Rational[] { 2, 5 })
            yield return Case($"HX-jump-excluded-bound, spot {spot}",
                new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, spot), new Segment(2, 3, 5, 0), new Point(3, 5) ]),
                tHalfOn04Open,
                I(0, 2, true, false), [(1, 1), (new Rational(3, 2), new Rational(3, 2))], 2);

        // HX-jump-interior: a jumps from 1 to 2 at 1, both inside Y = [0, 3]; the deviation at 1 is measured from the top of the jump
        var tOn03Again = new Sequence([ new Point(0, 0), new Segment(0, 3, 0, 1), new Point(3, 3) ]);
        foreach (var spot in new Rational[] { 1, new Rational(3, 2), 2 })
            yield return Case($"HX-jump-interior, spot {spot}",
                new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, spot), new Segment(1, 2, 2, 1), new Point(2, 3) ]),
                tOn03Again,
                I(0, 2, true, true), [(half, 0), (1, 1), (new Rational(3, 2), 1)], 1);

        // HX-point-inside (P3): a jumps from 0 to 3 at 1, over the whole of Y = [1/2, 2], which b reaches the top of at 2
        var bP3 = new Sequence([
            new Point(0, half), new Segment(0, half, half, 0), new Point(half, half), new Segment(half, 2, half, 1), new Point(2, 2)
        ]);
        foreach (var spot in new Rational[] { 0, 1, 3 })
            yield return Case($"HX-point-inside, spot {spot}", JumpAtOne(0, spot, 3, 0), bP3,
                I(1, 1, true, true), [(1, 1)], 1);

        // HX-point-outside (H3): a jumps from 0 to 5 at 1, over the whole of Y = [1, 3], which b reaches the top of at 2
        var bH3 = new Sequence([ new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3) ]);
        foreach (var spot in new Rational[] { 0, 2, 5 })
            yield return Case($"HX-point-outside, spot {spot}", JumpAtOne(0, spot, 5, 0), bH3,
                I(1, 1, true, true), [(1, 1)], 1);

        // HB-start-reach: b reaches l_y = 1 by a jump at 2, to 2, so every height in [1, 2] is first reached at 2
        yield return Case("HB-start-reach",
            new Sequence([ new Point(1, 1), new Segment(1, 3, 1, 1), new Point(3, 3) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 0), new Point(2, 2), new Segment(2, 4, 2, 1), new Point(4, 4) ]),
            I(1, 3, true, true), [(1, 1), (new Rational(3, 2), half), (2, 0), (new Rational(5, 2), 0)], 1);

        // HB-end-dom-open: b approaches u_y = 2 at its open end 2 without attaining it, and a jumps across it at 1
        yield return Case("HB-end-dom-open",
            new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 3, 0), new Point(2, 3) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1) ]),
            I(0, 1, true, true), [(half, 0), (1, 1)], 1);

        // HB-end-jump (H1): b jumps above u_y = 2 right after 1, so every height in ]1, 2] is first reached at 1
        yield return Case("HB-end-jump",
            new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 3, 0), new Point(2, 3) ]),
            I(0, 2, true, true), [(half, 0), (1, 0), (new Rational(3, 2), 0)], 0);

        // HB-over (H1'): b jumps from 0 to 5 right after 1, over the whole of Y = [1, 3], so every shared height is first reached at 1
        var bOver = new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 0), new Point(1, 0), new Segment(1, 2, 5, 0), new Point(2, 5) ]);
        yield return Case("HB-over",
            new Sequence([ new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3) ]), bOver,
            I(0, 2, true, true), [(0, 1), (half, half), (new Rational(3, 2), 0)], 1);
        // ... with a left-open (P4): Y = ]1, 3] is the whole of a's image, and the measured times are a's domain
        yield return Case("HB-over, HX-dom-open",
            new Sequence([ new Segment(0, 2, 1, 1), new Point(2, 3) ]), bOver,
            I(0, 2, false, true), [(half, half), (1, 0)], 1);

        // HT-holds-plateau-below: prefixes, where b stays on a plateau below the lowest shared height, 0 before 1, so b(ŝ_b) = 0 is attained before ŝ_b = 1 but l_y = 1 is not
        yield return Case("HT-holds-plateau-below",
            new Sequence([ new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 0), new Point(1, 0), new Segment(1, 3, 1, 1), new Point(3, 3) ]),
            I(0, 2, true, true), [(0, 1), (1, 1), (2, 1)], 1);

        // Infinite values, [TBP-SEQ-DEV] § 5: w is the rate 1 and h_1, h_2 the delays 1 and 2, each cut to [0, 4];
        // the shared heights are the finite ones.
        var w = new RateLatencyServiceCurve(1, 0).Cut(0, 4, true, true);
        var h1 = new DelayServiceCurve(1).Cut(0, 4, true, true);
        var h2 = new DelayServiceCurve(2).Cut(0, 4, true, true);

        // HI-b-infinite: Y = [0, 4], and h_2 first reaches every height above 0 at 2
        yield return Case("HI-b-infinite", w, h2,
            I(0, 4, true, true), [(0, 0), (1, 1), (3, 0)], 2);

        // HI-a-infinite: Y = [0, 4], the finite heights of [0, +inf]; a covers all of them at 1, and none after it
        yield return Case("HI-a-infinite", h1, w,
            I(0, 1, true, true), [(half, 0), (1, 3)], 3);

        // HI-both-infinite: Y = [0, +inf[, unbounded; every height above 0 is reached at 1 by a and at 2 by b
        yield return Case("HI-both-infinite", h1, h2,
            I(0, 1, true, true), [(half, 0), (1, 1)], 1);
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 4: the result is defined over the measured times of $a$, with their ends, and takes the values the definition gives.
    /// </summary>
    [Theory]
    [MemberData(nameof(HDevCases))]
    public void HorizontalDeviationFunctionFollowsTheDefinition(string id, Sequence a, Sequence b, Interval support, (Rational t, Rational v)[] values, Rational sup)
    {
        var hdev = Sequence.HorizontalDeviationFunction(a, b);
        _testOutputHelper.WriteLine($"{id}: {hdev.ToCodeString()}");

        Assert.True(hdev.Support.SetEquals(support), $"{id}: defined over {hdev.Support}, not {support}");
        foreach (var (t, v) in values)
            Assert.True(hdev.ValueAt(t) == v, $"{id}: {hdev.ValueAt(t)} at {t}, not {v}");
        Assert.True(Sequence.HorizontalDeviation(a, b) == sup, $"{id}: supremum {Sequence.HorizontalDeviation(a, b)}, not {sup}");
    }

    /// <summary>
    /// Pairs of sequences whose images share no value, named by the cases of [TBP-SEQ-DEV] § 6.
    /// </summary>
    public static IEnumerable<object[]> HDevRejectedCases()
    {
        var tOn01 = new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1) ]);
        yield return ["HY-gap", tOn01, new Sequence([ new Point(0, 2), new Segment(0, 1, 2, 1), new Point(1, 3) ])];
        yield return ["HY-touch, a open",
            new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1) ]),
            new Sequence([ new Point(2, 1), new Segment(2, 3, 1, 1), new Point(3, 2) ])];
        yield return ["HY-touch, b open", tOn01, new Sequence([ new Segment(2, 3, 1, 1), new Point(3, 2) ])];
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 4: where the images share no value, the deviation is undefined, and the methods throw.
    /// </summary>
    [Theory]
    [MemberData(nameof(HDevRejectedCases))]
    public void DisjointImagesAreRejected(string id, Sequence a, Sequence b)
    {
        _testOutputHelper.WriteLine(id);
        Assert.Throws<ArgumentException>(() => Sequence.HorizontalDeviationFunction(a, b));
        Assert.Throws<ArgumentException>(() => Sequence.HorizontalDeviation(a, b));
    }

    /// <summary>
    /// Non-negative, non-decreasing sequences over assorted domains and shapes, used by the property tests below.
    /// </summary>
    public static List<Sequence> PropertySequences =
    [
        new Sequence([ Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6) ]),
        new Sequence([ Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12) ]),
        new Sequence([
            Point.Origin(), new Segment(0, 2, 0, 2), new Point(2, 4), new Segment(2, 4, 4, 0),
            new Point(4, 4), new Segment(4, 6, 4, 1), new Point(6, 6) ]),
        new Sequence([
            Point.Origin(), new Segment(0, 3, 0, 1), new Point(3, 3), new Segment(3, 6, 5, 1),
            new Point(6, 8) ]),
        new Sequence([ new Point(2, 1), new Segment(2, 6, 1, 1), new Point(6, 5) ]),
        new Sequence([ new Segment(1, 5, 2, 1), new Point(5, 6) ]),
    ];

    public static IEnumerable<object[]> PropertySequenceTestCases => PropertySequences.ToXUnitTestCases();

    public static IEnumerable<object[]> PropertySequencePairs =>
        PropertySequences.SelectMany(a => PropertySequences.Select(b => new object[] { a, b }));

    private static IEnumerable<Rational> SampleTimes(Sequence s)
    {
        var times = new List<Rational>();
        for (var i = 0; i <= 20; i++)
            times.Add(s.DefinedFrom + (s.DefinedUntil - s.DefinedFrom) * new Rational(i, 20));
        times.AddRange(s.EnumerateBreakpoints().Select(bp => bp.center.Time));
        return times.Distinct().Where(s.IsDefinedAt);
    }

    [Theory]
    [MemberData(nameof(PropertySequencePairs))]
    public void HorizontalDeviationIsNonNegative(Sequence a, Sequence b)
    {
        if (Interval.Intersection(a.Image, b.Image) is null) return;
        Assert.True(Sequence.HorizontalDeviation(a, b) >= 0);
    }

    [Theory]
    [MemberData(nameof(PropertySequenceTestCases))]
    public void HorizontalDeviationOfASequenceFromItselfIsZero(Sequence a)
    {
        Assert.Equal(0, Sequence.HorizontalDeviation(a, a));
    }

    /// <summary>
    /// Delaying the second operand delays every crossing by the same amount, so the deviation cannot decrease.
    /// Once it is strictly positive, so that the positive part of [TBP-EB-FRTC] EB-FRTC-SEQ-D2 is inactive, a further delay adds exactly itself.
    /// </summary>
    [Theory]
    [MemberData(nameof(PropertySequencePairs))]
    public void DelayingTheSecondOperandDelaysTheDeviation(Sequence a, Sequence b)
    {
        if (Interval.Intersection(a.Image, b.Image) is null) return;

        var delay = new Rational(3, 2);
        var h = Sequence.HorizontalDeviation(a, b);
        var once = Sequence.HorizontalDeviation(a, b.Delay(delay, prependWithZero: false));
        var twice = Sequence.HorizontalDeviation(a, b.Delay(2 * delay, prependWithZero: false));

        Assert.True(once >= h);
        Assert.True(twice >= once);
        if (once > 0)
            Assert.Equal(delay, twice - once);
    }
}
