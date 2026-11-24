using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.UncheckedInternals;
using Unipi.Nancy.Utility;

namespace Unipi.Nancy.Numerics;

/// <summary>
/// Represents a rational number with infinite precision,
/// using <see cref="BigInteger"/> for both numerator and denominator.
/// </summary>
/// <remarks>
/// Adapted from Microsoft's BigRational, open sourced as MIT on <see href="https://github.com/MicrosoftArchive/bcl/blob/master/Libraries/BigRational/BigRationalLibrary/BigRational.cs">GitHub</see>.
/// </remarks>
[Serializable]
[ComVisible(false)]
[JsonObject(MemberSerialization.OptIn)]
[Newtonsoft.Json.JsonConverter(typeof(BigRationalNewtonsoftJsonConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(BigRationalSystemJsonConverter))]
public struct BigRational : IComparable, IComparable<BigRational>, IEquatable<BigRational>, IToCodeString, IStableHashCode, IToMppgString
{
    #region Static public values

    /// <summary>
    /// A value representing the number 0.
    /// </summary>
    public static BigRational Zero { get; } = new BigRational(BigInteger.Zero);

    /// <summary>
    /// A value representing the number 1.
    /// </summary>
    public static BigRational One { get; } = new BigRational(BigInteger.One);

    /// <summary>
    /// A value representing the number -1.
    /// </summary>
    public static BigRational MinusOne { get; } = new BigRational(BigInteger.MinusOne);

    /// <summary>
    /// A value representing the number $+\infty$.
    /// </summary>
    public static BigRational PlusInfinity { get; } = new BigRational
    {
        Numerator = 1,
        Denominator = 0
    };

    /// <summary>
    /// A value representing the number $-\infty$.
    /// </summary>
    public static BigRational MinusInfinity { get; } = new BigRational
    {
        Numerator = -1,
        Denominator = 0
    };

    #endregion Static public values

    #region Public Properties

    /// <summary>
    /// Returns an integer that indicates the sign of the rational
    /// </summary>
    /// <returns>
    /// A number that indicates the sign of the rational, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    ///     <term>Number</term>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <term>-1</term>
    ///     <description>The value of this object is negative.</description>
    /// </item>
    /// <item>
    ///     <term>0</term>
    ///     <description>The value of this object is 0 (zero).</description>
    /// </item>
    /// <item>
    ///     <term>1</term>
    ///     <description>The value of this object is positive.</description>
    /// </item>
    /// </list>
    /// </returns>
    public readonly Int32 Sign => Numerator.Sign;

    /// <summary>
    /// The numerator of the rational.
    /// </summary>
    /// <remarks>
    /// This property encodes infinity as well as finite values: check <see cref="IsPlusInfinite"/>/<see cref="IsMinusInfinite"/> before using it, or read the pair through <see cref="TryGetFinite"/>, which forces the check.
    /// </remarks>
    [JsonProperty(PropertyName = "num")]
    public BigInteger Numerator { get; private set; }

    /// <summary>
    /// The denominator of the rational.
    /// </summary>
    /// <remarks>
    /// This property encodes infinity as well as finite values: check <see cref="IsPlusInfinite"/>/<see cref="IsMinusInfinite"/> before using it, or read the pair through <see cref="TryGetFinite"/>, which forces the check.
    /// </remarks>
    [JsonProperty(PropertyName = "den")]
    public BigInteger Denominator { get; private set; }

    /// <summary>
    /// True if the number is finite.
    /// </summary>
    public readonly bool IsFinite => !Denominator.IsZero;

    /// <summary>
    /// True if the number is infinite.
    /// </summary>
    public readonly bool IsInfinite => Denominator.IsZero;

    /// <summary>
    /// True if the number is $+\infty$.
    /// </summary>
    public readonly bool IsPlusInfinite => Denominator.IsZero && Numerator.IsOne;

    /// <summary>
    /// True if the number is $-\infty$.
    /// </summary>
    public readonly bool IsMinusInfinite => Denominator.IsZero && Numerator == BigInteger.MinusOne;

    /// <summary>
    /// The numerator and denominator, if this value is finite.
    /// </summary>
    /// <returns>
    /// <see langword="false"/> for either infinity, with both <paramref name="numerator"/> and <paramref name="denominator"/> left at their default; <see langword="true"/> otherwise.
    /// </returns>
    public readonly bool TryGetFinite(out BigInteger numerator, out BigInteger denominator)
    {
        if (IsInfinite)
        {
            numerator = default;
            denominator = default;
            return false;
        }
        numerator = Numerator;
        denominator = Denominator;
        return true;
    }

    /// <summary>
    /// True if the number is 0.
    /// </summary>
    public readonly bool IsZero => Numerator.IsZero;

    /// <summary>
    /// True if the number is 1.
    /// </summary>
    public readonly bool IsOne => Denominator.IsOne && Numerator.IsOne;

    /// <summary>
    /// True if the number is $> 0$.
    /// </summary>
    public readonly bool IsPositive => this.Sign > 0;

    /// <summary>
    /// True if the number is $&lt; 0$.
    /// </summary>
    public readonly bool IsNegative => this.Sign < 0;

    /// <summary>
    /// True if the number is an integer.
    /// </summary>
    public readonly bool IsInteger => Denominator.IsOne;

    #endregion Public Properties

    #region Static members for Internal Support
    [StructLayout(LayoutKind.Explicit)]
    internal struct DoubleUlong
    {
        [FieldOffset(0)]
        public double dbl;
        [FieldOffset(0)]
        public ulong uu;
    }
    private const int DoubleMaxScale = 308;
    private static readonly BigInteger s_bnDoublePrecision = BigInteger.Pow(10, DoubleMaxScale);
    private static readonly BigInteger s_bnDoubleMaxValue = (BigInteger)Double.MaxValue;
    private static readonly BigInteger s_bnDoubleMinValue = (BigInteger)Double.MinValue;

    [StructLayout(LayoutKind.Explicit)]
    internal struct DecimalUInt32
    {
        [FieldOffset(0)]
        public Decimal dec;
        [FieldOffset(0)]
        public int flags;
    }
    private const int DecimalScaleMask = 0x00FF0000;
    private const int DecimalSignMask = unchecked((int)0x80000000);
    private const int DecimalMaxScale = 28;
    private static readonly BigInteger s_bnDecimalPrecision = BigInteger.Pow(10, DecimalMaxScale);
    private static readonly BigInteger s_bnDecimalMaxValue = (BigInteger)Decimal.MaxValue;
    private static readonly BigInteger s_bnDecimalMinValue = (BigInteger)Decimal.MinValue;

    private const String c_solidus = @"/";
    #endregion Static members for Internal Support

    #region Public Instance Methods

    // GetWholePart() and GetFractionPart()
    //
    // BigRational == Whole, Fraction
    //  0/2        ==     0,  0/2
    //  1/2        ==     0,  1/2
    // -1/2        ==     0, -1/2
    //  1/1        ==     1,  0/1
    // -1/1        ==    -1,  0/1
    // -3/2        ==    -1, -1/2
    //  3/2        ==     1,  1/2
    /// <summary>
    /// Returns the whole part of the rational number, rounded toward zero.
    /// </summary>
    /// <exception cref="UndeterminedResultException">The number is infinite.</exception>
    public readonly BigInteger GetWholePart()
    {
        if (IsInfinite)
            throw new UndeterminedResultException("Cannot get whole part of an infinite value");
        return BigInteger.Divide(Numerator, Denominator);
    }

    /// <summary>
    /// Returns the fraction part of the rational number: the number minus its whole part, which is zero or has the number's sign.
    /// </summary>
    /// <exception cref="UndeterminedResultException">The number is infinite.</exception>
    public readonly BigRational GetFractionPart()
    {
        if (IsInfinite)
            throw new UndeterminedResultException("Cannot get fraction part of an infinite value");
        // The remainder has the numerator's sign, and gcd(n mod d, d) = gcd(n, d) = 1, so only a zero remainder is not canonical as it is.
        var remainder = BigInteger.Remainder(Numerator, Denominator);
        if (remainder.IsZero)
            return BigRational.Zero;
        else
            return new BigRational(remainder, Denominator, skipNormalize: true);
    }

    /// <summary>
    /// Returns the largest integer less than or equal to the current rational.
    /// </summary>
    /// <exception cref="UndeterminedResultException">The number is infinite.</exception>
    public readonly BigInteger Floor()
    {
        if (IsInfinite)
            throw new UndeterminedResultException("Cannot get the floor of an infinite value");
        var quotient = BigInteger.DivRem(Numerator, Denominator, out var remainder);
        if (remainder.Sign < 0)
            return quotient - 1;
        else
            return quotient;
    }

    /// <summary>
    /// Returns the smallest integer greater than or equal to the current rational.
    /// </summary>
    /// <exception cref="UndeterminedResultException">The number is infinite.</exception>
    public readonly BigInteger Ceil()
    {
        if (IsInfinite)
            throw new UndeterminedResultException("Cannot get the ceiling of an infinite value");
        var quotient = BigInteger.DivRem(Numerator, Denominator, out var remainder);
        if (remainder.Sign > 0)
            return quotient + 1;
        else
            return quotient;
    }

    /// <inheritdoc />
    public override readonly bool Equals(object? obj)
    {
        return obj is BigRational other && Equals(other);
    }

    /// <inheritdoc />
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }

    /// <summary>
    /// A stable hashcode.
    /// </summary>
    public readonly int GetStableHashCode()
    {
        return (Numerator, Denominator).GetStableHashCode();
    }

    // IComparable
    /// <inheritdoc cref="CompareTo(BigRational)" path="/remarks"/>
    readonly int IComparable.CompareTo(object? obj)
    {
        if (obj == null)
            return 1;
        if (obj is not BigRational)
            throw new ArgumentException("Argument must be of type BigRational", nameof(obj));
        return Compare(this, (BigRational)obj);
    }

    // IComparable<BigRational>
    /// <inheritdoc />
    /// <remarks>
    /// Stronger than the interface requires: the result is exactly $-1$, $0$ or $1$, never another value of the same sign.
    /// Callers may therefore use it as a sign, rather than only testing it against zero.
    /// </remarks>
    public readonly int CompareTo(BigRational other)
    {
        return Compare(this, other);
    }

    // Object.ToString
    /// <inheritdoc />
    public override readonly String ToString()
    {
        StringBuilder ret = new StringBuilder();
        ret.Append(Numerator.ToString("R", CultureInfo.InvariantCulture));
        if (Denominator != 1)
        {
            ret.Append(c_solidus);
            ret.Append(Denominator.ToString("R", CultureInfo.InvariantCulture));
        }

        return ret.ToString();
    }

    /// <summary>
    /// Returns a string containing C# code to create this BigRational.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public readonly string ToCodeString(bool formatted = false, int indentation = 0)
    {
        if (Denominator == 1)
            return Numerator.ToString();

        var sb = new StringBuilder();
        sb.Append("new BigRational(");
        sb.Append(Numerator.ToString());
        sb.Append(", ");
        sb.Append(Denominator.ToString());
        sb.Append(")");

        return sb.ToString();
    }

    /// <summary>
    /// Return a string containing code to create an equivalent of this BigRational in a (min,+) playground.
    /// </summary>
    public readonly string ToMppgString()
    {
        if (IsPlusInfinite)
            return "+inf";
        else if (IsMinusInfinite)
            return "-inf";
        else if (IsInteger)
            return Numerator.ToString();
        else
            return $"{Numerator.ToString()}/{Denominator.ToString()}";
    }

    // IEquatable<BigRational>
    /// <inheritdoc />
    public readonly Boolean Equals(BigRational other)
    {
        // values are normalized, so equal values have the same numerator and denominator
        return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    #endregion Public Instance Methods

    #region Constructors

    /// <summary>
    /// Constructs the rational <paramref name="numerator"/>/<paramref name="denominator"/>, reduced and with a positive denominator.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <remarks>A zero denominator gives the infinity of the numerator's sign.</remarks>
    /// <exception cref="UndeterminedResultException">Both the numerator and the denominator are zero.</exception>
    public BigRational(int numerator, int denominator = 1)
    {
        this = new BigRational((Int64)numerator, denominator);
    }

    /// <summary>
    /// Constructs the rational <paramref name="numerator"/>/<paramref name="denominator"/>, reduced and with a positive denominator.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <remarks>A zero denominator gives the infinity of the numerator's sign.</remarks>
    /// <exception cref="UndeterminedResultException">Both the numerator and the denominator are zero.</exception>
    public BigRational(long numerator, long denominator = 1)
    {
        if (denominator == 0)
        {
            if (numerator == 0)
                throw new UndeterminedResultException("Zero over zero");
            else
            {
                Numerator = numerator < 0 ? BigInteger.MinusOne : BigInteger.One;
                Denominator = BigInteger.Zero;
            }
        }
        else if (numerator == 0)
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        else if (denominator == 1)
        {
            Numerator = new BigInteger(numerator);
            Denominator = BigInteger.One;
        }
        // Int64.MinValue has no Int64 opposite, so these parts are sign-normalized in BigInteger.
        else if (numerator == Int64.MinValue || denominator == Int64.MinValue)
            this = new BigRational(new BigInteger(numerator), new BigInteger(denominator));
        else
        {
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
            var divisor = GreatestCommonDivisor(numerator, denominator);
            Numerator = new BigInteger(numerator / divisor);
            Denominator = new BigInteger(denominator / divisor);
        }
    }

    /// <summary>
    /// Constructs a rational from Int64 parts, optionally skipping normalization.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <param name="skipNormalize">
    /// True when the numerator and denominator are already canonical: coprime, with a positive denominator.
    /// Otherwise they are normalized as by the public constructor.
    /// </param>
    internal BigRational(long numerator, long denominator, bool skipNormalize)
    {
        if (skipNormalize)
        {
            Numerator = new BigInteger(numerator);
            Denominator = new BigInteger(denominator);
        }
        else
        {
            this = new BigRational(numerator, denominator);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    public BigRational(BigInteger numerator)
    {
        Numerator = numerator;
        Denominator = BigInteger.One;
    }

    /// <summary>
    /// Constructs a rational from the shortest round-trippable decimal representation of the given double.
    /// </summary>
    public BigRational(Double value)
    {
        (Numerator, Denominator) = value.GetRationalParts();
        Simplify();
    }

    /// <summary>
    /// Constructs a rational from the shortest round-trippable decimal representation of the given float.
    /// </summary>
    public BigRational(Single value)
    {
        (Numerator, Denominator) = value.GetRationalParts();
        Simplify();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// The Decimal type represents floating point numbers exactly, with no rounding error.
    /// Values such as <c>0.1</c> in Decimal are actually representable, and convert cleanly to BigRational as <c>1/10</c>.
    /// </remarks>
    public BigRational(Decimal value)
    {
        if (value == Decimal.Zero)
        {
            this = BigRational.Zero;
            return;
        }

        (Numerator, Denominator) = value.GetRationalParts();
        Simplify();
    }

    /// <summary>
    /// Constructs the rational <paramref name="numerator"/>/<paramref name="denominator"/>, reduced and with a positive denominator.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <remarks>A zero denominator gives the infinity of the numerator's sign.</remarks>
    /// <exception cref="UndeterminedResultException">Both the numerator and the denominator are zero.</exception>
    public BigRational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.Sign == 0)
        {
            if (numerator < 0)
                Numerator = BigInteger.MinusOne;
            else if (numerator > 0)
                Numerator = BigInteger.One;
            else
                throw new UndeterminedResultException("Zero over zero");

            Denominator = BigInteger.Zero;
        }
        else if (numerator.Sign == 0)
        {
            // 0/m -> 0/1
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        else if (denominator.Sign < 0)
        {
            Numerator = -numerator;
            Denominator = -denominator;
        }
        else
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        Simplify();
    }

    /// <summary>
    /// Constructs a rational from BigInteger parts, optionally skipping normalization.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <param name="skipNormalize">
    /// True when the numerator and denominator are already canonical: coprime, with a positive denominator.
    /// Otherwise they are normalized as by the public constructor.
    /// </param>
    internal BigRational(BigInteger numerator, BigInteger denominator, bool skipNormalize)
    {
        if (skipNormalize)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
        else
        {
            if (denominator.Sign == 0)
            {
                if (numerator < 0)
                    Numerator = BigInteger.MinusOne;
                else if (numerator > 0)
                    Numerator = BigInteger.One;
                else
                    throw new UndeterminedResultException("Zero over zero");

                Denominator = BigInteger.Zero;
            }
            else if (numerator.Sign == 0)
            {
                // 0/m -> 0/1
                Numerator = BigInteger.Zero;
                Denominator = BigInteger.One;
            }
            else if (denominator.Sign < 0)
            {
                Numerator = -numerator;
                Denominator = -denominator;
            }
            else
            {
                Numerator = numerator;
                Denominator = denominator;
            }
            Simplify();
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="whole">The whole part.</param>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <exception cref="DivideByZeroException">The denominator is zero.</exception>
    public BigRational(BigInteger whole, BigInteger numerator, BigInteger denominator)
    {
        if (denominator.Sign == 0)
        {
            throw new DivideByZeroException();
        }
        else if (numerator.Sign == 0 && whole.Sign == 0)
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        else if (denominator.Sign < 0)
        {
            Denominator = -denominator;
            Numerator = (-whole * Denominator) - numerator;
        }
        else
        {
            Denominator = denominator;
            Numerator = (whole * denominator) + numerator;
        }
        Simplify();
    }
    #endregion Constructors

    #region Public Static Methods

    /// <summary>
    /// The absolute value of the number.
    /// </summary>
    public static BigRational Abs(BigRational r)
    {
        return (r.Numerator.Sign < 0 ? new BigRational(BigInteger.Abs(r.Numerator), r.Denominator, skipNormalize: true) : r);
    }

    /// <summary>
    /// The opposite of the number.
    /// </summary>
    public static BigRational Negate(BigRational r)
    {
        return new BigRational(-r.Numerator, r.Denominator, skipNormalize: !r.Denominator.IsZero);
    }

    /// <summary>
    /// The inverse of the number.
    /// </summary>
    public static BigRational Invert(BigRational r)
    {
        var sign = r.Numerator.Sign;
        if (sign > 0)
            return new BigRational(r.Denominator, r.Numerator, skipNormalize: true);
        else if (sign < 0)
            return new BigRational(-r.Denominator, -r.Numerator, skipNormalize: true);
        else
            return new BigRational(r.Denominator, r.Numerator);
    }

    /// <summary>
    /// The sum of the two numbers.
    /// </summary>
    public static BigRational Add(BigRational x, BigRational y)
    {
        return x + y;
    }

    /// <summary>
    /// The difference of the two numbers.
    /// </summary>
    public static BigRational Subtract(BigRational x, BigRational y)
    {
        return x - y;
    }

    /// <summary>
    /// The product of the two numbers.
    /// </summary>
    /// <remarks>
    /// Common factors across the two fractions are cancelled before the products are taken.
    /// </remarks>
    public static BigRational Multiply(BigRational x, BigRational y)
    {
        return x * y;
    }

    /// <summary>
    /// The division of the two numbers.
    /// </summary>
    /// <remarks>
    /// Common factors across the dividend and the reciprocal of the divisor are cancelled before the products are taken.
    /// </remarks>
    public static BigRational Divide(BigRational dividend, BigRational divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// The remainder of the two numbers.
    /// </summary>
    public static BigRational Remainder(BigRational dividend, BigRational divisor)
    {
        return dividend % divisor;
    }

    /// <summary>
    /// Performs a division with remainder.
    /// </summary>
    /// <param name="dividend">The dividend.</param>
    /// <param name="divisor">The divisor.</param>
    /// <param name="remainder">The remainder, dividend − quotient · divisor, which is zero or has the dividend's sign.</param>
    /// <returns>The quotient rounded toward zero, as an integer rational.</returns>
    /// <exception cref="UndeterminedResultException">Either operand is infinite.</exception>
    /// <exception cref="DivideByZeroException">The divisor is zero.</exception>
    public static BigRational DivRem(BigRational dividend, BigRational divisor, out BigRational remainder)
    {
        if (dividend.IsInfinite || divisor.IsInfinite)
            throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");
        if (divisor == BigRational.Zero)
            throw new DivideByZeroException("Divisor must be non-zero.");

        // a/b / c/d  == (ad)/(bc)
        // a/b % c/d  == (ad % bc)/bd

        // (ad) and (bc) need to be calculated for both the division and the remainder operations.
        var ad = dividend.Numerator * divisor.Denominator;
        var bc = dividend.Denominator * divisor.Numerator;
        var bd = dividend.Denominator * divisor.Denominator;

        remainder = new BigRational(ad % bc, bd);
        return new BigRational(ad / bc);
    }

    /// <summary>
    /// Computes the power <paramref name="baseValue"/>^<paramref name="exponent"/>
    /// </summary>
    /// <param name="baseValue">The base value.</param>
    /// <param name="exponent">The exponent.</param>
    /// <exception cref="ArgumentException">Thrown when the operation cannot be completed.</exception>
    public static BigRational Pow(BigRational baseValue, BigInteger exponent)
    {
        if (exponent.Sign == 0)
        {
            // 0^0 -> 1
            // n^0 -> 1
            return BigRational.One;
        }
        else if (exponent.Sign < 0)
        {
            if (baseValue == BigRational.Zero)
            {
                throw new ArgumentException("cannot raise zero to a negative power", nameof(baseValue));
            }
            // n^(-e) -> (1/n)^e
            baseValue = BigRational.Invert(baseValue);
            exponent = -exponent;
        }

        BigRational result = baseValue;
        while (exponent > BigInteger.One)
        {
            result = result * baseValue;
            exponent--;
        }

        return result;
    }

    /// <summary>
    /// The LCD is the least common multiple of the two denominators.
    /// For instance, the LCD of $\frac{1}{2}, \frac{1}{4}$ is 4 because the least common multiple of 2 and 4 is 4.
    /// Likewise, the LCD of $\frac{1}{2}, \frac{1}{3}$ is 6.
    /// </summary>
    /// <remarks>
    /// To find the LCD:
    /// <list type="number">
    ///     <item> Find the Greatest Common Divisor (GCD) of the denominators </item>
    ///     <item> Multiply the denominators together </item>
    ///     <item> Divide the product of the denominators by the GCD </item>
    /// </list>
    /// </remarks>
    public static BigInteger LeastCommonDenominator(BigRational x, BigRational y)
    {
        // LCD( a/b, c/d ) == (bd) / gcd(b,d)
        return (x.Denominator / BigInteger.GreatestCommonDivisor(x.Denominator, y.Denominator)) * y.Denominator;
    }

    /// <summary>
    /// The greatest common divisor of the two numbers: the largest rational that divides both into integers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>
    /// The greatest common divisor, which is positive, or zero when both operands are.
    /// When <paramref name="b"/> is zero it is the absolute value of <paramref name="a"/>, infinities included.
    /// </returns>
    /// <exception cref="UndeterminedResultException"><paramref name="b"/> is nonzero and either operand is infinite.</exception>
    public static BigRational GreatestCommonDivisor(BigRational a, BigRational b)
    {
        if (b.IsZero)
            return Abs(a);
        else if (a.IsInfinite || b.IsInfinite)
            throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");
        else
        {
            // For reduced p/q and r/s, the largest rational that divides both into integers is gcd(p, r) / lcm(q, s), which is reduced.
            var numerator = BigInteger.GreatestCommonDivisor(a.Numerator, b.Numerator);
            var denominator = a.Denominator / BigInteger.GreatestCommonDivisor(a.Denominator, b.Denominator) * b.Denominator;
            return new BigRational(numerator, denominator, skipNormalize: true);
        }
    }

    /// <summary>
    /// The greatest common divisor of the two Int64 values.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>The greatest common divisor, which is positive, or zero when both operands are.</returns>
    /// <exception cref="OverflowException">The greatest common divisor is 2^63: one operand is Int64.MinValue, and the other is zero or Int64.MinValue.</exception>
    public static long GreatestCommonDivisor(long a, long b)
    {
        if (a == Int64.MinValue || b == Int64.MinValue)
        {
            // |Int64.MinValue| is 2^63, so the gcd is the largest power of 2 that divides the other value, or 2^63 itself, which an Int64 cannot hold.
            Int64 other = a == Int64.MinValue ? b : a;
            if (other == 0 || other == Int64.MinValue)
                throw new OverflowException("The greatest common divisor is 2^63, which an Int64 cannot hold.");
            else
                return 1L << BitOperations.TrailingZeroCount(other);
        }
        else if (a == 0)
            return Math.Abs(b);
        else if (b == 0)
            return Math.Abs(a);
        else
            return GreatestCommonDivisorPositive(Math.Abs(a), Math.Abs(b));
    }

    /// <summary>
    /// The least common multiple of the two numbers: the smallest positive rational that both divide into integers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>The least common multiple, which is positive, or zero when exactly one operand is.</returns>
    /// <exception cref="UndeterminedResultException">Either operand is infinite, or both are zero.</exception>
    public static BigRational LeastCommonMultiple(BigRational a, BigRational b)
    {
        if (a.IsInfinite || b.IsInfinite)
            throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");
        else if (a.IsZero && b.IsZero)
            throw new UndeterminedResultException("Zero over zero");
        else if (a.IsZero || b.IsZero)
            return BigRational.Zero;
        else
        {
            // For reduced p/q and r/s, the smallest rational that both divide into integers is lcm(p, r) / gcd(q, s), which is reduced.
            var numerator = BigInteger.Abs(a.Numerator) / BigInteger.GreatestCommonDivisor(a.Numerator, b.Numerator) * BigInteger.Abs(b.Numerator);
            var denominator = BigInteger.GreatestCommonDivisor(a.Denominator, b.Denominator);
            return new BigRational(numerator, denominator, skipNormalize: true);
        }
    }

    /// <summary>
    /// Compares two values and returns an integer that indicates
    /// whether the first value is less than, equal to, or greater than the second value.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>,
    /// as shown in the following table.
    /// <list type="table">
    /// <listheader>
    ///     <term>Value</term>
    ///     <description>Condition</description>
    /// </listheader>
    /// <item>
    ///     <term>Less than zero</term>
    ///     <description><paramref name="left"/> is less than <paramref name="right"/>.</description>
    /// </item>
    /// <item>
    ///     <term>Zero</term>
    ///     <description><paramref name="left"/> equals <paramref name="right"/>.</description>
    /// </item>
    /// <item>
    ///     <term>Greater than zero</term>
    ///     <description><paramref name="left"/> is greater than <paramref name="right"/>.</description>
    /// </item>
    /// </list>
    /// The result is exactly $-1$, $0$ or $1$.
    /// </returns>
    public static int Compare(BigRational left, BigRational right)
    {
        if (left.IsInfinite)
        {
            if (right.IsInfinite)
                return Math.Sign(left.Sign.CompareTo(right.Sign));
            else
                return left.Sign;
        }
        else if (right.IsInfinite)
            return -right.Sign;
        // Otherwise both operands are finite, and continue below.

        // Components held in BigInteger's sign field are Int32 values, and their cross-products compare in Int64.
        var leftNumerator = left.Numerator;
        var leftDenominator = left.Denominator;
        var rightNumerator = right.Numerator;
        var rightDenominator = right.Denominator;
        if (BitsField(ref leftNumerator) is null && BitsField(ref leftDenominator) is null &&
            BitsField(ref rightNumerator) is null && BitsField(ref rightDenominator) is null)
        {
            Int32 a = SignField(ref leftNumerator), b = SignField(ref leftDenominator);
            Int32 c = SignField(ref rightNumerator), d = SignField(ref rightDenominator);
            if (b == d)
                return a < c ? -1 : a > c ? 1 : 0;
            else
            {
                Int64 leftProduct = (Int64)a * d, rightProduct = (Int64)c * b;
                return leftProduct < rightProduct ? -1 : leftProduct > rightProduct ? 1 : 0;
            }
        }
        else
            return CompareLargeFractions(leftNumerator, leftDenominator, rightNumerator, rightDenominator);
    }

    /// <summary>
    /// Max of the two numbers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    public static BigRational Max(BigRational a, BigRational b) => a > b ? a : b;

    /// <summary>
    /// Max of the three numbers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <param name="c">The third operand.</param>
    public static BigRational Max(BigRational a, BigRational b, BigRational c) => Max(a, Max(b, c));

    /// <summary>
    /// Max of a set of numbers.
    /// </summary>
    public static BigRational Max(params BigRational[] values) => values.Aggregate(Max);

    /// <summary>
    /// Max of a set of numbers.
    /// </summary>
    public static BigRational Max(IEnumerable<BigRational> values) => values.Aggregate(Max);

    /// <summary>
    /// Min of the two numbers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    public static BigRational Min(BigRational a, BigRational b) => a > b ? b : a;

    /// <summary>
    /// Min of the three numbers.
    /// </summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <param name="c">The third operand.</param>
    public static BigRational Min(BigRational a, BigRational b, BigRational c) => Min(a, Min(b, c));

    /// <summary>
    /// Min of a set of numbers.
    /// </summary>
    public static BigRational Min(params BigRational[] values) => values.Aggregate(Min);

    /// <summary>
    /// Min of a set of numbers.
    /// </summary>
    public static BigRational Min(IEnumerable<BigRational> values) => values.Aggregate(Min);

    #endregion Public Static Methods

    #region Operator Overloads
    /// <inheritdoc />
    public static bool operator ==(BigRational x, BigRational y)
    {
        // Values are canonical, infinities included, so equal values have the same numerator and denominator.
        return x.Numerator == y.Numerator && x.Denominator == y.Denominator;
    }

    /// <inheritdoc />
    public static bool operator !=(BigRational x, BigRational y)
    {
        // Values are canonical, infinities included, so different values differ in the numerator or the denominator.
        return x.Numerator != y.Numerator || x.Denominator != y.Denominator;
    }

    /// <inheritdoc />
    public static bool operator <(BigRational x, BigRational y)
    {
        return Compare(x, y) < 0;
    }

    /// <inheritdoc />
    public static bool operator <=(BigRational x, BigRational y)
    {
        return Compare(x, y) <= 0;
    }

    /// <inheritdoc />
    public static bool operator >(BigRational x, BigRational y)
    {
        return Compare(x, y) > 0;
    }

    /// <inheritdoc />
    public static bool operator >=(BigRational x, BigRational y)
    {
        return Compare(x, y) >= 0;
    }

    /// <inheritdoc />
    public static BigRational operator +(BigRational r)
    {
        return r;
    }

    /// <inheritdoc cref="Negate(BigRational)"/>
    public static BigRational operator -(BigRational r)
    {
        return Negate(r);
    }

    /// <inheritdoc />
    public static BigRational operator ++(BigRational r)
    {
        if (r.IsInfinite)
            return r;

        return r + BigRational.One;
    }

    /// <inheritdoc />
    public static BigRational operator --(BigRational r)
    {
        if (r.IsInfinite)
            return r;

        return r - BigRational.One;
    }

    /// <inheritdoc cref="Add(BigRational, BigRational)"/>
    public static BigRational operator +(BigRational x, BigRational y)
    {
        if (x.IsInfinite || y.IsInfinite)
        {
            if (x.IsInfinite && y.IsInfinite)
            {
                if (x.Sign == y.Sign)
                    return x;
                else
                    throw new UndeterminedResultException("Cannot sum opposite infinities");
            }
            else
            {
                return x.IsInfinite ? x : y;
            }
        }
        else if (x.IsZero)
            return y;
        else if (y.IsZero)
            return x;
        else
        {
            // Components held in BigInteger's sign field are Int32 values, and add in Int64.
            var xn = x.Numerator;
            var xd = x.Denominator;
            var yn = y.Numerator;
            var yd = y.Denominator;
            if (BitsField(ref xn) is null && BitsField(ref xd) is null &&
                BitsField(ref yn) is null && BitsField(ref yd) is null)
                return AddInt32Fractions(SignField(ref xn), SignField(ref xd), SignField(ref yn), SignField(ref yd));
            else
                return AddLargeFractions(xn, xd, yn, yd);
        }
    }

    /// <inheritdoc cref="Subtract(BigRational, BigRational)"/>
    public static BigRational operator -(BigRational x, BigRational y)
    {
        return x + (-y);
    }

    /// <inheritdoc cref="Multiply(BigRational, BigRational)"/>
    public static BigRational operator *(BigRational x, BigRational y)
    {
        if (x.IsZero || y.IsZero)
            return BigRational.Zero;
        else if (x.IsInfinite || y.IsInfinite)
            return x.Sign == y.Sign ? BigRational.PlusInfinity : BigRational.MinusInfinity;
        // Otherwise both operands are finite and nonzero, and continue below.

        // Each operand's units are tested once: an integer ±1 yields the other operand, and two integers multiply.
        if (x.Denominator.IsOne)
        {
            if (y.Denominator.IsOne)
            {
                var xNumerator = x.Numerator;
                var yNumerator = y.Numerator;
                Int32 xField = SignField(ref xNumerator), yField = SignField(ref yNumerator);
                // A sign field other than ±1 holds the value itself, an Int32 other than ±1, and two of them multiply in an Int64.
                if (xField != 1 && xField != -1 && yField != 1 && yField != -1)
                    return new BigRational((Int64)xField * yField, 1, skipNormalize: true);
                // Otherwise a numerator is ±1 or outside the sign field, and continues with the tests for ±1.
            }
            if (x.Numerator.IsOne)
                return y;
            else if (x.Numerator == BigInteger.MinusOne)
                return -y;
            // Otherwise x is an integer other than ±1, and continues with the tests on y.
        }
        if (y.Denominator.IsOne)
        {
            if (y.Numerator.IsOne)
                return x;
            else if (y.Numerator == BigInteger.MinusOne)
                return -x;
            else if (x.Denominator.IsOne)
                return new BigRational(x.Numerator * y.Numerator);
            // Otherwise x is a fraction and y an integer other than ±1, and they continue with the fraction tiers.
        }

        // Fractions go to NoInlining helpers, so that zero and integer products do not pay for their frames.
        // Components held in BigInteger's sign field are Int32 values, and their products are taken in Int64.
        var xn = x.Numerator;
        var xd = x.Denominator;
        var yn = y.Numerator;
        var yd = y.Denominator;
        if (BitsField(ref xn) is null && BitsField(ref xd) is null &&
            BitsField(ref yn) is null && BitsField(ref yd) is null)
            return MultiplyInt32Fractions(SignField(ref xn), SignField(ref xd), SignField(ref yn), SignField(ref yd));
        else
            return MultiplyLargeFractions(xn, xd, yn, yd);
    }

    /// <inheritdoc cref="Divide(BigRational, BigRational)"/>
    public static BigRational operator /(BigRational x, BigRational y)
    {
        if (x.IsZero)
        {
            if (y.IsZero)
                throw new UndeterminedResultException("Zero over zero");
            else
                return BigRational.Zero;
        }
        else if (y.IsZero)
            throw new DivideByZeroException();
        else if (x.IsInfinite)
        {
            if (y.IsInfinite)
                throw new UndeterminedResultException("Infinity over infinity");
            else
                return y.Numerator.Sign > 0 ? x : -x;
        }
        else if (y.IsInfinite)
            return BigRational.Zero;
        // Otherwise both operands are finite and nonzero, and x/y is x times the reciprocal of y = c/d, ±d/|c|, which is reduced.
        // The reciprocal is an integer when c is ±1, so each operand's units are tested once as in the multiplication: a dividend ±1 yields ±1/y, a divisor ±1 yields ±x, and an integer divided by ±1/d is the integer ±x·d.
        if (x.Denominator.IsOne)
        {
            if (y.Numerator.IsOne || y.Numerator == BigInteger.MinusOne)
            {
                var xNumerator = x.Numerator;
                var yDenominator = y.Denominator;
                Int32 xField = SignField(ref xNumerator), dField = SignField(ref yDenominator);
                // A sign field other than ±1 holds the value itself, an Int32 other than ±1, and two of them multiply in an Int64.
                if (xField != 1 && xField != -1 && dField != 1 && dField != -1)
                    return new BigRational(y.Numerator.Sign * (Int64)xField * dField, 1, skipNormalize: true);
                // Otherwise a value is ±1 or outside the sign field, and continues with the tests for ±1.
            }
            if (x.Numerator.IsOne)
                return Invert(y);
            else if (x.Numerator == BigInteger.MinusOne)
                return -Invert(y);
            // Otherwise x is an integer other than ±1, and continues with the tests on y.
        }
        if (y.Numerator.IsOne || y.Numerator == BigInteger.MinusOne)
        {
            if (y.Denominator.IsOne)
                return y.Numerator.IsOne ? x : -x;
            else if (x.Denominator.IsOne)
                return new BigRational(y.Numerator.IsOne ? x.Numerator * y.Denominator : -(x.Numerator * y.Denominator));
            // Otherwise x is a fraction and the reciprocal of y an integer other than ±1, and they continue with the fraction tiers.
        }

        // Fractions go to the multiplication's NoInlining helpers, which cancel x against the reciprocal ±d/|c|.
        // Components held in BigInteger's sign field are Int32 values, and their products are taken in Int64.
        var xn = x.Numerator;
        var xd = x.Denominator;
        var yn = y.Numerator;
        var yd = y.Denominator;
        if (BitsField(ref xn) is null && BitsField(ref xd) is null &&
            BitsField(ref yn) is null && BitsField(ref yd) is null)
        {
            Int32 c = SignField(ref yn), d = SignField(ref yd);
            return MultiplyInt32Fractions(SignField(ref xn), SignField(ref xd), c < 0 ? -d : d, Math.Abs(c));
        }
        else
            return MultiplyLargeFractions(xn, xd, yn.Sign < 0 ? -yd : yd, BigInteger.Abs(yn));
    }

    /// <inheritdoc cref="Remainder(BigRational, BigRational)"/>
    public static BigRational operator %(BigRational x, BigRational y)
    {
        if (x.IsInfinite || y.IsInfinite)
            throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");

        // a/b % c/d  == (ad % bc)/bd
        return new BigRational((x.Numerator * y.Denominator) % (x.Denominator * y.Numerator), (x.Denominator * y.Denominator));
    }
    #endregion Operator Overloads

    #region Arithmetic helpers

    // The greatest common divisor of two positive Int64 values, by the binary algorithm; the arguments are not checked.
    private static Int64 GreatestCommonDivisorPositive(Int64 left, Int64 right)
    {
        UInt64 a = (UInt64)left, b = (UInt64)right;
        int shift = BitOperations.TrailingZeroCount(a | b);
        a >>= BitOperations.TrailingZeroCount(a);
        do
        {
            b >>= BitOperations.TrailingZeroCount(b);
            if (a > b)
                (a, b) = (b, a);
            b -= a;
        } while (b != 0);
        return (Int64)(a << shift);
    }

    // BigInteger's _sign field: the value itself when _bits is null, and its sign otherwise.
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sign")]
    private static extern ref Int32 SignField(ref BigInteger value);

    // BigInteger's _bits field: null when the value is held in _sign, and otherwise its magnitude in 32-bit words, least significant first.
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_bits")]
    private static extern ref UInt32[]? BitsField(ref BigInteger value);

    // Whether the value is an Int64 other than Int64.MinValue, and that value, read from BigInteger's _sign and _bits fields.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetInt64(BigInteger value, out Int64 result)
    {
        UInt32[]? bits = BitsField(ref value);
        Int32 signField = SignField(ref value);
        if (bits is null)
        {
            result = signField;
            return true;
        }
        else if (bits.Length == 1)
        {
            result = signField * (Int64)bits[0];
            return true;
        }
        else if (bits.Length == 2 && bits[1] <= Int32.MaxValue)
        {
            result = signField * (Int64)(((UInt64)bits[1] << 32) | bits[0]);
            return true;
        }
        else
        {
            result = 0;
            return false;
        }
    }

    // The exact 128-bit product of two Int64 values.
    private static Int128 Product(Int64 x, Int64 y)
    {
        Int64 high = Math.BigMul(x, y, out Int64 low);
        return new Int128(unchecked((UInt64)high), unchecked((UInt64)low));
    }

    // The rational with the given reduced numerator and positive denominator, built from Int64 values when both fit.
    private static BigRational FromInt128(Int128 numerator, Int128 denominator)
    {
        if (numerator >= Int64.MinValue && numerator <= Int64.MaxValue && denominator <= Int64.MaxValue)
            return new BigRational((Int64)numerator, (Int64)denominator, skipNormalize: true);
        else
            return new BigRational((BigInteger)numerator, (BigInteger)denominator, skipNormalize: true);
    }

    // Adds the reduced fractions a/b and c/d into a reduced fraction, with the sums and products taken in Int64.
    // The components are Int32 values other than Int32.MinValue, and the denominators are positive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BigRational AddInt32Fractions(Int32 a, Int32 b, Int32 c, Int32 d)
    {
        // Equal denominators add the numerators, and only factors of b can cancel.
        if (b == d)
        {
            Int64 numerator = (Int64)a + c;
            if (numerator == 0)
                return BigRational.Zero;
            else if (b == 1)
                return new BigRational(numerator, 1, skipNormalize: true);
            else
            {
                Int64 divisor = GreatestCommonDivisorPositive(Math.Abs(numerator), b);
                return new BigRational(numerator / divisor, b / divisor, skipNormalize: true);
            }
        }
        // An integer plus a reduced fraction is reduced: a + c/d = (a·d + c)/d, and c is coprime to d.
        else if (b == 1)
            return new BigRational((Int64)a * d + c, d, skipNormalize: true);
        else if (d == 1)
            return new BigRational(a + (Int64)c * b, b, skipNormalize: true);
        // Otherwise, with g = gcd(b, d), a/b + c/d = (a·(d/g) + c·(b/g)) / (b·d/g), and only factors of g can cancel.
        // The sum is nonzero: reduced fractions with unequal denominators are not opposite.
        else
        {
            Int64 g = GreatestCommonDivisorPositive(b, d);
            if (g == 1)
                return new BigRational((Int64)a * d + (Int64)c * b, (Int64)b * d, skipNormalize: true);
            else
            {
                Int64 bReduced = b / g, dReduced = d / g;
                Int64 numerator = a * dReduced + c * bReduced;
                Int64 divisor = GreatestCommonDivisorPositive(Math.Abs(numerator), g);
                return new BigRational(numerator / divisor, bReduced * (d / divisor), skipNormalize: true);
            }
        }
    }

    // Adds the reduced fractions a/b and c/d into a reduced fraction, with 128-bit sums and products and Int64 gcds.
    // The components are Int64 values other than Int64.MinValue, and the denominators are positive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BigRational AddInt64Fractions(Int64 a, Int64 b, Int64 c, Int64 d)
    {
        // Equal denominators add the numerators, and only factors of b can cancel.
        if (b == d)
        {
            Int128 numerator = (Int128)a + c;
            if (numerator == 0)
                return BigRational.Zero;
            else if (b == 1)
                return FromInt128(numerator, 1);
            else
            {
                // gcd(|numerator|, b) = gcd(|numerator| mod b, b), and the remainder, below b, takes the gcd in Int64.
                Int64 remainder = (Int64)((UInt128)Int128.Abs(numerator) % (UInt64)b);
                Int64 divisor = remainder == 0 ? b : GreatestCommonDivisorPositive(remainder, b);
                return FromInt128(numerator / divisor, b / divisor);
            }
        }
        // An integer plus a reduced fraction is reduced: a + c/d = (a·d + c)/d, and c is coprime to d.
        else if (b == 1)
            return FromInt128(Product(a, d) + c, d);
        else if (d == 1)
            return FromInt128(a + Product(c, b), b);
        // Otherwise, with g = gcd(b, d), a/b + c/d = (a·(d/g) + c·(b/g)) / (b·d/g), and only factors of g can cancel.
        // The sum is nonzero: reduced fractions with unequal denominators are not opposite.
        else
        {
            Int64 g = GreatestCommonDivisorPositive(b, d);
            if (g == 1)
                return FromInt128(Product(a, d) + Product(c, b), Product(b, d));
            else
            {
                Int64 bReduced = b / g, dReduced = d / g;
                Int128 numerator = Product(a, dReduced) + Product(c, bReduced);
                // As above, the gcd with g is taken in Int64 from |numerator| mod g.
                Int64 remainder = (Int64)((UInt128)Int128.Abs(numerator) % (UInt64)g);
                Int64 divisor = remainder == 0 ? g : GreatestCommonDivisorPositive(remainder, g);
                return FromInt128(numerator / divisor, Product(bReduced, d / divisor));
            }
        }
    }

    // Adds the reduced nonzero fractions a/b and c/d, with positive denominators, into a reduced fraction:
    // in 128-bit arithmetic when every component is an Int64 other than Int64.MinValue, otherwise in BigInteger.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BigRational AddLargeFractions(BigInteger a, BigInteger b, BigInteger c, BigInteger d)
    {
        if (TryGetInt64(a, out Int64 a64) && TryGetInt64(b, out Int64 b64) &&
            TryGetInt64(c, out Int64 c64) && TryGetInt64(d, out Int64 d64))
            return AddInt64Fractions(a64, b64, c64, d64);
        else if (b == d)
            return new BigRational(a + c, b);
        else
        {
            // With g = gcd(b, d), a/b + c/d = (a·(d/g) + c·(b/g)) / (b·d/g), and only factors of g can cancel.
            var g = BigInteger.GreatestCommonDivisor(b, d);
            if (g.IsOne)
                return new BigRational(a * d + c * b, b * d, skipNormalize: true);
            else
            {
                var bReduced = b / g;
                var numerator = a * (d / g) + c * bReduced;
                var divisor = BigInteger.GreatestCommonDivisor(numerator, g);
                // The denominator b·d/g is bReduced·d, and dividing d rather than the product by the divisor keeps one multiplication.
                if (divisor.IsOne)
                    return new BigRational(numerator, bReduced * d, skipNormalize: true);
                else
                    return new BigRational(numerator / divisor, bReduced * (d / divisor), skipNormalize: true);
            }
        }
    }

    // Compares the fractions a/b and c/d, with positive denominators:
    // by 128-bit cross-products when every component is an Int64 other than Int64.MinValue, otherwise by bit lengths and then BigInteger cross-products.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int CompareLargeFractions(BigInteger a, BigInteger b, BigInteger c, BigInteger d)
    {
        if (TryGetInt64(a, out Int64 a64) && TryGetInt64(b, out Int64 b64) &&
            TryGetInt64(c, out Int64 c64) && TryGetInt64(d, out Int64 d64))
        {
            int leftSign = Math.Sign(a64), rightSign = Math.Sign(c64);
            if (b64 == d64)
                return a64 < c64 ? -1 : a64 > c64 ? 1 : 0;
            else if (leftSign != rightSign)
                return leftSign < rightSign ? -1 : 1;
            else if (leftSign == 0)
                return 0;
            else
            {
                // Equal signs order by the magnitudes |a|·d and |c|·b, compared as unsigned 128-bit products.
                UInt64 leftHigh = Math.BigMul(unchecked((UInt64)Math.Abs(a64)), unchecked((UInt64)d64), out UInt64 leftLow);
                UInt64 rightHigh = Math.BigMul(unchecked((UInt64)Math.Abs(c64)), unchecked((UInt64)b64), out UInt64 rightLow);
                int magnitude;
                if (leftHigh != rightHigh)
                    magnitude = leftHigh < rightHigh ? -1 : 1;
                else
                    magnitude = leftLow < rightLow ? -1 : leftLow > rightLow ? 1 : 0;
                return leftSign > 0 ? magnitude : -magnitude;
            }
        }
        else
        {
            int leftSign = a.Sign, rightSign = c.Sign;
            if (leftSign != rightSign)
                return leftSign < rightSign ? -1 : 1;
            else if (leftSign == 0)
                return 0;
            else if (b == d)
                return Math.Sign(a.CompareTo(c));
            else
            {
                var leftNumLength = BigInteger.Abs(a).GetBitLength();
                var rightNumLength = BigInteger.Abs(c).GetBitLength();
                var leftDenLength = b.GetBitLength();
                var rightDenLength = d.GetBitLength();

                // For magnitude index $m$, $\log_2 |x| \in ]m - 1, m + 1[$.
                // A separation of at least two orders the magnitudes without multiplication.
                var leftMagnitude = leftNumLength - leftDenLength;
                var rightMagnitude = rightNumLength - rightDenLength;
                if (leftMagnitude >= rightMagnitude + 2)
                    return leftSign;
                else if (rightMagnitude >= leftMagnitude + 2)
                    return -leftSign;
                else
                    // GCDs and divisions to shrink the products cost more than they save.
                    return Math.Sign(BigInteger.Compare(a * d, c * b));
            }
        }
    }

    // The three fraction tiers below compute (a/b) * (c/d) as ((a/g1) * (c/g2)) / ((b/g2) * (d/g1)), with g1 = gcd(a, d) and g2 = gcd(c, b).
    // Reduced operands with equal denominators or equal numerator magnitudes have both gcds equal to 1, so each tier skips the cancellation for them.

    // Multiplies the reduced fractions a/b and c/d into a reduced fraction, with the products taken in Int64.
    // The components are Int32 values other than Int32.MinValue, and the denominators are positive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BigRational MultiplyInt32Fractions(Int32 a, Int32 b, Int32 c, Int32 d)
    {
        if (b != d && a != c && a != -c)
        {
            CancelPair(ref a, ref d);
            CancelPair(ref c, ref b);
        }
        return new BigRational((Int64)a * c, (Int64)b * d, skipNormalize: true);
    }

    // Multiplies the reduced fractions a/b and c/d into a reduced fraction, with the products taken in 128 bits and built from Int64 values when they fit.
    // The components are Int64 values other than Int64.MinValue, and the denominators are positive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BigRational MultiplyInt64Fractions(Int64 a, Int64 b, Int64 c, Int64 d)
    {
        if (b != d && a != c && a != -c)
        {
            CancelPair(ref a, ref d);
            CancelPair(ref c, ref b);
        }
        Int64 numeratorHigh = Math.BigMul(a, c, out Int64 numerator);
        Int64 denominatorHigh = Math.BigMul(b, d, out Int64 denominator);
        // The products fit an Int64 when their high halves only extend the sign.
        if (numeratorHigh == numerator >> 63 && denominatorHigh == 0 && denominator >= 0)
            return new BigRational(numerator, denominator, skipNormalize: true);
        else
            return new BigRational((BigInteger)new Int128(unchecked((UInt64)numeratorHigh), unchecked((UInt64)numerator)),
                (BigInteger)new Int128(unchecked((UInt64)denominatorHigh), unchecked((UInt64)denominator)), skipNormalize: true);
    }

    // Multiplies the reduced fractions a/b and c/d into a reduced fraction: ±1 over a denominator times an integer through ProductFromOnePair, components that are Int64 values other than Int64.MinValue through MultiplyInt64Fractions, the rest by cancelling in BigInteger.
    // At least one component is outside the sign field, and the denominators are positive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BigRational MultiplyLargeFractions(BigInteger a, BigInteger b, BigInteger c, BigInteger d)
    {
        // ±1 over a denominator times an integer: one gcd, and on a miss both operands are reused.
        if (d.IsOne && (a.IsOne || a == BigInteger.MinusOne))
            return ProductFromOnePair(a.IsOne ? c : -c, b);
        else if (b.IsOne && (c.IsOne || c == BigInteger.MinusOne))
            return ProductFromOnePair(c.IsOne ? a : -a, d);
        else if (TryGetInt64(a, out Int64 a64) && TryGetInt64(b, out Int64 b64) &&
            TryGetInt64(c, out Int64 c64) && TryGetInt64(d, out Int64 d64))
            return MultiplyInt64Fractions(a64, b64, c64, d64);
        else if (b == d || BigInteger.Abs(a) == BigInteger.Abs(c))
            return new BigRational(a * c, b * d, skipNormalize: true);
        else
        {
            // The pairs are written out here: a BigInteger CancelPair measured slower on small-by-wide operands.
            if (!d.IsOne && !a.IsOne && a != BigInteger.MinusOne)
            {
                // An equal cross-pair cancels to ±1 and 1.
                if (BigInteger.Abs(a) == d)
                {
                    a = a.Sign;
                    d = BigInteger.One;
                }
                else
                {
                    var g1 = BigInteger.GreatestCommonDivisor(a, d);
                    if (!g1.IsOne)
                    {
                        a /= g1;
                        d /= g1;
                    }
                }
            }
            if (!b.IsOne && !c.IsOne && c != BigInteger.MinusOne)
            {
                // An equal cross-pair cancels to ±1 and 1.
                if (BigInteger.Abs(c) == b)
                {
                    c = c.Sign;
                    b = BigInteger.One;
                }
                else
                {
                    var g2 = BigInteger.GreatestCommonDivisor(c, b);
                    if (!g2.IsOne)
                    {
                        c /= g2;
                        b /= g2;
                    }
                }
            }
            // A cross-pair cancelled to 1 leaves a factor to reuse rather than multiply.
            return new BigRational(a.IsOne ? c : c.IsOne ? a : a * c, b.IsOne ? d : d.IsOne ? b : b * d, skipNormalize: true);
        }
    }

    // The product ±c/b of ±1/b and an integer c, with the sign already applied to c.
    // Below 2^63 the gcd is taken in Int64 and a miss reuses both operands; beyond, the normalizing constructor cancels.
    private static BigRational ProductFromOnePair(BigInteger numerator, BigInteger denominator)
    {
        if (TryGetInt64(numerator, out Int64 n) && TryGetInt64(denominator, out Int64 d))
        {
            Int64 originalDenominator = d;
            CancelPair(ref n, ref d);
            if (d == originalDenominator)
                return new BigRational(numerator, denominator, skipNormalize: true);
            else
                return new BigRational(n, d, skipNormalize: true);
        }
        else
            return new BigRational(numerator, denominator);
    }

    // Each CancelPair divides a numerator and the denominator of the other operand by their gcd.
    // Both are nonzero and the denominator is positive; a unit has nothing to cancel, and an equal pair cancels to ±1 and 1 without dividing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CancelPair(ref Int32 numerator, ref Int32 denominator)
    {
        if (denominator != 1 && numerator != 1 && numerator != -1)
        {
            if (Math.Abs(numerator) == denominator)
            {
                numerator = Math.Sign(numerator);
                denominator = 1;
            }
            else
            {
                // The numerator is not Int32.MinValue, so its magnitude fits.
                UInt32 g = (UInt32)Math.Abs(numerator), remainder = (UInt32)denominator;
                while (remainder != 0)
                    (g, remainder) = (remainder, g % remainder);
                if (g != 1)
                {
                    numerator /= (Int32)g;
                    denominator /= (Int32)g;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CancelPair(ref Int64 numerator, ref Int64 denominator)
    {
        if (denominator != 1 && numerator != 1 && numerator != -1)
        {
            if (Math.Abs(numerator) == denominator)
            {
                numerator = Math.Sign(numerator);
                denominator = 1;
            }
            else
            {
                Int64 g = GreatestCommonDivisorPositive(Math.Abs(numerator), denominator);
                if (g != 1)
                {
                    numerator /= g;
                    denominator /= g;
                }
            }
        }
    }

    #endregion Arithmetic helpers

    #region explicit conversions from BigRational
    /// <summary>
    /// Truncates the rational to a <see cref="SByte"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator SByte(BigRational value)
    {
        return (SByte)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="UInt16"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator UInt16(BigRational value)
    {
        return (UInt16)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="UInt32"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator UInt32(BigRational value)
    {
        return (UInt32)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="UInt64"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator UInt64(BigRational value)
    {
        return (UInt64)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="Byte"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator Byte(BigRational value)
    {
        return (Byte)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="Int16"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator Int16(BigRational value)
    {
        return (Int16)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="Int32"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator Int32(BigRational value)
    {
        return (Int32)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="Int64"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator Int64(BigRational value)
    {
        return (Int64)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// Truncates the rational to a <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator BigInteger(BigRational value)
    {
        return BigInteger.Divide(value.Numerator, value.Denominator);
    }

    /// <summary>
    /// Converts the rational to a <see cref="Single"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator Single(BigRational value)
    {
        // The Single value type represents a single-precision 32-bit number with
        // values ranging from negative 3.402823e38 to positive 3.402823e38
        // values that do not fit into this range are returned as Infinity
        return (Single)((Double)value);
    }

    /// <summary>
    /// Converts the rational to a <see cref="Double"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    public static explicit operator Double(BigRational value)
    {
        // The Double value type represents a double-precision 64-bit number with
        // values ranging from -1.79769313486232e308 to +1.79769313486232e308
        // values that do not fit into this range are returned as +/-Infinity
        if (SafeCastToDouble(value.Numerator) && SafeCastToDouble(value.Denominator))
        {
            return (Double)value.Numerator / (Double)value.Denominator;
        }

        // scale the numerator to preseve the fraction part through the integer division
        BigInteger denormalized = (value.Numerator * s_bnDoublePrecision) / value.Denominator;
        if (denormalized.IsZero)
            return (value.Sign < 0) ? BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000)) : 0d; // underflow to -+0

        Double result = 0;
        bool isDouble = false;
        int scale = DoubleMaxScale;

        while (scale > 0)
        {
            if (!isDouble)
            {
                if (SafeCastToDouble(denormalized))
                {
                    result = (Double)denormalized;
                    isDouble = true;
                }
                else
                {
                    denormalized = denormalized / 10;
                }
            }
            result = result / 10;
            scale--;
        }

        if (!isDouble)
            return (value.Sign < 0) ? Double.NegativeInfinity : Double.PositiveInfinity;
        else
            return result;
    }

    /// <summary>
    /// Converts the rational to a <see cref="Decimal"/>.
    /// </summary>
    /// <param name="value">The rational to convert.</param>
    /// <exception cref="InvalidConversionException">Thrown when converting infinity to decimal.</exception>
    /// <exception cref="OverflowException">Thrown when the value is too large or too small for a decimal.</exception>
    public static explicit operator Decimal(BigRational value)
    {
        if (value.IsInfinite)
        {
            if (value.IsPositive)
                throw new InvalidConversionException("Attempt to convert +infinity to decimal");
            else
                throw new InvalidConversionException("Attempt to convert -infinity to decimal");
        }

        // The Decimal value type represents decimal numbers ranging
        // from +79,228,162,514,264,337,593,543,950,335 to -79,228,162,514,264,337,593,543,950,335
        // the binary representation of a Decimal value is of the form, ((-2^96 to 2^96) / 10^(0 to 28))
        if (SafeCastToDecimal(value.Numerator) && SafeCastToDecimal(value.Denominator))
        {
            return (Decimal)value.Numerator / (Decimal)value.Denominator;
        }

        // scale the numerator to preseve the fraction part through the integer division
        BigInteger denormalized = (value.Numerator * s_bnDecimalPrecision) / value.Denominator;
        if (denormalized.IsZero)
        {
            return Decimal.Zero; // underflow - fraction is too small to fit in a decimal
        }
        for (int scale = DecimalMaxScale; scale >= 0; scale--)
        {
            if (!SafeCastToDecimal(denormalized))
            {
                denormalized = denormalized / 10;
            }
            else
            {
                DecimalUInt32 dec = new DecimalUInt32();
                dec.dec = (Decimal)denormalized;
                dec.flags = (dec.flags & ~DecimalScaleMask) | (scale << 16);
                return dec.dec;
            }
        }
        throw new OverflowException("Value was either too large or too small for a Decimal.");
    }
    #endregion explicit conversions from BigRational

    #region implicit conversions to BigRational

    /// <summary>
    /// Converts a <see cref="SByte"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(SByte value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="UInt16"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(UInt16 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="UInt32"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(UInt32 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="UInt64"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(UInt64 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="Byte"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(Byte value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="Int16"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(Int16 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="Int32"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(Int32 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="Int64"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(Int64 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// Converts a <see cref="BigInteger"/> to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator BigRational(BigInteger value)
    {
        return new BigRational(value);
    }

    /// <summary>
    /// Converts a single-precision floating-point number to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The floating-point value to convert.</param>
    public static implicit operator BigRational(Single value)
    {
        return new BigRational((Double)value);
    }

    /// <summary>
    /// Converts a double-precision floating-point number to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The floating-point value to convert.</param>
    public static implicit operator BigRational(Double value)
    {
        return new BigRational(value);
    }

    /// <summary>
    /// Converts a decimal number to a <see cref="BigRational"/>.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    public static implicit operator BigRational(Decimal value)
    {
        return new BigRational(value);
    }

    #endregion implicit conversions to BigRational

    #region instance helper methods

    /// <summary>
    /// Simplifies the rational, making it irreducible.
    /// This should be called during construction, existing values are assumed to be irreducible.
    /// </summary>
    private void Simplify()
    {
        // * if the numerator is {0, +1, -1} then the fraction is already reduced
        // * if the denominator is {+1} then the fraction is already reduced
        if (Numerator.IsZero)
            Denominator = BigInteger.One;
        else if (Numerator.IsOne || Numerator == BigInteger.MinusOne)
            return;
        else if (Denominator.IsOne)
            return;
        // early exit for identical parts
        else if (BigInteger.Abs(Numerator) == Denominator)
        {
            Numerator = Numerator.Sign;
            Denominator = BigInteger.One;
        }
        else
        {
            var gcd = BigInteger.GreatestCommonDivisor(Numerator, Denominator);
            if (gcd > BigInteger.One)
            {
                Numerator /= gcd;
                Denominator /= gcd;
            }
        }
    }

    #endregion instance helper methods

    #region static helper methods
    private static bool SafeCastToDouble(BigInteger value)
    {
        return s_bnDoubleMinValue <= value && value <= s_bnDoubleMaxValue;
    }

    private static bool SafeCastToDecimal(BigInteger value)
    {
        return s_bnDecimalMinValue <= value && value <= s_bnDecimalMaxValue;
    }

    private static void SplitDoubleIntoParts(double dbl, out int sign, out int exp, out ulong man, out bool isFinite)
    {
        DoubleUlong du;
        du.uu = 0;
        du.dbl = dbl;

        sign = 1 - ((int)(du.uu >> 62) & 2);
        man = du.uu & 0x000FFFFFFFFFFFFF;
        exp = (int)(du.uu >> 52) & 0x7FF;
        if (exp == 0)
        {
            // Denormalized number.
            isFinite = true;
            if (man != 0)
                exp = -1074;
        }
        else if (exp == 0x7FF)
        {
            // NaN or Infinite.
            isFinite = false;
            exp = Int32.MaxValue;
        }
        else
        {
            isFinite = true;
            man |= 0x0010000000000000; // mask in the implied leading 53rd significand bit
            exp -= 1075;
        }
    }

    private static double GetDoubleFromParts(int sign, int exp, ulong man)
    {
        DoubleUlong du;
        du.dbl = 0;

        if (man == 0)
        {
            du.uu = 0;
        }
        else
        {
            // Normalize so that 0x0010 0000 0000 0000 is the highest bit set
            int cbitShift = CbitHighZero(man) - 11;
            if (cbitShift < 0)
                man >>= -cbitShift;
            else
                man <<= cbitShift;

            // Move the point to just behind the leading 1: 0x001.0 0000 0000 0000
            // (52 bits) and skew the exponent (by 0x3FF == 1023)
            exp += 1075;

            if (exp >= 0x7FF)
            {
                // Infinity
                du.uu = 0x7FF0000000000000;
            }
            else if (exp <= 0)
            {
                // Denormalized
                exp--;
                if (exp < -52)
                {
                    // Underflow to zero
                    du.uu = 0;
                }
                else
                {
                    du.uu = man >> -exp;
                }
            }
            else
            {
                // Mask off the implicit high bit
                du.uu = (man & 0x000FFFFFFFFFFFFF) | ((ulong)exp << 52);
            }
        }

        if (sign < 0)
        {
            du.uu |= 0x8000000000000000;
        }

        return du.dbl;
    }

    private static int CbitHighZero(ulong uu)
    {
        if ((uu & 0xFFFFFFFF00000000) == 0)
            return 32 + CbitHighZero((uint)uu);
        return CbitHighZero((uint)(uu >> 32));
    }

    private static int CbitHighZero(uint u)
    {
        if (u == 0)
            return 32;

        int cbit = 0;
        if ((u & 0xFFFF0000) == 0)
        {
            cbit += 16;
            u <<= 16;
        }
        if ((u & 0xFF000000) == 0)
        {
            cbit += 8;
            u <<= 8;
        }
        if ((u & 0xF0000000) == 0)
        {
            cbit += 4;
            u <<= 4;
        }
        if ((u & 0xC0000000) == 0)
        {
            cbit += 2;
            u <<= 2;
        }
        if ((u & 0x80000000) == 0)
            cbit += 1;
        return cbit;
    }

    #endregion static helper methods
}
