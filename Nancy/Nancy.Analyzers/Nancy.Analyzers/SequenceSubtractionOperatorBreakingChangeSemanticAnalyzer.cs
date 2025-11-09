using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Unipi.Nancy.Analyzers;

/// <summary>
/// An analyzer that reports uses of Subtraction without explicit nonNegative values, to warn of the breaking change.
/// To make sure that we analyze the method of the specific class, we use semantic analysis instead of the syntax tree, so this analyzer will not work if the project is not compilable.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SequenceSubtractionOperatorBreakingChangeSemanticAnalyzer : DiagnosticAnalyzer
{
    private const string CommonApiClassName = "Sequence";
    private const string ChangeIntroducedIn = "1.2.15";

    // Preferred format of DiagnosticId is Your Prefix + Number, e.g. CA1234.
    private const string DiagnosticId = "NANCY0002";

    // Feel free to use raw strings if you don't need localization.
    private static readonly string Title = "Breaking change in operator -";

    // The message that will be displayed to the user.
    private static readonly string MessageFormat = "Breaking change: the result may be negative";

    private static readonly string Description = $"Starting from version {ChangeIntroducedIn}, the subtraction operator may produce negative results by default. See ToNonNegative() for a non-negative closure.";

    // The category of the diagnostic (Design, Naming etc.).
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Subscribe to semantic (compile time) action invocation, e.g. method invocation.
        context.RegisterOperationAction(AnalyzeOperation, OperationKind.BinaryOperator);

        // Check other 'context.Register...' methods that might be helpful for your purposes.
    }

    /// <summary>
    /// Executed on the completion of the semantic analysis associated with the Invocation operation.
    /// </summary>
    /// <param name="context">Operation context.</param>
    private void AnalyzeOperation(OperationAnalysisContext context)
    {
        if (context.Operation is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Subtract } binaryOperation)
            return;
        
        if (binaryOperation.OperatorMethod?.ReceiverType?.Name != CommonApiClassName)
            return;
        
        // The highlighted area in the analyzed source code. Keep it as specific as possible.
        var location = binaryOperation.Syntax switch {
            BinaryExpressionSyntax bes => bes.OperatorToken.GetLocation(),
            _ => binaryOperation.Syntax.GetLocation()
        };
        
        var diagnostic = Diagnostic.Create(Rule,
            location,
            // The value is passed to the 'MessageFormat' argument of your rule.
            "1.2.15");

        // Reporting a diagnostic is the primary outcome of analyzers.
        context.ReportDiagnostic(diagnostic);
    }
}