using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Commands;

namespace m0_desktop_tests;

[Collection(DesktopGraphCollection.Name)]
public sealed class SynchronisedHelperContractTests
{
    public SynchronisedHelperContractTests(
        DesktopBootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DetailDisposalRemovesBothHelperListeners(
        bool useSelectedSelectedHelper)
    {
        var master = CreateRetainedVertex(
            "SynchronisedMaster");
        var selectedEdgesMeta = CreateRetainedVertex(
            "SelectedEdges");
        var selectedEdges = master.AddVertex(
            selectedEdgesMeta,
            "");
        var detailHost = CreateRetainedVertex(
            "SynchronisedDetailHost");
        var detailMeta = CreateRetainedVertex(
            "SynchronisedDetail");
        var detail = new EasyVertex(
            MinusZero.Instance.TempStore)
        {
            Value = "SynchronisedDetail"
        };
        var detailEdge = detailHost.AddEdge(
            detailMeta,
            detail);

        if (useSelectedSelectedHelper)
        {
            _ = new SellectedSelectedSynchronisedHelper(
                master,
                detail);
        }
        else
        {
            _ = new FirstSelectedEdgeSynchronisedHelper(
                master,
                detail);
        }

        Assert.Equal(
            1,
            GraphUtil.GetQueryOutCount(
                selectedEdges,
                "$GraphChangeTrigger",
                null));
        Assert.Equal(
            1,
            GraphUtil.GetQueryOutCount(
                detailHost,
                "$GraphChangeTrigger",
                null));

        RunInTransaction(
            () => detailHost.DeleteEdge(detailEdge));

        Assert.Equal(
            0,
            GraphUtil.GetQueryOutCount(
                selectedEdges,
                "$GraphChangeTrigger",
                null));
        Assert.Equal(
            0,
            GraphUtil.GetQueryOutCount(
                detailHost,
                "$GraphChangeTrigger",
                null));
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
        ITransaction ambientTransaction =
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
