using Antlr4.Runtime;
using Unipi.Nancy.Expressions.Grammar;

namespace Unipi.Nancy.Expressions.Equivalences;

public partial class Equivalence
{
    /// <summary>
    /// Reads a set of equivalences from a text file.
    /// </summary>
    /// <param name="fileName">Path to text file.</param>
    /// <returns></returns>
    public static List<Equivalence> ReadEquivalencesFromFile(string fileName)
    {
        var equivalenceCatalog = File.ReadAllText(fileName);
        return ReadEquivalences(equivalenceCatalog);
    }

    /// <summary>
    /// Reads a set of equivalences from a string.
    /// </summary>
    /// <param name="equivalenceCatalog">The string containing the equivalences.</param>
    /// <returns></returns>
    public static List<Equivalence> ReadEquivalences(string equivalenceCatalog)
    {
        List<Equivalence> equivalenceList = [];
        var inputStream = new AntlrInputStream(equivalenceCatalog);
        var lexer = new NetCalGLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new NetCalGParser(commonTokenStream);
        var equivalences = parser.equivalenceCatalog().equivalence();
        var visitor = new EquivalenceGrammarVisitor();
        if (equivalences.Length > 0)
            equivalenceList.AddRange(equivalences.Select(equivalence => (Equivalence)visitor.Visit(equivalence)));

        return equivalenceList;
    }
}