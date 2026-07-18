using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class DiagnosticsContractTests
{
    [Fact]
    public void InheritedFirstQuerySharesLogicalScanAndLeavesOtherIndexesLazy()
    {
        var fixture = new GraphFixture();
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var meta = fixture.CreateVertex("Meta");
        fixture.AddInheritance(child, parent);
        parent.AddEdge(
            meta,
            fixture.CreateVertex("Existing"));
        _ = GraphUtil.GetQueryOut(
            child,
            "Meta",
            null);

        parent.AddEdge(
            meta,
            fixture.CreateVertex("Added"));
        var result = GraphUtil.GetQueryOut(
            child,
            "Meta",
            null);

        Assert.Equal(2, result.Count);

        Assert.True(
            ((EasyVertex)child)
                .GetOutOdgesByMeta()
                .ContainsKey("Meta"));
        Assert.Single(
            GraphUtil.GetQueryOut(
                child,
                null,
                "Added"));
        Assert.Single(
            GraphUtil.GetQueryOut(
                child,
                "Meta",
                "Added"));
    }
}
