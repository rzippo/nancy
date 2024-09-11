namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Interface to constraint the expression types to implement the Accept method and make themselves "visitable"
/// (according to the Visitor Design Pattern)
/// </summary>
public interface IVisitableCurve
{
    /// <summary>
    /// Method used for implementing the Visitor design pattern: the visited object must "accept" the visitor object.
    /// </summary>
    /// <param name="visitor">The Visitor object</param>
    public void Accept(ICurveExpressionVisitor visitor);
}