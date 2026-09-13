using System.Linq;
using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
[IterationTime(100)]
public class IterativeTraversalBenchmarks
{
    private EasyVertex inheritanceLeaf = null!;
    private IVertex inheritanceRoot = null!;
    private EasyVertex graphRoot = null!;
    private GraphIterator graphIterator = null!;

    [Params(10, 100, 1000)]
    public int Depth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var inheritance = new EasyVertex[Depth + 1];

        for (var index = 0;
            index < inheritance.Length;
            index++)
            inheritance[index] =
                (EasyVertex)fixture.CreateVertex(
                    $"Type-{index}");

        inheritanceRoot = inheritance[0];
        inheritanceRoot.Value = "RootType";

        for (var index = 1;
            index < inheritance.Length;
            index++)
            AttachInheritanceWithoutValidation(
                inheritance[index],
                inheritance[index - 1],
                fixture.InheritsMeta);

        inheritanceLeaf = inheritance[^1];
        inheritanceRoot.AddEdge(
            fixture.CreateVertex("Relation"),
            fixture.CreateVertex("Target"));
        _ = QueryCount(
            inheritanceLeaf,
            "Relation");
        _ = VertexOperations.InheritanceCompare(
            inheritanceLeaf,
            "MissingType");

        graphRoot =
            (EasyVertex)fixture.CreateVertex(
                "GraphRoot");
        var graphMeta =
            fixture.CreateVertex("Child");
        IVertex current = graphRoot;

        for (var index = 0;
            index < Depth;
            index++)
        {
            var next =
                fixture.CreateVertex(
                    $"Node-{index}");
            current.AddEdge(
                graphMeta,
                next);
            current = next;
        }

        graphIterator =
            new GraphIterator("MissingValue");
    }

    [Benchmark]
    public int RebuildDeepInheritedQuery()
    {
        inheritanceLeaf
            .OutEdgesDictionariesNeedsRebuild =
            true;
        return QueryCount(
            inheritanceLeaf,
            "Relation");
    }

    [Benchmark]
    public bool CompareMissingInheritedType()
    {
        return VertexOperations.InheritanceCompare(
            inheritanceLeaf,
            "MissingType");
    }

    [Benchmark]
    public int CollectSubgraphWithoutLinks()
    {
        return GraphUtil
            .GetSubGraphWithoutLinksAsList(
                graphRoot)
            .Count();
    }

    [Benchmark]
    public int DeepIteratorMiss()
    {
        return GraphUtil.DeepIterator(
                graphRoot,
                graphIterator.Compare,
                false,
                false,
                true)
            .Count();
    }

    private static int QueryCount(
        IVertex vertex,
        object meta)
    {
        vertex.QueryOutEdges(
            meta,
            null,
            out IEdge single,
            out var multiple);
        return single == null
            ? multiple?.Count ?? 0
            : 1;
    }

    private static void
        AttachInheritanceWithoutValidation(
            IVertex child,
            IVertex parent,
            IVertex inheritsMeta)
    {
        var edge =
            new EasyEdge(
                child,
                inheritsMeta,
                parent);
        child.OutEdgesRaw.Add(edge);
        child.AttachEdge(edge);
        parent.AttachInEdge(edge);
    }
}
