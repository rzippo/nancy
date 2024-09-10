using System.Diagnostics;
using System.Text;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.ExpressionsUtility;

/// <summary>
/// Static class for the rendering of Latex commands.
/// </summary>
public static class LatexRenderer
{
    /// <summary>
    /// Returns HTML content that shows the expression formatted using LaTeX.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="depth"></param>
    /// <param name="showRationalsAsName"></param>
    /// <remarks>Uses MathJax to render LaTeX in HTML.</remarks>
    public static string ToHtml<T>(
        this IGenericExpression<T> expression,
        int depth = 20, 
        bool showRationalsAsName = false
    )
    {
        var latexExpression = expression.ToLatexString(depth, showRationalsAsName);
        var html = ToHtml(latexExpression);
        return html;
    }

    /// <summary>
    /// Returns HTML content that shows the expression formatted using LaTeX.
    /// </summary>
    /// <param name="expressions">The expressions.</param>
    /// <param name="depth"></param>
    /// <param name="showRationalsAsName"></param>
    /// <remarks>Uses MathJax to render LaTeX in HTML.</remarks>
    public static string ToHtml<T>(
        this IEnumerable<IGenericExpression<T>> expressions,
        int depth = 20, 
        bool showRationalsAsName = false
    )
    {
        var latexExpressions = expressions
            .Select(e => e.ToLatexString(depth, showRationalsAsName));
        var html = ToHtml(latexExpressions);
        return html;
    }

    /// <summary>
    /// Returns HTML content that shows the LaTeX-formatted expression.
    /// </summary>
    /// <param name="latexExpression">String containing a LaTeX expression.</param>
    /// <remarks>Uses MathJax to render LaTeX in HTML.</remarks>
    public static string ToHtml(string latexExpression)
    {
        var guid = Guid.NewGuid();
        string htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>MathJax Test</title>
                <script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
                <script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
            </head>
            <body>
                <div id='output-{guid}'></div>
                <script>
                    document.getElementById('output-{guid}').innerHTML = '\\({latexExpression.Replace("\\", "\\\\")}\\)';
                    MathJax.typeset();
                </script>
            </body>
            </html>";
        return htmlContent;
    }

    /// <summary>
    /// Returns HTML content that shows the LaTeX-formatted expressions.
    /// </summary>
    /// <param name="latexExpressions">Strings containing LaTeX expressions.</param>
    /// <remarks>Uses MathJax to render LaTeX in HTML.</remarks>
    public static string ToHtml(IEnumerable<string> latexExpressions)
    {
        var guid = Guid.NewGuid();
        var htmlContent = new StringBuilder(@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>MathJax Test</title>
                <script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
                <script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
            </head>
            <body>
            ");

        foreach (var (latexExpression, i) in latexExpressions.WithIndex())
        {
            htmlContent.Append($@"
                <div id='output-{guid}-{i}'></div>
                <script>
                    document.getElementById('output-{guid}-{i}').innerHTML = '\\({latexExpression.Replace("\\", "\\\\")}\\)';
                    MathJax.typeset();
                </script>");
        }

        htmlContent.Append(@"
            </body>
            </html>");

        return htmlContent.ToString();
    }

    /// <summary>
    /// Returns HTML content that shows the expression formatted using LaTeX.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="depth"></param>
    /// <param name="showRationalsAsName"></param>
    public static string ToHtml(
        IGenericExpression<Curve> expression,
        int depth = 20, 
        bool showRationalsAsName = false
    )
        => ToHtml(expression.ToLatexString(depth, showRationalsAsName));

    /// <summary>
    /// Returns HTML content that shows the expression formatted using LaTeX.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="depth"></param>
    /// <param name="showRationalsAsName"></param>
    public static string ToHtml(
        IGenericExpression<Rational> expression,
        int depth = 20, 
        bool showRationalsAsName = false
    )
        => ToHtml(expression.ToLatexString(depth, showRationalsAsName));

    /// <summary>
    /// Opens an HTML file in the default web browser that renders the LaTeX expression.
    /// </summary>
    /// <param name="latexExpression">String containing a LaTeX expression.</param>
    /// <param name="htmlFilePath">The path where the html file should be written. If null, a temp file is used.</param>
    public static void ShowInBrowser(string latexExpression, string? htmlFilePath = null)
    {
        var htmlContent = ToHtml(latexExpression);
        WriteAndOpenHTML(htmlContent, htmlFilePath);
    }

    /// <summary>
    /// Opens an HTML file in the default web browser that renders the LaTeX expressions.
    /// </summary>
    /// <param name="latexExpressions">Strings containing LaTeX expressions.</param>
    /// <param name="htmlFilePath">The path where the html file should be written. If null, a temp file is used.</param>
    public static void ShowInBrowser(IEnumerable<string> latexExpressions, string? htmlFilePath = null)
    {
        var htmlContent = ToHtml(latexExpressions);
        WriteAndOpenHTML(htmlContent, htmlFilePath);
    }

    /// <summary>
    /// Writes the given HTML content to a file and opens it in the default web browser.
    /// </summary>
    /// <param name="htmlContent">The HTML content.</param>
    /// <param name="htmlFilePath">The path where the html file should be written. If null, a temp file is used.</param>
    private static void WriteAndOpenHTML(string htmlContent, string? htmlFilePath = null)
    {
        htmlFilePath ??= Path.GetTempFileName();
        if (Path.GetExtension(htmlFilePath) != "html")
        {
            var newPath = Path.ChangeExtension(htmlFilePath, "html");
            if (File.Exists(newPath))
                File.Delete(newPath);
            if (File.Exists(htmlFilePath))
                File.Move(htmlFilePath, newPath);
            htmlFilePath = newPath;
        }

        File.WriteAllText(htmlFilePath, htmlContent);

        // Open the HTML file in the default web browser
        Process.Start(new ProcessStartInfo(htmlFilePath) { UseShellExecute = true });
    }
}