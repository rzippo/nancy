using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Visitors;


[TestSubject(typeof(Nancy.Expressions.Visitors.LatexFormatterVisitor))]
public class ToLatexString
{
    public static List<(IExpression expr, string latex)> LatexFormattingExamples =
    [
        (
            Expressions.Convolution(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ),
            @"a \otimes b"
        ),
        (
            Expressions.Deconvolution(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ),
            @"a \oslash b"
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
            @"a \otimes b \otimes c"
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
            @"\left( a \otimes b \otimes c \right) + d"
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
            @"\left( \left( a \oslash b \right) \oslash c \right) + d"
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
            @"\left( a \otimes b \otimes c \right)^{\circ}"
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
            @"\left( a \otimes b \right)^{\underline{-1}}"
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
            @"\left( a \otimes b \right)^{\overline{-1}}"
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
            @"\left[ a - b \right]^{+}"
        ),
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                nonNegative: true,
                "a",
                "b"
            ).ToLowerNonDecreasing(),
            @"\mathrm{LND}\left( \left[ a - b \right]^{+} \right)"
        ),
        (
            Expressions.Subtraction(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                nonNegative: true,
                "a",
                "b"
            ).ToUpperNonDecreasing(),
            @"\mathrm{UND}\left( \left[ a - b \right]^{+} \right)"
        ),
#pragma warning restore CS0618 // Type or member is obsolete
        (
            Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), "a").ToUpperNonIncreasing(),
            @"\mathrm{UNI}\left( a \right)"
        ),
        (
            Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), "a").ToLowerNonIncreasing(),
            @"\mathrm{LNI}\left( a \right)"
        ),
        (
            Expressions.Addition(
                new SigmaRhoArrivalCurve(1, 2),
                new RateLatencyServiceCurve(2, 5),
                "a",
                "b"
            ).ToLowerNonDecreasing(),
            @"\mathrm{LND}\left( a + b \right)"
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
            @"\mathrm{LND}\left( \left[ a + b \right]^{+} \right)"
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
            @"\mathrm{UND}\left( \left[ a + b \right]^{+} \right)"
        ),
    ];

    public static IEnumerable<object[]> LatexFormattingTestCases =
        LatexFormattingExamples.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(LatexFormattingTestCases))]
    public void ToLatexString_LatexFormatting(IExpression expr, string latex)
    {
        var exprToString = expr.ToLatexString();
        Assert.Equal(latex, exprToString);
    }
}