using System.Collections.Concurrent;
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
/// Class which describes NetCal expressions that evaluate to curves. The class aims at providing the main methods to
/// build, manipulate and print network calculus expressions.
/// </summary>
public abstract record CurveExpression : IGenericExpression<Curve>, IVisitableCurve
{
    #region Properties

    public string Name { get; init; }

    /// <summary>
    /// Static dictionary field collecting the well-known equivalences, indexed by the "main" type of equivalence
    /// </summary>
    public static readonly ConcurrentDictionary<Type, List<Equivalence>> Equivalences = new();

    public ExpressionSettings? Settings { get; init; }
    
    /// <summary>
    /// Private cache field for <see cref="Value"/>
    /// </summary>
    internal Curve? _value;

    public Curve Value => _value ??= Compute();

    /// <inheritdoc cref="IExpression.IsComputed"/>
    public bool IsComputed
        => _value != null;
    
    /// <summary>
    /// Private cache field for <see cref="IsSubAdditive"/>.
    /// </summary>
    internal bool? _isSubAdditive;

    /// <summary>
    /// True if the curve described by the expression is sub-additive. Property evaluated avoiding as much as possible
    /// to make any computation.
    /// </summary>
    public bool IsSubAdditive
    {
        get
        {
            return _isSubAdditive ??= CheckIsSubAdditive();

            bool CheckIsSubAdditive()
            {
                var isSubAdditiveVisitor = new IsSubAdditiveVisitor();
                Accept(isSubAdditiveVisitor);

                return isSubAdditiveVisitor.IsSubAdditive;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsLeftContinuous"/>.
    /// </summary>
    internal bool? _isLeftContinuous;

    /// <summary>
    /// True if the curve described by the expression is left continuous. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsLeftContinuous
    {
        get
        {
            return _isLeftContinuous ??= CheckIsLeftContinuous();

            bool CheckIsLeftContinuous()
            {
                var isLeftContinuousVisitor = new IsLeftContinuousVisitor();
                Accept(isLeftContinuousVisitor);

                return isLeftContinuousVisitor.IsLeftContinuous;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsRightContinuous"/>.
    /// </summary>
    internal bool? _isRightContinuous;

    /// <summary>
    /// True if the curve described by the expression is right continuous. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsRightContinuous
    {
        get
        {
            return _isRightContinuous ??= CheckIsRightContinuous();

            bool CheckIsRightContinuous()
            {
                var isRightContinuousVisitor = new IsRightContinuousVisitor();
                Accept(isRightContinuousVisitor);

                return isRightContinuousVisitor.IsRightContinuous;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsNonNegative"/>.
    /// </summary>
    internal bool? _isNonNegative;

    /// <summary>
    /// True if the curve described by the expression is non-negative. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsNonNegative
    {
        get
        {
            return _isNonNegative ??= CheckIsNonNegative();

            bool CheckIsNonNegative()
            {
                var isNonNegativeVisitor = new IsNonNegativeVisitor();
                Accept(isNonNegativeVisitor);

                return isNonNegativeVisitor.IsNonNegative;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsNonDecreasing"/>.
    /// </summary>
    internal bool? _isNonDecreasing;

    /// <summary>
    /// True if the curve described by the expression is non-decreasing. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsNonDecreasing
    {
        get
        {
            return _isNonDecreasing ??= CheckIsNonDecreasing();

            bool CheckIsNonDecreasing()
            {
                var isNonDecreasingVisitor = new IsNonDecreasingVisitor();
                Accept(isNonDecreasingVisitor);

                return isNonDecreasingVisitor.IsNonDecreasing;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsConcave"/>.
    /// </summary>
    internal bool? _isConcave;

    /// <summary>
    /// True if the curve described by the expression is concave. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsConcave
    {
        get
        {
            return _isConcave ??= CheckIsConcave();

            bool CheckIsConcave()
            {
                var isConcaveVisitor = new IsConcaveVisitor();
                Accept(isConcaveVisitor);

                return isConcaveVisitor.IsConcave;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsConvex"/>.
    /// </summary>
    internal bool? _isConvex;

    /// <summary>
    /// True if the curve described by the expression is convex. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsConvex
    {
        get
        {
            return _isConvex ??= CheckIsConvex();

            bool CheckIsConvex()
            {
                var isConvexVisitor = new IsConvexVisitor();
                Accept(isConvexVisitor);

                return isConvexVisitor.IsConvex;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsZeroAtZero"/>.
    /// </summary>
    internal bool? _isZeroAtZero;

    /// <summary>
    /// True if the curve f described by the expression is 0 in 0 (f(0) = 0). Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsZeroAtZero
    {
        get
        {
            return _isZeroAtZero ??= CheckIsZeroAtZero();

            bool CheckIsZeroAtZero()
            {
                var isZeroAtZeroVisitor = new IsZeroAtZeroVisitor();
                Accept(isZeroAtZeroVisitor);

                return isZeroAtZeroVisitor.IsZeroAtZero;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsWellDefined"/>.
    /// </summary>
    internal bool? _isWellDefined;

    /// <summary>
    /// Class which describes NetCal expressions that evaluate to curves. The class aims at providing the main methods to
    /// build, manipulate and print network calculus expressions.
    /// </summary>
    /// <param name="ExpressionName">The name of the expression</param>
    /// <param name="Settings"></param>
    protected CurveExpression(string expressionName = "", ExpressionSettings? settings = null)
    {
        Name = expressionName;
        Settings = settings;
    }

    /// <summary>
    /// True if the operation described by the expression is well-defined according to the definition
    /// in [BT08] Section 2.1.
    /// </summary>
    public bool IsWellDefined
    {
        get
        {
            return _isWellDefined ??= CheckIsWellDefined();

            bool CheckIsWellDefined()
            {
                var isWellDefinedVisitor = new IsWellDefinedVisitor();
                Accept(isWellDefinedVisitor);

                return isWellDefinedVisitor.IsWellDefined;
            }
        }
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Adds the opposite operator to the expression.
    /// </summary>
    public CurveExpression Negate(string expressionName = "", ExpressionSettings? settings = null)
        => new NegateExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute its non-negative version.
    /// </summary>
    public CurveExpression ToNonNegative(string expressionName = "", ExpressionSettings? settings = null)
        => new ToNonNegativeExpression(this, expressionName, settings);

    /// <summary>
    /// Adds the sub-additive closure operator to the expression.
    /// </summary>
    public CurveExpression SubAdditiveClosure(string expressionName = "", ExpressionSettings? settings = null)
        => new SubAdditiveClosureExpression(this, expressionName, settings);

    /// <summary>
    /// Adds the super-additive closure operator to the expression.
    /// </summary>
    public CurveExpression SuperAdditiveClosure(string expressionName = "", ExpressionSettings? settings = null)
        => new SuperAdditiveClosureExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute its upper non-decreasing version.
    /// </summary>
    public CurveExpression ToUpperNonDecreasing(string expressionName = "", ExpressionSettings? settings = null)
        => new ToUpperNonDecreasingExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute its lower non-decreasing version.
    /// </summary>
    public CurveExpression ToLowerNonDecreasing(string expressionName = "", ExpressionSettings? settings = null)
        => new ToLowerNonDecreasingExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute a left continuous version of it.
    /// </summary>
    public CurveExpression ToLeftContinuous(string expressionName = "", ExpressionSettings? settings = null)
        => new ToLeftContinuousExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute a right continuous version of it.
    /// </summary>
    public CurveExpression ToRightContinuous(string expressionName = "", ExpressionSettings? settings = null)
        => new ToRightContinuousExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression an operation which enforces it to assume 0 at time 0.
    /// </summary>
    public CurveExpression WithZeroOrigin(string expressionName = "", ExpressionSettings? settings = null)
        => new WithZeroOriginExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute the lower pseudo-inverse function,
    /// $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    public CurveExpression LowerPseudoInverse(string expressionName = "", ExpressionSettings? settings = null)
        => new LowerPseudoInverseExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute the upper pseudo-inverse function,
    /// $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    public CurveExpression UpperPseudoInverse(string expressionName = "", ExpressionSettings? settings = null)
        => new UpperPseudoInverseExpression(this, expressionName, settings);

    #region Addition
    
    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Addition(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(AdditionExpression), this, expression) switch
        {
            1 => ((AdditionExpression)this).Append(expression, expressionName, settings),
            2 => ((AdditionExpression)expression).Append(this, expressionName, settings),
            _ => new AdditionExpression([this, expression], expressionName, settings)
        };

    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Addition(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is AdditionExpression e)
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new AdditionExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }

    /// <summary>
    /// Creates a new expression composed of the addition between the expression <see cref="left"/> and the curve
    /// <see cref="right"/> (internally converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public static CurveExpression Addition(CurveExpression left, Curve right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Addition(right, expressionName:expressionName, settings: settings);

    /// <summary>
    /// Implementation of the + operator as the addition between <see cref="CurveExpression"/> objects.
    /// </summary>
    public static CurveExpression operator +(CurveExpression left, Curve right)
        => Addition(left, right);

    #endregion Addition

    #region Subtraction
    
    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Subtraction(CurveExpression expression, bool nonNegative = true, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SubtractionExpression(this, expression, nonNegative, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Subtraction(Curve curve, bool nonNegative = true, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Subtraction(new ConcreteCurveExpression(curve, name), nonNegative, expressionName, settings);

    #endregion Subtraction
    
    #region Minimum 
    
    /// <summary>
    /// Creates a new expression composed of the minimum between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Minimum(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(MinimumExpression), this, expression) switch
        {
            1 => ((MinimumExpression)this).Append(expression, expressionName, settings),
            2 => ((MinimumExpression)expression).Append(this, expressionName, settings),
            _ => new MinimumExpression([this, expression], expressionName, settings)
        };

    /// <summary>
    /// Creates a new expression composed of the minimum between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Minimum(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is MinimumExpression e)
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new MinimumExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }

    #endregion Minimum
    
    #region Maximum
    
    /// <summary>
    /// Creates a new expression composed of the maximum between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Maximum(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(MaximumExpression), this, expression) switch
        {
            1 => ((MaximumExpression)this).Append(expression, expressionName, settings),
            2 => ((MaximumExpression)expression).Append(this, expressionName, settings),
            _ => new MaximumExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the maximum between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Maximum(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is MaximumExpression e)
            return e.Append(new ConcreteCurveExpression(curve, name, settings));
        return new MaximumExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }
    
    #endregion Maximum
    
    #region Convolution

    /// <summary>
    /// Creates a new expression composed of the convolution between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Convolution(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(ConvolutionExpression), this, expression) switch
        {
            1 => ((ConvolutionExpression)this).Append(expression, expressionName, settings),
            2 => ((ConvolutionExpression)expression).Append(this, expressionName, settings),
            _ => new ConvolutionExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the convolution between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Convolution(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is ConvolutionExpression e)
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new ConvolutionExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }
    
    #endregion Convolution
    
    #region Deconvolution

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Deconvolution(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DeconvolutionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Deconvolution(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Deconvolution(new ConcreteCurveExpression(curve, name), expressionName, settings);

    #endregion Deconvolution
    
    #region MaxPlusConvolution
    
    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the current expression and the one passed
    /// as argument.
    /// </summary>
    public CurveExpression MaxPlusConvolution(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(MaxPlusConvolutionExpression), this, expression) switch
        {
            1 => ((MaxPlusConvolutionExpression)this).Append(expression, expressionName, settings),
            2 => ((MaxPlusConvolutionExpression)expression).Append(this, expressionName, settings),
            _ => new MaxPlusConvolutionExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression MaxPlusConvolution(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is MaxPlusConvolutionExpression e)
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new MaxPlusConvolutionExpression([this, new ConcreteCurveExpression(curve, name)], expressionName,
            settings);
    }

    #endregion MaxPlusConvolution

    #region MaxPlusDeconvolution
    
    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the current expression and the one
    /// passed as argument.
    /// </summary>
    public CurveExpression MaxPlusDeconvolution(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new MaxPlusDeconvolutionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression MaxPlusDeconvolution(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => MaxPlusDeconvolution(new ConcreteCurveExpression(curve, name), expressionName, settings);
    
    #endregion MaxPlusDeconvolution

    #region Composition
    
    /// <summary>
    /// Creates a new expression composed of the composition between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Composition(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new CompositionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Composition(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Composition(new ConcreteCurveExpression(curve, name), expressionName, settings);

    #endregion Composition
    
    #region DelayBy
    
    /// <summary>
    /// Creates a new expression composed of the operation to delay the curve corresponding to the current expression
    /// by the rational number described by the argument <see cref="expression"/> of type
    /// <see cref="RationalExpression"/>. </summary>
    public CurveExpression DelayBy(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DelayByExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the operation to delay the curve corresponding to the current expression
    /// by the rational number <see cref="delay"/>.</summary>
    public CurveExpression DelayBy(Rational delay, string expressionName = "", ExpressionSettings? settings = null)
        => DelayBy(new RationalNumberExpression(delay), expressionName, settings);

    #endregion DelayBy

    #region ForwardBy

    /// <summary>
    /// Creates a new expression that forwards the current
    /// expression by the rational <see cref="expression"/>, i.e., computing $f(t + T)$. 
    /// </summary>
    public CurveExpression ForwardBy(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DelayByExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the current curve
    /// expression by the rational <see cref="time"/>, i.e., computing $f(t + T)$.
    /// </summary>
    public CurveExpression ForwardBy(Rational time, string expressionName = "", ExpressionSettings? settings = null)
        => ForwardBy(new RationalNumberExpression(time), expressionName, settings);

    #endregion ForwardBy
    
    #region Shift

    /// <summary>
    /// Creates a new expression that shifts the current
    /// expression by the rational <see cref="expression"/>, i.e., computing $f(t) + K$. 
    /// </summary>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public CurveExpression Shift(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new ShiftExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current curve
    /// expression by the rational <see cref="value"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public CurveExpression Shift(Rational value, string expressionName = "", ExpressionSettings? settings = null)
        => Shift(new RationalNumberExpression(value), expressionName, settings);

    #endregion Shift
    
    #region Scale
    
    /// <summary>
    /// Creates a new expression composed of the operation to scale the curve corresponding to the current expression
    /// by the rational number described by the argument <see cref="expression"/> of type
    /// <see cref="RationalExpression"/>. </summary>
    public CurveExpression Scale(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new ScaleExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the operation to scale the curve corresponding to the current expression
    /// by the rational number <see cref="scaleFactor"/>.</summary>
    public CurveExpression Scale(Rational scaleFactor, string expressionName = "", ExpressionSettings? settings = null)
        => Scale(new RationalNumberExpression(scaleFactor), expressionName, settings);

    #endregion Scale

    public Curve Compute() => _value ??= new CurveExpressionEvaluator().GetResult(this);

    /// <inheritdoc cref="IExpression.ComputeWithoutResult"/>
    public void ComputeWithoutResult()
    {
        Compute();
    }

    #region Replace

    /// <summary>
    /// Replaces every occurence of a sub-expression in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <param name="ignoreNotMatchedExpressions"></param>
    /// <returns>New expression object (of type <see cref="CurveExpression"/>) with replaced sub-expressions.</returns>
    public CurveExpression ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions = false
    )
    {
        var replacer = new OneTimeExpressionReplacer<Curve, T1>(this, newExpressionToReplace);
        return (CurveExpression)replacer.ReplaceByValue(expressionPattern, ignoreNotMatchedExpressions);
    }

    IGenericExpression<Curve> IGenericExpression<Curve>.ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions
    ) 
    => ReplaceByValue(expressionPattern, newExpressionToReplace, ignoreNotMatchedExpressions);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>New expression object (of type <see cref="CurveExpression"/>) with replaced sub-expression.</returns>
    public CurveExpression ReplaceByPosition<T1>(ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpressionToReplace)
        => ReplaceByPosition(expressionPosition.GetPositionPath(), newExpressionToReplace);

    IGenericExpression<Curve> IGenericExpression<Curve>.ReplaceByPosition<T1>(ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpressionToReplace) => ReplaceByPosition(expressionPosition, newExpressionToReplace);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="positionPath">Position of the expression to be replaced. The position is expressed as a path from
    /// the root of the expression by using a list of strings "Operand" for unary operators, "LeftOperand"/"RightOperand"
    /// for binary operators, "Operand(index)" for n-ary operators.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>New expression object (of type <see cref="CurveExpression"/>) with the replaced sub-expression.
    /// </returns>
    public CurveExpression ReplaceByPosition<T1>(
        IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpressionToReplace)
    {
        var replacer = new OneTimeExpressionReplacer<Curve, T1>(this, newExpressionToReplace);
        return (CurveExpression)replacer.ReplaceByPosition(positionPath);
    }

    IGenericExpression<Curve> IGenericExpression<Curve>.ReplaceByPosition<T1>(IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpressionToReplace) => ReplaceByPosition(positionPath, newExpressionToReplace);

    #endregion Replace
    
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
    public CurveExpression ApplyEquivalence(Equivalence equivalence, CheckType checkType = CheckType.CheckLeftOnly)
    {
        var replacer = new OneTimeExpressionReplacer<Curve, Curve>(this, equivalence, checkType);
        // In the case of equivalences the argument of ReplaceByValue is not significant
        return (CurveExpression)replacer.ReplaceByValue(equivalence.LeftSideExpression);
    }

    IGenericExpression<Curve> IGenericExpression<Curve>.ApplyEquivalence(Equivalence equivalence, CheckType checkType)
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
    public CurveExpression ApplyEquivalenceByPosition(IEnumerable<string> positionPath, Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        var replacer = new OneTimeExpressionReplacer<Curve, Curve>(this, equivalence, checkType);
        return (CurveExpression)replacer.ReplaceByPosition(positionPath);
    }

    IGenericExpression<Curve> IGenericExpression<Curve>.ApplyEquivalenceByPosition(IEnumerable<string> positionPath,
        Equivalence equivalence,
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
    public CurveExpression ApplyEquivalenceByPosition(ExpressionPosition expressionPosition, Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
        => ApplyEquivalenceByPosition(expressionPosition.GetPositionPath(), equivalence, checkType);

    IGenericExpression<Curve> IGenericExpression<Curve>.ApplyEquivalenceByPosition(
        ExpressionPosition expressionPosition, Equivalence equivalence,
        CheckType checkType)
        => ApplyEquivalenceByPosition(expressionPosition, equivalence, checkType);

    /// <summary>
    /// Checks if two expressions are equivalent by computing their values
    /// </summary>
    public bool Equivalent(IGenericExpression<Curve> other)
        => Curve.Equivalent(Compute(),
            other.Compute());
    
    /// <summary>
    /// Adds an equivalence to the static field <see cref="Equivalences"/>.
    /// </summary>
    /// <param name="type">The main type of operation (e.g., <see cref="ConvolutionExpression"/> in the equivalence
    /// $f \otimes f = f) involved in the equivalence.</param>
    /// <param name="equivalence">The equivalence to be added.</param>
    public static void AddEquivalence(Type type, Equivalence equivalence)
    {
        if (equivalence.LeftSideExpression.GetType() != type) return;
        Equivalences.TryAdd(type, []);
        Equivalences[type].Add(equivalence);
    }
    
    #endregion Equivalence
    
    public void Accept(IExpressionVisitor visitor)
        => Accept((ICurveExpressionVisitor)visitor);

    public abstract void Accept(ICurveExpressionVisitor visitor);
    
    /// <summary>
    /// Private function used during the creation of the expressions to keep the n-ary expressions at the same level
    /// of the expression tree. 
    /// </summary>
    /// <param name="type">Type of the expression that needs to be created</param>
    /// <param name="e1">Left operand</param>
    /// <param name="e2">Right operand</param>
    /// <returns>If <see cref="type"/> is equal to the type of <see cref="e1"/> the function returns 1, which means that
    /// the new expression must be created by appending <see cref="e2"/> to <see cref="e1"/>.
    /// The opposite holds if <see cref="type"/> is equal to the type of <see cref="e2"/>, in this case the function
    /// returns 2.
    /// The function returns 0 when the <see cref="type"/> is different by the type of <see cref="e1"/> and
    /// <see cref="e2"/>.</returns>
    private static int CheckNAryExpressionTypes(Type type, CurveExpression e1, CurveExpression e2)
    {
        if (e1.GetType() == type)
            return 1;
        return e2.GetType() == type ? 2 : 0;
    }

    public string ToLatexString(int depth = 20, bool showRationalsAsName = false)
    {
        var latexFormatterVisitor = new LatexFormatterVisitor(depth, showRationalsAsName);
        Accept(latexFormatterVisitor);

        var latexExpr = latexFormatterVisitor.Result.ToString();
        // this is too simplistic, the two parentheses may not be matched with each other
        // var startsWithLeftParenthesis = latexExpr.StartsWith("\\left(");
        // var endsWithRightParenthesis = latexExpr.EndsWith("\\right)");
        // if (startsWithLeftParenthesis && endsWithRightParenthesis)
        //     latexExpr = latexExpr[6..^7];

        return latexExpr;
    }

    public string ToUnicodeString(int depth = 20, bool showRationalsAsName = false)
    {
        var unicodeFormatterVisitor = new UnicodeFormatterVisitor(depth, showRationalsAsName);
        Accept(unicodeFormatterVisitor);

        var unicodeExpr = unicodeFormatterVisitor.Result.ToString();
        // this is too simplistic, the two parentheses may not be matched with each other
        // if (unicodeExpr is ['(', _, ..] && unicodeExpr[^1] == ')') // ^1 accesses the last character
        //     unicodeExpr = unicodeExpr[1..^1];

        return unicodeExpr;
    }

    /// <summary>
    /// Returns a string that represents the current expression using the Unicode character set.
    /// </summary>
    public sealed override string ToString()
        => ToUnicodeString();
    
    public double Estimate()
    {
        throw new NotImplementedException();
    }

    public ExpressionPosition RootPosition() => new();

    // todo: is this redundant?
    
    /// <summary>
    /// Changes the name of the expression.
    /// </summary>
    /// <param name="expressionName">The new name of the expression</param>
    /// <returns>The expression (new object of type <see cref="CurveExpression"/>) with the new name</returns>
    public CurveExpression WithName(string expressionName)
    {
        var changeNameVisitor = new ChangeNameCurveVisitor(expressionName);
        Accept(changeNameVisitor);

        return changeNameVisitor.Result;
    }

    IGenericExpression<Curve> IGenericExpression<Curve>.WithName(string expressionName) => WithName(expressionName);
    
    /// <summary>
    /// This operator returns true if the value of a curve expression is below or equal than the value of another one.
    /// </summary>
    public static bool operator <=(CurveExpression expressionL, CurveExpression expressionR)
        => expressionL.Compute() <= expressionR.Compute();

    /// <summary>
    /// This operator returns true if the value of a curve expression is greater or equal than the value of another one.
    /// </summary>
    public static bool operator >=(CurveExpression expressionL, CurveExpression expressionR)
        => expressionL.Compute() >= expressionR.Compute();

    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <see cref="rationalExpression"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="curveExpression"></param>
    /// <param name="rationalExpression"></param>
    /// <returns></returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator +(CurveExpression curveExpression, RationalExpression rationalExpression)
        => curveExpression.Shift(rationalExpression);
    
    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <see cref="rational"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="curveExpression"></param>
    /// <param name="rational"></param>
    /// <returns></returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator +(CurveExpression curveExpression, Rational rational)
        => curveExpression.Shift(rational);
    
    /// <summary>
    /// Returns true if for $t \ge$ <see cref="Curve.PseudoPeriodStart"/> the curve expression is constant.
    /// Implemented by computing the value of the expression.
    /// </summary>
    public bool IsUltimatelyConstant() => Compute().IsUltimatelyConstant;

    #endregion Methods
}