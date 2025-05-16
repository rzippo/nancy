using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Interactive;

/// <summary>
/// Adds support for Nancy.Expressions objects in a dotnet interactive context.
/// </summary>
public static class NancyExpressionKernelExtension
{
    /// <summary>
    /// Entry point to register the formatters for Nancy.Expressions objects.
    /// </summary>
    /// <param name="kernel"></param>
    public static void Load(Kernel kernel)
    {
        Formatter.Register<CurveExpression>(
            (expression, writer) => writer.Write(expression.ToHtml()),
            HtmlFormatter.MimeType
        );

        Formatter.Register<ConvolutionExpression>(
            (expression, writer) => writer.Write(expression.ToHtml()),
            HtmlFormatter.MimeType
        );

        Formatter.Register<RationalExpression>(
            (expression, writer) => writer.Write(expression.ToHtml()),
            HtmlFormatter.MimeType
        );
    }
}