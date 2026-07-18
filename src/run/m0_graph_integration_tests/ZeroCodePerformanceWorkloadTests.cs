using m0;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
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

    [Fact]
    public void FreshPerformanceWorkloadExecutesWithoutReportedErrors()
    {
        IEdge parsedRootEdge = ParseWorkload();
        RecordingUserInteraction userInteraction =
            new RecordingUserInteraction();
        IUserInteraction previousUserInteraction =
            MinusZero.Instance.UserInteraction;
        MinusZero.Instance.SetUserInteraction(userInteraction);

        try
        {
            foreach (string programName in
                ZeroCodePerformanceWorkload.ProgramNames)
            {
                IVertex program = parsedRootEdge.To
                    .OutEdgesRaw
                    .Single(
                        edge => string.Equals(
                            edge.To?.Value?.ToString(),
                            programName,
                            StringComparison.Ordinal))
                    .To;
                int previousErrorCount =
                    userInteraction.Exceptions.Count;
                IVertex result =
                    MinusZero.Instance.DefaultExecuter.Execute(
                        InstructionHelpers.CreateStack(),
                        program);

                Assert.NotNull(result);
                Assert.Equal(
                    previousErrorCount,
                    userInteraction.Exceptions.Count);

                if (programName == "Code7")
                    Assert.Equal(
                        4369,
                        GraphUtil.GetQueryOutCount(
                            result,
                            "E",
                            null));
                else if (programName == "Code8")
                    Assert.Equal(
                        100001,
                        Convert.ToInt32(
                            GraphUtil.GetQueryOutFirst(
                                result,
                                "A",
                                null)
                                ?.Value));
            }
        }
        finally
        {
            MinusZero.Instance.SetUserInteraction(
                previousUserInteraction);
        }
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

    private sealed class RecordingUserInteraction
        : NoOpUserInteraction
    {
        internal List<IVertex> Exceptions { get; } =
            new List<IVertex>();

        public override void InteractionOutputException(
            IVertex exception)
        {
            Exceptions.Add(exception);
        }
    }
}
