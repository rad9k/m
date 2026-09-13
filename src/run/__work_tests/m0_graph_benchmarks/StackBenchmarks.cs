using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class StackBenchmarks
{
    private const int CopiedEdgesPerInvocation = 100;

    private GraphFixture fixture = null!;
    private IReadOnlyList<IEdge> originalEdges = null!;
    private NoInEdgeInOutVertexVertex localStack = null!;
    private NoInEdgeInOutVertexVertex childStack = null!;

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Local");
        originalEdges = fixture.AddEdges(source, meta, 1000);

        localStack = CreateStackWithCopiedEdges(originalEdges);

        var parentStack = CreateStackWithCopiedEdges(originalEdges);
        childStack = new NoInEdgeInOutVertexVertex(fixture.Store);
        var stackFrameMeta = fixture.CreateVertex("$StackFrameInherits");
        childStack.AddEdge(stackFrameMeta, parentStack);

        QueryCount(localStack, "Local", null);
        QueryCount(childStack, "Local", null);
    }

    [Benchmark]
    public int QueryLocalStack()
    {
        return QueryCount(localStack, "Local", null);
    }

    [Benchmark]
    public int QueryParentStackFrame()
    {
        return QueryCount(childStack, "Local", null);
    }

    [Benchmark]
    public int CreateCopyAndUnregisterStack()
    {
        var stack = new NoInEdgeInOutVertexVertex(fixture.Store);

        for (var index = 0; index < CopiedEdgesPerInvocation; index++)
            stack.AddEdgeForNoInEdgeInOutVertexVertex(originalEdges[index]);

        var edgeCount = stack.OutEdgesRaw.Count;
        fixture.Store.RemoveVertexIdentifier(stack);
        return edgeCount;
    }

    [Benchmark]
    public int CreateEphemeralCopyStack()
    {
        var stack = new NoInEdgeInOutVertexVertex(
            fixture.Store,
            VertexIdentifierRegistrationMode.Ephemeral);

        for (var index = 0; index < CopiedEdgesPerInvocation; index++)
            stack.AddEdgeForNoInEdgeInOutVertexVertex(originalEdges[index]);

        return stack.OutEdgesRaw.Count;
    }

    private NoInEdgeInOutVertexVertex CreateStackWithCopiedEdges(IEnumerable<IEdge> edges)
    {
        var stack = new NoInEdgeInOutVertexVertex(fixture.Store);

        foreach (var edge in edges)
            stack.AddEdgeForNoInEdgeInOutVertexVertex(edge);

        return stack;
    }

    private static int QueryCount(IVertex vertex, object meta, object? value)
    {
        vertex.QueryOutEdges(meta, value!, out var result, out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
[IterationTime(100)]
public class StackOriginalEdgeBatchBenchmarks
{
    private GraphFixture fixture = null!;
    private IReadOnlyList<IEdge> originalEdges = null!;

    [Params(1, 10, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Local");
        originalEdges =
            fixture.AddEdges(
                source,
                meta,
                EdgeCount);
    }

    [Benchmark(Baseline = true)]
    public int AddOriginalEdgesOneByOne()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);

        for (var index = 0;
            index < originalEdges.Count;
            index++)
        {
            stack
                .AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                    originalEdges[index]);
        }

        return stack.OutEdgesRaw.Count;
    }

    [Benchmark]
    public int AddOriginalEdgesBatch()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);

        stack.AddRangeOriginalEdges(originalEdges);

        return stack.OutEdgesRaw.Count;
    }

    [Benchmark]
    public int AddOriginalEdgesOneByOneAndQuery()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);

        for (var index = 0;
            index < originalEdges.Count;
            index++)
        {
            stack
                .AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                    originalEdges[index]);
        }

        return QueryCount(
            stack,
            "Local",
            null);
    }

    [Benchmark]
    public int AddOriginalEdgesBatchAndQuery()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);

        stack.AddRangeOriginalEdges(originalEdges);

        return QueryCount(
            stack,
            "Local",
            null);
    }

    private static int QueryCount(
        IVertex vertex,
        object meta,
        object? value)
    {
        vertex.QueryOutEdges(
            meta,
            value!,
            out var result,
            out var results);
        return result != null
            ? 1
            : results?.Count ?? 0;
    }
}

[MemoryDiagnoser]
public class StackInfrastructureBenchmarks
{
    private GraphFixture fixture = null!;
    private IVertex stackFrameMeta = null!;
    private NoInEdgeInOutVertexVertex parentStack = null!;

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        parentStack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);
    }

    [Benchmark]
    public int CreateEmptyEphemeralStack()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);
        return stack.OutEdgesRaw.Count;
    }

    [Benchmark]
    public int CreateEmptyAndReadIncomingLists()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);
        return stack.InEdgesRaw.Count +
            stack.MetaInEdgesRaw.Count;
    }

    [Benchmark]
    public int CreateStackWithParentFrame()
    {
        var stack =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);
        stack.AddEdge(
            stackFrameMeta,
            parentStack);
        return stack.OutEdgesRaw.Count;
    }
}

[MemoryDiagnoser]
public class StackParentFrameCacheBenchmarks
{
    private GraphFixture fixture = null!;
    private NoInEdgeInOutVertexVertex deepestStack = null!;

    [Params(1, 10, 100, 1000)]
    public int FrameDepth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        var stackFrameMeta =
            fixture.CreateVertex("$StackFrameInherits");
        var localMeta = fixture.CreateVertex("Local");
        var source = fixture.CreateVertex("Source");
        var target = fixture.CreateVertex("Target");
        var localEdge =
            source.AddEdge(localMeta, target);
        var current =
            new NoInEdgeInOutVertexVertex(
                fixture.Store,
                VertexIdentifierRegistrationMode.Ephemeral);
        current.AddRangeOriginalEdges(
            new[] { localEdge });

        for (var depth = 0;
            depth < FrameDepth;
            depth++)
        {
            var child =
                new NoInEdgeInOutVertexVertex(
                    fixture.Store,
                    VertexIdentifierRegistrationMode.Ephemeral);
            child.AddEdge(
                stackFrameMeta,
                current);
            current = child;
        }

        deepestStack = current;
        _ = QueryCount(
            deepestStack,
            "Local");
        _ = QueryCount(
            deepestStack,
            "Missing");
    }

    [Benchmark]
    public int QueryDeepParentFrame()
    {
        return QueryCount(
            deepestStack,
            "Local");
    }

    [Benchmark]
    public int QueryMissingAcrossAllFrames()
    {
        return QueryCount(
            deepestStack,
            "Missing");
    }

    private static int QueryCount(
        IVertex vertex,
        object meta)
    {
        vertex.QueryOutEdges(
            meta,
            null!,
            out var result,
            out var results);
        return result != null
            ? 1
            : results?.Count ?? 0;
    }
}
