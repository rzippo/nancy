using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Unipi.Nancy.Analyzers.Tests;

/// <summary>
/// NANCY0003 marks the subtractions of sequences that do not say whether the result is non-negative, and NANCY0002 the operator - between sequences.
/// </summary>
public class SequenceSubtraction
{
    public static IEnumerable<object[]> MethodCases()
    {
        yield return ["var x = Sequence.{|NANCY0003:Subtraction|}(s, t);"];
        yield return ["var x = s.{|NANCY0003:Subtraction|}(t);"];
        yield return ["var x = Sequence.Subtraction(s, t, true);"];
        yield return ["var x = s.Subtraction(t, false);"];
    }

    [Theory]
    [MemberData(nameof(MethodCases))]
    public Task SubtractionWithoutNonNegativeIsMarked(string body)
        => NancyAnalyzerVerifier<SequenceSubtractionBreakingChangeSemanticAnalyzer>.VerifyBodyAsync(body);

    public static IEnumerable<object[]> OperatorCases()
    {
        yield return ["var x = s {|NANCY0002:-|} t;"];
        yield return ["var x = -s;"];
        yield return ["var x = s - 3;"];
        yield return ["var x = s - new Rational(1, 2);"];
    }

    [Theory]
    [MemberData(nameof(OperatorCases))]
    public Task OperatorBetweenSequencesIsMarked(string body)
        => NancyAnalyzerVerifier<SequenceSubtractionOperatorBreakingChangeSemanticAnalyzer>.VerifyBodyAsync(body);
}
