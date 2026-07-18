using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class GenerationCounterContractTests
{
    [Fact]
    public void WideMutationDefersDescendantInvalidationUntilQuery()
    {
        var fixture = new GraphFixture();
        var parent = fixture.CreateVertex("Parent");
        var children =
            fixture.CreateInheritanceChildren(parent, 32);
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");

        foreach (var child in children)
            Assert.Empty(
                GraphUtil.GetQueryOut(
                    child,
                    "Relation",
                    null));

        _ = GraphUtil.ExistQueryOut(
            meta,
            "$NoInherit",
            null);

        var edge = parent.AddEdge(meta, target);

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                children[^1],
                "Relation",
                null));
    }

    [Fact]
    public void UnrelatedHierarchyRemainsWarmAfterMutation()
    {
        var fixture = new GraphFixture();
        var firstParent =
            fixture.CreateVertex("FirstParent");
        var secondParent =
            fixture.CreateVertex("SecondParent");
        var firstChildren =
            fixture.CreateInheritanceChildren(
                firstParent,
                16,
                "FirstChild");
        var secondChildren =
            fixture.CreateInheritanceChildren(
                secondParent,
                16,
                "SecondChild");
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");

        foreach (var child in firstChildren)
            _ = GraphUtil.GetQueryOut(
                child,
                "Relation",
                null);

        foreach (var child in secondChildren)
            _ = GraphUtil.GetQueryOut(
                child,
                "Relation",
                null);

        firstParent.AddEdge(meta, target);

        Assert.Empty(
            GraphUtil.GetQueryOut(
                secondChildren[^1],
                "Relation",
                null));
    }

    [Fact]
    public void InheritedQuerySeesEveryAlternatingMutation()
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

        Assert.Empty(
            GraphUtil.GetQueryOut(
                child,
                "Relation",
                null));

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

        parent.DeleteEdge(secondEdge);
        Assert.Empty(
            GraphUtil.GetQueryOut(
                child,
                "Relation",
                null));
    }

    [Fact]
    public void TargetValueChangeDefersInheritedValueRebuild()
    {
        var fixture = new GraphFixture();
        IReadOnlyList<IVertex> hierarchy =
            fixture.CreateInheritanceChain(16);
        var root = hierarchy[0];
        var leaf = hierarchy[^1];
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Before");
        var edge = root.AddEdge(meta, target);

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                leaf,
                null,
                "Before"));

        target.Value = "After";

        Assert.Empty(
            GraphUtil.GetQueryOut(
                leaf,
                null,
                "Before"));
        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                leaf,
                null,
                "After"));
    }

    [Fact]
    public void ParentTopologyChangeIsVisibleToExistingDescendants()
    {
        var fixture = new GraphFixture();
        var newParent =
            fixture.CreateVertex("NewParent");
        var intermediate =
            fixture.CreateVertex("Intermediate");
        var child = fixture.CreateVertex("Child");
        var meta = fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");
        var inheritedEdge =
            newParent.AddEdge(meta, target);
        fixture.AddInheritance(child, intermediate);

        Assert.Empty(
            GraphUtil.GetQueryOut(
                child,
                "Relation",
                null));

        var topologyEdge =
            fixture.AddInheritance(
                intermediate,
                newParent);
        Assert.Equal(
            new[] { inheritedEdge },
            GraphUtil.GetQueryOut(
                child,
                "Relation",
                null));

        intermediate.DeleteEdge(topologyEdge);
        Assert.Empty(
            GraphUtil.GetQueryOut(
                child,
                "Relation",
                null));
    }

    [Fact]
    public void MetaInheritanceChangeRebuildsInheritedSourceQuery()
    {
        var fixture = new GraphFixture();
        var baseMeta =
            fixture.CreateVertex("BaseMeta");
        var derivedMeta =
            fixture.CreateVertex("DerivedMeta");
        var sourceParent =
            fixture.CreateVertex("SourceParent");
        var sourceChild =
            fixture.CreateVertex("SourceChild");
        var target = fixture.CreateVertex("Target");
        var edge =
            sourceParent.AddEdge(
                derivedMeta,
                target);
        fixture.AddInheritance(
            sourceChild,
            sourceParent);

        Assert.Empty(
            GraphUtil.GetQueryOut(
                sourceChild,
                "BaseMeta",
                null));

        var metaInheritance =
            fixture.AddInheritance(
                derivedMeta,
                baseMeta);
        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                sourceChild,
                "BaseMeta",
                null));

        derivedMeta.DeleteEdge(metaInheritance);
        Assert.Empty(
            GraphUtil.GetQueryOut(
                sourceChild,
                "BaseMeta",
                null));
    }

    [Fact]
    public void MetaValueChangeRefreshesInheritedDirectAndQueryIndexes()
    {
        var fixture = new GraphFixture();
        var meta = fixture.CreateVertex("BeforeMeta");
        var sourceParent =
            fixture.CreateVertex("SourceParent");
        var sourceChild =
            fixture.CreateVertex("SourceChild");
        var target = fixture.CreateVertex("Target");
        var edge =
            sourceParent.AddEdge(meta, target);
        fixture.AddInheritance(
            sourceChild,
            sourceParent);

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                sourceChild,
                "BeforeMeta",
                null));
        Assert.True(
            ((EasyVertex)sourceChild)
                .GetOutOdgesByMeta()
                .ContainsKey("BeforeMeta"));

        meta.Value = "AfterMeta";

        Assert.Empty(
            GraphUtil.GetQueryOut(
                sourceChild,
                "BeforeMeta",
                null));
        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                sourceChild,
                "AfterMeta",
                null));
        var directMetaIndex =
            ((EasyVertex)sourceChild)
                .GetOutOdgesByMeta();
        Assert.False(
            directMetaIndex.ContainsKey("BeforeMeta"));
        Assert.True(
            directMetaIndex.ContainsKey("AfterMeta"));
    }
}
