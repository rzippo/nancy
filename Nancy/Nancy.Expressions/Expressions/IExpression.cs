using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Core interface for Nancy expressions.
/// Does not specify its return or operand types, and therefore no operation.
/// </summary>
/// <remarks>
/// To be used as a common root for simple variable assignments.
/// To have computation methods, see casts to other types like <see cref="CurveExpression"/> or <see cref="RationalExpression"/>.
/// </remarks>
public interface IExpression
{
    /// <summary>
    /// The name of the expression.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Formats the expression in LaTeX.
    /// </summary>
    /// <param name="depth">Level of the expression tree up to which to print the expression fully expanded.</param>
    /// <param name="showRationalsAsName">
    /// If true, shows rational numbers with their expression name in place of their value.
    /// </param>
    public string ToLatexString(
        int depth = 20,
        bool showRationalsAsName = false
    );

    /// <summary>
    /// Returns the representation of the expression using characters of the Unicode character set.
    /// </summary>
    /// <param name="depth">Level of the expression tree up to which to print the expression fully expanded.</param>
    /// <param name="showRationalsAsName">
    /// If true, shows rational numbers with their expression name in place of their value.
    /// </param>
    public string ToUnicodeString(
        int depth = 20,
        bool showRationalsAsName = false
    );

    /// <summary>
    /// Returns the representation of the expression as MPPG source text, valid under syntax version 1.3,
    /// or 1.4 if the expression uses a monotonicity closure ($\mathrm{UND}$, $\mathrm{LND}$, $\mathrm{UNI}$, $\mathrm{LNI}$).
    /// </summary>
    /// <param name="depth">Level of the expression tree up to which to print the expression fully expanded.</param>
    /// <param name="showRationalsAsName">
    /// If true, shows rational numbers with their expression name in place of their value.
    /// </param>
    /// <exception cref="Visitors.MppgFormattingException">
    /// If the expression contains an operation for which the MPPG syntax has no notation.
    /// </exception>
    /// <remarks>
    /// The result is a single expression, with no statement around it, and does not require the expression to be computed.
    /// It only parses in a context where the names it uses are declared.
    /// </remarks>
    public string ToMppgString(
        int depth = 20,
        bool showRationalsAsName = false
    );

    /// <summary>
    /// Represents the expression in textual format.
    /// </summary>
    public string ToString();

    /// <summary>
    /// True if the expression has already been computed.
    /// </summary>
    public bool IsComputed { get; }

    /// <summary>
    /// Computes the expression, without returning its result.
    /// </summary>
    /// <remarks>
    /// Useful to perform operations through the <see cref="IExpression"/> interface, avoiding unnecessary casts.
    /// A cast to <see cref="IGenericExpression{T}"/> is needed to access the computed result value.
    /// </remarks>
    public void ComputeWithoutResult();
    
    /// <summary>
    /// Evaluates the computational complexity of the expression.
    /// </summary>
    /// <returns>The value representative of the complexity of the expression</returns>
    public double Estimate();
    
    /// <summary>
    /// This method can be used as a starting point to build a symbolic path through the expression.
    /// </summary>
    /// <returns>Returns the position of the root of the expression</returns>
    public ExpressionPosition RootPosition();
}