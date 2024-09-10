using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to update the name of a curve expression.
/// </summary>
/// <param name="newName">The new name of the expression</param>
public class ChangeNameCurveVisitor(string newName) : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public CurveExpression Result = Expressions.FromCurve(Curve.Zero());

    public virtual void Visit(ConcreteCurveExpression expression)
        => Result = Expressions.FromCurve(expression.Value, newName);

    public virtual void Visit(NegateExpression expression)
        => Result = Expressions.Negate((CurveExpression)expression.Expression, newName);

    public virtual void Visit(ToNonNegativeExpression expression)
        => Result = Expressions.ToNonNegative((CurveExpression)expression.Expression, newName);

    public virtual void Visit(SubAdditiveClosureExpression expression)
        => Result = Expressions.SubAdditiveClosure((CurveExpression)expression.Expression, newName);

    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => Result = Expressions.SuperAdditiveClosure((CurveExpression)expression.Expression, newName);

    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => Result = Expressions.ToUpperNonDecreasing((CurveExpression)expression.Expression, newName);

    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => Result = Expressions.ToLowerNonDecreasing((CurveExpression)expression.Expression, newName);

    public virtual void Visit(ToLeftContinuousExpression expression)
        => Result = Expressions.ToLeftContinuous((CurveExpression)expression.Expression, newName);

    public virtual void Visit(ToRightContinuousExpression expression)
        => Result = Expressions.ToRightContinuous((CurveExpression)expression.Expression, newName);

    public virtual void Visit(WithZeroOriginExpression expression)
        => Result = Expressions.WithZeroOrigin((CurveExpression)expression.Expression, newName);

    public virtual void Visit(LowerPseudoInverseExpression expression)
        => Result = Expressions.LowerPseudoInverse((CurveExpression)expression.Expression, newName);

    public virtual void Visit(UpperPseudoInverseExpression expression)
        => Result = Expressions.UpperPseudoInverse((CurveExpression)expression.Expression, newName);

    public virtual void Visit(AdditionExpression expression)
        => Result = new AdditionExpression(expression.Expressions, newName);


    public virtual void Visit(SubtractionExpression expression)
        => Result = Expressions.Subtraction((CurveExpression)expression.LeftExpression,
            (CurveExpression)expression.RightExpression, newName);

    public virtual void Visit(MinimumExpression expression)
        => Result = new MinimumExpression(expression.Expressions, newName);

    public virtual void Visit(MaximumExpression expression)
        => Result = new MaximumExpression(expression.Expressions, newName);


    public virtual void Visit(ConvolutionExpression expression)
        => Result = new ConvolutionExpression(expression.Expressions, newName);


    public virtual void Visit(DeconvolutionExpression expression)
        => Result = Expressions.Deconvolution((CurveExpression)expression.LeftExpression,
            (CurveExpression)expression.RightExpression, newName);

    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => Result = new MaxPlusConvolutionExpression(expression.Expressions, newName);


    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => Result = Expressions.MaxPlusDeconvolution((CurveExpression)expression.LeftExpression,
            (CurveExpression)expression.RightExpression, newName);

    public virtual void Visit(CompositionExpression expression)
        => Result = Expressions.Composition((CurveExpression)expression.LeftExpression,
            (CurveExpression)expression.RightExpression, newName);

    public virtual void Visit(DelayByExpression expression)
        => Result = Expressions.DelayBy((CurveExpression)expression.LeftExpression,
            (RationalExpression)expression.RightExpression, newName);

    public virtual void Visit(AnticipateByExpression expression)
        => Result = Expressions.AnticipateBy((CurveExpression)expression.LeftExpression,
            (RationalExpression)expression.RightExpression, newName);

    public virtual void Visit(CurvePlaceholderExpression expression)
        => Result = new CurvePlaceholderExpression(newName);

    public virtual void Visit(ScaleExpression expression)
        => Result = Expressions.Scale((CurveExpression)expression.LeftExpression,
            (RationalExpression)expression.RightExpression, newName);
}