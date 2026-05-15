using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Unipi.Nancy.Tests;

/// <summary>
/// Taken from <see href="https://www.patriksvensson.se/2017/11/using-embedded-resources-in-xunit-tests">here</see>.
/// </summary>
public sealed class EmbeddedResourceDataAttribute : DataAttribute
{
    private readonly string[] _args;

    public EmbeddedResourceDataAttribute(params string[] args)
    {
        _args = args;
    }

    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        var result = new object[_args.Length];
        for (var index = 0; index < _args.Length; index++)
        {
            result[index] = ReadManifestData(_args[index]);
        }
        return new ValueTask<IReadOnlyCollection<ITheoryDataRow>>(
            new ITheoryDataRow[] { new TheoryDataRow(result) }
        );
    }

    public override bool SupportsDiscoveryEnumeration() => true;

    public static string ReadManifestData(string resourceName)
    {
        var assembly = typeof(EmbeddedResourceDataAttribute).GetTypeInfo().Assembly;
        resourceName = resourceName.Replace("/", ".");
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException("Could not load manifest resource stream.");
            }
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
