using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using m0;
using m0.Foundation;
using m0.Lib.StdView;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class VertexToJsonContractTests
{
    public VertexToJsonContractTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void RepresentativeGraphMatchesGoldenJson()
    {
        IVertex root = CreateRetainedVertex("");
        IVertex meta = CreateRetainedVertex("Meta");
        IVertex complexMeta =
            CreateRetainedVertex("Complex");
        IVertex nameMeta = CreateRetainedVertex("Name");
        IVertex ageMeta = CreateRetainedVertex("Age");
        IVertex backMeta = CreateRetainedVertex("Back");
        root.AddVertex(meta, "one");
        root.AddVertex(meta, "two");
        IVertex complex = root.AddVertex(
            complexMeta,
            "");
        complex.AddVertex(nameMeta, "Alice");
        complex.AddVertex(ageMeta, 42);
        complex.AddEdge(backMeta, root);

        string json =
            VertexToJson.VertexToJson_Process(root);
        using JsonDocument document =
            JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Object,
            document.RootElement.ValueKind);
        Assert.Equal(
            "0A5789E31433680BE4FF7B4989DAAA40CF115750222CC6340AFD9FDBA5F52E0B",
            Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(json))));
    }

    private static IVertex CreateRetainedVertex(
        object value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }
}
