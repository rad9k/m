using BenchmarkDotNet.Attributes;
using m0;
using m0.Foundation;
using m0.Graph.ExecutionFlow;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

[MemoryDiagnoser]
[InProcess]
[WarmupCount(1)]
[IterationCount(3)]
[InvocationCount(256)]
public class TransactionWatcherBenchmarks
{
    private IVertex vertex = null!;
    private int eventCount;
    private int nextValue;

    [GlobalSetup]
    public void Setup()
    {
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(new NoOpUserInteraction());
        MinusZero.Instance.Initialize();
        vertex = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "TransactionBenchmark");
        ExecutionFlowHelper.AddTriggerAndListener(
            vertex,
            execution =>
            {
                eventCount++;
                return execution.Stack;
            });
    }

    [Benchmark]
    public int CommitSingleValueChange()
    {
        var initialEventCount = eventCount;
        ExecutionFlowHelper.StartTransaction();
        vertex.Value = ++nextValue;
        ExecutionFlowHelper.CommitTransaction();
        return eventCount - initialEventCount;
    }

    [Benchmark]
    public int CommitTenValueChanges()
    {
        var initialEventCount = eventCount;
        ExecutionFlowHelper.StartTransaction();

        for (var index = 0; index < 10; index++)
            vertex.Value = ++nextValue;

        ExecutionFlowHelper.CommitTransaction();
        return eventCount - initialEventCount;
    }
}

[MemoryDiagnoser]
[InProcess]
[WarmupCount(1)]
[IterationCount(3)]
[InvocationCount(256)]
public class TransactionNoWatcherBenchmarks
{
    private IVertex vertex = null!;
    private int nextValue;

    [GlobalSetup]
    public void Setup()
    {
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();
        vertex = MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            "TransactionNoWatcherBenchmark");
    }

    [Benchmark]
    public object CommitSingleValueChange()
    {
        ExecutionFlowHelper.StartTransaction();
        vertex.Value = ++nextValue;
        ExecutionFlowHelper.CommitTransaction();
        return vertex.Value;
    }

    [Benchmark]
    public object CommitTenValueChanges()
    {
        ExecutionFlowHelper.StartTransaction();

        for (var index = 0; index < 10; index++)
            vertex.Value = ++nextValue;

        ExecutionFlowHelper.CommitTransaction();
        return vertex.Value;
    }
}
