using m0;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class ZeroCodePerformanceWorkloadTests
{
    public ZeroCodePerformanceWorkloadTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void FreshPerformanceWorkloadParsesEveryProgram()
    {
        IEdge parsedRootEdge = ParseWorkload();
        HashSet<string> programNames = parsedRootEdge.To
            .OutEdgesRaw
            .Select(edge => edge.To?.Value?.ToString())
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal("code", parsedRootEdge.To.Value);
        Assert.All(
            ZeroCodePerformanceWorkload.ProgramNames,
            programName => Assert.Contains(programName, programNames));
    }

    private static IEdge ParseWorkload()
    {
        IVertex parseParent = MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                $"ZeroCodePerformance-{Guid.NewGuid():N}");
        IEdge sourceEdge = parseParent.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "Source");
        IVertex parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                ZeroCodePerformanceWorkload.Source,
                CodeRepresentationEnum.LinearizedManyLines,
                out IEdge parsedRootEdge);

        Assert.True(
            parseErrors == null || !parseErrors.Any(),
            parseErrors == null
                ? "The workload parser did not return a root edge."
                : string.Join(
                    Environment.NewLine,
                    parseErrors.Select(
                        edge => edge.To?.Value?.ToString() ??
                            "<error without value>")));
        Assert.NotNull(parsedRootEdge);
        return parsedRootEdge;
    }
}
