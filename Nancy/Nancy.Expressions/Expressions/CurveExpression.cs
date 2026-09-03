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

    /// <inheritdoc />
    public string Name { get; init; }

    /// <inheritdoc />
    public int Generation { get; init; }

    /// <summary>
    /// Static dictionary field collecting the well-known equivalences, indexed by the "main" type of equivalence
    /// </summary>
    public static readonly ConcurrentDictionary<Type, List<Equivalence>> Equivalences = new();

    /// <inheritdoc />
    public ExpressionSettings? Settings { get; init; }
    
    /// <summary>
    /// Private cache field for <see cref="Value"/>
    /// </summary>
    internal Curve? _value;

    /// <inheritdoc />
    public Curve Value => _value ??= Compute();

    /// <inheritdoc cref="IExpression.IsComputed"/>
    public bool IsComputed
        => _value != null;

    /// <summary>
    /// True if this node's own cached <see cref="Value"/> is small enough to be worth keeping.
    /// </summary>
    /// <remarks>
    /// The cached value's element count is checked against <see cref="CacheSettings.CheapCacheElementThreshold"/>, taken from <see cref="ExpressionSettings.CacheSettings"/> on <see cref="Settings"/>, or from the default threshold when unset.
    /// A node with no cached value yet has nothing to clear, and reports <see langword="true"/>.
    /// </remarks>
    protected internal virtual bool ValueCacheIsCheap
    {
        get
        {
            if (_value is null)
                return true;
            var threshold = Settings?.CacheSettings?.CheapCacheElementThreshold
                ?? new CacheSettings().CheapCacheElementThreshold;
            return _value.BaseSequence.Elements.Count <= threshold;
        }
    }

    /// <summary>
    /// Clears this node's own cached <see cref="Value"/>, and, per <paramref name="scope"/>, its descendants', mutating them in place.
    /// </summary>
    /// <remarks>
    /// Mutating in place is what makes this cheap: a shared node's memory is reclaimed while every ancestor holding a reference to it stays as it is.
    /// A node whose own <see cref="ValueCacheIsCheap"/> is <see langword="true"/> keeps its cache, and recursion still descends into its children, since a cheap node can have expensive descendants.
    /// A horizontal deviation is the case to have in mind: its own <see cref="Rational"/> result is two integers, while the curves it was computed from can be arbitrarily large.
    /// </remarks>
    public void ClearValueCache(CacheClearScope scope = CacheClearScope.Subtree)
    {
        if (!ValueCacheIsCheap)
            _value = null;

        if (scope == CacheClearScope.SelfOnly)
            return;

        foreach (var child in EnumerateChildren())
        {
            if (scope == CacheClearScope.SubtreeUntilNamed && !string.IsNullOrEmpty(child.Name))
                continue;
            ClearValueCacheDispatch.Clear(child, scope);
        }
    }

    private IEnumerable<IExpression> EnumerateChildren()
    {
        switch (this)
        {
            case IGenericUnaryExpression<Curve, Curve> u:
                yield return u.Operand;
                break;
            case IGenericUnaryExpression<Rational, Curve> u:
                yield return u.Operand;
                break;
            case IGenericBinaryExpression<Curve, Curve, Curve> b:
                yield return b.LeftOperand;
                yield return b.RightOperand;
                break;
            case IGenericBinaryExpression<Curve, Rational, Curve> b:
                yield return b.LeftOperand;
                yield return b.RightOperand;
                break;
            case IGenericBinaryExpression<Rational, Curve, Curve> b:
                yield return b.LeftOperand;
                yield return b.RightOperand;
                break;
            case IGenericBinaryExpression<Rational, Rational, Curve> b:
                yield return b.LeftOperand;
                yield return b.RightOperand;
                break;
            case CurveNAryExpression n:
                foreach (var operand in n.Operands)
                    yield return operand;
                break;
        }
    }

    #region IfKnown accessors

    /// <summary>
    /// <see cref="IsSubAdditive"/> if already cached, without forcing the check.
    /// </summary>
    public bool? SubAdditiveIfKnown => _isSubAdditive;

    /// <summary>
    /// <see cref="IsSuperAdditive"/> if already cached, without forcing the check.
    /// </summary>
    public bool? SuperAdditiveIfKnown => _isSuperAdditive;

    /// <summary>
    /// <see cref="IsLeftContinuous"/> if already cached, without forcing the check.
    /// </summary>
    public bool? LeftContinuousIfKnown => _isLeftContinuous;

    /// <summary>
    /// <see cref="IsRightContinuous"/> if already cached, without forcing the check.
    /// </summary>
    public bool? RightContinuousIfKnown => _isRightContinuous;

    /// <summary>
    /// <see cref="IsNonNegative"/> if already cached, without forcing the check.
    /// </summary>
    public bool? NonNegativeIfKnown => _isNonNegative;

    /// <summary>
    /// <see cref="IsNonDecreasing"/> if already cached, without forcing the check.
    /// </summary>
    public bool? NonDecreasingIfKnown => _isNonDecreasing;

    /// <summary>
    /// <see cref="IsIncreasing"/> if already cached, without forcing the check.
    /// </summary>
    public bool? IncreasingIfKnown => _isIncreasing;

    /// <summary>
    /// <see cref="IsConcave"/> if already cached, without forcing the check.
    /// </summary>
    public bool? ConcaveIfKnown => _isConcave;

    /// <summary>
    /// <see cref="IsConvex"/> if already cached, without forcing the check.
    /// </summary>
    public bool? ConvexIfKnown => _isConvex;

    /// <summary>
    /// <see cref="IsPassingThroughOrigin"/> if already cached, without forcing the check.
    /// </summary>
    public bool? PassingThroughOriginIfKnown => _isPassingThroughOrigin;

    /// <summary>
    /// <see cref="IsUltimatelyFinite"/> if already cached, without forcing the check.
    /// </summary>
    public bool? UltimatelyFiniteIfKnown => _isUltimatelyFinite;

    /// <summary>
    /// <see cref="IsPlain"/> if already cached, without forcing the check.
    /// </summary>
    public bool? PlainIfKnown => _isPlain;

    /// <summary>
    /// <see cref="IsUltimatelyPlain"/> if already cached, without forcing the check.
    /// </summary>
    public bool? UltimatelyPlainIfKnown => _isUltimatelyPlain;

    /// <summary>
    /// <see cref="IsUltimatelyAffine"/> if already cached, without forcing the check.
    /// </summary>
    public bool? UltimatelyAffineIfKnown => _isUltimatelyAffine;

    /// <summary>
    /// <see cref="IsUltimatelyConstant"/> if already cached, without forcing the check.
    /// </summary>
    public bool? UltimatelyConstantIfKnown => _isUltimatelyConstant;

    /// <summary>
    /// <see cref="IsWellDefined"/> if already cached, without forcing the check.
    /// </summary>
    public bool? WellDefinedIfKnown => _isWellDefined;

    #endregion IfKnown accessors

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
    /// Private cache field for <see cref="IsSuperAdditive"/>.
    /// </summary>
    internal bool? _isSuperAdditive;

    /// <summary>
    /// True if the curve described by the expression is super-additive. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsSuperAdditive
    {
        get
        {
            return _isSuperAdditive ??= CheckIsSuperAdditive();

            bool CheckIsSuperAdditive()
            {
                var isSuperAdditiveVisitor = new IsSuperAdditiveVisitor();
                Accept(isSuperAdditiveVisitor);

                return isSuperAdditiveVisitor.IsSuperAdditive;
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
    /// Private cache field for <see cref="IsIncreasing"/>.
    /// </summary>
    internal bool? _isIncreasing;

    /// <summary>
    /// True if the curve described by the expression is (strictly) increasing. Property evaluated avoiding as much
    /// as possible to make any computation.
    /// </summary>
    public bool IsIncreasing
    {
        get
        {
            return _isIncreasing ??= CheckIsIncreasing();

            bool CheckIsIncreasing()
            {
                var isIncreasingVisitor = new IsIncreasingVisitor();
                Accept(isIncreasingVisitor);

                return isIncreasingVisitor.IsIncreasing;
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
    /// Private cache field for <see cref="IsPassingThroughOrigin"/>.
    /// </summary>
    internal bool? _isPassingThroughOrigin;

    /// <summary>
    /// True if the curve $f$ described by the expression passes through the origin, i.e. $f(0) = 0$. Property
    /// evaluated avoiding as much as possible to make any computation.
    /// </summary>
    public bool IsPassingThroughOrigin
    {
        get
        {
            return _isPassingThroughOrigin ??= CheckIsPassingThroughOrigin();

            bool CheckIsPassingThroughOrigin()
            {
                var isPassingThroughOriginVisitor = new IsPassingThroughOriginVisitor();
                Accept(isPassingThroughOriginVisitor);

                return isPassingThroughOriginVisitor.IsPassingThroughOrigin;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsUltimatelyFinite"/>.
    /// </summary>
    internal bool? _isUltimatelyFinite;

    /// <summary>
    /// True if the curve described by the expression is ultimately finite. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsUltimatelyFinite
    {
        get
        {
            return _isUltimatelyFinite ??= CheckIsUltimatelyFinite();

            bool CheckIsUltimatelyFinite()
            {
                var visitor = new IsUltimatelyFiniteVisitor();
                Accept(visitor);

                return visitor.IsUltimatelyFinite;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsPlain"/>.
    /// </summary>
    internal bool? _isPlain;

    /// <summary>
    /// True if the curve described by the expression is plain, as defined in [BT08], Definition 1. Property
    /// evaluated avoiding as much as possible to make any computation.
    /// </summary>
    public bool IsPlain
    {
        get
        {
            return _isPlain ??= CheckIsPlain();

            bool CheckIsPlain()
            {
                var visitor = new IsPlainVisitor();
                Accept(visitor);

                return visitor.IsPlain;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsUltimatelyPlain"/>.
    /// </summary>
    internal bool? _isUltimatelyPlain;

    /// <summary>
    /// True if the curve described by the expression is ultimately plain, as defined in [BT08], Definition 1.
    /// Property evaluated avoiding as much as possible to make any computation.
    /// </summary>
    public bool IsUltimatelyPlain
    {
        get
        {
            return _isUltimatelyPlain ??= CheckIsUltimatelyPlain();

            bool CheckIsUltimatelyPlain()
            {
                var visitor = new IsUltimatelyPlainVisitor();
                Accept(visitor);

                return visitor.IsUltimatelyPlain;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsUltimatelyAffine"/>.
    /// </summary>
    internal bool? _isUltimatelyAffine;

    /// <summary>
    /// True if the curve described by the expression is ultimately affine. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsUltimatelyAffine
    {
        get
        {
            return _isUltimatelyAffine ??= CheckIsUltimatelyAffine();

            bool CheckIsUltimatelyAffine()
            {
                var visitor = new IsUltimatelyAffineVisitor();
                Accept(visitor);

                return visitor.IsUltimatelyAffine;
            }
        }
    }

    /// <summary>
    /// Private cache field for <see cref="IsUltimatelyConstant"/>.
    /// </summary>
    internal bool? _isUltimatelyConstant;

    /// <summary>
    /// True if the curve described by the expression is ultimately constant. Property evaluated avoiding as much as
    /// possible to make any computation.
    /// </summary>
    public bool IsUltimatelyConstant
    {
        get
        {
            return _isUltimatelyConstant ??= CheckIsUltimatelyConstant();

            bool CheckIsUltimatelyConstant()
            {
                var visitor = new IsUltimatelyConstantVisitor();
                Accept(visitor);

                return visitor.IsUltimatelyConstant;
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
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Optional settings for the operation.</param>
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

    /// <summary>
    /// True if the curve described by the expression has no discontinuity, i.e., it is both left- and right-continuous.
    /// </summary>
    public bool IsContinuous
        => IsLeftContinuous && IsRightContinuous;

    /// <summary>
    /// True if the curve described by the expression is subadditive with $f(0) = 0$.
    /// </summary>
    public bool IsRegularSubAdditive
        => IsSubAdditive && IsPassingThroughOrigin;

    /// <summary>
    /// True if the curve described by the expression is super-additive with $f(0) = 0$.
    /// </summary>
    public bool IsRegularSuperAdditive
        => IsSuperAdditive && IsPassingThroughOrigin;

    /// <summary>
    /// True if the curve described by the expression is concave with $f(0) = 0$.
    /// </summary>
    public bool IsRegularConcave
        => IsConcave && IsPassingThroughOrigin;

    /// <summary>
    /// True if the curve described by the expression is convex with $f(0) = 0$.
    /// </summary>
    public bool IsRegularConvex
        => IsConvex && IsPassingThroughOrigin;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <remarks>
    /// Made explicit so that the `with` operator carries what the caller set, the name, the generation and the settings, and leaves behind the cache fields, which the new expression has to earn again.
    /// </remarks>
    public CurveExpression(CurveExpression other)
    {
        Name = other.Name;
        Generation = other.Generation;
        Settings = other.Settings;
    }
    
    #endregion Constructors
    
    #region Equality

    /// <summary>
    /// True if <paramref name="other"/> is the same operator over the same operands.
    /// </summary>
    /// <remarks>
    /// <see cref="Name"/>, <see cref="Generation"/> and <see cref="Settings"/> do not participate:
    /// an expression by a different name is the same expression.
    /// Each arity overrides this further with its own operand comparison, on top of this base check.
    /// </remarks>
    public virtual bool Equals(CurveExpression? other)
        => other is not null;

    /// <inheritdoc />
    /// <remarks>
    /// Seeded by the concrete <see cref="Type"/>, so every expression's hash differs from the hash of the bare value it wraps, a <see cref="ConcreteCurveExpression"/> from its own <see cref="Curve"/> included.
    /// </remarks>
    public override int GetHashCode()
        => HashCode.Combine(-363510328, GetType());

    #endregion Equality

    #region Methods

    /// <summary>
    /// Adds the opposite operator to the expression.
    /// </summary>
    public CurveExpression Negate(string expressionName = "", ExpressionSettings? settings = null)
        => new NegateExpression(this, expressionName, settings);

    /// <summary>
    /// Implementation of the unary - operator as the negation of a <see cref="CurveExpression"/>.
    /// </summary>
    public static CurveExpression operator -(CurveExpression expression)
        => expression.Negate();

    /// <summary>
    /// Adds to the expression the operation to compute its non-negative version.
    /// </summary>
    public CurveExpression ToNonNegative(string expressionName = "", ExpressionSettings? settings = null)
        => new ToNonNegativeExpression(this, expressionName, settings);

    /// <summary>
    /// Adds the floor operator to the expression, $\lfloor f(t) \rfloor$.
    /// </summary>
    public CurveExpression Floor(string expressionName = "", ExpressionSettings? settings = null)
        => new FloorExpression(this, expressionName, settings);

    /// <summary>
    /// Adds the ceiling operator to the expression, $\lceil f(t) \rceil$.
    /// </summary>
    public CurveExpression Ceil(string expressionName = "", ExpressionSettings? settings = null)
        => new CeilExpression(this, expressionName, settings);

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
    /// Adds to the expression the operation to compute its upper non-increasing version.
    /// </summary>
    public CurveExpression ToUpperNonIncreasing(string expressionName = "", ExpressionSettings? settings = null)
        => new ToUpperNonIncreasingExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute its lower non-increasing version.
    /// </summary>
    public CurveExpression ToLowerNonIncreasing(string expressionName = "", ExpressionSettings? settings = null)
        => new ToLowerNonIncreasingExpression(this, expressionName, settings);

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
    /// Adds to the expression an operation which enforces it to assume the given value at time 0.
    /// </summary>
    public CurveExpression WithOriginAt(Rational value, string expressionName = "",
        ExpressionSettings? settings = null)
        => new WithOriginAtExpression(this, value, expressionName, settings);

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
    public CurveExpression Addition(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(AdditionExpression), this, operand) switch
        {
            1 => ((AdditionExpression)this).Append(operand, expressionName, settings),
            2 => ((AdditionExpression)operand).Append(this, expressionName, settings),
            _ => new AdditionExpression([this, operand], expressionName, settings)
        };

    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Addition(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is AdditionExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new AdditionExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }

    /// <summary>
    /// Creates a new expression composed of the addition between the two expressions.
    /// </summary>
    public static CurveExpression Addition(CurveExpression left, CurveExpression right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Addition(right, expressionName:expressionName, settings: settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the expression <paramref name="left"/> and the curve
    /// <paramref name="right"/> (internally converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public static CurveExpression Addition(CurveExpression left, Curve right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Addition(right, expressionName:expressionName, settings: settings);

    /// <summary>
    /// Implementation of the + operator as the addition between <see cref="CurveExpression"/> objects.
    /// </summary>
    public static CurveExpression operator +(CurveExpression left, CurveExpression right)
        => Addition(left, right);

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
    public CurveExpression Subtraction(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SubtractionExpression(this, operand, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Subtraction(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Subtraction(new ConcreteCurveExpression(curve, name), expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the one passed as
    /// argument.
    /// </summary>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public CurveExpression Subtraction(CurveExpression operand, bool nonNegative, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SubtractionExpression(this, operand, nonNegative, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public CurveExpression Subtraction(Curve curve, bool nonNegative, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Subtraction(new ConcreteCurveExpression(curve, name), nonNegative, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the two expressions.
    /// </summary>
    public static CurveExpression Subtraction(CurveExpression left, CurveExpression right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Subtraction(right, expressionName:expressionName, settings: settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the expression <paramref name="left"/> and the curve
    /// <paramref name="right"/> (internally converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public static CurveExpression Subtraction(CurveExpression left, Curve right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Subtraction(right, expressionName:expressionName, settings: settings);

    /// <summary>
    /// Implementation of the - operator as the subtraction between <see cref="CurveExpression"/> objects.
    /// </summary>
    public static CurveExpression operator -(CurveExpression left, CurveExpression right)
        => Subtraction(left, right);

    /// <summary>
    /// Implementation of the - operator as the subtraction between <see cref="CurveExpression"/> objects.
    /// </summary>
    public static CurveExpression operator -(CurveExpression left, Curve right)
        => Subtraction(left, right);

    #endregion Subtraction
    
    #region Minimum 
    
    /// <summary>
    /// Creates a new expression composed of the minimum between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Minimum(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(MinimumExpression), this, operand) switch
        {
            1 => ((MinimumExpression)this).Append(operand, expressionName, settings),
            2 => ((MinimumExpression)operand).Append(this, expressionName, settings),
            _ => new MinimumExpression([this, operand], expressionName, settings)
        };

    /// <summary>
    /// Creates a new expression composed of the minimum between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Minimum(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is MinimumExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new MinimumExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }

    #endregion Minimum
    
    #region Maximum
    
    /// <summary>
    /// Creates a new expression composed of the maximum between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Maximum(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(MaximumExpression), this, operand) switch
        {
            1 => ((MaximumExpression)this).Append(operand, expressionName, settings),
            2 => ((MaximumExpression)operand).Append(this, expressionName, settings),
            _ => new MaximumExpression([this, operand], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the maximum between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Maximum(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is MaximumExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteCurveExpression(curve, name, settings));
        return new MaximumExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }
    
    #endregion Maximum
    
    #region Convolution

    /// <summary>
    /// Creates a new expression composed of the convolution between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Convolution(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(ConvolutionExpression), this, operand) switch
        {
            1 => ((ConvolutionExpression)this).Append(operand, expressionName, settings),
            2 => ((ConvolutionExpression)operand).Append(this, expressionName, settings),
            _ => new ConvolutionExpression([this, operand], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the convolution between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression Convolution(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is ConvolutionExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteCurveExpression(curve, name), expressionName, settings);
        return new ConvolutionExpression([this, new ConcreteCurveExpression(curve, name)], expressionName, settings);
    }
    
    #endregion Convolution
    
    #region Deconvolution

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the current expression and the one passed as
    /// argument.
    /// </summary>
    public CurveExpression Deconvolution(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DeconvolutionExpression(this, operand, expressionName, settings);

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
    public CurveExpression MaxPlusConvolution(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(MaxPlusConvolutionExpression), this, operand) switch
        {
            1 => ((MaxPlusConvolutionExpression)this).Append(operand, expressionName, settings),
            2 => ((MaxPlusConvolutionExpression)operand).Append(this, expressionName, settings),
            _ => new MaxPlusConvolutionExpression([this, operand], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the current expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as argument.
    /// </summary>
    public CurveExpression MaxPlusConvolution(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is MaxPlusConvolutionExpression e && string.IsNullOrEmpty(Name))
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
    public CurveExpression MaxPlusDeconvolution(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new MaxPlusDeconvolutionExpression(this, operand, expressionName, settings);

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
    public CurveExpression Composition(CurveExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new CompositionExpression(this, operand, expressionName, settings);

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
    /// Creates a new expression that delays the current expression by the rational <paramref name="expression"/>,
    /// i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Curve.DelayBy"/>
    /// <seealso cref="ForwardBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression DelayBy(RationalExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new DelayByExpression(this, operand, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the current expression by the rational <paramref name="delay"/>,
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
    /// Creates a new expression that forwards the current expression by the rational <paramref name="expression"/>,
    /// i.e., computing $f(t + T)$, with $T \ge 0$. 
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Curve.ForwardBy"/>
    /// <seealso cref="DelayBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression ForwardBy(RationalExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new ForwardByExpression(this, operand, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the current expression by the rational <paramref name="time"/>,
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
    /// Creates a new expression that shifts the current expression by the rational <paramref name="expression"/>,
    /// i.e., computing $f(t - T)$. 
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Curve.HorizontalShift"/>
    /// <seealso cref="DelayBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    /// <seealso cref="ForwardBy(Unipi.Nancy.Expressions.RationalExpression,string,Unipi.Nancy.Expressions.ExpressionSettings?)"/>
    public CurveExpression HorizontalShift(RationalExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new HorizontalShiftExpression(this, operand, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current expression by the rational <paramref name="value"/>,
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
    /// expression by the rational <paramref name="expression"/>, i.e., computing $f(t) + K$. 
    /// </summary>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public CurveExpression VerticalShift(RationalExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new VerticalShiftExpression(this, operand, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current curve
    /// expression by the rational <paramref name="value"/>, i.e., computing $f(t) + K$.
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
    /// by the rational number described by the argument <paramref name="expression"/> of type
    /// <see cref="RationalExpression"/>. </summary>
    public CurveExpression Scale(RationalExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => new ScaleExpression(this, operand, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the operation to scale the curve corresponding to the current expression
    /// by the rational number <paramref name="scaleFactor"/>.</summary>
    public CurveExpression Scale(Rational scaleFactor, string expressionName = "", ExpressionSettings? settings = null)
        => Scale(new RationalNumberExpression(scaleFactor), expressionName, settings);

    /// <summary>
    /// Implementation of the * operator to scale a curve expression by a rational number.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static CurveExpression operator *(CurveExpression curve, Rational scaleFactor)
        => curve.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the * operator to scale a curve expression by a rational number.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static CurveExpression operator *(Rational scaleFactor, CurveExpression curve)
        => curve.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the * operator to scale a curve expression by a rational expression.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static CurveExpression operator *(CurveExpression curve, RationalExpression scaleFactor)
        => curve.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the * operator to scale a curve expression by a rational expression.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static CurveExpression operator *(RationalExpression scaleFactor, CurveExpression curve)
        => curve.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the / operator to scale down a curve expression by a rational number.
    /// Computes $g(t) = f(t) / k$.
    /// </summary>
    public static CurveExpression operator /(CurveExpression curve, Rational scaleFactor)
        => curve.Scale(1 / scaleFactor);

    /// <summary>
    /// Implementation of the / operator to scale down a curve expression by a rational expression.
    /// Computes $g(t) = f(t) / k$.
    /// </summary>
    public static CurveExpression operator /(CurveExpression curve, RationalExpression scaleFactor)
        => curve.Scale(scaleFactor.Invert());

    #endregion Scale
    
    #region Sampling

    /// <summary>
    /// Creates a new expression that computes the value of the curve expression at <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression ValueAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new ValueAtExpression(this, new RationalNumberExpression(time), expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the value of the curve expression at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression">The expression that provides the time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression ValueAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new ValueAtExpression(this, timeExpression, expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the left-limit value of the curve expression at <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression LeftLimitAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new LeftLimitAtExpression(this, new RationalNumberExpression(time), expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the left-limit value of the curve expression at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression">The expression that provides the time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression LeftLimitAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new LeftLimitAtExpression(this, timeExpression, expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the right-limit value of the curve expression at <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression RightLimitAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new RightLimitAtExpression(this, new RationalNumberExpression(time), expressionName, settings);
    
    /// <summary>
    /// Creates a new expression that computes the right-limit value of the curve expression at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression">The expression that provides the time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression RightLimitAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new RightLimitAtExpression(this, timeExpression, expressionName, settings);

    #endregion Sampling

    #region Extrema

    /// <summary>
    /// Creates a new expression that computes the supremum value attained by the curve expression, $\sup_{t \ge 0} f(t)$.
    /// </summary>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    /// <seealso cref="MinPlusAlgebra.Curve.SupValue"/>
    public RationalExpression SupValue(string expressionName = "", ExpressionSettings? settings = null)
        => new SupValueExpression(this, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the infimum value attained by the curve expression, $\inf_{t \ge 0} f(t)$.
    /// </summary>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    /// <seealso cref="MinPlusAlgebra.Curve.InfValue"/>
    public RationalExpression InfValue(string expressionName = "", ExpressionSettings? settings = null)
        => new InfValueExpression(this, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the maximum value attained by the curve expression.
    /// </summary>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// Computing the resulting expression throws an <see cref="InvalidOperationException"/> if the curve does not
    /// attain a maximum (i.e., its supremum is not attained by any point of the curve); use <see cref="SupValue"/>
    /// if the supremum is sufficient.
    /// </remarks>
    /// <seealso cref="MinPlusAlgebra.Curve.MaxValue"/>
    public RationalExpression MaxValue(string expressionName = "", ExpressionSettings? settings = null)
        => new MaxValueExpression(this, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the minimum value attained by the curve expression.
    /// </summary>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// Computing the resulting expression throws an <see cref="InvalidOperationException"/> if the curve does not
    /// attain a minimum (i.e., its infimum is not attained by any point of the curve); use <see cref="InfValue"/>
    /// if the infimum is sufficient.
    /// </remarks>
    /// <seealso cref="MinPlusAlgebra.Curve.MinValue"/>
    public RationalExpression MinValue(string expressionName = "", ExpressionSettings? settings = null)
        => new MinValueExpression(this, expressionName, settings);

    #endregion Extrema

    /// <inheritdoc />
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
    /// <param name="ignoreNotMatchedExpressions">Whether unmatched expressions should be ignored.</param>
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

    /// <inheritdoc />
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
    
    /// <inheritdoc />
    public void Accept(IExpressionVisitor<Curve> visitor)
        => Accept((ICurveExpressionVisitor) visitor);

    /// <inheritdoc />
    public abstract void Accept(ICurveExpressionVisitor visitor);
    
    /// <inheritdoc />
    public TResult Accept<TResult>(IExpressionVisitor<Curve, TResult> visitor)
        => Accept((ICurveExpressionVisitor<TResult>) visitor);

    /// <inheritdoc />
    public abstract TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor);
    
    #endregion Accept
    
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
    /// <remarks>
    /// A side whose <see cref="IExpression.Name"/> is already bound is never flattened into.
    /// The caller has bound it as a value in its own right, and its internal structure stays its own.
    /// </remarks>
    private static int CheckNAryExpressionTypes(Type type, CurveExpression e1, CurveExpression e2)
    {
        if (e1.GetType() == type && string.IsNullOrEmpty(e1.Name))
            return 1;
        return e2.GetType() == type && string.IsNullOrEmpty(e2.Name) ? 2 : 0;
    }

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
    public string ToMppgString(int depth = 20, bool showRationalsAsName = false)
    {
        var mppgFormatterVisitor = new MppgFormatterVisitor(depth, showRationalsAsName);
        var (sb, _) = Accept(mppgFormatterVisitor);
        var mppgExpr = sb.ToString();

        return mppgExpr;
    }

    /// <summary>
    /// Returns a string that represents the current expression using the Unicode character set.
    /// </summary>
    public sealed override string ToString()
        => ToUnicodeString();
    
    /// <inheritdoc />
    public double Estimate()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    IGenericExpression<Curve> IGenericExpression<Curve>.WithName(string expressionName) => WithName(expressionName);

    /// <returns>
    /// The expression (new object of type <see cref="CurveExpression"/>) with the new generation.
    /// </returns>
    /// <remarks>
    /// Same non-destructive, cache-preserving shape as <see cref="WithName"/>.
    /// </remarks>
    public CurveExpression WithGeneration(int generation)
    {
        var changeGenerationVisitor = new RenameCurveVisitor(newGeneration: generation);
        Accept(changeGenerationVisitor);

        return changeGenerationVisitor.Result;
    }

    /// <inheritdoc />
    IGenericExpression<Curve> IGenericExpression<Curve>.WithGeneration(int generation) => WithGeneration(generation);

    /// <summary>
    /// Collapses this expression to a plain leaf wrapping its current <see cref="Value"/>.
    /// </summary>
    /// <remarks>
    /// Pure: this expression's own tree is untouched, and remains exactly as valid as before.
    /// </remarks>
    public ConcreteCurveExpression ToConcrete(string? expressionName = null)
        => new(Value, expressionName ?? Name, Settings);

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
    /// by <paramref name="rationalExpression"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="curveExpression">The curve expression.</param>
    /// <param name="rationalExpression">The rational expression.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator +(CurveExpression curveExpression, RationalExpression rationalExpression)
        => curveExpression.VerticalShift(rationalExpression);
    
    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <paramref name="rational"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="curveExpression">The curve expression.</param>
    /// <param name="rational">The rational value.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator +(CurveExpression curveExpression, Rational rational)
        => curveExpression.VerticalShift(rational);

    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <paramref name="rationalExpression"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="curveExpression">The curve expression.</param>
    /// <param name="rationalExpression">The rational expression.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator +(RationalExpression rationalExpression, CurveExpression curveExpression)
        => curveExpression.VerticalShift(rationalExpression);
    
    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <paramref name="rational"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="curveExpression">The curve expression.</param>
    /// <param name="rational">The rational value.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator +(Rational rational, CurveExpression curveExpression)
        => curveExpression.VerticalShift(rational);
    
    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <paramref name="rationalExpression"/>, i.e., computing $f(t) - K$.
    /// </summary>
    /// <param name="curveExpression">The curve expression.</param>
    /// <param name="rationalExpression">The rational expression.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator -(CurveExpression curveExpression, RationalExpression rationalExpression)
        => curveExpression.VerticalShift(rationalExpression.Negate());
    
    /// <summary>
    /// Creates a new expression that shifts the <see cref="CurveExpression"/>
    /// by <paramref name="rational"/>, i.e., computing $f(t) - K$.
    /// </summary>
    /// <param name="curveExpression">The curve expression.</param>
    /// <param name="rational">The rational value.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift always moves the entire curve, including the point at the origin.
    /// </remarks>
    public static CurveExpression operator -(CurveExpression curveExpression, Rational rational)
        => curveExpression.VerticalShift(-rational);

    #endregion Methods
}
