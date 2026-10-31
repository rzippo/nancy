using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks the property and extrema terminals over every kind of source they read differently.
/// </summary>
/// <remarks>
/// Spans, arrays, lists and the elements of a sequence are read by index, other collections through their enumerator.
/// Every source runs the same body of the terminal, so each is compared with the reading of a single-use iterator.
/// </remarks>
public class ElementTerminalSources
{
    /// <summary>Elements in uninterrupted time order.</summary>
    public sealed record Shape(string Name, Element[] Elements)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    private static List<Shape> ShapeCatalogue() =>
    [
        new("single point", [new Point(0, 5)]),
        new("continuous ramp",
            [Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1), new Point(2, 2)]),
        new("steps of both signs",
            [new Point(0, 1), Segment.Constant(0, 1, 2), new Point(1, 0), Segment.Constant(1, 3, -1)]),
        new("plus infinity after a point", [Point.Origin(), new Segment(0, 1, Rational.PlusInfinity, 0)]),
        new("minus infinity before a point", [new Segment(0, 2, Rational.MinusInfinity, 0), new Point(2, -1)])
    ];

    public static IEnumerable<object[]> Shapes() => ShapeCatalogue().ToXUnitTestCases();

    private static IEnumerable<(string Name, IEnumerable<Element> Elements)> EnumerableSources(Element[] elements) =>
    [
        ("array as an enumerable", elements),
        ("list", elements.ToList()),
        ("read-only collection", new ReadOnlyCollection<Element>(elements))
    ];

    /// <param name="elements">A nonempty array whose first element also supplies the padding.</param>
    private static ReadOnlySpan<Element> PaddedSpan(Element[] elements)
        => new[] { elements[0] }.Concat(elements)
            .ToArray()
            .AsSpan(1);

    private static IEnumerable<Sequence> Sequences(Element[] elements) =>
    [
        // A sequence copies the elements it is given, from a list or a stream.
        new Sequence(elements.ToList()),
        new Sequence(ValidationCases.SingleUse(elements))
    ];

    private static void AssertReading<T>(T expected, T actual, string source)
        => Assert.True(Equals(expected, actual), $"{source}: {actual} instead of {expected}");

    /// <summary>Two points at one time, with a value that leaves the terminal undecided after the first so it reads the second.</summary>
    private static Element[] DuplicatePoints(Rational value) => [new Point(0, value), new Point(0, value)];

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsContinuousAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsContinuous();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsContinuous(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsContinuous(), "span");
        AssertReading(expected, PaddedSpan(elements).IsContinuous(), "span slice");
        AssertReading(expected, elements.IsContinuous(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsContinuous());
            Assert.Equal(expected, sequence.IsContinuous);
        }
    }

    [Fact]
    public void IsContinuousRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsContinuous());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsContinuous()));
    }

    [Fact]
    public void IsContinuousRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsContinuous());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsContinuous());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsContinuous());
    }

    [Fact]
    public void IsContinuousRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsContinuous()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsLeftContinuousAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsLeftContinuous();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsLeftContinuous(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsLeftContinuous(), "span");
        AssertReading(expected, PaddedSpan(elements).IsLeftContinuous(), "span slice");
        AssertReading(expected, elements.IsLeftContinuous(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsLeftContinuous());
            Assert.Equal(expected, sequence.IsLeftContinuous);
        }
    }

    [Fact]
    public void IsLeftContinuousRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsLeftContinuous());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsLeftContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsLeftContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsLeftContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsLeftContinuous()));
    }

    [Fact]
    public void IsLeftContinuousRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsLeftContinuous());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsLeftContinuous());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsLeftContinuous());
    }

    [Fact]
    public void IsLeftContinuousRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsLeftContinuous()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsRightContinuousAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsRightContinuous();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsRightContinuous(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsRightContinuous(), "span");
        AssertReading(expected, PaddedSpan(elements).IsRightContinuous(), "span slice");
        AssertReading(expected, elements.IsRightContinuous(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsRightContinuous());
            Assert.Equal(expected, sequence.IsRightContinuous);
        }
    }

    [Fact]
    public void IsRightContinuousRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsRightContinuous());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsRightContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsRightContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsRightContinuous()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsRightContinuous()));
    }

    [Fact]
    public void IsRightContinuousRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsRightContinuous());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsRightContinuous());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsRightContinuous());
    }

    [Fact]
    public void IsRightContinuousRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsRightContinuous()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsZeroAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsZero();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsZero(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsZero(), "span");
        AssertReading(expected, PaddedSpan(elements).IsZero(), "span slice");
        AssertReading(expected, elements.IsZero(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsZero());
            Assert.Equal(expected, sequence.IsZero);
        }
    }

    [Fact]
    public void IsZeroRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsZero());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsZero()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsZero()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsZero()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsZero()));
    }

    [Fact]
    public void IsZeroRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsZero());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsZero());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsZero());
    }

    [Fact]
    public void IsZeroRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsZero()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsInfiniteAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsInfinite();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsInfinite(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsInfinite(), "span");
        AssertReading(expected, PaddedSpan(elements).IsInfinite(), "span slice");
        AssertReading(expected, elements.IsInfinite(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsInfinite());
            Assert.Equal(expected, sequence.IsInfinite);
        }
    }

    [Fact]
    public void IsInfiniteRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(Rational.PlusInfinity);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsInfinite());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsInfinite()));
    }

    [Fact]
    public void IsInfiniteRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsInfinite());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsInfinite());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsInfinite());
    }

    [Fact]
    public void IsInfiniteRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsInfinite()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void HasPlusInfinityAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).HasPlusInfinity();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.HasPlusInfinity(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).HasPlusInfinity(), "span");
        AssertReading(expected, PaddedSpan(elements).HasPlusInfinity(), "span slice");
        AssertReading(expected, elements.HasPlusInfinity(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.HasPlusInfinity());
            Assert.Equal(expected, sequence.HasPlusInfinity);
        }
    }

    [Fact]
    public void HasPlusInfinityRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).HasPlusInfinity());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.HasPlusInfinity()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).HasPlusInfinity()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).HasPlusInfinity()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.HasPlusInfinity()));
    }

    [Fact]
    public void HasPlusInfinityRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().HasPlusInfinity());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.HasPlusInfinity());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().HasPlusInfinity());
    }

    [Fact]
    public void HasPlusInfinityRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).HasPlusInfinity()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void HasMinusInfinityAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).HasMinusInfinity();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.HasMinusInfinity(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).HasMinusInfinity(), "span");
        AssertReading(expected, PaddedSpan(elements).HasMinusInfinity(), "span slice");
        AssertReading(expected, elements.HasMinusInfinity(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.HasMinusInfinity());
            Assert.Equal(expected, sequence.HasMinusInfinity);
        }
    }

    [Fact]
    public void HasMinusInfinityRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).HasMinusInfinity());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.HasMinusInfinity()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).HasMinusInfinity()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).HasMinusInfinity()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.HasMinusInfinity()));
    }

    [Fact]
    public void HasMinusInfinityRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().HasMinusInfinity());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.HasMinusInfinity());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().HasMinusInfinity());
    }

    [Fact]
    public void HasMinusInfinityRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).HasMinusInfinity()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsNonPositiveAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsNonPositive();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsNonPositive(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsNonPositive(), "span");
        AssertReading(expected, PaddedSpan(elements).IsNonPositive(), "span slice");
        AssertReading(expected, elements.IsNonPositive(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsNonPositive());
            Assert.Equal(expected, sequence.IsNonPositive);
        }
    }

    [Fact]
    public void IsNonPositiveRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNonPositive());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsNonPositive()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsNonPositive()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsNonPositive()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsNonPositive()));
    }

    [Fact]
    public void IsNonPositiveRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsNonPositive());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsNonPositive());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonPositive());
    }

    [Fact]
    public void IsNonPositiveRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsNonPositive()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsPositiveAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsPositive();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsPositive(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsPositive(), "span");
        AssertReading(expected, PaddedSpan(elements).IsPositive(), "span slice");
        AssertReading(expected, elements.IsPositive(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsPositive());
            Assert.Equal(expected, sequence.IsPositive);
        }
    }

    [Fact]
    public void IsPositiveRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(1);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsPositive());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsPositive()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsPositive()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsPositive()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsPositive()));
    }

    [Fact]
    public void IsPositiveRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsPositive());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsPositive());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsPositive());
    }

    [Fact]
    public void IsPositiveRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsPositive()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsNegativeAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsNegative();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsNegative(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsNegative(), "span");
        AssertReading(expected, PaddedSpan(elements).IsNegative(), "span slice");
        AssertReading(expected, elements.IsNegative(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsNegative());
            Assert.Equal(expected, sequence.IsNegative);
        }
    }

    [Fact]
    public void IsNegativeRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(-1);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNegative());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsNegative()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsNegative()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsNegative()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsNegative()));
    }

    [Fact]
    public void IsNegativeRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsNegative());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsNegative());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNegative());
    }

    [Fact]
    public void IsNegativeRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsNegative()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsConstantAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsConstant();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsConstant(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsConstant(), "span");
        AssertReading(expected, PaddedSpan(elements).IsConstant(), "span slice");
        AssertReading(expected, elements.IsConstant(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsConstant());
            Assert.Equal(expected, sequence.IsConstant);
        }
    }

    [Fact]
    public void IsConstantRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsConstant());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsConstant()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsConstant()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsConstant()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsConstant()));
    }

    [Fact]
    public void IsConstantRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsConstant());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsConstant());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsConstant());
    }

    [Fact]
    public void IsConstantRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsConstant()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsAffineAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsAffine();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsAffine(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsAffine(), "span");
        AssertReading(expected, PaddedSpan(elements).IsAffine(), "span slice");
        AssertReading(expected, elements.IsAffine(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsAffine());
            Assert.Equal(expected, sequence.IsAffine);
        }
    }

    [Fact]
    public void IsAffineRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsAffine());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsAffine()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsAffine()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsAffine()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsAffine()));
    }

    [Fact]
    public void IsAffineRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsAffine());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsAffine());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsAffine());
    }

    [Fact]
    public void IsAffineRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsAffine()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsNonNegativeAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsNonNegative();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsNonNegative(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsNonNegative(), "span");
        AssertReading(expected, PaddedSpan(elements).IsNonNegative(), "span slice");
        AssertReading(expected, elements.IsNonNegative(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsNonNegative());
            Assert.Equal(expected, sequence.IsNonNegative);
        }
    }

    [Fact]
    public void IsNonNegativeRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNonNegative());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsNonNegative()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsNonNegative()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsNonNegative()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsNonNegative()));
    }

    [Fact]
    public void IsNonNegativeRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsNonNegative());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsNonNegative());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonNegative());
    }

    [Fact]
    public void IsNonNegativeRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsNonNegative()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsNonDecreasingAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsNonDecreasing();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsNonDecreasing(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsNonDecreasing(), "span");
        AssertReading(expected, PaddedSpan(elements).IsNonDecreasing(), "span slice");
        AssertReading(expected, elements.IsNonDecreasing(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsNonDecreasing());
            Assert.Equal(expected, sequence.IsNonDecreasing);
        }
    }

    [Fact]
    public void IsNonDecreasingRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNonDecreasing());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsNonDecreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsNonDecreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsNonDecreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsNonDecreasing()));
    }

    [Fact]
    public void IsNonDecreasingRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsNonDecreasing());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsNonDecreasing());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonDecreasing());
    }

    [Fact]
    public void IsNonDecreasingRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsNonDecreasing()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsIncreasingAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsIncreasing();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsIncreasing(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsIncreasing(), "span");
        AssertReading(expected, PaddedSpan(elements).IsIncreasing(), "span slice");
        AssertReading(expected, elements.IsIncreasing(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsIncreasing());
            Assert.Equal(expected, sequence.IsIncreasing);
        }
    }

    [Fact]
    public void IsIncreasingRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsIncreasing());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsIncreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsIncreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsIncreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsIncreasing()));
    }

    [Fact]
    public void IsIncreasingRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsIncreasing());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsIncreasing());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsIncreasing());
    }

    [Fact]
    public void IsIncreasingRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsIncreasing()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsNonIncreasingAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsNonIncreasing();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsNonIncreasing(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsNonIncreasing(), "span");
        AssertReading(expected, PaddedSpan(elements).IsNonIncreasing(), "span slice");
        AssertReading(expected, elements.IsNonIncreasing(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsNonIncreasing());
            Assert.Equal(expected, sequence.IsNonIncreasing);
        }
    }

    [Fact]
    public void IsNonIncreasingRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNonIncreasing());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsNonIncreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsNonIncreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsNonIncreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsNonIncreasing()));
    }

    [Fact]
    public void IsNonIncreasingRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsNonIncreasing());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsNonIncreasing());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonIncreasing());
    }

    [Fact]
    public void IsNonIncreasingRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsNonIncreasing()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsDecreasingAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsDecreasing();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsDecreasing(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsDecreasing(), "span");
        AssertReading(expected, PaddedSpan(elements).IsDecreasing(), "span slice");
        AssertReading(expected, elements.IsDecreasing(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsDecreasing());
            Assert.Equal(expected, sequence.IsDecreasing);
        }
    }

    [Fact]
    public void IsDecreasingRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsDecreasing());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsDecreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsDecreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsDecreasing()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsDecreasing()));
    }

    [Fact]
    public void IsDecreasingRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsDecreasing());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsDecreasing());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsDecreasing());
    }

    [Fact]
    public void IsDecreasingRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsDecreasing()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsFiniteAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsFinite();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsFinite(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsFinite(), "span");
        AssertReading(expected, PaddedSpan(elements).IsFinite(), "span slice");
        AssertReading(expected, elements.IsFinite(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsFinite());
            Assert.Equal(expected, sequence.IsFinite);
        }
    }

    [Fact]
    public void IsFiniteRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsFinite());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsFinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsFinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsFinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsFinite()));
    }

    [Fact]
    public void IsFiniteRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsFinite());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsFinite());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsFinite());
    }

    [Fact]
    public void IsFiniteRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsFinite()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsMinusInfiniteAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsMinusInfinite();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsMinusInfinite(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsMinusInfinite(), "span");
        AssertReading(expected, PaddedSpan(elements).IsMinusInfinite(), "span slice");
        AssertReading(expected, elements.IsMinusInfinite(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsMinusInfinite());
            Assert.Equal(expected, sequence.IsMinusInfinite);
        }
    }

    [Fact]
    public void IsMinusInfiniteRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(Rational.MinusInfinity);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsMinusInfinite());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsMinusInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsMinusInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsMinusInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsMinusInfinite()));
    }

    [Fact]
    public void IsMinusInfiniteRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsMinusInfinite());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsMinusInfinite());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsMinusInfinite());
    }

    [Fact]
    public void IsMinusInfiniteRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsMinusInfinite()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void IsPlusInfiniteAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).IsPlusInfinite();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.IsPlusInfinite(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).IsPlusInfinite(), "span");
        AssertReading(expected, PaddedSpan(elements).IsPlusInfinite(), "span slice");
        AssertReading(expected, elements.IsPlusInfinite(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.IsPlusInfinite());
            Assert.Equal(expected, sequence.IsPlusInfinite);
        }
    }

    [Fact]
    public void IsPlusInfiniteRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(Rational.PlusInfinity);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsPlusInfinite());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.IsPlusInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).IsPlusInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).IsPlusInfinite()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.IsPlusInfinite()));
    }

    [Fact]
    public void IsPlusInfiniteRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().IsPlusInfinite());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsPlusInfinite());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsPlusInfinite());
    }

    [Fact]
    public void IsPlusInfiniteRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).IsPlusInfinite()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void SupValueAndArgAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).SupValueAndArg();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.SupValueAndArg(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).SupValueAndArg(), "span");
        AssertReading(expected, PaddedSpan(elements).SupValueAndArg(), "span slice");
        AssertReading(expected, elements.SupValueAndArg(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.SupValueAndArg());
            Assert.Equal(expected, sequence.SupValueAndArg());
        }
    }

    [Fact]
    public void SupValueAndArgRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).SupValueAndArg());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.SupValueAndArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).SupValueAndArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).SupValueAndArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.SupValueAndArg()));
    }

    [Fact]
    public void SupValueAndArgRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().SupValueAndArg());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.SupValueAndArg());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupValueAndArg());
    }

    [Fact]
    public void SupValueAndArgRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).SupValueAndArg()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void InfValueAndArgAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).InfValueAndArg();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.InfValueAndArg(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).InfValueAndArg(), "span");
        AssertReading(expected, PaddedSpan(elements).InfValueAndArg(), "span slice");
        AssertReading(expected, elements.InfValueAndArg(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.InfValueAndArg());
            Assert.Equal(expected, sequence.InfValueAndArg());
        }
    }

    [Fact]
    public void InfValueAndArgRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).InfValueAndArg());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.InfValueAndArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).InfValueAndArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).InfValueAndArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.InfValueAndArg()));
    }

    [Fact]
    public void InfValueAndArgRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().InfValueAndArg());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.InfValueAndArg());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfValueAndArg());
    }

    [Fact]
    public void InfValueAndArgRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).InfValueAndArg()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void SupValueAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).SupValue();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.SupValue(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).SupValue(), "span");
        AssertReading(expected, PaddedSpan(elements).SupValue(), "span slice");
        AssertReading(expected, elements.SupValue(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.SupValue());
            Assert.Equal(expected, sequence.SupValue());
        }
    }

    [Fact]
    public void SupValueRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).SupValue());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.SupValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).SupValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).SupValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.SupValue()));
    }

    [Fact]
    public void SupValueRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().SupValue());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.SupValue());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupValue());
    }

    [Fact]
    public void SupValueRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).SupValue()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void SupArgAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).SupArg();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.SupArg(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).SupArg(), "span");
        AssertReading(expected, PaddedSpan(elements).SupArg(), "span slice");
        AssertReading(expected, elements.SupArg(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.SupArg());
            Assert.Equal(expected, sequence.SupArg());
        }
    }

    [Fact]
    public void SupArgRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).SupArg());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.SupArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).SupArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).SupArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.SupArg()));
    }

    [Fact]
    public void SupArgRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().SupArg());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.SupArg());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupArg());
    }

    [Fact]
    public void SupArgRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).SupArg()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void MaxValueAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).MaxValue();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.MaxValue(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).MaxValue(), "span");
        AssertReading(expected, PaddedSpan(elements).MaxValue(), "span slice");
        AssertReading(expected, elements.MaxValue(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.MaxValue());
            Assert.Equal(expected, sequence.MaxValue());
        }
    }

    [Fact]
    public void MaxValueRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).MaxValue());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.MaxValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).MaxValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).MaxValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.MaxValue()));
    }

    [Fact]
    public void MaxValueRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().MaxValue());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.MaxValue());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MaxValue());
    }

    [Fact]
    public void MaxValueRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).MaxValue()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void MaxArgAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).MaxArg();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.MaxArg(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).MaxArg(), "span");
        AssertReading(expected, PaddedSpan(elements).MaxArg(), "span slice");
        AssertReading(expected, elements.MaxArg(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.MaxArg());
            Assert.Equal(expected, sequence.MaxArg());
        }
    }

    [Fact]
    public void MaxArgRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).MaxArg());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.MaxArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).MaxArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).MaxArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.MaxArg()));
    }

    [Fact]
    public void MaxArgRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().MaxArg());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.MaxArg());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MaxArg());
    }

    [Fact]
    public void MaxArgRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).MaxArg()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void InfValueAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).InfValue();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.InfValue(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).InfValue(), "span");
        AssertReading(expected, PaddedSpan(elements).InfValue(), "span slice");
        AssertReading(expected, elements.InfValue(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.InfValue());
            Assert.Equal(expected, sequence.InfValue());
        }
    }

    [Fact]
    public void InfValueRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).InfValue());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.InfValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).InfValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).InfValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.InfValue()));
    }

    [Fact]
    public void InfValueRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().InfValue());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.InfValue());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfValue());
    }

    [Fact]
    public void InfValueRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).InfValue()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void InfArgAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).InfArg();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.InfArg(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).InfArg(), "span");
        AssertReading(expected, PaddedSpan(elements).InfArg(), "span slice");
        AssertReading(expected, elements.InfArg(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.InfArg());
            Assert.Equal(expected, sequence.InfArg());
        }
    }

    [Fact]
    public void InfArgRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).InfArg());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.InfArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).InfArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).InfArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.InfArg()));
    }

    [Fact]
    public void InfArgRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().InfArg());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.InfArg());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfArg());
    }

    [Fact]
    public void InfArgRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).InfArg()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void MinValueAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).MinValue();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.MinValue(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).MinValue(), "span");
        AssertReading(expected, PaddedSpan(elements).MinValue(), "span slice");
        AssertReading(expected, elements.MinValue(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.MinValue());
            Assert.Equal(expected, sequence.MinValue());
        }
    }

    [Fact]
    public void MinValueRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).MinValue());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.MinValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).MinValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).MinValue()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.MinValue()));
    }

    [Fact]
    public void MinValueRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().MinValue());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.MinValue());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MinValue());
    }

    [Fact]
    public void MinValueRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).MinValue()).ParamName);

    [Theory]
    [MemberData(nameof(Shapes))]
    public void MinArgAgreesAcrossSources(Shape shape)
    {
        var elements = shape.Elements;
        var expected = ValidationCases.SingleUse(elements).MinArg();
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(expected, source.MinArg(), name);
        AssertReading(expected, ((ReadOnlySpan<Element>)elements).MinArg(), "span");
        AssertReading(expected, PaddedSpan(elements).MinArg(), "span slice");
        AssertReading(expected, elements.MinArg(), "array");
        foreach (var sequence in Sequences(elements))
        {
            Assert.Equal(expected, sequence.Elements.MinArg());
            Assert.Equal(expected, sequence.MinArg());
        }
    }

    [Fact]
    public void MinArgRejectsAnElementItReads()
    {
        var elements = DuplicatePoints(0);
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).MinArg());
        foreach (var (_, source) in EnumerableSources(elements))
            Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => source.MinArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => ((ReadOnlySpan<Element>)elements).MinArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => PaddedSpan(elements).MinArg()));
        Assert.IsAssignableFrom<ArgumentException>(Record.Exception(() => elements.MinArg()));
    }

    [Fact]
    public void MinArgRejectsEmptySources()
    {
        Assert.Throws<ArgumentException>(() => new List<Element>().MinArg());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.MinArg());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MinArg());
    }

    [Fact]
    public void MinArgRejectsANullArray()
        => Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => ((Element[])null!).MinArg()).ParamName);

    [Fact]
    public void IsZeroLeavesAnUnreadElementUnchecked()
    {
        // The first point is not zero, which decides IsZero before the second point is read.
        var elements = DuplicatePoints(1);
        Assert.False(ValidationCases.SingleUse(elements).IsZero());
        foreach (var (name, source) in EnumerableSources(elements))
            AssertReading(false, source.IsZero(), name);
        Assert.False(((ReadOnlySpan<Element>)elements).IsZero());
        Assert.False(PaddedSpan(elements).IsZero());
        Assert.False(elements.IsZero());
    }
}
