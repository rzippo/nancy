using System;
using System.Collections.Generic;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Utility;

public static class PrintTraceExtension
{
    public static IEnumerable<string> PrintToLines(this IEnumerable<(Element, Element)> trace, string traceName = "trace")
    {
        yield return $"{traceName}: [";
        foreach(var (a, b) in trace)
            yield return $"\t{a.ToCodeString()}, {b.ToCodeString()}";
        yield return "]";
    }
    
    public static void Print(this IEnumerable<(Element, Element)> trace, string traceName = "trace")
    {
        foreach (var line in PrintToLines(trace, traceName))
            Console.WriteLine(line);
    }
    
    public static string PrintToString(this IEnumerable<(Element, Element)> trace, string traceName = "trace")
    {
        var sb = new StringBuilder();
        foreach (var line in PrintToLines(trace, traceName))
            sb.AppendLine(line);
        return sb.ToString();
    }
}