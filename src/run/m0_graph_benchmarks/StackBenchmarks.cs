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
