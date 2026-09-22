using System.Globalization;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// The kind of a single step in the path from the root of an expression to a sub-expression.
/// </summary>
public enum StepKind
{
    /// <summary>
    /// The single operand of a unary expression node.
    /// </summary>
    InnerOperand,

    /// <summary>
    /// The left operand of a binary expression node.
    /// </summary>
    LeftOperand,

    /// <summary>
    /// The right operand of a binary expression node.
    /// </summary>
    RightOperand,

    /// <summary>
    /// One operand of an n-ary expression node, selected by its position.
    /// </summary>
    IndexedOperand
}

/// <summary>
/// A single step in the path from the root of an expression to a sub-expression.
/// </summary>
/// <remarks>
/// A step is a value of a closed set: the indexed case carries a non-negative index and the others carry none.
/// An invalid step therefore cannot be represented.
/// The only way to build a non-default step is through the static factories, and the indexed one rejects a negative index.
/// </remarks>
public readonly record struct PathStep
{
    /// <summary>
    /// The kind of step this is.
    /// </summary>
    public StepKind Kind { get; }

    /// <summary>
    /// The operand index, meaningful only when <see cref="Kind"/> is <see cref="StepKind.IndexedOperand"/>.
    /// </summary>
    public int Index { get; }

    private PathStep(StepKind kind, int index)
    {
        Kind = kind;
        Index = index;
    }

    /// <summary>
    /// The step through the operand of a unary expression node.
    /// </summary>
    public static PathStep InnerOperand { get; } = new(StepKind.InnerOperand, 0);

    /// <summary>
    /// The step through the left operand of a binary expression node.
    /// </summary>
    public static PathStep LeftOperand { get; } = new(StepKind.LeftOperand, 0);

    /// <summary>
    /// The step through the right operand of a binary expression node.
    /// </summary>
    public static PathStep RightOperand { get; } = new(StepKind.RightOperand, 0);

    /// <summary>
    /// The step through operand number <paramref name="index"/> (starting from 0) of an n-ary expression node.
    /// </summary>
    /// <param name="index">The position of the operand, starting from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative.</exception>
    public static PathStep IndexedOperand(int index)
        => index >= 0
            ? new(StepKind.IndexedOperand, index)
            : throw new ArgumentOutOfRangeException(nameof(index), index, "An operand index cannot be negative.");

    /// <summary>
    /// Renders the step to the spelling used by the string form of a position.
    /// </summary>
    public override string ToString() => Kind switch
    {
        StepKind.InnerOperand => Positions.InnerOperand,
        StepKind.LeftOperand => Positions.LeftOperand,
        StepKind.RightOperand => Positions.RightOperand,
        StepKind.IndexedOperand => Index.ToString(CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unknown step kind {Kind}.")
    };

    /// <summary>
    /// Parses the spelling of a step, if it names one: "Operand", "LeftOperand", "RightOperand", or a non-negative decimal index.
    /// </summary>
    /// <param name="step">The spelling of the step.</param>
    /// <param name="result">When this method returns true, the parsed step; otherwise, the default value.</param>
    /// <returns>true if the spelling names a step; otherwise, false.</returns>
    public static bool TryParse(string step, out PathStep result)
    {
        switch (step)
        {
            case Positions.InnerOperand:
                result = InnerOperand;
                return true;
            case Positions.LeftOperand:
                result = LeftOperand;
                return true;
            case Positions.RightOperand:
                result = RightOperand;
                return true;
            default:
                if (int.TryParse(step, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) && index >= 0)
                {
                    result = IndexedOperand(index);
                    return true;
                }

                result = default;
                return false;
        }
    }

    /// <summary>
    /// Parses the spelling of a step: "Operand", "LeftOperand", "RightOperand", or a non-negative decimal index.
    /// </summary>
    /// <param name="step">The spelling of the step.</param>
    /// <exception cref="ArgumentException">If the spelling does not name a step.</exception>
    public static PathStep Parse(string step)
        => TryParse(step, out var result)
            ? result
            : throw new ArgumentException($"Invalid position step \"{step}\".", nameof(step));
}
