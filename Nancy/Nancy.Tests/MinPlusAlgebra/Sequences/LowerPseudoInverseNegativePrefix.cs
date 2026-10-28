using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks that restricting the inverse to nonnegative heights skips the negative source prefix lazily.
/// </summary>
public class LowerPseudoInverseNegativePrefix
{
    [Fact]
    public void StopsReadingAfterTheRequestedPrefix()
    {
        IEnumerable<Element> Source()
        {
            yield return new Point(0, -1);
            yield return Segment.Constant(0, 1, -1);
            yield return new Point(1, 1);
            yield return Segment.Constant(1, 2, 1);
            yield return new Point(2, 2);
            yield return new Segment(2, 3, 2, 1);
            yield return new Point(3, 3);
            throw new InvalidOperationException("The unrequested tail was consumed.");
        }

        var first = Source().LowerPseudoInverse(startFromZero: true)
            .Take(1)
            .Single();
        Assert.Equal(new Point(0, 1), first);
    }
}
