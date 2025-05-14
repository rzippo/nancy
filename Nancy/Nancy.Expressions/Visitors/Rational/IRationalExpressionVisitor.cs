using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern for curve expressions. 
/// </summary>
/// <remarks>
/// The <c>Visit</c> methods are <c>void</c>, meaning that the visitor only updates an internal state.
/// The semantics to retrieve the result depend on the visitor.
/// </remarks>
public interface IRationalExpressionVisitor : IExpressionVisitor<Rational>
{
    /// <summary>
    /// Visit method for the type <see cref="HorizontalDeviationExpression"/>
    /// </summary>
    public void Visit(HorizontalDeviationExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="VerticalDeviationExpression"/>
    /// </summary>
    public void Visit(VerticalDeviationExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ValueAtExpression"/>
    /// </summary>
    public void Visit(ValueAtExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="LeftLimitAtExpression"/>
    /// </summary>
    public void Visit(LeftLimitAtExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RightLimitAtExpression"/>
    /// </summary>
    public void Visit(RightLimitAtExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalAdditionExpression"/>
    /// </summary>
    public void Visit(RationalAdditionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalSubtractionExpression"/>
    /// </summary>
    public void Visit(RationalSubtractionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalProductExpression"/>
    /// </summary>
    public void Visit(RationalProductExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalDivisionExpression"/>
    /// </summary>
    public void Visit(RationalDivisionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalLeastCommonMultipleExpression"/>
    /// </summary>
    public void Visit(RationalLeastCommonMultipleExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalGreatestCommonDivisorExpression"/>
    /// </summary>
    public void Visit(RationalGreatestCommonDivisorExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalMinimumExpression"/>
    /// </summary>
    public void Visit(RationalMinimumExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalMaximumExpression"/>
    /// </summary>
    public void Visit(RationalMaximumExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalNumberExpression"/>
    /// </summary>
    public void Visit(RationalNumberExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="NegateRationalExpression"/>
    /// </summary>
    public void Visit(NegateRationalExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="InvertRationalExpression"/>
    /// </summary>
    public void Visit(InvertRationalExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalPlaceholderExpression"/>
    /// </summary>
    public void Visit(RationalPlaceholderExpression expression);
}

/// <summary>
/// Visitor interface of the Visitor design pattern for curve expressions. 
/// </summary>
/// <typeparam name="TResult">
/// Type of the value produce by the visit.
/// </typeparam>
/// <remarks>
/// All <c>Visit</c> methods compute and return a result of type <typeparamref name="TResult"/>.
/// </remarks>
public interface IRationalExpressionVisitor<out TResult> : IExpressionVisitor<Rational, TResult>
{
    /// <summary>
    /// Visit method for the type <see cref="HorizontalDeviationExpression"/>
    /// </summary>
    public TResult Visit(HorizontalDeviationExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="VerticalDeviationExpression"/>
    /// </summary>
    public TResult Visit(VerticalDeviationExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ValueAtExpression"/>
    /// </summary>
    public TResult Visit(ValueAtExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="LeftLimitAtExpression"/>
    /// </summary>
    public TResult Visit(LeftLimitAtExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RightLimitAtExpression"/>
    /// </summary>
    public TResult Visit(RightLimitAtExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalAdditionExpression"/>
    /// </summary>
    public TResult Visit(RationalAdditionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalSubtractionExpression"/>
    /// </summary>
    public TResult Visit(RationalSubtractionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalProductExpression"/>
    /// </summary>
    public TResult Visit(RationalProductExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalDivisionExpression"/>
    /// </summary>
    public TResult Visit(RationalDivisionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalLeastCommonMultipleExpression"/>
    /// </summary>
    public TResult Visit(RationalLeastCommonMultipleExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalGreatestCommonDivisorExpression"/>
    /// </summary>
    public TResult Visit(RationalGreatestCommonDivisorExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalMinimumExpression"/>
    /// </summary>
    public TResult Visit(RationalMinimumExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalMaximumExpression"/>
    /// </summary>
    public TResult Visit(RationalMaximumExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalNumberExpression"/>
    /// </summary>
    public TResult Visit(RationalNumberExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="NegateRationalExpression"/>
    /// </summary>
    public TResult Visit(NegateRationalExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="InvertRationalExpression"/>
    /// </summary>
    public TResult Visit(InvertRationalExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="RationalPlaceholderExpression"/>
    /// </summary>
    public TResult Visit(RationalPlaceholderExpression expression);
}