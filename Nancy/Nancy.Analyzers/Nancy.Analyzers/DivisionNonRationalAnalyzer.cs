using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Unipi.Nancy.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DivisionPrecisionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NANCY0005";

    private static readonly string Title =
        "Division between decimal and/or integer types may lose precision";

    private static readonly string MessageFormat =
        "Division between '{0}' and '{1}' may lose precision. Consider using the Rational numeric type.";

    private static readonly string Description =
        "Dividing decimals and/or integers can round or truncate results. Prefer an exact Rational representation.";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticId,
        title: Title,
        messageFormat: MessageFormat,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeBinaryDivision, OperationKind.BinaryOperator);
    }

    private static void AnalyzeBinaryDivision(OperationAnalysisContext context)
    {
        var binaryOperation = (IBinaryOperation)context.Operation;

        if (binaryOperation.OperatorKind != BinaryOperatorKind.Divide)
            return;

        var leftType = binaryOperation.LeftOperand?.Type;
        var rightType = binaryOperation.RightOperand?.Type;
        if (leftType is null || rightType is null)
            return;

        // Skip if either side already uses a Rational-like type.
        if (IsRational(leftType) || IsRational(rightType))
            return;

        // Flag only when both operands are decimal and/or integral.
        if (IsCandidateNumeric(leftType) && IsCandidateNumeric(rightType))
        {
            var location = binaryOperation.Syntax.GetLocation();
            var diag = Diagnostic.Create(
                Rule,
                location,
                leftType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                rightType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

            context.ReportDiagnostic(diag);
        }
    }

    private static bool IsCandidateNumeric(ITypeSymbol type)
        => IsIntegral(type) || IsDecimal(type);

    private static bool IsIntegral(ITypeSymbol type)
    {
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

        // Native ints (nint/nuint) are not SpecialType; Roslyn marks them via IsNativeIntegerType.
        if (type is INamedTypeSymbol named2 && named2.IsNativeIntegerType)
            return true;

        return false;
    }

    private static bool IsDecimal(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Decimal;

    private static bool IsRational(ITypeSymbol type)
    {
        // Treat any type literally named 'Rational' (in any namespace) as the safe/desired one.
        // Adjust this to a fully-qualified check if you have a specific Rational type.
        return type.Name == "Rational" || type.ToDisplayString().EndsWith(".Rational");
    }
}