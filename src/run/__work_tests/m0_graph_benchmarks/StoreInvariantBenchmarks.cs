using BenchmarkDotNet.Attributes;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store;
using m0.Store.FileSystem;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[InProcess]
[WarmupCount(2)]
[IterationCount(10)]
[InvocationCount(262144)]
public class StoreIdentifierBenchmarks
{
    private MemoryStore store = null!;
    private IVertex existingVertex = null!;

    [GlobalSetup]
    public void Setup()
    {
        var universe = new GraphTestStoreUniverse();
        store = new MemoryStore(
            "identifier-benchmark",
            universe,
            new[] { AccessLevelEnum.NoRestrictions });
        existingVertex = new EasyVertex(
            store,
            "existing");
    }

    [Benchmark]
    public object RegisterAndRemoveUniqueVertex()
    {
        var vertex = new EasyVertex(store);
        store.RemoveVertexIdentifier(vertex);
        return vertex.Identifier;
    }

    [Benchmark(OperationsPerInvoke = 16)]
    public void ReRegisterSameVertex()
    {
        for (var index = 0; index < 16; index++)
            store.StoreVertexIdentifier(existingVertex);
    }
}

[MemoryDiagnoser]
[InProcess]
[WarmupCount(2)]
[IterationCount(10)]
[InvocationCount(262144)]
public class FileContentValueBenchmarks
{
    private IVertex source = null!;
    private FileContentVertex content = null!;
    private int nextValue;

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Content");
        content = new FileContentVertex(fixture.Store)
        {
            Value = nextValue
        };
        source.AddEdge(meta, content);
        _ = GraphUtil.GetQueryOut(
            source,
            "Content",
            nextValue);
    }

    [Benchmark]
    public int ChangeValueAndQueryWarmSource()
    {
        var value = ++nextValue;
        content.Value = value;
        return GraphUtil.GetQueryOut(
            source,
            "Content",
            value).Count;
    }
}

[MemoryDiagnoser]
[InProcess]
[WarmupCount(2)]
[IterationCount(10)]
[InvocationCount(256)]
public class FileSystemRenameBenchmarks
{
    private FileSystemStore store = null!;
    private FileVertex fileVertex = null!;
    private ITransaction ambientTransaction = null!;
    private string directoryName = null!;
    private string currentFileName = "before.txt";

    [GlobalSetup]
    public void Setup()
    {
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();
        ambientTransaction =
            MinusZero.Instance.GetTopTransaction();

        directoryName = Path.Combine(
            Path.GetTempPath(),
            $"m0-fs-benchmark-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryName);
        File.WriteAllText(
            Path.Combine(directoryName, currentFileName),
            "content");

        store = new FileSystemStore(
            directoryName,
            MinusZero.Instance,
            new[] { AccessLevelEnum.NoRestrictions });
        _ = store.Root.OutEdges;
        fileVertex = (FileVertex)GraphUtil.GetQueryOutFirst(
            store.Root,
            "File",
            currentFileName);
    }

    [Benchmark]
    public object RenameFileAndCommit()
    {
        string nextFileName =
            currentFileName == "before.txt"
                ? "after.txt"
                : "before.txt";
        ExecutionFlowHelper.StartTransaction();

        try
        {
            fileVertex.Value = nextFileName;
            ExecutionFlowHelper.CommitTransaction();
            currentFileName = nextFileName;
            return fileVertex.Identifier;
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

    [GlobalCleanup]
    public void Cleanup()
    {
        store.RemoveVertexIdentifier(fileVertex);
        store.RemoveVertexIdentifier(store.Root);
        MinusZero.Instance.RemoveStore(store);
        Directory.Delete(directoryName, true);
    }
}
