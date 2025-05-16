using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Interactive;

// todo: merge this with Latex class
/// <summary>
/// Provides methods to display an expression using LaTeX in a dotnet interactive context.
/// </summary>
public static class NotebookFormatter
{
    /// <summary>
    /// Displays the expression using Latex and HTML.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <param name="depth"></param>
    /// <param name="showRationalsAsName"></param>
    public static void ShowInNotebook<T>(
        this IGenericExpression<T> expression,
        int depth = 20,
        bool showRationalsAsName = false
    )
    {
        var htmlContent = expression.ToHtml(depth, showRationalsAsName);
        htmlContent.DisplayAs(HtmlFormatter.MimeType);
    }

    /// <summary>
    /// Displays the set of expressions using Latex and HTML.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expressions"></param>
    /// <param name="depth"></param>
    /// <param name="showRationalsAsName"></param>
    public static void ShowInNotebook<T>(
        this IEnumerable<IGenericExpression<T>> expressions,
        int depth = 20,
        bool showRationalsAsName = false
    )
    {
        var htmlContent = expressions.ToHtml(depth, showRationalsAsName);
        htmlContent.DisplayAs(HtmlFormatter.MimeType);
    }
}