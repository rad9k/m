using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[IterationTime(100)]
public class IncrementalOutgoingIndexBenchmarks
{
    private EasyVertex source = null!;
    private IVertex meta = null!;
    private IVertex mutationTarget = null!;

    [Params(1, 100, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        meta = fixture.CreateVertex("Relation");
        mutationTarget =
            fixture.CreateVertex("MutationTarget");
        fixture.AddEdges(source, meta, EdgeCount);
        WarmAllIndexes();
    }

    [Benchmark]
    public int AddQueryMetaRemoveQuery()
    {
        var edge =
            source.AddEdge(meta, mutationTarget);
        var countAfterAdd =
            QueryCount(source, "Relation", null);
        source.DeleteEdge(edge);
        return countAfterAdd +
            QueryCount(source, "Relation", null);
    }

    [Benchmark]
    public int AddQueryAllIndexesRemoveQueryAllIndexes()
    {
        var edge =
            source.AddEdge(meta, mutationTarget);
        var count = QueryAllIndexes("MutationTarget");
        source.DeleteEdge(edge);
        return count +
            QueryAllIndexes("MutationTarget");
    }

    [Benchmark]
    public int AddRemoveWithoutQuery()
    {
        var edge =
            source.AddEdge(meta, mutationTarget);
        source.DeleteEdge(edge);
        return source.OutEdgesRaw.Count;
    }

    private int QueryAllIndexes(string value)
    {
        var count =
            source.GetOutOdgesByMeta()
                .ContainsKey("Relation")
                ? 1
                : 0;
        count += QueryCount(
            source,
            "Relation",
            null);
        count += QueryCount(
            source,
            null,
            value);
        count += QueryCount(
            source,
            "Relation",
            value);
        return count;
    }

    private void WarmAllIndexes()
    {
        _ = source.GetOutOdgesByMeta();
        _ = QueryCount(
            source,
            "Relation",
            null);
        _ = QueryCount(
            source,
            null,
            "Target-0");
        _ = QueryCount(
            source,
            "Relation",
            "Target-0");
    }

    private static int QueryCount(
        IVertex vertex,
        object? meta,
        object? value)
    {
        vertex.QueryOutEdges(
            meta!,
            value!,
            out var result,
            out var results);
        return result != null
            ? 1
            : results?.Count ?? 0;
    }
}
