using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Interactive;

public static class NancyExpressionKernelExtension
{
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