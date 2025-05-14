using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: is this redundant?

/// <summary>
/// Visitor class used to change the name of a curve expression.
/// </summary>
public class RenameCurveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public CurveExpression Result = Expressions.FromCurve(Curve.Zero());

    public string NewName { get; init; }

    /// <summary>
    /// Visitor class used to change the name of a curve expression.
    /// </summary>
    /// <param name="newName">The new name of the expression</param>
    public RenameCurveVisitor(string newName)
    {
        NewName = newName;
    }

    private void CommonVisit(CurveExpression expression)
    {
        Result = expression with
        {
            Name = NewName,
            // Since we know renaming does not change the result,
            // it is safe to explicitly copy over the cache fields
            _value = expression.Value,
            _isSubAdditive = expression.IsSubAdditive,
        };
    }

    public virtual void Visit(ConcreteCurveExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(NegateExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(ToNonNegativeExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(SubAdditiveClosureExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(ToLeftContinuousExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(ToRightContinuousExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(WithZeroOriginExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(LowerPseudoInverseExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(UpperPseudoInverseExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(AdditionExpression expression)
        => CommonVisit(expression);


    public virtual void Visit(SubtractionExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(MinimumExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(MaximumExpression expression)
        => CommonVisit(expression);


    public virtual void Visit(ConvolutionExpression expression)
        => CommonVisit(expression);


    public virtual void Visit(DeconvolutionExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => Result = expression with { Name = NewName };


    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(CompositionExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(DelayByExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ForwardByExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(HorizontalShiftExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(VerticalShiftExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(CurvePlaceholderExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ScaleExpression expression)
        => Result = expression with { Name = NewName };
}