using m0.Foundation;
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
    public void EphemeralStackConstructionDoesNotRegisterIdentifier()
    {
        var fixture = new GraphFixture();

        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);

        Assert.False(
            fixture.Store.VertexIdentifiersDictionary.ContainsKey(
                stack.Identifier));
    }

    [Fact]
    public void SpecializedStackLazilySupportsIncomingAndMetaIncomingEdges()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);

        var incomingEdge =
            source.AddEdge(meta, stack);
        var metaIncomingEdge =
            source.AddEdge(stack, target);

        Assert.Same(
            incomingEdge,
            Assert.Single(stack.InEdgesRaw));
        Assert.Same(
            metaIncomingEdge,
            Assert.Single(stack.MetaInEdgesRaw));
        Assert.False(stack.InEdgesRaw.IsReadOnly);
        Assert.False(stack.MetaInEdgesRaw.IsReadOnly);

        source.DeleteEdge(incomingEdge);
        source.DeleteEdge(metaIncomingEdge);

        Assert.Empty(stack.InEdgesRaw);
        Assert.Empty(stack.MetaInEdgesRaw);
    }

    [Fact]
    public void SpecializedStackLazilyPreservesOrdinaryInheritanceBookkeeping()
    {
        var fixture = new GraphFixture();
        var inherits =
            fixture.CreateVertex("$Inherits");
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);

        var outgoing =
            stack.AddEdge(inherits, parent);
        var incoming =
            child.AddEdge(inherits, stack);

        Assert.Same(
            outgoing,
            Assert.Single(stack.InheritsOutEdges));
        Assert.Same(
            incoming,
            Assert.Single(stack.InheritsInEdges));

        stack.DeleteEdge(outgoing);
        child.DeleteEdge(incoming);

        Assert.Empty(stack.InheritsOutEdges);
        Assert.Empty(stack.InheritsInEdges);
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
    public void LocalStackBindingShadowsParentFrameWithoutMerging()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var parentTarget =
            fixture.CreateVertex("ParentTarget");
        var childTarget =
            fixture.CreateVertex("ChildTarget");
        var parentEdge =
            source.AddEdge(localMeta, parentTarget);
        var childEdge =
            source.AddEdge(localMeta, childTarget);
        var parentStack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var childStack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        parentStack.AddRangeOriginalEdges(
            new[] { parentEdge });
        childStack.AddRangeOriginalEdges(
            new[] { childEdge });
        childStack.AddEdge(
            stackFrameMeta,
            parentStack);

        childStack.QueryOutEdges(
            "Local",
            null,
            out var result,
            out var results);

        Assert.Same(childEdge, result);
        Assert.Null(results);
    }

    [Fact]
    public void FirstParentFrameWinsWhenMultipleParentsExist()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var firstTarget =
            fixture.CreateVertex("First");
        var secondTarget =
            fixture.CreateVertex("Second");
        var firstEdge =
            source.AddEdge(localMeta, firstTarget);
        var secondEdge =
            source.AddEdge(localMeta, secondTarget);
        var firstParent =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var secondParent =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var childStack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        firstParent.AddRangeOriginalEdges(
            new[] { firstEdge });
        secondParent.AddRangeOriginalEdges(
            new[] { secondEdge });
        childStack.AddEdge(
            stackFrameMeta,
            firstParent);
        childStack.AddEdge(
            stackFrameMeta,
            secondParent);

        childStack.QueryOutEdges(
            "Local",
            null,
            out var result,
            out var results);

        Assert.Same(firstEdge, result);
        Assert.Null(results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(10_000)]
    public void DeepParentFrameLookupPreservesResult(
        int depth)
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(localMeta, target);
        var stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        var current =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        current.AddRangeOriginalEdges(
            new[] { edge });

        for (var index = 0;
            index < depth;
            index++)
        {
            var child =
                new NoInEdgeInOutVertexVertex(fixture.Store);
            child.AddEdge(
                stackFrameMeta,
                current);
            current = child;
        }

        current.QueryOutEdges(
            "Local",
            null,
            out var result,
            out var results);

        Assert.Same(edge, result);
        Assert.Null(results);
    }

    [Fact]
    public void ParentFrameCycleTerminatesMissingQuery()
    {
        var fixture = new GraphFixture();
        var stackFrameMeta =
            fixture.CreateVertex(
                "$StackFrameInherits");
        var first =
            new NoInEdgeInOutVertexVertex(
                fixture.Store);
        var second =
            new NoInEdgeInOutVertexVertex(
                fixture.Store);
        first.AddEdge(
            stackFrameMeta,
            second);
        second.AddEdge(
            stackFrameMeta,
            first);

        first.QueryOutEdges(
            "Missing",
            null,
            out var result,
            out var results);

        Assert.Null(result);
        Assert.Null(results);
    }

    [Fact]
    public void RemovingFirstParentFrameSelectsNextParent()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var firstEdge =
            source.AddEdge(
                localMeta,
                fixture.CreateVertex("First"));
        var secondEdge =
            source.AddEdge(
                localMeta,
                fixture.CreateVertex("Second"));
        var firstParent =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var secondParent =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var child =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        firstParent.AddRangeOriginalEdges(
            new[] { firstEdge });
        secondParent.AddRangeOriginalEdges(
            new[] { secondEdge });
        var firstParentEdge =
            child.AddEdge(
                stackFrameMeta,
                firstParent);
        child.AddEdge(
            stackFrameMeta,
            secondParent);

        Assert.Same(
            firstEdge,
            Assert.Single(
                GraphUtil.GetQueryOut(
                    child,
                    "Local",
                    null)));

        child.DeleteEdge(firstParentEdge);

        Assert.Same(
            secondEdge,
            Assert.Single(
                GraphUtil.GetQueryOut(
                    child,
                    "Local",
                    null)));
    }

    [Fact]
    public void BatchedParentFrameEdgeParticipatesInFallback()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(localMeta, target);
        var parent =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        parent.AddRangeOriginalEdges(
            new[] { edge });
        var stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        var frameSource =
            fixture.CreateVertex("FrameSource");
        var originalParentEdge =
            frameSource.AddEdge(
                stackFrameMeta,
                parent);
        var child =
            new NoInEdgeInOutVertexVertex(fixture.Store);

        child.AddRangeOriginalEdges(
            new[] { originalParentEdge });

        Assert.Same(
            edge,
            Assert.Single(
                GraphUtil.GetQueryOut(
                    child,
                    "Local",
                    null)));
    }

    [Fact]
    public void ParentFrameCacheHitsAndRebuildsAfterParentRemoval()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var localMeta = fixture.CreateVertex("Local");
        var edge =
            source.AddEdge(
                localMeta,
                fixture.CreateVertex("Target"));
        var parent =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        parent.AddRangeOriginalEdges(
            new[] { edge });
        var child =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        var parentEdge =
            child.AddEdge(
                fixture.CreateVertex(
                    "$StackFrameInherits"),
                parent);
        _ = GraphUtil.GetQueryOut(
            child,
            "Local",
            null);

        Assert.Same(
            edge,
            Assert.Single(
                GraphUtil.GetQueryOut(
                    child,
                    "Local",
                    null)));

        child.DeleteEdge(parentEdge);

        Assert.Empty(
            GraphUtil.GetQueryOut(
                child,
                "Local",
                null));
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
    public void DeleteEdgeRemovesOneDuplicateAndKeepsReverseListsUnchanged()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var originalEdge = source.AddEdge(meta, target);
        var targetIncomingCount =
            target.InEdgesRaw.Count;
        var metaIncomingCount =
            meta.MetaInEdgesRaw.Count;
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        stack.AddRangeOriginalEdges(
            new[] { originalEdge, originalEdge });

        stack.DeleteEdge(originalEdge);

        Assert.Same(
            originalEdge,
            Assert.Single(stack.OutEdgesRaw));
        Assert.Equal(
            targetIncomingCount,
            target.InEdgesRaw.Count);
        Assert.Equal(
            metaIncomingCount,
            meta.MetaInEdgesRaw.Count);
        Assert.Empty(stack.InEdgesRaw);
        Assert.Empty(stack.MetaInEdgesRaw);
    }

    [Fact]
    public void DeleteEdgesListRemovesRequestedEdgesAndRefreshesQuery()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var first =
            source.AddEdge(
                meta,
                fixture.CreateVertex("First"));
        var second =
            source.AddEdge(
                meta,
                fixture.CreateVertex("Second"));
        var third =
            source.AddEdge(
                meta,
                fixture.CreateVertex("Third"));
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        stack.AddRangeOriginalEdges(
            new[] { first, second, third });
        _ = GraphUtil.GetQueryOut(
            stack,
            "Meta",
            null);

        stack.DeleteEdgesList(
            new[] { first, third });

        Assert.Same(
            second,
            Assert.Single(stack.OutEdgesRaw));
        Assert.Same(
            second,
            Assert.Single(
                GraphUtil.GetQueryOut(
                    stack,
                    "Meta",
                    null)));
    }

    [Fact]
    public void NormalStackEdgeUsesStackAsSourceWithoutReverseWiring()
    {
        var fixture = new GraphFixture();
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var targetIncomingCount =
            target.InEdgesRaw.Count;
        var metaIncomingCount =
            meta.MetaInEdgesRaw.Count;
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);

        var edge = stack.AddEdge(meta, target);

        Assert.Same(stack, edge.From);
        Assert.Same(meta, edge.Meta);
        Assert.Same(target, edge.To);
        Assert.Same(edge, Assert.Single(stack.OutEdgesRaw));
        Assert.Equal(
            targetIncomingCount,
            target.InEdgesRaw.Count);
        Assert.Equal(
            metaIncomingCount,
            meta.MetaInEdgesRaw.Count);
        Assert.Empty(stack.InEdgesRaw);
        Assert.Empty(stack.MetaInEdgesRaw);
    }

    [Fact]
    public void WarmStackMutationUsesRebuildFallback()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var originalEdge = source.AddEdge(meta, target);
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        _ = GraphUtil.GetQueryOut(
            stack,
            "Meta",
            null);

        stack.AddEdgeForNoInEdgeInOutVertexVertex(
            originalEdge);

        Assert.Single(
            GraphUtil.GetQueryOut(
                stack,
                "Meta",
                null));
    }

    [Fact]
    public void OriginalEdgeBatchPreservesIdentityOrderAndDuplicates()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var firstTarget =
            fixture.CreateVertex("First");
        var secondTarget =
            fixture.CreateVertex("Second");
        var first = source.AddEdge(meta, firstTarget);
        var second = source.AddEdge(meta, secondTarget);
        var firstIncomingCount =
            firstTarget.InEdgesRaw.Count;
        var secondIncomingCount =
            secondTarget.InEdgesRaw.Count;
        var metaIncomingCount =
            meta.MetaInEdgesRaw.Count;
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        _ = GraphUtil.GetQueryOut(
            stack,
            "Meta",
            null);

        stack.AddRangeOriginalEdges(
            new[] { first, second, first });

        Assert.Equal(
            new[] { first, second, first },
            stack.OutEdgesRaw);
        Assert.Equal(
            new[] { first, second, first },
            GraphUtil.GetQueryOut(
                stack,
                "Meta",
                null));
        Assert.Equal(
            firstIncomingCount,
            firstTarget.InEdgesRaw.Count);
        Assert.Equal(
            secondIncomingCount,
            secondTarget.InEdgesRaw.Count);
        Assert.Equal(
            metaIncomingCount,
            meta.MetaInEdgesRaw.Count);
    }

    [Fact]
    public void EmptyOriginalEdgeBatchKeepsWarmIndexCurrent()
    {
        var fixture = new GraphFixture();
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        _ = GraphUtil.GetQueryOut(
            stack,
            "Meta",
            null);

        stack.AddRangeOriginalEdges(
            Array.Empty<IEdge>());
        Assert.Empty(
            GraphUtil.GetQueryOut(
                stack,
                "Meta",
                null));
    }

    [Fact]
    public void PartialOriginalEdgeBatchInvalidatesWarmIndex()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(meta, target);
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);
        _ = GraphUtil.GetQueryOut(
            stack,
            "Meta",
            null);

        Assert.Throws<InvalidOperationException>(
            () => stack.AddRangeOriginalEdges(
                YieldEdgeThenThrow(edge)));

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                stack,
                "Meta",
                null));
    }

    [Fact]
    public void OriginalEdgeStackHelperUsesSingleBatch()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(meta, target);
        var stack =
            new NoInEdgeInOutVertexVertex(fixture.Store);

        InstructionHelpers
            .AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                stack,
                new[] { edge, edge });

        Assert.Equal(
            new[] { edge, edge },
            stack.OutEdgesRaw);
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

    private static IEnumerable<IEdge>
        YieldEdgeThenThrow(IEdge edge)
    {
        yield return edge;
        throw new InvalidOperationException(
            "Expected test failure.");
    }
}
