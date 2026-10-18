using Microsoft.CodeAnalysis;

namespace Unipi.Nancy.Analyzers;

/// <summary>
/// Recognizes the types of Nancy's <c>Unipi.Nancy.MinPlusAlgebra</c> namespace, rather than any type of the same name.
/// </summary>
internal static class MinPlusAlgebraTypes
{
    private const string Namespace = "Unipi.Nancy.MinPlusAlgebra";

    /// <summary>
    /// True if <paramref name="type"/> is the type <paramref name="name"/> of <c>Unipi.Nancy.MinPlusAlgebra</c>.
    /// </summary>
    public static bool Is(ITypeSymbol? type, string name)
        => type is not null &&
           type.Name == name &&
           type.ContainingNamespace.ToDisplayString() == Namespace;
}
