//This file defines struct Rational with either BigRational or LongRational implementation
//Used to switch between the two at compile time without replacing most of the code

//Needs however to manually port over numerical implementations 
//Probably not the best solution

using System;
using System.Collections.Generic;
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
    [Newtonsoft.Json.JsonConverter(typeof(RationalNewtonsoftJsonConverter))]
    public struct Rational : IComparable, IComparable<Rational>, IEquatable<Rational>, IStableHashCode, IToCodeString, IToMppgString
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
            => Denominator.IsZero && Numerator == -1;
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
            return new Rational(BigInteger.Remainder(Numerator, Denominator), Denominator);
            #elif LONG_RATIONAL
            return new Rational(Numerator % Denominator, Denominator);
            #endif
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Floor" />
        public readonly BigInteger Floor()
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Floor" />
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

        #if BIG_RATIONAL
        /// <inheritdoc cref="BigRational.Ceil" />
        public readonly BigInteger Ceil()
        #elif LONG_RATIONAL
        /// <inheritdoc cref="LongRational.Ceil" />
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
            if (!(obj is Rational))
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
        // a/b = c/d, iff ad = bc
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

                Numerator = numerator < 0 ? BigInteger.MinusOne : BigInteger.One;
                Denominator = BigInteger.Zero;
                return;
            }

            if (numerator == 0)
            {
                Numerator = BigInteger.Zero;
                Denominator = BigInteger.One;
                return;
            }

            if (denominator == 1)
            {
                Numerator = new BigInteger(numerator);
                Denominator = BigInteger.One;
                return;
            }

            // These values cannot always be sign-normalized within Int64.
            if (numerator == long.MinValue || denominator == long.MinValue)
            {
                this = new Rational(new BigInteger(numerator), new BigInteger(denominator));
                return;
            }

            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            var divisor = GreatestCommonDivisor(numerator, denominator);
            var numeratorReduced = numerator / divisor;
            var denominatorReduced = denominator / divisor;

            Numerator = new BigInteger(numeratorReduced);
            Denominator = new BigInteger(denominatorReduced);
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
            var (numerator, denominator) = value.GetRationalParts();
            AssignRationalParts(
                numerator,
                denominator,
                nameof(Double),
                value.ToString("G", CultureInfo.InvariantCulture)
            );
        }

        #if BIG_RATIONAL
        [System.Runtime.CompilerServices.UnsafeAccessor(
            System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_sign")]
        private static extern ref int InlineSign(ref BigInteger value);

        /// <summary>
        /// Computes the greatest common divisor of two positive native magnitudes.
        /// </summary>
        /// <remarks>The caller must supply strictly positive arguments; no validation is performed.</remarks>
        private static long GreatestCommonDivisorPositive(long left, long right)
        {
            ulong a = (ulong)left, b = (ulong)right;
            int shift = BitOperations.TrailingZeroCount(a | b);
            a >>= BitOperations.TrailingZeroCount(a);
            do
            {
                b >>= BitOperations.TrailingZeroCount(b);
                if (a > b)
                    (a, b) = (b, a);
                b -= a;
            } while (b != 0);
            return (long)(a << shift);
        }

        #endif

        /// <summary>
        /// Constructs a rational from the shortest round-trippable decimal representation of the given float.
        /// </summary>
        public Rational(Single value)
        {
            var (numerator, denominator) = value.GetRationalParts();
            AssignRationalParts(
                numerator,
                denominator,
                nameof(Single),
                value.ToString("G", CultureInfo.InvariantCulture)
            );
        }
        
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
            a = Abs(a);
            b = Abs(b);
            return (a / Rational.GreatestCommonDivisor(a, b)) * b;
        }

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
        
            if(right.IsInfinite)
                return -right.Sign;

            // Fast sign path
            int signLeft = left.Numerator.Sign;
            int signRight = right.Numerator.Sign;
            if (signLeft != signRight) return Math.Sign(signLeft.CompareTo(signRight));
            if (signLeft == 0) return 0; // both zero if signs equal and s1==0

            // Fast same-denominator path
            if (left.Denominator == right.Denominator)
                return Math.Sign(left.Numerator.CompareTo(right.Numerator));

            // Need to compare fractions with the same sign.
        
            // Fast log2 comparison
            // log2(left) is in ] log2(left_num) - log2(right_num) -1, log2(left_num) - log2(right_num) +1 [
            // log2(right) is in ] log2(right_num) - log2(right_num) -1, log2(right_num) - log2(right_num) +1 [
            // Therefore, if the above intervals do not overlap we can conclude the comparison  
            var leftLen = left.Numerator.GetBitLength() - left.Denominator.GetBitLength();
            var rightLen = right.Numerator.GetBitLength() - right.Denominator.GetBitLength();
            if (leftLen >= rightLen + 2)
                return signLeft;
            if (rightLen >= leftLen + 2)
                return -signLeft;

            // Fast comparison failed, fall back to multiplication comparison
            // We do not do any simplification because the cost of the GCDs and divisions would be larger than what we can save in the multiplication 
            return Math.Sign(BigInteger.Compare(left.Numerator * right.Denominator, right.Numerator * left.Denominator));
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
        public static Rational Max(Rational a, Rational b, Rational c) => Max(a, Max(b, c));    //good enough

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
            // Shortcut common comparisons to infinite
            if (y.IsPlusInfinite)
                return x.IsPlusInfinite;
            if (y.IsMinusInfinite)
                return x.IsMinusInfinite;

            return x.Numerator == y.Numerator && x.Denominator == y.Denominator;
        }

        /// <inheritdoc />
        public static bool operator !=(Rational x, Rational y)
        {
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
            if (r == PlusInfinity)
                return MinusInfinity;
            if (r == MinusInfinity)
                return PlusInfinity;

            #if BIG_RATIONAL
            return new Rational(-r.Numerator, r.Denominator, skipNormalize: !r.Denominator.IsZero);
            #else
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
                #if BIG_RATIONAL
                // LCD via cross-cancel: a/b + c/d = (a*d' + c*b') / (g*b'*d')
                // where g = gcd(b, d), b' = b/g, d' = d/g
                var g = BigInteger.GreatestCommonDivisor(r1.Denominator, r2.Denominator);
                if (!g.IsOne)
                {
                    var bPrime = r1.Denominator / g;
                    var dPrime = r2.Denominator / g;
                    var num = r1.Numerator * dPrime + r2.Numerator * bPrime;
                    var den = bPrime * dPrime * g;
                    // second-level GCD: unlike mul/div, addition can retain common factors in the LCD result
                    var h = BigInteger.GreatestCommonDivisor(num, den);
                    if (!h.IsOne) { num /= h; den /= h; }
                    return new Rational(num, den, true);
                }
                else
                {
                    return new Rational(
                        r1.Numerator * r2.Denominator + r2.Numerator * r1.Denominator,
                        r1.Denominator * r2.Denominator
                    );
                }
                #elif LONG_RATIONAL
                // a/b + c/d  == (ad + bc)/bd
                return new Rational((r1.Numerator * r2.Denominator) + (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
                #endif
            }
        }

        /// <inheritdoc />
        public static Rational operator -(Rational r1, Rational r2)
        {
            return r1 + (-r2);
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
                    int xSign = InlineSign(ref xNumerator), ySign = InlineSign(ref yNumerator);
                    // A sign field other than ±1 holds the value itself, an Int32 other than ±1, and two of them multiply in an Int64.
                    if (xSign != 1 && xSign != -1 && ySign != 1 && ySign != -1)
                        return new Rational((long)xSign * ySign, 1, skipNormalize: true);
                    // Otherwise a numerator is ±1 or outside the sign field, and continues with the unit tests.
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

            // Fractions go to non-inlined helpers, so that zero and integer products do not pay for their frames.
            // This hotspot is an approved exception to the single-use helper rule.
            if (IsInSignField(x.Numerator) && IsInSignField(x.Denominator) && IsInSignField(y.Numerator) && IsInSignField(y.Denominator))
            {
                var xn = x.Numerator;
                var xd = x.Denominator;
                var yn = y.Numerator;
                var yd = y.Denominator;
                return MultiplyInt32Fractions(InlineSign(ref xn), InlineSign(ref xd), InlineSign(ref yn), InlineSign(ref yd));
            }
            else
                return MultiplyLargeFractions(x, y);
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

        #if BIG_RATIONAL

        // Whether BigInteger keeps the value in its Int32 _sign field rather than in an array.
        // It does for every Int32 except Int32.MinValue; a stored ±1 may instead be the sign of an array-backed value.
        private static bool IsInSignField(BigInteger value)
        {
            int sign = InlineSign(ref value);
            return sign == 1 ? value.IsOne : sign != -1 || value == BigInteger.MinusOne;
        }

        // The three fraction tiers below compute (a/b) * (c/d) as ((a/g1) * (c/g2)) / ((b/g2) * (d/g1)), with g1 = gcd(a, d) and g2 = gcd(c, b).
        // Reduced operands with equal denominators or equal numerator magnitudes have both gcds equal to 1, so each tier skips the cancellation for them.

        // Multiplies the reduced fractions a/b and c/d into a reduced fraction, with the products taken in Int64.
        // The components are Int32 values other than Int32.MinValue.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static Rational MultiplyInt32Fractions(Int32 a, Int32 b, Int32 c, Int32 d)
        {
            if (b != d && a != c && a != -c)
            {
                CancelPair(ref a, ref d);
                CancelPair(ref c, ref b);
            }
            return new Rational((Int64)a * c, (Int64)b * d, skipNormalize: true);
        }

        // Multiplies the reduced fractions a/b and c/d into a reduced fraction, with the products taken in 128 bits and stored as Int64 when they fit.
        // The components are Int64 values other than Int64.MinValue.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
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

        // Multiplies two reduced fractions into a reduced fraction: ±1/b times an integer through ProductFromOnePair, components of at most 63 bits through MultiplyInt64Fractions, the rest by cancelling in BigInteger.
        // At least one component is outside the sign field.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static Rational MultiplyLargeFractions(Rational x, Rational y)
        {
            // ±1/b times an integer c: one gcd, and on a miss both operands are reused.
            if (y.Denominator.IsOne && (x.Numerator.IsOne || x.Numerator == BigInteger.MinusOne))
                return ProductFromOnePair(x.Numerator.IsOne ? y.Numerator : -y.Numerator, x.Denominator);
            else if (x.Denominator.IsOne && (y.Numerator.IsOne || y.Numerator == BigInteger.MinusOne))
                return ProductFromOnePair(y.Numerator.IsOne ? x.Numerator : -x.Numerator, y.Denominator);
            else if (x.Numerator.GetBitLength() <= 63 && x.Denominator.GetBitLength() <= 63 &&
                y.Numerator.GetBitLength() <= 63 && y.Denominator.GetBitLength() <= 63)
            {
                Int64 xn = (Int64)x.Numerator, yn = (Int64)y.Numerator;
                // Int64.MinValue has no positive counterpart, so it goes to the BigInteger branch.
                if (xn != Int64.MinValue && yn != Int64.MinValue)
                    return MultiplyInt64Fractions(xn, (Int64)x.Denominator, yn, (Int64)y.Denominator);
                // Otherwise it continues with the BigInteger branch below.
            }
            // Otherwise the components are beyond Int64, and continue with the BigInteger branch.
            BigInteger a = x.Numerator, b = x.Denominator, c = y.Numerator, d = y.Denominator;
            if (b == d || BigInteger.Abs(a) == BigInteger.Abs(c))
                return new Rational(a * c, b * d, skipNormalize: true);
            else
            {
                // The pairs are written out here: a BigInteger CancelPair measured 5–7 ns slower on small-by-wide operands.
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
            if (BigInteger.Abs(numerator).GetBitLength() <= 63 && denominator.GetBitLength() <= 63)
            {
                long n = (long)numerator, d = (long)denominator, originalDenominator = d;
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

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void CancelPair(ref int numerator, ref int denominator)
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
                    // Inline storage excludes int.MinValue, so the magnitude fits.
                    uint g = (uint)Math.Abs(numerator), remainder = (uint)denominator;
                    while (remainder != 0)
                        (g, remainder) = (remainder, g % remainder);
                    if (g != 1)
                    {
                        numerator /= (int)g;
                        denominator /= (int)g;
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void CancelPair(ref long numerator, ref long denominator)
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
                    long g = GreatestCommonDivisorPositive(Math.Abs(numerator), denominator);
                    if (g != 1)
                    {
                        numerator /= g;
                        denominator /= g;
                    }
                }
            }
        }

        #endif

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

            if (y.IsZero)
            {
                throw new DivideByZeroException();
            }

            if (x.IsInfinite)
            {
                if (y.IsInfinite)
                    throw new UndeterminedResultException("Infinity over infinity");
                else
                    return y.Numerator.Sign > 0 ? x : -x;
            }

            if (y.IsInfinite)
                return Rational.Zero;

            // Otherwise both operands are finite and nonzero, and x/y is x times the reciprocal of y = c/d, ±d/|c|, which is reduced.
            // The reciprocal is an integer when c is ±1, so each operand's units are tested once as in the multiplication: a dividend ±1 yields ±1/y, a divisor ±1 yields ±x, and an integer divided by ±1/d is the integer ±x·d.
            if (x.Denominator.IsOne)
            {
                if (y.Numerator.IsOne || y.Numerator == BigInteger.MinusOne)
                {
                    var xNumerator = x.Numerator;
                    var yDenominator = y.Denominator;
                    int xSign = InlineSign(ref xNumerator), dSign = InlineSign(ref yDenominator);
                    // A sign field other than ±1 holds the value itself, an Int32 other than ±1, and two of them multiply in an Int64.
                    if (xSign != 1 && xSign != -1 && dSign != 1 && dSign != -1)
                        return new Rational(y.Numerator.Sign * (long)xSign * dSign, 1, skipNormalize: true);
                    // Otherwise a value is ±1 or outside the sign field, and continues with the unit tests.
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

            // The reciprocal of c/d is ±d/|c|, reduced, and the multiplication tiers cancel x against it.
            if (IsInSignField(x.Numerator) && IsInSignField(x.Denominator) && IsInSignField(y.Numerator) && IsInSignField(y.Denominator))
            {
                var xn = x.Numerator;
                var xd = x.Denominator;
                var yn = y.Numerator;
                var yd = y.Denominator;
                int c = InlineSign(ref yn), d = InlineSign(ref yd);
                return MultiplyInt32Fractions(InlineSign(ref xn), InlineSign(ref xd), c < 0 ? -d : d, Math.Abs(c));
            }
            else
                return MultiplyLargeFractions(x, Invert(y));
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
        public static Rational operator %(Rational r1, Rational r2)
        {
            if (r1.IsInfinite || r2.IsInfinite)
                throw new UndeterminedResultException("Modulus with an infinite rational involved is undetermined.");

            // a/b % c/d  == (ad % bc)/bd
            return new Rational((r1.Numerator * r2.Denominator) % (r1.Denominator * r2.Numerator), (r1.Denominator * r2.Denominator));
        }
        #endregion Operator Overloads

        #region explicit conversions from Rational

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.SByte" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.SByte" />
        #endif
        public static explicit operator SByte(Rational value)
        {
            return (SByte)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.UInt16" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.UInt16" />
        #endif
        public static explicit operator UInt16(Rational value)
        {
            return (UInt16)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.UInt32" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.UInt32" />
        #endif
        public static explicit operator UInt32(Rational value)
        {
            return (UInt32)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.UInt64" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.UInt64" />
        #endif
        public static explicit operator UInt64(Rational value)
        {
            return (UInt64)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Byte" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Byte" />
        #endif
        public static explicit operator Byte(Rational value)
        {
            return (Byte)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Int16" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Int16" />
        #endif
        public static explicit operator Int16(Rational value)
        {
            return (Int16)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Int32" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Int32" />
        #endif
        public static explicit operator Int32(Rational value)
        {
            return (Int32)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Int64" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Int64" />
        #endif
        public static explicit operator Int64(Rational value)
        {
            return (Int64)(BigInteger.Divide(value.Numerator, value.Denominator));
        }

        #if BIG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.BigRational.op_Explicit(Unipi.Nancy.Numerics.BigRational)~System.Numerics.BigInteger" />
        #elif LONG_RATIONAL
        /// <inheritdoc cref="M:Unipi.Nancy.Numerics.LongRational.op_Explicit(Unipi.Nancy.Numerics.LongRational)~System.Numerics.BigInteger" />
        #endif
        public static explicit operator BigInteger(Rational value)
        {
            return BigInteger.Divide(value.Numerator, value.Denominator);
        }

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
            var _value = (BigInteger)value;
            #elif LONG_RATIONAL
            var _value = (long)value;
            #endif
            return new Rational(_value);
        }

        /// <summary>
        /// Converts a <see cref="UInt16"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// Converts a <see cref="UInt32"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// Converts a <see cref="UInt64"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// Converts a <see cref="Byte"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// Converts a <see cref="Int16"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// Converts a <see cref="Int32"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// Converts a <see cref="Int64"/> to a <see cref="Rational"/>.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
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
        /// This should be called during construction, existing Rational are assumed to be irreducible.
        /// </summary>
        private void Simplify()
        {
            #if BIG_RATIONAL
            // * if the numerator is {0, +1, -1} then the fraction is already reduced
            // * if the denominator is {+1} then the fraction is already reduced
            if (Numerator.IsZero)
                Denominator = BigInteger.One;
            else if (Numerator.IsOne || Numerator == -1)
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

        private void AssignRationalParts(BigInteger numerator, BigInteger denominator, string valueType, string valueText)
        {
            #if BIG_RATIONAL
            Numerator = numerator;
            Denominator = denominator;
            #elif LONG_RATIONAL
            if (!SafeCastToInt64(numerator) || !SafeCastToInt64(denominator))
                throw new OverflowException($"{valueType} {valueText} cannot be represented exactly as LongRational.");

            Numerator = (long) numerator;
            Denominator = (long) denominator;
            #endif

            Simplify();
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

        private static bool SafeCastToInt64(BigInteger value)
        {
            return long.MinValue <= value && value <= long.MaxValue;
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
