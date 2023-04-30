using System;
using System.Collections.Generic;
using System.Linq;

namespace Unipi.Nancy.Numerics;

/// <summary>
/// Provides LINQ extension methods for Rational, LongRational and BigRational
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static LongRational Sum(this IEnumerable<LongRational> source)
    {
        return source
            .Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static LongRational Sum<T>(this IEnumerable<T> source, Func<T, LongRational> selector)
    {
        return source
            .Select(selector)
            .Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static BigRational Sum(this IEnumerable<BigRational> source)
    {
        return source
            .Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static BigRational Sum<T>(this IEnumerable<T> source, Func<T, BigRational> selector)
    {
        return source
            .Select(selector)
            .Aggregate((x, y) => x + y);
    }

    //The following work in both implementations
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Rational Sum(this IEnumerable<Rational> source)
    {
        return source
            .Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Rational Sum<T>(this IEnumerable<T> source, Func<T, Rational> selector)
    {
        return source
            .Select(selector)
            .Aggregate((x, y) => x + y);
    }

}