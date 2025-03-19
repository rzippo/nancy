using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the deconvolution
/// </summary>
public record DeconvolutionExpression : CurveBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// Statically adds the set of well-known equivalences involving the deconvolution operation to the dictionary of
    /// the equivalences (<see cref="Equivalences"/>)
    /// </summary>
    static DeconvolutionExpression()
    {
        AddEquivalence(typeof(DeconvolutionExpression), new DeconvolutionWithConvolution());
        AddEquivalence(typeof(DeconvolutionExpression), new DeconvolutionWeakCommutativity());
        AddEquivalence(typeof(DeconvolutionExpression), new DeconvDistributivityWithMax());
        AddEquivalence(typeof(DeconvolutionExpression), new DeconvDistributivityWithMin());
        AddEquivalence(typeof(DeconvolutionExpression), new DeconvAndSubAdditiveClosure());
        AddEquivalence(typeof(DeconvolutionExpression), new SelfDeconvolutionSubAdditive());
    }
    
    
    /// <summary>
    /// Creates a deconvolution expression
    /// </summary>
    public DeconvolutionExpression(
        Curve curveL,
        string nameL,
        Curve curveR,
        string nameR,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), expressionName, settings)
    {
    }

    
    /// <summary>
    /// Creates a deconvolution expression
    /// </summary>
    public DeconvolutionExpression(
        Curve curveL,
        string nameL,
        CurveExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a deconvolution expression
    /// </summary>
    public DeconvolutionExpression(
        CurveExpression LeftExpression,
        CurveExpression RightExpression,
        string ExpressionName = "",
        ExpressionSettings? Settings = null) 
        : base(LeftExpression, RightExpression, ExpressionName, Settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}