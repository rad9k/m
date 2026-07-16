using BenchmarkDotNet.Attributes;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class CrossStoreDetachAttachBenchmarks
{
    private MemoryStore sourceStore = null!;
    private MemoryStore targetStore = null!;
    private IVertex target = null!;

    [Params(1, 100, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        sourceStore = new MemoryStore(
            $"detach-source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        targetStore = new MemoryStore(
            $"detach-target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        var meta = new EasyVertex(sourceStore)
        {
            Value = "Meta"
        };
        target = new EasyVertex(targetStore)
        {
            Value = "Target"
        };

        for (var index = 0; index < EdgeCount; index++)
        {
            var source = new EasyVertex(sourceStore)
            {
                Value = $"Source-{index}"
            };
            source.AddEdge(meta, target);
        }
    }

    [Benchmark]
    public int DetachAndAttach()
    {
        sourceStore.Detach();
        sourceStore.Attach();
        return target.InEdgesRaw.Count;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        MinusZero.Instance.RemoveStore(sourceStore);
        MinusZero.Instance.RemoveStore(targetStore);
    }
}
