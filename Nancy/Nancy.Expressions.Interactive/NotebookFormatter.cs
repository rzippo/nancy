using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Interactive;

public static class NotebookFormatter
{
    public static void ShowInNotebook<T>(
        this IGenericExpression<T> expression,
        int depth = 20,
        bool showRationalsAsName = false
    )
    {
        var htmlContent = expression.ToHtml(depth, showRationalsAsName);
        htmlContent.DisplayAs(HtmlFormatter.MimeType);
    }

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