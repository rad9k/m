using System.Diagnostics;
using m0;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

internal static class ZeroCodeDiagnosticsRunner
{
    internal static void Run(IEnumerable<string> requestedPrograms)
    {
        RecordingUserInteraction userInteraction =
            new RecordingUserInteraction();
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(userInteraction);
        MinusZero.Instance.Initialize();

        string[] programNames = requestedPrograms.Any()
            ? requestedPrograms.ToArray()
            : ZeroCodePerformanceWorkload.ProgramNames;

        foreach (string programName in programNames)
            RunProgram(programName, userInteraction);
    }

    private static void RunProgram(
        string programName,
        RecordingUserInteraction userInteraction)
    {
        IEdge parsedRootEdge = ParseWorkload();
        IVertex program = parsedRootEdge.To
            .OutEdgesRaw
            .FirstOrDefault(
                edge => string.Equals(
                    edge.To?.Value?.ToString(),
                    programName,
                    StringComparison.Ordinal))
            ?.To ??
            throw new InvalidOperationException(
                $"Program '{programName}' was not parsed.");

        userInteraction.Clear();
        ZeroCodePerformanceCounters.Reset();
        ZeroCodePerformanceCounters.Enabled = true;
        long allocatedBytesBefore =
            GC.GetAllocatedBytesForCurrentThread();
        long started = Stopwatch.GetTimestamp();
        IVertex result;

        try
        {
            result = MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                program);
        }
        finally
        {
            ZeroCodePerformanceCounters.Enabled = false;
        }

        TimeSpan elapsed = Stopwatch.GetElapsedTime(started);
        long allocatedBytes =
            GC.GetAllocatedBytesForCurrentThread() -
            allocatedBytesBefore;
        ZeroCodePerformanceSnapshot snapshot =
            ZeroCodePerformanceCounters.GetSnapshot();

        Console.WriteLine(
            $"{programName}: {elapsed.TotalMilliseconds:F3} ms, " +
            $"{allocatedBytes:N0} B allocated, " +
            $"{result?.OutEdgesRaw.Count ?? 0} result edges, " +
            $"{userInteraction.Exceptions.Count} errors");
        Console.WriteLine(
            ZeroCodePerformanceCounters.FormatReport(snapshot));

        foreach (IVertex exception in userInteraction.Exceptions)
            Console.WriteLine(
                "Execution error: " +
                FormatException(exception));
    }

    private static IEdge ParseWorkload()
    {
        IVertex parseParent = MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                $"ZeroCodeDiagnostics-{Guid.NewGuid():N}");
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
                "Fresh ZeroCode workload failed to parse: " +
                string.Join(
                    "; ",
                    parseErrors.Select(
                        edge => edge.To?.Value?.ToString() ??
                            "<error without value>")));

        return parsedRootEdge ??
            throw new InvalidOperationException(
                "Fresh ZeroCode workload did not produce a root edge.");
    }

    private static string FormatException(IVertex exception)
    {
        IEnumerable<string> values = exception
            .OutEdgesRaw
            .Select(
                edge =>
                    $"{edge.Meta?.Value}: {edge.To?.Value}");
        return string.Join(" | ", values);
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

        internal void Clear()
        {
            Exceptions.Clear();
        }
    }
}
