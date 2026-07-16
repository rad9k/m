using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class QueryContractTests
{
    [Fact]
    public void QueryOutReturnsZeroOneAndManyWithoutChangingEdgeOrder()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var firstTarget = fixture.CreateVertex("First");
        var secondTarget = fixture.CreateVertex("Second");
        var firstEdge = source.AddEdge(meta, firstTarget);

        Assert.Empty(GraphUtil.GetQueryOut(source, "Missing", null));
        Assert.Equal(new[] { firstEdge }, GraphUtil.GetQueryOut(source, "Meta", null));

        var secondEdge = source.AddEdge(meta, secondTarget);

        Assert.Equal(new[] { firstEdge, secondEdge }, GraphUtil.GetQueryOut(source, "Meta", null));
        Assert.Equal(new[] { firstEdge }, GraphUtil.GetQueryOut(source, null, "First"));
        Assert.Equal(new[] { secondEdge }, GraphUtil.GetQueryOut(source, "Meta", "Second"));
    }

    [Fact]
    public void QueryOutValueIndexReflectsTargetValueChangeAfterWarmup()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Before");
        var edge = source.AddEdge(meta, target);

        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "Before"));

        target.Value = "After";

        Assert.Empty(GraphUtil.GetQueryOut(source, "Meta", "Before"));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "After"));
    }

    [Fact]
    public void QueryInValueIndexReflectsSourceValueChangeAfterWarmup()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Before");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(meta, target);

        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, "Meta", "Before"));

        source.Value = "After";

        Assert.Empty(GraphUtil.GetQueryIn(target, "Meta", "Before"));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, "Meta", "After"));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, "Meta", null));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, null, "After"));
    }

    [Fact]
    public void QueryPreservesParallelEdgeOrderAndIdentity()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var firstEdge = source.AddEdge(meta, target);
        var secondEdge = source.AddEdge(meta, target);

        var results = GraphUtil.GetQueryOut(source, "Meta", "Target");

        Assert.Equal(2, results.Count);
        Assert.Same(firstEdge, results[0]);
        Assert.Same(secondEdge, results[1]);
    }

    [Fact]
    public void QueryGroupsDistinctMetaVerticesWithEqualValues()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var firstMeta = fixture.CreateVertex("SameMetaValue");
        var secondMeta = fixture.CreateVertex("SameMetaValue");
        var firstEdge = source.AddEdge(firstMeta, fixture.CreateVertex("First"));
        var secondEdge = source.AddEdge(secondMeta, fixture.CreateVertex("Second"));

        var results = GraphUtil.GetQueryOut(source, "SameMetaValue", null);

        Assert.Equal(2, results.Count);
        Assert.Same(firstEdge, results[0]);
        Assert.Same(secondEdge, results[1]);
        Assert.NotSame(results[0].Meta, results[1].Meta);
    }
}
