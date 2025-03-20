using Antlr4.Runtime;
using Unipi.Nancy.Expressions.Grammar;

namespace Unipi.Nancy.Expressions.Equivalences;

public partial class Equivalence
{
    public static List<Equivalence> ReadEquivalencesFromFile(string fileName)
    {
        var equivalenceCatalog = File.ReadAllText(fileName);
        return ReadEquivalencesFromFile(equivalenceCatalog);
    }

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