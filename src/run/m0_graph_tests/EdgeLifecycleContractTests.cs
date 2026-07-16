using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class EdgeLifecycleContractTests
{
    [Fact]
    public void AddEdgeWiresEveryPhysicalCollection()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");

        var edge = source.AddEdge(meta, target);

        Assert.Same(source, edge.From);
        Assert.Same(meta, edge.Meta);
        Assert.Same(target, edge.To);
        Assert.Contains(edge, source.OutEdgesRaw);
        Assert.Contains(edge, target.InEdgesRaw);
        Assert.Contains(edge, meta.MetaInEdgesRaw);
    }

    [Fact]
    public void DeleteEdgeUnwiresEveryPhysicalCollection()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(meta, target);

        source.DeleteEdge(edge);

        Assert.DoesNotContain(edge, source.OutEdgesRaw);
        Assert.DoesNotContain(edge, target.InEdgesRaw);
        Assert.DoesNotContain(edge, meta.MetaInEdgesRaw);
    }

    [Fact]
    public void IncomingEdgesRemainPhysicalWhenTargetHasInheritance()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var relationMeta = fixture.CreateVertex("Relation");
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var incomingToParent = source.AddEdge(relationMeta, parent);

        fixture.AddInheritance(child, parent);

        Assert.Contains(incomingToParent, parent.InEdgesRaw);
        Assert.DoesNotContain(incomingToParent, child.InEdgesRaw);
        Assert.Empty(m0.Graph.GraphUtil.GetQueryIn(child, "Relation", "Source"));
    }
}
