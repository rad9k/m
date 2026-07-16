using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class InheritanceContractTests
{
    [Fact]
    public void LogicalOutEdgesContainParentRawEdges()
    {
        var fixture = new GraphFixture();
        var relationMeta = fixture.CreateVertex("Relation");
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var target = fixture.CreateVertex("Target");
        var parentEdge = parent.AddEdge(relationMeta, target);
        var inheritanceEdge = fixture.AddInheritance(child, parent);

        Assert.Equal(new[] { inheritanceEdge }, child.OutEdgesRaw);
        Assert.Contains(inheritanceEdge, child.OutEdges);
        Assert.Contains(parentEdge, child.OutEdges);
        Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, "Relation", null));
    }

    [Fact]
    public void QueryMetaMatchesDerivedMetaButDirectMetaViewDoesNotAliasIt()
    {
        var fixture = new GraphFixture();
        var baseMeta = fixture.CreateVertex("BaseMeta");
        var derivedMeta = fixture.CreateVertex("DerivedMeta");
        var source = fixture.CreateVertex("Source");
        var target = fixture.CreateVertex("Target");
        fixture.AddInheritance(derivedMeta, baseMeta);
        var edge = source.AddEdge(derivedMeta, target);

        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "BaseMeta", null));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "DerivedMeta", null));

        var directMetaView = ((EasyVertex)source).GetOutOdgesByMeta();

        Assert.True(directMetaView.ContainsKey("DerivedMeta"));
        Assert.False(directMetaView.ContainsKey("BaseMeta"));
    }

    [Fact]
    public void AddingParentEdgeInvalidatesAlreadyBuiltChildIndexes()
    {
        var fixture = new GraphFixture();
        var relationMeta = fixture.CreateVertex("Relation");
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var target = fixture.CreateVertex("Target");
        fixture.AddInheritance(child, parent);

        Assert.Empty(GraphUtil.GetQueryOut(child, "Relation", null));

        var parentEdge = parent.AddEdge(relationMeta, target);

        Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, "Relation", null));

        parent.DeleteEdge(parentEdge);

        Assert.Empty(GraphUtil.GetQueryOut(child, "Relation", null));
    }

    [Fact]
    public void ChangingMetaInheritanceInvalidatesAlreadyBuiltSourceIndex()
    {
        var fixture = new GraphFixture();
        var baseMeta = fixture.CreateVertex("BaseMeta");
        var derivedMeta = fixture.CreateVertex("DerivedMeta");
        var source = fixture.CreateVertex("Source");
        var edge = source.AddEdge(derivedMeta, fixture.CreateVertex("Target"));

        Assert.Empty(GraphUtil.GetQueryOut(source, "BaseMeta", null));

        var inheritanceEdge = fixture.AddInheritance(derivedMeta, baseMeta);

        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "BaseMeta", null));

        derivedMeta.DeleteEdge(inheritanceEdge);

        Assert.Empty(GraphUtil.GetQueryOut(source, "BaseMeta", null));
    }

    [Fact]
    public void InheritanceListsTrackPhysicalInheritanceEdgeLifecycle()
    {
        var fixture = new GraphFixture();
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var edge = fixture.AddInheritance(child, parent);

        Assert.Contains(edge, ((EasyVertex)child).InheritsOutEdges);
        Assert.Contains(edge, ((EasyVertex)parent).InheritsInEdges);

        child.DeleteEdge(edge);

        Assert.DoesNotContain(edge, ((EasyVertex)child).InheritsOutEdges);
        Assert.DoesNotContain(edge, ((EasyVertex)parent).InheritsInEdges);
    }

    [Fact]
    public void TargetValueChangeInvalidatesValueIndexesOfInheritingSourcesOnly()
    {
        var fixture = new GraphFixture();
        var relationMeta = fixture.CreateVertex("Relation");
        var parent = fixture.CreateVertex("Parent");
        var child = fixture.CreateVertex("Child");
        var target = fixture.CreateVertex("Before");
        var parentEdge = parent.AddEdge(relationMeta, target);
        fixture.AddInheritance(child, parent);
        Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, null, "Before"));
        Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, "Relation", "Before"));
        Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, "Relation", null));
        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        try
        {
            target.Value = "After";

            Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, "Relation", null));
            Assert.Empty(GraphUtil.GetQueryOut(child, null, "Before"));
            Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, null, "After"));
            Assert.Empty(GraphUtil.GetQueryOut(child, "Relation", "Before"));
            Assert.Equal(new[] { parentEdge }, GraphUtil.GetQueryOut(child, "Relation", "After"));

            var counters = GraphPerformanceCounters.GetSnapshot();
            Assert.Equal(0, counters.LogicalOutEdgesRebuilds);
            Assert.Equal(0, counters.QueryMetaIndexRebuilds);
            Assert.Equal(1, counters.ValueIndexRebuilds);
            Assert.Equal(1, counters.QueryMetaAndValueIndexRebuilds);
        }
        finally
        {
            GraphPerformanceCounters.Enabled = false;
            GraphPerformanceCounters.Reset();
        }
    }

    [Fact]
    public void AddingSelfInheritanceIsRejectedBeforePhysicalMutation()
    {
        var fixture = new GraphFixture();
        var vertex = fixture.CreateVertex("Vertex");

        Assert.Throws<InvalidOperationException>(
            () => fixture.AddInheritance(vertex, vertex));

        Assert.Empty(vertex.OutEdgesRaw);
        Assert.Empty(vertex.InEdgesRaw);
        Assert.Empty(((EasyVertex)vertex).InheritsOutEdges);
        Assert.Empty(((EasyVertex)vertex).InheritsInEdges);
    }

    [Fact]
    public void AddingTwoVertexInheritanceCycleIsRejectedBeforePhysicalMutation()
    {
        var fixture = new GraphFixture();
        var first = fixture.CreateVertex("First");
        var second = fixture.CreateVertex("Second");
        var existingEdge = fixture.AddInheritance(first, second);

        Assert.Throws<InvalidOperationException>(
            () => fixture.AddInheritance(second, first));

        Assert.Equal(new[] { existingEdge }, first.OutEdgesRaw);
        Assert.Empty(second.OutEdgesRaw);
        Assert.Equal(new[] { existingEdge }, second.InEdgesRaw);
    }

    [Fact]
    public void AddingThreeVertexInheritanceCycleIsRejectedBeforePhysicalMutation()
    {
        var fixture = new GraphFixture();
        var first = fixture.CreateVertex("First");
        var second = fixture.CreateVertex("Second");
        var third = fixture.CreateVertex("Third");
        var firstEdge = fixture.AddInheritance(first, second);
        var secondEdge = fixture.AddInheritance(second, third);

        Assert.Throws<InvalidOperationException>(
            () => fixture.AddInheritance(third, first));

        Assert.Equal(new[] { firstEdge }, first.OutEdgesRaw);
        Assert.Equal(new[] { secondEdge }, second.OutEdgesRaw);
        Assert.Empty(third.OutEdgesRaw);
    }

    [Fact]
    public void LegacyInheritanceCycleDoesNotDuplicateStartingVertexEdges()
    {
        var fixture = new GraphFixture();
        var relationMeta = fixture.CreateVertex("Relation");
        var first = fixture.CreateVertex("First");
        var second = fixture.CreateVertex("Second");
        var third = fixture.CreateVertex("Third");
        var relationEdge = first.AddEdge(
            relationMeta,
            fixture.CreateVertex("Target"));
        AttachInheritanceWithoutValidation(first, second, fixture.InheritsMeta);
        AttachInheritanceWithoutValidation(second, third, fixture.InheritsMeta);
        AttachInheritanceWithoutValidation(third, first, fixture.InheritsMeta);

        Assert.Equal(
            new[] { relationEdge },
            GraphUtil.GetQueryOut(first, "Relation", null));
    }

    [Fact]
    public void LegacyInheritanceCycleTerminatesRecursiveUtilities()
    {
        var fixture = new GraphFixture();
        var first = fixture.CreateVertex("First");
        var second = fixture.CreateVertex("Second");
        var third = fixture.CreateVertex("Third");
        AttachInheritanceWithoutValidation(first, second, fixture.InheritsMeta);
        AttachInheritanceWithoutValidation(second, third, fixture.InheritsMeta);
        AttachInheritanceWithoutValidation(third, first, fixture.InheritsMeta);
        var instance = fixture.CreateVertex("Instance");
        var isMeta = fixture.CreateVertex("$Is");
        instance.AddEdge(isMeta, first);

        Assert.False(VertexOperations.InheritanceCompare(first, "Missing"));
        Assert.False(VertexOperations.IsInherited(first, "Missing"));
        Assert.Same(first, GraphUtil.GetMostInheritedMeta(instance, third));
    }

    [Fact]
    public void IsInheritedMatchesDirectParentValue()
    {
        var fixture = new GraphFixture();
        var atomType = fixture.CreateVertex("AtomType");
        var childType = fixture.CreateVertex("ChildType");
        fixture.AddInheritance(childType, atomType);

        Assert.True(VertexOperations.IsInherited(childType, "AtomType"));
        Assert.False(VertexOperations.IsInherited(atomType, "AtomType"));
    }

    [Fact]
    public void IsInheritedMatchesTransitiveParentValue()
    {
        var fixture = new GraphFixture();
        var atomType = fixture.CreateVertex("AtomType");
        var middleType = fixture.CreateVertex("MiddleType");
        var childType = fixture.CreateVertex("ChildType");
        fixture.AddInheritance(middleType, atomType);
        fixture.AddInheritance(childType, middleType);

        Assert.True(VertexOperations.IsInherited(childType, "AtomType"));
        Assert.True(VertexOperations.IsInherited(childType, "MiddleType"));
    }

    [Fact]
    public void IsInheritedDoesNotMatchUnrelatedOutgoingMetaValue()
    {
        var fixture = new GraphFixture();
        var atomTypeMeta = fixture.CreateVertex("AtomType");
        var parentType = fixture.CreateVertex("ParentType");
        var childType = fixture.CreateVertex("ChildType");
        parentType.AddEdge(
            atomTypeMeta,
            fixture.CreateVertex("UnrelatedTarget"));
        fixture.AddInheritance(childType, parentType);

        Assert.False(VertexOperations.IsInherited(childType, "AtomType"));
    }

    [Fact]
    public void DiamondInheritanceRemainsValidWithoutDuplicateBaseEdges()
    {
        var fixture = new GraphFixture();
        var relationMeta = fixture.CreateVertex("Relation");
        var baseVertex = fixture.CreateVertex("Base");
        var left = fixture.CreateVertex("Left");
        var right = fixture.CreateVertex("Right");
        var child = fixture.CreateVertex("Child");
        var relationEdge = baseVertex.AddEdge(
            relationMeta,
            fixture.CreateVertex("Target"));
        fixture.AddInheritance(left, baseVertex);
        fixture.AddInheritance(right, baseVertex);
        fixture.AddInheritance(child, left);
        fixture.AddInheritance(child, right);

        Assert.Equal(
            new[] { relationEdge },
            GraphUtil.GetQueryOut(child, "Relation", null));
    }

    private static void AttachInheritanceWithoutValidation(
        IVertex child,
        IVertex parent,
        IVertex inheritsMeta)
    {
        var edge = new EasyEdge(child, inheritsMeta, parent);
        child.OutEdgesRaw.Add(edge);
        child.AttachEdge(edge);
        parent.AttachInEdge(edge);
    }
}
