using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Composition
{
    public static List<(Sequence f, Sequence g, Sequence expected)> KnownCompositions =
    [
        // simple initial tests
        (
            f: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 1),
                Point.Zero(1),
                new Segment(1, 2, 0, 2)
            ]),
            g: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 1),
                Point.Zero(1),
                new Segment(1, 2, 0, 1),
                new Point(2, 1),
                new Segment(2, 3, 1, 0),
                new Point(3, 1),
                new Segment(3, 4, 1, 1)
            ]),
            expected: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 3),
                Point.Zero(3),
                new Segment(3, 4, 0, 2)
            ])
        ),
        (
            f: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 1),
                Point.Zero(1),
                new Segment(1, 4, 0, 3)
            ]),
            g: new Sequence([
                Point.Origin(),
                new Segment(0, 1, 0, 2),
                new Point(1, 2),
                new Segment(1, 2, 2, 0),
                new Point(2, 2),
                new Segment(2, 3, 2, 2)
            ]),
            expected: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 0.5m),
                Point.Zero(0.5m),
                new Segment(0.5m, 1, 0, 6),
                new Point(1, 3),
                new Segment(1, 2, 3, 0),
                new Point(2, 3),
                new Segment(2, 3, 3, 6)
            ])
        ),
        // extracted from Curve tests
        (
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,1,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,1,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,2), new Point(2,2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,1), new Point(2,1), new Segment(2,3,1,0), new Point(3,1), new Segment(3,4,1,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,0,0), new Point(3,0), new Segment(3,4,0,2) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,4,0,3), new Point(4,9) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,0), new Point(2,2), new Segment(2,3,2,2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),1,0,6), new Point(1,3), new Segment(1,2,3,0), new Point(2,3), new Segment(2,3,3,6) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,1), new Segment(1,new Rational(5, 4),1,0), new Point(new Rational(5, 4),1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,new Rational(5, 4),0), new Point(1,new Rational(5, 4)), new Segment(1,2,new Rational(5, 4),0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,new Rational(5, 4),2,0), new Point(new Rational(5, 4),2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,new Rational(5, 4),0), new Point(1,new Rational(5, 4)), new Segment(1,2,new Rational(5, 4),0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,2,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,3), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),1,0,6), new Point(1,3), new Segment(1,2,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0), new Point(2,2), new Segment(2,3,3,0), new Point(3,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,2,0), new Point(1,2), new Segment(1,2,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,1), new Segment(1,2,1,0), new Point(2,2), new Segment(2,3,2,0), new Point(3,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,2), new Segment(1,2,2,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,3), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),1,0,6), new Point(1,3), new Segment(1,2,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,1), new Segment(1,2,1,0), new Point(2,2), new Segment(2,new Rational(23, 10),2,0), new Point(new Rational(23, 10),2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(13, 3),1,new Rational(3, 10)) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(10, 3),1,0), new Point(new Rational(10, 3),2), new Segment(new Rational(10, 3),new Rational(13, 3),2,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0), new Point(2,2), new Segment(2,new Rational(23, 10),3,0), new Point(new Rational(23, 10),3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(13, 3),1,new Rational(3, 10)) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(10, 3),2,0), new Point(new Rational(10, 3),2), new Segment(new Rational(10, 3),new Rational(13, 3),3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,3), new Point(1,3), new Segment(1,2,3,0), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,new Rational(3, 2),2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,6), new Point(new Rational(1, 2),3), new Segment(new Rational(1, 2),new Rational(3, 2),3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,3), new Point(1,3), new Segment(1,2,3,0), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,new Rational(3, 2),2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,6), new Point(new Rational(1, 2),3), new Segment(new Rational(1, 2),new Rational(3, 2),3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,1), new Segment(1,2,1,0), new Point(2,2), new Segment(2,new Rational(7, 3),2,0), new Point(new Rational(7, 3),2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,new Rational(1, 2),0), new Point(1,1), new Segment(1,2,1,0), new Point(2,new Rational(5, 3)), new Segment(2,3,new Rational(5, 3),0), new Point(3,new Rational(7, 3)), new Segment(3,4,new Rational(7, 3),0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,1), new Segment(1,3,1,0), new Point(3,2), new Segment(3,4,2,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0), new Point(2,2), new Segment(2,new Rational(7, 3),3,0), new Point(new Rational(7, 3),3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,new Rational(1, 2),0), new Point(1,1), new Segment(1,2,1,0), new Point(2,new Rational(5, 3)), new Segment(2,3,new Rational(5, 3),0), new Point(3,new Rational(7, 3)), new Segment(3,4,new Rational(7, 3),0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,0), new Point(2,2), new Segment(2,3,2,0), new Point(3,3), new Segment(3,4,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,13,4,3), new Point(13,43) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,5,4), new Point(1,9), new Segment(1,2,9,4) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,19,12) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,200,100), new Point(2,400) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,300,0), new Point(1,300), new Segment(1,2,400,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,200,100), new Point(3,500) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0), new Point(2,2), new Segment(2,3,3,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,300,0), new Point(1,300), new Segment(1,2,400,0), new Point(2,400), new Segment(2,3,500,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,200,100), new Point(4,600) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,2,0), new Point(1,2), new Segment(1,2,4,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,400,0), new Point(1,400), new Segment(1,2,600,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,200,100), new Point(4,600) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,3,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,300,100), new Point(1,400), new Segment(1,2,500,100) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,100,0,0), new Point(100,0), new Segment(100,104,200,100), new Point(104,600) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,2,0), new Point(1,2), new Segment(1,2,4,0), new Point(2,4), new Segment(2,3,6,0), new Point(3,6), new Segment(3,4,8,0), new Point(4,8), new Segment(4,5,10,0), new Point(5,10), new Segment(5,6,12,0), new Point(6,12), new Segment(6,7,14,0), new Point(7,14), new Segment(7,8,16,0), new Point(8,16), new Segment(8,9,18,0), new Point(9,18), new Segment(9,10,20,0), new Point(10,20), new Segment(10,11,22,0), new Point(11,22), new Segment(11,12,24,0), new Point(12,24), new Segment(12,13,26,0), new Point(13,26), new Segment(13,14,28,0), new Point(14,28), new Segment(14,15,30,0), new Point(15,30), new Segment(15,16,32,0), new Point(16,32), new Segment(16,17,34,0), new Point(17,34), new Segment(17,18,36,0), new Point(18,36), new Segment(18,19,38,0), new Point(19,38), new Segment(19,20,40,0), new Point(20,40), new Segment(20,21,42,0), new Point(21,42), new Segment(21,22,44,0), new Point(22,44), new Segment(22,23,46,0), new Point(23,46), new Segment(23,24,48,0), new Point(24,48), new Segment(24,25,50,0), new Point(25,50), new Segment(25,26,52,0), new Point(26,52), new Segment(26,27,54,0), new Point(27,54), new Segment(27,28,56,0), new Point(28,56), new Segment(28,29,58,0), new Point(29,58), new Segment(29,30,60,0), new Point(30,60), new Segment(30,31,62,0), new Point(31,62), new Segment(31,32,64,0), new Point(32,64), new Segment(32,33,66,0), new Point(33,66), new Segment(33,34,68,0), new Point(34,68), new Segment(34,35,70,0), new Point(35,70), new Segment(35,36,72,0), new Point(36,72), new Segment(36,37,74,0), new Point(37,74), new Segment(37,38,76,0), new Point(38,76), new Segment(38,39,78,0), new Point(39,78), new Segment(39,40,80,0), new Point(40,80), new Segment(40,41,82,0), new Point(41,82), new Segment(41,42,84,0), new Point(42,84), new Segment(42,43,86,0), new Point(43,86), new Segment(43,44,88,0), new Point(44,88), new Segment(44,45,90,0), new Point(45,90), new Segment(45,46,92,0), new Point(46,92), new Segment(46,47,94,0), new Point(47,94), new Segment(47,48,96,0), new Point(48,96), new Segment(48,49,98,0), new Point(49,98), new Segment(49,50,100,0), new Point(50,100), new Segment(50,51,102,0), new Point(51,102), new Segment(51,52,104,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,50,0,0), new Point(50,0), new Segment(50,51,400,0), new Point(51,400), new Segment(51,52,600,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,12,3,new Rational(1, 3)), new Point(12,7) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,0,4), new Point(3,12), new Segment(3,4,12,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,3,new Rational(4, 3)), new Point(3,7), new Segment(3,4,7,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,2), new Point(2,2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,1), new Point(2,1), new Segment(2,3,1,0), new Point(3,1), new Segment(3,4,1,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,0,0), new Point(3,0), new Segment(3,4,0,2) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,8,0,3), new Point(8,21) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,0), new Point(2,2), new Segment(2,5,2,2) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),1,0,6), new Point(1,3), new Segment(1,2,3,0), new Point(2,3), new Segment(2,5,3,6) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,3), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,4,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),1,0,6), new Point(1,3), new Segment(1,4,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,0,3), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,5,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),1,0,6), new Point(1,3), new Segment(1,5,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,3), new Point(1,3), new Segment(1,2,3,0), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,6), new Point(new Rational(1, 2),3), new Segment(new Rational(1, 2),2,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,3), new Point(1,3), new Segment(1,2,3,0), new Point(2,3) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,6), new Point(new Rational(1, 2),3), new Segment(new Rational(1, 2),2,3,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,13,4,3), new Point(13,43) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,5,4), new Point(1,9), new Segment(1,2,9,4) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,19,12) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,200,100), new Point(2,400) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,300,0), new Point(1,300), new Segment(1,2,400,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,200,100), new Point(3,500) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0), new Point(2,2), new Segment(2,3,3,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,300,0), new Point(1,300), new Segment(1,2,400,0), new Point(2,400), new Segment(2,3,500,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,200,100), new Point(4,600) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,2,0), new Point(1,2), new Segment(1,2,4,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,400,0), new Point(1,400), new Segment(1,2,600,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,200,100), new Point(4,600) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,3,1) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,300,100), new Point(1,400), new Segment(1,2,500,100) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,100,0,0), new Point(100,0), new Segment(100,104,200,100), new Point(104,600) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,2,0), new Point(1,2), new Segment(1,2,4,0), new Point(2,4), new Segment(2,3,6,0), new Point(3,6), new Segment(3,4,8,0), new Point(4,8), new Segment(4,5,10,0), new Point(5,10), new Segment(5,6,12,0), new Point(6,12), new Segment(6,7,14,0), new Point(7,14), new Segment(7,8,16,0), new Point(8,16), new Segment(8,9,18,0), new Point(9,18), new Segment(9,10,20,0), new Point(10,20), new Segment(10,11,22,0), new Point(11,22), new Segment(11,12,24,0), new Point(12,24), new Segment(12,13,26,0), new Point(13,26), new Segment(13,14,28,0), new Point(14,28), new Segment(14,15,30,0), new Point(15,30), new Segment(15,16,32,0), new Point(16,32), new Segment(16,17,34,0), new Point(17,34), new Segment(17,18,36,0), new Point(18,36), new Segment(18,19,38,0), new Point(19,38), new Segment(19,20,40,0), new Point(20,40), new Segment(20,21,42,0), new Point(21,42), new Segment(21,22,44,0), new Point(22,44), new Segment(22,23,46,0), new Point(23,46), new Segment(23,24,48,0), new Point(24,48), new Segment(24,25,50,0), new Point(25,50), new Segment(25,26,52,0), new Point(26,52), new Segment(26,27,54,0), new Point(27,54), new Segment(27,28,56,0), new Point(28,56), new Segment(28,29,58,0), new Point(29,58), new Segment(29,30,60,0), new Point(30,60), new Segment(30,31,62,0), new Point(31,62), new Segment(31,32,64,0), new Point(32,64), new Segment(32,33,66,0), new Point(33,66), new Segment(33,34,68,0), new Point(34,68), new Segment(34,35,70,0), new Point(35,70), new Segment(35,36,72,0), new Point(36,72), new Segment(36,37,74,0), new Point(37,74), new Segment(37,38,76,0), new Point(38,76), new Segment(38,39,78,0), new Point(39,78), new Segment(39,40,80,0), new Point(40,80), new Segment(40,41,82,0), new Point(41,82), new Segment(41,42,84,0), new Point(42,84), new Segment(42,43,86,0), new Point(43,86), new Segment(43,44,88,0), new Point(44,88), new Segment(44,45,90,0), new Point(45,90), new Segment(45,46,92,0), new Point(46,92), new Segment(46,47,94,0), new Point(47,94), new Segment(47,48,96,0), new Point(48,96), new Segment(48,49,98,0), new Point(49,98), new Segment(49,50,100,0), new Point(50,100), new Segment(50,51,102,0), new Point(51,102), new Segment(51,52,104,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,50,0,0), new Point(50,0), new Segment(50,51,400,0), new Point(51,400), new Segment(51,52,600,0) })
		),
		(
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,12,3,new Rational(1, 3)), new Point(12,7) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,0,4), new Point(3,12), new Segment(3,4,12,0) }),
			new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,3,new Rational(4, 3)), new Point(3,7), new Segment(3,4,7,0) })
		),
		// new tests for new algorithm with relaxed assumptions.
        // f domain must be a superset of g image.
        // single point
        (
            f: new Sequence([
	            new Segment(0, 2, 0, 3)
            ]),
            g: new Sequence([
	            new Point(1, 1)
            ]),
            expected: new Sequence([
                new Point(1, 3)
            ])
        ),
        // single segment
        (
            f: new Sequence([
                new Point(0, 1),
                new Segment(0, 4, 1, 2)
            ]),
            g: new Sequence([
                new Segment(1, 2, 0, 1)
            ]),
            expected: new Sequence([
                new Segment(1, 2, 1, 2)
            ])
        ),
        // single segment
        (
            f: new Sequence([
                new Point(0, 1),
                new Segment(0, 4, 1, 2)
            ]),
            g: new Sequence([
                new Segment(1, 2, 0, 2)
            ]),
            expected: new Sequence([
                new Segment(1, 2, 1, 4)
            ])
        ),
        // jump at the end
        (
            f: new Sequence([
	            new Point(0, 0),
	            new Segment(0, 5, 0, 1)
            ]),
            g: new Sequence([
	            new Point(1, 1),
	            new Segment(1, 2, 1, 1),
	            new Point(2, 4)
            ]),
            expected: new Sequence([
                new Point(1, 1),
                new Segment(1, 2, 1, 1),
                new Point(2, 4)
            ])
        ),
        // right-open, right-closed
        (
	        new Sequence([
		        Point.Origin(),
		        new Segment(0, 2, 0, 0.5m),
		        new Point(2, 1),
		        new Segment(2, 3, 1, 1),
		        new Point(3, 3),
		        new Segment(3, 5, 3, 0.5m),
		        new Point(5, 4)
	        ]),
	        new Sequence([
		        new Point(2, 1),
		        new Segment(2, 4, 1, 1),
		        new Point(4, 3),
		        new Segment(4, 6, 3, 0.5m),
		        new Point(6, 4)
	        ]),
	        new Sequence([
		        new Point(2, 0.5m),
		        new Segment(2, 3, 0.5m, 0.5m),
		        new Point(3, 1),
		        new Segment(3, 4, 1, 1),
		        new Point(4, 3),
		        new Segment(4, 6, 3, 0.25m),
		        new Point(6, 3.5m)
	        ])
        ),
        // left-open, right-closed
        (
	        new Sequence([
		        Point.Origin(),
		        new Segment(0, 2, 0, 0.5m),
		        new Point(2, 1),
		        new Segment(2, 3, 1, 1),
		        new Point(3, 3),
		        new Segment(3, 5, 3, 0.5m),
		        new Point(5, 4)
	        ]),
	        new Sequence([
				new Segment(2, 4, 1, 1),
				new Point(4, 3),
				new Segment(4, 6, 3, 0.5m),
				new Point(6, 4)
	        ]),
	        new Sequence([
				new Segment(2, 3, 0.5m, 0.5m),
				new Point(3, 1),
				new Segment(3, 4, 1, 1),
				new Point(4, 3),
				new Segment(4, 6, 3, 0.25m),
				new Point(6, 3.5m)
	        ])
	    ),
        // left-open, right-open
        (
	        new Sequence([
		        Point.Origin(),
		        new Segment(0, 2, 0, 0.5m),
		        new Point(2, 1),
		        new Segment(2, 3, 1, 1),
		        new Point(3, 3),
		        new Segment(3, 5, 3, 0.5m),
		        new Point(5, 4)
	        ]),
	        new Sequence([
		        new Segment(2, 4, 1, 1),
		        new Point(4, 3),
		        new Segment(4, 6, 3, 0.5m)
	        ]),
	        new Sequence([
		        new Segment(2, 3, 0.5m, 0.5m),
		        new Point(3, 1),
		        new Segment(3, 4, 1, 1),
		        new Point(4, 3),
		        new Segment(4, 6, 3, 0.25m)
	        ])
        )
    ];

    public static IEnumerable<object[]> GetTestCases()
	    => KnownCompositions.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void EquivalenceToExpected(Sequence f, Sequence g, Sequence expected)
    {
        var result = Sequence.Composition(f, g);
        Assert.True(Sequence.Equivalent(result, expected));
    }

    /// <summary>
    /// A well-behaved outer operand over [0, 6], left-continuous at its jump at 2.
    /// </summary>
    public static Sequence OuterOperand = new Sequence([
        Point.Origin(),
        new Segment(0, 2, 0, new Rational(1, 2)),
        new Point(2, 1),
        new Segment(2, 6, 3, new Rational(1, 2)),
        new Point(6, 5)
    ]);

    /// <summary>
    /// Pairs on which the composition is defined, covering the shapes the operands can take at and between the boundaries of the inner one's domain.
    /// </summary>
    public static List<(Sequence f, Sequence g)> WellDefinedPairs =
    [
        // the boundary cases: closed, open with a constant element, open with an increasing one
        (OuterOperand, new Sequence([ new Point(1, 2), new Segment(1, 3, 2, 1), new Point(3, 4) ])),
        (OuterOperand, new Sequence([ new Segment(1, 3, 2, 1), new Point(3, 4) ])),
        (OuterOperand, new Sequence([ new Point(1, 0), new Segment(1, 3, 0, 1) ])),
        (OuterOperand, new Sequence([ new Segment(1, 2, 2, 0), new Point(2, 2), new Segment(2, 3, 2, 1), new Point(3, 3) ])),
        (OuterOperand, new Sequence([ new Point(1, 0), new Segment(1, 2, 0, 2), new Point(2, 2), new Segment(2, 3, 2, 0) ])),

        // the inner operand jumps inside its domain, so its image has a gap
        (OuterOperand, new Sequence([ new Point(1, 1), new Segment(1, 2, 1, 0), new Point(2, 4), new Segment(2, 3, 4, 0), new Point(3, 4) ])),
        // and jumps over a breakpoint of the outer operand, which is therefore never read
        (OuterOperand, new Sequence([ new Point(1, 1), new Segment(1, 2, 1, 0), new Point(2, 3), new Segment(2, 3, 3, 0), new Point(3, 3) ])),

        // the image is exactly the outer operand's domain
        (OuterOperand, new Sequence([ new Point(1, 0), new Segment(1, 3, 0, 3), new Point(3, 6) ])),
        // the image ends exactly at the outer operand's right end
        (OuterOperand, new Sequence([ new Point(1, 3), new Segment(1, 3, 3, new Rational(3, 2)), new Point(3, 6) ])),

        // the outer operand is not monotone, which the operation does not require
        (
            new Sequence([
                Point.Origin(), new Segment(0, 2, 0, 2), new Point(2, 4),
                new Segment(2, 4, 4, -1), new Point(4, 2), new Segment(4, 6, 2, 1), new Point(6, 4)
            ]),
            new Sequence([ new Point(1, 1), new Segment(1, 3, 1, 2), new Point(3, 5) ])
        ),
        // the outer operand is infinite over part of its domain
        (
            new Sequence([
                Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2),
                new Segment(2, 4, Rational.PlusInfinity, 0), new Point(4, Rational.PlusInfinity),
                new Segment(4, 6, 4, 1), new Point(6, 6)
            ]),
            new Sequence([ new Point(1, 1), new Segment(1, 3, 1, 2), new Point(3, 5) ])
        ),

        // a constant inner operand: the image is a single value, however many elements it takes
        (OuterOperand, new Sequence([ new Point(1, 2) ])),
        (OuterOperand, new Sequence([ new Segment(1, 3, 2, 0) ])),
        (OuterOperand, new Sequence([ new Segment(1, 3, 2, 0), new Point(3, 2) ])),
        (OuterOperand, new Sequence([ new Point(1, 2), new Segment(1, 3, 2, 0) ])),
        (OuterOperand, new Sequence([ new Point(1, 2), new Segment(1, 3, 2, 0), new Point(3, 2) ])),
        (OuterOperand, new Sequence([ new Point(1, 2), new Segment(1, 2, 2, 0), new Point(2, 2), new Segment(2, 3, 2, 0), new Point(3, 2) ])),
        // and one that is constant against an outer operand of a single point
        (new Sequence([ new Point(2, 7) ]), new Sequence([ new Point(1, 2), new Segment(1, 3, 2, 0), new Point(3, 2) ])),

        // barely increasing, for contrast with the constant ones above
        (OuterOperand, new Sequence([ new Point(1, 2), new Segment(1, 3, 2, new Rational(1, 1000)), new Point(3, new Rational(2002, 1000)) ])),
    ];

    public static IEnumerable<object[]> WellDefinedPairsTestCases => WellDefinedPairs.ToXUnitTestCases();

    /// <summary>
    /// The composition must agree pointwise with $t \mapsto f(g(t))$, and be defined exactly where the inner operand is.
    /// </summary>
    [Theory]
    [MemberData(nameof(WellDefinedPairsTestCases))]
    public void CompositionAgreesWithPointwiseEvaluation(Sequence f, Sequence g)
    {
        var h = Sequence.Composition(f, g);

        Assert.Equal(g.DefinedFrom, h.DefinedFrom);
        Assert.Equal(g.DefinedUntil, h.DefinedUntil);
        Assert.Equal(g.IsLeftClosed, h.IsLeftClosed);
        Assert.Equal(g.IsRightClosed, h.IsRightClosed);

        var times = new List<Rational>();
        for (var i = 0; i <= 40; i++)
            times.Add(g.DefinedFrom + (g.DefinedUntil - g.DefinedFrom) * new Rational(i, 40));
        times.AddRange(g.EnumerateBreakpoints().Select(bp => bp.center.Time));
        times.AddRange(h.EnumerateBreakpoints().Select(bp => bp.center.Time));

        foreach (var t in times.Distinct().Where(g.IsDefinedAt))
        {
            Assert.True(h.IsDefinedAt(t));
            Assert.Equal(f.ValueAt(g.ValueAt(t)), h.ValueAt(t));
        }
    }

    /// <summary>
    /// Inner operands the operation does not accept.
    /// </summary>
    public static List<(Sequence f, Sequence g)> RejectedPairs =
    [
        // negative
        (OuterOperand, new Sequence([ new Point(1, -1), new Segment(1, 3, -1, 1), new Point(3, 1) ])),
        // infinite
        (OuterOperand, new Sequence([ new Point(1, 2), new Segment(1, 3, 2, 1), new Point(3, Rational.PlusInfinity) ])),
        // decreasing
        (OuterOperand, new Sequence([ new Point(1, 4), new Segment(1, 3, 4, -1), new Point(3, 2) ])),
        // the image leaves the outer operand's domain
        (OuterOperand, new Sequence([ new Point(1, 4), new Segment(1, 3, 4, 2), new Point(3, 8) ])),
    ];

    public static IEnumerable<object[]> RejectedPairsTestCases => RejectedPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(RejectedPairsTestCases))]
    public void UnsupportedOperandsAreRejected(Sequence f, Sequence g)
    {
        Assert.Throws<ArgumentException>(() => Sequence.Composition(f, g));
    }
}
