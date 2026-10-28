using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class BooleanCacheLifecycle
{
    [Fact]
    public void ARegularPropertyRetainsFailureUntilReset()
    {
        var curve = new ControlledCurve();
        var failure = Assert.Throws<InvalidOperationException>(() => curve.IsRegularSubAdditive);
        curve.Fail = false;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => curve.IsRegularSubAdditive));
        Assert.Equal(1, curve.Reads);
        curve.ResetCachedProperties();
        Assert.True(curve.IsRegularSubAdditive);
        Assert.Equal(2, curve.Reads);
        Assert.True(curve.IsRegularSubAdditive);
        Assert.Equal(2, curve.Reads);
    }

    private sealed class ControlledCurve() : Curve(Zero())
    {
        public bool Fail = true;
        public int Reads;

        public override bool IsSubAdditive
        {
            get
            {
                Reads++;
                if (Fail)
                    throw new InvalidOperationException("Controlled property failure.");
                return true;
            }
        }
    }
}
