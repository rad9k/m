using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class GraphChangeEventGarbageCollectionTests
{
    [Fact]
    public void EdgeAddedEventsAndEveryEdgePayloadAreDisposedAfterCommit()
    {
        var source = CreateRetainedVertex(
            "EdgeAddedEventSource");
        var meta = CreateRetainedVertex(
            "EdgeAddedEventMeta");
        var target = CreateRetainedVertex(
            "EdgeAddedEventTarget");
        var eventVertices = new HashSet<IVertex>();
        var edgePayloadVertices = new HashSet<IVertex>();
        var listenerEdges = AddDirectionalListeners(
            source,
            meta,
            target,
            GraphChangeFilterEnum.OutputEdgeAdded,
            GraphChangeFilterEnum.MetaEdgeAdded,
            GraphChangeFilterEnum.InputEdgeAdded,
            "EdgeAddedGc",
            execution => CaptureEventVertices(
                execution,
                eventVertices,
                edgePayloadVertices));

        try
        {
            RunInTransaction(
                () => source.AddEdge(
                    meta,
                    target));

            Assert.Equal(3, eventVertices.Count);
            Assert.Equal(3, edgePayloadVertices.Count);
            AssertEventGraphDisposed(
                eventVertices,
                edgePayloadVertices,
                source,
                meta,
                target);
        }
        finally
        {
            RemoveListenersAndCollect(
                listenerEdges);
        }
    }

    [Fact]
    public void EdgeRemovedEventsAndEveryEdgePayloadAreDisposedAfterCommit()
    {
        var source = CreateRetainedVertex(
            "EdgeRemovedEventSource");
        var meta = CreateRetainedVertex(
            "EdgeRemovedEventMeta");
        var target = CreateRetainedVertex(
            "EdgeRemovedEventTarget");
        var edgeToRemove = source.AddEdge(
            meta,
            target);
        var eventVertices = new HashSet<IVertex>();
        var edgePayloadVertices = new HashSet<IVertex>();
        var listenerEdges = AddDirectionalListeners(
            source,
            meta,
            target,
            GraphChangeFilterEnum.OutputEdgeRemoved,
            GraphChangeFilterEnum.MetaEdgeRemoved,
            GraphChangeFilterEnum.InputEdgeRemoved,
            "EdgeRemovedGc",
            execution => CaptureEventVertices(
                execution,
                eventVertices,
                edgePayloadVertices));

        try
        {
            RunInTransaction(
                () => source.DeleteEdge(
                    edgeToRemove));

            Assert.Equal(3, eventVertices.Count);
            Assert.Equal(3, edgePayloadVertices.Count);
            AssertEventGraphDisposed(
                eventVertices,
                edgePayloadVertices,
                source,
                meta,
                target);
        }
        finally
        {
            RemoveListenersAndCollect(
                listenerEdges);
        }
    }

    [Fact]
    public void MultipleListenersShareEventsWithoutRetainingThem()
    {
        const int listenerCount = 8;
        var source = CreateRetainedVertex(
            "MultipleListenerEventSource");
        var callbackCount = 0;
        var eventVertices = new HashSet<IVertex>();
        var listenerEdges = new List<IEdge>();

        for (var index = 0; index < listenerCount; index++)
        {
            listenerEdges.Add(
                ExecutionFlowHelper.AddTriggerAndListener(
                    source,
                    execution =>
                    {
                        callbackCount++;
                        return CaptureEventVertices(
                            execution,
                            eventVertices,
                            null);
                    }));
        }

        try
        {
            RunInTransaction(
                () => source.Value =
                    "MultipleListenerEventSourceChanged");

            Assert.Equal(
                listenerCount,
                callbackCount);
            Assert.Single(eventVertices);
            Assert.All(
                eventVertices,
                AssertDisposedAndUnregistered);
        }
        finally
        {
            RemoveListenersAndCollect(
                listenerEdges);
        }
    }

    [Fact]
    public void RepeatedBaseEdgeNavigationDoesNotGrowTempStoreRegistry()
    {
        const int navigationCount = 50;
        var baseEdgeMeta = MinusZero.Instance.Root.Get(
            false,
            @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        var firstVisualiser = CreateRetainedVertex(
            "NavigationFirstVisualiser");
        var secondVisualiser = CreateRetainedVertex(
            "NavigationSecondVisualiser");
        var destinations = new[]
        {
            CreateRetainedVertex("NavigationDestinationA"),
            CreateRetainedVertex("NavigationDestinationB"),
            CreateRetainedVertex("NavigationDestinationC")
        };
        firstVisualiser.AddEdge(
            baseEdgeMeta,
            destinations[0]);
        secondVisualiser.AddEdge(
            baseEdgeMeta,
            destinations[0]);
        var callbackCount = 0;
        var listenerEdges = new List<IEdge>
        {
            ExecutionFlowHelper.AddTriggerAndListener(
                firstVisualiser,
                execution =>
                {
                    callbackCount++;
                    return execution.Stack;
                }),
            ExecutionFlowHelper.AddTriggerAndListener(
                secondVisualiser,
                execution =>
                {
                    callbackCount++;
                    return execution.Stack;
                })
        };

        try
        {
            Navigate(
                firstVisualiser,
                secondVisualiser,
                baseEdgeMeta,
                destinations[1]);
            RunInTransaction(
                () =>
                {
                });

            var tempStore = Assert.IsAssignableFrom<StoreBase>(
                MinusZero.Instance.TempStore);
            var baselineRegistryCount =
                tempStore.VertexIdentifiersDictionary.Count;

            for (var index = 0; index < navigationCount; index++)
            {
                Navigate(
                    firstVisualiser,
                    secondVisualiser,
                    baseEdgeMeta,
                    destinations[index % destinations.Length]);

                Assert.Equal(
                    baselineRegistryCount,
                    tempStore.VertexIdentifiersDictionary.Count);
                Assert.DoesNotContain(
                    tempStore.VertexIdentifiersDictionary.Values,
                    IsLiveGraphChangeEvent);

                foreach (var destination in destinations)
                {
                    Assert.DoesNotContain(
                        destination.InEdgesRaw,
                        edge => edge.From.DisposedState !=
                            DisposeStateEnum.Live);
                }
            }

            Assert.True(callbackCount > 0);
        }
        finally
        {
            RemoveListenersAndCollect(
                listenerEdges);
        }
    }

    [Fact]
    public void CommittedTempStoreContainsNoEdgesFromDisposedSources()
    {
        var source = CreateRetainedVertex(
            "DisposedSourceInvariantSource");
        var meta = CreateRetainedVertex(
            "DisposedSourceInvariantMeta");
        var target = CreateRetainedVertex(
            "DisposedSourceInvariantTarget");
        var listenerEdge =
            ExecutionFlowHelper.AddTriggerAndListener(
                source,
                execution => execution.Stack);

        try
        {
            RunInTransaction(
                () => source.AddEdge(
                    meta,
                    target));

            var tempStore = Assert.IsAssignableFrom<StoreBase>(
                MinusZero.Instance.TempStore);

            foreach (var vertex in
                tempStore.VertexIdentifiersDictionary.Values)
            {
                Assert.Equal(
                    DisposeStateEnum.Live,
                    vertex.DisposedState);

                foreach (var edge in vertex.InEdgesRaw)
                {
                    Assert.Equal(
                        DisposeStateEnum.Live,
                        edge.From.DisposedState);
                    Assert.Contains(
                        edge,
                        edge.From.OutEdgesRaw);
                }

                foreach (var edge in vertex.MetaInEdgesRaw)
                {
                    Assert.Equal(
                        DisposeStateEnum.Live,
                        edge.From.DisposedState);
                    Assert.Contains(
                        edge,
                        edge.From.OutEdgesRaw);
                }
            }
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

    private static IList<IEdge> AddDirectionalListeners(
        IVertex source,
        IVertex meta,
        IVertex target,
        GraphChangeFilterEnum outputFilter,
        GraphChangeFilterEnum metaFilter,
        GraphChangeFilterEnum inputFilter,
        string triggerNamePrefix,
        ExecutionFlowHelper.DotNetDelegate listener)
    {
        return new List<IEdge>
        {
            ExecutionFlowHelper.AddTriggerAndListener(
                source,
                new List<string>(),
                new List<GraphChangeFilterEnum>
                {
                    outputFilter
                },
                triggerNamePrefix + "Output",
                listener),
            ExecutionFlowHelper.AddTriggerAndListener(
                meta,
                new List<string>(),
                new List<GraphChangeFilterEnum>
                {
                    metaFilter
                },
                triggerNamePrefix + "Meta",
                listener),
            ExecutionFlowHelper.AddTriggerAndListener(
                target,
                new List<string>(),
                new List<GraphChangeFilterEnum>
                {
                    inputFilter
                },
                triggerNamePrefix + "Input",
                listener)
        };
    }

    private static INoInEdgeInOutVertexVertex CaptureEventVertices(
        IExecution execution,
        ISet<IVertex> eventVertices,
        ISet<IVertex>? edgePayloadVertices)
    {
        foreach (var eventEdge in GraphUtil.GetQueryOut(
            execution.Stack,
            "event",
            null))
        {
            var eventVertex = eventEdge.To;
            eventVertices.Add(eventVertex);
            var edgePayload = GraphUtil.GetQueryOutFirst(
                eventVertex,
                "Edge",
                null);

            if (edgePayload != null)
                edgePayloadVertices?.Add(edgePayload);
        }

        return execution.Stack;
    }

    private static void AssertEventGraphDisposed(
        IEnumerable<IVertex> eventVertices,
        IEnumerable<IVertex> edgePayloadVertices,
        params IVertex[] referencedVertices)
    {
        Assert.All(
            eventVertices,
            AssertDisposedAndUnregistered);
        Assert.All(
            edgePayloadVertices,
            payloadVertex =>
            {
                AssertDisposedAndUnregistered(
                    payloadVertex);
                Assert.Empty(
                    payloadVertex.OutEdgesRaw);

                foreach (var referencedVertex in
                    referencedVertices)
                {
                    Assert.DoesNotContain(
                        referencedVertex.InEdgesRaw,
                        edge => ReferenceEquals(
                            edge.From,
                            payloadVertex));
                    Assert.DoesNotContain(
                        referencedVertex.MetaInEdgesRaw,
                        edge => ReferenceEquals(
                            edge.From,
                            payloadVertex));
                }
            });
    }

    private static void AssertDisposedAndUnregistered(
        IVertex vertex)
    {
        Assert.Equal(
            DisposeStateEnum.Disposed,
            vertex.DisposedState);
        var store = Assert.IsAssignableFrom<StoreBase>(
            vertex.Store);
        Assert.False(
            store.VertexIdentifiersDictionary.ContainsKey(
                vertex.Identifier));
    }

    private static bool IsLiveGraphChangeEvent(
        IVertex vertex)
    {
        if (vertex.DisposedState != DisposeStateEnum.Live)
            return false;

        return GraphUtil.GetQueryOutFirst(
                vertex,
                "Trigger",
                null) != null &&
            GraphUtil.GetQueryOutFirst(
                vertex,
                "Source",
                null) != null &&
            GraphUtil.GetQueryOutFirst(
                vertex,
                "Type",
                null) != null;
    }

    private static void Navigate(
        IVertex firstVisualiser,
        IVertex secondVisualiser,
        IVertex baseEdgeMeta,
        IVertex destination)
    {
        RunInTransaction(
            () =>
            {
                GraphUtil.ReplaceEdge(
                    firstVisualiser,
                    baseEdgeMeta,
                    destination);
                GraphUtil.ReplaceEdge(
                    secondVisualiser,
                    baseEdgeMeta,
                    destination);
            });
    }

    private static void RemoveListenersAndCollect(
        IEnumerable<IEdge> listenerEdges)
    {
        foreach (var listenerEdge in listenerEdges)
            GraphChangeTrigger.RemoveListener(
                listenerEdge);

        RunInTransaction(
            () =>
            {
            });
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
}
