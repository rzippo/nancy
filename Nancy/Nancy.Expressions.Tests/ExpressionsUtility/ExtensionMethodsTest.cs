using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

[TestSubject(typeof(ExtensionMethods))]
public class ExtensionMethodsTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExtensionMethodsTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GetCombinations_ExtensionMethods_Int()
    {
        var array = Enumerable.Range(0, 4);
        var indexesComb = array.GetCombinations(2);
        var str = new StringBuilder();
        foreach (var indexes in indexesComb)
        {
            foreach (var indexTerm in indexes)
            {
                str.Append(indexTerm);
                str.Append(' ');
            }
            str.Append('\n');
        }
        _testOutputHelper.WriteLine(str.ToString());
    }
}