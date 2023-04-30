//This file defines struct Rational with either BigRational or LongRational implementation
//Used to switch between the two at compile time without replacing most of the code

//Needs however to manually port over numerical implementations 
//Probably not the best solution

#if BIG_RATIONAL
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Numerics
{
    /// <summary>
    /// Represents a rational number with infinite precision, 
    /// using <see cref="BigInteger"/> for both numerator and denominator.
    /// </summary>
    /// <remarks>
    /// This type is identical to <see cref="BigRational"/> based to compiler flag <code>BIG_RATIONAL</code>.
    /// Replace this with <code>LONG_RATIONAL</code> to change implementation to <see cref="LongRational"/>.
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Rational : IComparable, IComparable<Rational>, IEquatable<Rational>, IToCodeString
    {
        #region Static public values

        /// <inheritdoc cref="BigRational.Zero"/>
        public static Rational Zero { get; } = new Rational(BigInteger.Zero);

        /// <inheritdoc cref="BigRational.One"/>
        public static Rational One { get; } = new Rational(BigInteger.One);

        /// <inheritdoc cref="BigRational.MinusOne"/>
        public static Rational MinusOne { get; } = new Rational(BigInteger.MinusOne);

        /// <inheritdoc cref="BigRational.PlusInfinity"/>
        public static Rational PlusInfinity { get; } = new Rational
        {
            Numerator = 1,
            Denominator = 0
        };

        /// <inheritdoc cref="BigRational.MinusInfinity"/>
        public static Rational MinusInfinity { get; } = new Rational
        {
            Numerator = -1,
            Denominator = 0
        };

        #endregion Static values

        #region Public Properties

        /// <inheritdoc cref="BigRational.Sign"/>
        public Int32 Sign => Numerator.Sign;

        /// <inheritdoc cref="BigRational.Numerator"/>
        [JsonProperty(PropertyName = "num")]
        public BigInteger Numerator { get; private set; }

        /// <inheritdoc cref="BigRational.Denominator"/>
        [JsonProperty(PropertyName = "den")]
        public BigInteger Denominator { get; private set; }

        /// <inheritdoc cref="BigRational.IsFinite"/>
        public bool IsFinite => Denominator != 0;

        /// <inheritdoc cref="BigRational.IsInfinite"/>
        public bool IsInfinite => Denominator == 0;

        /// <inheritdoc cref="BigRational.IsPlusInfinite"/>
        public bool IsPlusInfinite => Denominator == 0 && Numerator == 1;

        /// <inheritdoc cref="BigRational.IsMinusInfinite"/>
        public bool IsMinusInfinite => Denominator == 0 && Numerator == -1;

        /// <inheritdoc cref="BigRational.IsZero"/>
        public bool IsZero => this == Zero;

        /// <inheritdoc cref="BigRational.IsPositive"/>
        public bool IsPositive => this.Sign > 0;

        /// <inheritdoc cref="BigRational.IsNegative"/>
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
        public Rational GetFractionPart()
        {
            return new Rational(BigInteger.Remainder(Numerator, Denominator), Denominator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public BigInteger Floor()
        {
            var wholePart = GetWholePart();
            var fractionPart = GetFractionPart();

            if (wholePart > 0 || wholePart == 0 && fractionPart >= 0)
            {
                return wholePart;
            }
            else
            {
                if (fractionPart < 0)
                    return wholePart - 1;
                else
                    return wholePart;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public BigInteger Ceil()
        {
            var wholePart = GetWholePart();
            var fractionPart = GetFractionPart();

            if (wholePart > 0 || wholePart == 0 && fractionPart >= 0)
            {
                if (fractionPart > 0)
                    return wholePart + 1;
                else
                    return wholePart;
            }
            else
            {
                return wholePart;
            }
        }

        // todo: benchmarks to verify it's faster
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int FastFloor()
        {
            var d = (decimal)this;
            return (int)Math.Floor(d);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int FastCeil()
        {
            var d = (decimal)this;
            return (int)Math.Ceiling(d);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Rational other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Numerator, Denominator);
        }

        // IComparable
        int IComparable.CompareTo(object? obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Rational))
                throw new ArgumentException("Argument must be of type Rational", nameof(obj));
            return Compare(this, (Rational)obj);
        }

        // IComparable<Rational>
        /// <inheritdoc />
        public int CompareTo(Rational other)
        {
            return Compare(this, other);
        }

        // Object.ToString
        /// <inheritdoc />
        public override String ToString()
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
        /// Returns a string containing C# code to create this Rational.
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

        // IEquatable<Rational>
        // a/b = c/d, iff ad = bc
        /// <inheritdoc />
        public Boolean Equals(Rational other)
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

        /// <inheritdoc cref="BigRational(int, int)"/>
        public Rational(int numerator, int denominator = 1)
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

        /// <inheritdoc cref="BigRational(BigInteger)"/>
        public Rational(BigInteger numerator)
        {
            Numerator = numerator;
            Denominator = BigInteger.One;
        }

        /// <summary>
        /// This constructor is unreliable, as proved by the related test, and should not be used in its current state.
        /// </summary>
        internal Rational(Double value)
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
                this = Rational.Zero;
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

        /// <inheritdoc cref="BigRational(decimal)"/>
        public Rational(Decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            if (bits == null || bits.Length != 4 || (bits[3] & ~(DecimalSignMask | DecimalScaleMask)) != 0 || (bits[3] & DecimalScaleMask) > (28 << 16))
            {
                throw new ArgumentException("invalid Decimal", "value");
            }

            if (value == Decimal.Zero)
            {
                this = Rational.Zero;
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

        /// <inheritdoc cref="BigRational(BigInteger, BigInteger)"/>
        public Rational(BigInteger numerator, BigInteger denominator)
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

        /// <inheritdoc cref="BigRational(BigInteger, BigInteger, BigInteger)"/>
        public Rational(BigInteger whole, BigInteger numerator, BigInteger denominator)
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

        /// <inheritdoc cref="BigRational.Abs"/>
        public static Rational Abs(Rational r)
        {
            return (r.Numerator.Sign < 0 ? new Rational(BigInteger.Abs(r.Numerator), r.Denominator) : r);
        }

        /// <inheritdoc cref="BigRational.Negate"/>
        public static Rational Negate(Rational r)
        {
            return new Rational(BigInteger.Negate(r.Numerator), r.Denominator);
        }

        /// <inheritdoc cref="BigRational.Invert"/>
        public static Rational Invert(Rational r)
        {
            return new Rational(r.Denominator, r.Numerator);
        }

        /// <inheritdoc cref="BigRational.Add"/>
        public static Rational Add(Rational x, Rational y)
        {
            return x + y;
        }

        /// <inheritdoc cref="BigRational.Subtract"/>
        public static Rational Subtract(Rational x, Rational y)
        {
            return x - y;
        }


        /// <inheritdoc cref="BigRational.Multiply"/>
        public static Rational Multiply(Rational x, Rational y)
        {
            return x * y;
        }

        /// <inheritdoc cref="BigRational.Divide"/>
        public static Rational Divide(Rational dividend, Rational divisor)
        {
            return dividend / divisor;
        }

        /// <inheritdoc cref="BigRational.Remainder"/>
        public static Rational Remainder(Rational dividend, Rational divisor)
        {
            return dividend % divisor;
        }

        /// <inheritdoc cref="BigRational.DivRem"/>
        public static Rational DivRem(Rational dividend, Rational divisor, out Rational remainder)
        {
            // a/b / c/d  == (ad)/(bc)
            // a/b % c/d  == (ad % bc)/bd

            // (ad) and (bc) need to be calculated for both the division and the remainder operations.
            BigInteger ad = dividend.Numerator * divisor.Denominator;
            BigInteger bc = dividend.Denominator * divisor.Numerator;
            BigInteger bd = dividend.Denominator * divisor.Denominator;

            remainder = new Rational(ad % bc, bd);
            return new Rational(ad, bc);
        }

        /// <inheritdoc cref="BigRational.Pow"/>
        public static Rational Pow(Rational baseValue, BigInteger exponent)
        {
            if (exponent.Sign == 0)
            {
                // 0^0 -> 1
                // n^0 -> 1
                return Rational.One;
            }
            else if (exponent.Sign < 0)
            {
                if (baseValue == Rational.Zero)
                {
                    throw new ArgumentException("cannot raise zero to a negative power", "baseValue");
                }
                // n^(-e) -> (1/n)^e
                baseValue = Rational.Invert(baseValue);
                exponent = BigInteger.Negate(exponent);
            }

            Rational result = baseValue;
            while (exponent > BigInteger.One)
            {
                result = result * baseValue;
                exponent--;
            }

            return result;
        }

        /// <inheritdoc cref="BigRational.LeastCommonDenominator"/>
        public static BigInteger LeastCommonDenominator(Rational x, Rational y)
        {
            // LCD( a/b, c/d ) == (bd) / gcd(b,d)
            return (x.Denominator / BigInteger.GreatestCommonDivisor(x.Denominator, y.Denominator)) * y.Denominator;
        }

        /// <inheritdoc cref="BigRational.GreatestCommonDivisor"/>
        public static Rational GreatestCommonDivisor(Rational a, Rational b)
        {
            while (b != 0)
            {
                Rational temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <inheritdoc cref="BigRational.LeastCommonMultiple"/>
        public static Rational LeastCommonMultiple(Rational a, Rational b)
        {
            return (a / Rational.GreatestCommonDivisor(a, b)) * b;
        }

        /// <inheritdoc cref="BigRational.Compare"/>
        public static int Compare(Rational r1, Rational r2)
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
                // a/b >= c/d, iff ad >= bc
                return BigInteger.Compare(r1.Numerator * r2.Denominator, r2.Numerator * r1.Denominator);
            }

            int intCompare(int left, int right)
            {
                return left - right;
            }
        }

        /// <inheritdoc cref="BigRational.Max(BigRational, BigRational)"/>
        public static Rational Max(Rational a, Rational b) => a > b ? a : b;

        /// <inheritdoc cref="BigRational.Max(BigRational, BigRational, BigRational)"/>
        public static Rational Max(Rational a, Rational b, Rational c) => Max(a, Max(b, c));    //good enough

        /// <inheritdoc cref="BigRational.Min(BigRational, BigRational)"/>
        public static Rational Min(Rational a, Rational b) => a > b ? b : a;

        /// <inheritdoc cref="BigRational.Min(BigRational, BigRational, BigRational)"/>
        public static Rational Min(Rational a, Rational b, Rational c) => Min(a, Min(b, c));

        #endregion Public Static Methods

        #region Operator Overloads

        /// <inheritdoc />
        public static bool operator ==(Rational x, Rational y)
        {
            // Shortcut common comparisons to infinite
            if (y.IsPlusInfinite)
                return x.IsPlusInfinite;
            if (y.IsMinusInfinite)
                return x.IsMinusInfinite;

            return Compare(x, y) == 0;
        }

        /// <inheritdoc />
        public static bool operator !=(Rational x, Rational y)
        {
            return Compare(x, y) != 0;
        }

        /// <inheritdoc />
        public static bool operator <(Rational x, Rational y)
        {
            return Compare(x, y) < 0;
        }

        /// <inheritdoc />
        public static bool operator <=(Rational x, Rational y)
        {
            return Compare(x, y) <= 0;
        }

        /// <inheritdoc />
        public static bool operator >(Rational x, Rational y)
        {
            return Compare(x, y) > 0;
        }

        /// <inheritdoc />
        public static bool operator >=(Rational x, Rational y)
        {
            return Compare(x, y) >= 0;
        }

        /// <inheritdoc />
        public static Rational operator +(Rational r)
        {
            return r;
        }

        /// <inheritdoc />
        public static Rational operator -(Rational r)
        {
            if (r == PlusInfinity)
                return MinusInfinity;
            if (r == MinusInfinity)
                return PlusInfinity;

            return new Rational(-r.Numerator, r.Denominator);
        }

        /// <inheritdoc />
        public static Rational operator ++(Rational r)
        {
            if (r.IsInfinite)
                return r;

            return r + Rational.One;
        }

        /// <inheritdoc />
        public static Rational operator --(Rational r)
        {
            if (r.IsInfinite)
                return r;

            return r - Rational.One;
        }

        /// <inheritdoc />
        public static Rational operator +(Rational r1, Rational r2)
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
                return new Rational((r1.Numerator * r2.Denominator) + (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
            }
        }

        /// <inheritdoc />
        public static Rational operator -(Rational r1, Rational r2)
        {
            return r1 + (-r2);
        }

        /// <inheritdoc />
        public static Rational operator *(Rational r1, Rational r2)
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
                return new Rational((r1.Numerator * r2.Numerator), (r1.Denominator * r2.Denominator));
            }
        }

        /// <inheritdoc />
        public static Rational operator /(Rational r1, Rational r2)
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
                return new Rational((r1.Numerator * r2.Denominator), (r1.Denominator * r2.Numerator));
            }
        }

        /// <inheritdoc />
        public static Rational operator %(Rational r1, Rational r2)
        {
            if (r1.IsInfinite || r2.IsInfinite)
                throw new NotImplementedException();

            // a/b % c/d  == (ad % bc)/bd
            return new Rational((r1.Numerator * r2.Denominator) % (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
        }
        #endregion Operator Overloads

        #region explicit conversions from Rational

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator SByte(Rational value)
        {
            return (SByte)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator UInt16(Rational value)
        {
            return (UInt16)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator UInt32(Rational value)
        {
            return (UInt32)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator UInt64(Rational value)
        {
            return (UInt64)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Byte(Rational value)
        {
            return (Byte)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Int16(Rational value)
        {
            return (Int16)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Int32(Rational value)
        {
            return (Int32)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Int64(Rational value)
        {
            return (Int64)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator BigInteger(Rational value)
        {
            return BigInteger.Divide(value.Numerator, value.Denominator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Single(Rational value)
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
        public static explicit operator Double(Rational value)
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
        public static explicit operator Decimal(Rational value)
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
        #endregion explicit conversions from Rational

        #region implicit conversions to Rational

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(SByte value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(UInt16 value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(UInt32 value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(UInt64 value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(Byte value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(Int16 value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(Int32 value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(Int64 value)
        {
            return new Rational((BigInteger)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Rational(BigInteger value)
        {
            return new Rational(value);
        }

        /*
         * This operators are commented out as they're unreliable
         * due to the BigRational(double) constructor
         * 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator BigRational(Single value)
        {
            return new BigRational((Double)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        public static implicit operator Rational(Decimal value)
        {
            return new Rational(value);
        }

        #endregion implicit conversions to Rational

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
}
#endif

#if LONG_RATIONAL
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;

// Adapted from BigRational with the aim of reducing perfomance impact
// Profiling highlighted heavy use of Compare, which was measured to be up to 30x slower on a BigInteger implementation compared to long
namespace Unipi.Nancy.Numerics
{
    /// <summary>
    /// Represents a rational number with infinite precision, 
    /// using <see cref="long"/> for both numerator and denominator.
    /// </summary>
    /// <remarks>
    /// This type is identical to <see cref="LongRational"/> based to compiler flag <code>LONG_RATIONAL</code>.
    /// Replace this with <code>BIG_RATIONAL</code> to change implementation to <see cref="BigRational"/>.
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Rational : IComparable, IComparable<Rational>, IEquatable<Rational>, IToCodeString
    {
        #region Static public values

        /// <inheritdoc cref="LongRational.Zero"/>
        public static Rational Zero { get; } = new Rational(0);

        /// <inheritdoc cref="LongRational.One"/>
        public static Rational One { get; } = new Rational(1);

        /// <inheritdoc cref="LongRational.MinusOne"/>
        public static Rational MinusOne { get; } = new Rational(-1);

        /// <inheritdoc cref="LongRational.PlusInfinity"/>
        public static Rational PlusInfinity { get; } = new Rational
        {
            Numerator = 1,
            Denominator = 0
        };

        /// <inheritdoc cref="LongRational.MinusInfinity"/>
        public static Rational MinusInfinity { get; } = new Rational
        {
            Numerator = -1,
            Denominator = 0
        };

        #endregion Static values

        #region Public Properties

        /// <inheritdoc cref="LongRational.Sign"/>
        public int Sign => Math.Sign(Numerator);

        /// <inheritdoc cref="LongRational.Numerator"/>
        [JsonProperty(PropertyName = "num")]
        public long Numerator { get; private set; }

        /// <inheritdoc cref="LongRational.Denominator"/>
        [JsonProperty(PropertyName = "den")]
        public long Denominator { get; private set; }

        /// <inheritdoc cref="LongRational.IsInfinite"/>
        public bool IsInfinite => Denominator == 0;

        /// <inheritdoc cref="LongRational.IsFinite"/>
        public bool IsFinite => Denominator != 0;

        /// <inheritdoc cref="LongRational.IsPlusInfinite"/>
        public bool IsPlusInfinite => Denominator == 0 && Numerator == 1;

        /// <inheritdoc cref="LongRational.IsMinusInfinite"/>
        public bool IsMinusInfinite => Denominator == 0 && Numerator == -1;

        /// <inheritdoc cref="LongRational.IsZero"/>
        public bool IsZero => this == Zero;

        /// <inheritdoc cref="LongRational.IsPositive"/>
        public bool IsPositive => this.Sign > 0;

        /// <inheritdoc cref="LongRational.IsNegative"/>
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
        // Rational == Whole, Fraction
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
        public long GetWholePart()
        {
            return Numerator / Denominator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Rational GetFractionPart()
        {
            return new Rational(Numerator % Denominator, Denominator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long Floor()
        {
            var wholePart = GetWholePart();
            var fractionPart = GetFractionPart();

            if (wholePart > 0 || wholePart == 0 && fractionPart >= 0)
            {
                return wholePart;
            }
            else
            {
                if (fractionPart < 0)
                    return wholePart - 1;
                else
                    return wholePart;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long Ceil()
        {
            var wholePart = GetWholePart();
            var fractionPart = GetFractionPart();

            if (wholePart > 0 || wholePart == 0 && fractionPart >= 0)
            {
                if (fractionPart > 0)
                    return wholePart + 1;
                else
                    return wholePart;
            }
            else
            {
                return wholePart;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int FastFloor() => (int) Floor();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int FastCeil() => (int) Ceil();

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Rational))
                return false;
            return this.Equals((Rational)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Numerator, Denominator).GetHashCode();
        }

        // IComparable
        int IComparable.CompareTo(object? obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Rational))
                throw new ArgumentException("Argument must be of type Rational", nameof(obj));
            return Compare(this, (Rational)obj);
        }

        /// <inheritdoc cref="IComparable{Rational}.CompareTo"/>
        public int CompareTo(Rational other)
        {
            return Compare(this, other);
        }

        // Object.ToString
        /// <inheritdoc />
        public override String ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append(Numerator.ToString(CultureInfo.InvariantCulture));
            if (Denominator != 1)
            {
                ret.Append(c_solidus);
                ret.Append(Denominator.ToString(CultureInfo.InvariantCulture));
            }

            return ret.ToString();
        }

        /// <summary>
        /// Returns a string containing C# code to create this Rational.
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

        /// <inheritdoc cref="IEquatable{Rational}.Equals(Rational)"/>
        public Boolean Equals(Rational other)
        {
            // a/b = c/d, iff ad = bc
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

        /// <inheritdoc cref="LongRational(long, long)"/>
        public Rational(long numerator, long denominator = 1)
        {
            if (denominator == 0)
            {
                if (numerator < 0)
                    Numerator = -1;
                else if (numerator > 0)
                    Numerator = 1;
                else
                    throw new UndeterminedResultException("Zero over zero");

                Denominator = 0;
            }
            else if (numerator == 0)
            {
                // 0/m -> 0/1
                Numerator = 0;
                Denominator = 1;
            }
            else if (denominator < 0)
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

        /// <inheritdoc cref="LongRational(long)"/>
        public Rational(long numerator)
        {
            Numerator = numerator;
            Denominator = 1;
        }

        // Rational(decimal) -
        //
        // The decimal type represents floating point numbers exactly, with no rounding error.
        // Values such as "0.1" in decimal are actually representable, and convert cleanly
        // to Rational as "1/10"
        /// <inheritdoc cref="LongRational(decimal)"/>
        public Rational(decimal value)
        {
            //This is an intuitive and surely inefficient implementation.
            //Wrote fast to just work

            string representation = value.ToString("0.00", CultureInfo.InvariantCulture);
            var parts = representation.Split('.');
            Rational integerPart = new Rational(long.Parse(parts[0]));
            Rational decimalPart = (parts.Length > 1) ? new Rational(GetNineDigits(parts[1]), 1_000_000_000) : 0;
            Rational sum = integerPart + decimalPart;

            Numerator = sum.Numerator;
            Denominator = sum.Denominator;

            long GetNineDigits(string decimals)
            {
                if (decimals.Length > 9)
                    decimals = decimals.Substring(0, 9);

                if (decimals.Length < 9)
                {
                    StringBuilder sb = new StringBuilder(decimals);
                    for (int i = 0; i < 9 - decimals.Length; i++)
                        sb.Append('0');
                    decimals = sb.ToString();
                }

                return long.Parse(decimals);
            }
        }

        #endregion Constructors

        #region Public Static Methods

        /// <inheritdoc cref="LongRational.Abs"/>
        public static Rational Abs(Rational r)
        {
            return (r.Numerator < 0 ? new Rational(Math.Abs(r.Numerator), r.Denominator) : r);
        }

        /// <inheritdoc cref="LongRational.Negate"/>
        public static Rational Negate(Rational r)
        {
            return new Rational(-r.Numerator, r.Denominator);
        }

        /// <inheritdoc cref="LongRational.Invert"/>
        public static Rational Invert(Rational r)
        {
            return new Rational(r.Denominator, r.Numerator);
        }

        /// <inheritdoc cref="LongRational.Add"/>
        public static Rational Add(Rational x, Rational y)
        {
            return x + y;
        }

        /// <inheritdoc cref="LongRational.Subtract"/>
        public static Rational Subtract(Rational x, Rational y)
        {
            return x - y;
        }


        /// <inheritdoc cref="LongRational.Multiply"/>
        public static Rational Multiply(Rational x, Rational y)
        {
            return x * y;
        }

        /// <inheritdoc cref="LongRational.Divide"/>
        public static Rational Divide(Rational dividend, Rational divisor)
        {
            return dividend / divisor;
        }

        /// <inheritdoc cref="LongRational.Remainder"/>
        public static Rational Remainder(Rational dividend, Rational divisor)
        {
            return dividend % divisor;
        }

        /// <inheritdoc cref="LongRational.DivRem"/>
        public static Rational DivRem(Rational dividend, Rational divisor, out Rational remainder)
        {
            // a/b / c/d  == (ad)/(bc)
            // a/b % c/d  == (ad % bc)/bd

            // (ad) and (bc) need to be calculated for both the division and the remainder operations.
            long ad = dividend.Numerator * divisor.Denominator;
            long bc = dividend.Denominator * divisor.Numerator;
            long bd = dividend.Denominator * divisor.Denominator;

            remainder = new Rational(ad % bc, bd);
            return new Rational(ad, bc);
        }

        /// <inheritdoc cref="LongRational.Pow"/>
        public static Rational Pow(Rational baseValue, long exponent)
        {
            if (exponent == 0)
            {
                // 0^0 -> 1
                // n^0 -> 1
                return Rational.One;
            }
            else if (exponent < 0)
            {
                if (baseValue == Rational.Zero)
                {
                    throw new ArgumentException("cannot raise zero to a negative power", nameof(baseValue));
                }
                // n^(-e) -> (1/n)^e
                baseValue = Rational.Invert(baseValue);
                exponent = -exponent;
            }

            Rational result = baseValue;
            while (exponent > 1)
            {
                result = result * baseValue;
                exponent--;
            }

            return result;
        }

        /// <inheritdoc cref="LongRational.LeastCommonDenominator(LongRational, LongRational)"/>
        public static long LeastCommonDenominator(Rational x, Rational y)
        {
            // LCD( a/b, c/d ) == (bd) / gcd(b,d)
            return (x.Denominator / GreatestCommonDivisor(x.Denominator, y.Denominator)) * y.Denominator;
        }

        /// <inheritdoc cref="LongRational.GreatestCommonDivisor(LongRational, LongRational)"/>
        public static Rational GreatestCommonDivisor(Rational a, Rational b)
        {
            while (b != 0)
            {
                Rational temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <inheritdoc cref="LongRational.GreatestCommonDivisor(long, long)"/>
        public static long GreatestCommonDivisor(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <inheritdoc cref="LongRational.LeastCommonMultiple(LongRational, LongRational)"/>
        public static Rational LeastCommonMultiple(Rational a, Rational b)
        {
            return (a / Rational.GreatestCommonDivisor(a, b)) * b;
        }

        /// <inheritdoc cref="LongRational.Compare(LongRational, LongRational)"/>
        public static int Compare(Rational r1, Rational r2)
        {
            if (r1.IsInfinite || r2.IsInfinite)
            {
                //I will call int.CompareTo with adapted parameters to delegate knowledge about return values
                if (r1.IsInfinite && r2.IsInfinite)
                    return r1.Sign.CompareTo(r2.Sign);
                else
                {
                    //An infinite value is always bigger in absolute value
                    if (r1.IsInfinite)
                    {
                        return r1.Sign;                        
                    }
                    else
                    {
                        return -r2.Sign;
                    }
                }
            }
            else
            {
                // a/b = c/d, iff ad = bc
                return (r1.Numerator * r2.Denominator).CompareTo(r2.Numerator * r1.Denominator);
            }
        }

        /// <inheritdoc cref="LongRational.Max(LongRational, LongRational)"/>
        public static Rational Max(Rational a, Rational b) => a > b ? a : b;

        /// <inheritdoc cref="LongRational.Max(LongRational, LongRational, LongRational)"/>
        public static Rational Max(Rational a, Rational b, Rational c) => Max(a, Max(b, c));    //good enough

        /// <inheritdoc cref="LongRational.Min(LongRational, LongRational)"/>
        public static Rational Min(Rational a, Rational b) => a > b ? b : a;

        /// <inheritdoc cref="LongRational.Min(LongRational, LongRational, LongRational)"/>
        public static Rational Min(Rational a, Rational b, Rational c) => Min(a, Min(b, c));


        #endregion Public Static Methods

        #region Operator Overloads

        /// <inheritdoc />
        public static bool operator ==(Rational x, Rational y)
        {
            return Compare(x, y) == 0;
        }

        /// <inheritdoc />
        public static bool operator !=(Rational x, Rational y)
        {
            return Compare(x, y) != 0;
        }

        /// <inheritdoc />
        public static bool operator <(Rational x, Rational y)
        {
            return Compare(x, y) < 0;
        }

        /// <inheritdoc />
        public static bool operator <=(Rational x, Rational y)
        {
            return Compare(x, y) <= 0;
        }

        /// <inheritdoc />
        public static bool operator >(Rational x, Rational y)
        {
            return Compare(x, y) > 0;
        }

        /// <inheritdoc />
        public static bool operator >=(Rational x, Rational y)
        {
            return Compare(x, y) >= 0;
        }

        /// <inheritdoc />
        public static Rational operator +(Rational r)
        {
            return r;
        }

        /// <inheritdoc />
        public static Rational operator -(Rational r)
        {
            if (r == PlusInfinity)
                return MinusInfinity;
            if (r == MinusInfinity)
                return PlusInfinity;

            return new Rational(-r.Numerator, r.Denominator);
        }

        /// <inheritdoc />
        public static Rational operator ++(Rational r)
        {
            if (r.IsInfinite)
                return r;

            return r + Rational.One;
        }

        /// <inheritdoc />
        public static Rational operator --(Rational r)
        {
            if (r.IsInfinite)
                return r;

            return r - Rational.One;
        }

        /// <inheritdoc />
        public static Rational operator +(Rational r1, Rational r2)
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
                return new Rational((r1.Numerator * r2.Denominator) + (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
            }
        }

        /// <inheritdoc />
        public static Rational operator -(Rational r1, Rational r2)
        {
            return r1 + (-r2);
        }

        /// <inheritdoc />
        public static Rational operator *(Rational r1, Rational r2)
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
                return new Rational((r1.Numerator * r2.Numerator), (r1.Denominator * r2.Denominator));
            }
        }

        /// <inheritdoc />
        public static Rational operator /(Rational r1, Rational r2)
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
                return new Rational((r1.Numerator * r2.Denominator), (r1.Denominator * r2.Numerator));
            }
        }

        /// <inheritdoc />
        public static Rational operator %(Rational r1, Rational r2)
        {
            if (r1.IsInfinite || r2.IsInfinite)
                throw new NotImplementedException();

            // a/b % c/d  == (ad % bc)/bd
            return new Rational((r1.Numerator * r2.Denominator) % (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
        }
        #endregion Operator Overloads

        #region explicit conversions from Rational

        /// <inheritdoc cref="LongRational.explicit operator sbyte"/>
        public static explicit operator SByte(Rational value)
        {
            return (SByte)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator UInt16"/>
        public static explicit operator UInt16(Rational value)
        {
            return (UInt16)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator UInt32"/>
        public static explicit operator UInt32(Rational value)
        {
            return (UInt32)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator UInt64"/>
        public static explicit operator UInt64(Rational value)
        {
            return (UInt64)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator Byte"/>
        public static explicit operator Byte(Rational value)
        {
            return (Byte)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator Int16"/>
        public static explicit operator Int16(Rational value)
        {
            return (Int16)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator Int32"/>
        public static explicit operator Int32(Rational value)
        {
            return (Int32)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator Int64"/>
        public static explicit operator Int64(Rational value)
        {
            return (Int64)(value.Numerator / value.Denominator);
        }

        /// <inheritdoc cref="LongRational.explicit operator Single"/>
        public static explicit operator Single(Rational value)
        {
            // The Single value type represents a single-precision 32-bit number with
            // values ranging from negative 3.402823e38 to positive 3.402823e38      
            // values that do not fit into this range are returned as Infinity
            return (Single)((Double)value);
        }

        /// <inheritdoc cref="LongRational.explicit operator Double"/>
        public static explicit operator Double(Rational value)
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

        /// <inheritdoc cref="LongRational.explicit operator Decimal"/>
        public static explicit operator Decimal(Rational value)
        {
            if (value.IsInfinite)
            {
                return value.Sign > 0 ? Decimal.MaxValue : Decimal.MinValue;
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
        #endregion explicit conversions from Rational

        #region implicit conversions to Rational

        /// <inheritdoc cref="LongRational.implicit operator LongRational(SByte)"/>
        public static implicit operator Rational(SByte value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(UInt16)"/>
        public static implicit operator Rational(UInt16 value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(UInt32)"/>
        public static implicit operator Rational(UInt32 value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(UInt64)"/>
        public static implicit operator Rational(UInt64 value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(Byte)"/>
        public static implicit operator Rational(Byte value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(Int16)"/>
        public static implicit operator Rational(Int16 value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(Int32)"/>
        public static implicit operator Rational(Int32 value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(Int64)"/>
        public static implicit operator Rational(Int64 value)
        {
            return new Rational((long)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(BigInteger)"/>
        public static implicit operator Rational(BigInteger value)
        {
            return new Rational((long)value);
        }

        /*
         * This operators are commented out as they're unreliable
         * due to the Rational(double) constructor
         * 
        /// <inheritdoc cref="LongRational.implicit operator LongRational(Single)"/>
        public static implicit operator Rational(Single value)
        {
            return new Rational((Double)value);
        }

        /// <inheritdoc cref="LongRational.implicit operator LongRational(Double)"/>
        public static implicit operator Rational(Double value)
        {
            return new Rational(value);
        }
        */

        /// <inheritdoc cref="LongRational.implicit operator LongRational(Decimal)"/>
        public static implicit operator Rational(Decimal value)
        {
            return new Rational(value);
        }

        #endregion implicit conversions to Rational

        #region instance helper methods
        private void Simplify()
        {
            // * if the numerator is {0, +1, -1} then the fraction is already reduced
            // * if the denominator is {+1} then the fraction is already reduced
            if (Numerator == 0)
            {
                Denominator = 1;
            }

            long gcd = GreatestCommonDivisor(Numerator, Denominator);
            if (gcd > 1)
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
}
#endif