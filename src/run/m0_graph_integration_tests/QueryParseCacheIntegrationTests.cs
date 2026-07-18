using m0;
using m0.Graph;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class QueryParseCacheIntegrationTests
{
    public QueryParseCacheIntegrationTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void ParsedQueriesAreReusedAndSeparatedByMetaMode()
    {
        var previousRegularCapacity =
            EasyVertex.QueryParseCacheCapacity;
        var previousMetaCapacity =
            EasyVertex.MetaQueryParseCacheCapacity;

        try
        {
            EasyVertex.ResetQueryParseCaches();
            EasyVertex.QueryParseCacheCapacity = 2;
            EasyVertex.MetaQueryParseCacheCapacity = 2;

            _ = MinusZero.Instance.Root.Get(
                false,
                @"System\Meta");
            _ = MinusZero.Instance.Root.Get(
                false,
                @"System\Meta");
            _ = MinusZero.Instance.Root.Get(
                true,
                @"System\Meta");

            Assert.Equal(
                1,
                EasyVertex.QueryParseCacheEntryCount);
            Assert.Equal(
                1,
                EasyVertex.MetaQueryParseCacheEntryCount);
        }
        finally
        {
            EasyVertex.ResetQueryParseCaches();
            EasyVertex.QueryParseCacheCapacity =
                previousRegularCapacity;
            EasyVertex.MetaQueryParseCacheCapacity =
                previousMetaCapacity;
        }
    }

    [Fact]
    public void ResetClearsBothModeCaches()
    {
        var previousRegularCapacity =
            EasyVertex.QueryParseCacheCapacity;
        var previousMetaCapacity =
            EasyVertex.MetaQueryParseCacheCapacity;

        try
        {
            EasyVertex.ResetQueryParseCaches();
            EasyVertex.QueryParseCacheCapacity = 2;
            EasyVertex.MetaQueryParseCacheCapacity = 2;

            _ = MinusZero.Instance.Root.Get(
                false,
                @"System");
            _ = MinusZero.Instance.Root.Get(
                true,
                @"System");

            Assert.Equal(
                1,
                EasyVertex.QueryParseCacheEntryCount);
            Assert.Equal(
                1,
                EasyVertex.MetaQueryParseCacheEntryCount);

            EasyVertex.ResetQueryParseCaches();

            Assert.Equal(
                0,
                EasyVertex.QueryParseCacheEntryCount);
            Assert.Equal(
                0,
                EasyVertex.MetaQueryParseCacheEntryCount);
        }
        finally
        {
            EasyVertex.ResetQueryParseCaches();
            EasyVertex.QueryParseCacheCapacity =
                previousRegularCapacity;
            EasyVertex.MetaQueryParseCacheCapacity =
                previousMetaCapacity;
        }
    }

    [Fact]
    public void ParseErrorsAreNotCached()
    {
        var previousRegularCapacity =
            EasyVertex.QueryParseCacheCapacity;

        try
        {
            EasyVertex.ResetQueryParseCaches();
            EasyVertex.QueryParseCacheCapacity = 2;

            _ = MinusZero.Instance.Root.Get(false, "<");
            _ = MinusZero.Instance.Root.Get(false, "<");

            Assert.Equal(
                0,
                EasyVertex.QueryParseCacheEntryCount);
        }
        finally
        {
            EasyVertex.ResetQueryParseCaches();
            EasyVertex.QueryParseCacheCapacity =
                previousRegularCapacity;
        }
    }
}
