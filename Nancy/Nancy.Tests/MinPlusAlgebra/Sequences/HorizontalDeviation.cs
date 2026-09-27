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
            2
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
            2
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
        var hdevFunctionCurves = Curve.HorizontalDeviationFunction(f, g);
        var fCut = f.Cut(0, kF, true, true);
        var gCut = g.Cut(0, kG, true, true);
        var hdevFunctionSequence = Sequence.HorizontalDeviationFunction(fCut, gCut);
        _testOutputHelper.WriteLine($"var hdevFunctionCurves = {hdevFunctionCurves.ToCodeString()};");
        _testOutputHelper.WriteLine($"var hdevFunctionSequence = {hdevFunctionSequence.ToCodeString()};");
        Assert.True(hdevFunctionCurves.Match(hdevFunctionSequence));
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
    /// Curves and cuts for which the cut of g opens where g first attains the value it starts from, i.e. the hypothesis of the equivalence, [TBP-EB-FRTC] EB-FRTC-SEQ-T2, holds.
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
    ];

    /// <summary>
    /// Curves and cuts for which g had already attained that value before the cut of g opens, i.e. the hypothesis of the equivalence, [TBP-EB-FRTC] EB-FRTC-SEQ-T2, does not hold.
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
    /// The hypothesis of [TBP-EB-FRTC] EB-FRTC-SEQ-T2, in the form that can be computed:
    /// the lower pseudo-inverse of the curve, at the value the cut starts from, is where the cut starts.
    /// </summary>
    private static bool HasReachedTheValueBeforeTheCut(Curve g, Sequence fCut, Sequence gCut)
    {
        var imageOverlap = Interval.Intersection(fCut.Image, gCut.Image)!.Value;
        var gCutStart = imageOverlap.Lower == gCut.InfValue()
            ? gCut.DefinedFrom
            : gCut.LowerPseudoInverse().ValueAt(imageOverlap.Lower);

        return g.LowerPseudoInverse().ValueAt(gCut.ValueAt(gCutStart)) < gCutStart;
    }

    [Theory]
    [MemberData(nameof(HDevFunctionCutsWithHypothesisTestCases))]
    public void HorizontalDeviationFunctionOfCutsMatchesTheCurvesUnderTheHypothesis(Curve f, Curve g, Interval xF, Interval xG)
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

        Assert.False(HasReachedTheValueBeforeTheCut(g, fCut, gCut));
        Assert.True(hdevFunctionCurves.Match(hdevFunctionSequence));
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

        Assert.True(HasReachedTheValueBeforeTheCut(g, fCut, gCut));
        Assert.False(hdevFunctionCurves.Match(hdevFunctionSequence));
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
