using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Unipi.Nancy.Analyzers.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DivisionPrecisionCodeFixProvider))]
[Shared]
public sealed class DivisionPrecisionCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = DivisionPrecisionAnalyzer.DiagnosticId;

    private const string TitleIntegralCtor = "Use Rational(numerator, denominator)";
    private const string TitleGenericCtorWrap = "Wrap both sides with Rational(...)";
    public override ImmutableArray<string> FixableDiagnosticIds
        => [DiagnosticId];

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;

        // Find the division expression whose operator token was flagged
        var node = root.FindNode(span, getInnermostNodeForTie: true);
        var syntax = node.FirstAncestorOrSelf<BinaryExpressionSyntax>(
            n => n.IsKind(SyntaxKind.DivideExpression));
        if (syntax is null) 
            return;
            
        var semanticModel = await context.Document
            .GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (semanticModel is null) 
            return;
            
        var operation = semanticModel.GetOperation(syntax, context.CancellationToken)
            as IBinaryOperation;
        if (operation is null)
            return;

        var leftType = operation.LeftOperand.Type;
        var rightType = operation.RightOperand.Type;

        if (
            leftType == null || rightType == null ||
            leftType.SpecialType == SpecialType.System_Decimal ||
            rightType.SpecialType == SpecialType.System_Decimal
        )
        {
            // not simple integer types, must wrap
            context.RegisterCodeFix(
                CodeAction.Create(
                    TitleGenericCtorWrap,
                    ct => ApplyWrapBothSidesAsync(context.Document, syntax, ct),
                    equivalenceKey: $"{DivisionPrecisionAnalyzer.DiagnosticId}_WrapBoth"),
                diagnostic);
        }
        else
        {
            // simple integer types, can use numerator / denominator
            context.RegisterCodeFix(
                CodeAction.Create(
                    TitleIntegralCtor,
                    ct => ApplyNumeratorDenominatorAsync(context.Document, syntax, ct),
                    equivalenceKey: $"{DivisionPrecisionAnalyzer.DiagnosticId}_NumDen"),
                diagnostic);
        }
    }

    private static async Task<Document> ApplyWrapBothSidesAsync(Document document, BinaryExpressionSyntax division, CancellationToken ct)
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null) return document;

        var gen = SyntaxGenerator.GetGenerator(document);

        var leftExpr = division.Left;
        var rightExpr = division.Right;

        var newLeft = NewRationalFrom(leftExpr);
        var newRight = NewRationalFrom(rightExpr);

        // Keep it a division, but now between rationals.
        var newDivision = SyntaxFactory.BinaryExpression(
                SyntaxKind.DivideExpression,
                ParenthesizeIfNeeded(newLeft),
                division.OperatorToken.WithoutTrivia(),
                ParenthesizeIfNeeded(newRight))
            .WithTriviaFrom(division);

        document = await ReplaceAsync(document, division, newDivision, ct)
            .ConfigureAwait(false);
        
        // ensure the Unipi.Nancy.Numerics namespace is imported
        document = await EnsureUsingAsync(document, "Unipi.Nancy.Numerics", ct)
            .ConfigureAwait(false);

        return document;
    }

    private static async Task<Document> ApplyNumeratorDenominatorAsync(
        Document document, 
        BinaryExpressionSyntax division, 
        CancellationToken ct
    )
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null) return document;

        // Only do this when both operands are integral types
        var leftType = semanticModel.GetTypeInfo(division.Left, ct).Type;
        var rightType = semanticModel.GetTypeInfo(division.Right, ct).Type;

        if (!(IsIntegral(leftType) && IsIntegral(rightType)))
        {
            // if cannot do what was asked, do nothing.
            return document;

            // Fall back to wrapping both sides if not integral
            // return await ApplyWrapBothSidesAsync(document, division, ct)
            //     .ConfigureAwait(false);
        }

        // new Rational(left, right)
        var rationalIdent = SyntaxFactory.IdentifierName("Rational");
        var leftArg = SyntaxFactory.Argument(ParenthesizeIfNeeded(division.Left.WithoutTrivia()));
        var rightArg = SyntaxFactory.Argument(ParenthesizeIfNeeded(division.Right.WithoutTrivia()));

        var argList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList([leftArg, rightArg]));

        var objCreation = SyntaxFactory.ObjectCreationExpression(rationalIdent)
            .WithNewKeyword(SyntaxFactory.Token(SyntaxKind.NewKeyword))
            .WithArgumentList(argList)
            .WithTriviaFrom(division);

        document = await ReplaceAsync(document, division, objCreation, ct)
            .ConfigureAwait(false);
        
        // ensure the Unipi.Nancy.Numerics namespace is imported
        document = await EnsureUsingAsync(document, "Unipi.Nancy.Numerics", ct)
            .ConfigureAwait(false);

        return document;
    }

    private static ExpressionSyntax NewRationalFrom(ExpressionSyntax expr)
    {
        // Construct: new Rational(<expr>)
        var rationalType = SyntaxFactory.IdentifierName("Rational");
        var arg = SyntaxFactory.Argument(ParenthesizeIfNeeded(expr.WithoutTrivia()));
        var argList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(arg));

        return SyntaxFactory.ObjectCreationExpression(rationalType)
            .WithNewKeyword(SyntaxFactory.Token(SyntaxKind.NewKeyword))
            .WithArgumentList(argList)
            .WithLeadingTrivia(expr.GetLeadingTrivia())
            .WithTrailingTrivia(expr.GetTrailingTrivia());
    }

    private static async Task<Document> ReplaceAsync(
        Document document, 
        SyntaxNode original, 
        SyntaxNode replacement, 
        CancellationToken ct
    )
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;
        var newRoot = root.ReplaceNode(original, replacement);
        return document.WithSyntaxRoot(newRoot);
    }

    private static bool IsIntegral(ITypeSymbol? type)
    {
        if (type is null) return false;

        switch (type.SpecialType)
        {
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return true;
        }
        if (type is INamedTypeSymbol named && named.IsNativeIntegerType)
            return true;

        return false;
    }

    private static ExpressionSyntax ParenthesizeIfNeeded(ExpressionSyntax expr)
    {
        // Put parens around anything that could change semantics/precedence when moved into an arg
        // or used as a direct child of another binary expression.
        if (expr is LiteralExpressionSyntax ||
            expr is IdentifierNameSyntax ||
            expr is MemberAccessExpressionSyntax ||
            expr is InvocationExpressionSyntax ||
            expr is ObjectCreationExpressionSyntax ||
            expr is ParenthesizedExpressionSyntax ||
            expr is ElementAccessExpressionSyntax ||
            expr is ThisExpressionSyntax ||
            expr is BaseExpressionSyntax)
        {
            return expr;
        }

        return SyntaxFactory.ParenthesizedExpression(expr.WithoutTrivia())
            .WithTriviaFrom(expr);
    }
    
    private static async Task<Document> EnsureUsingAsync(
        Document document,
        string namespaceName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
            return document;

        // Already has using?
        var hasUsing = compilationUnit.Usings
            .Any(u => u?.Name?.ToString() == namespaceName);

        if (hasUsing)
            return document;

        // Create the new using directive
        var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        // Insert it in alphabetical order or at the top (after existing usings)
        var newRoot = compilationUnit.AddUsings(newUsing);

        return document.WithSyntaxRoot(newRoot);
    }
}