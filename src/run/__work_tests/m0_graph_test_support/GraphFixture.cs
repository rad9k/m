using System.Threading;
using m0.Foundation;
using m0.Graph;
using m0.Store;

namespace m0_graph_test_support;

public sealed class GraphFixture
{
    private static int nextStoreIdentifier;

    public GraphFixture()
    {
        Universe = new GraphTestStoreUniverse();
        Store = new MemoryStore(
            $"graph-test-{Interlocked.Increment(ref nextStoreIdentifier)}",
            Universe,
            new[] { AccessLevelEnum.NoRestrictions },
            true);

        Root = Store.Root;
        Root.Value = "Root";
        InheritsMeta = CreateVertex("$Inherits");
    }

    public GraphTestStoreUniverse Universe { get; }

    public MemoryStore Store { get; }

    public IVertex Root { get; }

    public IVertex InheritsMeta { get; }

    public EasyVertex CreateVertex(object value)
    {
        var vertex = new EasyVertex(Store)
        {
            Value = value
        };

        return vertex;
    }

    public IEdge AddInheritance(IVertex child, IVertex parent)
    {
        return child.AddEdge(InheritsMeta, parent);
    }

    public IReadOnlyList<IEdge> AddEdges(
        IVertex source,
        IVertex meta,
        int count,
        string targetValuePrefix = "Target")
    {
        var edges = new List<IEdge>(count);

        for (var index = 0; index < count; index++)
            edges.Add(source.AddEdge(meta, CreateVertex($"{targetValuePrefix}-{index}")));

        return edges;
    }

    public IReadOnlyList<IVertex> CreateInheritanceChain(
        int inheritanceEdgeCount,
        string vertexValuePrefix = "Level")
    {
        if (inheritanceEdgeCount < 0)
            throw new ArgumentOutOfRangeException(nameof(inheritanceEdgeCount));

        var hierarchy = new List<IVertex>(inheritanceEdgeCount + 1)
        {
            CreateVertex($"{vertexValuePrefix}-0")
        };

        for (var level = 1; level <= inheritanceEdgeCount; level++)
        {
            var child = CreateVertex($"{vertexValuePrefix}-{level}");
            AddInheritance(child, hierarchy[level - 1]);
            hierarchy.Add(child);
        }

        return hierarchy;
    }

    public IReadOnlyList<IVertex> CreateInheritanceChildren(
        IVertex parent,
        int childCount,
        string childValuePrefix = "Child")
    {
        if (childCount < 0)
            throw new ArgumentOutOfRangeException(nameof(childCount));

        var children = new List<IVertex>(childCount);

        for (var index = 0; index < childCount; index++)
        {
            var child = CreateVertex($"{childValuePrefix}-{index}");
            AddInheritance(child, parent);
            children.Add(child);
        }

        return children;
    }
}
