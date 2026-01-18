using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.ExpressionsUtility.Internals;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// The class aims at providing the main methods to build, manipulate and print expressions which evaluate to rational
/// numbers and are based on NetCal curves.
/// </summary>
public abstract record RationalExpression : IGenericExpression<Rational>, IVisitableRational
{
    #region Properties

    /// <inheritdoc />
    public string Name { get; init; }

    /// <inheritdoc />
    public ExpressionSettings? Settings { get; init; }

    /// <summary>
    /// Private cache field for <see cref="Value"/>
    /// </summary>
    internal Rational? _value;

    /// <summary>
    /// The class aims at providing the main methods to build, manipulate and print expressions which evaluate to rational
    /// numbers and are based on NetCal curves.
    /// </summary>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings"></param>
    protected RationalExpression(
        string expressionName = "", 
        ExpressionSettings? settings = null
    )
    {
        Name = expressionName;
        Settings = settings;
    }

    /// <inheritdoc />
    public Rational Value => _value ??= Compute();
    
    /// <inheritdoc cref="IExpression.IsComputed"/>
    public bool IsComputed
        => _value != null;
    
    #endregion Properties

    #region Constructors

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other"></param>
    /// <remarks>
    /// This constructor was made explicit to *not* copy the private cache fields when using the with operator.
    /// </remarks>
    public RationalExpression(RationalExpression other)
    {
        Name = other.Name;
    }

    #endregion Constructors

    #region Methods

    /// <inheritdoc />
    public Rational Compute() => _value ??= new RationalExpressionEvaluator().GetResult(this);

    /// <inheritdoc cref="IExpression.ComputeWithoutResult"/>
    public void ComputeWithoutResult()
    {
        Compute();
    }

    #region Accept

    /// <inheritdoc />
    public void Accept(IExpressionVisitor<Rational> visitor)
        => Accept((IRationalExpressionVisitor)visitor);

    /// <summary>
    /// Method used for implementing the Visitor design pattern: the visited object must "accept" the visitor object.
    /// </summary>
    /// <param name="visitor">The Visitor object</param>
    public abstract void Accept(IRationalExpressionVisitor visitor);

    /// <inheritdoc />
    public TResult Accept<TResult>(IExpressionVisitor<Rational, TResult> visitor)
        => Accept((IRationalExpressionVisitor<TResult>)visitor);

    /// <inheritdoc />
    public abstract TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor);

    #endregion Accept

    /// <inheritdoc />
    public string ToLatexString(int depth = 20, bool showRationalsAsName = false)
    {
        var latexFormatterVisitor = new LatexFormatterVisitor(depth, showRationalsAsName);
        var (sb, _) = Accept(latexFormatterVisitor);
        var latexExpr = sb.ToString();

        return latexExpr;
    }

    /// <inheritdoc />
    public string ToUnicodeString(int depth = 20, bool showRationalsAsName = false)
    {
        var unicodeFormatterVisitor = new UnicodeFormatterVisitor(depth, showRationalsAsName);
        var (sb, _) = Accept(unicodeFormatterVisitor);
        var unicodeExpr = sb.ToString();

        return unicodeExpr;
    }

    /// <inheritdoc />
    public sealed override string ToString()
        => ToUnicodeString();

    /// <inheritdoc />
    public double Estimate()
    {
        throw new NotImplementedException();
    }

    #region Replace

    /// <summary>
    /// Replaces every occurence of a sub-expression in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <param name="ignoreNotMatchedExpressions"></param>
    /// <returns>New expression object (of type <see cref="RationalExpression"/>) with replaced sub-expressions.</returns>
    public RationalExpression ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions
    )
    {
        var replacer = new OneTimeExpressionReplacer<Rational, T1>(this, newExpressionToReplace);
        return (RationalExpression)replacer.ReplaceByValue(
            expressionPattern,
            ignoreNotMatchedExpressions
        );
    }

    IGenericExpression<Rational> IGenericExpression<Rational>.ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions) 
        => ReplaceByValue(expressionPattern, newExpressionToReplace, ignoreNotMatchedExpressions);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>New expression object (of type <see cref="RationalExpression"/>) with replaced sub-expression.</returns>
    public RationalExpression ReplaceByPosition<T1>(ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpressionToReplace)
        => ReplaceByPosition(expressionPosition.GetPositionPath(), newExpressionToReplace);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>New expression object (of type <see cref="RationalExpression"/>) with replaced sub-expression.</returns>
    IGenericExpression<Rational> IGenericExpression<Rational>.ReplaceByPosition<T1>(
        ExpressionPosition expressionPosition, IGenericExpression<T1> newExpressionToReplace) =>
        ReplaceByPosition(expressionPosition, newExpressionToReplace);
    
    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newValueToReplace">The new value to replace the sub-expression.</param>
    /// /// <param name="name">The name of the new value.</param>
    /// <returns>New expression object (of type <see cref="RationalExpression"/>) with replaced sub-expression.</returns>
    public RationalExpression ReplaceByPosition(ExpressionPosition expressionPosition,
        Curve newValueToReplace,
        [CallerArgumentExpression("newValueToReplace")] string name = ""
    )
        => ReplaceByPosition(expressionPosition.GetPositionPath(), newValueToReplace.ToExpression(name));

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newValueToReplace">The new value to replace the sub-expression.</param>
    /// <param name="name">The name of the new value.</param>
    /// <returns>New expression object (of type <see cref="RationalExpression"/>) with replaced sub-expression.</returns>
    public RationalExpression ReplaceByPosition(ExpressionPosition expressionPosition,
        Rational newValueToReplace,
        [CallerArgumentExpression("newValueToReplace")] string name = ""
    )
        => ReplaceByPosition(expressionPosition.GetPositionPath(), newValueToReplace.ToExpression(name));

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="positionPath">Position of the expression to be replaced. The position is expressed as a path from
    /// the root of the expression by using a list of strings "Operand" for unary operators, "LeftOperand"/"RightOperand"
    /// for binary operators, "Operand(index)" for n-ary operators.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>New expression object (of type <see cref="RationalExpression"/>) with the replaced sub-expression.
    /// </returns>
    public RationalExpression ReplaceByPosition<T1>(IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpressionToReplace)
    {
        var replacer = new OneTimeExpressionReplacer<Rational, T1>(this, newExpressionToReplace);
        return (RationalExpression)replacer.ReplaceByPosition(positionPath);
    }

    IGenericExpression<Rational> IGenericExpression<Rational>.ReplaceByPosition<T1>(IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpressionToReplace) => ReplaceByPosition(positionPath, newExpressionToReplace);

    #endregion Replace

    /// <inheritdoc />
    public ExpressionPosition RootPosition() => new();

    /// <summary>
    /// Changes the name of the expression.
    /// </summary>
    /// <param name="expressionName">The new name of the expression</param>
    /// <returns>
    /// The expression (new object of type <see cref="RationalExpression"/>) with the new name.
    /// </returns>
    /// <remarks>
    /// Renaming can be done using the with operator, but that will clear out the cache fields, causing re-computation of the expression.
    /// This method will instead copy over the cache fields.
    /// </remarks>
    public RationalExpression WithName(string expressionName)
    {
        var changeNameVisitor = new RenameRationalVisitor(expressionName);
        Accept(changeNameVisitor);

        return changeNameVisitor.Result;
    }

    IGenericExpression<Rational> IGenericExpression<Rational>.WithName(string expressionName) =>
        WithName(expressionName);

    /// <summary>
    /// This operator returns true if the value of a rational expression is below or equal than the value of another one.
    /// </summary>
    public static bool operator <=(RationalExpression expressionL, RationalExpression expressionR)
        => expressionL.Compute() <= expressionR.Compute();

    /// <summary>
    /// This operator returns true if the value of a rational expression is greater or equal than the value of another
    /// one.</summary>
    public static bool operator >=(RationalExpression expressionL, RationalExpression expressionR)
        => expressionL.Compute() >= expressionR.Compute();

    /// <summary>
    /// Adds the negation operator to the expression.
    /// </summary>
    public RationalExpression Negate(string expressionName = "", ExpressionSettings? settings = null)
        => new NegateRationalExpression(this, expressionName, settings);

    /// <summary>
    /// Adds the inversion operator to the expression.
    /// </summary>
    public RationalExpression Invert(string expressionName = "", ExpressionSettings? settings = null)
        => new InvertRationalExpression(this, expressionName, settings);

    #region Addition

    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the one passed as argument.
    /// </summary>
    public RationalExpression Addition(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(RationalAdditionExpression), this, expression) switch
        {
            1 => ((RationalAdditionExpression)this).Append(expression, expressionName, settings),
            2 => ((RationalAdditionExpression)expression).Append(this, expressionName, settings),
            _ => new RationalAdditionExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the addition between the expression <paramref name="left"/> and the expression
    /// <paramref name="right"/> passed as arguments.</summary>
    public static RationalExpression Addition(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.Addition(right, expressionName, settings:settings);

    /// <summary>
    /// Implementation of the + operator as the addition between <see cref="RationalExpression"/> objects.
    /// </summary>
    public static RationalExpression operator +(RationalExpression left, RationalExpression right)
        => Addition(left, right);

    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the rational number
    /// (internally converted to <see cref="RationalNumberExpression"/>) passed as argument.
    /// </summary>
    public RationalExpression Addition(Rational rational, [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is RationalAdditionExpression e)
            return e.Append(new RationalNumberExpression(rational, name), expressionName, settings);
        return new RationalAdditionExpression([this, new RationalNumberExpression(rational, name)], expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression composed of the addition between the expression <paramref name="left"/> and the rational
    /// number <paramref name="right"/> (internally converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression Addition(RationalExpression left, Rational right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Addition(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Implementation of the + operator as the addition between a rational expression (<paramref name="left"/>) and a
    /// rational number (<paramref name="right"/>), which is internally converted to a <see cref="RationalNumberExpression"/>.
    /// </summary>
    public static RationalExpression operator +(RationalExpression left, Rational right)
        => Addition(left, right);

    #endregion Addition

    #region Subtraction

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the one passed as
    /// argument.
    /// </summary>
    public RationalExpression Subtraction(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new RationalSubtractionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the expression <paramref name="left"/> and the
    /// expression <paramref name="right"/> passed as arguments.
    /// </summary>
    public static RationalExpression Subtraction(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.Subtraction(right, expressionName, settings);

    /// <summary>
    /// Implementation of the - operator as the subtraction between <see cref="RationalExpression"/> objects.
    /// </summary>
    public static RationalExpression operator -(RationalExpression left, RationalExpression right)
        => Subtraction(left, right);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the rational number
    /// (internally converted to <see cref="RationalNumberExpression"/>) passed as argument.
    /// </summary>
    public RationalExpression Subtraction(Rational rational, [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        return new RationalSubtractionExpression(this, new RationalNumberExpression(rational, name), expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression composed of the subtraction between the expression <paramref name="left"/> and the rational
    /// number <paramref name="right"/> (internally converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression Subtraction(RationalExpression left, Rational right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Subtraction(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Implementation of the - operator as the subtraction between a rational expression and a ration number
    /// (internally converted to a <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression operator -(RationalExpression left, Rational right)
        => Subtraction(left, right);

    #endregion Subtraction

    #region Product

    /// <summary>
    /// Creates a new expression composed of the product between the current expression and the one passed as
    /// argument.
    /// </summary>
    public RationalExpression Product(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(RationalProductExpression), this, expression) switch
        {
            1 => ((RationalProductExpression)this).Append(expression, expressionName, settings),
            2 => ((RationalProductExpression)expression).Append(this, expressionName, settings),
            _ => new RationalProductExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the product between the expression <paramref name="left"/> and the expression
    /// <paramref name="right"/> passed as arguments.</summary>
    public static RationalExpression Product(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.Product(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Implementation of the * operator as the product between <see cref="RationalExpression"/> objects.
    /// </summary>
    public static RationalExpression operator *(RationalExpression left, RationalExpression right)
        => Product(left, right);

    /// <summary>
    /// Creates a new expression composed of the product between the current expression and the rational number
    /// (internally converted to <see cref="RationalNumberExpression"/>) passed as argument.
    /// </summary>
    public RationalExpression Product(Rational rational, [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is RationalProductExpression e)
            return e.Append(new RationalNumberExpression(rational, name), expressionName, settings);
        return new RationalProductExpression([this, new RationalNumberExpression(rational, name)], expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression composed of the product between the expression <paramref name="left"/> and the rational
    /// number <paramref name="right"/> (internally converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression Product(RationalExpression left, Rational right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Product(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Implementation of the * operator as the product between a rational expression (<paramref name="left"/>) and a
    /// rational number (<paramref name="right"/>), which is internally converted to a <see cref="RationalNumberExpression"/>.
    /// </summary>
    public static RationalExpression operator *(RationalExpression left, Rational right)
        => Product(left, right);

    #endregion Product

    #region Division

    /// <summary>
    /// Creates a new expression composed of the division between the current expression and the one passed as
    /// argument.
    /// </summary>
    public RationalExpression Division(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new RationalDivisionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the division between the expression <paramref name="left"/> and the expression
    /// <paramref name="right"/> passed as arguments.</summary>
    public static RationalExpression Division(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.Division(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Implementation of the / operator as the division between <see cref="RationalExpression"/> objects.
    /// </summary>
    public static RationalExpression operator /(RationalExpression left, RationalExpression right)
        => Division(left, right);

    /// <summary>
    /// Creates a new expression composed of the division between the current expression and the rational number
    /// (internally converted to <see cref="RationalNumberExpression"/>) passed as argument.
    /// </summary>
    public RationalExpression Division(Rational rational, [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        return new RationalDivisionExpression(
            this, new RationalNumberExpression(rational, name),
            expressionName, settings
        );
    }

    /// <summary>
    /// Creates a new expression composed of the division between the expression <paramref name="left"/> and the rational
    /// number <paramref name="right"/> (internally converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression Division(RationalExpression left, Rational right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Division(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Implementation of the / operator as the division between a rational expression (<paramref name="left"/>) and a
    /// rational number (<paramref name="right"/>), which is internally converted to a <see cref="RationalNumberExpression"/>.
    /// </summary>
    public static RationalExpression operator /(RationalExpression left, Rational right)
        => Division(left, right);

    #endregion Division

    #region LeastCommonMultiple

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the current expression and the one passed as
    /// argument.
    /// </summary>
    public RationalExpression LeastCommonMultiple(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(RationalLeastCommonMultipleExpression), this, expression) switch
        {
            1 => ((RationalLeastCommonMultipleExpression)this).Append(expression, expressionName, settings),
            2 => ((RationalLeastCommonMultipleExpression)expression).Append(this, expressionName, settings),
            _ => new RationalLeastCommonMultipleExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the expression <paramref name="left"/> and the expression
    /// <paramref name="right"/> passed as arguments.</summary>
    public static RationalExpression LeastCommonMultiple(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.LeastCommonMultiple(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the current expression and the rational number
    /// (internally converted to <see cref="RationalNumberExpression"/>) passed as argument.
    /// </summary>
    public RationalExpression LeastCommonMultiple(Rational rational,
        [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is RationalLeastCommonMultipleExpression e)
            return e.Append(new RationalNumberExpression(rational, name), expressionName, settings);
        return new RationalLeastCommonMultipleExpression([this, new RationalNumberExpression(rational, name)],
            expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the expression <paramref name="left"/> and the rational
    /// number <paramref name="right"/> (internally converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression LeastCommonMultiple(RationalExpression left, Rational right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.LeastCommonMultiple(right, expressionName:expressionName, settings:settings);

    #endregion LeastCommonMultiple

    #region GreatestCommonDivisor

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the current expression and the one passed as
    /// argument.
    /// </summary>
    public RationalExpression GreatestCommonDivisor(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(RationalGreatestCommonDivisorExpression), this, expression) switch
        {
            1 => ((RationalGreatestCommonDivisorExpression)this).Append(expression, expressionName, settings),
            2 => ((RationalGreatestCommonDivisorExpression)expression).Append(this, expressionName, settings),
            _ => new RationalGreatestCommonDivisorExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the expression <paramref name="left"/> and the expression
    /// <paramref name="right"/> passed as arguments.</summary>
    public static RationalExpression GreatestCommonDivisor(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.GreatestCommonDivisor(right);

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the current expression and the rational number
    /// (internally converted to <see cref="RationalNumberExpression"/>) passed as argument.
    /// </summary>
    public RationalExpression GreatestCommonDivisor(Rational rational,
        [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is RationalGreatestCommonDivisorExpression e)
            return e.Append(new RationalNumberExpression(rational, name), expressionName, settings);
        return new RationalGreatestCommonDivisorExpression([this, new RationalNumberExpression(rational, name)],
            expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the expression <paramref name="left"/> and the rational
    /// number <paramref name="right"/> (internally converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression GreatestCommonDivisor(RationalExpression left, Rational right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.GreatestCommonDivisor(right);

    #endregion GreatestCommonDivisor

    #region Minimum

    /// <summary>
    /// Creates a new expression that computes the minimum between the current expression and the one passed as argument.
    /// </summary>
    public RationalExpression Min(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(RationalMinimumExpression), this, expression) switch
        {
            1 => ((RationalMinimumExpression)this).Append(expression, expressionName, settings),
            2 => ((RationalMinimumExpression)expression).Append(this, expressionName, settings),
            _ => new RationalMinimumExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression that computes the minimum between the expressions <paramref name="left"/> and <paramref name="right"/> passed as arguments.
    /// </summary>
    public static RationalExpression Min(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.Min(right, expressionName, settings:settings);

    /// <summary>
    /// Creates a new expression that computes the minimum between the current expression and the rational number passed as argument.
    /// </summary>
    public RationalExpression Min(Rational rational, [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is RationalMinimumExpression e)
            return e.Append(new RationalNumberExpression(rational, name), expressionName, settings);
        return new RationalMinimumExpression([this, new RationalNumberExpression(rational, name)], expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression that computes the minimum between the expression <paramref name="left"/> and the rational number <paramref name="right"/> passed as arguments.
    /// </summary>
    public static RationalExpression Min(RationalExpression left, Rational right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Min(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Creates a new expression that computes the minimum between the two rational numbers passed as arguments
    /// (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression Min(Rational rationalL, Rational rationalR,
        [CallerArgumentExpression("rationalL")] string nameL = "", [CallerArgumentExpression("rationalR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalMinimumExpression([rationalL, rationalR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the minimum between the rational number <paramref name="rationalL"/> and the
    /// expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression Min(Rational rationalL, RationalExpression expressionR,
        [CallerArgumentExpression("rationalL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalNumberExpression(rationalL, nameL).Min(expressionR, expressionName, settings);
    
    #endregion Minimum
    
    #region Maximum

    /// <summary>
    /// Creates a new expression that computes the maximum between the current expression and the one passed as argument.
    /// </summary>
    public RationalExpression Max(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(RationalMaximumExpression), this, expression) switch
        {
            1 => ((RationalMaximumExpression)this).Append(expression, expressionName, settings),
            2 => ((RationalMaximumExpression)expression).Append(this, expressionName, settings),
            _ => new RationalMaximumExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression that computes the maximum between the expressions <paramref name="left"/> and <paramref name="right"/> passed as arguments.
    /// </summary>
    public static RationalExpression Max(RationalExpression left, RationalExpression right,
        string expressionName = "", ExpressionSettings? settings = null)
        => left.Max(right, expressionName, settings:settings);

    /// <summary>
    /// Creates a new expression that computes the maximum between the current expression and the rational number passed as argument.
    /// </summary>
    public RationalExpression Max(Rational rational, [CallerArgumentExpression("rational")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is RationalMaximumExpression e)
            return e.Append(new RationalNumberExpression(rational, name), expressionName, settings);
        return new RationalMaximumExpression([this, new RationalNumberExpression(rational, name)], expressionName,
            settings);
    }

    /// <summary>
    /// Creates a new expression that computes the maximum between the expression <paramref name="left"/> and the rational number <paramref name="right"/> passed as arguments.
    /// </summary>
    public static RationalExpression Max(RationalExpression left, Rational right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Max(right, expressionName:expressionName, settings:settings);

    /// <summary>
    /// Creates a new expression that computes the maximum between the two rational numbers passed as arguments
    /// (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression Max(Rational rationalL, Rational rationalR,
        [CallerArgumentExpression("rationalL")] string nameL = "", [CallerArgumentExpression("rationalR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalMaximumExpression([rationalL, rationalR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the maximum between the rational number <paramref name="rationalL"/> and the
    /// expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression Max(Rational rationalL, RationalExpression expressionR,
        [CallerArgumentExpression("rationalL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalNumberExpression(rationalL, nameL).Max(expressionR, expressionName, settings);
    
    #endregion Maximum

    /// <summary>
    /// Private function used during the creation of the expressions to keep the n-ary expressions at the same level
    /// of the expression tree. 
    /// </summary>
    /// <param name="type">Type of the expression that needs to be created</param>
    /// <param name="e1">Left operand</param>
    /// <param name="e2">Right operand</param>
    /// <returns>If <paramref name="type"/> is equal to the type of <paramref name="e1"/> the function returns 1, which means that
    /// the new expression must be created by appending <paramref name="e2"/> to <paramref name="e1"/>.
    /// The opposite holds if <paramref name="type"/> is equal to the type of <paramref name="e2"/>, in this case the function
    /// returns 2.
    /// The function returns 0 when the <paramref name="type"/> is different by the type of <paramref name="e1"/> and
    /// <paramref name="e2"/>.</returns>
    private static int CheckNAryExpressionTypes(Type type, RationalExpression e1, RationalExpression e2)
    {
        if (e1.GetType() == type)
            return 1;
        return e2.GetType() == type ? 2 : 0;
    }

    #region Equivalence

    /// <summary>
    /// Applies an equivalence to the current expression.
    /// </summary>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// </returns>
    public RationalExpression ApplyEquivalence(Equivalence equivalence, CheckType checkType = CheckType.CheckLeftOnly)
    {
        var replacer = new OneTimeExpressionReplacer<Rational, Curve>(this, equivalence, checkType);
        // In the case of equivalences the argument of ReplaceByValue is not significant
        return (RationalExpression)replacer.ReplaceByValue(equivalence.LeftSideExpression);
    }

    IGenericExpression<Rational> IGenericExpression<Rational>.ApplyEquivalence(Equivalence equivalence,
        CheckType checkType)
        => ApplyEquivalence(equivalence, checkType);

    /// <summary>
    /// Applies an equivalence to the current expression, allowing the user to specify the position in the expression in
    /// which the equivalence should be applied.
    /// </summary>
    /// <param name="positionPath">Position of the sub-expression to be replaced with an equivalent one.
    /// The position is expressed as a path from the root of the expression by using a list of strings "Operand" for
    /// unary operators, "LeftOperand"/"RightOperand" for binary operators, "Operand(index)" for n-ary operators</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// </returns>
    public RationalExpression ApplyEquivalenceByPosition(IEnumerable<string> positionPath, Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        var replacer = new OneTimeExpressionReplacer<Rational, Curve>(this, equivalence, checkType);
        return (RationalExpression)replacer.ReplaceByPosition(positionPath);
    }

    IGenericExpression<Rational> IGenericExpression<Rational>.ApplyEquivalenceByPosition(
        IEnumerable<string> positionPath, Equivalence equivalence,
        CheckType checkType)
        => ApplyEquivalenceByPosition(positionPath, equivalence, checkType);

    /// <summary>
    /// Applies an equivalence to the current expression, allowing the user to specify the position in the expression in
    /// which the equivalence should be applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// </returns>
    public RationalExpression ApplyEquivalenceByPosition(ExpressionPosition expressionPosition, Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
        => ApplyEquivalenceByPosition(expressionPosition.GetPositionPath(), equivalence, checkType);

    IGenericExpression<Rational> IGenericExpression<Rational>.ApplyEquivalenceByPosition(
        ExpressionPosition expressionPosition, Equivalence equivalence,
        CheckType checkType)
        => ApplyEquivalenceByPosition(expressionPosition, equivalence, checkType);

    #endregion Equivalence

    #endregion Methods
}