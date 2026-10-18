using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Analyzers.Tests;

/// <summary>
/// Runs an analyzer over code that uses Nancy, and compares its diagnostics with those marked in the code as <c>{|ID:text|}</c>.
/// </summary>
internal static class NancyAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Verifies a whole compilation unit.
    /// </summary>
    public static Task VerifyAsync(string source)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };
        test.TestState.AdditionalReferences.Add(typeof(Curve).Assembly);
        return test.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies a method body, which can use the curves <c>c</c> and <c>d</c>, the rate-latency curve <c>rl</c>, the sequences <c>s</c> and <c>t</c>,
    /// and methods <c>AcceptRational</c>, <c>AcceptBigRational</c> and <c>AcceptLongRational</c> taking one rational of each type.
    /// </summary>
    public static Task VerifyBodyAsync(string body)
        => VerifyAsync($$"""
            using Unipi.Nancy.MinPlusAlgebra;
            using Unipi.Nancy.NetworkCalculus;
            using Unipi.Nancy.Numerics;

            class Snippet
            {
                void Run(Curve c, Curve d, RateLatencyServiceCurve rl, Sequence s, Sequence t)
                {
                    {{body}}
                }

                static void AcceptRational(Rational value) { }
                static void AcceptBigRational(BigRational value) { }
                static void AcceptLongRational(LongRational value) { }
            }
            """);
}
