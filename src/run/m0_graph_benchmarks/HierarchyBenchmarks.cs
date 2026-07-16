using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class DeepHierarchyQueryBenchmarks
{
    private IVertex root = null!;
    private IVertex deepestChild = null!;
    private IVertex relationMeta = null!;
    private IVertex target = null!;

    [Params(1, 10, 100)]
    public int InheritanceDepth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var hierarchy = fixture.CreateInheritanceChain(InheritanceDepth);
        root = hierarchy[0];
        deepestChild = hierarchy[^1];
        relationMeta = fixture.CreateVertex("Relation");
        target = fixture.CreateVertex("Target");
        root.AddEdge(relationMeta, target);
        QueryCount(deepestChild);
    }

    [Benchmark]
    public int QueryInheritedEdgeAtDeepestChild()
    {
        return QueryCount(deepestChild);
    }

    [Benchmark]
    public int AddRootEdgeQueryDeepestChildAndRemove()
    {
        var edge = root.AddEdge(relationMeta, target);
        var resultCount = QueryCount(deepestChild);
        root.DeleteEdge(edge);
        return resultCount;
    }

    private static int QueryCount(IVertex vertex)
    {
        vertex.QueryOutEdges("Relation", null, out var result, out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
public class WideHierarchyMutationBenchmarks
{
    private GraphFixture fixture = null!;
    private IVertex parent = null!;
    private IVertex relationMeta = null!;
    private IVertex target = null!;
    private IReadOnlyList<IVertex> children = null!;

    [Params(1, 100, 1000)]
    public int ChildCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        parent = fixture.CreateVertex("Parent");
        relationMeta = fixture.CreateVertex("Relation");
        target = fixture.CreateVertex("Target");
        children = fixture.CreateInheritanceChildren(parent, ChildCount);

        foreach (var child in children)
            QueryCount(child);
    }

    [Benchmark]
    public int AddAndRemoveParentEdge()
    {
        var edge = parent.AddEdge(relationMeta, target);
        parent.DeleteEdge(edge);
        return children.Count;
    }

    [Benchmark]
    public int AddQueryAllChildrenAndRemoveParentEdge()
    {
        var edge = parent.AddEdge(relationMeta, target);
        var resultCount = 0;

        foreach (var child in children)
            resultCount += QueryCount(child);

        parent.DeleteEdge(edge);
        return resultCount;
    }

    private static int QueryCount(IVertex vertex)
    {
        vertex.QueryOutEdges("Relation", null, out var result, out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
public class InheritanceValidationBenchmarks
{
    private IVertex child = null!;
    private IVertex parent = null!;
    private IVertex inheritsMeta = null!;

    [Params(0, 10, 100)]
    public int ParentDepth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var hierarchy = fixture.CreateInheritanceChain(ParentDepth);
        child = fixture.CreateVertex("Child");
        parent = hierarchy[^1];
        inheritsMeta = fixture.InheritsMeta;
    }

    [Benchmark]
    public int AddAndRemoveValidInheritanceEdge()
    {
        var edge = child.AddEdge(inheritsMeta, parent);
        child.DeleteEdge(edge);
        return child.OutEdgesRaw.Count;
    }
}
