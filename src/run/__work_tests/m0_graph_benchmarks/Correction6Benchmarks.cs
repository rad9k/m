using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class IsInheritedBenchmarks
{
    private IVertex childType = null!;

    [Params(1, 10, 100)]
    public int InheritanceDepth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var hierarchy = fixture.CreateInheritanceChain(
            InheritanceDepth);
        hierarchy[0].Value = "AtomType";
        childType = hierarchy[^1];
        VertexOperations.IsInherited(childType, "AtomType");
    }

    [Benchmark]
    public bool FindRootType()
    {
        return VertexOperations.IsInherited(
            childType,
            "AtomType");
    }
}

[MemoryDiagnoser]
public class TargetCardinalityBenchmarks
{
    private IVertex source = null!;
    private IVertex relationMeta = null!;
    private IVertex target = null!;

    [Params(0, 100, 1000)]
    public int ExistingIncomingEdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        relationMeta = fixture.CreateVertex("Relation");
        relationMeta.AddVertex(
            fixture.CreateVertex("$MaxTargetCardinality"),
            int.MaxValue);
        target = fixture.CreateVertex("Target");
        source = fixture.CreateVertex("CandidateSource");

        for (var index = 0;
             index < ExistingIncomingEdgeCount;
             index++)
            fixture.CreateVertex($"Source-{index}")
                .AddEdge(relationMeta, target);

        VertexOperations.TestIfNewEdgeValid(
            source,
            relationMeta,
            target);
    }

    [Benchmark]
    public IVertex ValidateTargetCardinality()
    {
        return VertexOperations.TestIfNewEdgeValid(
            source,
            relationMeta,
            target);
    }
}

[MemoryDiagnoser]
public class SharedSubgraphDeepCopyBenchmarks
{
    private GraphFixture fixture = null!;
    private IVertex original = null!;

    [Params(1, 10, 100)]
    public int BranchCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        var branchMeta = fixture.CreateVertex("Branch");
        var sharedMeta = fixture.CreateVertex("Shared");
        var shared = fixture.CreateVertex("SharedValue");
        original = fixture.CreateVertex("Original");

        for (var index = 0; index < BranchCount; index++)
        {
            var branch = fixture.CreateVertex($"Branch-{index}");
            original.AddEdge(branchMeta, branch);
            branch.AddEdge(sharedMeta, shared);
        }
    }

    [Benchmark(OperationsPerInvoke = 16)]
    public int CopySharedSubgraphLifecycle()
    {
        var copiedEdgeCount = 0;

        for (var copyIndex = 0; copyIndex < 16; copyIndex++)
        {
            var copy = fixture.CreateVertex("CopyPlaceholder");
            GraphUtil.DeepCopyByVertex(original, copy);
            copiedEdgeCount += copy.OutEdgesRaw.Count;
            copy.Dispose();
        }

        return copiedEdgeCount;
    }
}
