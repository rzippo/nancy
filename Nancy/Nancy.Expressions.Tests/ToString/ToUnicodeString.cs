using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Visitors;


[TestSubject(typeof(Nancy.Expressions.Visitors.UnicodeFormatterVisitor))]
public class ToUnicodeString
{
    public static List<(IExpression expr, string unicode)> UnicodeFormattingExamples =
    [
        (
            Expressions.Convolution(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ),
            "a ⊗ b"
        ),
        (
            Expressions.Deconvolution(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ),
            "a ⊘ b"
        ),
        (
            Expressions.Convolution(
                [
                    new SigmaRhoArrivalCurve(1, 2),
                    new RateLatencyServiceCurve(2, 5),
                    new RateLatencyServiceCurve(2, 5),
                ],
                [
                    "a",
                    "b",
                    "c"
                ]
            ),
            "a ⊗ b ⊗ c"
        ),
        (
            Expressions.Addition(
                Expressions.Convolution(
                    [
                        new SigmaRhoArrivalCurve(1, 2),
                        new RateLatencyServiceCurve(2, 5),
                        new RateLatencyServiceCurve(2, 5),
                    ],
                    [
                        "a",
                        "b",
                        "c"
                    ]
                ),
                new RateLatencyServiceCurve(2, 5),
                "d"
            ),
            "(a ⊗ b ⊗ c) + d"
        ),
        (
            Expressions.Addition(
                Expressions.Deconvolution(
                    Expressions.Deconvolution(
                        new SigmaRhoArrivalCurve(1, 2),
                        new RateLatencyServiceCurve(2, 5),
                        "a",
                        "b"
                    ),
                    new RateLatencyServiceCurve(2, 5),
                    "c"
                ),
                new RateLatencyServiceCurve(2, 5),
                "d"
            ),
            "((a ⊘ b) ⊘ c) + d"
        ),
        (
            Expressions.Convolution(
                [
                    new SigmaRhoArrivalCurve(1, 2),
                    new RateLatencyServiceCurve(2, 5),
                    new RateLatencyServiceCurve(2, 5),
                ],
                [
                    "a",
                    "b",
                    "c"
                ]
            ).WithZeroOrigin(),
            @"(a ⊗ b ⊗ c)°"
        ),
        (
            Expressions.Convolution(
                [
                    new SigmaRhoArrivalCurve(1, 2),
                    new RateLatencyServiceCurve(2, 5),
                ],
                [
                    "a",
                    "b"
                ]
            ).LowerPseudoInverse(),
            @"(a ⊗ b)↓⁻¹"
        ),
        (
            Expressions.Convolution(
                [
                    new SigmaRhoArrivalCurve(1, 2),
                    new RateLatencyServiceCurve(2, 5),
                ],
                [
                    "a",
                    "b"
                ]
            ).UpperPseudoInverse(),
            @"(a ⊗ b)↑⁻¹"
        ),
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ),
            @"a - b"
        ),
#pragma warning disable CS0618 // Type or member is obsolete
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                nonNegative: false,
                "a",
                "b"
            ),
            @"a - b"
        ),
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                nonNegative: true,
                "a",
                "b"
            ),
            @"[a - b]⁺"
        ),
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                nonNegative: true,
                "a",
                "b"
            ).ToLowerNonDecreasing(),
            @"LND([a - b]⁺)"
        ),
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                nonNegative: true,
                "a",
                "b"
            ).ToUpperNonDecreasing(),
            @"UND([a - b]⁺)"
        ),
#pragma warning restore CS0618 // Type or member is obsolete
        (
            Expressions.Addition(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ).ToLowerNonDecreasing(),
            @"LND(a + b)"
        ),
        (
            Expressions.Addition(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            )
            .ToNonNegative()
            .ToLowerNonDecreasing()
            ,
            @"LND([a + b]⁺)"
        ),
        (
            Expressions.Addition(
                    new SigmaRhoArrivalCurve(1, 2),
                    new RateLatencyServiceCurve(2, 5),
                    "a",
                    "b"
                )
                .ToNonNegative()
                .ToUpperNonDecreasing()
            ,
            @"UND([a + b]⁺)"
        ),
        (
            Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), "a").ToUpperNonIncreasing(),
            @"UNI(a)"
        ),
        (
            Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), "a").ToLowerNonIncreasing(),
            @"LNI(a)"
        ),
    ];

    public static IEnumerable<object[]> UnicodeFormattingTestCases =
        UnicodeFormattingExamples.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(UnicodeFormattingTestCases))]
    public void ToUnicodeString_UnicodeFormatting(IExpression expr, string unicode)
    {
        var exprToString = expr.ToUnicodeString();
        Assert.Equal(unicode, exprToString);
    }
    
    [Fact]
    public void ToUnicodeString_UnicodeFormatting_Fact()
    {
        var a = new SigmaRhoArrivalCurve(1, 2);
        var b = new RateLatencyServiceCurve(2, 5);
        var e = Expressions.Convolution(a, b);
        
        var eExpected = "a ⊗ b";
        var eRegex = $"\\(?{Regex.Escape(eExpected)}\\)?";
        Assert.Matches(eRegex, e.ToUnicodeString());
    }
}