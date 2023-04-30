using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Interface for classes having the ToCodeString() method.
/// </summary>
public interface IToCodeString
{
    /// <summary>
    /// Returns a string containing C# code to create this object.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    /// <param name="formatted"></param>
    /// <param name="indentation"></param>
    public string ToCodeString(bool formatted = false, int indentation = 0);
}

/// <summary>
/// Provides ToCodeString() for collections of IToCodeString objects.
/// </summary>
public static class ToCodeStringExtensions
{
    /// <summary>
    /// Returns a string containing C# code to create this set as a List.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="formatted"></param>
    /// <param name="indentation"></param>
    /// <typeparam name="T"></typeparam>
    public static string ToCodeString<T>(this IReadOnlyList<T> source, bool formatted = false, int indentation = 0) where T : IToCodeString
    {
        var newline = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new List<{typeof(T).Name}>{{{newline}");
        foreach (var (v, i) in source.WithIndex())
        {
            sb.Append($"{tabs(1)}{v.ToCodeString()}");
            if (i < source.Count - 1)
                sb.Append($",{newline}");
        }
        if(formatted)
            sb.Append($"{tabs(0)}}}");
        else
            sb.Append($" }}");
        return sb.ToString();

        string tabs(int n)
        {
            if (!formatted)
                return "";
            var sbt = new StringBuilder();
            for (int i = 0; i < indentation + n; i++)
                sbt.Append("\t");
            return sbt.ToString();
        }
    }
}