using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes NetCal expressions that evaluate to sequences.
/// The class aims at providing the main methods to build, manipulate and print network calculus expressions.
/// </summary>
public abstract record SequenceExpression : IGenericExpression<Sequence>, IVisitableSequence
{
    #region Properties

    /// <inheritdoc />
    public string Name { get; init; }

    /// <inheritdoc />
    public int Generation { get; init; }

    /// <inheritdoc />
    public ExpressionSettings? Settings { get; init; }

    /// <summary>
    /// Private cache field for <see cref="Value"/>
    /// </summary>
    internal Sequence? _value;

    /// <inheritdoc />
    public Sequence Value => _value ??= Compute();

    /// <inheritdoc cref="IExpression.IsComputed"/>
    public bool IsComputed
        => _value != null;

    /// <summary>
    /// Private cache field for <see cref="IsLeftContinuous"/>.
    /// </summary>
    internal bool? _isLeftContinuous;

    /// <summary>
    /// True if the sequence described by the expression is left continuous.
    /// This computes the expression's value, unlike its counterpart on a curve expression, which has structural visitors to answer without doing so.
    /// </summary>
    public bool IsLeftContinuous => _isLeftContinuous ??= Value.IsLeftContinuous;

    /// <summary>
    /// Private cache field for <see cref="IsRightContinuous"/>.
    /// </summary>
    internal bool? _isRightContinuous;

    /// <summary>
    /// True if the sequence described by the expression is right continuous.
    /// This computes the expression's value, unlike its counterpart on a curve expression, which has structural visitors to answer without doing so.
    /// </summary>
    public bool IsRightContinuous => _isRightContinuous ??= Value.IsRightContinuous;

    /// <summary>
    /// Private cache field for <see cref="IsNonNegative"/>.
    /// </summary>
    internal bool? _isNonNegative;

    /// <summary>
    /// True if the sequence described by the expression is non-negative.
    /// This computes the expression's value, unlike its counterpart on a curve expression, which has structural visitors to answer without doing so.
    /// </summary>
    public bool IsNonNegative => _isNonNegative ??= Value.IsNonNegative;

    /// <summary>
    /// Private cache field for <see cref="IsNonDecreasing"/>.
    /// </summary>
    internal bool? _isNonDecreasing;

    /// <summary>
    /// True if the sequence described by the expression is non-decreasing.
    /// This computes the expression's value, unlike its counterpart on a curve expression, which has structural visitors to answer without doing so.
    /// </summary>
    public bool IsNonDecreasing => _isNonDecreasing ??= Value.IsNonDecreasing;

    /// <summary>
    /// Private cache field for <see cref="IsIncreasing"/>.
    /// </summary>
    internal bool? _isIncreasing;

    /// <summary>
    /// True if the sequence described by the expression is increasing.
    /// This computes the expression's value, unlike its counterpart on a curve expression, which has structural visitors to answer without doing so.
    /// </summary>
    public bool IsIncreasing => _isIncreasing ??= Value.IsIncreasing;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a sequence expression.
    /// </summary>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">The settings of the expression.</param>
    protected SequenceExpression(string expressionName = "", ExpressionSettings? settings = null)
    {
        Name = expressionName;
        Settings = settings;
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other">The expression to copy.</param>
    /// <remarks>
    /// This constructor was made explicit to *not* copy the private cache fields when using the with operator.
    /// </remarks>
    public SequenceExpression(SequenceExpression other)
    {
        Name = other.Name;
        Generation = other.Generation;
        Settings = other.Settings;
    }

    #endregion Constructors

    /// <summary>
    /// True when this node's cached value is small enough that clearing it would free little.
    /// </summary>
    /// <remarks>
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
            return _value.Count <= threshold;
        }
    }

    /// <summary>
    /// Clears this node's own cached <see cref="Value"/>, and, per <paramref name="scope"/>, its descendants', mutating them in place.
    /// </summary>
    public void ClearValueCache(CacheClearScope scope = CacheClearScope.Subtree)
    {
        if (!ValueCacheIsCheap)
            _value = null;

        if (scope == CacheClearScope.SelfOnly)
            return;

        foreach (var child in (this as IExpressionNode)?.Children ?? [])
        {
            if (scope == CacheClearScope.SubtreeUntilNamed && !string.IsNullOrEmpty(child.Name))
                continue;
            child.ClearValueCache(scope);
        }
    }

    #region Equality

    /// <summary>
    /// True if <paramref name="other"/> is the same operator over the same operands.
    /// </summary>
    /// <remarks>
    /// <see cref="Name"/>, <see cref="Generation"/> and <see cref="Settings"/> do not participate:
    /// an expression by a different name is the same expression.
    /// Each arity overrides this further with its own operand comparison, on top of this base check.
    /// The synthesized equality a record would otherwise use compares the cache fields too, which makes an expression's hash change the moment its value is read.
    /// </remarks>
    public virtual bool Equals(SequenceExpression? other)
        => other is not null;

    /// <inheritdoc />
    /// <remarks>
    /// Seeded by the concrete <see cref="Type"/>, so every expression's hash differs from the hash of the bare value it wraps, a <see cref="ConcreteSequenceExpression"/> from its own <see cref="Sequence"/> included.
    /// </remarks>
    public override int GetHashCode()
        => HashCode.Combine(-363510328, GetType());

    #endregion Equality

    #region Methods

    /// <summary>
    /// Adds the opposite operator to the expression.
    /// </summary>
    public SequenceExpression Negate(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceNegateExpression(this, expressionName, settings);

    /// <summary>
    /// Implementation of the unary - operator as the negation of a <see cref="SequenceExpression"/>.
    /// </summary>
    public static SequenceExpression operator -(SequenceExpression expression)
        => expression.Negate();

    /// <summary>
    /// Creates a new expression composed of the floor of the current expression.
    /// </summary>
    public SequenceExpression Floor(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceFloorExpression(this, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the ceiling of the current expression.
    /// </summary>
    public SequenceExpression Ceil(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceCeilExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute its non-negative version.
    /// </summary>
    public SequenceExpression ToNonNegative(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceToNonNegativeExpression(this, expressionName, settings);





    /// <summary>
    /// Adds to the expression the operation to compute a left continuous version of it.
    /// </summary>
    public SequenceExpression ToLeftContinuous(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceToLeftContinuousExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute a right continuous version of it.
    /// </summary>
    public SequenceExpression ToRightContinuous(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceToRightContinuousExpression(this, expressionName, settings);


    /// <summary>
    /// Adds to the expression the operation to compute the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    public SequenceExpression LowerPseudoInverse(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceLowerPseudoInverseExpression(this, expressionName, settings);

    /// <summary>
    /// Adds to the expression the operation to compute the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    public SequenceExpression UpperPseudoInverse(string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceUpperPseudoInverseExpression(this, expressionName, settings);

    #region Addition

    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Addition(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(SequenceAdditionExpression), this, expression) switch
        {
            1 => ((SequenceAdditionExpression)this).Append(expression, expressionName, settings),
            2 => ((SequenceAdditionExpression)expression).Append(this, expressionName, settings),
            _ => new SequenceAdditionExpression([this, expression], expressionName, settings)
        };

    /// <summary>
    /// Creates a new expression composed of the addition between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Addition(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is SequenceAdditionExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteSequenceExpression(sequence, name), expressionName, settings);
        return new SequenceAdditionExpression([this, new ConcreteSequenceExpression(sequence, name)], expressionName, settings);
    }

    /// <summary>
    /// Creates a new expression composed of the addition between the expression <paramref name="left"/> and the sequence <paramref name="right"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public static SequenceExpression Addition(SequenceExpression left, Sequence right, string expressionName = "",
        ExpressionSettings? settings = null)
        => left.Addition(right, expressionName:expressionName, settings: settings);

    /// <summary>
    /// Implementation of the + operator as the addition between <see cref="SequenceExpression"/> objects.
    /// </summary>
    public static SequenceExpression operator +(SequenceExpression left, Sequence right)
        => Addition(left, right);

    /// <summary>
    /// Implementation of the + operator as the addition between <see cref="SequenceExpression"/> objects.
    /// </summary>
    public static SequenceExpression operator +(SequenceExpression left, SequenceExpression right)
        => left.Addition(right);

    #endregion Addition

    #region Subtraction

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Subtraction(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceSubtractionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Subtraction(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Subtraction(new ConcreteSequenceExpression(sequence, name), expressionName, settings);

    /// <summary>
    /// Implementation of the - operator as the subtraction between <see cref="SequenceExpression"/> objects.
    /// </summary>
    public static SequenceExpression operator -(SequenceExpression left, SequenceExpression right)
        => left.Subtraction(right);

    /// <summary>
    /// Implementation of the - operator as the subtraction between <see cref="SequenceExpression"/> objects.
    /// </summary>
    public static SequenceExpression operator -(SequenceExpression left, Sequence right)
        => left.Subtraction(right);

    /// <summary>
    /// Creates an expression that concatenates <paramref name="expression"/> after this one.
    /// </summary>
    /// <param name="expression">The sequence that follows this one.</param>
    /// <param name="preserveDelay">If true, the delay of <paramref name="expression"/> is kept as a gap between the two.</param>
    /// <param name="preserveShift">If true, the value <paramref name="expression"/> starts from is kept as a jump at the join.</param>
    /// <param name="expressionName">The name of the resulting expression.</param>
    /// <param name="settings">Settings for the resulting expression.</param>
    /// <remarks>
    /// The order matters, so this is not the same expression as concatenating the two the other way round.
    /// </remarks>
    public SequenceExpression Concat(
        SequenceExpression expression,
        bool preserveDelay = false,
        bool preserveShift = false,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceConcatExpression(this, expression, preserveDelay, preserveShift, expressionName, settings);

    /// <inheritdoc cref="Concat(SequenceExpression,bool,bool,string,ExpressionSettings)"/>
    /// <param name="sequence">The sequence that follows this one.</param>
    /// <param name="name">The name of <paramref name="sequence"/>.</param>
    /// <param name="preserveDelay">If true, the delay of <paramref name="sequence"/> is kept as a gap between the two.</param>
    /// <param name="preserveShift">If true, the value <paramref name="sequence"/> starts from is kept as a jump at the join.</param>
    /// <param name="expressionName">The name of the resulting expression.</param>
    /// <param name="settings">Settings for the resulting expression.</param>
    public SequenceExpression Concat(
        Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        bool preserveDelay = false,
        bool preserveShift = false,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => Concat(new ConcreteSequenceExpression(sequence, name), preserveDelay, preserveShift, expressionName, settings);

    #endregion Subtraction

    #region Minimum

    /// <summary>
    /// Creates a new expression composed of the minimum between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Minimum(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(SequenceMinimumExpression), this, expression) switch
        {
            1 => ((SequenceMinimumExpression)this).Append(expression, expressionName, settings),
            2 => ((SequenceMinimumExpression)expression).Append(this, expressionName, settings),
            _ => new SequenceMinimumExpression([this, expression], expressionName, settings)
        };

    /// <summary>
    /// Creates a new expression composed of the minimum between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Minimum(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
    {
        if (this is SequenceMinimumExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteSequenceExpression(sequence, name), expressionName, settings);
        return new SequenceMinimumExpression([this, new ConcreteSequenceExpression(sequence, name)], expressionName, settings);
    }

    #endregion Minimum

    #region Maximum

    /// <summary>
    /// Creates a new expression composed of the maximum between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Maximum(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(SequenceMaximumExpression), this, expression) switch
        {
            1 => ((SequenceMaximumExpression)this).Append(expression, expressionName, settings),
            2 => ((SequenceMaximumExpression)expression).Append(this, expressionName, settings),
            _ => new SequenceMaximumExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the maximum between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Maximum(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        {
        if (this is SequenceMaximumExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteSequenceExpression(sequence, name), expressionName, settings);
        return new SequenceMaximumExpression([this, new ConcreteSequenceExpression(sequence, name)], expressionName, settings);
    }

    #endregion Maximum

    #region Convolution

    /// <summary>
    /// Creates a new expression composed of the convolution between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Convolution(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(SequenceConvolutionExpression), this, expression) switch
        {
            1 => ((SequenceConvolutionExpression)this).Append(expression, expressionName, settings),
            2 => ((SequenceConvolutionExpression)expression).Append(this, expressionName, settings),
            _ => new SequenceConvolutionExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the convolution between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Convolution(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        {
        if (this is SequenceConvolutionExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteSequenceExpression(sequence, name), expressionName, settings);
        return new SequenceConvolutionExpression([this, new ConcreteSequenceExpression(sequence, name)], expressionName, settings);
    }

    #endregion Convolution

    #region Deconvolution

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Deconvolution(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceDeconvolutionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Deconvolution(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => Deconvolution(new ConcreteSequenceExpression(sequence, name), expressionName, settings);

    #endregion Deconvolution

    #region MaxPlusConvolution

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression MaxPlusConvolution(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => CheckNAryExpressionTypes(typeof(SequenceMaxPlusConvolutionExpression), this, expression) switch
        {
            1 => ((SequenceMaxPlusConvolutionExpression)this).Append(expression, expressionName, settings),
            2 => ((SequenceMaxPlusConvolutionExpression)expression).Append(this, expressionName, settings),
            _ => new SequenceMaxPlusConvolutionExpression([this, expression], expressionName, settings),
        };

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression MaxPlusConvolution(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        {
        if (this is SequenceMaxPlusConvolutionExpression e && string.IsNullOrEmpty(Name))
            return e.Append(new ConcreteSequenceExpression(sequence, name), expressionName, settings);
        return new SequenceMaxPlusConvolutionExpression([this, new ConcreteSequenceExpression(sequence, name)], expressionName,
            settings);
    }

    #endregion MaxPlusConvolution

    #region MaxPlusDeconvolution

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression MaxPlusDeconvolution(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceMaxPlusDeconvolutionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression MaxPlusDeconvolution(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => MaxPlusDeconvolution(new ConcreteSequenceExpression(sequence, name), expressionName, settings);

    #endregion MaxPlusDeconvolution

    #region Composition

    /// <summary>
    /// Creates a new expression composed of the composition between the current expression and the one passed as argument.
    /// </summary>
    public SequenceExpression Composition(SequenceExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceCompositionExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the current expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as argument.
    /// </summary>
    public SequenceExpression Composition(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceCompositionExpression(this, new ConcreteSequenceExpression(sequence, name), expressionName, settings);

    #endregion Composition

    #region DelayBy

    /// <summary>
    /// Creates a new expression that delays the current expression by the rational <paramref name="expression"/>, i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="HorizontalShift(RationalExpression,string,ExpressionSettings)"/>
    public SequenceExpression Delay(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceDelayExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the current expression by the rational <paramref name="delay"/>, i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Numerics.Rational,string,ExpressionSettings)"/>
    public SequenceExpression Delay(Rational delay, string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceDelayExpression(this, new RationalNumberExpression(delay), expressionName, settings);

    #endregion DelayBy

    #region ForwardBy

    /// <summary>
    /// Creates a new expression that forwards the current expression by the rational <paramref name="expression"/>, i.e., computing $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="HorizontalShift(RationalExpression,string,ExpressionSettings)"/>
    public SequenceExpression Forward(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceForwardExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the current expression by the rational <paramref name="time"/>, i.e., computing $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="HorizontalShift(Unipi.Nancy.Numerics.Rational,string,ExpressionSettings)"/>
    public SequenceExpression Forward(Rational time, string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceForwardExpression(this, new RationalNumberExpression(time), expressionName, settings);

    #endregion ForwardBy

    #region HorizontalShift

    /// <summary>
    /// Creates a new expression that shifts the current expression by the rational <paramref name="expression"/>, i.e., computing $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Sequence.HorizontalShift"/>
    public SequenceExpression HorizontalShift(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceHorizontalShiftExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current expression by the rational <paramref name="value"/>, i.e., computing $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Sequence.HorizontalShift"/>
    public SequenceExpression HorizontalShift(Rational value, string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceHorizontalShiftExpression(this, new RationalNumberExpression(value), expressionName, settings);

    #endregion HorizontalShift

    #region VerticalShift

    /// <summary>
    /// Creates a new expression that shifts the current expression by the rational <paramref name="expression"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public SequenceExpression VerticalShift(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceVerticalShiftExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the current sequence expression by the rational <paramref name="value"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public SequenceExpression VerticalShift(Rational value, string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalShiftExpression(this, new RationalNumberExpression(value), expressionName, settings);

    #endregion VerticalShift

    #region Scale

    /// <summary>
    /// Creates a new expression composed of the operation to scale the sequence corresponding to the current expression by the rational number described by the argument <paramref name="expression"/> of type <see cref="RationalExpression"/>.
    /// </summary>
    public SequenceExpression Scale(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceScaleExpression(this, expression, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the operation to scale the sequence corresponding to the current expression by the rational number <paramref name="scaleFactor"/>.
    /// </summary>
    public SequenceExpression Scale(Rational scaleFactor, string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceScaleExpression(this, new RationalNumberExpression(scaleFactor), expressionName, settings);

    /// <summary>
    /// Implementation of the * operator to scale a sequence expression by a rational number.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression operator *(SequenceExpression sequence, Rational scaleFactor)
        => sequence.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the * operator to scale a sequence expression by a rational number.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression operator *(Rational scaleFactor, SequenceExpression sequence)
        => sequence.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the * operator to scale a sequence expression by a rational expression.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression operator *(SequenceExpression sequence, RationalExpression scaleFactor)
        => sequence.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the * operator to scale a sequence expression by a rational expression.
    /// Computes $g(t) = k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression operator *(RationalExpression scaleFactor, SequenceExpression sequence)
        => sequence.Scale(scaleFactor);

    /// <summary>
    /// Implementation of the / operator to scale down a sequence expression by a rational number.
    /// Computes $g(t) = f(t) / k$.
    /// </summary>
    public static SequenceExpression operator /(SequenceExpression sequence, Rational scaleFactor)
        => sequence.Scale(1 / scaleFactor);

    /// <summary>
    /// Implementation of the / operator to scale down a sequence expression by a rational expression.
    /// Computes $g(t) = f(t) / k$.
    /// </summary>
    public static SequenceExpression operator /(SequenceExpression sequence, RationalExpression scaleFactor)
        => sequence.Scale(scaleFactor.Invert());

    #endregion Scale

    #region Sampling

    /// <summary>
    /// Creates a new expression that computes the value of the sequence expression at <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression ValueAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceValueAtExpression(this, new RationalNumberExpression(time), expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the value of the sequence expression at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression">The expression that provides the time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression ValueAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceValueAtExpression(this, timeExpression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the left-limit value of the sequence expression at <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression LeftLimitAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceLeftLimitAtExpression(this, new RationalNumberExpression(time), expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the left-limit value of the sequence expression at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression">The expression that provides the time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression LeftLimitAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceLeftLimitAtExpression(this, timeExpression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the right-limit value of the sequence expression at <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression RightLimitAt(
        Rational time,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceRightLimitAtExpression(this, new RationalNumberExpression(time), expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the right-limit value of the sequence expression at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    /// <param name="timeExpression">The expression that provides the time value.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
    public RationalExpression RightLimitAt(
        RationalExpression timeExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceRightLimitAtExpression(this, timeExpression, expressionName, settings);

    #endregion Sampling

    /// <inheritdoc />
    public Sequence Compute()
        => _value ??= new SequenceExpressionEvaluator().GetResult(this);

    /// <inheritdoc cref="IExpression.ComputeWithoutResult"/>
    public void ComputeWithoutResult()
    {
        Compute();
    }

    #region Replace

    /// <summary>
    /// Replaces every occurrence of a sub-expression in the expression to which the method is applied, and returns what the rewrite did.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <param name="ignoreNotMatchedExpressions">Whether unmatched expressions should be ignored.</param>
    /// <returns>
    /// The result, carrying the new expression, how many sites were replaced, and where.
    /// When the pattern matches nothing, the expression is the original, unchanged, and <see cref="ExpressionRewriteResult.Matched"/> is <see langword="false"/>.
    /// </returns>
    public ExpressionRewriteResult ReplaceByValueWithResult<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions = false
    )
        => ExpressionRewriter
            .ReplaceByValue(this, expressionPattern, newExpressionToReplace, ignoreNotMatchedExpressions);

    /// <summary>
    /// Replaces every occurrence of a sub-expression in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <param name="ignoreNotMatchedExpressions">Whether unmatched expressions should be ignored.</param>
    /// <returns>
    /// New expression object (of type <see cref="SequenceExpression"/>) with replaced sub-expressions.
    /// When the pattern matches nothing, this is the original expression, unchanged.
    /// Use <see cref="ReplaceByValueWithResult{T1}"/> to learn whether anything matched.
    /// </returns>
    public SequenceExpression ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions = false
    )
        => (SequenceExpression)ReplaceByValueWithResult(expressionPattern, newExpressionToReplace, ignoreNotMatchedExpressions).Expression;

    IGenericExpression<Sequence> IGenericExpression<Sequence>.ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions
    )
    => ReplaceByValue(expressionPattern, newExpressionToReplace, ignoreNotMatchedExpressions);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied, and returns what the rewrite did.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>
    /// The result, carrying the new expression and the one position that was replaced.
    /// A valid position is always replaced, so nothing is left unmatched; a position that does not fit the expression is rejected with an <see cref="ArgumentException"/>.
    /// </returns>
    public ExpressionRewriteResult ReplaceByPositionWithResult<T1>(ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpressionToReplace)
        => ExpressionRewriter
            .ReplaceByPosition(this, expressionPosition.Steps, newExpressionToReplace);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newExpressionToReplace">The new sub-expression.</param>
    /// <returns>
    /// New expression object (of type <see cref="SequenceExpression"/>) with replaced sub-expression.
    /// A valid position is always replaced, so nothing is left unmatched; a position that does not fit the expression is rejected with an <see cref="ArgumentException"/>.
    /// </returns>
    public SequenceExpression ReplaceByPosition<T1>(ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpressionToReplace)
        => (SequenceExpression)ReplaceByPositionWithResult(expressionPosition, newExpressionToReplace).Expression;

    IGenericExpression<Sequence> IGenericExpression<Sequence>.ReplaceByPosition<T1>(ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpressionToReplace) => ReplaceByPosition(expressionPosition, newExpressionToReplace);

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newValueToReplace">The new value to replace the sub-expression.</param>
    /// <param name="name">The name of the new value.</param>
    /// <returns>New expression object (of type <see cref="SequenceExpression"/>) with replaced sub-expression.</returns>
    /// <remarks>
    /// A <see cref="Curve"/> is hosted by the sequence nodes that take a curve operand, the cuts <see cref="CurveCutExpression"/> and <see cref="CurveCutToNeighbourhoodExpression"/>.
    /// A valid position is always replaced; a position that does not fit the expression is rejected with an <see cref="ArgumentException"/>.
    /// </remarks>
    public SequenceExpression ReplaceByPosition(ExpressionPosition expressionPosition,
        Curve newValueToReplace,
        [CallerArgumentExpression("newValueToReplace")] string name = ""
    )
        => ReplaceByPosition(expressionPosition, newValueToReplace.ToExpression(name));

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced.</param>
    /// <param name="newValueToReplace">The new value to replace the sub-expression.</param>
    /// <param name="name">The name of the new value.</param>
    /// <returns>New expression object (of type <see cref="SequenceExpression"/>) with replaced sub-expression.</returns>
    /// <remarks>
    /// A <see cref="Rational"/> is hosted by the sequence nodes that take a rational operand, such as <see cref="SequenceScaleExpression"/>.
    /// A valid position is always replaced; a position that does not fit the expression is rejected with an <see cref="ArgumentException"/>.
    /// </remarks>
    public SequenceExpression ReplaceByPosition(ExpressionPosition expressionPosition,
        Rational newValueToReplace,
        [CallerArgumentExpression("newValueToReplace")] string name = ""
    )
        => ReplaceByPosition(expressionPosition, newValueToReplace.ToExpression(name));

    IGenericExpression<Sequence> IGenericExpression<Sequence>.ReplaceByPosition<T1>(IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpressionToReplace) => ReplaceByPosition(new ExpressionPosition(positionPath), newExpressionToReplace);

    #endregion Replace

    #region Equivalence

    /// <summary>
    /// Applies an equivalence to the current expression, at every site where it matches, and returns what the rewrite did.
    /// </summary>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side expression, this parameter identifies the direction of application of the equivalence (match of the left side, and substitution with the right side, or vice versa, or both).</param>
    /// <returns>
    /// The result, carrying the new expression, how many sites were rewritten, where, and what the equivalence's placeholders bound to.
    /// When the equivalence matches nothing, the expression is the original, unchanged, and <see cref="ExpressionRewriteResult.Matched"/> is <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The host's value type takes no part: an equivalence applies at every matching subtree, so a curve equivalence has sites under a sequence expression, reached through the cut nodes.
    /// </remarks>
    public ExpressionRewriteResult ApplyEquivalenceWithResult(Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
        => ExpressionRewriter
            .ApplyEquivalence(this, equivalence, checkType);

    /// <summary>
    /// Applies an equivalence to the current expression.
    /// </summary>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side expression, this parameter identifies the direction of application of the equivalence (match of the left side, and substitution with the right side, or vice versa, or both).</param>
    /// <returns>
    /// The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// When the equivalence matches nothing, this is the original expression, unchanged.
    /// Use <see cref="ApplyEquivalenceWithResult"/> to learn whether anything matched.
    /// </returns>
    public SequenceExpression ApplyEquivalence(Equivalence equivalence, CheckType checkType = CheckType.CheckLeftOnly)
        => (SequenceExpression)ApplyEquivalenceWithResult(equivalence, checkType).Expression;

    IGenericExpression<Sequence> IGenericExpression<Sequence>.ApplyEquivalence(Equivalence equivalence, CheckType checkType)
        => ApplyEquivalence(equivalence, checkType);

    IGenericExpression<Sequence> IGenericExpression<Sequence>.ApplyEquivalenceByPosition(IEnumerable<string> positionPath,
        Equivalence equivalence,
        CheckType checkType)
        => ApplyEquivalenceByPosition(new ExpressionPosition(positionPath), equivalence, checkType);

    /// <summary>
    /// Applies an equivalence to the current expression at a certain position, and returns what the rewrite did.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side expression, this parameter identifies the direction of application of the equivalence (match of the left side, and substitution with the right side, or vice versa, or both).</param>
    /// <returns>
    /// The result, carrying the new expression, the one position if it was rewritten, and what the equivalence's placeholders bound to.
    /// When the equivalence matches nothing at the position, the expression is the original, unchanged, and <see cref="ExpressionRewriteResult.Matched"/> is <see langword="false"/>.
    /// </returns>
    public ExpressionRewriteResult ApplyEquivalenceByPositionWithResult(ExpressionPosition expressionPosition,
        Equivalence equivalence, CheckType checkType = CheckType.CheckLeftOnly)
        => ExpressionRewriter
            .ApplyEquivalenceByPosition(this, expressionPosition.Steps, equivalence, checkType);

    /// <summary>
    /// Applies an equivalence to the current expression, allowing the user to specify the position in the expression in which the equivalence should be applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side expression, this parameter identifies the direction of application of the equivalence (match of the left side, and substitution with the right side, or vice versa, or both).</param>
    /// <returns>
    /// The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// When the equivalence matches nothing at the position, this is the original expression, unchanged.
    /// Use <see cref="ApplyEquivalenceByPositionWithResult"/> to learn whether anything matched.
    /// </returns>
    public SequenceExpression ApplyEquivalenceByPosition(ExpressionPosition expressionPosition, Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
        => (SequenceExpression)ApplyEquivalenceByPositionWithResult(expressionPosition, equivalence, checkType).Expression;

    /// <inheritdoc />
    IGenericExpression<Sequence> IGenericExpression<Sequence>.ApplyEquivalenceByPosition(
        ExpressionPosition expressionPosition, Equivalence equivalence,
        CheckType checkType)
        => ApplyEquivalenceByPosition(expressionPosition, equivalence, checkType);

    /// <summary>
    /// Checks if two expressions are equivalent by computing their values.
    /// </summary>
    public bool Equivalent(IGenericExpression<Sequence> other)
        => Sequence.Equivalent(Compute(),
            other.Compute());

    #endregion Equivalence

    #region Accept

    /// <inheritdoc />
    public void Accept(IExpressionVisitor<Sequence> visitor)
        => Accept((ISequenceExpressionVisitor) visitor);

    /// <inheritdoc />
    public abstract void Accept(ISequenceExpressionVisitor visitor);

    /// <inheritdoc />
    public TResult Accept<TResult>(IExpressionVisitor<Sequence, TResult> visitor)
        => Accept((ISequenceExpressionVisitor<TResult>) visitor);

    /// <inheritdoc />
    public abstract TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor);

    #endregion Accept

    /// <summary>
    /// Private function used during the creation of the expressions to keep the n-ary expressions at the same level of the expression tree.
    /// </summary>
    /// <param name="type">Type of the expression that needs to be created</param>
    /// <param name="e1">Left operand</param>
    /// <param name="e2">Right operand</param>
    /// <returns>
    /// If <paramref name="type"/> is equal to the type of <paramref name="e1"/> the function returns 1, which means that the new expression must be created by appending <paramref name="e2"/> to <paramref name="e1"/>.
    /// The opposite holds if <paramref name="type"/> is equal to the type of <paramref name="e2"/>, in this case the function returns 2.
    /// The function returns 0 when the <paramref name="type"/> is different by the type of <paramref name="e1"/> and <paramref name="e2"/>.
    /// </returns>
    private static int CheckNAryExpressionTypes(Type type, SequenceExpression e1, SequenceExpression e2)
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

        return sb.ToString();
    }


    /// <inheritdoc />
    public string ToUnicodeString(int depth = 20, bool showRationalsAsName = false)
    {
        var unicodeFormatterVisitor = new UnicodeFormatterVisitor(depth, showRationalsAsName);
        var (sb, _) = Accept(unicodeFormatterVisitor);

        return sb.ToString();
    }


    /// <summary>
    /// Returns a string that represents the current expression using the Unicode character set.
    /// </summary>
    public sealed override string ToString()
        => ToUnicodeString();

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">
    /// Always, as on <see cref="CurveExpression.Estimate"/>: no cost model is implemented for either tree.
    /// </exception>
    public double Estimate()
        => throw new NotImplementedException();

    /// <inheritdoc />
    public ExpressionPosition RootPosition() => new();

    /// <summary>
    /// Changes the name of the expression.
    /// </summary>
    /// <param name="expressionName">The new name of the expression</param>
    /// <returns>
    /// The expression (new object of type <see cref="SequenceExpression"/>) with the new name.
    /// </returns>
    /// <remarks>
    /// Renaming can be done using the with operator, but that will clear out the cache fields, causing re-computation of the expression.
    /// This method will instead copy over the cache fields.
    /// </remarks>
    public SequenceExpression WithName(string expressionName)
    {
        var changeNameVisitor = new RenameSequenceVisitor(expressionName);
        Accept(changeNameVisitor);

        return changeNameVisitor.Result;
    }

    /// <inheritdoc />
    IGenericExpression<Sequence> IGenericExpression<Sequence>.WithName(string expressionName) => WithName(expressionName);

    /// <summary>
    /// Returns a copy of this expression with the given generation.
    /// </summary>
    /// <param name="generation">The new generation.</param>
    /// <remarks>
    /// Same non-destructive, cache-preserving shape as <see cref="WithName"/>.
    /// </remarks>
    public SequenceExpression WithGeneration(int generation)
    {
        var changeGenerationVisitor = new RenameSequenceVisitor(newGeneration: generation);
        Accept(changeGenerationVisitor);

        return changeGenerationVisitor.Result;
    }

    /// <inheritdoc />
    IGenericExpression<Sequence> IGenericExpression<Sequence>.WithGeneration(int generation) => WithGeneration(generation);

    /// <summary>
    /// Not supported: the mppg language has no syntax for sequence expressions.
    /// </summary>
    /// <exception cref="NotSupportedException">Always.</exception>
    /// <remarks>
    /// The grammar in <c>NetCalG.g4</c> describes curves and rationals only, so there is nothing for a sequence expression to be written as.
    /// Returning the plain <see cref="ToString"/> instead would hand back something that looks like mppg and does not parse, which is worse than refusing.
    /// </remarks>
    public string ToMppgString(int depth = 20, bool showRationalsAsName = false)
        => throw new NotSupportedException("The mppg language has no syntax for sequence expressions.");

    /// <summary>
    /// This operator returns true if the value of a sequence expression is below or equal than the value of another one.
    /// </summary>
    public static bool operator <=(SequenceExpression expressionL, SequenceExpression expressionR)
        => expressionL.Compute() <= expressionR.Compute();

    /// <summary>
    /// This operator returns true if the value of a sequence expression is greater or equal than the value of another one.
    /// </summary>
    public static bool operator >=(SequenceExpression expressionL, SequenceExpression expressionR)
        => expressionL.Compute() >= expressionR.Compute();

    /// <summary>
    /// Creates a new expression that shifts the <see cref="SequenceExpression"/>
    /// by <paramref name="rationalExpression"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="sequenceExpression">The sequence expression.</param>
    /// <param name="rationalExpression">The rational expression.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public static SequenceExpression operator +(SequenceExpression sequenceExpression, RationalExpression rationalExpression)
        => sequenceExpression.VerticalShift(rationalExpression);

    /// <summary>
    /// Creates a new expression that shifts the <see cref="SequenceExpression"/>
    /// by <paramref name="rational"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="sequenceExpression">The sequence expression.</param>
    /// <param name="rational">The rational value.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public static SequenceExpression operator +(SequenceExpression sequenceExpression, Rational rational)
        => sequenceExpression.VerticalShift(rational);

    /// <summary>
    /// Creates a new expression that shifts the <see cref="SequenceExpression"/>
    /// by <paramref name="rationalExpression"/>, i.e., computing $f(t) - K$.
    /// </summary>
    /// <param name="sequenceExpression">The sequence expression.</param>
    /// <param name="rationalExpression">The rational expression.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public static SequenceExpression operator -(SequenceExpression sequenceExpression, RationalExpression rationalExpression)
        => sequenceExpression.VerticalShift(rationalExpression.Negate());

    /// <summary>
    /// Creates a new expression that shifts the <see cref="SequenceExpression"/>
    /// by <paramref name="rational"/>, i.e., computing $f(t) - K$.
    /// </summary>
    /// <param name="sequenceExpression">The sequence expression.</param>
    /// <param name="rational">The rational value.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public static SequenceExpression operator -(SequenceExpression sequenceExpression, Rational rational)
        => sequenceExpression.VerticalShift(-rational);

    /// <summary>
    /// Creates a new expression that shifts the <see cref="SequenceExpression"/>
    /// by <paramref name="rationalExpression"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="sequenceExpression">The sequence expression.</param>
    /// <param name="rationalExpression">The rational expression.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public static SequenceExpression operator +(RationalExpression rationalExpression, SequenceExpression sequenceExpression)
        => sequenceExpression.VerticalShift(rationalExpression);

    /// <summary>
    /// Creates a new expression that shifts the <see cref="SequenceExpression"/>
    /// by <paramref name="rational"/>, i.e., computing $f(t) + K$.
    /// </summary>
    /// <param name="sequenceExpression">The sequence expression.</param>
    /// <param name="rational">The rational value.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// The shift moves every element, including a point at the origin where the sequence has one.
    /// </remarks>
    public static SequenceExpression operator +(Rational rational, SequenceExpression sequenceExpression)
        => sequenceExpression.VerticalShift(rational);

    #endregion Methods
}
