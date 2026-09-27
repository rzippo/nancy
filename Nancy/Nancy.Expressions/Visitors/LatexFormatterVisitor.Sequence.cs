using System.Text;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// The sequence half of <see cref="LatexFormatterVisitor"/>.
/// </summary>
public partial class LatexFormatterVisitor : ISequenceExpressionVisitor<(StringBuilder LatexBuilder, bool NeedsParentheses)>
{
    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ConcreteSequenceExpression expression)
        => (FormatName(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequencePlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceSubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceConcatExpression expression)
        => VisitBinaryInfix(expression, " \\frown ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceMinimumExpression expression)
        => VisitNAryInfix(expression, @" \wedge ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceMaximumExpression expression)
        => VisitNAryInfix(expression, @" \vee ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \otimes ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceMaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \overline{\otimes} ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \oslash ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceMaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \overline{\oslash} ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCompositionExpression expression)
        => VisitBinaryInfix(expression, @" \circ ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceScaleExpression expression)
        => VisitBinaryInfix(expression, @" \cdot ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceDelayExpression expression)
        => VisitBinaryPrefix(expression, "delay");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceForwardExpression expression)
        => VisitBinaryPrefix(expression, "forward");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceHorizontalShiftExpression expression)
        => VisitBinaryPrefix(expression, "hShift");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceVerticalShiftExpression expression)
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
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceNegateExpression expression)
        => VisitUnaryPrefix(expression, "-");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceToNonNegativeExpression expression)
        => VisitToNonNegative(expression);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceToLeftContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{l}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceToRightContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{r}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceLowerPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\underline{-1}}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceUpperPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\overline{-1}}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCutExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cut");
        sb.Append(@"\left( ");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append(expression.Interval);
        sb.Append(@" \right)");

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCutToNeighbourhoodExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cutToNeighbourhood");
        sb.Append(@"\left( ");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append($"[{expression.CutStart}, {expression.CutEnd}]");
        sb.Append(@" \right)");

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(CurveCutExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cut");
        sb.Append(@"\left( ");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append(expression.Interval);
        sb.Append(@" \right)");

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(CurveCutToNeighbourhoodExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);

        CurrentDepth++;
        var (inner, _) = GeneralizedAccept(expression.Operand);
        CurrentDepth--;

        var sb = new StringBuilder("cutToNeighbourhood");
        sb.Append(@"\left( ");
        sb.Append(inner);
        sb.Append(", ");
        sb.Append($"[{expression.CutStart}, {expression.CutEnd}]");
        sb.Append(@" \right)");

        return (sb, false);
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceValueAtExpression expression)
        => VisitValueAt(expression);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceLeftLimitAtExpression expression)
        => VisitLimitAt(expression, "^{-}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceRightLimitAtExpression expression)
        => VisitLimitAt(expression, "^{+}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceHorizontalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "hdev");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceVerticalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "vdev");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceFloorExpression expression)
        => VisitEnclosing(expression, @"\lfloor ", @" \rfloor");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCeilExpression expression)
        => VisitEnclosing(expression, @"\lceil ", @" \rceil");
}
