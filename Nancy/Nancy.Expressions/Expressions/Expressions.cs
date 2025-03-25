using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Static class with functions to build NetCal expressions
/// </summary>
public static class Expressions
{
    /// <summary>
    /// Creates a <see cref="ConcreteCurveExpression"/> object from a <see cref="Curve"/> object.
    /// </summary>
    public static ConcreteCurveExpression FromCurve(Curve curve,
        [CallerArgumentExpression("curve")] string name = "") =>
        new(curve, name);

    /// <summary>
    /// Creates a <see cref="RationalNumberExpression"/> object from a <see cref="Rational"/> object.
    /// </summary>
    public static RationalNumberExpression FromRational(Rational number,
        [CallerArgumentExpression("number")] string name = "") =>
        new(number, name);

    #region Negate

    /// <summary>
    /// Adds the opposite operator to the expression passed as argument.
    /// </summary>
    public static CurveExpression Negate(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.Negate(expressionName, settings);

    /// <summary>
    /// Adds the opposite operator to the curve passed as argument (internally converted to
    /// <see cref="ConcreteCurveExpression"/>.
    /// </summary>
    public static CurveExpression Negate(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new NegateExpression(curve, name, expressionName, settings);

    #endregion Negate

    #region ToNonNegative

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute its non-negative version.
    /// </summary>
    public static CurveExpression ToNonNegative(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.ToNonNegative(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument the operation to compute its non-negative version.
    /// </summary>
    public static CurveExpression ToNonNegative(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ToNonNegativeExpression(curve, name, expressionName, settings);

    #endregion ToNonNegative

    #region SubAdditiveClosure

    /// <summary>
    /// Adds the sub-additive closure operator to the expression passed as argument.
    /// </summary>
    public static CurveExpression SubAdditiveClosure(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.SubAdditiveClosure(expressionName, settings);

    /// <summary>
    /// Adds the sub-additive closure operator to the curve passed as argument
    /// (internally converted to <see cref="ConcreteCurveExpression"/>).
    /// </summary>
    public static CurveExpression SubAdditiveClosure(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SubAdditiveClosureExpression(curve, name, expressionName, settings);

    #endregion SubAdditiveClosure

    #region SuperAdditiveClosure

    /// <summary>
    /// Adds the super-additive closure operator to the expression passed as argument.
    /// </summary>
    public static CurveExpression SuperAdditiveClosure(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.SuperAdditiveClosure(expressionName, settings);

    /// <summary>
    /// Adds the super-additive closure operator to the curve passed as argument (internally converted
    /// to <see cref="ConcreteCurveExpression"/>).
    /// </summary>
    public static CurveExpression SuperAdditiveClosure(Curve curve,
        [CallerArgumentExpression("curve")] string name = "", string expressionName = "",
        ExpressionSettings? settings = null)
        => new SuperAdditiveClosureExpression(curve, name, expressionName, settings);

    #endregion SuperAdditiveClosure

    #region ToUpperNonDecreasing

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute its upper non-decreasing version.
    /// </summary>
    public static CurveExpression ToUpperNonDecreasing(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.ToUpperNonDecreasing(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument the operation to compute its upper non-decreasing version.
    /// </summary>
    public static CurveExpression ToUpperNonDecreasing(Curve curve,
        [CallerArgumentExpression("curve")] string name = "", string expressionName = "",
        ExpressionSettings? settings = null)
        => new ToUpperNonDecreasingExpression(curve, name, expressionName, settings);

    #endregion ToUpperNonDecreasing

    #region ToLowerNonDecreasing

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute its lower non-decreasing version.
    /// </summary>
    public static CurveExpression ToLowerNonDecreasing(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.ToLowerNonDecreasing(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument (converted to <see cref="ConcreteCurveExpression"/>) the operation to
    /// compute its lower non-decreasing version.
    /// </summary>
    public static CurveExpression ToLowerNonDecreasing(Curve curve,
        [CallerArgumentExpression("curve")] string name = "", string expressionName = "",
        ExpressionSettings? settings = null)
        => new ToLowerNonDecreasingExpression(curve, name, expressionName, settings);

    #endregion ToLowerNonDecreasing

    #region ToLeftContinuous

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute a left continuous version of it.
    /// </summary>
    public static CurveExpression ToLeftContinuous(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.ToLeftContinuous(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument (converted to <see cref="ConcreteCurveExpression"/>) the operation to
    /// compute a left continuous version of it.
    /// </summary>
    public static CurveExpression ToLeftContinuous(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ToLeftContinuousExpression(curve, name, expressionName, settings);

    #endregion ToLeftContinuous

    #region ToRightContinuous

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute a right continuous version of it.
    /// </summary>
    public static CurveExpression ToRightContinuous(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.ToRightContinuous(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument (converted to <see cref="ConcreteCurveExpression"/>) the operation to
    /// compute a right continuous version of it.
    /// </summary>
    public static CurveExpression ToRightContinuous(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ToRightContinuousExpression(curve, name, expressionName, settings);

    #endregion ToRightContinuous

    #region WithZeroOrigin

    /// <summary>
    /// Adds to the expression passed as argument an operation which enforces it to assume 0 at time 0.
    /// </summary>
    public static CurveExpression WithZeroOrigin(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.WithZeroOrigin(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument (converted to <see cref="ConcreteCurveExpression"/> an operation which
    /// enforces it to assume 0 at time 0.
    /// </summary>
    public static CurveExpression WithZeroOrigin(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new WithZeroOriginExpression(curve, name, expressionName, settings);

    #endregion WithZeroOrigin

    #region LowerPseudoInverse

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute the lower pseudo-inverse function,
    /// $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    public static CurveExpression LowerPseudoInverse(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.LowerPseudoInverse(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument (converted to <see cref="ConcreteCurveExpression"/>) the operation to
    /// compute the lower pseudo-inverse function,
    /// $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    public static CurveExpression LowerPseudoInverse(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new LowerPseudoInverseExpression(curve, name, expressionName, settings);

    #endregion LowerPseudoInverse

    #region UpperPseudoInverse

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute the upper pseudo-inverse function,
    /// $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    public static CurveExpression UpperPseudoInverse(CurveExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.UpperPseudoInverse(expressionName, settings);

    /// <summary>
    /// Adds to the curve passed as argument (converted to <see cref="ConcreteCurveExpression"/>) the operation to
    /// compute the upper pseudo-inverse function,
    /// $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    public static CurveExpression UpperPseudoInverse(Curve curve, [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new UpperPseudoInverseExpression(curve, name, expressionName, settings);

    #endregion UpperPseudoInverse

    #region Addition

    /// <summary>
    /// Creates a new expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static CurveExpression Addition(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Addition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the expression and the curve (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Addition(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Addition(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the two curves (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Addition(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new AdditionExpression([curveL, curveR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the curve <see cref="curveL"/> (converted to
    /// <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static CurveExpression Addition(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromCurve(curveL, nameL).Addition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the curves passed as argument in the collection
    /// <see cref="curves"/>.
    /// </summary>
    public static CurveExpression Addition(IReadOnlyCollection<Curve> curves, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new AdditionExpression(curves, names, expressionName, settings);

    #endregion Addition

    #region Subtraction

    /// <summary>
    /// Creates a new expression composed of the subtraction between the two expressions passed as arguments.
    /// </summary>
    public static CurveExpression Subtraction(CurveExpression expressionL, CurveExpression expressionR, bool nonNegative = true,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Subtraction(expressionR, nonNegative, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Subtraction(CurveExpression expression, Curve curve, bool nonNegative = true,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Subtraction(curve, nonNegative, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the two curves (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Subtraction(Curve curveL, Curve curveR, bool nonNegative = true,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SubtractionExpression(curveL, nameL, curveR, nameR, nonNegative, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the curve <see cref="curveL"/> (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as
    /// arguments.
    /// </summary>
    public static CurveExpression Subtraction(Curve curveL, CurveExpression expressionR, bool nonNegative = true,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SubtractionExpression(curveL, nameL, expressionR, nonNegative, expressionName, settings);

    #endregion Subtraction

    #region Minimum

    /// <summary>
    /// Creates a new expression composed of the minimum between the expressions passed as arguments.
    /// </summary>
    public static CurveExpression Minimum(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Minimum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the expression and the curve (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Minimum(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Minimum(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the two curves (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Minimum(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new MinimumExpression([curveL, curveR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the curve <see cref="curveL"/> (converted to
    /// <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static CurveExpression Minimum(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromCurve(curveL, nameL).Minimum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the curves passed as argument in the collection
    /// <see cref="curves"/>.
    /// </summary>
    public static CurveExpression Minimum(IReadOnlyCollection<Curve> curves, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new MinimumExpression(curves, names, expressionName, settings);

    #endregion Minimum

    #region Maximum

    /// <summary>
    /// Creates a new expression composed of the maximum between the expressions passed as arguments.
    /// </summary>
    public static CurveExpression Maximum(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Maximum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the expression and the curve (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Maximum(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Maximum(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the two curves (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Maximum(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new MaximumExpression([curveL, curveR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the curve <see cref="curveL"/> (converted to
    /// <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static CurveExpression Maximum(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromCurve(curveL, nameL).Maximum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the curves passed as argument in the collection
    /// <see cref="curves"/>.
    /// </summary>
    public static CurveExpression Maximum(IReadOnlyCollection<Curve> curves, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new MaximumExpression(curves, names, expressionName, settings);

    #endregion Maximum

    #region Convolution

    /// <summary>
    /// Creates a new expression composed of the convolution between the expressions passed as arguments.
    /// </summary>
    public static CurveExpression Convolution(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Convolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the expression and the curve (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Convolution(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Convolution(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the two curves (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Convolution(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ConvolutionExpression([curveL, curveR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the curve <see cref="curveL"/> (converted to
    /// <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static CurveExpression Convolution(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromCurve(curveL, nameL).Convolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the curves passed as argument in the collection
    /// <see cref="curves"/>.
    /// </summary>
    public static CurveExpression Convolution(IReadOnlyCollection<Curve> curves, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new ConvolutionExpression(curves, names, expressionName, settings);

    #endregion Convolution

    #region Deconvolution

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the two expressions passed as arguments.
    /// </summary>
    public static CurveExpression Deconvolution(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Deconvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Deconvolution(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Deconvolution(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the two curves (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Deconvolution(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new DeconvolutionExpression(curveL, nameL, curveR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the curve <see cref="curveL"/> (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as
    /// arguments.
    /// </summary>
    public static CurveExpression Deconvolution(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new DeconvolutionExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion Deconvolution

    #region MaxPlusConvolution

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the expressions passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusConvolution(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.MaxPlusConvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the expression and the curve
    /// (converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusConvolution(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.MaxPlusConvolution(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the two curves (converted to
    /// <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusConvolution(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new MaxPlusConvolutionExpression([curveL, curveR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the curve <see cref="curveL"/> (converted
    /// to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusConvolution(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromCurve(curveL, nameL).MaxPlusConvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the curves passed as argument in the
    /// collection <see cref="curves"/>.
    /// </summary>
    public static CurveExpression MaxPlusConvolution(IReadOnlyCollection<Curve> curves,
        IReadOnlyCollection<string> names, string expressionName = "", ExpressionSettings? settings = null)
        => new MaxPlusConvolutionExpression(curves, names, expressionName, settings);

    #endregion MaxPlusConvolution

    #region MaxPlusDeconvolution

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the two expressions passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusDeconvolution(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.MaxPlusDeconvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusDeconvolution(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.MaxPlusDeconvolution(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the two curves (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusDeconvolution(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new MaxPlusDeconvolutionExpression(curveL, nameL, curveR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the curve <see cref="curveL"/>
    /// (internally converted to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/>
    /// passed as arguments.
    /// </summary>
    public static CurveExpression MaxPlusDeconvolution(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new MaxPlusDeconvolutionExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion MaxPlusDeconvolution

    #region Composition

    /// <summary>
    /// Creates a new expression composed of the composition between the two expressions passed as arguments.
    /// </summary>
    public static CurveExpression Composition(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Composition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the expression and the curve (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Composition(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Composition(curve, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the two curves (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static CurveExpression Composition(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new CompositionExpression(curveL, nameL, curveR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the curve <see cref="curveL"/> (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/> passed as
    /// arguments.
    /// </summary>
    public static CurveExpression Composition(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new CompositionExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion Composition

    #region HorizontalDeviation

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the two expressions passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => new HorizontalDeviationExpression(expressionL, expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the expression and the curve
    /// (internally converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new HorizontalDeviationExpression(expression, new ConcreteCurveExpression(curve, name), expressionName,
            settings);

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the two curves (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new HorizontalDeviationExpression(curveL, nameL, curveR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the curve <see cref="curveL"/>
    /// (internally converted to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/>
    /// passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new HorizontalDeviationExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion HorizontalDeviation

    #region VerticalDeviation

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the two expressions passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(CurveExpression expressionL, CurveExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => new VerticalDeviationExpression(expressionL, expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the expression and the curve
    /// (internally converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(CurveExpression expression, Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new VerticalDeviationExpression(expression, new ConcreteCurveExpression(curve, name), expressionName,
            settings);

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the two curves (internally
    /// converted to <see cref="ConcreteCurveExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(Curve curveL, Curve curveR,
        [CallerArgumentExpression("curveL")] string nameL = "", [CallerArgumentExpression("curveR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new VerticalDeviationExpression(curveL, nameL, curveR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the curve <see cref="curveL"/>
    /// (internally converted to <see cref="ConcreteCurveExpression"/>) and the expression <see cref="expressionR"/>
    /// passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(Curve curveL, CurveExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new VerticalDeviationExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion VerticalDeviation

    #region DelayBy

    /// <summary>
    /// Creates a new expression that delays the curve expression
    /// <see cref="expressionL"/> by the rational expression <see cref="expressionR"/>, i.e., computing $f(t - T)$.
    /// </summary>
    public static CurveExpression DelayBy(CurveExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.DelayBy(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the curve expression
    /// <see cref="expression"/> by the rational <see cref="delay"/>, i.e., computing $f(t - T)$.
    /// </summary>
    public static CurveExpression DelayBy(CurveExpression expression, Rational delay,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.DelayBy(delay, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the curve <see cref="curveL"/>
    /// by the rational <see cref="delay"/>, i.e., computing $f(t - T)$.
    /// </summary>
    public static CurveExpression DelayBy(Curve curveL, Rational delay,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new DelayByExpression(curveL, nameL, delay, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the curve <see cref="curveL"/>
    /// by the rational expression <see cref="expressionR"/>, i.e., computing $f(t - T)$.
    /// </summary>
    public static CurveExpression DelayBy(Curve curveL, RationalExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new DelayByExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion DelayBy

    #region ForwardBy

    /// <summary>
    /// Creates a new expression that forwards the curve expression
    /// <see cref="expressionL"/> by the rational expression <see cref="expressionR"/>, i.e., computing $f(t + T)$.
    /// </summary>
    public static CurveExpression ForwardBy(CurveExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.ForwardBy(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the curve expression
    /// <see cref="expression"/> by the rational <see cref="time"/>, i.e., computing $f(t + T)$.
    /// </summary>
    public static CurveExpression ForwardBy(CurveExpression expression, Rational time,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.ForwardBy(time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the curve <see cref="curveL"/>
    /// by the rational <see cref="time"/>, i.e., computing $f(t + T)$.
    /// </summary>
    public static CurveExpression ForwardBy(Curve curveL, Rational time,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ForwardByExpression(curveL, nameL, time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the curve <see cref="curveL"/>
    /// by the rational expression <see cref="expressionR"/>, i.e., computing $f(t + T)$.
    /// </summary>
    public static CurveExpression ForwardBy(Curve curveL, RationalExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ForwardByExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion ForwardBy

    #region Scale

    /// <summary>
    /// Creates a new expression that scales the curve expression
    /// <see cref="expressionL"/> by the rational expression <see cref="expressionR"/>, i.e. $k \cdot f(t)$. 
    /// </summary>
    public static CurveExpression Scale(CurveExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Scale(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that scales the curve expression
    /// <see cref="expression"> by the rational <see cref="scaleFactor"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static CurveExpression Scale(CurveExpression expression, Rational scaleFactor,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Scale(scaleFactor, expressionName, settings);

    /// <summary>
    /// Creates a new expression that scales the curve <see cref="curveL"/>
    /// by the rational <see cref="scaleFactor"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static CurveExpression Scale(Curve curveL, Rational scaleFactor,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ScaleExpression(curveL, nameL, scaleFactor, expressionName, settings);

    /// <summary>
    /// Creates a new expression that scales the curve <see cref="curveL"/>
    /// by the rational expression <see cref="expressionR"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static CurveExpression Scale(Curve curveL, RationalExpression expressionR,
        [CallerArgumentExpression("curveL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new ScaleExpression(curveL, nameL, expressionR, expressionName, settings);

    #endregion Scale

    #region RationalAddition

    /// <summary>
    /// Creates a new expression composed of the addition between the expression passed as first argument and the
    /// rational number passed as second argument (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression RationalAddition(RationalExpression expression, Rational number,
        [CallerArgumentExpression("number")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Addition(number, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static RationalExpression RationalAddition(RationalExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Addition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the two rational numbers passed as arguments
    /// (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression RationalAddition(Rational rationalL, Rational rationalR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "", [CallerArgumentExpression("rationalR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalAdditionExpression([rationalL, rationalR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the rational number <see cref="rationalL"/> and the
    /// expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression RationalAddition(Rational rationalL, RationalExpression expressionR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromRational(rationalL, nameL).Addition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the rational numbers passed as
    /// arguments using the collection <see cref="numbers"/>.
    /// </summary>
    public static RationalExpression RationalAddition(IReadOnlyCollection<Rational> numbers,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalAdditionExpression(numbers, names, expressionName, settings);

    #endregion RationalAddition
    
    #region RationalSubtraction
    
    /// <summary>
    /// Creates a new expression composed of the subtraction between the two rational expressions passed as arguments.
    /// </summary>
    public static RationalExpression RationalSubtraction(RationalExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Subtraction(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the expression and the number (internally
    /// converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression RationalSubtraction(RationalExpression expression, Rational number,
        [CallerArgumentExpression("number")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Subtraction(number, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the two numbers (internally
    /// converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression RationalSubtraction(Rational numberL, Rational numberR,
        [CallerArgumentExpression("numberL")] string nameL = "", [CallerArgumentExpression("numberR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalSubtractionExpression(FromRational(numberL,nameL),FromRational(numberR, nameR), expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the number <see cref="numberL"/> (internally
    /// converted to <see cref="RationalNumberExpression"/>) and the expression <see cref="expressionR"/> passed as
    /// arguments.
    /// </summary>
    public static RationalExpression RationalSubtraction(Rational numberL, RationalExpression expressionR,
        [CallerArgumentExpression("numberL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalSubtractionExpression(FromRational(numberL,nameL), expressionR, expressionName, settings);

    #endregion RationalSubtraction
    
    #region Product
    
    /// <summary>
    /// Creates a new expression composed of the product between the expression passed as first argument and the
    /// rational number passed as second argument (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression Product(RationalExpression expression, Rational number,
        [CallerArgumentExpression("number")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Product(number, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the product between the expressions passed as arguments.
    /// </summary>
    public static RationalExpression Product(RationalExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Product(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the product between the two rational numbers passed as arguments
    /// (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression Product(Rational rationalL, Rational rationalR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "", [CallerArgumentExpression("rationalR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalProductExpression([rationalL, rationalR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the product between the rational number <see cref="rationalL"/> and the
    /// expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression Product(Rational rationalL, RationalExpression expressionR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromRational(rationalL, nameL).Product(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the product between the rational numbers passed as
    /// arguments using the collection <see cref="numbers"/>.
    /// </summary>
    public static RationalExpression RationalProduct(IReadOnlyCollection<Rational> numbers,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalProductExpression(numbers, names, expressionName, settings);

    #endregion Product
    
    #region Division
    
    /// <summary>
    /// Creates a new expression composed of the division between the two rational expressions passed as arguments.
    /// </summary>
    public static RationalExpression Division(RationalExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Division(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the division between the expression and the number (internally
    /// converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression Division(RationalExpression expression, Rational number,
        [CallerArgumentExpression("number")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Division(number, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the division between the two numbers (internally
    /// converted to <see cref="RationalNumberExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression Division(Rational numberL, Rational numberR,
        [CallerArgumentExpression("numberL")] string nameL = "", [CallerArgumentExpression("numberR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalDivisionExpression(FromRational(numberL,nameL),FromRational(numberR, nameR), expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the division between the number <see cref="numberL"/> (internally
    /// converted to <see cref="RationalNumberExpression"/>) and the expression <see cref="expressionR"/> passed as
    /// arguments.
    /// </summary>
    public static RationalExpression Division(Rational numberL, RationalExpression expressionR,
        [CallerArgumentExpression("numberL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalDivisionExpression(FromRational(numberL,nameL), expressionR, expressionName, settings);

    #endregion Division
    
    #region LeastCommonMultiple
    
    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the expression passed as first argument and the
    /// rational number passed as second argument (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression LeastCommonMultiple(RationalExpression expression, Rational number,
        [CallerArgumentExpression("number")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.LeastCommonMultiple(number, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the expressions passed as arguments.
    /// </summary>
    public static RationalExpression LeastCommonMultiple(RationalExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.LeastCommonMultiple(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the two rational numbers passed as arguments
    /// (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression LeastCommonMultiple(Rational rationalL, Rational rationalR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "", [CallerArgumentExpression("rationalR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalLeastCommonMultipleExpression([rationalL, rationalR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the rational number <see cref="rationalL"/> and the
    /// expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression LeastCommonMultiple(Rational rationalL, RationalExpression expressionR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromRational(rationalL, nameL).LeastCommonMultiple(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the l.c.m. between the rational numbers passed as
    /// arguments using the collection <see cref="numbers"/>.
    /// </summary>
    public static RationalExpression LeastCommonMultiple(IReadOnlyCollection<Rational> numbers,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalLeastCommonMultipleExpression(numbers, names, expressionName, settings);

    #endregion LeastCommonMultiple
    
    #region GreatestCommonDivisor
    
    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the expression passed as first argument and the
    /// rational number passed as second argument (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression GreatestCommonDivisor(RationalExpression expression, Rational number,
        [CallerArgumentExpression("number")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.GreatestCommonDivisor(number, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the expressions passed as arguments.
    /// </summary>
    public static RationalExpression GreatestCommonDivisor(RationalExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.GreatestCommonDivisor(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the two rational numbers passed as arguments
    /// (converted to <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression GreatestCommonDivisor(Rational rationalL, Rational rationalR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "", [CallerArgumentExpression("rationalR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalGreatestCommonDivisorExpression([rationalL, rationalR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the rational number <see cref="rationalL"/> and the
    /// expression <see cref="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression GreatestCommonDivisor(Rational rationalL, RationalExpression expressionR,
        [CallerArgumentExpression("rationalL")]
        string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromRational(rationalL, nameL).GreatestCommonDivisor(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the g.c.d. between the rational numbers passed as
    /// arguments using the collection <see cref="numbers"/>.
    /// </summary>
    public static RationalExpression GreatestCommonDivisor(IReadOnlyCollection<Rational> numbers,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new RationalGreatestCommonDivisorExpression(numbers, names, expressionName, settings);

    #endregion GreatestCommonDivisor

    #region RationalNegate

    /// <summary>
    /// Adds the negation operator to the expression passed as argument.
    /// </summary>
    public static RationalExpression Negate(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.Negate(expressionName, settings);

    /// <summary>
    /// Adds the negation operator to the number passed as argument (which is converted to
    /// <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression Negate(Rational number, string expressionName = "",
        ExpressionSettings? settings = null)
        => new NegateRationalExpression(number, expressionName, settings);

    #endregion RationalNegate

    #region Invert

    /// <summary>
    /// Adds the inversion operator to the expression passed as argument.
    /// </summary>
    public static RationalExpression Invert(RationalExpression expression, string expressionName = "",
        ExpressionSettings? settings = null)
        => expression.Invert(expressionName, settings);

    /// <summary>
    /// Adds the inversion operator to the number passed as argument (which is converted to
    /// <see cref="RationalNumberExpression"/>).
    /// </summary>
    public static RationalExpression Invert(Rational number, string expressionName = "",
        ExpressionSettings? settings = null)
        => new InvertRationalExpression(number, expressionName, settings);

    #endregion Invert

    /// <summary>
    /// Creates a placeholder expression for a curve expression.
    /// </summary>
    public static CurveExpression Placeholder(string name, ExpressionSettings? settings = null)
        => new CurvePlaceholderExpression(name, settings);

    /// <summary>
    /// Creates a placeholder expression for a rational expression.
    /// </summary>
    public static RationalExpression RationalPlaceholder(string name, ExpressionSettings? settings = null)
        => new RationalPlaceholderExpression(name, settings);

    /// <summary>
    /// Computes the value of the expression (rational or curve expression) passed as argument.
    /// </summary>
    public static T Compute<T>(IGenericExpression<T> expression)
        => expression.Compute();

    /// <summary>
    /// Verifies if two curve expressions (<see cref="e1"/> and <see cref="e2"/>) are equivalent, i.e., their
    /// values are equivalent.
    /// </summary>
    public static bool Equivalent(CurveExpression e1, CurveExpression e2)
        => e1.Equivalent(e2);
    
    //todo: add method to check equivalent expressions without computing them
}