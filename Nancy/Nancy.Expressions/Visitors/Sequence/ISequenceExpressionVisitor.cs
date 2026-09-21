
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern for sequence expressions.
/// </summary>
/// <remarks>
/// The <c>Visit</c> methods are <c>void</c>, meaning that the visitor only updates an internal state.
/// The semantics to retrieve the result depend on the visitor.
/// </remarks>
public interface ISequenceExpressionVisitor : IExpressionVisitor<MinPlusAlgebra.Sequence>
{
    /// <summary>
    /// Visit method for the type <see cref="ConcreteSequenceExpression"/>
    /// </summary>
    public void Visit(ConcreteSequenceExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceNegateExpression"/>
    /// </summary>
    public void Visit(SequenceNegateExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceFloorExpression"/>
    /// </summary>
    public void Visit(SequenceFloorExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceCeilExpression"/>
    /// </summary>
    public void Visit(SequenceCeilExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceToLeftContinuousExpression"/>
    /// </summary>
    public void Visit(SequenceToLeftContinuousExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceToRightContinuousExpression"/>
    /// </summary>
    public void Visit(SequenceToRightContinuousExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceLowerPseudoInverseExpression"/>
    /// </summary>
    public void Visit(SequenceLowerPseudoInverseExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceUpperPseudoInverseExpression"/>
    /// </summary>
    public void Visit(SequenceUpperPseudoInverseExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceScaleExpression"/>
    /// </summary>
    public void Visit(SequenceScaleExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceDelayExpression"/>
    /// </summary>
    public void Visit(SequenceDelayExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceForwardExpression"/>
    /// </summary>
    public void Visit(SequenceForwardExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceHorizontalShiftExpression"/>
    /// </summary>
    public void Visit(SequenceHorizontalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceVerticalShiftExpression"/>
    /// </summary>
    public void Visit(SequenceVerticalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceCompositionExpression"/>
    /// </summary>
    public void Visit(SequenceCompositionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequencePlaceholderExpression"/>
    /// </summary>
    public void Visit(SequencePlaceholderExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="CurveCutExpression"/>
    /// </summary>
    public void Visit(CurveCutExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="CurveCutExpression"/>
    /// </summary>
    public void Visit(CurveCutToNeighbourhoodExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="CurveCutExpression"/>
    /// </summary>
    public void Visit(SequenceCutExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="CurveCutExpression"/>
    /// </summary>
    public void Visit(SequenceCutToNeighbourhoodExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceAdditionExpression"/>
    /// </summary>
    public void Visit(SequenceAdditionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceSubtractionExpression"/>
    /// </summary>
    public void Visit(SequenceSubtractionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceSubtractionExpression"/>
    /// </summary>
    public void Visit(SequenceToNonNegativeExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMinimumExpression"/>
    /// </summary>
    public void Visit(SequenceMinimumExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMaximumExpression"/>
    /// </summary>
    public void Visit(SequenceMaximumExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceConvolutionExpression"/>
    /// </summary>
    public void Visit(SequenceConvolutionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMaxPlusConvolutionExpression"/>
    /// </summary>
    public void Visit(SequenceMaxPlusConvolutionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceDeconvolutionExpression"/>
    /// </summary>
    public void Visit(SequenceDeconvolutionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMaxPlusDeconvolutionExpression"/>
    /// </summary>
    public void Visit(SequenceMaxPlusDeconvolutionExpression expression);
}

/// <summary>
/// Visitor interface of the Visitor design pattern for sequence expressions.
/// </summary>
/// <typeparam name="TResult">
/// Type of the value produce by the visit.
/// </typeparam>
/// <remarks>
/// All <c>Visit</c> methods compute and return a result of type <typeparamref name="TResult"/>.
/// </remarks>
public interface ISequenceExpressionVisitor<out TResult> : IExpressionVisitor<MinPlusAlgebra.Sequence, TResult>
{
    /// <summary>
    /// Visit method for the type <see cref="ConcreteSequenceExpression"/>
    /// </summary>
    public TResult Visit(ConcreteSequenceExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="CurveCutToNeighbourhoodExpression"/>
    /// </summary>
    public TResult Visit(CurveCutToNeighbourhoodExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceCutExpression"/>
    /// </summary>
    public TResult Visit(SequenceCutExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceCutToNeighbourhoodExpression"/>
    /// </summary>
    public TResult Visit(SequenceCutToNeighbourhoodExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceSubtractionExpression"/>
    /// </summary>
    public TResult Visit(SequenceSubtractionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceToNonNegativeExpression"/>
    /// </summary>
    public TResult Visit(SequenceToNonNegativeExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceNegateExpression"/>
    /// </summary>
    public TResult Visit(SequenceNegateExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceFloorExpression"/>
    /// </summary>
    public TResult Visit(SequenceFloorExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceCeilExpression"/>
    /// </summary>
    public TResult Visit(SequenceCeilExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceToLeftContinuousExpression"/>
    /// </summary>
    public TResult Visit(SequenceToLeftContinuousExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceToRightContinuousExpression"/>
    /// </summary>
    public TResult Visit(SequenceToRightContinuousExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceLowerPseudoInverseExpression"/>
    /// </summary>
    public TResult Visit(SequenceLowerPseudoInverseExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceUpperPseudoInverseExpression"/>
    /// </summary>
    public TResult Visit(SequenceUpperPseudoInverseExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceScaleExpression"/>
    /// </summary>
    public TResult Visit(SequenceScaleExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceDelayExpression"/>
    /// </summary>
    public TResult Visit(SequenceDelayExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceForwardExpression"/>
    /// </summary>
    public TResult Visit(SequenceForwardExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceHorizontalShiftExpression"/>
    /// </summary>
    public TResult Visit(SequenceHorizontalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceVerticalShiftExpression"/>
    /// </summary>
    public TResult Visit(SequenceVerticalShiftExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceCompositionExpression"/>
    /// </summary>
    public TResult Visit(SequenceCompositionExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequencePlaceholderExpression"/>
    /// </summary>
    public TResult Visit(SequencePlaceholderExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="CurveCutExpression"/>
    /// </summary>
    public TResult Visit(CurveCutExpression expression);

    /// <summary>
    /// Visit method for the type <see cref="SequenceAdditionExpression"/>
    /// </summary>
    public TResult Visit(SequenceAdditionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMinimumExpression"/>
    /// </summary>
    public TResult Visit(SequenceMinimumExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMaximumExpression"/>
    /// </summary>
    public TResult Visit(SequenceMaximumExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceConvolutionExpression"/>
    /// </summary>
    public TResult Visit(SequenceConvolutionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMaxPlusConvolutionExpression"/>
    /// </summary>
    public TResult Visit(SequenceMaxPlusConvolutionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceDeconvolutionExpression"/>
    /// </summary>
    public TResult Visit(SequenceDeconvolutionExpression expression);
    
    /// <summary>
    /// Visit method for the type <see cref="SequenceMaxPlusDeconvolutionExpression"/>
    /// </summary>
    public TResult Visit(SequenceMaxPlusDeconvolutionExpression expression);
}
