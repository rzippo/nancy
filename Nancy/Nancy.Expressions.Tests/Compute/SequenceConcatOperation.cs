using System;
using Xunit;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// Concatenation joins the second sequence after the first, so it is the one sequence operation whose operands are not interchangeable.
/// The expression node is binary for that reason.
/// </summary>
public class SequenceConcatOperation
{
    // Right-open, so that the sequence following it can be left-closed at the join without two points meeting there.
    private static readonly Sequence A =
        new([Point.Origin(), new Segment(0, 2, 0, 1)]);
    // Starts at 1 rather than 0, so that preserveDelay has something to preserve.
    // Starts at a non-zero value, so that preserveShift does too.
    // Starts at 1 rather than 0, so that preserveDelay has something to preserve.
    // Starts at a non-zero value, so that preserveShift does too.
    private static readonly Sequence B =
        new([new Point(1, 10), new Segment(1, 3, 10, 2)]);

    private static SequenceExpression Ae => A.ToExpression("a");
    private static SequenceExpression Be => B.ToExpression("b");

    public static TheoryData<bool, bool> Flags => new() { { false, false }, { true, false }, { false, true }, { true, true } };

    /// <summary>
    /// The expression stands for the operation, so it answers what the operation answers, refusals included:
    /// <see cref="Sequence.Concat(Sequence,Sequence,bool,bool)"/> rejects a delay it is asked to preserve, and the
    /// expression rejects it the same way rather than hiding the difference.
    /// </summary>
    [Theory]
    [MemberData(nameof(Flags))]
    public void TheExpressionAnswersWhatTheSequenceOperationAnswers(bool preserveDelay, bool preserveShift)
    {
        Sequence expected = null;
        Exception refused = null;
        try
        {
            expected = Sequence.Concat(A, B, preserveDelay, preserveShift);
        }
        catch (ArgumentException e)
        {
            refused = e;
        }

        var expression = Ae.Concat(Be, preserveDelay, preserveShift);

        if (refused is not null)
        {
            var thrown = Assert.Throws<ArgumentException>(() => expression.Compute());
            Assert.Equal(refused.Message, thrown.Message);
        }
        else
        {
            Assert.True(expected!.Equivalent(expression.Compute()));
        }
    }

    [Fact]
    public void TheOrderOfTheOperandsMatters()
    {
        var oneWay = Ae.Concat(Be).Compute();
        var theOther = Be.Concat(Ae).Compute();

        Assert.False(oneWay.Equivalent(theOther));
    }

    /// <summary>
    /// The flags are the node's own state rather than an operand.
    /// Two concatenations of the same operands that differ in a flag are different expressions, and compute different values.
    /// </summary>
    [Fact]
    public void TheFlagsParticipateInEquality()
    {
        var plain = new SequenceConcatExpression(Ae, Be);
        var same = new SequenceConcatExpression(Ae, Be);
        var delayed = new SequenceConcatExpression(Ae, Be, preserveDelay: true);
        var shifted = new SequenceConcatExpression(Ae, Be, preserveShift: true);

        Assert.Equal(plain, same);
        Assert.NotEqual(plain, delayed);
        Assert.NotEqual(plain, shifted);
        Assert.NotEqual(delayed, shifted);
    }

    /// <summary>
    /// A rewrite rebuilds the node around new operands, and the flags are not operands, so they survive it.
    /// </summary>
    [Fact]
    public void ARewriteInsideAConcatenationKeepsTheFlags()
    {
        var expression = new SequenceConcatExpression(Ae, Be, preserveDelay: true, preserveShift: true);
        var replacement = A.ToExpression("c");

        var rewritten = expression.ReplaceByValue(Ae, replacement);

        var concat = Assert.IsType<SequenceConcatExpression>(rewritten);
        Assert.True(concat.PreserveDelay);
        Assert.True(concat.PreserveShift);
        Assert.Equal("c", concat.LeftOperand.Name);
    }

    [Fact]
    public void TheRenderingsNameTheOperation()
    {
        var expression = Ae.Concat(Be);

        Assert.Equal("a ⌢ b", expression.ToUnicodeString());
        Assert.Contains("\\frown", expression.ToLatexString());
    }
}
