using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Unipi.Nancy.Plots;

public static class ParametersNameParsing
{
    /// <summary>
    /// Given an expression string, tries to parse the name of the variables and return it as a list.
    /// If the parsing fails, or if less then <c>n</c> names are parsed, it fills the gaps with default names. 
    /// </summary>
    /// <param name="expr">The expression to parse.</param>
    /// <param name="n">Expected number of names.</param>
    /// <returns>A list of <c>n</c> names.</returns>
    /// <remarks>
    /// The expression should be a valid constructor of a collection, which references the elements,
    /// e.g. <c>[f, g, h]</c>, <c>new []{f, g, h}</c> etc.
    /// </remarks>
    public static List<string> ParseNames(this string expr, int n)
    {
        if (string.IsNullOrEmpty(expr))
            return DefaultNames(n);

        // matches collection expressions, like "[f, g, h]"
        var bracketsNotationRegex = new Regex(@"\[(?:([\w\d_\s+*-]+)(?:,\s*)?)+\]");
        // matches array expressions, like "new []{f, g, h}"
        var arrayNotationRegex = new Regex(@"new \w*?\[\]\{(?:([\w\d_\s+*-]+)(?:,\s*)?)+\}");
        // matches list expressions, like "new List<>{f, g, h}"
        var listNotationRegex = new Regex(@"new List<.*>(?:\(\))?\{(?:([\w\d_\s+*-]+)(?:,\s*)?)+\}");

        List<string> parsedNames;
        if (bracketsNotationRegex.IsMatch(expr))
        {
            var match = bracketsNotationRegex.Match(expr);
            parsedNames = match.Groups[1].Captures
                .Select(c => c.Value)
                .ToList();
        }
        else if (arrayNotationRegex.IsMatch(expr))
        {
            var match = arrayNotationRegex.Match(expr);
            parsedNames = match.Groups[1].Captures
                .Select(c => c.Value)
                .ToList();
        }
        else if (listNotationRegex.IsMatch(expr))
        {
            var match = listNotationRegex.Match(expr);
            parsedNames = match.Groups[1].Captures
                .Select(c => c.Value)
                .ToList();
        }
        else
        {
            // all else failed
            parsedNames = DefaultNames(n);
        }
        
        // fallback in case of parsing errors
        if(parsedNames.Count < n)
            parsedNames.AddRange(DefaultNames(n - parsedNames.Count));

        return parsedNames;
        List<string> DefaultNames(int number)
            => Enumerable.Range(0, number)
                .Select(i => $"{(char)('f' + i)}")
                .ToList();
    }

}