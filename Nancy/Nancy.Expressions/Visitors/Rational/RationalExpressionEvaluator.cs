using System.Numerics;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to compute the value of a rational expression.
/// </summary>
public record RationalExpressionEvaluator : IRationalExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    private Rational _result = Rational.Zero;

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
    public RationalExpressionEvaluator(ExpressionSettings? settings = null)
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
    public Rational GetResult(RationalExpression expression)
    {
        expression.Accept(this);
        return _result;
    }

    /// <inheritdoc />
    public virtual void Visit(HorizontalDeviationExpression expression)
        => _result = Curve.HorizontalDeviation(Read(expression.LeftOperand), Read(expression.RightOperand),
            ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(VerticalDeviationExpression expression)
        => _result = Curve.VerticalDeviation(Read(expression.LeftOperand), Read(expression.RightOperand),
            ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(SequenceHorizontalDeviationExpression expression)
        => _result = Sequence.HorizontalDeviation(Read(expression.LeftOperand), Read(expression.RightOperand),
            ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(SequenceVerticalDeviationExpression expression)
        => _result = Sequence.VerticalDeviation(Read(expression.LeftOperand), Read(expression.RightOperand),
            ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(SequenceValueAtExpression expression)
        => _result = Read(expression.LeftOperand).ValueAt(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(SequenceLeftLimitAtExpression expression)
        => _result = Read(expression.LeftOperand).LeftLimitAt(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(SequenceRightLimitAtExpression expression)
        => _result = Read(expression.LeftOperand).RightLimitAt(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(ZDeviationExpression expression)
        => _result = Curve.ZDeviation(Read(expression.LeftOperand), Read(expression.RightOperand),
            ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(ValueAtExpression expression)
        => _result = Read(expression.LeftOperand).ValueAt(Read(expression.RightOperand),
            ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(LeftLimitAtExpression expression)
        => _result = Read(expression.LeftOperand).LeftLimitAt(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(RightLimitAtExpression expression)
        => _result = Read(expression.LeftOperand).RightLimitAt(Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(RationalAdditionExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.Zero, (current, e) => current + Read(e));

    /// <inheritdoc />
    public virtual void Visit(RationalSubtractionExpression expression)
        => _result = Read(expression.LeftOperand) - Read(expression.RightOperand);
    
    /// <inheritdoc />
    public virtual void Visit(RationalProductExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.One, (current, e) => current * Read(e));

    /// <inheritdoc />
    public virtual void Visit(RationalDivisionExpression expression)
        => _result = Read(expression.LeftOperand) / Read(expression.RightOperand);

    /// <inheritdoc />
    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => _result = expression.FlattenOperands()
            .Select(e => Read(e) )
            .Aggregate((current, next) => Rational.LeastCommonMultiple(current, next));

    /// <inheritdoc />
    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => _result = expression.FlattenOperands()
            .Select(e => Read(e) )
            .Aggregate((current, next) => Rational.GreatestCommonDivisor(current, next));

    /// <inheritdoc />
    public virtual void Visit(RationalMinimumExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.PlusInfinity, (current, e) => Rational.Min(current, Read(e)));
    
    /// <inheritdoc />
    public virtual void Visit(RationalMaximumExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.MinusInfinity, (current, e) => Rational.Max(current, Read(e)));
    
    /// <inheritdoc />
    public virtual void Visit(RationalNumberExpression expression) => _result = expression.Value;

    /// <inheritdoc />
    public virtual void Visit(NegateRationalExpression expression) => _result = Rational.Negate(Read(expression.Operand));

    /// <inheritdoc />
    public virtual void Visit(InvertRationalExpression expression) => _result = Rational.Invert(Read(expression.Operand));

    /// <inheritdoc />
    public virtual void Visit(RationalAbsoluteValueExpression expression) => _result = Rational.Abs(Read(expression.Operand));

    /// <inheritdoc />
    public virtual void Visit(RationalModuloExpression expression) => _result = Read(expression.LeftOperand) % Read(expression.RightOperand);

    /// <inheritdoc />
    public virtual void Visit(RationalPowerExpression expression) => _result = Rational.Pow(Read(expression.LeftOperand), (System.Numerics.BigInteger)Read(expression.RightOperand));

    /// <inheritdoc />
    public virtual void Visit(RationalPlaceholderExpression expression)
        => throw new InvalidOperationException("Can't evaluate an expression with placeholders!");

    /// <inheritdoc />
    public virtual void Visit(RationalFloorExpression expression) => _result = Read(expression.Operand).Floor();

    /// <inheritdoc />
    public virtual void Visit(RationalCeilExpression expression) => _result = Read(expression.Operand).Ceil();

    /// <inheritdoc />
    public virtual void Visit(SupValueExpression expression)
        => _result = Read(expression.Operand).SupValue(ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(InfValueExpression expression)
        => _result = Read(expression.Operand).InfValue(ComputationSettingsOf(expression.Settings));

    /// <inheritdoc />
    public virtual void Visit(MaxValueExpression expression)
        => _result = Read(expression.Operand).MaxValue(ComputationSettingsOf(expression.Settings)) ??
                     throw new InvalidOperationException(
                         "The curve does not attain a maximum value (its supremum is not attained); use SupValue() instead.");

    /// <inheritdoc />
    public virtual void Visit(MinValueExpression expression)
        => _result = Read(expression.Operand).MinValue(ComputationSettingsOf(expression.Settings)) ??
                     throw new InvalidOperationException(
                         "The curve does not attain a minimum value (its infimum is not attained); use InfValue() instead.");
}