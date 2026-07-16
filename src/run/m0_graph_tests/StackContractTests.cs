using m0.Graph;
using m0.ZeroCode.Helpers;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class StackContractTests
{
    [Fact]
    public void DirectStackConstructionRemainsRegistered()
    {
        var fixture = new GraphFixture();

        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);

        Assert.Same(
            stack,
            fixture.Store.VertexIdentifiersDictionary[
                stack.Identifier]);
    }

    [Fact]
    public void CopiedStackEdgePreservesOriginalEndpointsWithoutAddingIncomingEdges()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Local");
        var target = fixture.CreateVertex("Target");
        var originalEdge = source.AddEdge(meta, target);
        var targetIncomingCount = target.InEdgesRaw.Count;
        var metaIncomingCount = meta.MetaInEdgesRaw.Count;
        var stack = new NoInEdgeInOutVertexVertex(fixture.Store);

        stack.AddEdgeForNoInEdgeInOutVertexVertex(originalEdge);

        var copiedEdge = Assert.Single(stack.OutEdgesRaw);
        Assert.NotSame(originalEdge, copiedEdge);
        Assert.Same(source, copiedEdge.From);
        Assert.Same(meta, copiedEdge.Meta);
        Assert.Same(target, copiedEdge.To);
        Assert.Equal(targetIncomingCount, target.InEdgesRaw.Count);
        Assert.Equal(metaIncomingCount, meta.MetaInEdgesRaw.Count);
    }

    [Fact]
    public void StackQueryFallsBackToParentFrame()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var target = fixture.CreateVertex("Target");
        var originalEdge = source.AddEdge(localMeta, target);
        var parentStack = new NoInEdgeInOutVertexVertex(fixture.Store);
        var childStack = new NoInEdgeInOutVertexVertex(fixture.Store);
        var stackFrameMeta = fixture.CreateVertex("$StackFrameInherits");
        parentStack.AddEdgeForNoInEdgeInOutVertexVertex(originalEdge);
        childStack.AddEdge(stackFrameMeta, parentStack);

        childStack.QueryOutEdges("Local", null, out var result, out var results);

        var inheritedEdge = result ?? Assert.Single(results);
        Assert.Same(source, inheritedEdge.From);
        Assert.Same(target, inheritedEdge.To);
    }

    [Fact]
    public void OriginalEdgeStackPathPreservesIdentityOrderAndDuplicates()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var originalEdge = source.AddEdge(meta, target);
        var stack = new NoInEdgeInOutVertexVertex(fixture.Store);

        stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(originalEdge);
        stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(originalEdge);

        Assert.Equal(2, stack.OutEdgesRaw.Count);
        Assert.Same(originalEdge, stack.OutEdgesRaw[0]);
        Assert.Same(originalEdge, stack.OutEdgesRaw[1]);
    }

    [Fact]
    public void StackAcceptsArtificialEdgeWithNullSource()
    {
        var fixture = new GraphFixture();
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var artificialEdge = GraphUtil.CreateArtificialEdge(meta, target);
        var stack = new NoInEdgeInOutVertexVertex(fixture.Store);

        stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(artificialEdge);

        var stackEdge = Assert.Single(stack.OutEdgesRaw);
        Assert.Same(artificialEdge, stackEdge);
        Assert.Null(stackEdge.From);
    }

    [Fact]
    public void ConvertingExistingStackUsesZeroCopyAlias()
    {
        var fixture = new GraphFixture();
        var stack = new NoInEdgeInOutVertexVertex(fixture.Store);

        var converted = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(stack);

        Assert.Same(stack, converted);
    }
}
