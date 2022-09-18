using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class JsonMerge
{
    /// <summary>
    /// This set of tests comes from non-overflowing lower envelope tests, stopped before the merging phase.
    /// </summary>
    public static string[] ElementsNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/Sequences/JsonMergeTests/1.json"
    };

    /// <summary>
    /// This set of tests comes from lower envelope tests which overflow if Rational is used, stopped before the merging phase.
    /// Thus the test may contain numbers larger than long.MaxValue, and should be not executed if Rational is used.
    /// </summary>
    public static string[] LargeElementsNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/Sequences/JsonMergeTests/8.json"
    };

    public static IEnumerable<object[]> GetJsonTestCases()
    {
        foreach (var curveName in ElementsNames)
        {
            string json = EmbeddedResourceDataAttribute.ReadManifestData(curveName);
            var elements = JsonConvert.DeserializeObject<Element[]>(json, new GenericCurveConverter(), new RationalConverter())!;

            yield return new object[] { elements };
        }
    }

    public static bool CheckEquivalence(IEnumerable<Element> reference, IEnumerable<Element> merged)
    {
        foreach (var referenceElement in reference)
        {
            var candidate = FindCandidate(referenceElement);
            if (candidate == null || !CheckCandidate(referenceElement, candidate))
                return false;
            else
                continue;
        }

        return true;

        //local functions

        Element FindCandidate(Element referenceElement)
        {
            var check = reference
                .Where(mergedElement => mergedElement.StartTime <= referenceElement.StartTime && referenceElement.EndTime <= mergedElement.EndTime);

            var candidates = merged
                .Where(mergedElement => mergedElement.StartTime <= referenceElement.StartTime && referenceElement.EndTime <= mergedElement.EndTime );

            if (referenceElement is Point p)
            {
                if (candidates.Count() > 1)
                    return candidates.Where(c => c is Point).Single();
                else
                    return (Segment)candidates.Single();
            }
            else
                return (Segment)candidates.Single();
        }

        bool CheckCandidate(Element referenceElement, Element candidateElement)
        {
            switch (referenceElement)
            {
                case Point referencePoint:
                    if (candidateElement is Point candidatePoint)
                        return candidatePoint.Time == referencePoint.Time && candidatePoint.Value == referencePoint.Value;
                    else
                        return candidateElement.ValueAt(referencePoint.Time) == referencePoint.Value;

                case Segment referenceSegment:
                    if (candidateElement is Point)
                    {
                        return false;
                    }
                    else
                    {
                        var cut = ((Segment) candidateElement).Cut(referenceSegment.StartTime, referenceSegment.EndTime);
                        return cut == referenceSegment;
                    }

                default:
                    throw new InvalidCastException();
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetJsonTestCases))]
    public void FromJsonTest(Element[] elements)
    {
        var mergeElements = elements.Merge(true);

        Assert.True(CheckEquivalence(reference: elements, merged: mergeElements));

        Assert.True(mergeElements.AreInTimeSequence());
    }
}