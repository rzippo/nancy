using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: is this redundant?

/// <summary>
/// Visitor class used to update the name of a curve expression.
/// </summary>
public class ChangeNameCurveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public CurveExpression Result = Expressions.FromCurve(Curve.Zero());

    public string NewName { get; init; }

    /// <summary>
    /// Visitor class used to update the name of a curve expression.
    /// </summary>
    /// <param name="newName">The new name of the expression</param>
    public ChangeNameCurveVisitor(string newName)
    {
        NewName = newName;
    }

    public virtual void Visit(ConcreteCurveExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(NegateExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ToNonNegativeExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(SubAdditiveClosureExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ToLeftContinuousExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ToRightContinuousExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(WithZeroOriginExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(LowerPseudoInverseExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(UpperPseudoInverseExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(AdditionExpression expression)
        => Result = expression with { Name = NewName };


    public virtual void Visit(SubtractionExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(MinimumExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(MaximumExpression expression)
        => Result = expression with { Name = NewName };


    public virtual void Visit(ConvolutionExpression expression)
        => Result = expression with { Name = NewName };


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

    public void Visit(ShiftExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(CurvePlaceholderExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(ScaleExpression expression)
        => Result = expression with { Name = NewName };
}