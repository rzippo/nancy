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
        => VisitBinaryPrefix(expression, " delay");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceForwardExpression expression)
        => VisitBinaryPrefix(expression, " forward");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceHorizontalShiftExpression expression)
        => VisitBinaryPrefix(expression, " hShift");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceVerticalShiftExpression expression)
        => VisitBinaryPrefix(expression, " vShift");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceNegateExpression expression)
        => VisitUnaryPrefix(expression, "-");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceToNonNegativeExpression expression)
        => VisitUnaryPostfix(expression, "^{+}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceToLeftContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{l}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceToRightContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{r}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceLowerPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\underline{-1}}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceUpperPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\overline{-1}}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCutExpression expression)
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
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCutToNeighbourhoodExpression expression)
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
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(CurveCutExpression expression)
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
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(CurveCutToNeighbourhoodExpression expression)
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
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceValueAtExpression expression)
        => VisitBinaryPrefix(expression, " value");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceLeftLimitAtExpression expression)
        => VisitBinaryPrefix(expression, " leftLimit");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceRightLimitAtExpression expression)
        => VisitBinaryPrefix(expression, " rightLimit");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceHorizontalDeviationExpression expression)
        => VisitBinaryPrefix(expression, @" hdev");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceVerticalDeviationExpression expression)
        => VisitBinaryPrefix(expression, @" vdev");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceFloorExpression expression)
        => VisitUnaryPrefix(expression, "floor");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SequenceCeilExpression expression)
        => VisitUnaryPrefix(expression, "ceil");
}
