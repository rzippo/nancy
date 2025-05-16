using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Interactive;

/// <summary>
/// Provides methods to display an expression using LaTeX in a dotnet interactive context.
/// </summary>
public static class Latex
{
    /// <summary>
    /// Displays the expression using Latex and HTML.
    /// </summary>
    /// <param name="expression">The expression to display.</param>
    public static void Show<T>(
        this IGenericExpression<T> expression
    )
    {
        var html = expression.ToHtml();
        html.DisplayAs(HtmlFormatter.MimeType);
    }

    /// <summary>
    /// Displays the expression in Latex syntax.
    /// </summary>
    /// <remarks>
    /// Does not _render_ the Latex syntax.
    /// Use <see cref="Show{T}"/> for that.
    /// </remarks>
    /// <param name="expression">The expression to display.</param>
    public static void ShowSyntax<T>(
        this IGenericExpression<T> expression
    )
    {
        var latex = expression.ToLatexString();
        latex.DisplayAs(PlainTextFormatter.MimeType);
    }
}