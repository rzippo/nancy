using System;
using System.Collections.Generic;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Utility;

/// <summary>
/// Helper methods that print a trace such as the ones produced by <see cref="Curve.TraceConvolution"/> and similar methods.
/// </summary>
public static class PrintTraceExtensions
{
    /// <summary>
    /// Helper method that prints a trace such as the ones produced by <see cref="Curve.TraceConvolution"/> and similar methods.
    /// Prints to a series of string, one for each line. 
    /// </summary>
    public static IEnumerable<string> PrintToLines(this IEnumerable<(Element, Element)> trace, string traceName = "trace")
    {
        yield return $"{traceName}: [";
        foreach(var (a, b) in trace)
            yield return $"\t{a.ToCodeString()}, {b.ToCodeString()}";
        yield return "]";
    }
    
    /// <summary>
    /// Helper method that prints a trace such as the ones produced by <see cref="Curve.TraceConvolution"/> and similar methods.
    /// Prints directly to standard output. 
    /// </summary>
    public static void Print(this IEnumerable<(Element, Element)> trace, string traceName = "trace")
    {
        foreach (var line in PrintToLines(trace, traceName))
            Console.WriteLine(line);
    }
    
    /// <summary>
    /// Helper method that prints a trace such as the ones produced by <see cref="Curve.TraceConvolution"/> and similar methods.
    /// Prints to a single string. 
    /// </summary>
    public static string PrintToString(this IEnumerable<(Element, Element)> trace, string traceName = "trace")
    {
        var sb = new StringBuilder();
        foreach (var line in PrintToLines(trace, traceName))
            sb.AppendLine(line);
        return sb.ToString();
    }
}