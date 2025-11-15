//This file defines struct Rational with either BigRational or LongRational implementation
//Used to switch between the two at compile time without replacing most of the code

//Needs however to manually port over numerical implementations 
//Probably not the best solution

using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.UncheckedInternals;
using Unipi.Nancy.Utility;

namespace Unipi.Nancy.Numerics
{
    #if BIG_RATIONAL
    /// <summary>
    /// Represents a rational number with infinite precision, 
    /// using <see cref="BigInteger"/> for both numerator and denominator.
    /// </summary>
    /// <remarks>
    /// This type is identical to <see cref="BigRational"/> based to compiler flag <c>BIG_RATIONAL</c>.
    /// Replace this with <c>LONG_RATIONAL</c> to change implementation to <see cref="LongRational"/>.
    /// </remarks>
    #elif LONG_RATIONAL
    /// <summary>
    /// Represents a rational number with infinite precision, 
    /// using <see cref="long"/> for both numerator and denominator.
    /// </summary>
    /// <remarks>
    /// Adapted from BigRational with the aim of reducing perfomance impact. 
    /// Profiling highlighted heavy use of Compare, which was measured to be up to 30x slower on a BigInteger implementation compared to long.
    /// This type is identical to <see cref="LongRational"/> based to compiler flag <c>LONG_RATIONAL</c>.
    /// Replace this with <c>BIG_RATIONAL</c> to change implementation to <see cref="BigRational"/>.
    /// </remarks>
    #endif
    [Serializable]
    [ComVisible(false)]
    [JsonObject(MemberSerialization.OptIn)]
    [System.Text.Json.Serialization.JsonConverter(typeof(RationalSystemJsonConverter))]
    public struct Rational : IComparable, IComparable<Rational>, IEquatable<Rational>, IToCodeString, IStableHashCode
    {
        #region Static public values

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Zero"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Zero"/>
        #endif
        public static Rational Zero { get; } = 
        #if BIG_RATIONAL
            new Rational(BigInteger.Zero);
        #elif LONG_RATIONAL
            new Rational(0);
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.One"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.One"/>
        #endif
        public static Rational One { get; } = 
        #if BIG_RATIONAL
            new Rational(BigInteger.One);
        #elif LONG_RATIONAL
            new Rational(1);
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.MinusOne"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.MinusOne"/>
        #endif
        public static Rational MinusOne { get; } = 
        #if BIG_RATIONAL
            new Rational(BigInteger.MinusOne);
        #elif LONG_RATIONAL
            new Rational(-1);
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.PlusInfinity"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.PlusInfinity"/>
        #endif
        public static Rational PlusInfinity { get; } = new Rational
        {
            Numerator = 1,
            Denominator = 0
        };

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.MinusInfinity"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.MinusInfinity"/>
        #endif
        public static Rational MinusInfinity { get; } = new Rational
        {
            Numerator = -1,
            Denominator = 0
        };

        #endregion Static values

        #region Public Properties

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Sign"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Sign"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly Int32 Sign 
        #if BIG_RATIONAL
            => Numerator.Sign;
        #elif LONG_RATIONAL
            => Math.Sign(Numerator);
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Numerator"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Numerator"/>
        #endif
        [JsonProperty(PropertyName = "num")]
        [JsonPropertyName("num")]
        [System.Text.Json.Serialization.JsonConverter(typeof(BigIntegerSystemJsonConverter))]
        #if BIG_RATIONAL
        public BigInteger Numerator { get; private set; }
        #elif LONG_RATIONAL
        public long Numerator { get; private set; }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Denominator"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Denominator"/>
        #endif
        [JsonProperty(PropertyName = "den")]
        [JsonPropertyName("den")]
        [System.Text.Json.Serialization.JsonConverter(typeof(BigIntegerSystemJsonConverter))]
        #if BIG_RATIONAL
        public BigInteger Denominator { get; private set; }
        #elif LONG_RATIONAL
        public long Denominator { get; private set; }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsFinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsFinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsFinite => Denominator != 0;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsInfinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsInfinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsInfinite => Denominator == 0;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsPlusInfinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsPlusInfinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsPlusInfinite => Denominator == 0 && Numerator == 1;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsMinusInfinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsMinusInfinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsMinusInfinite => Denominator == 0 && Numerator == -1;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsZero"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsZero"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsZero => this == Zero;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsPositive"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsPositive"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsPositive => this.Sign > 0;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsNegative"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsNegative"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsNegative => this.Sign < 0;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsInteger"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsInteger"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsInteger => 
        #if BIG_RATIONAL
            Denominator.IsOne;
        #elif LONG_RATIONAL
            Denominator == 1;
        #endif

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
        #if BIG_RATIONAL
        public readonly BigInteger GetWholePart()
        {
            return BigInteger.Divide(Numerator, Denominator);
        }
        #elif LONG_RATIONAL
        public readonly long GetWholePart()
        {
            return Numerator / Denominator;
        }
        #endif

        /// <summary>
        /// 
        /// </summary>
        public readonly Rational GetFractionPart()
        {
            #if BIG_RATIONAL
            return new Rational(BigInteger.Remainder(Numerator, Denominator), Denominator);
            #elif LONG_RATIONAL
            return new Rational(Numerator % Denominator, Denominator);
            #endif
        }

        /// <summary>
        /// 
        /// </summary>
        #if BIG_RATIONAL
        public readonly BigInteger Floor()
        #elif LONG_RATIONAL
        public readonly long Floor()
        #endif
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
        #if BIG_RATIONAL
        public readonly BigInteger Ceil()
        #elif LONG_RATIONAL
        public readonly long Ceil()
        #endif
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
        public readonly int FastFloor()
        {
            var d = (decimal)this;
            return (int)Math.Floor(d);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly int FastCeil()
        {
            var d = (decimal)this;
            return (int)Math.Ceiling(d);
        }

        /// <inheritdoc />
        public override readonly bool Equals(object? obj)
        {
            return obj is Rational other && Equals(other);
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
        readonly int IComparable.CompareTo(object? obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Rational))
                throw new ArgumentException("Argument must be of type Rational", nameof(obj));
            return Compare(this, (Rational)obj);
        }

        // IComparable<Rational>
        /// <inheritdoc />
        public readonly int CompareTo(Rational other)
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
        /// Returns a string containing C# code to create this Rational.
        /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
        /// </summary>
        public readonly string ToCodeString(bool formatted = false, int indentation = 0)
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
        public readonly Boolean Equals(Rational other)
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(int, int)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational(int, int)"/>
        #endif
        public Rational(int numerator, int denominator = 1)
        {
            if (denominator == 0)
            {
                if (numerator < 0)
                    #if BIG_RATIONAL
                    Numerator = BigInteger.MinusOne;
                    #elif LONG_RATIONAL
                    Numerator = -1;
                    #endif
                else if (numerator > 0)
                    #if BIG_RATIONAL
                    Numerator = BigInteger.One;
                    #elif LONG_RATIONAL
                    Numerator = 1;
                    #endif
                else
                    throw new UndeterminedResultException("Zero over zero");

                #if BIG_RATIONAL
                Denominator = BigInteger.Zero;
                #elif LONG_RATIONAL
                Denominator = 0;
                #endif
            }
            else if (numerator == 0)
            {
                // 0/m -> 0/1
                #if BIG_RATIONAL
                Numerator = BigInteger.Zero;
                Denominator = BigInteger.One;
                #elif LONG_RATIONAL
                Numerator = 0;
                Denominator = 1;
                #endif
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(long, long)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational(long, long)"/>
        #endif
        public Rational(long numerator, long denominator = 1)
        {
            if (denominator == 0)
            {
                if (numerator < 0)
                    #if BIG_RATIONAL
                    Numerator = BigInteger.MinusOne;
                    #elif LONG_RATIONAL
                    Numerator = -1;
                    #endif
                else if (numerator > 0)
                    #if BIG_RATIONAL
                    Numerator = BigInteger.One;
                    #elif LONG_RATIONAL
                    Numerator = 1;
                    #endif
                else
                    throw new UndeterminedResultException("Zero over zero");

                #if BIG_RATIONAL
                Denominator = BigInteger.Zero;
                #elif LONG_RATIONAL
                Denominator = 0;
                #endif
            }
            else if (numerator == 0)
            {
                // 0/m -> 0/1
                #if BIG_RATIONAL
                Numerator = BigInteger.Zero;
                Denominator = BigInteger.One;
                #elif LONG_RATIONAL
                Numerator = 0;
                Denominator = 1;
                #endif
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
        
        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(BigInteger)"/>
        public Rational(BigInteger numerator)
        {
            Numerator = numerator;
            Denominator = BigInteger.One;
        }
        #endif

        #if BIG_RATIONAL
        /// <summary>
        /// This constructor is unreliable, as proved by the related test, and should not be used in its current state.
        /// </summary>
        internal Rational(Double value)
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentException("Argument is not a number", nameof(value));
            }
            else if (Double.IsInfinity(value))
            {
                throw new ArgumentException("Argument is infinity", nameof(value));
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
                Numerator = -Numerator;
            }
            Simplify();
        }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(decimal)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational(decimal)"/>
        #endif
        public Rational(Decimal value)
        {
            #if BIG_RATIONAL
            if (value == Decimal.Zero) {
                this = Rational.Zero;
            }
            {
                (Numerator, Denominator) = value.GetRationalParts();
                Simplify();
            }
            #elif LONG_RATIONAL
            //This is an intuitive and surely inefficient implementation.
            //Wrote fast to just work

            // uses 18 digits as it is the maximum for a long decimal number
            string representation = value.ToString("f18", CultureInfo.InvariantCulture);
            var parts = representation.Split('.');
            Rational integerPart = new Rational(long.Parse(parts[0]));
            Rational decimalPart = (parts.Length > 1) ? new Rational(GetEighteenDigits(parts[1]), 1_000_000_000_000_000_000) : 0;
            Rational sum = integerPart + decimalPart;

            Numerator = sum.Numerator;
            Denominator = sum.Denominator;
            Simplify();
            long GetEighteenDigits(string decimals)
            {
                const int nDigits = 18;
                if (decimals.Length > nDigits)
                    decimals = decimals.Substring(0, nDigits);

                if (decimals.Length < nDigits)
                {
                    StringBuilder sb = new StringBuilder(decimals);
                    for (int i = 0; i < nDigits - decimals.Length; i++)
                        sb.Append('0');
                    decimals = sb.ToString();
                }

                return long.Parse(decimals);
            }
            #endif
        }

        #if BIG_RATIONAL
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
        #endif

        #if BIG_RATIONAL
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
        #endif

        #endregion Constructors

        #region Public Static Methods

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Abs"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Abs"/>
        #endif
        public static Rational Abs(Rational r)
        {
            #if BIG_RATIONAL
            return (r.Numerator.Sign < 0 ? new Rational(BigInteger.Abs(r.Numerator), r.Denominator) : r);
            #elif LONG_RATIONAL
            return (r.Numerator < 0 ? new Rational(Math.Abs(r.Numerator), r.Denominator) : r);
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Negate"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Negate"/>
        #endif
        public static Rational Negate(Rational r)
        {
            return new Rational(-r.Numerator, r.Denominator);
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Invert"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Invert"/>
        #endif
        public static Rational Invert(Rational r)
        {
            return new Rational(r.Denominator, r.Numerator);
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Add"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Add"/>
        #endif
        public static Rational Add(Rational x, Rational y)
        {
            return x + y;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Subtract"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Subtract"/>
        #endif
        public static Rational Subtract(Rational x, Rational y)
        {
            return x - y;
        }


        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Multiply"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Multiply"/>
        #endif
        public static Rational Multiply(Rational x, Rational y)
        {
            return x * y;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Divide"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Divide"/>
        #endif
        public static Rational Divide(Rational dividend, Rational divisor)
        {
            return dividend / divisor;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Remainder"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Remainder"/>
        #endif
        public static Rational Remainder(Rational dividend, Rational divisor)
        {
            return dividend % divisor;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.DivRem"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.DivRem"/>
        #endif
        public static Rational DivRem(Rational dividend, Rational divisor, out Rational remainder)
        {
            // a/b / c/d  == (ad)/(bc)
            // a/b % c/d  == (ad % bc)/bd

            // (ad) and (bc) need to be calculated for both the division and the remainder operations.
            var ad = dividend.Numerator * divisor.Denominator;
            var bc = dividend.Denominator * divisor.Numerator;
            var bd = dividend.Denominator * divisor.Denominator;

            remainder = new Rational(ad % bc, bd);
            return new Rational(ad, bc);
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Pow"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Pow"/>
        #endif
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
                    throw new ArgumentException("cannot raise zero to a negative power", nameof(baseValue));
                }
                // n^(-e) -> (1/n)^e
                baseValue = Rational.Invert(baseValue);
                exponent = -exponent;
            }

            Rational result = baseValue;
            while (exponent > BigInteger.One)
            {
                result = result * baseValue;
                exponent--;
            }

            return result;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.LeastCommonDenominator"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.LeastCommonDenominator"/>
        #endif
        public static BigInteger LeastCommonDenominator(Rational x, Rational y)
        {
            // LCD( a/b, c/d ) == (bd) / gcd(b,d)
            return (x.Denominator / BigInteger.GreatestCommonDivisor(x.Denominator, y.Denominator)) * y.Denominator;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.GreatestCommonDivisor(BigRational, BigRational)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.GreatestCommonDivisor(LongRational, LongRational)"/>
        #endif
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.LeastCommonMultiple"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.LeastCommonMultiple"/>
        #endif
        public static Rational LeastCommonMultiple(Rational a, Rational b)
        {
            return (a / Rational.GreatestCommonDivisor(a, b)) * b;
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Compare"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Compare"/>
        #endif
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Max(BigRational, BigRational)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Max(LongRational, LongRational)"/>
        #endif
        public static Rational Max(Rational a, Rational b) => a > b ? a : b;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Max(BigRational, BigRational, BigRational)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Max(LongRational, LongRational, LongRational)"/>
        #endif
        public static Rational Max(Rational a, Rational b, Rational c) => Max(a, Max(b, c));    //good enough

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Max(BigRational[])"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Max(LongRational[])"/>
        #endif
        public static Rational Max(params Rational[] values) => values.Aggregate(Max);

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Min(BigRational, BigRational)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Min(LongRational, LongRational)"/>
        #endif
        public static Rational Min(Rational a, Rational b) => a > b ? b : a;

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Min(BigRational, BigRational, BigRational)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Min(LongRational, LongRational, LongRational)"/>
        #endif
        public static Rational Min(Rational a, Rational b, Rational c) => Min(a, Min(b, c));
        
        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Min(BigRational[])"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Min(LongRational[])"/>
        #endif
        public static Rational Min(params Rational[] values) => values.Aggregate(Min);

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
        public static explicit operator SByte(Rational value)
        {
            return (SByte)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator UInt16(Rational value)
        {
            return (UInt16)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator UInt32(Rational value)
        {
            return (UInt32)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator UInt64(Rational value)
        {
            return (UInt64)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator Byte(Rational value)
        {
            return (Byte)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator Int16(Rational value)
        {
            return (Int16)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator Int32(Rational value)
        {
            return (Int32)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator Int64(Rational value)
        {
            return (Int64)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator BigInteger(Rational value)
        {
            return BigInteger.Divide(value.Numerator, value.Denominator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
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
        public static explicit operator Decimal(Rational value)
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
        #endregion explicit conversions from Rational

        #region implicit conversions to Rational

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(SByte value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(UInt16 value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(UInt32 value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(UInt64 value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(Byte value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(Int16 value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(Int32 value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(Int64 value)
        {
            #if BIG_RATIONAL
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        #if BIG_RATIONAL
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Rational(BigInteger value)
        {
            return new Rational(value);
        }
        #endif

        /*
         * This operators are commented out as they're unreliable
         * due to the BigRational(double) constructor
         * 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigRational(Single value)
        {
            return new BigRational((Double)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigRational(Double value)
        {
            return new BigRational(value);
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
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
                #if BIG_RATIONAL
                Denominator = BigInteger.One;
                #elif LONG_RATIONAL
                Denominator = 1;
                #endif
            }

            #if BIG_RATIONAL
            BigInteger gcd = BigInteger.GreatestCommonDivisor(Numerator, Denominator);
            #elif LONG_RATIONAL
            long gcd = GreatestCommonDivisor(Numerator, Denominator);
            #endif
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