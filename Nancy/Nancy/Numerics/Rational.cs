//This file defines struct Rational with either BigRational or LongRational implementation
//Used to switch between the two at compile time without replacing most of the code

//Needs however to manually port over numerical implementations
//Probably not the best solution

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    [Newtonsoft.Json.JsonConverter(typeof(RationalNewtonsoftJsonConverter))]
    public struct Rational : IComparable, IComparable<Rational>, IEquatable<Rational>, IToCodeString, IStableHashCode, IToMppgString
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

        #endregion Static public values

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

        /// <summary>
        /// The numerator of the rational.
        /// </summary>
        /// <remarks>
        /// This property encodes infinity as well as finite values: check <see cref="IsPlusInfinite"/>/<see cref="IsMinusInfinite"/> before using it, or read the pair through <see cref="TryGetFinite"/>, which forces the check.
        /// </remarks>
        [JsonProperty(PropertyName = "num")]
        [JsonPropertyName("num")]
        [System.Text.Json.Serialization.JsonConverter(typeof(BigIntegerSystemJsonConverter))]
        #if BIG_RATIONAL
        public BigInteger Numerator { get; private set; }
        #elif LONG_RATIONAL
        public long Numerator { get; private set; }
        #endif

        /// <summary>
        /// The denominator of the rational.
        /// </summary>
        /// <remarks>
        /// This property encodes infinity as well as finite values: check <see cref="IsPlusInfinite"/>/<see cref="IsMinusInfinite"/> before using it, or read the pair through <see cref="TryGetFinite"/>, which forces the check.
        /// </remarks>
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
        public readonly bool IsFinite
        #if BIG_RATIONAL
            => !Denominator.IsZero;
        #elif LONG_RATIONAL
            => Denominator != 0;
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsInfinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsInfinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsInfinite
        #if BIG_RATIONAL
            => Denominator.IsZero;
        #elif LONG_RATIONAL
            => Denominator == 0;
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsPlusInfinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsPlusInfinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsPlusInfinite
        #if BIG_RATIONAL
            => Denominator.IsZero && Numerator.IsOne;
        #elif LONG_RATIONAL
            => Denominator == 0 && Numerator == 1;
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsMinusInfinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsMinusInfinite"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsMinusInfinite
        #if BIG_RATIONAL
            => Denominator.IsZero && Numerator == BigInteger.MinusOne;
        #elif LONG_RATIONAL
            => Denominator == 0 && Numerator == -1;
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.TryGetFinite"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.TryGetFinite"/>
        #endif
        #if BIG_RATIONAL
        public readonly bool TryGetFinite(out BigInteger numerator, out BigInteger denominator)
        #elif LONG_RATIONAL
        public readonly bool TryGetFinite(out long numerator, out long denominator)
        #endif
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsZero"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsZero"/>
        #endif
        [System.Text.Json.Serialization.JsonIgnore]
        public readonly bool IsZero
        #if BIG_RATIONAL
            => Numerator.IsZero;
        #elif LONG_RATIONAL
            => Numerator == 0;
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.IsOne"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.IsOne"/>
        #endif
        public readonly bool IsOne
        #if BIG_RATIONAL
            => Denominator.IsOne && Numerator.IsOne;
        #elif LONG_RATIONAL
            => Denominator == 1 && Numerator == 1;
        #endif

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
        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.GetWholePart" />
        public readonly BigInteger GetWholePart()
        {
            if (IsInfinite)
                throw new UndeterminedResultException("Cannot get whole part of an infinite value");
            return BigInteger.Divide(Numerator, Denominator);
        }
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.GetWholePart" />
        public readonly long GetWholePart()
        {
            if (IsInfinite)
                throw new UndeterminedResultException("Cannot get whole part of an infinite value");
            return Numerator / Denominator;
        }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.GetFractionPart" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.GetFractionPart" />
        #endif
        public readonly Rational GetFractionPart()
        {
            if (IsInfinite)
                throw new UndeterminedResultException("Cannot get fraction part of an infinite value");
            #if BIG_RATIONAL
            // The remainder has the numerator's sign, and gcd(n mod d, d) = gcd(n, d) = 1, so only a zero remainder is not canonical as it is.
            var remainder = BigInteger.Remainder(Numerator, Denominator);
            if (remainder.IsZero)
                return Rational.Zero;
            else
                return new Rational(remainder, Denominator, skipNormalize: true);
            #elif LONG_RATIONAL
            return new Rational(Numerator % Denominator, Denominator);
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Floor" />
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
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Floor" />
        public readonly long Floor()
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
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Ceil" />
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
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Ceil" />
        public readonly long Ceil()
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
        #endif

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
        /// <inheritdoc cref="CompareTo(Rational)" path="/remarks"/>
        readonly int IComparable.CompareTo(object? obj)
        {
            if (obj == null)
                return 1;
            if (obj is not Rational)
                throw new ArgumentException("Argument must be of type Rational", nameof(obj));
            return Compare(this, (Rational)obj);
        }

        // IComparable<Rational>
        /// <inheritdoc />
        /// <remarks>
        /// Stronger than the interface requires: the result is exactly $-1$, $0$ or $1$, never another value of the same sign.
        /// Callers may therefore use it as a sign, rather than only testing it against zero.
        /// </remarks>
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

        /// <summary>
        /// Return a string containing code to create an equivalent of this Rational in a (min,+) playground.
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

        // IEquatable<Rational>
        /// <inheritdoc />
        public readonly Boolean Equals(Rational other)
        {
            // values are normalized, so equal values have the same numerator and denominator
            return Numerator == other.Numerator && Denominator == other.Denominator;
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
            #if BIG_RATIONAL
            this = new Rational((Int64)numerator, denominator);
            #elif LONG_RATIONAL
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
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(long, long)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational(long, long)"/>
        #endif
        public Rational(long numerator, long denominator = 1)
        {
            #if BIG_RATIONAL
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
                this = new Rational(new BigInteger(numerator), new BigInteger(denominator));
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
            #elif LONG_RATIONAL
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
            #endif
        }

        #if BIG_RATIONAL
        /// <summary>
        /// Constructs a rational from Int64 parts, optionally skipping normalization.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <param name="skipNormalize">
        /// True when the numerator and denominator are already canonical.
        /// </param>
        internal Rational(long numerator, long denominator, bool skipNormalize)
        {
            if (skipNormalize)
            {
                Numerator = new BigInteger(numerator);
                Denominator = new BigInteger(denominator);
            }
            else
            {
                this = new Rational(numerator, denominator);
            }
        }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(BigInteger)"/>
        public Rational(BigInteger numerator)
        {
            Numerator = numerator;
            Denominator = BigInteger.One;
        }
        #endif

        /// <summary>
        /// Constructs a rational from the shortest round-trippable decimal representation of the given double.
        /// </summary>
        public Rational(Double value)
        {
            #if BIG_RATIONAL
            (Numerator, Denominator) = value.GetRationalParts();
            Simplify();
            #elif LONG_RATIONAL
            var (numerator, denominator) = value.GetRationalParts();
            AssignRationalParts(
                numerator,
                denominator,
                nameof(Double),
                value.ToString("G", CultureInfo.InvariantCulture)
            );
            #endif
        }

        /// <summary>
        /// Constructs a rational from the shortest round-trippable decimal representation of the given float.
        /// </summary>
        public Rational(Single value)
        {
            #if BIG_RATIONAL
            (Numerator, Denominator) = value.GetRationalParts();
            Simplify();
            #elif LONG_RATIONAL
            var (numerator, denominator) = value.GetRationalParts();
            AssignRationalParts(
                numerator,
                denominator,
                nameof(Single),
                value.ToString("G", CultureInfo.InvariantCulture)
            );
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational(decimal)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational(decimal)"/>
        #endif
        public Rational(Decimal value)
        {
            #if BIG_RATIONAL
            if (value == Decimal.Zero)
            {
                this = Rational.Zero;
                return;
            }

            (Numerator, Denominator) = value.GetRationalParts();
            Simplify();
            #elif LONG_RATIONAL
            var (numerator, denominator) = value.GetRationalParts();
            var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
            if (gcd > 1) {
                numerator /= gcd;
                denominator /= gcd;
            }

            if(numerator.GetByteCount() > sizeof(long) ||  denominator.GetByteCount() > sizeof(long))
                throw new OverflowException($"Decimal {value} cannot be represented exactly as LongRational.");

            Numerator = (long) numerator;
            Denominator = (long) denominator;
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
        /// <inheritdoc cref="BigRational(BigInteger, BigInteger, bool)"/>
        internal Rational(BigInteger numerator, BigInteger denominator, bool skipNormalize)
        {
            if (skipNormalize)
            {
                Numerator = numerator;
                Denominator = denominator;
            }
            else
            {
                this = new Rational(numerator, denominator);
            }
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
                // whole + n/d = (whole·d + n)/d = (whole·|d| - n)/|d|.
                Denominator = -denominator;
                Numerator = (whole * Denominator) - numerator;
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
            return (r.Numerator.Sign < 0 ? new Rational(BigInteger.Abs(r.Numerator), r.Denominator, skipNormalize: true) : r);
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
            #if BIG_RATIONAL
            return new Rational(-r.Numerator, r.Denominator, skipNormalize: !r.Denominator.IsZero);
            #else
            return new Rational(-r.Numerator, r.Denominator);
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Invert"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Invert"/>
        #endif
        public static Rational Invert(Rational r)
        {
            #if BIG_RATIONAL
            var sign = r.Numerator.Sign;
            if (sign > 0)
                return new Rational(r.Denominator, r.Numerator, skipNormalize: true);
            else if (sign < 0)
                return new Rational(-r.Denominator, -r.Numerator, skipNormalize: true);
            else
                return new Rational(r.Denominator, r.Numerator);
            #else
            return new Rational(r.Denominator, r.Numerator);
            #endif
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
            if (dividend.IsInfinite || divisor.IsInfinite)
                throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");
            if (divisor == Rational.Zero)
                throw new DivideByZeroException("Divisor must be non-zero.");

            // a/b / c/d  == (ad)/(bc)
            // a/b % c/d  == (ad % bc)/bd

            // (ad) and (bc) need to be calculated for both the division and the remainder operations.
            var ad = dividend.Numerator * divisor.Denominator;
            var bc = dividend.Denominator * divisor.Numerator;
            var bd = dividend.Denominator * divisor.Denominator;

            remainder = new Rational(ad % bc, bd);
            return new Rational(ad / bc);
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

            #if BIG_RATIONAL
            if (exponent.IsOne)
                return baseValue;
            // The product x·x shares its denominator, so the multiplication skips cancelling, and two BigInteger powers cost more.
            else if (exponent == 2)
                return baseValue * baseValue;
            // From the cube on, the powers of a reduced numerator and denominator are coprime, and the denominator stays positive or zero.
            else if (exponent <= Int32.MaxValue)
                return new Rational(BigInteger.Pow(baseValue.Numerator, (Int32)exponent), BigInteger.Pow(baseValue.Denominator, (Int32)exponent), skipNormalize: true);
            // BigInteger.Pow takes an Int32 exponent, so larger ones repeat the multiplication.
            else
            {
                Rational result = baseValue;
                while (exponent > BigInteger.One)
                {
                    result = result * baseValue;
                    exponent--;
                }
                return result;
            }
            #elif LONG_RATIONAL
            Rational result = baseValue;
            while (exponent > BigInteger.One)
            {
                result = result * baseValue;
                exponent--;
            }

            return result;
            #endif
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
        public static Rational GreatestCommonDivisor(Rational a, Rational b)
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
                return new Rational(numerator, denominator, skipNormalize: true);
            }
        }
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.GreatestCommonDivisor(LongRational, LongRational)"/>
        public static Rational GreatestCommonDivisor(Rational a, Rational b)
        {
            a = Abs(a);
            b = Abs(b);
            while (b != 0)
            {
                Rational temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.GreatestCommonDivisor(long, long)"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.GreatestCommonDivisor(long, long)"/>
        #endif
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.LeastCommonMultiple"/>
        public static Rational LeastCommonMultiple(Rational a, Rational b)
        {
            if (a.IsInfinite || b.IsInfinite)
                throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");
            else if (a.IsZero && b.IsZero)
                throw new UndeterminedResultException("Zero over zero");
            else if (a.IsZero || b.IsZero)
                return Rational.Zero;
            else
            {
                // For reduced p/q and r/s, the smallest rational that both divide into integers is lcm(p, r) / gcd(q, s), which is reduced.
                var numerator = BigInteger.Abs(a.Numerator) / BigInteger.GreatestCommonDivisor(a.Numerator, b.Numerator) * BigInteger.Abs(b.Numerator);
                var denominator = BigInteger.GreatestCommonDivisor(a.Denominator, b.Denominator);
                return new Rational(numerator, denominator, skipNormalize: true);
            }
        }
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.LeastCommonMultiple"/>
        public static Rational LeastCommonMultiple(Rational a, Rational b)
        {
            a = Abs(a);
            b = Abs(b);
            return (a / Rational.GreatestCommonDivisor(a, b)) * b;
        }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Compare"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Compare"/>
        #endif
        public static int Compare(Rational left, Rational right)
        {
            #if BIG_RATIONAL
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
            #elif LONG_RATIONAL
            if (left.IsInfinite || right.IsInfinite)
            {
                if (left.IsInfinite && right.IsInfinite)
                    return intCompare(left.Sign, right.Sign);
                else
                {
                    //An infinite value is always bigger in absolute value
                    if (left.IsInfinite)
                    {
                        return intCompare(2*left.Sign, right.Sign);
                    }
                    else
                    {
                        return intCompare(left.Sign, 2*right.Sign);
                    }
                }
            }
            else
            {
                // a/b >= c/d, iff ad >= bc
                return Math.Sign(BigInteger.Compare(left.Numerator * right.Denominator, right.Numerator * left.Denominator));
            }

            int intCompare(int left, int right)
            {
                return Math.Sign(left - right);
            }
            #endif
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
        public static Rational Max(Rational a, Rational b, Rational c) => Max(a, Max(b, c));

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Max(BigRational[])"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Max(LongRational[])"/>
        #endif
        public static Rational Max(params Rational[] values) => values.Aggregate(Max);

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Max(IEnumerable{BigRational})"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Max(IEnumerable{LongRational})"/>
        #endif
        public static Rational Max(IEnumerable<Rational> values) => values.Aggregate(Max);

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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Min(IEnumerable{BigRational})"/>
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Min(IEnumerable{LongRational})"/>
        #endif
        public static Rational Min(IEnumerable<Rational> values) => values.Aggregate(Min);

        #endregion Public Static Methods

        #region Operator Overloads

        /// <inheritdoc />
        public static bool operator ==(Rational x, Rational y)
        {
            // Values are canonical, infinities included, so equal values have the same numerator and denominator.
            return x.Numerator == y.Numerator && x.Denominator == y.Denominator;
        }

        /// <inheritdoc />
        public static bool operator !=(Rational x, Rational y)
        {
            // Values are canonical, infinities included, so different values differ in the numerator or the denominator.
            return x.Numerator != y.Numerator || x.Denominator != y.Denominator;
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
            #if BIG_RATIONAL
            return Negate(r);
            #else
            if (r == PlusInfinity)
                return MinusInfinity;
            if (r == MinusInfinity)
                return PlusInfinity;
            return new Rational(-r.Numerator, r.Denominator);
            #endif
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
        public static Rational operator +(Rational x, Rational y)
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
            #if BIG_RATIONAL
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
            #elif LONG_RATIONAL
            else
            {
                // a/b + c/d  == (ad + bc)/bd
                return new Rational((x.Numerator * y.Denominator) + (x.Denominator * y.Numerator), (x.Denominator * y.Denominator));
            }
            #endif
        }

        /// <inheritdoc />
        public static Rational operator -(Rational x, Rational y)
        {
            return x + (-y);
        }

        /// <inheritdoc cref="Multiply(Rational, Rational)"/>
        public static Rational operator *(Rational x, Rational y)
        {
            #if BIG_RATIONAL
            if (x.IsZero || y.IsZero)
                return Rational.Zero;
            else if (x.IsInfinite || y.IsInfinite)
                return x.Sign == y.Sign ? Rational.PlusInfinity : Rational.MinusInfinity;
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
                        return new Rational((Int64)xField * yField, 1, skipNormalize: true);
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
                    return new Rational(x.Numerator * y.Numerator);
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
            #elif LONG_RATIONAL
            if (x.IsZero || y.IsZero)
            {
                return Zero;
            }
            else if (x.IsInfinite || y.IsInfinite)
            {
                return x.Sign == y.Sign ? PlusInfinity : MinusInfinity;
            }
            else
            {
                // a/b * c/d  == (ac)/(bd)
                return new Rational((x.Numerator * y.Numerator), (x.Denominator * y.Denominator));
            }
            #endif
        }

        /// <inheritdoc cref="Divide(Rational, Rational)" />
        public static Rational operator /(Rational x, Rational y)
        {
            #if BIG_RATIONAL
            if (x.IsZero)
            {
                if (y.IsZero)
                    throw new UndeterminedResultException("Zero over zero");
                else
                    return Rational.Zero;
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
                return Rational.Zero;
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
                        return new Rational(y.Numerator.Sign * (Int64)xField * dField, 1, skipNormalize: true);
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
                    return new Rational(y.Numerator.IsOne ? x.Numerator * y.Denominator : -(x.Numerator * y.Denominator));
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
            #elif LONG_RATIONAL
            if (x.IsZero || y.IsZero)
            {
                if (x.IsZero && y.IsZero)
                {
                    throw new UndeterminedResultException("Zero over zero");
                }
                else if (x.IsZero)
                {
                    return Zero;
                }
                else
                {
                    throw new DivideByZeroException();
                }
            }
            else if (x.IsInfinite || y.IsInfinite)
            {
                if (x.IsInfinite && y.IsInfinite)
                {
                    throw new UndeterminedResultException("Infinity over infinity");
                }
                else if (x.IsInfinite)
                {
                    return x * y.Sign;
                }
                else
                {
                    return Zero;
                }
            }
            else
            {
                // a/b / c/d  == (ad)/(bc)
                return new Rational((x.Numerator * y.Denominator), (x.Denominator * y.Numerator));
            }
            #endif
        }

        /// <inheritdoc />
        public static Rational operator %(Rational x, Rational y)
        {
            if (x.IsInfinite || y.IsInfinite)
                throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");

            // a/b % c/d  == (ad % bc)/bd
            return new Rational((x.Numerator * y.Denominator) % (x.Denominator * y.Numerator), (x.Denominator * y.Denominator));
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

        #if BIG_RATIONAL
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
        private static Rational FromInt128(Int128 numerator, Int128 denominator)
        {
            if (numerator >= Int64.MinValue && numerator <= Int64.MaxValue && denominator <= Int64.MaxValue)
                return new Rational((Int64)numerator, (Int64)denominator, skipNormalize: true);
            else
                return new Rational((BigInteger)numerator, (BigInteger)denominator, skipNormalize: true);
        }

        // Adds the reduced fractions a/b and c/d into a reduced fraction, with the sums and products taken in Int64.
        // The components are Int32 values other than Int32.MinValue, and the denominators are positive.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Rational AddInt32Fractions(Int32 a, Int32 b, Int32 c, Int32 d)
        {
            // Equal denominators add the numerators, and only factors of b can cancel.
            if (b == d)
            {
                Int64 numerator = (Int64)a + c;
                if (numerator == 0)
                    return Rational.Zero;
                else if (b == 1)
                    return new Rational(numerator, 1, skipNormalize: true);
                else
                {
                    Int64 divisor = GreatestCommonDivisorPositive(Math.Abs(numerator), b);
                    return new Rational(numerator / divisor, b / divisor, skipNormalize: true);
                }
            }
            // An integer plus a reduced fraction is reduced: a + c/d = (a·d + c)/d, and c is coprime to d.
            else if (b == 1)
                return new Rational((Int64)a * d + c, d, skipNormalize: true);
            else if (d == 1)
                return new Rational(a + (Int64)c * b, b, skipNormalize: true);
            // Otherwise, with g = gcd(b, d), a/b + c/d = (a·(d/g) + c·(b/g)) / (b·d/g), and only factors of g can cancel.
            // The sum is nonzero: reduced fractions with unequal denominators are not opposite.
            else
            {
                Int64 g = GreatestCommonDivisorPositive(b, d);
                if (g == 1)
                    return new Rational((Int64)a * d + (Int64)c * b, (Int64)b * d, skipNormalize: true);
                else
                {
                    Int64 bReduced = b / g, dReduced = d / g;
                    Int64 numerator = a * dReduced + c * bReduced;
                    Int64 divisor = GreatestCommonDivisorPositive(Math.Abs(numerator), g);
                    return new Rational(numerator / divisor, bReduced * (d / divisor), skipNormalize: true);
                }
            }
        }

        // Adds the reduced fractions a/b and c/d into a reduced fraction, with 128-bit sums and products and Int64 gcds.
        // The components are Int64 values other than Int64.MinValue, and the denominators are positive.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Rational AddInt64Fractions(Int64 a, Int64 b, Int64 c, Int64 d)
        {
            // Equal denominators add the numerators, and only factors of b can cancel.
            if (b == d)
            {
                Int128 numerator = (Int128)a + c;
                if (numerator == 0)
                    return Rational.Zero;
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
        private static Rational AddLargeFractions(BigInteger a, BigInteger b, BigInteger c, BigInteger d)
        {
            if (TryGetInt64(a, out Int64 a64) && TryGetInt64(b, out Int64 b64) &&
                TryGetInt64(c, out Int64 c64) && TryGetInt64(d, out Int64 d64))
                return AddInt64Fractions(a64, b64, c64, d64);
            else if (b == d)
                return new Rational(a + c, b);
            else
            {
                // With g = gcd(b, d), a/b + c/d = (a·(d/g) + c·(b/g)) / (b·d/g), and only factors of g can cancel.
                var g = BigInteger.GreatestCommonDivisor(b, d);
                if (g.IsOne)
                    return new Rational(a * d + c * b, b * d, skipNormalize: true);
                else
                {
                    var bReduced = b / g;
                    var numerator = a * (d / g) + c * bReduced;
                    var divisor = BigInteger.GreatestCommonDivisor(numerator, g);
                    // The denominator b·d/g is bReduced·d, and dividing d rather than the product by the divisor keeps one multiplication.
                    if (divisor.IsOne)
                        return new Rational(numerator, bReduced * d, skipNormalize: true);
                    else
                        return new Rational(numerator / divisor, bReduced * (d / divisor), skipNormalize: true);
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
        private static Rational MultiplyInt32Fractions(Int32 a, Int32 b, Int32 c, Int32 d)
        {
            if (b != d && a != c && a != -c)
            {
                CancelPair(ref a, ref d);
                CancelPair(ref c, ref b);
            }
            return new Rational((Int64)a * c, (Int64)b * d, skipNormalize: true);
        }

        // Multiplies the reduced fractions a/b and c/d into a reduced fraction, with the products taken in 128 bits and built from Int64 values when they fit.
        // The components are Int64 values other than Int64.MinValue, and the denominators are positive.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Rational MultiplyInt64Fractions(Int64 a, Int64 b, Int64 c, Int64 d)
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
                return new Rational(numerator, denominator, skipNormalize: true);
            else
                return new Rational((BigInteger)new Int128(unchecked((UInt64)numeratorHigh), unchecked((UInt64)numerator)),
                    (BigInteger)new Int128(unchecked((UInt64)denominatorHigh), unchecked((UInt64)denominator)), skipNormalize: true);
        }

        // Multiplies the reduced fractions a/b and c/d into a reduced fraction: ±1 over a denominator times an integer through ProductFromOnePair, components that are Int64 values other than Int64.MinValue through MultiplyInt64Fractions, the rest by cancelling in BigInteger.
        // At least one component is outside the sign field, and the denominators are positive.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Rational MultiplyLargeFractions(BigInteger a, BigInteger b, BigInteger c, BigInteger d)
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
                return new Rational(a * c, b * d, skipNormalize: true);
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
                return new Rational(a.IsOne ? c : c.IsOne ? a : a * c, b.IsOne ? d : d.IsOne ? b : b * d, skipNormalize: true);
            }
        }

        // The product ±c/b of ±1/b and an integer c, with the sign already applied to c.
        // Below 2^63 the gcd is taken in Int64 and a miss reuses both operands; beyond, the normalizing constructor cancels.
        private static Rational ProductFromOnePair(BigInteger numerator, BigInteger denominator)
        {
            if (TryGetInt64(numerator, out Int64 n) && TryGetInt64(denominator, out Int64 d))
            {
                Int64 originalDenominator = d;
                CancelPair(ref n, ref d);
                if (d == originalDenominator)
                    return new Rational(numerator, denominator, skipNormalize: true);
                else
                    return new Rational(n, d, skipNormalize: true);
            }
            else
                return new Rational(numerator, denominator);
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
        #endif

        #endregion Arithmetic helpers

        #region explicit conversions from Rational

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.SByte" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.SByte" />
        #endif
        public static explicit operator SByte(Rational value)
        {
            #if BIG_RATIONAL
            return (SByte)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (SByte)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.UInt16" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.UInt16" />
        #endif
        public static explicit operator UInt16(Rational value)
        {
            #if BIG_RATIONAL
            return (UInt16)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (UInt16)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.UInt32" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.UInt32" />
        #endif
        public static explicit operator UInt32(Rational value)
        {
            #if BIG_RATIONAL
            return (UInt32)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (UInt32)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.UInt64" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.UInt64" />
        #endif
        public static explicit operator UInt64(Rational value)
        {
            #if BIG_RATIONAL
            return (UInt64)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (UInt64)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Byte" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Byte" />
        #endif
        public static explicit operator Byte(Rational value)
        {
            #if BIG_RATIONAL
            return (Byte)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (Byte)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Int16" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Int16" />
        #endif
        public static explicit operator Int16(Rational value)
        {
            #if BIG_RATIONAL
            return (Int16)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (Int16)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Int32" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Int32" />
        #endif
        public static explicit operator Int32(Rational value)
        {
            #if BIG_RATIONAL
            return (Int32)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (Int32)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Int64" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Int64" />
        #endif
        public static explicit operator Int64(Rational value)
        {
            #if BIG_RATIONAL
            return (Int64)TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return (Int64)(BigInteger.Divide(value.Numerator, value.Denominator));
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Numerics.BigInteger" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Numerics.BigInteger" />
        #endif
        public static explicit operator BigInteger(Rational value)
        {
            #if BIG_RATIONAL
            return TruncatedQuotient(value);
            #elif LONG_RATIONAL
            return BigInteger.Divide(value.Numerator, value.Denominator);
            #endif
        }

        #if BIG_RATIONAL
        // The quotient of the numerator by the denominator, rounded toward zero, in Int64 when both are Int64 values other than Int64.MinValue.
        private static BigInteger TruncatedQuotient(Rational value)
        {
            if (TryGetInt64(value.Numerator, out Int64 numerator) && TryGetInt64(value.Denominator, out Int64 denominator))
                return numerator / denominator;
            else
                return BigInteger.Divide(value.Numerator, value.Denominator);
        }
        #endif

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Single" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Single" />
        #endif
        public static explicit operator Single(Rational value)
        {
            // The Single value type represents a single-precision 32-bit number with
            // values ranging from negative 3.402823e38 to positive 3.402823e38
            // values that do not fit into this range are returned as Infinity
            return (Single)((Double)value);
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Double" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Double" />
        #endif
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Decimal" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Decimal" />
        #endif
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
        /// Converts a <see cref="SByte"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(SByte value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="UInt16"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(UInt16 value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="UInt32"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(UInt32 value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="UInt64"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(UInt64 value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="Byte"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(Byte value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="Int16"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(Int16 value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="Int32"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(Int32 value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        /// <summary>
        /// Converts a <see cref="Int64"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(Int64 value)
        {
            #if BIG_RATIONAL
            return new Rational((BigInteger)value);
            #elif LONG_RATIONAL
            return new Rational((long)value);
            #endif
        }

        #if BIG_RATIONAL
        /// <summary>
        /// Converts a <see cref="BigInteger"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Rational(BigInteger value)
        {
            return new Rational(value);
        }
        #endif

        /// <summary>
        /// Converts a single-precision floating-point number to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The floating-point value to convert.</param>
        public static implicit operator Rational(Single value)
        {
            return new Rational((Double)value);
        }

        /// <summary>
        /// Converts a double-precision floating-point number to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The floating-point value to convert.</param>
        public static implicit operator Rational(Double value)
        {
            return new Rational(value);
        }

        /// <summary>
        /// Converts a decimal number to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        public static implicit operator Rational(Decimal value)
        {
            return new Rational(value);
        }

        #endregion implicit conversions to Rational

        #region instance helper methods

        /// <summary>
        /// Simplifies the rational, making it irreducible.
        /// This should be called during construction, existing values are assumed to be irreducible.
        /// </summary>
        private void Simplify()
        {
            #if BIG_RATIONAL
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
            #elif LONG_RATIONAL
            // * if the numerator is {0, +1, -1} then the fraction is already reduced
            // * if the denominator is {+1} then the fraction is already reduced
            if (Numerator == 0)
                Denominator = 1;
            else if (Numerator is 1 or -1)
                return;
            else if (Denominator == 1)
                return;
            // early exit for identical parts
            else if (Math.Abs(Numerator) == Denominator)
            {
                Numerator = Numerator > 0 ? 1 : -1;
                Denominator = 1;
            }
            else
            {
                var gcd = GreatestCommonDivisor(Numerator, Denominator);
                if (gcd > BigInteger.One)
                {
                    Numerator /= gcd;
                    Denominator /= gcd;
                }
            }
            #endif
        }

        #if LONG_RATIONAL
        private void AssignRationalParts(BigInteger numerator, BigInteger denominator, string valueType, string valueText)
        {
            if (!SafeCastToInt64(numerator) || !SafeCastToInt64(denominator))
                throw new OverflowException($"{valueType} {valueText} cannot be represented exactly as LongRational.");

            Numerator = (long) numerator;
            Denominator = (long) denominator;
            Simplify();
        }
        #endif

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

        #if LONG_RATIONAL
        private static bool SafeCastToInt64(BigInteger value)
        {
            return long.MinValue <= value && value <= long.MaxValue;
        }
        #endif

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
