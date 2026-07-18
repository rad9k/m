using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class QueryContractTests
{
    [Fact]
    public void ReadOnlyQueryOutResultPreservesZeroOneManyAndFullScan()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var firstEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex("First"));
        var secondEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex("Second"));

        AssertQueryResult(
            Array.Empty<IEdge>(),
            GraphUtil.GetQueryOutResult(
                source,
                "Missing",
                null));
        AssertQueryResult(
            new[] { firstEdge },
            GraphUtil.GetQueryOutResult(
                source,
                "Meta",
                "First"));
        AssertQueryResult(
            new[] { firstEdge, secondEdge },
            GraphUtil.GetQueryOutResult(
                source,
                "Meta",
                null));
        AssertQueryResult(
            new[] { firstEdge, secondEdge },
            GraphUtil.GetQueryOutResult(
                source,
                null,
                null));
    }

    [Fact]
    public void ReadOnlyQueryInResultPreservesZeroOneManyAndFullScan()
    {
        var fixture = new GraphFixture();
        var target = fixture.CreateVertex("Target");
        var meta = fixture.CreateVertex("Meta");
        var firstEdge =
            fixture.CreateVertex("First")
                .AddEdge(meta, target);
        var secondEdge =
            fixture.CreateVertex("Second")
                .AddEdge(meta, target);

        AssertQueryResult(
            Array.Empty<IEdge>(),
            GraphUtil.GetQueryInResult(
                target,
                "Missing",
                null));
        AssertQueryResult(
            new[] { firstEdge },
            GraphUtil.GetQueryInResult(
                target,
                "Meta",
                "First"));
        AssertQueryResult(
            new[] { firstEdge, secondEdge },
            GraphUtil.GetQueryInResult(
                target,
                "Meta",
                null));
        AssertQueryResult(
            new[] { firstEdge, secondEdge },
            GraphUtil.GetQueryInResult(
                target,
                null,
                null));
    }

    [Fact]
    public void ReadOnlyQueryResultDoesNotExposeMutableListContract()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        source.AddEdge(
            meta,
            fixture.CreateVertex("First"));
        source.AddEdge(
            meta,
            fixture.CreateVertex("Second"));

        object result =
            GraphUtil.GetQueryOutResult(
                source,
                "Meta",
                null);

        Assert.IsAssignableFrom<
            IReadOnlyList<IEdge>>(result);
        Assert.False(result is IList<IEdge>);
    }

    [Fact]
    public void QueryOutReturnsZeroOneAndManyWithoutChangingEdgeOrder()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var firstTarget = fixture.CreateVertex("First");
        var secondTarget = fixture.CreateVertex("Second");
        var firstEdge = source.AddEdge(meta, firstTarget);

        Assert.Empty(GraphUtil.GetQueryOut(source, "Missing", null));
        Assert.Equal(new[] { firstEdge }, GraphUtil.GetQueryOut(source, "Meta", null));

        var secondEdge = source.AddEdge(meta, secondTarget);

        Assert.Equal(new[] { firstEdge, secondEdge }, GraphUtil.GetQueryOut(source, "Meta", null));
        Assert.Equal(new[] { firstEdge }, GraphUtil.GetQueryOut(source, null, "First"));
        Assert.Equal(new[] { secondEdge }, GraphUtil.GetQueryOut(source, "Meta", "Second"));
    }

    [Fact]
    public void QueryOutValueIndexReflectsTargetValueChangeAfterWarmup()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Before");
        var edge = source.AddEdge(meta, target);

        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "Before"));

        target.Value = "After";

        Assert.Empty(GraphUtil.GetQueryOut(source, "Meta", "Before"));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "After"));
    }

    [Fact]
    public void QueryInValueIndexReflectsSourceValueChangeAfterWarmup()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Before");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var edge = source.AddEdge(meta, target);

        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, "Meta", "Before"));

        source.Value = "After";

        Assert.Empty(GraphUtil.GetQueryIn(target, "Meta", "Before"));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, "Meta", "After"));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, "Meta", null));
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryIn(target, null, "After"));
    }

    [Fact]
    public void QueryPreservesParallelEdgeOrderAndIdentity()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var firstEdge = source.AddEdge(meta, target);
        var secondEdge = source.AddEdge(meta, target);

        var results = GraphUtil.GetQueryOut(source, "Meta", "Target");

        Assert.Equal(2, results.Count);
        Assert.Same(firstEdge, results[0]);
        Assert.Same(secondEdge, results[1]);
    }

    [Fact]
    public void QueryGroupsDistinctMetaVerticesWithEqualValues()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var firstMeta = fixture.CreateVertex("SameMetaValue");
        var secondMeta = fixture.CreateVertex("SameMetaValue");
        var firstEdge = source.AddEdge(firstMeta, fixture.CreateVertex("First"));
        var secondEdge = source.AddEdge(secondMeta, fixture.CreateVertex("Second"));

        var results = GraphUtil.GetQueryOut(source, "SameMetaValue", null);

        Assert.Equal(2, results.Count);
        Assert.Same(firstEdge, results[0]);
        Assert.Same(secondEdge, results[1]);
        Assert.NotSame(results[0].Meta, results[1].Meta);
    }

    [Fact]
    public void ValueKeysPreserveCurrentToStringEquivalence()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var integerEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(1));
        var stringEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex("1"));
        var customEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(
                    new StableQueryValue("1")));

        var expected =
            new[]
            {
                integerEdge,
                stringEdge,
                customEdge
            };

        Assert.Equal(
            expected,
            GraphUtil.GetQueryOut(
                source,
                null,
                1));
        Assert.Equal(
            expected,
            GraphUtil.GetQueryOut(
                source,
                null,
                "1"));
        Assert.Equal(
            expected,
            GraphUtil.GetQueryOut(
                source,
                "Meta",
                new StableQueryValue("1")));
    }

    [Fact]
    public void LongValueKeysPreserveOrderIdentityAndExactMatching()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var sharedPrefix = new string('x', 1024);
        var matchingValue = sharedPrefix + "-match";
        var firstEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(
                    matchingValue));
        source.AddEdge(
            meta,
            fixture.CreateVertex(
                sharedPrefix + "-other"));
        var secondEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(
                    new string(
                        matchingValue.ToCharArray())));

        var result =
            GraphUtil.GetQueryOut(
                source,
                "Meta",
                matchingValue);

        Assert.Equal(2, result.Count);
        Assert.Same(firstEdge, result[0]);
        Assert.Same(secondEdge, result[1]);
    }

    [Fact]
    public void DecimalAndEmptyValueKeysPreserveToStringSemantics()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        const decimal decimalValue = 123.5m;
        var decimalText = decimalValue.ToString();
        var decimalEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(
                    decimalValue));
        var decimalTextEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(
                    decimalText));
        var firstEmptyEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(""));
        var secondEmptyEdge =
            source.AddEdge(
                meta,
                fixture.CreateVertex(""));

        Assert.Equal(
            new[]
            {
                decimalEdge,
                decimalTextEdge
            },
            GraphUtil.GetQueryOut(
                source,
                null,
                decimalValue));
        Assert.Equal(
            new[]
            {
                firstEmptyEdge,
                secondEmptyEdge
            },
            GraphUtil.GetQueryOut(
                source,
                "Meta",
                ""));
    }

    private static void AssertQueryResult(
        IReadOnlyList<IEdge> expected,
        EdgeQueryResult actual)
    {
        Assert.Equal(
            expected.Count,
            actual.Count);
        Assert.Equal(
            expected.Count == 0,
            actual.IsEmpty);
        Assert.Same(
            expected.Count == 0
                ? null
                : expected[0],
            actual.FirstOrDefault);

        var position = 0;

        foreach (var edge in actual)
        {
            Assert.Same(
                expected[position],
                edge);
            position++;
        }

        Assert.Equal(
            expected.Count,
            position);

        for (var index = 0;
            index < expected.Count;
            index++)
        {
            Assert.Same(
                expected[index],
                actual[index]);
        }

        Assert.Throws<
            ArgumentOutOfRangeException>(
                () => _ = actual[expected.Count]);
    }

    private sealed class StableQueryValue
    {
        private readonly string value;

        public StableQueryValue(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
