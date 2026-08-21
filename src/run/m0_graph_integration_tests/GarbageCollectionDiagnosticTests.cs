using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class GarbageCollectionDiagnosticTests
{
    public GarbageCollectionDiagnosticTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void NonTransactedEventOrphanIsDisposedByFollowingCommit()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var source = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "GcDiagnosticSource");
        IVertex? observedEventVertex = null;
        var listenerEdge =
            ExecutionFlowHelper.AddTriggerAndListener_NonTransacted(
                source,
                execution =>
                {
                    observedEventVertex =
                        GraphUtil.GetQueryOutFirst(
                            execution.Stack,
                            "event",
                            null);
                    return execution.Stack;
                });

        try
        {
            source.Value = "GcDiagnosticSourceChanged";

            var eventVertex = Assert.IsAssignableFrom<IVertex>(
                observedEventVertex);
            var tempStore = Assert.IsAssignableFrom<StoreBase>(
                MinusZero.Instance.TempStore);
            var eventIdentifier = eventVertex.Identifier;
            var ownedPayloadVertices =
                eventVertex.OutEdgesRaw
                    .Where(edge =>
                    {
                        var metaValue =
                            edge.Meta?.Value?.ToString();
                        return metaValue == "OldValue" ||
                            metaValue == "NewValue";
                    })
                    .Select(edge => edge.To)
                    .ToList();

            Assert.Empty(eventVertex.InEdgesRaw);
            Assert.Empty(eventVertex.MetaInEdgesRaw);
            Assert.Equal(0, eventVertex.ExternalReferenceCount);
            Assert.Equal(
                DisposeStateEnum.Live,
                eventVertex.DisposedState);
            Assert.Same(
                eventVertex,
                tempStore.VertexIdentifiersDictionary[eventIdentifier]);

            ExecutionFlowHelper.StartTransaction();
            ExecutionFlowHelper.CommitTransaction();

            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
            Assert.Equal(
                DisposeStateEnum.Disposed,
                eventVertex.DisposedState);
            Assert.False(
                tempStore.VertexIdentifiersDictionary.ContainsKey(
                    eventIdentifier));
            Assert.All(
                ownedPayloadVertices,
                payloadVertex =>
                {
                    Assert.Equal(
                        DisposeStateEnum.Disposed,
                        payloadVertex.DisposedState);
                    Assert.False(
                        tempStore.VertexIdentifiersDictionary.ContainsKey(
                            payloadVertex.Identifier));
                });
        }
        finally
        {
            GraphChangeTrigger.RemoveListener(
                listenerEdge);

            if (observedEventVertex?.DisposedState ==
                DisposeStateEnum.Live)
            {
                observedEventVertex.Dispose();
            }
        }
    }

    [Fact]
    public void TransactionalEventOrphanIsDisposedDuringOwningCommit()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var source = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "TransactionalGcDiagnosticSource");
        IVertex? observedEventVertex = null;
        var listenerEdge =
            ExecutionFlowHelper.AddTriggerAndListener(
                source,
                execution =>
                {
                    observedEventVertex =
                        GraphUtil.GetQueryOutFirst(
                            execution.Stack,
                            "event",
                            null);
                    return execution.Stack;
                });

        try
        {
            ExecutionFlowHelper.StartTransaction();
            source.Value =
                "TransactionalGcDiagnosticSourceChanged";
            ExecutionFlowHelper.CommitTransaction();

            var eventVertex = Assert.IsAssignableFrom<IVertex>(
                observedEventVertex);
            var tempStore = Assert.IsAssignableFrom<StoreBase>(
                MinusZero.Instance.TempStore);

            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
            Assert.Equal(
                DisposeStateEnum.Disposed,
                eventVertex.DisposedState);
            Assert.False(
                tempStore.VertexIdentifiersDictionary.ContainsKey(
                    eventVertex.Identifier));
        }
        finally
        {
            if (!ReferenceEquals(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper.RollbackTransaction();
            }

            GraphChangeTrigger.RemoveListener(
                listenerEdge);
        }
    }

    [Fact]
    public void RecursiveOrphansAreDisposedDuringOwningCommit()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var holder = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "RecursiveGcDiagnosticHolder");
        var parentEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "RecursiveGcDiagnosticParent");
        var parent = parentEdge.To;
        var child = parent.AddVertex(
            MinusZero.Instance.Empty,
            "RecursiveGcDiagnosticChild");
        var leaf = child.AddVertex(
            MinusZero.Instance.Empty,
            "RecursiveGcDiagnosticLeaf");

        ExecutionFlowHelper.StartTransaction();

        try
        {
            holder.DeleteEdge(parentEdge);
            ExecutionFlowHelper.CommitTransaction();

            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
            Assert.Equal(
                DisposeStateEnum.Disposed,
                parent.DisposedState);
            Assert.Equal(
                DisposeStateEnum.Disposed,
                child.DisposedState);
            Assert.Equal(
                DisposeStateEnum.Disposed,
                leaf.DisposedState);
        }
        finally
        {
            if (!ReferenceEquals(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper.RollbackTransaction();
            }
        }
    }

    [Fact]
    public void NestedCommitDoesNotDrainExplicitParentTransaction()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var holder = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "ExplicitParentGcHolder");
        var orphanEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "ExplicitParentGcOrphan");
        var orphan = orphanEdge.To;

        ExecutionFlowHelper.StartTransaction();
        var explicitParentTransaction =
            MinusZero.Instance.GetTopTransaction();

        try
        {
            holder.DeleteEdge(orphanEdge);

            ExecutionFlowHelper.StartTransaction();
            ExecutionFlowHelper.CommitTransaction();

            Assert.Same(
                explicitParentTransaction,
                MinusZero.Instance.GetTopTransaction());
            Assert.Equal(
                DisposeStateEnum.Live,
                orphan.DisposedState);

            ExecutionFlowHelper.RollbackTransaction();

            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
            Assert.Equal(
                DisposeStateEnum.Live,
                orphan.DisposedState);
            Assert.Contains(
                holder.OutEdgesRaw,
                edge => ReferenceEquals(
                    edge.To,
                    orphan));
        }
        finally
        {
            if (!ReferenceEquals(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper.RollbackTransaction();
            }
        }
    }
}
