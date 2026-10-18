using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Unipi.Nancy.Analyzers.Tests;

/// <summary>
/// NANCY0006 marks the conversions of floating-point values to the rational types of Nancy.
/// </summary>
public class FloatingPointToRational
{
    public static IEnumerable<object[]> Cases()
    {
        yield return ["double v = 0.1; var x = new Rational({|NANCY0006:v|});"];
        yield return ["float v = 0.1f; var x = new Rational({|NANCY0006:v|});"];
        yield return ["double v = 0.1; Rational x = {|NANCY0006:v|};"];
        yield return ["float v = 0.1f; Rational x = {|NANCY0006:v|};"];
        yield return ["double v = 0.1; var x = {|NANCY0006:(Rational) v|};"];
        yield return ["double v = 0.1; BigRational x = {|NANCY0006:v|};"];
        yield return ["float v = 0.1f; LongRational x = {|NANCY0006:v|};"];
        yield return ["AcceptRational({|NANCY0006:0.1|}); AcceptBigRational({|NANCY0006:0.1|}); AcceptLongRational({|NANCY0006:0.1f|});"];
        yield return ["Rational a = 0.1m; var b = new Rational(1, 10); BigRational e = 0.1m; var f = new LongRational(1, 10);"];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public Task FloatingPointConvertedToRationalIsMarked(string body)
        => NancyAnalyzerVerifier<FloatingPointToRationalAnalyzer>.VerifyBodyAsync(body);
}
