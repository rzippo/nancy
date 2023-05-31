using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public partial class ConvolutionIsomorphism
{
    private readonly ITestOutputHelper output;

    public ConvolutionIsomorphism(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<Sequence> LeftContinuousExamples()
    {
        return Curves.ConvolutionIsomorphism.LeftContinuousExamples
            .Select(c => c.PseudoPeriodicSequence);
    }

    public static IEnumerable<Sequence> RightContinuousExamples()
    {
        return Curves.ConvolutionIsomorphism.RightContinuousExamples
            .Select(c => c.PseudoPeriodicSequence);
    }

    public static IEnumerable<Sequence> ContinuousExamples()
    {
        return Curves.ConvolutionIsomorphism.ContinuousExamples
            .Select(c => c.PseudoPeriodicSequence);
    }
}