using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: is this redundant?

/// <summary>
/// Visitor class used to change the name of a rational expression.
/// </summary>
public class RenameRationalVisitor : IRationalExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public RationalExpression Result = Expressions.FromRational(Rational.Zero);

    /// <summary>
    /// The new name of the expression, or <see langword="null"/> to keep the visited expression's own.
    /// </summary>
    public string? NewName { get; init; }

    /// <summary>
    /// The new generation of the expression, or <see langword="null"/> to keep the visited expression's own.
    /// </summary>
    public int? NewGeneration { get; init; }

    /// <summary>
    /// Visitor class used to change the name and/or generation of a rational expression.
    /// </summary>
    /// <param name="newName">The new name of the expression, or <see langword="null"/> to keep it as-is.</param>
    /// <param name="newGeneration">The new generation of the expression, or <see langword="null"/> to keep it as-is.</param>
    public RenameRationalVisitor(string? newName = null, int? newGeneration = null)
    {
        NewName = newName;
        NewGeneration = newGeneration;
    }

    private void CommonVisit(RationalExpression expression)
    {
        Result = expression with
        {
            Name = NewName ?? expression.Name,
            Generation = NewGeneration ?? expression.Generation,
            // Since we know renaming does not change the result,
            // it is safe to explicitly copy over the cache fields
            _value = Volatile.Read(ref expression._value) is { IsValueCreated: true } cache_value ? cache_value : null,
        };
    }

    /// <inheritdoc />
    public virtual void Visit(HorizontalDeviationExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceHorizontalDeviationExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceVerticalDeviationExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceValueAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceLeftLimitAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceRightLimitAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(VerticalDeviationExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ZDeviationExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ValueAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(LeftLimitAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RightLimitAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalAdditionExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalSubtractionExpression expression)
        => CommonVisit(expression);
    
    /// <inheritdoc />
    public virtual void Visit(RationalProductExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalDivisionExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalMinimumExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalMaximumExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalNumberExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(NegateRationalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(InvertRationalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalAbsoluteValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalModuloExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalPowerExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalPlaceholderExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalFloorExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(RationalCeilExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SupValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(InfValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MaxValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MinValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SupArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(InfArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MaxArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MinArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SupValueOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SupArgOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(InfValueOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(InfArgOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MaxValueOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MaxArgOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MinValueOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MinArgOverIntervalExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceSupValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceSupArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceInfValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceInfArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMaxValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMaxArgExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMinValueExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMinArgExpression expression)
        => CommonVisit(expression);
}