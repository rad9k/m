using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

public enum OutgoingIndexMutationStrategy
{
    RebuildOnly,
    AdaptiveOne,
    AdaptiveLearning,
    AlwaysIncremental
}

public enum OutgoingIndexQueryPattern
{
    MetaOnly,
    AllWarmIndexes
}

public enum OutgoingIndexBurstPattern
{
    FiveFiveFive,
    FiftyHundredFifty
}

[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
[InvocationCount(1)]
public class IncrementalOutgoingBurstBenchmarks
{
    private EasyVertex source = null!;
    private IVertex meta = null!;
    private IVertex[] mutationTargets = null!;

    [Params(100, 1000)]
    public int EdgeCount { get; set; }

    [Params(1, 2, 5, 10, 50, 100)]
    public int BurstCount { get; set; }

    [Params(
        OutgoingIndexMutationStrategy.RebuildOnly,
        OutgoingIndexMutationStrategy.AdaptiveOne,
        OutgoingIndexMutationStrategy.AlwaysIncremental)]
    public OutgoingIndexMutationStrategy Strategy
    {
        get;
        set;
    }

    [Params(
        OutgoingIndexQueryPattern.MetaOnly,
        OutgoingIndexQueryPattern.AllWarmIndexes)]
    public OutgoingIndexQueryPattern QueryPattern
    {
        get;
        set;
    }

    [IterationSetup]
    public void SetupIteration()
    {
        var fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        meta = fixture.CreateVertex("Relation");
        fixture.AddEdges(source, meta, EdgeCount);
        mutationTargets =
            new IVertex[BurstCount];

        for (var index = 0;
            index < mutationTargets.Length;
            index++)
        {
            mutationTargets[index] =
                fixture.CreateVertex(
                    $"MutationTarget-{index}");
        }

        WarmRequiredIndexes();
        source.SetIncrementalOutIndexMutationBudget(
            Strategy switch
            {
                OutgoingIndexMutationStrategy.RebuildOnly =>
                    0,
                OutgoingIndexMutationStrategy.AdaptiveOne =>
                    1,
                OutgoingIndexMutationStrategy
                    .AlwaysIncremental =>
                    int.MaxValue,
                _ => throw new ArgumentOutOfRangeException()
            });
    }

    [Benchmark]
    public int AddBurstAndQuery()
    {
        for (var index = 0;
            index < mutationTargets.Length;
            index++)
        {
            source.AddEdge(
                meta,
                mutationTargets[index]);
        }

        return QueryPattern ==
            OutgoingIndexQueryPattern.MetaOnly
                ? QueryCount(
                    source,
                    "Relation",
                    null)
                : QueryAllIndexes();
    }

    private int QueryAllIndexes()
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
            "MutationTarget-0");
        count += QueryCount(
            source,
            "Relation",
            "MutationTarget-0");
        return count;
    }

    private void WarmRequiredIndexes()
    {
        if (QueryPattern ==
            OutgoingIndexQueryPattern.MetaOnly)
        {
            _ = QueryCount(
                source,
                "Relation",
                null);
            return;
        }

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

[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
[InvocationCount(1)]
public class IncrementalOutgoingWriteOnlyBurstBenchmarks
{
    private EasyVertex source = null!;
    private IVertex meta = null!;
    private IVertex[] mutationTargets = null!;

    [Params(100, 1000)]
    public int EdgeCount { get; set; }

    [Params(1, 2, 5, 10, 50, 100)]
    public int BurstCount { get; set; }

    [Params(
        OutgoingIndexMutationStrategy.RebuildOnly,
        OutgoingIndexMutationStrategy.AdaptiveOne,
        OutgoingIndexMutationStrategy.AlwaysIncremental)]
    public OutgoingIndexMutationStrategy Strategy
    {
        get;
        set;
    }

    [IterationSetup]
    public void SetupIteration()
    {
        var fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        meta = fixture.CreateVertex("Relation");
        fixture.AddEdges(source, meta, EdgeCount);
        mutationTargets =
            new IVertex[BurstCount];

        for (var index = 0;
            index < mutationTargets.Length;
            index++)
        {
            mutationTargets[index] =
                fixture.CreateVertex(
                    $"MutationTarget-{index}");
        }

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
        source.SetIncrementalOutIndexMutationBudget(
            Strategy switch
            {
                OutgoingIndexMutationStrategy.RebuildOnly =>
                    0,
                OutgoingIndexMutationStrategy.AdaptiveOne =>
                    1,
                OutgoingIndexMutationStrategy
                    .AlwaysIncremental =>
                    int.MaxValue,
                _ => throw new ArgumentOutOfRangeException()
            });
    }

    [Benchmark]
    public int AddBurstWithoutQuery()
    {
        for (var index = 0;
            index < mutationTargets.Length;
            index++)
        {
            source.AddEdge(
                meta,
                mutationTargets[index]);
        }

        return source.OutEdgesRaw.Count;
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

[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
[InvocationCount(1)]
public class IncrementalOutgoingRepeatedBurstBenchmarks
{
    private EasyVertex source = null!;
    private IVertex meta = null!;
    private IVertex[] mutationTargets = null!;

    [Params(100, 1000)]
    public int EdgeCount { get; set; }

    [Params(
        OutgoingIndexBurstPattern.FiveFiveFive,
        OutgoingIndexBurstPattern.FiftyHundredFifty)]
    public OutgoingIndexBurstPattern BurstPattern
    {
        get;
        set;
    }

    [Params(
        OutgoingIndexMutationStrategy.RebuildOnly,
        OutgoingIndexMutationStrategy.AdaptiveLearning,
        OutgoingIndexMutationStrategy.AlwaysIncremental)]
    public OutgoingIndexMutationStrategy Strategy
    {
        get;
        set;
    }

    [Params(
        OutgoingIndexQueryPattern.MetaOnly,
        OutgoingIndexQueryPattern.AllWarmIndexes)]
    public OutgoingIndexQueryPattern QueryPattern
    {
        get;
        set;
    }

    [IterationSetup]
    public void SetupIteration()
    {
        var fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        meta = fixture.CreateVertex("Relation");
        fixture.AddEdges(source, meta, EdgeCount);
        mutationTargets =
            new IVertex[
                BurstPattern ==
                    OutgoingIndexBurstPattern.FiveFiveFive
                    ? 15
                    : 200];

        for (var index = 0;
            index < mutationTargets.Length;
            index++)
        {
            mutationTargets[index] =
                fixture.CreateVertex(
                    $"MutationTarget-{index}");
        }

        WarmRequiredIndexes();

        if (Strategy !=
            OutgoingIndexMutationStrategy.AdaptiveLearning)
        {
            source.SetIncrementalOutIndexMutationBudget(
                Strategy ==
                    OutgoingIndexMutationStrategy.RebuildOnly
                    ? 0
                    : int.MaxValue);
        }
    }

    [Benchmark]
    public int AddThreeBurstsWithQueryAfterEach()
    {
        var targetOffset = 0;
        var result = 0;

        for (var phase = 0; phase < 3; phase++)
        {
            var phaseSize = GetPhaseSize(phase);

            for (var index = 0;
                index < phaseSize;
                index++)
            {
                source.AddEdge(
                    meta,
                    mutationTargets[
                        targetOffset + index]);
            }

            targetOffset += phaseSize;
            result += QueryPattern ==
                OutgoingIndexQueryPattern.MetaOnly
                    ? QueryCount(
                        source,
                        "Relation",
                        null)
                    : QueryAllIndexes(
                        $"MutationTarget-{targetOffset - 1}");
        }

        return result;
    }

    private int GetPhaseSize(int phase)
    {
        if (BurstPattern ==
            OutgoingIndexBurstPattern.FiveFiveFive)
            return 5;

        return phase == 1
            ? 100
            : 50;
    }

    private int QueryAllIndexes(string targetValue)
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
            targetValue);
        count += QueryCount(
            source,
            "Relation",
            targetValue);
        return count;
    }

    private void WarmRequiredIndexes()
    {
        if (QueryPattern ==
            OutgoingIndexQueryPattern.MetaOnly)
        {
            _ = QueryCount(
                source,
                "Relation",
                null);
            return;
        }

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
