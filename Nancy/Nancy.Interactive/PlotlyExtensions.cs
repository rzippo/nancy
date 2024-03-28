using System.Text;
using Giraffe.ViewEngine;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.DotNet.Interactive.Http;
using Microsoft.FSharp.Collections;
using XPlot.Plotly;

namespace Unipi.Nancy.Interactive;

/// <summary>
/// This static class ports and exposes some important methods from the
/// <a href="https://www.nuget.org/packages/XPlot.Plotly.Interactive/">XPlot.Plotly.Interactive</a> package.
/// For some reason, the HTML produced by the public methods does not render consistently in .NET interactive,
/// while the methods made public here do.
/// </summary>
public static class PlotlyExtensions
{
    /// <summary>
    /// Produces a &lt;script&gt; tag that imports plotly-1.49.2 using require, and contains the given script.
    /// </summary>
    /// <param name="script">The script contained by the tag.</param>
    /// <returns>A string with the &lt;script&gt; tag.</returns>
    public static string GetScriptElementWithRequire(string script)
    {
        var newScript = new StringBuilder();
        newScript.AppendLine("""<script type="text/javascript">""");
        newScript.AppendLine("""
var renderPlotly = function() {
    var xplotRequire = require.config({context:'xplot-3.0.1',paths:{plotly:'https://cdn.plot.ly/plotly-1.49.2.min'}}) || require;
    xplotRequire(['plotly'], function(Plotly) { 
""");
        newScript.AppendLine(script);
        newScript.AppendLine(@"});
};");
        newScript.AppendLine(JavascriptUtilities.GetCodeForEnsureRequireJs(
            new Uri("https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js"), "renderPlotly"));
        newScript.AppendLine("</script>");
        return newScript.ToString();
    }

    /// <summary>
    /// Produces HTML code for the given chart, that will render consistently in .NET Interactive.
    /// </summary>
    /// <param name="chart">The chart to show in the HTML.</param>
    /// <returns>A string with the HTML code.</returns>
    public static string GetNotebookHtml(this PlotlyChart chart)
    {
        var styleStr = $"width: {chart.Width}px; height: {chart.Height}px;";
        var div =
            HtmlElements.div.Invoke(new FSharpList<HtmlElements.XmlAttribute>(
                HtmlElements.XmlAttribute.NewKeyValue("style", styleStr),
                FSharpList<HtmlElements.XmlAttribute>.Cons(HtmlElements.XmlAttribute.NewKeyValue("id", chart.Id),
                    FSharpList<HtmlElements.XmlAttribute>.Empty)
            )).Invoke(FSharpList<HtmlElements.XmlNode>.Empty);
        var divElem = RenderView.AsString.htmlDocument(div);

        var js = chart.GetInlineJS()
            .Replace("<script>", String.Empty)
            .Replace("</script>", String.Empty);
        var htmlString = new HtmlString(divElem + GetScriptElementWithRequire(js)); 
        return htmlString.ToString();
    }

    /// <summary>
    /// Displays the chart in .NET Interactive, using HTML.
    /// </summary>
    /// <param name="chart">The chart to display.</param>
    public static void DisplayOnNotebook(this PlotlyChart chart)
    {
        chart
            .GetNotebookHtml()
            .DisplayAs(HtmlFormatter.MimeType);
    }
}