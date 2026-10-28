using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

internal static class ValidationCases
{
    internal static List<Element[]> InvalidInputs() =>
    [
        [new Point(0, 0), new Point(1, 1)],
        [new Segment(0, 2, 0, 1), new Point(1, 1)],
        [new Point(2, 0), new Segment(1, 3, 1, 1)],
        [new Point(0, 0), new Segment(1, 2, 0, 1)],
        [new Segment(0, 1, 0, 1), new Segment(1, 2, 1, 1)]
    ];

    internal static Element[] ValidInput() =>
        [new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2)];

    internal static IEnumerable<Element> SingleUse(IEnumerable<Element> elements)
    {
        var used = false;
        return Enumerate();
        IEnumerable<Element> Enumerate()
        {
            if (used) throw new InvalidOperationException("The input was enumerated twice.");
            used = true;
            foreach (var element in elements) yield return element;
        }
    }
}
