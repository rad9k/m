using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
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
public class DeepHierarchyMutationBenchmarks
{
    private IVertex root = null!;
    private IVertex relationMeta = null!;
    private IVertex target = null!;

    [Params(1, 10, 100)]
    public int InheritanceDepth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var hierarchy =
            fixture.CreateInheritanceChain(InheritanceDepth);
        root = hierarchy[0];
        relationMeta = fixture.CreateVertex("Relation");
        target = fixture.CreateVertex("Target");

        foreach (var vertex in hierarchy)
            QueryCount(vertex);
    }

    [Benchmark]
    public int AddAndRemoveRootEdgeWithoutQuery()
    {
        var edge = root.AddEdge(relationMeta, target);
        root.DeleteEdge(edge);
        return root.OutEdgesRaw.Count;
    }

    private static int QueryCount(IVertex vertex)
    {
        vertex.QueryOutEdges(
            "Relation",
            null,
            out var result,
            out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
public class IndependentHierarchyMutationBenchmarks
{
    private IVertex firstParent = null!;
    private IVertex secondParent = null!;
    private IVertex secondChild = null!;
    private IVertex relationMeta = null!;
    private IVertex target = null!;

    [Params(1, 100, 1000)]
    public int ChildCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        firstParent = fixture.CreateVertex("FirstParent");
        secondParent = fixture.CreateVertex("SecondParent");
        relationMeta = fixture.CreateVertex("Relation");
        target = fixture.CreateVertex("Target");
        var firstChildren =
            fixture.CreateInheritanceChildren(
                firstParent,
                ChildCount,
                "FirstChild");
        var secondChildren =
            fixture.CreateInheritanceChildren(
                secondParent,
                ChildCount,
                "SecondChild");
        secondChild = secondChildren[0];

        foreach (var child in firstChildren)
            QueryCount(child);

        foreach (var child in secondChildren)
            QueryCount(child);
    }

    [Benchmark]
    public int MutateFirstHierarchyAndQuerySecond()
    {
        var edge =
            firstParent.AddEdge(relationMeta, target);
        var resultCount = QueryCount(secondChild);
        firstParent.DeleteEdge(edge);
        return resultCount;
    }

    private static int QueryCount(IVertex vertex)
    {
        vertex.QueryOutEdges(
            "Relation",
            null,
            out var result,
            out var results);
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

[MemoryDiagnoser]
[IterationTime(100)]
public class InheritedIndexRebuildBenchmarks
{
    private IVertex parent = null!;
    private EasyVertex child = null!;
    private IVertex mutationMeta = null!;
    private IVertex mutationTarget = null!;

    [Params(1, 100, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        parent = fixture.CreateVertex("Parent");
        child = fixture.CreateVertex("Child");
        fixture.AddInheritance(child, parent);
        fixture.AddEdges(
            parent,
            fixture.CreateVertex("ExistingMeta"),
            EdgeCount);
        mutationMeta =
            fixture.CreateVertex("MutationMeta");
        mutationTarget =
            fixture.CreateVertex("MutationTarget");
        WarmAllIndexes();
    }

    [Benchmark]
    public int FirstDirectMetaIndexAfterParentMutation()
    {
        var edge =
            parent.AddEdge(mutationMeta, mutationTarget);
        int count = child.GetOutOdgesByMeta()
            .ContainsKey("MutationMeta") ? 1 : 0;
        parent.DeleteEdge(edge);
        return count;
    }

    [Benchmark]
    public int FirstQueryMetaIndexAfterParentMutation()
    {
        var edge =
            parent.AddEdge(mutationMeta, mutationTarget);
        int count = QueryCount(
            child,
            "MutationMeta",
            null);
        parent.DeleteEdge(edge);
        return count;
    }

    [Benchmark]
    public int FirstValueIndexAfterParentMutation()
    {
        var edge =
            parent.AddEdge(mutationMeta, mutationTarget);
        int count = QueryCount(
            child,
            null,
            "MutationTarget");
        parent.DeleteEdge(edge);
        return count;
    }

    [Benchmark]
    public int FirstMetaAndValueIndexAfterParentMutation()
    {
        var edge =
            parent.AddEdge(mutationMeta, mutationTarget);
        int count = QueryCount(
            child,
            "MutationMeta",
            "MutationTarget");
        parent.DeleteEdge(edge);
        return count;
    }

    [Benchmark]
    public int AllIndexesAfterParentMutation()
    {
        var edge =
            parent.AddEdge(mutationMeta, mutationTarget);
        int count = child.GetOutOdgesByMeta()
            .ContainsKey("MutationMeta") ? 1 : 0;
        count += QueryCount(
            child,
            "MutationMeta",
            null);
        count += QueryCount(
            child,
            null,
            "MutationTarget");
        count += QueryCount(
            child,
            "MutationMeta",
            "MutationTarget");
        parent.DeleteEdge(edge);
        return count;
    }

    private void WarmAllIndexes()
    {
        _ = child.GetOutOdgesByMeta();
        _ = QueryCount(child, "ExistingMeta", null);
        _ = QueryCount(child, null, "Target-0");
        _ = QueryCount(
            child,
            "ExistingMeta",
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
