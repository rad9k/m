using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class EventContractTests
{
    public EventContractTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void NonTransactedListenerRunsBeforeMutationReturns()
    {
        var vertex = CreateTestVertex("Before");
        var eventCount = 0;
        ExecutionFlowHelper.AddTriggerAndListener_NonTransacted(
            vertex,
            execution =>
            {
                eventCount++;
                return execution.Stack;
            });

        vertex.Value = "After";

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void TransactionalListenerIsDeferredUntilCommit()
    {
        var vertex = CreateTestVertex("Before");
        var eventCount = 0;
        ExecutionFlowHelper.AddTriggerAndListener(
            vertex,
            execution =>
            {
                eventCount++;
                return execution.Stack;
            });
        var ambientTransaction = MinusZero.Instance.GetTopTransaction();

        ExecutionFlowHelper.StartTransaction();

        try
        {
            vertex.Value = "First";
            vertex.Value = "Second";
            Assert.Equal(0, eventCount);

            ExecutionFlowHelper.CommitTransaction();

            Assert.True(eventCount > 0);
            Assert.Same(ambientTransaction, MinusZero.Instance.GetTopTransaction());
        }
        finally
        {
            if (!ReferenceEquals(ambientTransaction, MinusZero.Instance.GetTopTransaction()))
                ExecutionFlowHelper.RollbackTransaction();
        }
    }

    [Fact]
    public void TransactionalListenerReceivesOneNetValueChange()
    {
        var vertex = CreateTestVertex("Before");
        var eventCount = 0;
        object? oldValue = null;
        object? newValue = null;
        ExecutionFlowHelper.AddTriggerAndListener(
            vertex,
            execution =>
            {
                eventCount++;
                var eventVertex =
                    GraphUtil.GetQueryOutFirst(
                        execution.Stack,
                        "event",
                        null);
                oldValue = GraphUtil.GetQueryOutFirst(
                    eventVertex,
                    "OldValue",
                    null)?.Value;
                newValue = GraphUtil.GetQueryOutFirst(
                    eventVertex,
                    "NewValue",
                    null)?.Value;
                return execution.Stack;
            });

        ExecutionFlowHelper.StartTransaction();
        vertex.Value = "First";
        vertex.Value = "Second";
        ExecutionFlowHelper.CommitTransaction();

        Assert.Equal(1, eventCount);
        Assert.Equal("Before", oldValue);
        Assert.Equal("Second", newValue);
    }

    [Fact]
    public void NestedGraphChangeSuppressionRestoresPreviousState()
    {
        var transaction = MinusZero.Instance.GetTopTransaction();
        transaction.GraphChangeWatchActive = false;

        try
        {
            ExecutionFlowHelper.GraphChangeWatchOff();
            ExecutionFlowHelper.GraphChangeWatchOn();

            Assert.False(transaction.GraphChangeWatchActive);
        }
        finally
        {
            transaction.GraphChangeWatchActive = true;
        }
    }

    [Fact]
    public void AddedThenRemovedEdgeProducesNoListenerEvent()
    {
        var source = CreateTestVertex("Source");
        var meta = CreateTestVertex("Meta");
        var target = CreateTestVertex("Target");
        var eventCount = 0;
        ExecutionFlowHelper.AddTriggerAndListener(
            source,
            execution =>
            {
                eventCount++;
                return execution.Stack;
            });
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();

        ExecutionFlowHelper.StartTransaction();

        try
        {
            var edge = source.AddEdge(meta, target);
            source.DeleteEdge(edge);
            ExecutionFlowHelper.CommitTransaction();

            Assert.Equal(0, eventCount);
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
    public void NonTransactedListenerDoesNotRunInsideNestedTransaction()
    {
        var vertex = CreateTestVertex("Before");
        var eventCount = 0;
        ExecutionFlowHelper.AddTriggerAndListener_NonTransacted(
            vertex,
            execution =>
            {
                eventCount++;
                return execution.Stack;
            });
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();

        ExecutionFlowHelper.StartTransaction();

        try
        {
            vertex.Value = "After";
            Assert.Equal(0, eventCount);

            ExecutionFlowHelper.CommitTransaction();

            Assert.Equal(0, eventCount);
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
    public void CachedWatcherDefinitionsObserveScopeChangesAndDeduplicate()
    {
        var source = CreateTestVertex("Source");
        var childMeta = CreateTestVertex("Child");
        var child = CreateTestVertex("Child-Before");
        source.AddEdge(childMeta, child);
        var callbackCount = 0;
        var receivedEventCount = 0;
        var listenerRemoved = false;
        var listenerEdge =
            ExecutionFlowHelper.AddTriggerAndListener(
                source,
                new List<string>(),
                new List<GraphChangeFilterEnum>
                {
                    GraphChangeFilterEnum.ValueChange
                },
                $"DynamicScope-{Guid.NewGuid():N}",
                execution =>
                {
                    callbackCount++;
                    receivedEventCount +=
                        GraphUtil.GetQueryOutCount(
                            execution.Stack,
                            "event",
                            null);
                    return execution.Stack;
                });
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();

        try
        {
            ExecutionFlowHelper.StartTransaction();
            source.Value = "Source-After";
            ExecutionFlowHelper.CommitTransaction();
            Assert.Equal(1, callbackCount);

            callbackCount = 0;
            receivedEventCount = 0;
            var scopeQueryMeta =
                MinusZero.Instance.Root.Get(
                    false,
                    @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\ScopeQuery");
            listenerEdge.From.AddVertex(
                scopeQueryMeta,
                "Child:");
            listenerEdge.From.AddVertex(
                scopeQueryMeta,
                "Child:");

            ExecutionFlowHelper.StartTransaction();
            child.Value = "Child-After";
            ExecutionFlowHelper.CommitTransaction();

            Assert.Equal(1, callbackCount);
            Assert.Equal(1, receivedEventCount);

            GraphChangeTrigger.RemoveListener(listenerEdge);
            listenerRemoved = true;

            ExecutionFlowHelper.StartTransaction();
            child.Value = "Child-After-Removal";
            ExecutionFlowHelper.CommitTransaction();

            Assert.Equal(1, callbackCount);
            Assert.Equal(1, receivedEventCount);
        }
        finally
        {
            if (!ReferenceEquals(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper.RollbackTransaction();
            }

            if (!listenerRemoved)
                GraphChangeTrigger.RemoveListener(listenerEdge);
        }
    }

    private static IVertex CreateTestVertex(string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(MinusZero.Instance.Empty, value);
    }
}
