using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
[IterationTime(100)]
public class FileContentIndexBenchmarks
{
    private GraphFixture fixture = null!;
    private EasyVertex source = null!;
    private FileContentVertex content = null!;
    private string contentValue = null!;
    private string fileName = null!;

    [Params(1024, 1048576, 10485760)]
    public int FileSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        var contentMeta =
            fixture.CreateVertex("Content");
        var fileNameMeta =
            fixture.CreateVertex("Filename");
        contentValue =
            new string('x', FileSize);
        fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-content-index-{Guid.NewGuid():N}.txt");
        File.WriteAllText(
            fileName,
            contentValue);
        content =
            new FileContentVertex(
                fileName,
                fixture.Store);
        source.AddEdge(
            contentMeta,
            content);
        source.AddEdge(
            fileNameMeta,
            fixture.CreateVertex("payload.txt"));
    }

    [Benchmark]
    public int RebuildMetaOnlyIndex()
    {
        source.OutEdgesDictionariesNeedsRebuild =
            true;

        return QueryCount(
            "Content",
            null);
    }

    [Benchmark]
    public int RebuildUnrelatedMetaAndValueIndex()
    {
        source.OutEdgesDictionariesNeedsRebuild =
            true;

        return QueryCount(
            "Filename",
            "payload.txt");
    }

    [Benchmark]
    public int RebuildValueOnlyIndex()
    {
        source.OutEdgesDictionariesNeedsRebuild =
            true;

        return QueryCount(
            null,
            "payload.txt");
    }

    [Benchmark]
    public int RebuildContentEqualityIndex()
    {
        source.OutEdgesDictionariesNeedsRebuild =
            true;

        return QueryCount(
            "Content",
            contentValue);
    }

    [Benchmark]
    public int ReadContentValue()
    {
        return ((string)content.Value).Length;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        File.Delete(fileName);
    }

    private int QueryCount(
        object? meta,
        object? value)
    {
        source.QueryOutEdges(
            meta!,
            value!,
            out IEdge single,
            out var multiple);

        return single == null
            ? multiple?.Count ?? 0
            : 1;
    }
}
