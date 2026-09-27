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
    public SequenceExpressionEvaluator(ExpressionSettings? settings = null)
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
        var curve = Read(expression.Operand);
        var cs = expression.Interval.Lower;
        var ce = expression.Interval.Upper;
        var csi = expression.Interval.IsLowerIncluded;
        var cei = expression.Interval.IsUpperIncluded;

        var cut = curve.Cut(cs, ce, csi, cei, ComputationSettingsOf(expression.Settings));
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(CurveCutToNeighbourhoodExpression expression)
    {
        // compute, then cut
        var curve = Read(expression.Operand);
        var cut = curve.CutToNeighbourhood(expression.CutStart, expression.CutEnd,
            settings: ComputationSettingsOf(expression.Settings));
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceCutExpression expression)
    {
        // compute, then cut
        var sequence = Read(expression.Operand);
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
        var sequence = Read(expression.Operand);
        var cut = sequence.CutToNeighbourhood(expression.CutStart, expression.CutEnd);
        _result = cut;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceAdditionExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => Read(e));
        var addition = sequences
            .Aggregate(Sequence.Addition);
        _result = addition;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceSubtractionExpression expression)
    {
        var a = Read(expression.LeftOperand);
        var b = Read(expression.RightOperand);
        var subtraction = Sequence.Subtraction(a, b);
        _result = subtraction;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceConcatExpression expression)
    {
        var a = Read(expression.LeftOperand);
        var b = Read(expression.RightOperand);
        _result = Sequence.Concat(a, b, expression.PreserveDelay, expression.PreserveShift);
    }

    /// <inheritdoc />
    public void Visit(SequenceToNonNegativeExpression expression)
    {
        var sequence = Read(expression.Operand);
        var nonNegative = sequence.ToNonNegative();
        _result = nonNegative;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMinimumExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => Read(e));
        var minimum = sequences
            .Aggregate((a, b) => Sequence.Minimum(a, b, settings: ComputationSettingsOf(expression.Settings)));
        _result = minimum;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMaximumExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => Read(e));
        var maximum = sequences
            .Aggregate((a, b) => Sequence.Maximum(a, b, settings: ComputationSettingsOf(expression.Settings)));
        _result = maximum;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceConvolutionExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => Read(e));
        var convolution = sequences
            .Aggregate((a, b) => Sequence.Convolution(a, b, ComputationSettingsOf(expression.Settings)));
        _result = convolution;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMaxPlusConvolutionExpression expression)
    {
        var sequences = expression.Operands
            .Select(e => Read(e));
        var convolution = sequences
            .Aggregate((a, b) => Sequence.MaxPlusConvolution(a, b, ComputationSettingsOf(expression.Settings)));
        _result = convolution;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceDeconvolutionExpression expression)
    {
        var a = Read(expression.LeftOperand);
        var b = Read(expression.RightOperand);
        var deconvolution = Sequence.Deconvolution(a, b, settings: ComputationSettingsOf(expression.Settings));
        _result = deconvolution;
    }
    
    /// <inheritdoc />
    public void Visit(SequenceMaxPlusDeconvolutionExpression expression)
    {
        var a = Read(expression.LeftOperand);
        var b = Read(expression.RightOperand);
        var deconvolution = Sequence.MaxPlusDeconvolution(a, b, ComputationSettingsOf(expression.Settings));
        _result = deconvolution;
    }

    /// <inheritdoc />
    public void Visit(SequenceNegateExpression expression)
        => _result = Read(expression.Operand).Negate();

    /// <inheritdoc />
    public void Visit(SequenceFloorExpression expression)
        => _result = Read(expression.Operand).Floor();

    /// <inheritdoc />
    public void Visit(SequenceCeilExpression expression)
        => _result = Read(expression.Operand).Ceil();

    /// <inheritdoc />
    public void Visit(SequenceToLeftContinuousExpression expression)
        => _result = Read(expression.Operand).ToLeftContinuous();

    /// <inheritdoc />
    public void Visit(SequenceToRightContinuousExpression expression)
        => _result = Read(expression.Operand).ToRightContinuous();

    /// <inheritdoc />
    public void Visit(SequenceLowerPseudoInverseExpression expression)
        => _result = Read(expression.Operand).LowerPseudoInverse();

    /// <inheritdoc />
    public void Visit(SequenceUpperPseudoInverseExpression expression)
        => _result = Read(expression.Operand).UpperPseudoInverse();

    /// <inheritdoc />
    public void Visit(SequenceScaleExpression expression)
        => _result = Read(expression.LeftOperand).Scale(Read(expression.RightOperand));

    /// <inheritdoc />
    public void Visit(SequenceDelayExpression expression)
        => _result = Read(expression.LeftOperand).Delay(Read(expression.RightOperand));

    /// <inheritdoc />
    public void Visit(SequenceForwardExpression expression)
        => _result = Read(expression.LeftOperand).Forward(Read(expression.RightOperand));

    /// <inheritdoc />
    public void Visit(SequenceHorizontalShiftExpression expression)
        => _result = Read(expression.LeftOperand).HorizontalShift(Read(expression.RightOperand));

    /// <inheritdoc />
    public void Visit(SequenceVerticalShiftExpression expression)
        => _result = Read(expression.LeftOperand).VerticalShift(Read(expression.RightOperand));

    /// <inheritdoc />
    public void Visit(SequenceCompositionExpression expression)
        => _result = Sequence.Composition(Read(expression.LeftOperand), Read(expression.RightOperand));

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Always: a placeholder stands for an expression and has no value.</exception>
    public void Visit(SequencePlaceholderExpression expression)
        => throw new InvalidOperationException("A placeholder expression cannot be evaluated.");
}
