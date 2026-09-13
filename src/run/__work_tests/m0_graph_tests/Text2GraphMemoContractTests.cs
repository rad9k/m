using m0.ZeroCode;

namespace m0_graph_tests;

public sealed class Text2GraphMemoContractTests
{
    [Fact]
    public void CollidingHashesRemainDistinctMemoEntries()
    {
        var first =
            new Text2GraphProcessing
                .TryIsKeywordMemoKey(
                    0,
                    397,
                    0,
                    100,
                    100,
                    false,
                    false,
                    true,
                    false,
                    null,
                    "",
                    false);
        var second =
            new Text2GraphProcessing
                .TryIsKeywordMemoKey(
                    1,
                    0,
                    0,
                    100,
                    100,
                    false,
                    false,
                    true,
                    false,
                    null,
                    "",
                    false);

        Assert.Equal(
            first.GetHashCode(),
            second.GetHashCode());
        Assert.NotEqual(first, second);

        var dictionary =
            new Dictionary<
                Text2GraphProcessing
                    .TryIsKeywordMemoKey,
                string>
            {
                [first] = "First",
                [second] = "Second"
            };

        Assert.Equal(2, dictionary.Count);
        Assert.Equal("First", dictionary[first]);
        Assert.Equal("Second", dictionary[second]);
    }
}
