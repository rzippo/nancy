using Unipi.Nancy.Expressions.Nodes;
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
    /// The settings the computation was asked for with, carried down to every operand.
    /// </summary>
    private readonly EvaluationContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="settings">
    /// Settings to compute the whole expression under, as given to <see cref="IGenericExpression{TExpressionResult}.Compute"/>.
    /// When omitted, each node computes under its own.
    /// </param>
    public CurveExpressionEvaluator(ExpressionSettings? settings = null)
    {
        _context = new EvaluationContext(settings);
    }

    /// <summary>
    /// The computation settings for a node whose own settings are <paramref name="own"/>.
    /// </summary>
    private ComputationSettings? ComputationSettingsOf(ExpressionSettings? own)
        => _context.ComputationSettingsOf(own);

    /// <summary>
    /// Computes <paramref name="operand"/> under the settings this computation was asked for with.
    /// </summary>
    private T Read<T>(IGenericExpression<T> operand)
        => _context.Read(operand);

    /// <summary>
    /// Clears the values of the operands read, if <paramref name="cacheSettings"/> asks for it.
    /// </summary>
    /// <inheritdoc cref="EvaluationContext.ReleaseOperands" path="/param"/>
    internal void ReleaseOperands(CacheSettings? cacheSettings)
        => _context.ReleaseOperands(cacheSettings);

    /// <summary>
    /// Visits the expression and returns tht result
    /// </summary>
    public Curve GetResult(CurveExpression expression)
    {
        expression.Accept(this);
        return _result;
    }

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression) => _result = expression.Value;

    private void VisitUnary(CurveUnaryExpression<Curve> expression, Func<Curve, Curve> operation)
        => _result = operation(Read(expression.Operand));

    private void VisitBinary(CurveBinaryExpression<Curve, Curve> expression,
        Func<Curve, Curve, Curve> operation)
        => _result = operation(Read(expression.LeftOperand), Read(expression.RightOperand));

    private void VisitNAry(CurveNAryExpression expression, Func<IReadOnlyCollection<Curve>, Curve> operation)
    {
        List<Curve> curves = [];
        curves.AddRange(expression.FlattenOperands().Select(e => Read(e)));

        _result = operation(curves);
    }

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression)
        => VisitUnary(expression, curve => curve.Negate());

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression)
        => VisitUnary(expression, curve => curve.ToNonNegative());

    /// <inheritdoc />
    public virtual void Visit(SubAdditiveClosureExpression expression)
        => VisitUnary(expression, curve => curve.SubAdditiveClosure(ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => VisitUnary(expression, curve => curve.SuperAdditiveClosure(ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => VisitUnary(expression, curve => curve.ToUpperNonDecreasing(ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => VisitUnary(expression, curve => curve.ToLowerNonDecreasing(ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonIncreasingExpression expression)
        => VisitUnary(expression, curve => curve.ToUpperNonIncreasing(ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonIncreasingExpression expression)
        => VisitUnary(expression, curve => curve.ToLowerNonIncreasing(ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(ToLeftContinuousExpression expression)
        => VisitUnary(expression, curve => curve.ToLeftContinuous());

    /// <inheritdoc />
    public virtual void Visit(ToRightContinuousExpression expression)
        => VisitUnary(expression, curve => curve.ToRightContinuous());

    /// <inheritdoc />
    public virtual void Visit(WithZeroOriginExpression expression)
        => VisitUnary(expression, curve => curve.WithZeroOrigin());

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
        => VisitUnary(expression, curve => curve.LowerPseudoInverse());

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
        => VisitUnary(expression, curve => curve.UpperPseudoInverse());

    /// <inheritdoc />
    public virtual void Visit(AdditionExpression expression)
        => VisitNAry(expression, curves => Curve.Addition(curves, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression)
#pragma warning disable CS0618 // Type or member is obsolete
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.Subtraction(leftCurve, rightCurve,
            expression.NonNegative, ComputationSettingsOf(expression.Settings)));
#pragma warning restore CS0618 // Type or member is obsolete

    /// <inheritdoc />
    public virtual void Visit(MinimumExpression expression)
        => VisitNAry(expression, curves => Curve.Minimum(curves, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(MaximumExpression expression)
        => VisitNAry(expression, curves => Curve.Maximum(curves, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(ConvolutionExpression expression)
        => VisitNAry(expression, curves => Curve.Convolution(curves, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(DeconvolutionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.Deconvolution(leftCurve, rightCurve, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => VisitNAry(expression, curves => Curve.MaxPlusConvolution(curves, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.MaxPlusDeconvolution(leftCurve, rightCurve, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(CompositionExpression expression)
        => VisitBinary(expression, (leftCurve, rightCurve) => Curve.Composition(leftCurve, rightCurve, ComputationSettingsOf(expression.Settings)));

    /// <inheritdoc />
    public virtual void Visit(DelayByExpression expression)
        => _result = Read(expression.LeftOperand).DelayBy(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(ForwardByExpression expression)
        => _result = Read(expression.LeftOperand).ForwardBy(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => _result = Read(expression.LeftOperand).HorizontalShift(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(VerticalShiftExpression expression)
        => _result = Read(expression.LeftOperand).VerticalShift(Read(expression.RightOperand), false);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException("Can't evaluate an expression with placeholders!");

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
        => _result = Read(expression.LeftOperand).Scale(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(WithOriginAtExpression expression)
        => _result = Read(expression.Operand).WithOriginAt(expression.OriginValue);

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => VisitUnary(expression, curve => curve.Floor());

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => VisitUnary(expression, curve => curve.Ceil());
}