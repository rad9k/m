using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class DiagnosticsContractTests
{
    [Fact]
    public void EnabledCountersReportRebuildsAndScannedEdges()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        fixture.AddEdges(source, meta, 2);
        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        try
        {
            source.QueryOutEdges("Meta", null, out _, out _);
            var snapshot = GraphPerformanceCounters.GetSnapshot();

            Assert.Equal(1, snapshot.LogicalOutEdgesRebuilds);
            Assert.Equal(1, snapshot.QueryMetaIndexRebuilds);
            Assert.Equal(2, snapshot.RebuildScannedEdges);
        }
        finally
        {
            GraphPerformanceCounters.Enabled = false;
            GraphPerformanceCounters.Reset();
        }
    }
}
