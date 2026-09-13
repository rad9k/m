using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class TransactionGarbageCollectionFailureModeTests
{
    [Fact]
    public void ListenerCreatedOrphanIsCollectedInSameCommit()
    {
        var source = CreateRetainedVertex(
            "ListenerMutationSource");
        var holder = CreateRetainedVertex(
            "ListenerMutationHolder");
        var orphanEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "ListenerMutationOrphan");
        var orphan = orphanEdge.To;
        var listenerInvoked = false;
        var listenerEdge =
            ExecutionFlowHelper.AddTriggerAndListener(
                source,
                execution =>
                {
                    if (!listenerInvoked)
                    {
                        listenerInvoked = true;
                        holder.DeleteEdge(
                            orphanEdge);
                    }

                    return execution.Stack;
                });

        try
        {
            RunInTransaction(
                () => source.Value =
                    "ListenerMutationSourceChanged");

            Assert.True(listenerInvoked);
            Assert.Equal(
                DisposeStateEnum.Disposed,
                orphan.DisposedState);
            AssertNotRegistered(orphan);
        }
        finally
        {
            GraphChangeTrigger.RemoveListener(
                listenerEdge);
            RunInTransaction(
                () =>
                {
                });
        }
    }

    [Fact]
    public void ListenerExceptionDoesNotPinEventOrLeaveChildTransactionCurrent()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var source = CreateRetainedVertex(
            "ThrowingListenerSource");
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
                    throw new InvalidOperationException(
                        "Expected listener failure.");
                });
        Exception? commitException = null;
        ITransaction? transactionAfterException = null;
        DisposeStateEnum eventStateAfterException =
            DisposeStateEnum.Live;
        var eventExternalReferencesAfterException = -1;

        ExecutionFlowHelper.StartTransaction();
        source.Value = "ThrowingListenerSourceChanged";

        try
        {
            try
            {
                ExecutionFlowHelper.CommitTransaction();
            }
            catch (Exception exception)
            {
                commitException = exception;
                transactionAfterException =
                    MinusZero.Instance.GetTopTransaction();

                if (observedEventVertex != null)
                {
                    eventStateAfterException =
                        observedEventVertex.DisposedState;
                    eventExternalReferencesAfterException =
                        observedEventVertex
                            .ExternalReferenceCount;
                }
            }
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

            if (observedEventVertex != null)
            {
                while (observedEventVertex
                    .ExternalReferenceCount > 0)
                {
                    observedEventVertex
                        .RemoveExternalReference();
                }
            }

            RunInTransaction(
                () =>
                {
                });
        }

        Assert.IsType<InvalidOperationException>(
            commitException);
        Assert.Same(
            ambientTransaction,
            transactionAfterException);
        Assert.Equal(
            0,
            eventExternalReferencesAfterException);
        Assert.Equal(
            DisposeStateEnum.Disposed,
            eventStateAfterException);
    }

    [Fact]
    public void GarbageCollectionRequestWithoutTransactionIsNotLost()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var holder = CreateRetainedVertex(
            "NoTransactionHolder");
        var orphanEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "NoTransactionOrphan");
        var orphan = orphanEdge.To;
        DisposeStateEnum stateAfterFollowingCommit;
        bool registeredAfterFollowingCommit;

        MinusZero.Instance.SetTopTransaction(
            null);

        try
        {
            holder.DeleteEdge(orphanEdge);
        }
        finally
        {
            MinusZero.Instance.SetTopTransaction(
                ambientTransaction);
        }

        RunInTransaction(
            () =>
            {
            });

        stateAfterFollowingCommit =
            orphan.DisposedState;
        registeredAfterFollowingCommit =
            IsRegistered(orphan);

        if (orphan.DisposedState ==
            DisposeStateEnum.Live)
        {
            orphan.Dispose();
        }

        Assert.Equal(
            DisposeStateEnum.Disposed,
            stateAfterFollowingCommit);
        Assert.False(
            registeredAfterFollowingCommit);
    }

    private static IVertex CreateRetainedVertex(
        string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }

    private static void RunInTransaction(
        Action action)
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        ExecutionFlowHelper.StartTransaction();

        try
        {
            action();
            ExecutionFlowHelper.CommitTransaction();
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

    private static bool IsRegistered(
        IVertex vertex)
    {
        var store = Assert.IsAssignableFrom<StoreBase>(
            vertex.Store);
        return store.VertexIdentifiersDictionary.ContainsKey(
            vertex.Identifier);
    }

    private static void AssertNotRegistered(
        IVertex vertex)
    {
        Assert.False(IsRegistered(vertex));
    }
}
