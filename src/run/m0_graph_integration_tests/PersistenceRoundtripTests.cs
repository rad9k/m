using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store.Binary;
using m0.Store.Json;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class PersistenceRoundtripTests
{
    public PersistenceRoundtripTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData("json")]
    [InlineData("binary")]
    public void StoreRoundtripPreservesGraphStructure(string format)
    {
        var extension = format == "json" ? ".m0j" : ".m0b";
        var path = Path.Combine(
            Path.GetTempPath(),
            $"m0-graph-roundtrip-{Guid.NewGuid():N}{extension}");
        IStore? originalStore = null;
        IStore? reloadedStore = null;
        var previousNoBackup = MinusZero.Instance.CommandLineParameters.NoBackup;
        MinusZero.Instance.CommandLineParameters.NoBackup = true;

        try
        {
            originalStore = CreateStore(format, path);
            var meta = originalStore.Root.AddVertex(MinusZero.Instance.Empty, "Relation");
            var source = originalStore.Root.AddVertex(MinusZero.Instance.Empty, "Source");
            var target = originalStore.Root.AddVertex(MinusZero.Instance.Empty, "Target");
            source.AddEdge(meta, target);
            originalStore.Detach();
            originalStore.CommitTransaction();
            MinusZero.Instance.RemoveStore(originalStore);
            originalStore = null;

            reloadedStore = CreateStore(format, path);
            var reloadedSource = GraphUtil.GetQueryOutFirst(
                reloadedStore.Root,
                "$Empty",
                "Source");
            var reloadedEdge = Assert.Single(
                GraphUtil.GetQueryOut(reloadedSource, "Relation", "Target"));

            Assert.Equal("Source", reloadedEdge.From.Value);
            Assert.Equal("Relation", reloadedEdge.Meta.Value);
            Assert.Equal("Target", reloadedEdge.To.Value);
            Assert.Contains(reloadedEdge, reloadedEdge.To.InEdgesRaw);
            Assert.Contains(reloadedEdge, reloadedEdge.Meta.MetaInEdgesRaw);
        }
        finally
        {
            if (originalStore != null)
                MinusZero.Instance.RemoveStore(originalStore);

            if (reloadedStore != null)
                MinusZero.Instance.RemoveStore(reloadedStore);

            MinusZero.Instance.CommandLineParameters.NoBackup = previousNoBackup;
            File.Delete(path);
            File.Delete(path + ".backup");
        }
    }

    private static IStore CreateStore(string format, string path)
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };

        return format == "json"
            ? new JsonSerializationStore(path, MinusZero.Instance, accessLevels)
            : new BinaryStore(path, MinusZero.Instance, accessLevels);
    }
}
