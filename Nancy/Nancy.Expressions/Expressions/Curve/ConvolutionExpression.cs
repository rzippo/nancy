using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the convolution (n-ary operation)
/// </summary>
public record ConvolutionExpression : CurveNAryExpression
{
    /// <summary>
    /// Statically adds the set of well-known equivalences involving the convolution operation to the dictionary of the
    /// equivalences (<see cref="Equivalences"/>)
    /// </summary>
    static ConvolutionExpression()
    {
        AddEquivalence(typeof(ConvolutionExpression), new ConvolutionSubAdditiveWithDominance());
        AddEquivalence(typeof(ConvolutionExpression), new ConvSubAdditiveAsSelfConvMinimum());
        AddEquivalence(typeof(ConvolutionExpression), new ConvolutionDistributivityMin());
        AddEquivalence(typeof(ConvolutionExpression), new ConvAdditionByAConstant());
        AddEquivalence(typeof(ConvolutionExpression), new ConvAndSubadditiveClosure());
        AddEquivalence(typeof(ConvolutionExpression), new ConvolutionWithConcaveFunctions());
        AddEquivalence(typeof(ConvolutionExpression), new SelfConvolutionSubAdditive());
    }

    /// <summary>
    /// Creates a convolution expression
    /// </summary>
    /// <param name="expressions">The operands (expressions) of the addition</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public ConvolutionExpression(IReadOnlyCollection<IGenericExpression<Curve>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a convolution expression from a collection of curves
    /// </summary>
    /// <param name="curves">Collection of curves representing the operands of the addition</param>
    /// <param name="names">Collection of the names of the curves operands</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public ConvolutionExpression(IReadOnlyCollection<Curve> curves,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null) : base(curves, names, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}