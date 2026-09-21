using System;
using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to compute the value of a sequence expression.
/// </summary>
public class SequenceExpressionEvaluator : ISequenceExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor.
    /// </summary>
    private Sequence _result = Sequence.Zero(0, 1);
    
    /// <summary>
    /// Visits the expression and returns its result.
    /// </summary>
    public Sequence GetResult(SequenceExpression expression)
    {
        expression.Accept(this);
        return _result;
    }

    /// <inheritdoc />
    public void Visit(ConcreteSequenceExpression expression)
        => _result = expression.Value; 

    /// <inheritdoc />
    public void Visit(CurveCutExpression expression)
    {
        // compute, then cut
        var curve = expression.Operand.Value;
                var cs = expression.Interval.Lower;
        var ce = expression.Interval.Upper;
        var csi = expression.Interval.IsLowerIncluded;
        var cei = expression.Interval.IsUpperIncluded;
        
        var cut = curve.Cut(cs, ce, csi, cei);
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(CurveCutToNeighbourhoodExpression expression)
    {
        // compute, then cut
        var curve = expression.Operand.Value;
        var cut = curve.CutToNeighbourhood(expression.CutStart, expression.CutEnd);
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceCutExpression expression)
    {
        // compute, then cut
        var sequence = expression.Operand.Value;
                var cs = expression.Interval.Lower;
        var ce = expression.Interval.Upper;
        var csi = expression.Interval.IsLowerIncluded;
        var cei = expression.Interval.IsUpperIncluded;
        
        var cut = sequence.Cut(cs, ce, csi, cei);
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceCutToNeighbourhoodExpression expression)
    {
        // compute, then cut
        var sequence = expression.Operand.Value;
        var cut = sequence.CutToNeighbourhood(expression.CutStart, expression.CutEnd);
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceAdditionExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => e.Value);
        var addition = sequences
            .Aggregate(Sequence.Addition);
        _result = addition;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceSubtractionExpression expression)
    {
        var a = expression.LeftOperand.Value;
        var b = expression.RightOperand.Value;
        var subtraction = Sequence.Subtraction(a, b);
        _result = subtraction;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceToNonNegativeExpression expression)
    {
        var sequence = expression.Operand.Value;
        var nonNegative = sequence.ToNonNegative();
        _result = nonNegative;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMinimumExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => e.Value);
        var minimum = sequences
            .Aggregate((a, b) => Sequence.Minimum(a, b, settings: expression.Settings?.ComputationSettings));
        _result = minimum;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMaximumExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => e.Value);
        var maximum = sequences
            .Aggregate((a, b) => Sequence.Maximum(a, b, settings: expression.Settings?.ComputationSettings));
        _result = maximum;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceConvolutionExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => e.Value);
        var convolution = sequences
            .Aggregate((a, b) => Sequence.Convolution(a, b, expression.Settings?.ComputationSettings));
        _result = convolution;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMaxPlusConvolutionExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => e.Value);
        var convolution = sequences
            .Aggregate((a, b) => Sequence.MaxPlusConvolution(a, b, expression.Settings?.ComputationSettings));
        _result = convolution;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceDeconvolutionExpression expression)
    {
        var a = expression.LeftOperand.Value;
        var b = expression.RightOperand.Value;
        var deconvolution = Sequence.Deconvolution(a, b, settings: expression.Settings?.ComputationSettings);
        _result = deconvolution;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMaxPlusDeconvolutionExpression expression)
    {
        var a = expression.LeftOperand.Value;
        var b = expression.RightOperand.Value;
        var deconvolution = Sequence.MaxPlusDeconvolution(a, b, expression.Settings?.ComputationSettings);
        _result = deconvolution;
    }

    /// <inheritdoc />
    public void Visit(SequenceNegateExpression expression)
        => _result = expression.Operand.Value.Negate();

    /// <inheritdoc />
    public void Visit(SequenceFloorExpression expression)
        => _result = expression.Operand.Value.Floor();

    /// <inheritdoc />
    public void Visit(SequenceCeilExpression expression)
        => _result = expression.Operand.Value.Ceil();

    /// <inheritdoc />
    public void Visit(SequenceToLeftContinuousExpression expression)
        => _result = expression.Operand.Value.ToLeftContinuous();

    /// <inheritdoc />
    public void Visit(SequenceToRightContinuousExpression expression)
        => _result = expression.Operand.Value.ToRightContinuous();

    /// <inheritdoc />
    public void Visit(SequenceLowerPseudoInverseExpression expression)
        => _result = expression.Operand.Value.LowerPseudoInverse();

    /// <inheritdoc />
    public void Visit(SequenceUpperPseudoInverseExpression expression)
        => _result = expression.Operand.Value.UpperPseudoInverse();

    /// <inheritdoc />
    public void Visit(SequenceScaleExpression expression)
        => _result = expression.LeftOperand.Value.Scale(expression.RightOperand.Value);

    /// <inheritdoc />
    public void Visit(SequenceDelayExpression expression)
        => _result = expression.LeftOperand.Value.Delay(expression.RightOperand.Value);

    /// <inheritdoc />
    public void Visit(SequenceForwardExpression expression)
        => _result = expression.LeftOperand.Value.Forward(expression.RightOperand.Value);

    /// <inheritdoc />
    public void Visit(SequenceHorizontalShiftExpression expression)
        => _result = expression.LeftOperand.Value.HorizontalShift(expression.RightOperand.Value);

    /// <inheritdoc />
    public void Visit(SequenceVerticalShiftExpression expression)
        => _result = expression.LeftOperand.Value.VerticalShift(expression.RightOperand.Value);

    /// <inheritdoc />
    public void Visit(SequenceCompositionExpression expression)
        => _result = Sequence.Composition(expression.LeftOperand.Value, expression.RightOperand.Value);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Always: a placeholder stands for an expression and has no value.</exception>
    public void Visit(SequencePlaceholderExpression expression)
        => throw new InvalidOperationException("A placeholder expression cannot be evaluated.");
}
