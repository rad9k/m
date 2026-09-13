using BenchmarkDotNet.Attributes;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
[IterationTime(100)]
public class IterativeExecutionBenchmarks
{
    private RecordingExecution execution = null!;
    private INoInEdgeInOutVertexVertex stack = null!;
    private IVertex chainRoot = null!;

    [Params(10, 100, 1000)]
    public int Depth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();

        var nextAtomMeta =
            MinusZero.Instance.Root.Get(
                false,
                @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");
        chainRoot = CreateTempVertex("ChainRoot");
        IVertex current = chainRoot;

        for (var index = 0;
            index < Depth;
            index++)
        {
            var next =
                CreateTempVertex(index);
            current.AddEdge(
                nextAtomMeta,
                next);
            current = next;
        }

        stack = InstructionHelpers.CreateStack();
        execution =
            new RecordingExecution(stack);
    }

    [Benchmark]
    public int ExecuteNextAtomChain()
    {
        execution.Reset();
        _ = ZeroCodeExecutonUtil
            .SequentiallyExecuteInstructions_NextEdges(
                execution,
                stack,
                chainRoot,
                out _);
        return execution.ExecutionCount;
    }

    private static IVertex CreateTempVertex(
        object value)
    {
        return MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                value);
    }

    private sealed class RecordingExecution
        : IExecution
    {
        public RecordingExecution(
            INoInEdgeInOutVertexVertex stack)
        {
            Stack = stack;
            NewVertexCreationSpace = stack;
        }

        public int ExecutionCount { get; private set; }

        public INoInEdgeInOutVertexVertex Stack
        {
            get;
            set;
        }

        public IVertex NewVertexCreationSpace
        {
            get;
            set;
        }

        public bool MetaMode { get; set; }

        public void Reset()
        {
            ExecutionCount = 0;
        }

        public void AddStackFrame()
        {
            throw new NotSupportedException();
        }

        public void AddStackFrame(
            IVertex newStackFrame)
        {
            throw new NotSupportedException();
        }

        public void RemoveStackFrame()
        {
            throw new NotSupportedException();
        }

        public void CreateEmptyStack()
        {
            throw new NotSupportedException();
        }

        public INoInEdgeInOutVertexVertex
            ExecuteInstructionByMontevideoPrinciples(
                IVertex inputQs,
                IVertex instructionVertex,
                out bool isStackFrameReturn)
        {
            return ExecuteInstruction(
                inputQs,
                instructionVertex,
                out isStackFrameReturn);
        }

        public INoInEdgeInOutVertexVertex
            ExecuteInstruction(
                IVertex inputQs,
                IVertex instructionVertex,
                out bool isStackFrameReturn)
        {
            ExecutionCount++;
            isStackFrameReturn = false;
            return Stack;
        }
    }
}
