using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// A sequence cut computes under the settings of its node, or under those given to <c>Compute</c>.
/// </summary>
/// <remarks>
/// Which settings the cut computed under is read off its value:
/// with <see cref="ComputationSettings.UseKnownPropertyPropagation"/> on, <see cref="Sequence.Cut(Interval, ComputationSettings)"/> of the whole support returns its operand,
/// and with it off, a new sequence.
/// </remarks>
public class SequenceCutUnderGivenSettings
{
    private static readonly Sequence S = new([Point.Origin(), new Segment(0, 2, 0, 1)]);

    private static readonly Interval WholeSupport = new(0, 2, true, false);

    private static readonly ExpressionSettings Propagating =
        new() { ComputationSettings = new ComputationSettings { UseKnownPropertyPropagation = true } };

    private static readonly ExpressionSettings NotPropagating =
        new() { ComputationSettings = new ComputationSettings { UseKnownPropertyPropagation = false } };

    private static SequenceExpression Cut(ExpressionSettings own = null)
        => S.ToExpression("s").Cut(WholeSupport, settings: own);

    [Fact]
    public void TheCutOfTheWholeSupportIsItsOperandOnlyWhenPropagating()
    {
        // pins the observable the other tests rely on
        Assert.Same(S, S.Cut(WholeSupport, Propagating.ComputationSettings));
        Assert.NotSame(S, S.Cut(WholeSupport, NotPropagating.ComputationSettings));
    }

    [Fact]
    public void TheCutComputesUnderItsOwnSettings()
    {
        Assert.Same(S, Cut(own: Propagating).Compute());
        Assert.NotSame(S, Cut(own: NotPropagating).Compute());
    }

    [Fact]
    public void TheCutComputesUnderTheSettingsGivenToCompute()
    {
        Assert.Same(S, Cut().Compute(Propagating));
        Assert.NotSame(S, Cut().Compute(NotPropagating));
    }
}
