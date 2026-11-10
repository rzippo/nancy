using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class MaxDegreeOfParallelism
{
    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(512)]
    public void AcceptsTheRangeOfParallelOptionsAndPlinq(int value)
    {
        var settings = new ComputationSettings { MaxDegreeOfParallelism = value };
        Assert.Equal(value, settings.MaxDegreeOfParallelism);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    [InlineData(513)]
    public void RejectsValuesOutsideTheRange(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ComputationSettings { MaxDegreeOfParallelism = value });
    }
}
