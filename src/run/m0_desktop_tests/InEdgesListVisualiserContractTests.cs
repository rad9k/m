using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
using m0.ZeroTypes;

namespace m0_desktop_tests;

[Collection(DesktopGraphCollection.Name)]
public sealed class InEdgesListVisualiserContractTests
{
    public InEdgesListVisualiserContractTests(
        DesktopBootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void ItemsSourceMatchesPhysicalIncomingEdgesAfterCommit()
    {
        StaTestHost.Run(
            () =>
            {
                var source = CreateRetainedVertex(
                    "UiIncomingSource");
                var meta = CreateRetainedVertex(
                    "UiIncomingMeta");
                var target = CreateRetainedVertex(
                    "UiIncomingTarget");
                var visualiserAndListener =
                    CreateTrackingVisualiser(
                        target,
                        "UiIncomingSnapshot");
                var visualiser =
                    visualiserAndListener.Visualiser;

                try
                {
                    visualiser.BaseEdgeToUpdated();

                    RunInTransaction(
                        () => source.AddEdge(
                            meta,
                            target));

                    AssertItemsMatchPhysicalGraph(
                        visualiser,
                        target);
                }
                finally
                {
                    GraphChangeTrigger.RemoveListener(
                        visualiserAndListener.ListenerEdge);
                    RunInTransaction(
                        () =>
                        {
                        });
                }
            });
    }

    [Fact]
    public void ManualRefreshRemovesDisposedEventEdgesFromItemsSource()
    {
        StaTestHost.Run(
            () =>
            {
                var source = CreateRetainedVertex(
                    "UiRefreshSource");
                var meta = CreateRetainedVertex(
                    "UiRefreshMeta");
                var target = CreateRetainedVertex(
                    "UiRefreshTarget");
                var visualiserAndListener =
                    CreateTrackingVisualiser(
                        target,
                        "UiManualRefresh");
                var visualiser =
                    visualiserAndListener.Visualiser;

                try
                {
                    visualiser.BaseEdgeToUpdated();

                    RunInTransaction(
                        () => source.AddEdge(
                            meta,
                            target));

                    visualiser.BaseEdgeToUpdated();

                    AssertItemsMatchPhysicalGraph(
                        visualiser,
                        target);
                }
                finally
                {
                    GraphChangeTrigger.RemoveListener(
                        visualiserAndListener.ListenerEdge);
                    RunInTransaction(
                        () =>
                        {
                        });
                }
            });
    }

    private static void AssertItemsMatchPhysicalGraph(
        TestableInEdgesListVisualiser visualiser,
        IVertex target)
    {
        var physicalEdges =
            target.InEdgesRaw.ToList();
        var displayedEdges =
            visualiser.GetDisplayedEdges();

        Assert.Equal(
            physicalEdges.Count,
            displayedEdges.Count);
        Assert.All(
            displayedEdges,
            edge =>
            {
                Assert.Contains(
                    edge,
                    physicalEdges);
                Assert.Equal(
                    DisposeStateEnum.Live,
                    edge.From.DisposedState);
            });
    }

    private static IVertex CreateRetainedVertex(
        string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }

    private static (
        TestableInEdgesListVisualiser Visualiser,
        IEdge ListenerEdge)
        CreateTrackingVisualiser(
            IVertex target,
            string triggerName)
    {
        var visualiserState = CreateRetainedVertex(
            triggerName + "State");
        var baseEdgeMeta = MinusZero.Instance.Root.Get(
            false,
            @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        var baseEdgeVertex =
            EdgeHelper.CreateTempEdgeVertex(
                null,
                null,
                target);
        visualiserState.AddEdge(
            baseEdgeMeta,
            baseEdgeVertex);
        var showEventEdgesMeta = CreateRetainedVertex(
            "ShowFromToSourceChangedVertex");
        visualiserState.AddVertex(
            showEventEdgesMeta,
            "True");
        var visualiser =
            TestableInEdgesListVisualiser.Create(
                visualiserState);
        var listenerEdge =
            ExecutionFlowHelper.AddTriggerAndListener(
                target,
                new List<string>(),
                new List<GraphChangeFilterEnum>
                {
                    GraphChangeFilterEnum.InputEdgeAdded
                },
                triggerName,
                execution =>
                {
                    visualiser.BaseEdgeToUpdated();
                    return execution.Stack;
                });

        return (
            visualiser,
            listenerEdge);
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

    private sealed class TestableInEdgesListVisualiser :
        InEdgesListVisualiser
    {
        internal TestableInEdgesListVisualiser()
            : base(
                null,
                null,
                false)
        {
        }

        internal static TestableInEdgesListVisualiser Create(
            IVertex visualiserState)
        {
            var visualiser =
                (TestableInEdgesListVisualiser)
                    RuntimeHelpers.GetUninitializedObject(
                        typeof(
                            TestableInEdgesListVisualiser));
            var helper =
                (ListVisualiserHelper)
                    RuntimeHelpers.GetUninitializedObject(
                        typeof(
                            ListVisualiserHelper));
            helper.Vertex = visualiserState;
            visualiser.VisualiserHelper = helper;
            visualiser.ThisDataGrid =
                new DataGrid();
            return visualiser;
        }

        internal IList<IEdge> GetDisplayedEdges()
        {
            if (ThisDataGrid.ItemsSource is not
                IEnumerable itemsSource)
            {
                return new List<IEdge>();
            }

            return itemsSource
                .Cast<IEdge>()
                .ToList();
        }
    }
}
