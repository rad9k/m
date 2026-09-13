using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class IncrementalOutgoingIndexContractTests
{
    [Fact]
    public void WarmIndexesUpdateAcrossLocalAddAndRemoveWithoutRebuild()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Relation");
        var existingTarget =
            fixture.CreateVertex("Existing");
        var addedTarget =
            fixture.CreateVertex("Added");
        source.AddEdge(meta, existingTarget);
        WarmAllIndexes(source, "Relation", "Existing");

        var addedEdge =
            source.AddEdge(meta, addedTarget);

        Assert.Equal(
            new[] { addedEdge },
            GraphUtil.GetQueryOut(
                source,
                null,
                "Added"));
        Assert.Equal(
            new[] { addedEdge },
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                "Added"));
        Assert.Equal(
            2,
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                null).Count);
        Assert.True(
            source.GetOutOdgesByMeta()
                .ContainsKey("Relation"));

        source.DeleteEdge(addedEdge);

        Assert.Empty(
            GraphUtil.GetQueryOut(
                source,
                null,
                "Added"));
        Assert.Empty(
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                "Added"));
        Assert.Single(
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                null));
    }

    [Fact]
    public void RemovingFirstMiddleAndLastEdgesPreservesBucketOrder()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");
        var first = source.AddEdge(meta, target);
        var middle = source.AddEdge(meta, target);
        var last = source.AddEdge(meta, target);
        WarmAllIndexes(source, "Relation", "Target");

        source.DeleteEdge(middle);
        Assert.Equal(
            new[] { first, last },
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                "Target"));

        source.DeleteEdge(first);
        Assert.Equal(
            new[] { last },
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                "Target"));

        source.DeleteEdge(last);
        Assert.Empty(
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                "Target"));
        Assert.False(
            source.GetOutOdgesByMeta()
                .ContainsKey("Relation"));
    }

    [Fact]
    public void IncrementalDerivedMetaUsesInheritedQueryKeysOnlyForQueryView()
    {
        var fixture = new GraphFixture();
        var baseMeta =
            fixture.CreateVertex("BaseMeta");
        var derivedMeta =
            fixture.CreateVertex("DerivedMeta");
        var source = fixture.CreateVertex("Source");
        var target = fixture.CreateVertex("Target");
        fixture.AddInheritance(
            derivedMeta,
            baseMeta);
        WarmAllIndexes(source, "Missing", "Missing");

        var edge =
            source.AddEdge(derivedMeta, target);

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                source,
                "BaseMeta",
                null));
        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                source,
                "DerivedMeta",
                null));
        var directMeta =
            source.GetOutOdgesByMeta();
        Assert.True(
            directMeta.ContainsKey("DerivedMeta"));
        Assert.False(
            directMeta.ContainsKey("BaseMeta"));

        source.DeleteEdge(edge);
        Assert.Empty(
            GraphUtil.GetQueryOut(
                source,
                "BaseMeta",
                null));
    }

    [Fact]
    public void InheritedSourceMutationUsesFullRebuildFallback()
    {
        var fixture = new GraphFixture();
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");
        fixture.AddInheritance(child, parent);
        WarmAllIndexes(child, "Relation", "Target");

        var edge =
            child.AddEdge(meta, target);

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                child,
                "Relation",
                "Target"));
    }

    [Fact]
    public void ConfiguredMutationBudgetFallsBackAfterFiveWrites()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Relation");
        var targets =
            Enumerable.Range(0, 11)
                .Select(index =>
                    fixture.CreateVertex(
                        $"Target-{index}"))
                .ToArray();
        WarmAllIndexes(source, "Relation", "Target-0");
        source.SetIncrementalOutIndexMutationBudget(5);

        for (var index = 0; index < 5; index++)
            source.AddEdge(meta, targets[index]);

        Assert.Equal(
            5,
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                null).Count);

        for (var index = 5; index < 11; index++)
            source.AddEdge(meta, targets[index]);

        Assert.Equal(
            11,
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                null).Count);
    }

    [Fact]
    public void AdaptiveMutationBudgetLearnsRepeatedQueryBurst()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Relation");
        var firstTargets =
            Enumerable.Range(0, 5)
                .Select(index =>
                    fixture.CreateVertex(
                        $"First-{index}"))
                .ToArray();
        var secondTargets =
            Enumerable.Range(0, 5)
                .Select(index =>
                    fixture.CreateVertex(
                        $"Second-{index}"))
                .ToArray();
        WarmAllIndexes(source, "Relation", "Missing");

        foreach (var target in firstTargets)
            source.AddEdge(meta, target);

        Assert.Equal(
            5,
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                null).Count);

        foreach (var target in secondTargets)
            source.AddEdge(meta, target);

        Assert.Equal(
            10,
            GraphUtil.GetQueryOut(
                source,
                "Relation",
                null).Count);
    }

    [Fact]
    public void RandomLocalAddRemoveSequenceMatchesRawEdgeOracle()
    {
        const int stepCount = 200;
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var metas = new[]
        {
            fixture.CreateVertex("Meta-0"),
            fixture.CreateVertex("Meta-1"),
            fixture.CreateVertex("Meta-2")
        };
        var targets = new[]
        {
            fixture.CreateVertex("Target-0"),
            fixture.CreateVertex("Target-1"),
            fixture.CreateVertex("Target-2")
        };
        var liveEdges = new List<IEdge>();
        var random = new Random(14717);
        WarmAllIndexes(source, "Missing", "Missing");

        for (var step = 0; step < stepCount; step++)
        {
            if (liveEdges.Count == 0 ||
                random.Next(100) < 60)
            {
                var edge =
                    source.AddEdge(
                        metas[random.Next(metas.Length)],
                        targets[random.Next(targets.Length)]);
                liveEdges.Add(edge);
            }
            else
            {
                var removeIndex =
                    random.Next(liveEdges.Count);
                source.DeleteEdge(
                    liveEdges[removeIndex]);
                liveEdges.RemoveAt(removeIndex);
            }

            foreach (var meta in metas)
                foreach (var target in targets)
                {
                    var expected =
                        source.OutEdgesRaw
                            .Where(edge =>
                                Equals(
                                    edge.Meta.Value,
                                    meta.Value) &&
                                Equals(
                                    edge.To.Value,
                                    target.Value))
                            .ToArray();

                    Assert.Equal(
                        expected,
                        GraphUtil.GetQueryOut(
                            source,
                            meta.Value,
                            target.Value));
                }
        }
    }

    private static void WarmAllIndexes(
        EasyVertex source,
        object meta,
        object value)
    {
        _ = source.OutEdges;
        _ = source.GetOutOdgesByMeta();
        _ = GraphUtil.GetQueryOut(
            source,
            meta,
            null);
        _ = GraphUtil.GetQueryOut(
            source,
            null,
            value);
        _ = GraphUtil.GetQueryOut(
            source,
            meta,
            value);
    }
}
