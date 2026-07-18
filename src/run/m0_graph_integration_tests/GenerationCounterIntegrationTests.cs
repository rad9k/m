using m0;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0_graph_test_support;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class GenerationCounterIntegrationTests
{
    public GenerationCounterIntegrationTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void InheritedQuerySeesWritesAndRollbackInsideTransaction()
    {
        var fixture = new GraphFixture();
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var meta = fixture.CreateVertex("Relation");
        var firstTarget =
            fixture.CreateVertex("FirstTarget");
        var secondTarget =
            fixture.CreateVertex("SecondTarget");
        fixture.AddInheritance(child, parent);
        _ = GraphUtil.GetQueryOut(
            child,
            "Relation",
            null);
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();

        ExecutionFlowHelper.StartTransaction();

        try
        {
            var firstEdge =
                parent.AddEdge(meta, firstTarget);
            Assert.Equal(
                new[] { firstEdge },
                GraphUtil.GetQueryOut(
                    child,
                    "Relation",
                    null));

            var secondEdge =
                parent.AddEdge(meta, secondTarget);
            Assert.Equal(
                new[] { firstEdge, secondEdge },
                GraphUtil.GetQueryOut(
                    child,
                    "Relation",
                    null));

            parent.DeleteEdge(firstEdge);
            Assert.Equal(
                new[] { secondEdge },
                GraphUtil.GetQueryOut(
                    child,
                    "Relation",
                    null));

            ExecutionFlowHelper.RollbackTransaction();

            Assert.Empty(
                GraphUtil.GetQueryOut(
                    child,
                    "Relation",
                    null));
            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
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
    public void LocalRollbackRestoresWarmIndexesIncrementally()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");
        _ = GraphUtil.GetQueryOut(
            source,
            "Relation",
            null);
        _ = GraphUtil.GetQueryOut(
            source,
            "Relation",
            "Target");
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        ExecutionFlowHelper.StartTransaction();

        try
        {
            var edge =
                source.AddEdge(meta, target);
            Assert.Equal(
                new[] { edge },
                GraphUtil.GetQueryOut(
                    source,
                    "Relation",
                    "Target"));

            ExecutionFlowHelper.RollbackTransaction();

            Assert.Empty(
                GraphUtil.GetQueryOut(
                    source,
                    "Relation",
                    "Target"));
            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
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
