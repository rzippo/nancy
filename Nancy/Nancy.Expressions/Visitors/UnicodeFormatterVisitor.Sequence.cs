using System.Text;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// The sequence half of <see cref="UnicodeFormatterVisitor"/>.
/// </summary>
public partial class UnicodeFormatterVisitor : ISequenceExpressionVisitor<(StringBuilder UnicodeBuilder, bool NeedsParentheses)>
{
    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ConcreteSequenceExpression expression)
        => (FormatName(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequencePlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceSubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceConcatExpression expression)
        => VisitBinaryInfix(expression, " ⌢ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceMinimumExpression expression)
        => VisitNAryInfix(expression, " ∧ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceMaximumExpression expression)
        => VisitNAryInfix(expression, " ∨ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceConvolutionExpression expression)
        => VisitNAryInfix(expression, " ⊗ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceMaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, " \u0305⊗ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " ⊘ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceMaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " \u0305⊘ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceCompositionExpression expression)
        => VisitBinaryInfix(expression, " ∘ ");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceScaleExpression expression)
        => VisitBinaryInfix(expression, "·");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceDelayExpression expression)
        => VisitBinaryPrefix(expression, "delay");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceForwardExpression expression)
        => VisitBinaryPrefix(expression, "forward");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceHorizontalShiftExpression expression)
        => VisitBinaryPrefix(expression, "hShift");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceVerticalShiftExpression expression)
    {
        switch (expression.RightOperand)
        {
            case NegateRationalExpression negate:
            {
                var inner = negate.Operand;
                var substitute = new SequenceVerticalShiftExpression(
                    (SequenceExpression)expression.LeftOperand,
                    (RationalExpression)inner);
                return VisitBinaryInfix(substitute, " - ");
            }

            case RationalNumberExpression rex when rex.Value.IsNegative:
            {
                var substitute = new SequenceVerticalShiftExpression(
                    (SequenceExpression)expression.LeftOperand,
                    new RationalNumberExpression(-rex.Value));
                return VisitBinaryInfix(substitute, " - ");
            }

            default:
            {
                return VisitBinaryInfix(expression, " + ");
            }
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceNegateExpression expression)
        => VisitUnaryPrefix(expression, "-");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceToNonNegativeExpression expression)
        => VisitToNonNegative(expression);

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceToLeftContinuousExpression expression)
        => VisitUnaryPrefix(expression, "toLeftContinuous");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceToRightContinuousExpression expression)
        => VisitUnaryPrefix(expression, "toRightContinuous");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceLowerPseudoInverseExpression expression)
        => VisitPseudoInverse(expression, '↓');

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceUpperPseudoInverseExpression expression)
        => VisitPseudoInverse(expression, '↑');

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceCutExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cut(");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append(expression.Interval);
        sb.Append(')');

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceCutToNeighbourhoodExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cutToNeighbourhood(");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append($"[{expression.CutStart}, {expression.CutEnd}]");
        sb.Append(')');

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(CurveCutExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cut(");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append(expression.Interval);
        sb.Append(')');

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(CurveCutToNeighbourhoodExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cutToNeighbourhood(");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append($"[{expression.CutStart}, {expression.CutEnd}]");
        sb.Append(')');

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceValueAtExpression expression)
        => VisitValueAt(expression);

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceLeftLimitAtExpression expression)
        => VisitLimitAt(expression, "⁻");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceRightLimitAtExpression expression)
        => VisitLimitAt(expression, "⁺");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceHorizontalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "hdev");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceVerticalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "vdev");

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceFloorExpression expression)
        => VisitEnclosing(expression, '⌊', '⌋');

    /// <inheritdoc />
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SequenceCeilExpression expression)
        => VisitEnclosing(expression, '⌈', '⌉');
}
