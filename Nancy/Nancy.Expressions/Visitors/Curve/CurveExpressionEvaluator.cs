using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to compute the value of a curve expression.
/// </summary>
public record CurveExpressionEvaluator : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    private Curve _result = Curve.Zero();

    /// <summary>
    /// Visits the expression and returns tht result
    /// </summary>
    public Curve GetResult(CurveExpression expression)
    {
        expression.Accept(this);
        return _result;
    }

    public virtual void Visit(ConcreteCurveExpression expression) => _result = expression.Value;

    private void VisitUnary(CurveUnaryExpression<Curve> expression, Func<Curve, Curve> operation)
        => _result = operation(expression.Expression.Value);

    private void VisitBinary(CurveBinaryExpression<Curve, Curve> expression,
        Func<Curve, Curve, Curve> operation)
        => _result = operation(expression.LeftExpression.Value, expression.RightExpression.Value);

    private void VisitNAry(CurveNAryExpression expression, Func<IReadOnlyCollection<Curve>, Curve> operation)
    {
        List<Curve> curves = [];
        curves.AddRange(expression.Expressions.Select(e => e.Value));

        _result = operation(curves);
    }

    public virtual void Visit(NegateExpression expression)
        => VisitUnary(expression, curve => curve.Negate());

    public virtual void Visit(ToNonNegativeExpression expression)
        => VisitUnary(expression, curve => curve.ToNonNegative());

    public virtual void Visit(SubAdditiveClosureExpression expression)
        => VisitUnary(expression, curve => curve.SubAdditiveClosure(expression.Settings?.ComputationSettings));

    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => VisitUnary(expression, curve => curve.SuperAdditiveClosure(expression.Settings?.ComputationSettings));

    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => VisitUnary(expression, curve => curve.ToUpperNonDecreasing());

    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => VisitUnary(expression, curve => curve.ToLowerNonDecreasing());

    public virtual void Visit(ToLeftContinuousExpression expression)
        => VisitUnary(expression, curve => curve.ToLeftContinuous());

    public virtual void Visit(ToRightContinuousExpression expression)
        => VisitUnary(expression, curve => curve.ToRightContinuous());

    public virtual void Visit(WithZeroOriginExpression expression)
        => VisitUnary(expression, curve => curve.WithZeroOrigin());

    public virtual void Visit(LowerPseudoInverseExpression expression)
        => VisitUnary(expression, curve => curve.LowerPseudoInverse());

    public virtual void Visit(UpperPseudoInverseExpression expression)
        => VisitUnary(expression, curve => curve.UpperPseudoInverse());

    public virtual void Visit(AdditionExpression expression)
        => VisitNAry(expression, curves => Curve.Addition(curves, expression.Settings?.ComputationSettings));

    public virtual void Visit(SubtractionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.Subtraction(leftCurve, rightCurve, expression.NonNegative));

    public virtual void Visit(MinimumExpression expression)
        => VisitNAry(expression, curves => Curve.Minimum(curves, expression.Settings?.ComputationSettings));

    public virtual void Visit(MaximumExpression expression)
        => VisitNAry(expression, curves => Curve.Maximum(curves, expression.Settings?.ComputationSettings));

    public virtual void Visit(ConvolutionExpression expression)
        => VisitNAry(expression, curves => Curve.Convolution(curves, expression.Settings?.ComputationSettings));

    public virtual void Visit(DeconvolutionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.Deconvolution(leftCurve, rightCurve, expression.Settings?.ComputationSettings));

    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => VisitNAry(expression, curves => Curve.MaxPlusConvolution(curves, expression.Settings?.ComputationSettings));

    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.MaxPlusDeconvolution(leftCurve, rightCurve, expression.Settings?.ComputationSettings));

    public virtual void Visit(CompositionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.Composition(leftCurve, rightCurve, expression.Settings?.ComputationSettings));

    public virtual void Visit(DelayByExpression expression)
        => _result = expression.LeftExpression.Value.DelayBy(expression.RightExpression.Value);

    public virtual void Visit(AnticipateByExpression expression)
        => _result = expression.LeftExpression.Value.AnticipateBy(expression.RightExpression.Value);

    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException("Can't evaluate an expression with placeholders!");

    public virtual void Visit(ScaleExpression expression)
        => _result = expression.LeftExpression.Value.Scale(expression.RightExpression.Value);
}