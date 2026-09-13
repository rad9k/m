using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class GraphHelperContractTests
{
    [Fact]
    public void FindEdgeByMetaVertexRequiresReferenceIdentity()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var firstMeta = fixture.CreateVertex("SameMeta");
        var secondMeta = fixture.CreateVertex("SameMeta");
        var firstEdge = source.AddEdge(
            firstMeta,
            fixture.CreateVertex("First"));
        var secondEdge = source.AddEdge(
            secondMeta,
            fixture.CreateVertex("Second"));

        Assert.Same(firstEdge, GraphUtil.FindEdgeByMetaVertex(source, firstMeta));
        Assert.Same(secondEdge, GraphUtil.FindEdgeByMetaVertex(source, secondMeta));
    }

    [Fact]
    public void FindEdgeByMetaVertexDoesNotMatchInheritedMeta()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var baseMeta = fixture.CreateVertex("BaseMeta");
        var derivedMeta = fixture.CreateVertex("DerivedMeta");
        fixture.AddInheritance(derivedMeta, baseMeta);
        var derivedEdge = source.AddEdge(
            derivedMeta,
            fixture.CreateVertex("Target"));

        Assert.Null(GraphUtil.FindEdgeByMetaVertex(source, baseMeta));
        Assert.Same(
            derivedEdge,
            GraphUtil.FindEdgeByMetaValue(source, "BaseMeta"));
    }

    [Fact]
    public void CreateOrReplaceEdgeReplacesOnlyExactMetaVertex()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var firstMeta = fixture.CreateVertex("SameMeta");
        var secondMeta = fixture.CreateVertex("SameMeta");
        var firstEdge = source.AddEdge(
            firstMeta,
            fixture.CreateVertex("First"));
        var secondEdge = source.AddEdge(
            secondMeta,
            fixture.CreateVertex("Second"));
        var replacementTarget = fixture.CreateVertex("Replacement");

        var replacementEdge = GraphUtil.CreateOrReplaceEdge(
            source,
            secondMeta,
            replacementTarget);

        Assert.Contains(firstEdge, source.OutEdgesRaw);
        Assert.DoesNotContain(secondEdge, source.OutEdgesRaw);
        Assert.Same(secondMeta, replacementEdge.Meta);
        Assert.Same(replacementTarget, replacementEdge.To);
        Assert.Equal(
            new[] { firstEdge, replacementEdge },
            GraphUtil.GetQueryOut(source, "SameMeta", null));
    }

    [Fact]
    public void CreateOrReplaceEdgeByValueCreatesWhenExactMetaIsMissing()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var unrelatedMeta = fixture.CreateVertex("Unrelated");
        var requestedMeta = fixture.CreateVertex("Requested");
        var unrelatedEdge = source.AddEdge(
            unrelatedMeta,
            fixture.CreateVertex("Existing"));

        var createdTarget = GraphUtil.CreateOrReplaceEdgeByValue(
            source,
            requestedMeta,
            "Created");

        Assert.Contains(unrelatedEdge, source.OutEdgesRaw);
        var createdEdge = Assert.Single(
            GraphUtil.GetQueryOut(source, "Requested", "Created"));
        Assert.Same(requestedMeta, createdEdge.Meta);
        Assert.Same(createdTarget, createdEdge.To);
    }

    [Fact]
    public void DeleteEdgeWithNullIsNoOp()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var edge = source.AddEdge(
            fixture.CreateVertex("Meta"),
            fixture.CreateVertex("Target"));

        source.DeleteEdge(null!);

        Assert.Equal(new[] { edge }, source.OutEdgesRaw);
    }
}
