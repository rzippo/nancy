using System;
using System.Collections.Generic;
using System.Linq;

namespace Unipi.Nancy.Tests;

/// <summary>
/// Helper methods that convert lists of test cases in value tuple form into the object[] form required by xUnit.
/// </summary>
public static class ToXUnitTestCasesExtensions
{
    /// <summary>
    /// Helper method that converts the list of objects into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1>(this IEnumerable<T1> list)
    {
        return list.Select(t1 => new object[] { t1! });
    }
    
    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> tuplesList)
    {
        foreach (var (t1, t2) in tuplesList)
        {
            yield return new object[] { t1!, t2! };
        }
    }
    
    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2, T3>(this IEnumerable<ValueTuple<T1, T2, T3>> tuplesList)
    {
        foreach (var (t1, t2, t3) in tuplesList)
        {
            yield return new object[] { t1!, t2!, t3! };
        }
    }
    
    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2, T3, T4>(this IEnumerable<ValueTuple<T1, T2, T3, T4>> tuplesList)
    {
        foreach (var (t1, t2, t3, t4) in tuplesList)
        {
            yield return new object[] { t1!, t2!, t3!, t4! };
        }
    }
    
    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2, T3, T4, T5>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5>> tuplesList)
    {
        foreach (var (t1, t2, t3, t4, t5) in tuplesList)
        {
            yield return new object[] { t1!, t2!, t3!, t4!, t5! };
        }
    }

    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2, T3, T4, T5, T6>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6>> tuplesList)
    {
        foreach (var (t1, t2, t3, t4, t5, t6) in tuplesList)
        {
            yield return new object[] { t1!, t2!, t3!, t4!, t5!, t6! };
        }
    }

    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>> tuplesList)
    {
        foreach (var (t1, t2, t3, t4, t5, t6, t7) in tuplesList)
        {
            yield return new object[] { t1!, t2!, t3!, t4!, t5!, t6!, t7! };
        }
    }
    
    /// <summary>
    /// Helper method that converts the list of tuples into a list of object[], as required by xUnit. 
    /// </summary>
    public static IEnumerable<object[]> ToXUnitTestCases<T1, T2, T3, T4, T5, T6, T7, T8>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>>> tuplesList) 
    {
        foreach (var (t1, t2, t3, t4, t5, t6, t7, t8) in tuplesList)
        {
            yield return new object[] { t1!, t2!, t3!, t4!, t5!, t6!, t7!, t8! };
        }
    }

    // continue with others until needed... 
    // .NET source code goes up to 21, example:
    // https://learn.microsoft.com/en-us/dotnet/api/system.tupleextensions.deconstruct?view=net-8.0

}