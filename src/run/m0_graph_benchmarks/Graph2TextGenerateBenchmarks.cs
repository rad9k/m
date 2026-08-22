using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using m0;
using m0.Foundation;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[SimpleJob(
    RuntimeMoniker.Net10_0,
    launchCount: 1,
    warmupCount: 3,
    iterationCount: 5,
    invocationCount: 1)]
public class Graph2TextGenerateBenchmarks
{
    private IEdge parsedRootEdge = null!;

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

        IVertex parseParent =
            MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                $"Graph2TextBenchmark-{Guid.NewGuid():N}");
        IEdge sourceEdge =
            parseParent.AddVertexAndReturnEdge(
                MinusZero.Instance.Empty,
                "Source");
        IVertex parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                ZeroCodePerformanceWorkload.Source,
                CodeRepresentationEnum.LinearizedManyLines,
                out parsedRootEdge);

        if (parseErrors != null && parseErrors.Any())
            throw new InvalidOperationException(
                "Graph2Text benchmark source failed to parse.");
    }

    [Benchmark]
    public string Generate()
    {
        return MinusZero.Instance
            .DefaultFormalTextGenerator.Generate(
                parsedRootEdge,
                CodeRepresentationEnum.VertexAndManyLines);
    }
}
