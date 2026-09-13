using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class DeepCopyContractTests
{
    [Fact]
    public void DeepCopyCreatesIndependentVerticesForSimpleTree()
    {
        var fixture = new GraphFixture();
        var childMeta = fixture.CreateVertex("Child");
        var original = fixture.CreateVertex("Original");
        var originalChild = fixture.CreateVertex("ChildValue");
        original.AddEdge(childMeta, originalChild);
        var copy = fixture.CreateVertex("CopyPlaceholder");

        GraphUtil.DeepCopyByVertex(original, copy);

        var copiedChild = Assert.Single(GraphUtil.GetQueryOut(copy, "Child", null)).To;
        Assert.Equal("Original", copy.Value);
        Assert.Equal("ChildValue", copiedChild.Value);
        Assert.NotSame(original, copy);
        Assert.NotSame(originalChild, copiedChild);
    }

    [Fact]
    public void DeepCopyPreservesSharedChildInsideCopiedSubgraph()
    {
        var fixture = new GraphFixture();
        var branchMeta = fixture.CreateVertex("Branch");
        var sharedMeta = fixture.CreateVertex("Shared");
        var original = fixture.CreateVertex("Original");
        var left = fixture.CreateVertex("Left");
        var right = fixture.CreateVertex("Right");
        var shared = fixture.CreateVertex("SharedValue");
        original.AddEdge(branchMeta, left);
        original.AddEdge(branchMeta, right);
        left.AddEdge(sharedMeta, shared);
        right.AddEdge(sharedMeta, shared);
        var copy = fixture.CreateVertex("CopyPlaceholder");

        GraphUtil.DeepCopyByVertex(original, copy);

        var copiedBranches = GraphUtil.GetQueryOut(copy, "Branch", null);
        var copiedLeftShared = Assert.Single(
            GraphUtil.GetQueryOut(copiedBranches[0].To, "Shared", null)).To;
        var copiedRightShared = Assert.Single(
            GraphUtil.GetQueryOut(copiedBranches[1].To, "Shared", null)).To;
        Assert.NotSame(shared, copiedLeftShared);
        Assert.Same(copiedLeftShared, copiedRightShared);
    }

    [Fact]
    public void DeepCopyMapsCycleBackToCopiedRoot()
    {
        var fixture = new GraphFixture();
        var childMeta = fixture.CreateVertex("Child");
        var backReferenceMeta = fixture.CreateVertex("BackReference");
        var original = fixture.CreateVertex("Original");
        var child = fixture.CreateVertex("ChildValue");
        original.AddEdge(childMeta, child);
        child.AddEdge(backReferenceMeta, original);
        var copy = fixture.CreateVertex("CopyPlaceholder");

        GraphUtil.DeepCopyByVertex(original, copy);

        var copiedChild = Assert.Single(
            GraphUtil.GetQueryOut(copy, "Child", null)).To;
        var copiedBackReference = Assert.Single(
            GraphUtil.GetQueryOut(
                copiedChild,
                "BackReference",
                null)).To;
        Assert.NotSame(child, copiedChild);
        Assert.Same(copy, copiedBackReference);
    }

    [Fact]
    public void DeepCopyRemapsMetaVertexInsideCopiedScope()
    {
        var fixture = new GraphFixture();
        var definitionMeta = fixture.CreateVertex("Definition");
        var localMeta = fixture.CreateVertex("LocalMeta");
        var original = fixture.CreateVertex("Original");
        var child = fixture.CreateVertex("Child");
        original.AddEdge(definitionMeta, localMeta);
        original.AddEdge(localMeta, child);
        var copy = fixture.CreateVertex("CopyPlaceholder");

        GraphUtil.DeepCopyByVertex(original, copy);

        var copiedLocalMeta = Assert.Single(
            GraphUtil.GetQueryOut(copy, "Definition", null)).To;
        var copiedChildEdge = Assert.Single(
            copy.OutEdgesRaw,
            edge => ReferenceEquals(
                edge.Meta,
                copiedLocalMeta));
        Assert.NotSame(localMeta, copiedLocalMeta);
        Assert.NotSame(child, copiedChildEdge.To);
    }
}
