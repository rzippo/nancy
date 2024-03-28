using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Interactive;

public static class NancyKernelExtension
{
    public static void Load(Kernel kernel)
    {
        Formatter.Register<Curve>(
            (curve, writer) => writer.Write(curve.GetPlotAsHtml("f")),
            HtmlFormatter.MimeType
        );

        Formatter.Register<IReadOnlyCollection<Curve>>(
            (curves, writer) => writer.Write(curves.GetPlotAsHtml()),
            HtmlFormatter.MimeType
        );

        Formatter.Register<Sequence>(
            (sequence, writer) => writer.Write(sequence.GetPlotAsHtml("f")),
            HtmlFormatter.MimeType
        );

        Formatter.Register<IReadOnlyCollection<Sequence>>(
            (sequences, writer) => writer.Write(sequences.GetPlotAsHtml()),
            HtmlFormatter.MimeType
        );
    }
}