using Unipi.Nancy.Expressions.Grammar;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Visitor class which translates a textual equivalence, written using the new grammar defined in the library, to a
/// C# object of type Equivalence.
/// </summary>
public class EquivalenceGrammarVisitor : NetCalGBaseVisitor<object>
{
    /// <summary>
    /// Private field containing the list of placeholder strings found in the equivalence (to check if the same
    /// placeholders are found on both the left side and the right side of the equivalence)
    /// </summary>
    private readonly List<string> _placeholders = [];

    /// <inheritdoc /> 
    public override object VisitEquivalence(NetCalGParser.EquivalenceContext context)
    {
        // Parse the equivalence from text and set left and right expressions of the equivalence object
        var equivalenceExpr = context.equivalenceExpression();
        var left = (CurveExpression)equivalenceExpr.curveExpression()[0].Accept(this);
        var right = (CurveExpression)equivalenceExpr.curveExpression()[1].Accept(this);
        var equivalence = new Equivalence(left, right);

        // Parse hypothesis and add them to the equivalence
        var hypothesisList = context.hypothesis();
        foreach (var hyp in hypothesisList)
        {
            var placeholder = hyp.placeholder()[0].GetText();
            if (hyp.IN() != null) // placeholder IN set (property)* 
            {
                if (!_placeholders.Contains(placeholder))
                    _placeholders.Add(placeholder);
                if (hyp.set().U_SET() != null) // The hypothesis refers to a curve expression
                {
                    var properties = hyp.property();
                    foreach (var prop in properties)
                    {
                        var parsedProperty = prop.Accept(this);
                        if (parsedProperty is Predicate<CurveExpression> predicate)
                        {
                            equivalence.AddHypothesis(placeholder, predicate);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Only the set U is currently supported!");
                }
            }
            else if (hyp.CONV() != null) // placeholder CONV placeholder WELL_DEFINED
            {
                var placeholder1 = hyp.placeholder()[0].GetText();
                var placeholder2 = hyp.placeholder()[1].GetText();

                if (!_placeholders.Contains(placeholder1) || !_placeholders.Contains(placeholder2))
                    throw new InvalidOperationException("Well defined property contains the wrong placeholders!");

                equivalence.AddHypothesis(placeholder1, placeholder2, Property);

                bool Property(CurveExpression f, CurveExpression g) => Expressions.Convolution(f, g).IsWellDefined;
            }
            else // placeholder relationalOperator placeholder
            {
                var placeholder1 = hyp.placeholder()[0].GetText();
                var placeholder2 = hyp.placeholder()[1].GetText();


                if (!_placeholders.Contains(placeholder1) || !_placeholders.Contains(placeholder2))
                    throw new InvalidOperationException("Relational operator contains the wrong placeholders!");

                equivalence.AddHypothesis(placeholder1, placeholder2,
                    (Func<CurveExpression, CurveExpression, bool>)hyp.relationalOperator().Accept(this));
            }
        }

        return equivalence;
    }

    /// <inheritdoc /> 
    public override object VisitConvolutionExpression(NetCalGParser.ConvolutionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Convolution(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitDeconvolutionExpression(NetCalGParser.DeconvolutionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Deconvolution(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitMaxPlusConvolutionExpression(NetCalGParser.MaxPlusConvolutionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.MaxPlusConvolution(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitMaxPlusDeconvolutionExpression(
        NetCalGParser.MaxPlusDeconvolutionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.MaxPlusDeconvolution(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitMinimumExpression(NetCalGParser.MinimumExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Minimum(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitMaximumExpression(NetCalGParser.MaximumExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Maximum(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitAdditionExpression(NetCalGParser.AdditionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Addition(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitSubtractionExpression(NetCalGParser.SubtractionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Subtraction(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitCompositionExpression(NetCalGParser.CompositionExpressionContext context)
    {
        var left = (CurveExpression)context.curveExpression()[0].Accept(this);
        var right = (CurveExpression)context.curveExpression()[1].Accept(this);
        return Expressions.Composition(left, right);
    }

    /// <inheritdoc /> 
    public override object VisitParenthesizedExpression(NetCalGParser.ParenthesizedExpressionContext context)
    {
        return context.curveExpression().Accept(this);
    }

    /// <inheritdoc /> 
    public override object VisitConstantCurve(NetCalGParser.ConstantCurveContext context)
    {
        return Expressions.Placeholder(context.placeholder().GetText());
    }

    /// <inheritdoc /> 
    public override object VisitNonDecreasingProperty(NetCalGParser.NonDecreasingPropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsNonDecreasing;
    }

    /// <inheritdoc /> 
    public override object VisitNonNegativeProperty(NetCalGParser.NonNegativePropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsNonNegative;
    }

    /// <inheritdoc /> 
    public override object VisitSubadditiveProperty(NetCalGParser.SubadditivePropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsSubAdditive;
    }

    /// <inheritdoc /> 
    public override object VisitConvexProperty(NetCalGParser.ConvexPropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsConvex;
    }

    /// <inheritdoc /> 
    public override object VisitConcaveProperty(NetCalGParser.ConcavePropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsConcave;
    }

    /// <inheritdoc /> 
    public override object VisitLeftContinuousProperty(NetCalGParser.LeftContinuousPropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsLeftContinuous;
    }

    /// <inheritdoc /> 
    public override object VisitRightContinuousProperty(NetCalGParser.RightContinuousPropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsRightContinuous;
    }

    /// <inheritdoc /> 
    public override object VisitZeroAtZeroProperty(NetCalGParser.ZeroAtZeroPropertyContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsZeroAtZero;
    }

    /// <inheritdoc /> 
    public override object VisitUltimatelyConstant(NetCalGParser.UltimatelyConstantContext context)
    {
        return (Predicate<CurveExpression>)Property;

        bool Property(CurveExpression expression) => expression.IsUltimatelyConstant();
    }

    /// <inheritdoc /> 
    public override object VisitRelationalOperator(NetCalGParser.RelationalOperatorContext context)
    {
        if (context.GREATER_OR_EQUAL() != null)
            return (CurveExpression f, CurveExpression g) => f >= g;

        if (context.LESS_THAN_OR_EQUAL() != null)
            return (CurveExpression f, CurveExpression g) => f <= g;

        throw new ArgumentException(context.GetText() + " not supported!", nameof(context));
    }
}