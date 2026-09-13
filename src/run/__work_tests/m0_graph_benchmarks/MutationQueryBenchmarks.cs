using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class MutationQueryBenchmarks
{
    private const int OperationsPerInvocation = 100;

    private IVertex source = null!;
    private IVertex target = null!;

    [Params(1, 100, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var meta = fixture.CreateVertex("Meta");
        source = fixture.CreateVertex("Source");
        var edges = fixture.AddEdges(source, meta, EdgeCount);
        target = edges[0].To;
        target.Value = "A";
        QueryCurrentValue();
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvocation)]
    public int ChangeTargetValueAndQuery()
    {
        var resultCount = 0;

        for (var index = 0; index < OperationsPerInvocation; index++)
        {
            target.Value = index % 2 == 0 ? "B" : "A";
            resultCount += QueryCurrentValue();
        }

        return resultCount;
    }

    private int QueryCurrentValue()
    {
        source.QueryOutEdges("Meta", target.Value, out var result, out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
public class InheritedTargetValueMutationBenchmarks
{
    private const int OperationsPerInvocation = 100;

    private IVertex inheritedSource = null!;
    private IVertex target = null!;

    [Params(1, 10, 100)]
    public int InheritanceDepth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var hierarchy = fixture.CreateInheritanceChain(InheritanceDepth);
        var meta = fixture.CreateVertex("Meta");
        hierarchy[0].AddEdge(meta, fixture.CreateVertex("A"));
        inheritedSource = hierarchy[^1];
        target = AssertSingleQueryResult();
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvocation)]
    public int ChangeInheritedTargetValueAndQuery()
    {
        var resultCount = 0;

        for (var index = 0; index < OperationsPerInvocation; index++)
        {
            target.Value = index % 2 == 0 ? "B" : "A";
            inheritedSource.QueryOutEdges(
                "Meta",
                target.Value,
                out var result,
                out var results);
            resultCount += result != null ? 1 : results?.Count ?? 0;
        }

        return resultCount;
    }

    private IVertex AssertSingleQueryResult()
    {
        inheritedSource.QueryOutEdges("Meta", "A", out var result, out var results);
        var edge = result ?? results?.Single()
            ?? throw new InvalidOperationException("Expected one inherited edge.");
        return edge.To;
    }
}
