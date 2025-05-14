using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern for curve expressions. 
/// </summary>
/// <remarks>
/// The <c>Visit</c> methods are <c>void</c>, meaning that the visitor only updates an internal state.
/// The semantics to retrieve the result depend on the visitor.
/// </remarks>
public interface ICurveExpressionVisitor : IExpressionVisitor<Curve>
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
    /// Visit method for the type <see cref="ForwardByExpression"/>
    /// </summary>
    public void Visit(ForwardByExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="HorizontalShiftExpression"/>
    /// </summary>
    public void Visit(HorizontalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="VerticalShiftExpression"/>
    /// </summary>
    public void Visit(VerticalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="CurvePlaceholderExpression"/>
    /// </summary>
    public void Visit(CurvePlaceholderExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ScaleExpression"/>
    /// </summary>
    public void Visit(ScaleExpression expression);
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
public interface ICurveExpressionVisitor<out TResult> : IExpressionVisitor<Curve, TResult>
{
    /// <summary>
    /// Visit method for the type <see cref="ConcreteCurveExpression"/>
    /// </summary>
    public TResult Visit(ConcreteCurveExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="NegateExpression"/>
    /// </summary>
    public TResult Visit(NegateExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ToNonNegativeExpression"/>
    /// </summary>
    public TResult Visit(ToNonNegativeExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SubAdditiveClosureExpression"/>
    /// </summary>
    public TResult Visit(SubAdditiveClosureExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SuperAdditiveClosureExpression"/>
    /// </summary>
    public TResult Visit(SuperAdditiveClosureExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ToUpperNonDecreasingExpression"/>
    /// </summary>
    public TResult Visit(ToUpperNonDecreasingExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ToLowerNonDecreasingExpression"/>
    /// </summary>
    public TResult Visit(ToLowerNonDecreasingExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ToLeftContinuousExpression"/>
    /// </summary>
    public TResult Visit(ToLeftContinuousExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ToRightContinuousExpression"/>
    /// </summary>
    public TResult Visit(ToRightContinuousExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="WithZeroOriginExpression"/>
    /// </summary>
    public TResult Visit(WithZeroOriginExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="LowerPseudoInverseExpression"/>
    /// </summary>
    public TResult Visit(LowerPseudoInverseExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="UpperPseudoInverseExpression"/>
    /// </summary>
    public TResult Visit(UpperPseudoInverseExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="AdditionExpression"/>
    /// </summary>
    public TResult Visit(AdditionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SubtractionExpression"/>
    /// </summary>
    public TResult Visit(SubtractionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ConcreteCurveExpression"/>
    /// </summary>
    public TResult Visit(MinimumExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="MaximumExpression"/>
    /// </summary>
    public TResult Visit(MaximumExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ConvolutionExpression"/>
    /// </summary>
    public TResult Visit(ConvolutionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="DeconvolutionExpression"/>
    /// </summary>
    public TResult Visit(DeconvolutionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="MaxPlusConvolutionExpression"/>
    /// </summary>
    public TResult Visit(MaxPlusConvolutionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="MaxPlusDeconvolutionExpression"/>
    /// </summary>
    public TResult Visit(MaxPlusDeconvolutionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="CompositionExpression"/>
    /// </summary>
    public TResult Visit(CompositionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="DelayByExpression"/>
    /// </summary>
    public TResult Visit(DelayByExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ForwardByExpression"/>
    /// </summary>
    public TResult Visit(ForwardByExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="VerticalShiftExpression"/>
    /// </summary>
    public TResult Visit(VerticalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="CurvePlaceholderExpression"/>
    /// </summary>
    public TResult Visit(CurvePlaceholderExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="ScaleExpression"/>
    /// </summary>
    public TResult Visit(ScaleExpression expression);
}