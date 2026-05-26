using System;
using System.Collections.Generic;
using System.Linq;

namespace Unipi.Nancy.Numerics;

/// <summary>
/// Provides LINQ extension methods for <see cref="Rational"/>, <see cref="LongRational"/> and <see cref="BigRational"/>.
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Computes the sum of a sequence of <see cref="LongRational"/> values.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="defaultZero">
    /// If <see langword="true"/>, returns <see cref="LongRational.Zero"/> when the sequence is empty;
    /// otherwise, throws <see cref="InvalidOperationException"/>.
    /// </param>
    /// <returns>The sum of the values in the sequence.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sequence is empty and <paramref name="defaultZero"/> is <see langword="false"/>.
    /// </exception>
    public static LongRational Sum(this IEnumerable<LongRational> source, bool defaultZero = false)
    {
        return defaultZero
            ? source.Aggregate(LongRational.Zero, (x, y) => x + y)
            : source.Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// Computes the sum of the <see cref="LongRational"/> values obtained by applying a selector function
    /// to each element of a sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <param name="defaultZero">
    /// If <see langword="true"/>, returns <see cref="LongRational.Zero"/> when the sequence is empty;
    /// otherwise, throws <see cref="InvalidOperationException"/>.
    /// </param>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <returns>The sum of the projected values.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sequence is empty and <paramref name="defaultZero"/> is <see langword="false"/>.
    /// </exception>
    public static LongRational Sum<T>(this IEnumerable<T> source, Func<T, LongRational> selector, bool defaultZero = false)
    {
        return defaultZero
            ? source.Select(selector).Aggregate(LongRational.Zero, (x, y) => x + y)
            : source.Select(selector).Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// Computes the sum of a sequence of <see cref="BigRational"/> values.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="defaultZero">
    /// If <see langword="true"/>, returns <see cref="BigRational.Zero"/> when the sequence is empty;
    /// otherwise, throws <see cref="InvalidOperationException"/>.
    /// </param>
    /// <returns>The sum of the values in the sequence.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sequence is empty and <paramref name="defaultZero"/> is <see langword="false"/>.
    /// </exception>
    public static BigRational Sum(this IEnumerable<BigRational> source, bool defaultZero = false)
    {
        return defaultZero
            ? source.Aggregate(BigRational.Zero, (x, y) => x + y)
            : source.Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// Computes the sum of the <see cref="BigRational"/> values obtained by applying a selector function
    /// to each element of a sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <param name="defaultZero">
    /// If <see langword="true"/>, returns <see cref="BigRational.Zero"/> when the sequence is empty;
    /// otherwise, throws <see cref="InvalidOperationException"/>.
    /// </param>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <returns>The sum of the projected values.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sequence is empty and <paramref name="defaultZero"/> is <see langword="false"/>.
    /// </exception>
    public static BigRational Sum<T>(this IEnumerable<T> source, Func<T, BigRational> selector, bool defaultZero = false)
    {
        return defaultZero
            ? source.Select(selector).Aggregate(BigRational.Zero, (x, y) => x + y)
            : source.Select(selector).Aggregate((x, y) => x + y);
    }

    //The following work in both implementations
    /// <summary>
    /// Computes the sum of a sequence of <see cref="Rational"/> values.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="defaultZero">
    /// If <see langword="true"/>, returns <see cref="Rational.Zero"/> when the sequence is empty;
    /// otherwise, throws <see cref="InvalidOperationException"/>.
    /// </param>
    /// <returns>The sum of the values in the sequence.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sequence is empty and <paramref name="defaultZero"/> is <see langword="false"/>.
    /// </exception>
    public static Rational Sum(this IEnumerable<Rational> source, bool defaultZero = false)
    {
        return defaultZero
            ? source.Aggregate(Rational.Zero, (x, y) => x + y)
            : source.Aggregate((x, y) => x + y);
    }

    /// <summary>
    /// Computes the sum of the <see cref="Rational"/> values obtained by applying a selector function
    /// to each element of a sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <param name="defaultZero">
    /// If <see langword="true"/>, returns <see cref="Rational.Zero"/> when the sequence is empty;
    /// otherwise, throws <see cref="InvalidOperationException"/>.
    /// </param>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <returns>The sum of the projected values.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sequence is empty and <paramref name="defaultZero"/> is <see langword="false"/>.
    /// </exception>
    public static Rational Sum<T>(this IEnumerable<T> source, Func<T, Rational> selector, bool defaultZero = false)
    {
        return defaultZero
            ? source.Select(selector).Aggregate(Rational.Zero, (x, y) => x + y)
            : source.Select(selector).Aggregate((x, y) => x + y);
    }
}
