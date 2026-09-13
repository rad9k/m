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

[MemoryDiagnoser]
public class StoreLookupBenchmarks
{
    private readonly List<IStore> createdStores =
        new();
    private string storeTypeName = null!;
    private string storeIdentifier = null!;

    [Params(2, 32)]
    public int StoreCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var accessLevels =
            new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix =
            Guid.NewGuid().ToString("N");

        for (var index = 0;
             index < StoreCount;
             index++)
        {
            createdStores.Add(
                new MemoryStore(
                    $"lookup-{identifierSuffix}-{index}",
                    MinusZero.Instance,
                    accessLevels,
                    true));
        }

        IStore targetStore = createdStores[^1];
        storeTypeName = targetStore.TypeName;
        storeIdentifier = targetStore.Identifier;
    }

    [Benchmark(Baseline = true)]
    public IStore LegacyLinqLookup()
    {
        return MinusZero.Instance.Stores
            .Where(store =>
                store.TypeName == storeTypeName &
                store.Identifier == storeIdentifier)
            .FirstOrDefault()!;
    }

    [Benchmark]
    public IStore AllocationFreeLoopLookup()
    {
        return MinusZero.Instance.GetStore(
            storeTypeName,
            storeIdentifier);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (IStore store in createdStores)
            MinusZero.Instance.RemoveStore(store);
    }
}
