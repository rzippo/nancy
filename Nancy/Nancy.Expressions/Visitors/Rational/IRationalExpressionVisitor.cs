using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern for curve expressions. 
/// </summary>
public interface IRationalExpressionVisitor : IExpressionVisitor
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