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

    #region Constructors

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other"></param>
    /// <remarks>
    /// This constructor was made explicit to *not* copy the private cache fields when using the with operator.
    /// </remarks>
    public CurveExpression(CurveExpression other)
    {
        Name = other.Name;
    }
    
    #endregion Constructors
    
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
    /// Creates a new expression that delays the current expression by the rational <see cref="expression"/>,
    /// i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Curve.DelayBy"/>
    /// <seealso cref="ForwardBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression DelayBy(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DelayByExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the current expression by the rational <see cref="delay"/>,
    /// i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Curve.DelayBy"/>
    /// <seealso cref="ForwardBy(Unipi.Nancy.Numerics.Rational,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Numerics.Rational,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression DelayBy(Rational delay, string expressionName = "", ExpressionSettings? settings = null)
        => DelayBy(new RationalNumberExpression(delay), expressionName, settings);

    #endregion DelayBy

    #region ForwardBy

    /// <summary>
    /// Creates a new expression that forwards the current expression by the rational <see cref="expression"/>,
    /// i.e., computing $f(t + T)$, with $T \ge 0$. 
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Curve.ForwardBy"/>
    /// <seealso cref="DelayBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression ForwardBy(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DelayByExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the current expression by the rational <see cref="time"/>,
    /// i.e., computing $f(t + T)$, with $T \ge 0$. 
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Curve.ForwardBy"/>
    /// <seealso cref="DelayBy(Unipi.Nancy.Numerics.Rational,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Numerics.Rational,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression ForwardBy(Rational time, string expressionName = "", ExpressionSettings? settings = null)
        => ForwardBy(new RationalNumberExpression(time), expressionName, settings);

    #endregion ForwardBy
    
    #region HorizontalShift

    /// <summary>
    /// Creates a new expression that shifts the current expression by the rational <see cref="expression"/>,
    /// i.e., computing $f(t - T)$. 
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Curve.HorizontalShift"/>
    /// <seealso cref="DelayBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="ForwardBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression HorizontalShift(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new HorizontalShiftExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current expression by the rational <see cref="value"/>,
    /// i.e., computing $f(t - T)$. 
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Curve.HorizontalShift"/>
    /// <seealso cref="DelayBy(Unipi.Nancy.Numerics.Rational,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="ForwardBy(Unipi.Nancy.Numerics.Rational,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression HorizontalShift(Rational value, string expressionName = "", ExpressionSettings? settings = null)
        => HorizontalShift(new RationalNumberExpression(value), expressionName, settings);

    #endregion HorizontalShift
    
    #region VerticalShift

    /// <summary>
    /// Creates a new expression that shifts the current
    /// expression by the rational <see cref="expression"/>, i.e., computing $f(t) + K$. 
    /// </summary>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public CurveExpression VerticalShift(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new VerticalShiftExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current curve
    /// expression by the rational <see cref="value"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public CurveExpression VerticalShift(Rational value, string expressionName = "", ExpressionSettings? settings = null)
        => VerticalShift(new RationalNumberExpression(value), expressionName, settings);

    #endregion VerticalShift
    
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
    
    #region Sampling

    /// <summary>
    /// Creates a new expression that computes the value of the curve expression at <see cref="time"/>.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="expressionName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public RationalExpression ValueAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new ValueAtExpression(this, new RationalNumberExpression(time), expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the value of the curve expression at the time given by <see cref="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression"></param>
    /// <param name="expressionName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public RationalExpression ValueAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new ValueAtExpression(this, timeExpression, expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the left-limit value of the curve expression at <see cref="time"/>.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="expressionName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public RationalExpression LeftLimitAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new LeftLimitAtExpression(this, new RationalNumberExpression(time), expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the left-limit value of the curve expression at the time given by <see cref="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression"></param>
    /// <param name="expressionName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public RationalExpression LeftLimitAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new LeftLimitAtExpression(this, timeExpression, expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the right-limit value of the curve expression at <see cref="time"/>.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="expressionName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public RationalExpression RightLimitAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new RightLimitAtExpression(this, new RationalNumberExpression(time), expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the right-limit value of the curve expression at the time given by <see cref="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression"></param>
    /// <param name="expressionName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public RationalExpression RightLimitAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new RightLimitAtExpression(this, timeExpression, expressionName, settings);
        
    #endregion Sampling

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

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newValueToReplace">The new value to replace the sub-expression.</param>
    /// /// <param name="name">The name of the new value.</param>
    /// <returns>New expression object (of type <see cref="CurveExpression"/>) with replaced sub-expression.</returns>
    public CurveExpression ReplaceByPosition(ExpressionPosition expressionPosition,
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
    /// <returns>New expression object (of type <see cref="CurveExpression"/>) with replaced sub-expression.</returns>
    public CurveExpression ReplaceByPosition(ExpressionPosition expressionPosition,
        Rational newValueToReplace,
        [CallerArgumentExpression("newValueToReplace")] string name = ""
    )
        => ReplaceByPosition(expressionPosition.GetPositionPath(), newValueToReplace.ToExpression(name));

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
    
    #region Accept
    
    public void Accept(IExpressionVisitor<Curve> visitor)
        => Accept((ICurveExpressionVisitor) visitor);

    public abstract void Accept(ICurveExpressionVisitor visitor);
    
    public TResult Accept<TResult>(IExpressionVisitor<Curve, TResult> visitor)
        => Accept((ICurveExpressionVisitor<TResult>) visitor);

    public abstract TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor);
    
    #endregion Accept
    
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
        var (sb, _) = Accept(latexFormatterVisitor);
        var latexExpr = sb.ToString();

        return latexExpr;
    }

    public string ToUnicodeString(int depth = 20, bool showRationalsAsName = false)
    {
        var unicodeFormatterVisitor = new UnicodeFormatterVisitor(depth, showRationalsAsName);
        var (sb, _) = Accept(unicodeFormatterVisitor);
        var unicodeExpr = sb.ToString();
        
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

    /// <summary>
    /// Changes the name of the expression.
    /// </summary>
    /// <param name="expressionName">The new name of the expression</param>
    /// <returns>
    /// The expression (new object of type <see cref="CurveExpression"/>) with the new name.
    /// </returns>
    /// <remarks>
    /// Renaming can be done using the with operator, but that will clear out the cache fields, causing re-computation of the expression.
    /// This method will instead copy over the cache fields.
    /// </remarks>
    public CurveExpression WithName(string expressionName)
    {
        var changeNameVisitor = new RenameCurveVisitor(expressionName);
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
        => curveExpression.VerticalShift(rationalExpression);
    
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
        => curveExpression.VerticalShift(rational);
    
    /// <summary>
    /// Returns true if for $t \ge$ <see cref="Curve.PseudoPeriodStart"/> the curve expression is constant.
    /// Implemented by computing the value of the expression.
    /// </summary>
    public bool IsUltimatelyConstant() => Compute().IsUltimatelyConstant;

    #endregion Methods
}