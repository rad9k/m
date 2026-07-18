using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using m0;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph.ExecutionFlow;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[SimpleJob(
    RuntimeMoniker.Net10_0,
    launchCount: 1,
    warmupCount: 3,
    iterationCount: 5,
    invocationCount: 1)]
public class ZeroCodeExecutionBenchmarks
{
    [Params(
        "Code1",
        "Code3",
        "Code4",
        "Code7b",
        "Code12",
        "Code14")]
    public string ProgramName { get; set; } = null!;

    private IVertex program = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        program = ZeroCodeBenchmarkSetup.GetProgram(
            ProgramName);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        ExecutionFlowHelper.StartTransaction();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        ExecutionFlowHelper.RollbackTransaction();
    }

    [Benchmark]
    public IVertex Execute()
    {
        return MinusZero.Instance.DefaultExecuter.Execute(
            InstructionHelpers.CreateStack(),
            program);
    }
}

[MemoryDiagnoser]
[SimpleJob(
    RuntimeMoniker.Net10_0,
    launchCount: 1,
    warmupCount: 1,
    iterationCount: 3,
    invocationCount: 1)]
public class ZeroCodeLongRunningBenchmarks
{
    [Params("Code7", "Code8")]
    public string ProgramName { get; set; } = null!;

    private IVertex program = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        program = ZeroCodeBenchmarkSetup.GetProgram(
            ProgramName);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        ExecutionFlowHelper.StartTransaction();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        ExecutionFlowHelper.RollbackTransaction();
    }

    [Benchmark]
    public IVertex Execute()
    {
        return MinusZero.Instance.DefaultExecuter.Execute(
            InstructionHelpers.CreateStack(),
            program);
    }
}

internal static class ZeroCodeBenchmarkSetup
{
    internal static IVertex GetProgram(string programName)
    {
        if (!MinusZero.Instance.IsInitialized)
        {
            MinusZero.Instance.DoLog = false;
            MinusZero.Instance.SetUserInteraction(
                new NoOpUserInteraction());
            MinusZero.Instance.Initialize();
        }

        ZeroCodePerformanceCounters.Enabled = false;
        IVertex parseParent = MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                $"ZeroCodeBenchmark-{Guid.NewGuid():N}");
        IEdge sourceEdge = parseParent.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "Source");
        IVertex parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                ZeroCodePerformanceWorkload.Source,
                CodeRepresentationEnum.LinearizedManyLines,
                out IEdge parsedRootEdge);

        if (parseErrors != null && parseErrors.Any())
            throw new InvalidOperationException(
                "Fresh ZeroCode workload failed to parse.");

        return parsedRootEdge.To
            .OutEdgesRaw
            .Single(
                edge => string.Equals(
                    edge.To?.Value?.ToString(),
                    programName,
                    StringComparison.Ordinal))
            .To;
    }
}
