using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

public enum QueryResultDirection
{
    Out,
    In
}

public enum QueryResultCardinality
{
    Empty,
    Single,
    Many,
    FullScan
}

[MemoryDiagnoser]
[IterationTime(100)]
[WarmupCount(3)]
[IterationCount(10)]
public class QueryResultBenchmarks
{
    private const int ManyEdgeCount = 1000;

    private IVertex source = null!;
    private IVertex target = null!;
    private object? meta;
    private object? endpointValue;

    [ParamsAllValues]
    public QueryResultDirection Direction { get; set; }

    [ParamsAllValues]
    public QueryResultCardinality Cardinality { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var relationMeta =
            fixture.CreateVertex("Relation");
        source = fixture.CreateVertex("OutSource");
        target = fixture.CreateVertex("InTarget");

        for (var index = 0;
            index < ManyEdgeCount;
            index++)
        {
            source.AddEdge(
                relationMeta,
                fixture.CreateVertex(
                    $"OutTarget-{index}"));
            fixture.CreateVertex(
                    $"InSource-{index}")
                .AddEdge(
                    relationMeta,
                    target);
        }

        (meta, endpointValue) =
            Cardinality switch
            {
                QueryResultCardinality.Empty =>
                    ("Missing", null),
                QueryResultCardinality.Single =>
                    Direction ==
                    QueryResultDirection.Out
                        ? ("Relation", "OutTarget-0")
                        : ("Relation", "InSource-0"),
                QueryResultCardinality.Many =>
                    ("Relation", null),
                QueryResultCardinality.FullScan =>
                    (null, null),
                _ => throw new ArgumentOutOfRangeException()
            };

        _ = LegacyListWrapperCount();
    }

    [Benchmark]
    public int LegacyListWrapperCount()
    {
        return Direction ==
            QueryResultDirection.Out
                ? GraphUtil.GetQueryOut(
                    source,
                    meta!,
                    endpointValue!).Count
                : GraphUtil.GetQueryIn(
                    target,
                    meta!,
                    endpointValue!).Count;
    }

    [Benchmark]
    public int ReadOnlyResultCount()
    {
        return Direction ==
            QueryResultDirection.Out
                ? GraphUtil.GetQueryOutResult(
                    source,
                    meta!,
                    endpointValue!).Count
                : GraphUtil.GetQueryInResult(
                    target,
                    meta!,
                    endpointValue!).Count;
    }

    [Benchmark]
    public int LegacyListWrapperEnumerate()
    {
        IList<IEdge> result =
            Direction ==
            QueryResultDirection.Out
                ? GraphUtil.GetQueryOut(
                    source,
                    meta!,
                    endpointValue!)
                : GraphUtil.GetQueryIn(
                    target,
                    meta!,
                    endpointValue!);
        var count = 0;

        foreach (var edge in result)
        {
            if (edge != null)
                count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadOnlyResultEnumerate()
    {
        EdgeQueryResult result =
            Direction ==
            QueryResultDirection.Out
                ? GraphUtil.GetQueryOutResult(
                    source,
                    meta!,
                    endpointValue!)
                : GraphUtil.GetQueryInResult(
                    target,
                    meta!,
                    endpointValue!);
        var count = 0;

        foreach (var edge in result)
        {
            if (edge != null)
                count++;
        }

        return count;
    }
}
