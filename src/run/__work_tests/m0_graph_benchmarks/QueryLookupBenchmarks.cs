using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class QueryLookupBenchmarks
{
    private IVertex directSource = null!;
    private IVertex derivedMetaSource = null!;
    private IVertex inheritedSource = null!;

    [Params(1, 100, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var directMeta = fixture.CreateVertex("DirectMeta");
        directSource = fixture.CreateVertex("DirectSource");
        fixture.AddEdges(directSource, directMeta, EdgeCount);

        var baseMeta = fixture.CreateVertex("BaseMeta");
        var derivedMeta = fixture.CreateVertex("DerivedMeta");
        fixture.AddInheritance(derivedMeta, baseMeta);
        derivedMetaSource = fixture.CreateVertex("DerivedMetaSource");
        derivedMetaSource.AddEdge(derivedMeta, fixture.CreateVertex("DerivedTarget"));

        var inheritedRelationMeta = fixture.CreateVertex("InheritedRelation");
        var parent = fixture.CreateVertex("Parent");
        inheritedSource = fixture.CreateVertex("Child");
        parent.AddEdge(inheritedRelationMeta, fixture.CreateVertex("InheritedTarget"));
        fixture.AddInheritance(inheritedSource, parent);

        QueryCount(directSource, "DirectMeta", null);
        QueryCount(directSource, "DirectMeta", "Target-0");
        QueryCount(directSource, "MissingMeta", null);
        QueryCount(derivedMetaSource, "BaseMeta", null);
        QueryCount(inheritedSource, "InheritedRelation", null);
    }

    [Benchmark(Baseline = true)]
    public int DirectMetaMany()
    {
        return QueryCount(directSource, "DirectMeta", null);
    }

    [Benchmark]
    public int DirectMetaAndValueSingleton()
    {
        return QueryCount(directSource, "DirectMeta", "Target-0");
    }

    [Benchmark]
    public int DirectMetaMiss()
    {
        return QueryCount(directSource, "MissingMeta", null);
    }

    [Benchmark]
    public int DerivedMetaMatchedByBaseMeta()
    {
        return QueryCount(derivedMetaSource, "BaseMeta", null);
    }

    [Benchmark]
    public int InheritedSourceEdge()
    {
        return QueryCount(inheritedSource, "InheritedRelation", null);
    }

    private static int QueryCount(IVertex vertex, object? meta, object? value)
    {
        vertex.QueryOutEdges(meta!, value!, out var result, out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
public class MetaVertexIdentityLookupBenchmarks
{
    private IVertex source = null!;
    private IVertex searchedMeta = null!;

    [Params(1, 100, 1000)]
    public int CandidateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");

        for (var candidateIndex = 0;
             candidateIndex < CandidateCount;
             candidateIndex++)
        {
            var meta = fixture.CreateVertex("SharedMetaValue");
            source.AddEdge(
                meta,
                fixture.CreateVertex($"Target-{candidateIndex}"));
            searchedMeta = meta;
        }

        GraphUtil.FindEdgeByMetaVertex(source, searchedMeta);
    }

    [Benchmark]
    public IEdge FindExactMetaAmongSameValueCandidates()
    {
        return GraphUtil.FindEdgeByMetaVertex(source, searchedMeta);
    }
}
