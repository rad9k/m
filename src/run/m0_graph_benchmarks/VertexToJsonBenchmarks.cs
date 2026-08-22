using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using m0;
using m0.Foundation;
using m0.Lib.StdView;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[SimpleJob(
    RuntimeMoniker.Net10_0,
    launchCount: 1,
    warmupCount: 3,
    iterationCount: 5,
    invocationCount: 1)]
public class VertexToJsonBenchmarks
{
    [Params(100, 1000)]
    public int ItemCount { get; set; }

    private IVertex root = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        if (!MinusZero.Instance.IsInitialized)
        {
            MinusZero.Instance.DoLog = false;
            MinusZero.Instance.SetUserInteraction(
                new NoOpUserInteraction());
            MinusZero.Instance.Initialize();
        }

        IVertex itemMeta =
            MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                $"JsonItem-{Guid.NewGuid():N}");
        IVertex valueMeta =
            MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                $"JsonValue-{Guid.NewGuid():N}");
        root = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "");

        for (int index = 0;
            index < ItemCount;
            index++)
        {
            IVertex item = root.AddVertex(
                itemMeta,
                "");
            item.AddVertex(
                valueMeta,
                index);
        }
    }

    [Benchmark]
    public string Serialize()
    {
        return VertexToJson.VertexToJson_Process(root);
    }
}
