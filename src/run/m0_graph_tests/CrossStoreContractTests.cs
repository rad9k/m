using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store;

namespace m0_graph_tests;

public sealed class CrossStoreContractTests
{
    [Fact]
    public void CrossStoreEdgeRestoresPhysicalLinksAfterDetachAndAttach()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        var sourceStore = new MemoryStore(
            $"source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var targetStore = new MemoryStore(
            $"target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        try
        {
            var source = CreateVertex(sourceStore, "Source");
            var meta = CreateVertex(sourceStore, "Meta");
            var target = CreateVertex(targetStore, "Target");
            var edge = source.AddEdge(meta, target);
            Assert.Single(GraphUtil.GetQueryOut(source, "Meta", "Target"));
            Assert.Single(GraphUtil.GetQueryIn(target, "Meta", "Source"));

            sourceStore.Detach();

            Assert.Empty(target.InEdgesRaw);
            Assert.Empty(meta.MetaInEdgesRaw);
            Assert.Null(edge.To);
            Assert.Null(edge.Meta);
            Assert.Empty(GraphUtil.GetQueryIn(target, "Meta", "Source"));

            sourceStore.Attach();

            Assert.Same(target, edge.To);
            Assert.Same(meta, edge.Meta);
            Assert.Contains(edge, target.InEdgesRaw);
            Assert.Contains(edge, meta.MetaInEdgesRaw);
            Assert.Same(edge, Assert.Single(GraphUtil.GetQueryOut(source, "Meta", "Target")));
            Assert.Same(edge, Assert.Single(GraphUtil.GetQueryIn(target, "Meta", "Source")));
        }
        finally
        {
            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }
    }

    [Fact]
    public void InDetachRemovesEveryIncomingEdgeAndInvalidatesWarmIndex()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        var sourceStore = new MemoryStore(
            $"source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var targetStore = new MemoryStore(
            $"target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        try
        {
            var firstSource = CreateVertex(sourceStore, "FirstSource");
            var secondSource = CreateVertex(sourceStore, "SecondSource");
            var meta = CreateVertex(sourceStore, "Meta");
            var target = CreateVertex(targetStore, "Target");
            firstSource.AddEdge(meta, target);
            secondSource.AddEdge(meta, target);
            Assert.Equal(2, GraphUtil.GetQueryIn(target, "Meta", null).Count);

            targetStore.InDetach(sourceStore);

            Assert.Empty(target.InEdgesRaw);
            Assert.Empty(GraphUtil.GetQueryIn(target, "Meta", null));
            Assert.Empty(firstSource.OutEdgesRaw);
            Assert.Empty(secondSource.OutEdgesRaw);
            targetStore.Attach();
        }
        finally
        {
            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }
    }

    [Fact]
    public void AttachFailureDoesNotLeavePartialReverseEdge()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        var sourceStore = new MemoryStore(
            $"source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var metaStore = new MemoryStore(
            $"meta-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var targetStore = new MemoryStore(
            $"target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        try
        {
            var source = CreateVertex(sourceStore, "Source");
            var meta = CreateVertex(metaStore, "Meta");
            var target = CreateVertex(targetStore, "Target");
            var edge = source.AddEdge(meta, target);
            sourceStore.Detach();
            MinusZero.Instance.RemoveStore(metaStore);

            Assert.ThrowsAny<Exception>(() => ((IDetachableEdge)edge).Attach());

            Assert.Null(edge.To);
            Assert.Null(edge.Meta);
            Assert.DoesNotContain(edge, target.InEdgesRaw);
            Assert.Equal(DetachStateEnum.Detached, ((IDetachableEdge)edge).DetachState);
        }
        finally
        {
            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(metaStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }
    }

    [Fact]
    public void AttachHookFailureRollsBackEveryPartialLink()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        var sourceStore = new MemoryStore(
            $"source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var targetStore = new MemoryStore(
            $"target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        try
        {
            var source = CreateVertex(sourceStore, "Source");
            var inheritsMeta = CreateVertex(sourceStore, "$Inherits");
            var target = new ThrowingAttachInEdgeVertex(targetStore)
            {
                Value = "Target"
            };
            var edge = source.AddEdge(inheritsMeta, target);
            sourceStore.Detach();
            target.ThrowAfterAttachInEdge = true;

            Assert.Throws<InvalidOperationException>(
                () => ((IDetachableEdge)edge).Attach());

            Assert.Null(edge.To);
            Assert.Null(edge.Meta);
            Assert.DoesNotContain(edge, target.InEdgesRaw);
            Assert.DoesNotContain(edge, target.InheritsInEdges);
            Assert.DoesNotContain(edge, inheritsMeta.MetaInEdgesRaw);
            Assert.False(edge.EdgeRemovalExecuting);
            Assert.Equal(DetachStateEnum.Detached, ((IDetachableEdge)edge).DetachState);
        }
        finally
        {
            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }
    }

    [Fact]
    public void AttachRejectsDisposedTargetBeforeMutatingReverseLinks()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        var sourceStore = new MemoryStore(
            $"source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var targetStore = new MemoryStore(
            $"target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        try
        {
            var source = CreateVertex(sourceStore, "Source");
            var meta = CreateVertex(sourceStore, "Meta");
            var target = CreateVertex(targetStore, "Target");
            var edge = source.AddEdge(meta, target);
            sourceStore.Detach();
            target.DisposedState = DisposeStateEnum.Disposed;

            Assert.ThrowsAny<Exception>(() => ((IDetachableEdge)edge).Attach());

            Assert.Null(edge.To);
            Assert.Null(edge.Meta);
            Assert.DoesNotContain(edge, target.InEdgesRaw);
            Assert.DoesNotContain(edge, meta.MetaInEdgesRaw);
            Assert.Equal(DetachStateEnum.Detached, ((IDetachableEdge)edge).DetachState);
        }
        finally
        {
            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }
    }

    [Fact]
    public void RemovingAlreadyDetachedEdgeLeavesRemovalGuardReset()
    {
        var accessLevels = new[] { AccessLevelEnum.NoRestrictions };
        var identifierSuffix = Guid.NewGuid().ToString("N");
        var sourceStore = new MemoryStore(
            $"source-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);
        var targetStore = new MemoryStore(
            $"target-{identifierSuffix}",
            MinusZero.Instance,
            accessLevels,
            true);

        try
        {
            var source = CreateVertex(sourceStore, "Source");
            var meta = CreateVertex(sourceStore, "Meta");
            var target = CreateVertex(targetStore, "Target");
            var edge = source.AddEdge(meta, target);
            sourceStore.Detach();

            source.OutEdgesRaw.Remove(edge);

            Assert.False(edge.EdgeRemovalExecuting);
            Assert.Empty(source.OutEdgesRaw);
        }
        finally
        {
            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }
    }

    private static EasyVertex CreateVertex(IStore store, object value)
    {
        return new EasyVertex(store)
        {
            Value = value
        };
    }

    private sealed class ThrowingAttachInEdgeVertex : EasyVertex
    {
        public ThrowingAttachInEdgeVertex(IStore store)
            : base(store)
        {
        }

        public bool ThrowAfterAttachInEdge { get; set; }

        public override void AttachInEdge(IEdge edge)
        {
            base.AttachInEdge(edge);

            if (ThrowAfterAttachInEdge)
                throw new InvalidOperationException("AttachInEdge test failure.");
        }
    }
}
