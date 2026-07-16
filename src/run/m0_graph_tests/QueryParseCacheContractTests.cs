using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class QueryParseCacheContractTests
{
    [Fact]
    public void RepeatedLookupReusesVertexAndOneCacheReference()
    {
        var fixture = new GraphFixture();
        var queryVertex = CreatePinnedVertex(fixture, "Query");
        var cache = new BoundedQueryParseCache(2);
        var factoryCalls = 0;

        var firstLease = cache.GetOrCreate(
            "same-query",
            () =>
            {
                factoryCalls++;
                return new QueryParseCacheValue(
                    queryVertex,
                    true);
            });

        Assert.False(firstLease.Hit);
        Assert.True(firstLease.RetainedByCache);
        Assert.Same(queryVertex, firstLease.Vertex);
        Assert.Equal(2, queryVertex.ExternalReferenceCount);
        firstLease.Dispose();
        Assert.Equal(1, queryVertex.ExternalReferenceCount);

        var secondLease = cache.GetOrCreate(
            "same-query",
            () => throw new InvalidOperationException(
                "The cached query must not be parsed again."));

        Assert.True(secondLease.Hit);
        Assert.Same(queryVertex, secondLease.Vertex);
        Assert.Equal(2, queryVertex.ExternalReferenceCount);
        secondLease.Dispose();

        Assert.Equal(1, factoryCalls);
        Assert.Equal(
            new QueryParseCacheState(1, 2),
            cache.State);

        cache.Clear();
        Assert.Equal(0, queryVertex.ExternalReferenceCount);
    }

    [Fact]
    public void LeastRecentlyUsedEntryIsEvictedAndReleasedExactlyOnce()
    {
        var fixture = new GraphFixture();
        var first = CreatePinnedVertex(fixture, "First");
        var second = CreatePinnedVertex(fixture, "Second");
        var third = CreatePinnedVertex(fixture, "Third");
        var cache = new BoundedQueryParseCache(2);

        AddAndReleaseLease(cache, "first", first);
        AddAndReleaseLease(cache, "second", second);

        var promotedLease = cache.GetOrCreate(
            "first",
            () => throw new InvalidOperationException());
        promotedLease.Dispose();

        AddAndReleaseLease(cache, "third", third);

        Assert.Equal(1, first.ExternalReferenceCount);
        Assert.Equal(0, second.ExternalReferenceCount);
        Assert.Equal(1, third.ExternalReferenceCount);
        Assert.Equal(
            new QueryParseCacheState(2, 2),
            cache.State);

        cache.Capacity = 1;

        Assert.Equal(0, first.ExternalReferenceCount);
        Assert.Equal(1, third.ExternalReferenceCount);
        Assert.Equal(
            new QueryParseCacheState(1, 1),
            cache.State);

        cache.Clear();
        Assert.Equal(0, third.ExternalReferenceCount);
    }

    [Fact]
    public void NonCacheableParseResultIsReleasedAndRetried()
    {
        var fixture = new GraphFixture();
        var queryVertex = CreatePinnedVertex(
            fixture,
            "InvalidQuery");
        var cache = new BoundedQueryParseCache(2);
        var factoryCalls = 0;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var lease = cache.GetOrCreate(
                "invalid",
                () =>
                {
                    factoryCalls++;
                    return new QueryParseCacheValue(
                        queryVertex,
                        false);
                });

            Assert.False(lease.Hit);
            Assert.False(lease.RetainedByCache);
            Assert.Equal(1, queryVertex.ExternalReferenceCount);
            lease.Dispose();
            Assert.Equal(0, queryVertex.ExternalReferenceCount);
        }

        Assert.Equal(2, factoryCalls);
        Assert.Equal(
            new QueryParseCacheState(0, 2),
            cache.State);
    }

    [Fact]
    public void ParallelLookupCreatesOneEntryAndBalancesLeases()
    {
        var fixture = new GraphFixture();
        var queryVertex = CreatePinnedVertex(
            fixture,
            "ParallelQuery");
        var cache = new BoundedQueryParseCache(4);
        var factoryCalls = 0;

        Parallel.For(
            0,
            64,
            _ =>
            {
                var lease = cache.GetOrCreate(
                    "shared",
                    () =>
                    {
                        Interlocked.Increment(
                            ref factoryCalls);
                        return new QueryParseCacheValue(
                            queryVertex,
                            true);
                    });

                lease.Dispose();
            });

        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, queryVertex.ExternalReferenceCount);
        Assert.Equal(
            new QueryParseCacheState(1, 4),
            cache.State);

        cache.Clear();
        Assert.Equal(0, queryVertex.ExternalReferenceCount);
    }

    [Fact]
    public void ZeroCapacityCreatesOnlyTemporaryLease()
    {
        var fixture = new GraphFixture();
        var queryVertex = CreatePinnedVertex(
            fixture,
            "UncachedQuery");
        var cache = new BoundedQueryParseCache(0);

        var lease = cache.GetOrCreate(
            "query",
            () => new QueryParseCacheValue(
                queryVertex,
                true));

        Assert.False(lease.RetainedByCache);
        Assert.Equal(1, queryVertex.ExternalReferenceCount);
        Assert.Equal(
            new QueryParseCacheState(0, 0),
            cache.State);

        lease.Dispose();
        Assert.Equal(0, queryVertex.ExternalReferenceCount);
    }

    [Fact]
    public void ClearReleasesCacheReferenceButPreservesActiveLease()
    {
        var fixture = new GraphFixture();
        var queryVertex = CreatePinnedVertex(
            fixture,
            "LeasedQuery");
        var cache = new BoundedQueryParseCache(1);
        var lease = cache.GetOrCreate(
            "query",
            () => new QueryParseCacheValue(
                queryVertex,
                true));

        Assert.Equal(2, queryVertex.ExternalReferenceCount);

        cache.Clear();

        Assert.Equal(1, queryVertex.ExternalReferenceCount);
        Assert.Equal(
            new QueryParseCacheState(0, 1),
            cache.State);

        lease.Dispose();
        Assert.Equal(0, queryVertex.ExternalReferenceCount);
    }

    private static void AddAndReleaseLease(
        BoundedQueryParseCache cache,
        string query,
        EasyVertex vertex)
    {
        var lease = cache.GetOrCreate(
            query,
            () => new QueryParseCacheValue(
                vertex,
                true));
        lease.Dispose();
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
