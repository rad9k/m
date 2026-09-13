using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store;
using m0.Store.FileSystem;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
[IterationTime(100)]
public class NoInheritOverlayBenchmarks
{
    private FileSystemStore fileSystemStore = null!;
    private FileVertex fileVertex = null!;
    private EasyVertex easyChild = null!;
    private IVertex noInheritMeta = null!;
    private IVertex blockedMeta = null!;
    private IVertex marker = null!;
    private IEdge markerEdge = null!;
    private string directoryName = null!;

    [Params(1, 1000)]
    public int EdgeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();

        directoryName = Path.Combine(
            Path.GetTempPath(),
            $"m0-no-inherit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryName);
        File.WriteAllText(
            Path.Combine(
                directoryName,
                "payload.txt"),
            "content");
        fileSystemStore =
            new FileSystemStore(
                directoryName,
                MinusZero.Instance,
                new[]
                {
                    AccessLevelEnum.NoRestrictions
                });
        _ = fileSystemStore.Root.OutEdges;
        fileVertex =
            (FileVertex)GraphUtil.GetQueryOutFirst(
                fileSystemStore.Root,
                "File",
                "payload.txt");

        var tempStore =
            MinusZero.Instance.TempStore;
        noInheritMeta =
            CreateTempVertex(
                tempStore,
                "$NoInherit");
        blockedMeta =
            CreateTempVertex(
                tempStore,
                "Blocked");
        marker =
            CreateTempVertex(
                tempStore,
                "Marker");
        markerEdge = blockedMeta.AddEdge(
            noInheritMeta,
            marker);
        var parent =
            CreateTempVertex(
                tempStore,
                "Parent");

        for (var index = 0;
            index < EdgeCount;
            index++)
            parent.AddEdge(
                blockedMeta,
                CreateTempVertex(
                    tempStore,
                    index));

        fileVertex.AddEdge(
            MinusZero.Instance.Inherits,
            parent);
        easyChild =
            (EasyVertex)CreateTempVertex(
                tempStore,
                "EasyChild");
        easyChild.AddEdge(
            MinusZero.Instance.Inherits,
            parent);
    }

    [Benchmark(Baseline = true)]
    public int RebuildEasyVertex()
    {
        easyChild.OutEdgesDictionariesNeedsRebuild =
            true;
        return easyChild.OutEdges.Count;
    }

    [Benchmark]
    public int RebuildFileSystemOverlay()
    {
        fileVertex.OutEdgesDictionariesNeedsRebuild =
            true;
        return fileVertex.OutEdges.Count;
    }

    [Benchmark]
    public int RebuildFileSystemFilenameIndex()
    {
        fileVertex.OutEdgesDictionariesNeedsRebuild =
            true;
        fileVertex.QueryOutEdges(
            "Filename",
            "payload.txt",
            out IEdge single,
            out var multiple);
        return single == null
            ? multiple?.Count ?? 0
            : 1;
    }

    [Benchmark]
    public int ToggleMarkerAndQueryEasyVertex()
    {
        blockedMeta.DeleteEdge(markerEdge);
        int count = easyChild.OutEdges.Count;
        markerEdge = blockedMeta.AddEdge(
            noInheritMeta,
            marker);
        return count + easyChild.OutEdges.Count;
    }

    [Benchmark]
    public int ToggleMarkerAndQueryFileSystemOverlay()
    {
        blockedMeta.DeleteEdge(markerEdge);
        int count = fileVertex.OutEdges.Count;
        markerEdge = blockedMeta.AddEdge(
            noInheritMeta,
            marker);
        return count + fileVertex.OutEdges.Count;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        fileSystemStore.RemoveVertexIdentifier(
            fileVertex);
        fileSystemStore.RemoveVertexIdentifier(
            fileSystemStore.Root);
        MinusZero.Instance.RemoveStore(
            fileSystemStore);
        Directory.Delete(
            directoryName,
            true);
    }

    private static IVertex CreateTempVertex(
        IStore store,
        object value)
    {
        return new EasyVertex(store)
        {
            Value = value
        };
    }
}
