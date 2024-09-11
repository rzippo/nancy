using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern for curve expressions. 
/// </summary>
public interface ICurveExpressionVisitor : IExpressionVisitor
{
    /// <summary>
    /// Visit method for the type <see cref="ConcreteCurveExpression"/>
    /// </summary>
    public void Visit(ConcreteCurveExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="NegateExpression"/>
    /// </summary>
    public void Visit(NegateExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ToNonNegativeExpression"/>
    /// </summary>
    public void Visit(ToNonNegativeExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="SubAdditiveClosureExpression"/>
    /// </summary>
    public void Visit(SubAdditiveClosureExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="SuperAdditiveClosureExpression"/>
    /// </summary>
    public void Visit(SuperAdditiveClosureExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ToUpperNonDecreasingExpression"/>
    /// </summary>
    public void Visit(ToUpperNonDecreasingExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ToLowerNonDecreasingExpression"/>
    /// </summary>
    public void Visit(ToLowerNonDecreasingExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ToLeftContinuousExpression"/>
    /// </summary>
    public void Visit(ToLeftContinuousExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ToRightContinuousExpression"/>
    /// </summary>
    public void Visit(ToRightContinuousExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="WithZeroOriginExpression"/>
    /// </summary>
    public void Visit(WithZeroOriginExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="LowerPseudoInverseExpression"/>
    /// </summary>
    public void Visit(LowerPseudoInverseExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="UpperPseudoInverseExpression"/>
    /// </summary>
    public void Visit(UpperPseudoInverseExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="AdditionExpression"/>
    /// </summary>
    public void Visit(AdditionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="SubtractionExpression"/>
    /// </summary>
    public void Visit(SubtractionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ConcreteCurveExpression"/>
    /// </summary>
    public void Visit(MinimumExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="MaximumExpression"/>
    /// </summary>
    public void Visit(MaximumExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ConvolutionExpression"/>
    /// </summary>
    public void Visit(ConvolutionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="DeconvolutionExpression"/>
    /// </summary>
    public void Visit(DeconvolutionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="MaxPlusConvolutionExpression"/>
    /// </summary>
    public void Visit(MaxPlusConvolutionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="MaxPlusDeconvolutionExpression"/>
    /// </summary>
    public void Visit(MaxPlusDeconvolutionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="CompositionExpression"/>
    /// </summary>
    public void Visit(CompositionExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="DelayByExpression"/>
    /// </summary>
    public void Visit(DelayByExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="AnticipateByExpression"/>
    /// </summary>
    public void Visit(AnticipateByExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="CurvePlaceholderExpression"/>
    /// </summary>
    public void Visit(CurvePlaceholderExpression expression);
    /// <summary>
    /// Visit method for the type <see cref="ScaleExpression"/>
    /// </summary>
    public void Visit(ScaleExpression expression);
}