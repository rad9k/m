using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class BaseLinkRoundTripTests
{
    public BaseLinkRoundTripTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void BaseLinkMetaSurvivesVertexAndManyLinesRoundTrip()
    {
        IVertex baseLink = MinusZero.Instance.Root.Get(
            false,
            @"System\Meta\Base\Link");
        IVertex zeroUmlLink = MinusZero.Instance.Root.Get(
            false,
            @"System\Meta\ZeroUML\Link");
        IVertex target = MinusZero.Instance.Root.Get(
            false,
            @"System\Meta\Base\Vertex");

        Assert.NotNull(baseLink);
        Assert.NotNull(zeroUmlLink);
        Assert.NotNull(target);
        Assert.NotSame(baseLink, zeroUmlLink);

        IVertex? bucket = null;
        IVertex? parseParent = null;

        try
        {
            bucket = MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                "BaseLinkRoundTrip-" + Guid.NewGuid().ToString("N"));
            IEdge hostEdge = bucket.AddVertexAndReturnEdge(
                MinusZero.Instance.Empty,
                "Host");
            hostEdge.To.AddEdge(baseLink, target);

            string text = MinusZero.Instance.DefaultFormalTextGenerator.Generate(
                hostEdge,
                CodeRepresentationEnum.VertexAndManyLines);

            Assert.False(string.IsNullOrWhiteSpace(text));

            IVertex parsedHost = ParseSource(
                text,
                CodeRepresentationEnum.VertexAndManyLines,
                out parseParent);

            IEdge? linkEdge = null;

            foreach (IEdge edge in parsedHost.OutEdgesRaw)
            {
                if (edge.Meta?.Value?.ToString() == "Link")
                {
                    linkEdge = edge;
                    break;
                }
            }

            Assert.True(
                linkEdge != null,
                "Generated text did not reload an edge meta named Link."
                + Environment.NewLine
                + text);
            Assert.Same(baseLink, linkEdge.Meta);
            Assert.NotSame(zeroUmlLink, linkEdge.Meta);
        }
        finally
        {
            DeleteFromTempRoot(bucket);
            DeleteFromTempRoot(parseParent);
        }
    }

    private static void DeleteFromTempRoot(IVertex? vertex)
    {
        if (vertex == null)
            return;

        IVertex tempRoot = MinusZero.Instance.TempStore.Root;

        foreach (IEdge edge in tempRoot.OutEdgesRaw.ToList())
        {
            if (ReferenceEquals(edge.To, vertex))
                tempRoot.DeleteEdge(edge);
        }
    }

    private static IVertex ParseSource(
        string source,
        CodeRepresentationEnum codeRepresentation,
        out IVertex parseParent)
    {
        parseParent = MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                "BaseLinkRoundTripParse-" + Guid.NewGuid().ToString("N"));
        IEdge sourceEdge = parseParent.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "Source");
        IVertex parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                source,
                codeRepresentation,
                out IEdge parsedRootEdge);

        Assert.True(
            parseErrors == null || !parseErrors.Any(),
            parseErrors == null
                ? "The parser did not return a root edge."
                : string.Join(
                    Environment.NewLine,
                    parseErrors.Select(
                        edge => edge.To?.Value?.ToString() ??
                            "<error without value>"))
                + Environment.NewLine
                + source);
        _ = parsedRootEdge;
        return sourceEdge.To;
    }
}
