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

    [Fact]
    public void SharedTargetSurvivesUntilLastIncomingEdgeIsRemoved()
    {
        var firstHolder = CreateRetainedVertex(
            "SharedTargetFirstHolder");
        var secondHolder = CreateRetainedVertex(
            "SharedTargetSecondHolder");
        var sharedTarget = CreateRetainedVertex(
            "SharedTarget");
        var rootEdge = FindIncomingEdgeFrom(
            sharedTarget,
            MinusZero.Instance.TempStore.Root);
        var firstEdge = firstHolder.AddEdge(
            MinusZero.Instance.Empty,
            sharedTarget);
        var secondEdge = secondHolder.AddEdge(
            MinusZero.Instance.Empty,
            sharedTarget);

        RunInTransaction(
            () =>
            {
                MinusZero.Instance.TempStore.Root.DeleteEdge(
                    rootEdge);
                firstHolder.DeleteEdge(firstEdge);
            });

        Assert.Equal(
            DisposeStateEnum.Live,
            sharedTarget.DisposedState);
        Assert.Single(sharedTarget.InEdgesRaw);

        RunInTransaction(
            () => secondHolder.DeleteEdge(secondEdge));

        Assert.Equal(
            DisposeStateEnum.Disposed,
            sharedTarget.DisposedState);
        AssertNotRegistered(sharedTarget);
    }

    [Fact]
    public void MetaUsagePreventsDisposalUntilLastMetaEdgeIsRemoved()
    {
        var metaHolder = CreateRetainedVertex(
            "MetaUsageHolder");
        var metaCandidate = metaHolder.AddVertex(
            MinusZero.Instance.Empty,
            "MetaUsageCandidate");
        var source = CreateRetainedVertex(
            "MetaUsageSource");
        var target = CreateRetainedVertex(
            "MetaUsageTarget");
        var metaCandidateHolderEdge =
            FindIncomingEdgeFrom(
                metaCandidate,
                metaHolder);
        var usageEdge = source.AddEdge(
            metaCandidate,
            target);

        RunInTransaction(
            () => metaHolder.DeleteEdge(
                metaCandidateHolderEdge));

        Assert.Empty(metaCandidate.InEdgesRaw);
        Assert.Single(metaCandidate.MetaInEdgesRaw);
        Assert.Equal(
            DisposeStateEnum.Live,
            metaCandidate.DisposedState);

        RunInTransaction(
            () => source.DeleteEdge(usageEdge));

        Assert.Equal(
            DisposeStateEnum.Disposed,
            metaCandidate.DisposedState);
        AssertNotRegistered(metaCandidate);
    }

    [Fact]
    public void OrphanReattachedBeforeCommitIsNotDisposed()
    {
        var originalHolder = CreateRetainedVertex(
            "ReattachedOriginalHolder");
        var replacementHolder = CreateRetainedVertex(
            "ReattachedReplacementHolder");
        var orphanEdge = originalHolder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "ReattachedCandidate");
        var candidate = orphanEdge.To;

        RunInTransaction(
            () =>
            {
                originalHolder.DeleteEdge(orphanEdge);
                replacementHolder.AddEdge(
                    MinusZero.Instance.Empty,
                    candidate);
            });

        Assert.Equal(
            DisposeStateEnum.Live,
            candidate.DisposedState);
        Assert.Single(candidate.InEdgesRaw);
        Assert.Same(
            replacementHolder,
            candidate.InEdgesRaw[0].From);
        AssertRegistered(candidate);
    }

    [Fact]
    public void ExternalReferencePinsOrphanUntilReleased()
    {
        var holder = CreateRetainedVertex(
            "ExternalReferenceHolder");
        var orphanEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "ExternalReferenceCandidate");
        var candidate = orphanEdge.To;
        candidate.AddExternalReference();

        RunInTransaction(
            () => holder.DeleteEdge(orphanEdge));

        Assert.Empty(candidate.InEdgesRaw);
        Assert.Empty(candidate.MetaInEdgesRaw);
        Assert.Equal(1, candidate.ExternalReferenceCount);
        Assert.Equal(
            DisposeStateEnum.Live,
            candidate.DisposedState);

        candidate.RemoveExternalReference();
        RunInTransaction(
            () =>
            {
            });

        Assert.Equal(
            DisposeStateEnum.Disposed,
            candidate.DisposedState);
        AssertNotRegistered(candidate);
    }

    [Fact]
    public void DuplicateGarbageCollectionSchedulingDisposesVertexOnce()
    {
        var holder = CreateRetainedVertex(
            "DuplicateSchedulingHolder");
        var orphanEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "DuplicateSchedulingCandidate");
        var candidate = orphanEdge.To;

        RunInTransaction(
            () =>
            {
                holder.DeleteEdge(orphanEdge);
                candidate.CheckIfShouldDispose();
                candidate.CheckIfShouldDispose();
                candidate.CheckIfShouldDispose();
            });

        Assert.Equal(
            DisposeStateEnum.Disposed,
            candidate.DisposedState);
        AssertNotRegistered(candidate);
    }

    [Fact]
    public void RollbackRestoresIncomingEdgeAndPreventsDisposal()
    {
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        var holder = CreateRetainedVertex(
            "RollbackGcHolder");
        var orphanEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "RollbackGcCandidate");
        var candidate = orphanEdge.To;
        ExecutionFlowHelper.StartTransaction();

        try
        {
            holder.DeleteEdge(orphanEdge);

            Assert.Empty(candidate.InEdgesRaw);

            ExecutionFlowHelper.RollbackTransaction();

            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
            Assert.Equal(
                DisposeStateEnum.Live,
                candidate.DisposedState);
            Assert.Contains(
                candidate.InEdgesRaw,
                edge => ReferenceEquals(
                    edge.From,
                    holder));
            AssertRegistered(candidate);
        }
        finally
        {
            RestoreAmbientTransaction(
                ambientTransaction);
        }
    }

    [Fact]
    public void StoreRootIsNeverDisposed()
    {
        var root = MinusZero.Instance.TempStore.Root;

        RunInTransaction(
            () =>
            {
                root.CheckIfShouldDispose();
                root.CheckIfShouldDispose();
            });

        Assert.True(root.IsRoot);
        Assert.Equal(
            DisposeStateEnum.Live,
            root.DisposedState);
        AssertRegistered(root);
    }

    [Fact]
    public void UnreachableCycleDocumentsReferenceCountingBehavior()
    {
        var holder = CreateRetainedVertex(
            "CycleHolder");
        var firstEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "CycleFirst");
        var secondEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "CycleSecond");
        var first = firstEdge.To;
        var second = secondEdge.To;
        first.AddEdge(
            MinusZero.Instance.Empty,
            second);
        second.AddEdge(
            MinusZero.Instance.Empty,
            first);

        RunInTransaction(
            () =>
            {
                holder.DeleteEdge(firstEdge);
                holder.DeleteEdge(secondEdge);
            });

        Assert.Single(first.InEdgesRaw);
        Assert.Single(second.InEdgesRaw);
        Assert.Equal(
            DisposeStateEnum.Live,
            first.DisposedState);
        Assert.Equal(
            DisposeStateEnum.Live,
            second.DisposedState);
        AssertRegistered(first);
        AssertRegistered(second);
    }

    [Fact]
    public void LargeRecursiveCascadeDisposesEveryVertex()
    {
        const int vertexCount = 100;
        var holder = CreateRetainedVertex(
            "LargeCascadeHolder");
        var firstEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "LargeCascade0");
        var cascadeVertices = new List<IVertex>
        {
            firstEdge.To
        };

        for (var index = 1; index < vertexCount; index++)
        {
            cascadeVertices.Add(
                cascadeVertices[^1].AddVertex(
                    MinusZero.Instance.Empty,
                    "LargeCascade" + index));
        }

        RunInTransaction(
            () => holder.DeleteEdge(firstEdge));

        Assert.All(
            cascadeVertices,
            vertex =>
            {
                Assert.Equal(
                    DisposeStateEnum.Disposed,
                    vertex.DisposedState);
                AssertNotRegistered(vertex);
            });
    }

    [Fact]
    public void WideCascadeDisposesParentAndEveryLeaf()
    {
        const int leafCount = 100;
        var holder = CreateRetainedVertex(
            "WideCascadeHolder");
        var parentEdge = holder.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "WideCascadeParent");
        var parent = parentEdge.To;
        var leaves = new List<IVertex>();

        for (var index = 0; index < leafCount; index++)
        {
            leaves.Add(
                parent.AddVertex(
                    MinusZero.Instance.Empty,
                    "WideCascadeLeaf" + index));
        }

        RunInTransaction(
            () => holder.DeleteEdge(parentEdge));

        Assert.Equal(
            DisposeStateEnum.Disposed,
            parent.DisposedState);
        AssertNotRegistered(parent);
        Assert.All(
            leaves,
            vertex =>
            {
                Assert.Equal(
                    DisposeStateEnum.Disposed,
                    vertex.DisposedState);
                AssertNotRegistered(vertex);
            });
    }

    private static IVertex CreateRetainedVertex(
        string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }

    private static IEdge FindIncomingEdgeFrom(
        IVertex vertex,
        IVertex expectedSource)
    {
        return Assert.Single(
            vertex.InEdgesRaw,
            edge => ReferenceEquals(
                edge.From,
                expectedSource));
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
            RestoreAmbientTransaction(
                ambientTransaction);
        }
    }

    private static void RestoreAmbientTransaction(
        ITransaction ambientTransaction)
    {
        if (!ReferenceEquals(
            ambientTransaction,
            MinusZero.Instance.GetTopTransaction()))
        {
            ExecutionFlowHelper.RollbackTransaction();
        }
    }

    private static void AssertRegistered(
        IVertex vertex)
    {
        var store = Assert.IsAssignableFrom<StoreBase>(
            vertex.Store);
        Assert.Same(
            vertex,
            store.VertexIdentifiersDictionary[
                vertex.Identifier]);
    }

    private static void AssertNotRegistered(
        IVertex vertex)
    {
        var store = Assert.IsAssignableFrom<StoreBase>(
            vertex.Store);
        Assert.False(
            store.VertexIdentifiersDictionary.ContainsKey(
                vertex.Identifier));
    }
}
