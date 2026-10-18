using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Unipi.Nancy.Analyzers.Tests;

/// <summary>
/// NANCY0001 marks the subtractions of curves that do not say whether the result is non-negative, and NANCY0004 the operator - between curves.
/// </summary>
public class CurveSubtraction
{
    public static IEnumerable<object[]> MethodCases()
    {
        yield return ["var x = Curve.{|NANCY0001:Subtraction|}(c, d);"];
        yield return ["var x = c.{|NANCY0001:Subtraction|}(d);"];
        yield return ["var x = rl.{|NANCY0001:Subtraction|}(d);"];
        yield return ["var x = Curve.Subtraction(c, d, true);"];
        yield return ["var x = c.Subtraction(d, false);"];
    }

    [Theory]
    [MemberData(nameof(MethodCases))]
    public Task SubtractionWithoutNonNegativeIsMarked(string body)
        => NancyAnalyzerVerifier<CurveSubtractionBreakingChangeSemanticAnalyzer>.VerifyBodyAsync(body);

    public static IEnumerable<object[]> OperatorCases()
    {
        yield return ["var x = c {|NANCY0004:-|} d;"];
        yield return ["var x = rl {|NANCY0004:-|} d;"];
        yield return ["var x = -c;"];
    }

    [Theory]
    [MemberData(nameof(OperatorCases))]
    public Task OperatorBetweenCurvesIsMarked(string body)
        => NancyAnalyzerVerifier<CurveSubtractionOperatorBreakingChangeSemanticAnalyzer>.VerifyBodyAsync(body);
}
