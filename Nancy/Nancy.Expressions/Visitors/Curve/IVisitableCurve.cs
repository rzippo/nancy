namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Interface to constraint the expression types to implement the Accept method and make themselves "visitable"
/// (according to the Visitor Design Pattern)
/// </summary>
public interface IVisitableCurve
{
    /// <summary>
    /// Method used to implement the Visitor design pattern: the visited object must "accept" the visitor object.
    /// </summary>
    /// <param name="visitor">The Visitor object</param>
    /// <remarks>
    /// This method is to be used by visitors that keep an internal state and result.
    /// The semantics to retrieve the result depend on the visitor.
    /// </remarks>
    public void Accept(ICurveExpressionVisitor visitor);
    
    /// <summary>
    /// Method used to implement the Visitor design pattern: the visited object must "accept" the visitor object.
    /// </summary>
    /// <param name="visitor">The Visitor object</param>
    /// <typeparam name="TResult">The type of the result of the visit.</typeparam>
    /// <remarks>
    /// This method is to be used by visitors that compute and return a result of type <typeparamref name="TResult"/>.
    /// </remarks>
    public abstract TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor);
}