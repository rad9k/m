using m0.Foundation;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class CardinalityContractTests
{
    public CardinalityContractTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(-1, 3)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void MaxSourceCardinalityAllowsConfiguredEdgeCount(
        int? maximum,
        int allowedEdgeCount)
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var relationMeta = fixture.CreateVertex("Relation");
        ConfigureCardinality(
            fixture,
            relationMeta,
            "$MaxCardinality",
            maximum);

        VerifyAllowedEdgeCount(
            relationMeta,
            allowedEdgeCount,
            index => source,
            index => fixture.CreateVertex($"Target-{index}"));
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(-1, 3)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void MaxTargetCardinalityAllowsConfiguredIncomingEdgeCount(
        int? maximum,
        int allowedEdgeCount)
    {
        var fixture = new GraphFixture();
        var target = fixture.CreateVertex("Target");
        var relationMeta = fixture.CreateVertex("Relation");
        ConfigureCardinality(
            fixture,
            relationMeta,
            "$MaxTargetCardinality",
            maximum);

        VerifyAllowedEdgeCount(
            relationMeta,
            allowedEdgeCount,
            index => fixture.CreateVertex($"Source-{index}"),
            index => target);
    }

    private static void ConfigureCardinality(
        GraphFixture fixture,
        IVertex relationMeta,
        string cardinalityName,
        int? maximum)
    {
        if (maximum == null)
            return;

        relationMeta.AddVertex(
            fixture.CreateVertex(cardinalityName),
            maximum.Value);
    }

    private static void VerifyAllowedEdgeCount(
        IVertex relationMeta,
        int allowedEdgeCount,
        Func<int, IVertex> sourceFactory,
        Func<int, IVertex> targetFactory)
    {
        const int attemptCount = 3;

        for (var index = 0; index < attemptCount; index++)
        {
            var source = sourceFactory(index);
            var target = targetFactory(index);
            var validationError = VertexOperations.TestIfNewEdgeValid(
                source,
                relationMeta,
                target);

            if (index < allowedEdgeCount)
            {
                Assert.Null(validationError);
                source.AddEdge(relationMeta, target);
            }
            else
            {
                Assert.NotNull(validationError);
            }
        }
    }
}
