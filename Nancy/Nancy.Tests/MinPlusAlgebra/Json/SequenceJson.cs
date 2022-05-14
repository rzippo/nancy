using Newtonsoft.Json;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class SequenceJson
{
    private readonly ITestOutputHelper output;

    public SequenceJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void SequenceSerialization()
    {
        Sequence[] testSequences = new[]
        {
            TestFunctions.SequenceA,
            TestFunctions.SequenceB,
        };

        foreach (var sequence in testSequences)
        {
            string serialization = JsonConvert.SerializeObject(sequence);
            output.WriteLine(serialization);
            Sequence deserialized = JsonConvert.DeserializeObject<Sequence>(serialization)!;

            Assert.Equal(sequence, deserialized);
        }
    }
}