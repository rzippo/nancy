using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Unipi.Nancy.Analyzers.Tests;

/// <summary>
/// NANCY0005 marks the divisions between integers and decimals, which may round or truncate.
/// </summary>
public class DivisionPrecision
{
    public static IEnumerable<object[]> Cases()
    {
        yield return ["int a = 1; int b = 2; var x = {|NANCY0005:a / b|};"];
        yield return ["long a = 1; long b = 2; var x = {|NANCY0005:a / b|};"];
        yield return ["decimal a = 1.0m; long b = 2; var x = {|NANCY0005:a / b|};"];
        yield return ["decimal a = 1.0m; decimal b = 2m; var x = {|NANCY0005:a / b|};"];
        yield return ["Rational a = 1; int b = 2; var x = a / b;"];
        yield return ["double a = 1; double b = 2; var x = a / b;"];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public Task DivisionBetweenIntegersOrDecimalsIsMarked(string body)
        => NancyAnalyzerVerifier<DivisionPrecisionAnalyzer>.VerifyBodyAsync(body);
}
