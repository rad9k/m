using BenchmarkDotNet.Attributes;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
public class QueryParseCacheBenchmarks
{
    private BoundedQueryParseCache cache = null!;
    private Func<QueryParseCacheValue> currentFactory = null!;
    private string[] missQueries = null!;
    private EasyVertex[] missVertices = null!;
    private int currentMissIndex;

    [Params(16, 128, 512)]
    public int Capacity { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        cache = new BoundedQueryParseCache(Capacity);
        missQueries = new string[Capacity * 2];
        missVertices = new EasyVertex[Capacity * 2];

        var hotVertex = CreatePinnedVertex(
            fixture,
            "HotQuery");
        var hotLease = cache.GetOrCreate(
            "hot-query",
            () => new QueryParseCacheValue(
                hotVertex,
                true));
        hotLease.Dispose();

        for (var index = 0;
             index < missQueries.Length;
             index++)
        {
            missQueries[index] = $"miss-{index}";
            missVertices[index] = CreatePinnedVertex(
                fixture,
                $"Parsed-{index}");
        }

        currentFactory = () =>
            new QueryParseCacheValue(
                missVertices[currentMissIndex],
                true);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        cache.Clear();
    }

    [Benchmark]
    public bool WarmHitAndLruPromotion()
    {
        var lease = cache.GetOrCreate(
            "hot-query",
            currentFactory);
        var hit = lease.Hit;
        lease.Dispose();
        return hit;
    }

    [Benchmark]
    public bool MissAndEvictLeastRecentlyUsed()
    {
        currentMissIndex++;

        if (currentMissIndex == missQueries.Length)
            currentMissIndex = 0;

        var lease = cache.GetOrCreate(
            missQueries[currentMissIndex],
            currentFactory);
        var hit = lease.Hit;
        lease.Dispose();
        return hit;
    }

    private static EasyVertex CreatePinnedVertex(
        GraphFixture fixture,
        string value)
    {
        var vertex = fixture.CreateVertex(value);
        vertex.IsRoot = true;
        return vertex;
    }
}
