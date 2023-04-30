using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;

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
public struct BigRational : IComparable, IComparable<BigRational>, IEquatable<BigRational>, IToCodeString
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

    #endregion Static values

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
    public Int32 Sign => Numerator.Sign;

    /// <summary>
    /// The numerator of the rational.
    /// </summary>
    [JsonProperty(PropertyName = "num")]
    public BigInteger Numerator { get; private set; }

    /// <summary>
    /// The denominator of the rational.
    /// </summary>
    [JsonProperty(PropertyName = "den")]
    public BigInteger Denominator { get; private set; }

    /// <summary>
    /// True of the number is finite.
    /// </summary>
    public bool IsFinite => Denominator != 0;

    /// <summary>
    /// True of the number is infinite.
    /// </summary>
    public bool IsInfinite => Denominator == 0;

    /// <summary>
    /// True of the number is $+\infty$.
    /// </summary>
    public bool IsPlusInfinite => Denominator == 0 && Numerator == 1;

    /// <summary>
    /// True of the number is $-\infty$.
    /// </summary>
    public bool IsMinusInfinite => Denominator == 0 && Numerator == -1;

    /// <summary>
    /// True of the number is 0.
    /// </summary>
    public bool IsZero => this == Zero;

    /// <summary>
    /// True of the number is $> 0$.
    /// </summary>
    public bool IsPositive => this.Sign > 0;

    /// <summary>
    /// True of the number is $&lt; 0$.
    /// </summary>
    public bool IsNegative => this.Sign < 0;

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
    /// 
    /// </summary>
    /// <returns></returns>
    public BigInteger GetWholePart()
    {
        return BigInteger.Divide(Numerator, Denominator);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public BigRational GetFractionPart()
    {
        return new BigRational(BigInteger.Remainder(Numerator, Denominator), Denominator);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (!(obj is BigRational))
            return false;
        return this.Equals((BigRational)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (Numerator / Denominator).GetHashCode();
    }

    // IComparable
    int IComparable.CompareTo(object? obj)
    {
        if (obj == null)
            return 1;
        if (!(obj is BigRational))
            throw new ArgumentException("Argument must be of type BigRational", "obj");
        return Compare(this, (BigRational)obj);
    }

    // IComparable<BigRational>
    /// <inheritdoc />
    public int CompareTo(BigRational other)
    {
        return Compare(this, other);
    }

    // Object.ToString
    /// <inheritdoc />
    public override String ToString()
    {
        StringBuilder ret = new StringBuilder();
        ret.Append(Numerator.ToString("R", CultureInfo.InvariantCulture));
        ret.Append(c_solidus);
        ret.Append(Denominator.ToString("R", CultureInfo.InvariantCulture));
        return ret.ToString();
    }

    /// <summary>
    /// Returns a string containing C# code to create this BigRational.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public string ToCodeString(bool formatted = false, int indentation = 0)
    {
        if (Denominator == 1)
            return Numerator.ToString();

        var sb = new StringBuilder();
        sb.Append("new Rational(");
        sb.Append(Numerator.ToString());
        sb.Append(", ");
        sb.Append(Denominator.ToString());
        sb.Append(")");

        return sb.ToString();
    }

    // IEquatable<BigRational>
    // a/b = c/d, iff ad = bc
    /// <inheritdoc />
    public Boolean Equals(BigRational other)
    {
        if (this.Denominator == other.Denominator)
        {
            return Numerator == other.Numerator;
        }
        else
        {
            return (Numerator * other.Denominator) == (Denominator * other.Numerator);
        }
    }

    #endregion Public Instance Methods

    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="numerator"></param>
    /// <param name="denominator"></param>
    /// <exception cref="UndeterminedResultException"></exception>
    public BigRational(int numerator, int denominator = 1)
    {
        if (denominator == 0)
        {
            if (numerator < 0)
                Numerator = BigInteger.MinusOne;
            else if (numerator > 0)
                Numerator = BigInteger.One;
            else
                throw new UndeterminedResultException("Zero over zero");

            Denominator = BigInteger.Zero;
        }
        else if (numerator == 0)
        {
            // 0/m -> 0/1
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        else if (denominator < 0)
        {
            Numerator = BigInteger.Negate(numerator);
            Denominator = BigInteger.Negate(denominator);
        }
        else
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        Simplify();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="numerator"></param>
    public BigRational(BigInteger numerator)
    {
        Numerator = numerator;
        Denominator = BigInteger.One;
    }

    /// <summary>
    /// This constructor is unreliable, as proved by the related test, and should not be used in its current state.
    /// </summary>
    internal BigRational(Double value)
    {
        if (Double.IsNaN(value))
        {
            throw new ArgumentException("Argument is not a number", "value");
        }
        else if (Double.IsInfinity(value))
        {
            throw new ArgumentException("Argument is infinity", "value");
        }

        bool isFinite;
        int sign;
        int exponent;
        ulong significand;
        SplitDoubleIntoParts(value, out sign, out exponent, out significand, out isFinite);

        if (significand == 0)
        {
            this = BigRational.Zero;
            return;
        }

        Numerator = significand;
        Denominator = 1 << 52;

        if (exponent > 0)
        {
            Numerator = BigInteger.Pow(Numerator, exponent);
        }
        else if (exponent < 0)
        {
            Denominator = BigInteger.Pow(Denominator, -exponent);
        }
        if (sign < 0)
        {
            Numerator = BigInteger.Negate(Numerator);
        }
        Simplify();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// The Decimal type represents floating point numbers exactly, with no rounding error.
    /// Values such as <code>0.1</code> in Decimal are actually representable, and convert cleanly to BigRational as <code>1/10</code>.
    /// </remarks>
    public BigRational(Decimal value)
    {
        int[] bits = Decimal.GetBits(value);
        if (bits == null || bits.Length != 4 || (bits[3] & ~(DecimalSignMask | DecimalScaleMask)) != 0 || (bits[3] & DecimalScaleMask) > (28 << 16))
        {
            throw new ArgumentException("invalid Decimal", "value");
        }

        if (value == Decimal.Zero)
        {
            this = BigRational.Zero;
            return;
        }

        // build up the numerator
        ulong ul = (((ulong)(uint)bits[2]) << 32) | ((ulong)(uint)bits[1]);   // (hi    << 32) | (mid)
        Numerator = (new BigInteger(ul) << 32) | (uint)bits[0];             // (hiMid << 32) | (low)

        bool isNegative = (bits[3] & DecimalSignMask) != 0;
        if (isNegative)
        {
            Numerator = BigInteger.Negate(Numerator);
        }

        // build up the denominator
        int scale = (bits[3] & DecimalScaleMask) >> 16;     // 0-28, power of 10 to divide numerator by
        Denominator = BigInteger.Pow(10, scale);

        Simplify();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="numerator"></param>
    /// <param name="denominator"></param>
    /// <exception cref="UndeterminedResultException"></exception>
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
            Numerator = BigInteger.Negate(numerator);
            Denominator = BigInteger.Negate(denominator);
        }
        else
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        Simplify();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="whole"></param>
    /// <param name="numerator"></param>
    /// <param name="denominator"></param>
    /// <exception cref="DivideByZeroException"></exception>
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
            Denominator = BigInteger.Negate(denominator);
            Numerator = (BigInteger.Negate(whole) * Denominator) + BigInteger.Negate(numerator);
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
        return (r.Numerator.Sign < 0 ? new BigRational(BigInteger.Abs(r.Numerator), r.Denominator) : r);
    }

    /// <summary>
    /// The opposite of the number.
    /// </summary>
    public static BigRational Negate(BigRational r)
    {
        return new BigRational(BigInteger.Negate(r.Numerator), r.Denominator);
    }

    /// <summary>
    /// The inverse of the number.
    /// </summary>
    public static BigRational Invert(BigRational r)
    {
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
    public static BigRational Multiply(BigRational x, BigRational y)
    {
        return x * y;
    }

    /// <summary>
    /// The division of the two numbers.
    /// </summary>
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
    /// Performs a division with reminder.
    /// </summary>
    /// <param name="dividend"></param>
    /// <param name="divisor"></param>
    /// <param name="remainder">The reminder resulting from the division.</param>
    /// <returns>The integer result of the division.</returns>
    public static BigRational DivRem(BigRational dividend, BigRational divisor, out BigRational remainder)
    {
        // a/b / c/d  == (ad)/(bc)
        // a/b % c/d  == (ad % bc)/bd

        // (ad) and (bc) need to be calculated for both the division and the remainder operations.
        BigInteger ad = dividend.Numerator * divisor.Denominator;
        BigInteger bc = dividend.Denominator * divisor.Numerator;
        BigInteger bd = dividend.Denominator * divisor.Denominator;

        remainder = new BigRational(ad % bc, bd);
        return new BigRational(ad, bc);
    }

    /// <summary>
    /// Computes the power <paramref name="baseValue"/>^<paramref name="exponent"/>
    /// </summary>
    /// <param name="baseValue"></param>
    /// <param name="exponent"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
                throw new ArgumentException("cannot raise zero to a negative power", "baseValue");
            }
            // n^(-e) -> (1/n)^e
            baseValue = BigRational.Invert(baseValue);
            exponent = BigInteger.Negate(exponent);
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
    /// Greatest Common Divisor of the two numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static BigRational GreatestCommonDivisor(BigRational a, BigRational b)
    {
        while (b != 0)
        {
            BigRational temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    /// <summary>
    /// Least Common Multiple of the two numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static BigRational LeastCommonMultiple(BigRational a, BigRational b)
    {
        return (a / BigRational.GreatestCommonDivisor(a, b)) * b;
    }

    /// <summary>
    /// Compares two values and returns an integer that indicates
    /// whether the first value is less than, equal to, or greater than the second value.
    /// </summary>
    /// <param name="r1">The first value to compare.</param>
    /// <param name="r2">The second value to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="r1"/> and <paramref name="r2"/>, 
    /// as shown in the following table.
    /// <list type="table">
    /// <listheader>
    ///     <term>Value</term>
    ///     <description>Condition</description>
    /// </listheader>
    /// <item>
    ///     <term>Less than zero</term>
    ///     <description><paramref name="r1"/> is less than <paramref name="r2"/>.</description>
    /// </item>
    /// <item>
    ///     <term>Zero</term>
    ///     <description><paramref name="r1"/> equals <paramref name="r2"/>.</description>
    /// </item>
    /// <item>
    ///     <term>Greater than zero</term>
    ///     <description><paramref name="r1"/> is greater than <paramref name="r2"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static int Compare(BigRational r1, BigRational r2)
    {
        if (r1.IsInfinite || r2.IsInfinite)
        {
            if (r1.IsInfinite && r2.IsInfinite)
                return intCompare(r1.Sign, r2.Sign);
            else
            {
                //An infinite value is always bigger in absolute value
                if (r1.IsInfinite)
                {
                    return intCompare(2*r1.Sign, r2.Sign);
                }
                else
                {
                    return intCompare(r1.Sign, 2*r2.Sign);
                }
            }
        }
        else
        {
            // a/b = c/d, iff ad = bc
            return BigInteger.Compare(r1.Numerator * r2.Denominator, r2.Numerator * r1.Denominator);
        }

        int intCompare(int left, int right)
        {
            return left - right;
        }
    }

    /// <summary>
    /// Max of the two numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static BigRational Max(BigRational a, BigRational b) => a > b ? a : b;

    /// <summary>
    /// Max of the three numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public static BigRational Max(BigRational a, BigRational b, BigRational c) => Max(a, Max(b, c));    //good enough

    /// <summary>
    /// Min of the two numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static BigRational Min(BigRational a, BigRational b) => a > b ? b : a;

    /// <summary>
    /// Min of the three numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public static BigRational Min(BigRational a, BigRational b, BigRational c) => Min(a, Min(b, c));

    #endregion Public Static Methods

    #region Operator Overloads
    /// <inheritdoc />
    public static bool operator ==(BigRational x, BigRational y)
    {
        // Shortcut common comparisons to infinite
        if (y.IsPlusInfinite)
            return x.IsPlusInfinite;
        if (y.IsMinusInfinite)
            return x.IsMinusInfinite;

        return Compare(x, y) == 0;
    }

    /// <inheritdoc />
    public static bool operator !=(BigRational x, BigRational y)
    {
        return Compare(x, y) != 0;
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
        if (r == PlusInfinity)
            return MinusInfinity;
        if (r == MinusInfinity)
            return PlusInfinity;

        return new BigRational(-r.Numerator, r.Denominator);
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
    public static BigRational operator +(BigRational r1, BigRational r2)
    {
        if (r1.IsInfinite || r2.IsInfinite)
        {
            if (r1.IsInfinite && r2.IsInfinite)
            {
                if (r1.Sign == r2.Sign)
                    return r1;
                else
                    throw new UndeterminedResultException("Cannot sum opposite infinities");
            }
            else
            {
                return r1.IsInfinite ? r1 : r2;
            }
        }
        else
        {
            // a/b + c/d  == (ad + bc)/bd
            return new BigRational((r1.Numerator * r2.Denominator) + (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
        }
    }

    /// <inheritdoc cref="Subtract(BigRational, BigRational)"/>
    public static BigRational operator -(BigRational r1, BigRational r2)
    {
        return r1 + (-r2);
    }

    /// <inheritdoc cref="Multiply(BigRational, BigRational)"/>
    public static BigRational operator *(BigRational r1, BigRational r2)
    {
        if (r1.IsZero || r2.IsZero)
        {
            return Zero;
        }
        else if (r1.IsInfinite || r2.IsInfinite)
        {
            return r1.Sign == r2.Sign ? PlusInfinity : MinusInfinity;
        }
        else
        {
            // a/b * c/d  == (ac)/(bd)
            return new BigRational((r1.Numerator * r2.Numerator), (r1.Denominator * r2.Denominator));
        }
    }

    /// <inheritdoc cref="Divide(BigRational, BigRational)"/>
    public static BigRational operator /(BigRational r1, BigRational r2)
    {
        if (r1.IsZero || r2.IsZero)
        {
            if (r1.IsZero && r2.IsZero)
            {
                throw new UndeterminedResultException();
            }
            else if (r1.IsZero)
            {
                return Zero;
            }
            else
            {
                throw new DivideByZeroException();
            }
        }
        else if (r1.IsInfinite || r2.IsInfinite)
        {
            if (r1.IsInfinite && r2.IsInfinite)
            {
                throw new UndeterminedResultException();
            }
            else if (r1.IsInfinite)
            {
                return r1 * r2.Sign;
            }
            else
            {
                return Zero;
            }
        }
        else
        {
            // a/b / c/d  == (ad)/(bc)
            return new BigRational((r1.Numerator * r2.Denominator), (r1.Denominator * r2.Numerator));
        }
    }

    /// <inheritdoc cref="Remainder(BigRational, BigRational)"/>
    public static BigRational operator %(BigRational r1, BigRational r2)
    {
        if (r1.IsInfinite || r2.IsInfinite)
            throw new NotImplementedException();

        // a/b % c/d  == (ad % bc)/bd
        return new BigRational((r1.Numerator * r2.Denominator) % (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
    }
    #endregion Operator Overloads

    #region explicit conversions from BigRational
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator SByte(BigRational value)
    {
        return (SByte)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator UInt16(BigRational value)
    {
        return (UInt16)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator UInt32(BigRational value)
    {
        return (UInt32)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator UInt64(BigRational value)
    {
        return (UInt64)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator Byte(BigRational value)
    {
        return (Byte)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator Int16(BigRational value)
    {
        return (Int16)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator Int32(BigRational value)
    {
        return (Int32)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator Int64(BigRational value)
    {
        return (Int64)(BigInteger.Divide(value.Numerator, value.Denominator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator BigInteger(BigRational value)
    {
        return BigInteger.Divide(value.Numerator, value.Denominator);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static explicit operator Single(BigRational value)
    {
        // The Single value type represents a single-precision 32-bit number with
        // values ranging from negative 3.402823e38 to positive 3.402823e38      
        // values that do not fit into this range are returned as Infinity
        return (Single)((Double)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="OverflowException"></exception>
    public static explicit operator Decimal(BigRational value)
    {
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
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(SByte value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(UInt16 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(UInt32 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(UInt64 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(Byte value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(Int16 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(Int32 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(Int64 value)
    {
        return new BigRational((BigInteger)value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(BigInteger value)
    {
        return new BigRational(value);
    }

    /*
     * This operators are commented out as they're unreliable
     * due to the BigRational(double) constructor
     * 
    public static implicit operator BigRational(Single value)
    {
        return new BigRational((Double)value);
    }

    public static implicit operator BigRational(Double value)
    {
        return new BigRational(value);
    }
    */

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator BigRational(Decimal value)
    {
        return new BigRational(value);
    }

    #endregion implicit conversions to BigRational

    #region instance helper methods
    private void Simplify()
    {
        // * if the numerator is {0, +1, -1} then the fraction is already reduced
        // * if the denominator is {+1} then the fraction is already reduced
        if (Numerator == BigInteger.Zero)
        {
            Denominator = BigInteger.One;
        }

        BigInteger gcd = BigInteger.GreatestCommonDivisor(Numerator, Denominator);
        if (gcd > BigInteger.One)
        {
            Numerator = Numerator / gcd;
            Denominator = Denominator / gcd;
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