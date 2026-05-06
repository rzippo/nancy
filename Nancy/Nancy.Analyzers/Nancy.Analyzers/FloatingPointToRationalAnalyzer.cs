using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Unipi.Nancy.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FloatingPointToRationalAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NANCY0006";

    private static readonly string Title =
        "Avoid constructing rationals from floating-point values";

    private static readonly string MessageFormat =
        "Avoid converting '{0}' to '{1}'. Prefer decimal numbers for exact decimal translation, or construct the rational directly.";

    private static readonly string Description =
        "Floating-point values can carry binary imprecision. Converting them to rationals can preserve that imprecision and produce large rational values. Prefer decimal numbers, or construct Rational values directly.";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(
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

        context.RegisterOperationAction(AnalyzeConversion, OperationKind.Conversion);
        context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);
    }

    private static void AnalyzeConversion(OperationAnalysisContext context)
    {
        var conversion = (IConversionOperation)context.Operation;
        var sourceType = conversion.Operand.Type;
        var targetType = conversion.Type;

        if (!IsFloatingPoint(sourceType) || !IsNancyRational(targetType))
            return;

        ReportDiagnostic(context, conversion.Syntax.GetLocation(), sourceType!, targetType!);
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;

        if (!IsNancyRational(objectCreation.Type) || objectCreation.Arguments.Length != 1)
            return;

        var argument = objectCreation.Arguments[0];
        var sourceType = argument.Value.Type;
        if (!IsFloatingPoint(sourceType))
            return;

        ReportDiagnostic(context, argument.Syntax.GetLocation(), sourceType!, objectCreation.Type!);
    }

    private static void ReportDiagnostic(
        OperationAnalysisContext context,
        Location location,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var diagnostic = Diagnostic.Create(
            Rule,
            location,
            sourceType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            targetType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsFloatingPoint(ITypeSymbol? type)
    {
        return type?.SpecialType is SpecialType.System_Single or SpecialType.System_Double;
    }

    private static bool IsNancyRational(ITypeSymbol? type)
    {
        type = UnwrapNullable(type);
        if (type is not INamedTypeSymbol namedType)
            return false;

        if (namedType.Name is not ("Rational" or "BigRational" or "LongRational"))
            return false;

        return namedType.ContainingNamespace.ToDisplayString() == "Unipi.Nancy.Numerics";
    }

    private static ITypeSymbol? UnwrapNullable(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
                TypeArguments.Length: 1
            } nullableType)
        {
            return nullableType.TypeArguments[0];
        }

        return type;
    }
}
