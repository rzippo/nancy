using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Unipi.Nancy.Utility;

/// <summary>
/// Set of extension methods, providing <see cref="IStableHashCode.GetStableHashCode"/> through system classes.
/// By "stable" we mean that the same object, constructed with the same values, will produce the same hash code
/// across different processes or executions.
/// By contrast, <see cref="object.GetHashCode"/> is not stable.
/// </summary>
public static class StableHashCodeExtensions
{
    #region Base types

    /// <summary>
    /// A stable hashcode.
    /// </summary>
    /// <remarks>Generic implementation for all objects. May not be stable.</remarks>
    public static int GetStableHashCode(this object o)
    {
        switch (o)
        {
            case string s:
                return GetStableHashCode(s);
            
            case IStableHashCode shc:
                return shc.GetStableHashCode();

            case System.Collections.IEnumerable ie:
                var hashes = ie
                    .OfType<object>()
                    .Select(GetStableHashCode);
                return HashStableCombine(hashes);

            // either known stable, or not explicitly implemented
            default:
                return o.GetHashCode();
        }
    }
    
    /// <summary>
    /// A stable hashcode.
    /// </summary>
    /// <remarks>The default implementation is already stable.</remarks>
    public static int GetStableHashCode(this long l)
        => l.GetHashCode();
    
    /// <summary>
    /// A stable hashcode.
    /// </summary>
    /// <remarks>The default implementation is already stable.</remarks>
    public static int GetStableHashCode(this BigInteger bi)
        => bi.GetHashCode();

    /// <summary>
    /// A stable hashcode.
    /// </summary>
    /// <remarks>The default implementation is NOT stable, as a security feature.</remarks>
    public static int GetStableHashCode(this string str)
    {
        // https://stackoverflow.com/a/36845864/8695112
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for(int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i+1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
            }

            return hash1 + (hash2*1566083941);
        }
    }

    #endregion Base types

    #region ValueTuple

    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1>(this ValueTuple<T1> tuple)
    {
        return tuple.Item1?.GetStableHashCode() ?? 0;
    }
    
    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2>(this ValueTuple<T1, T2> tuple)
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0
        );
    }
    
    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2, T3}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2, T3>(this ValueTuple<T1, T2, T3> tuple)
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0,
            tuple.Item3?.GetStableHashCode() ?? 0
        );
    }
    
    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2, T3, T4>(this ValueTuple<T1, T2, T3, T4> tuple)
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0,
            tuple.Item3?.GetStableHashCode() ?? 0,
            tuple.Item4?.GetStableHashCode() ?? 0
        );
    }
    
    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2, T3, T4, T5>(this ValueTuple<T1, T2, T3, T4, T5> tuple)
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0,
            tuple.Item3?.GetStableHashCode() ?? 0,
            tuple.Item4?.GetStableHashCode() ?? 0,
            tuple.Item5?.GetStableHashCode() ?? 0
        );
    }

    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2, T3, T4, T5, T6>(this ValueTuple<T1, T2, T3, T4, T5, T6> tuple)
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0,
            tuple.Item3?.GetStableHashCode() ?? 0,
            tuple.Item4?.GetStableHashCode() ?? 0,
            tuple.Item5?.GetStableHashCode() ?? 0,
            tuple.Item6?.GetStableHashCode() ?? 0
        );
    }

    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2, T3, T4, T5, T6, T7>(this ValueTuple<T1, T2, T3, T4, T5, T6, T7> tuple)
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0,
            tuple.Item3?.GetStableHashCode() ?? 0,
            tuple.Item4?.GetStableHashCode() ?? 0,
            tuple.Item5?.GetStableHashCode() ?? 0,
            tuple.Item6?.GetStableHashCode() ?? 0,
            tuple.Item7?.GetStableHashCode() ?? 0
        );
    }

    /// <summary>
    /// Returns the stable hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.
    /// </summary>
    /// <returns>A 32-bit signed integer stable hash code.</returns>
    public static int GetStableHashCode<T1, T2, T3, T4, T5, T6, T7, TRest>(this ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple)
    where TRest : struct
    {
        return HashStableCombine(
            tuple.Item1?.GetStableHashCode() ?? 0,
            tuple.Item2?.GetStableHashCode() ?? 0,
            tuple.Item3?.GetStableHashCode() ?? 0,
            tuple.Item4?.GetStableHashCode() ?? 0,
            tuple.Item5?.GetStableHashCode() ?? 0,
            tuple.Item6?.GetStableHashCode() ?? 0,
            tuple.Item7?.GetStableHashCode() ?? 0,
            tuple.Rest.GetStableHashCode()
        );
    }

    #endregion ValueTuple

    /// <summary>
    /// Combines a set of hashes into a single stable hash.
    /// </summary>
    /// <param name="hashes"></param>
    /// <returns></returns>
    public static int HashStableCombine(params int[] hashes)
    {
        unchecked
        {
            int hash = 17;
            foreach (var nextHash in hashes)
            {
                hash = hash * 31 + nextHash;
            }

            return hash;
        }
    }

    /// <summary>
    /// Combines a set of hashes into a single stable hash.
    /// </summary>
    /// <param name="hashes"></param>
    /// <returns></returns>
    public static int HashStableCombine(IEnumerable<int> hashes)
    {
        unchecked
        {
            int hash = 17;
            foreach (var nextHash in hashes)
            {
                hash = hash * 31 + nextHash;
            }

            return hash;
        }
    }
}