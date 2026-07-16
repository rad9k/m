using BenchmarkDotNet.Running;
using m0.Graph;
using m0_graph_benchmarks;

if (args.Contains("--stack-lifecycle-diagnostics"))
{
    GraphDiagnosticsRunner.RunStackLifecycle();
    return;
}

if (args.Contains("--query-cache-diagnostics"))
{
    GraphDiagnosticsRunner.RunQueryParseCache();
    return;
}

if (args.Contains("--transaction-diagnostics"))
{
    GraphDiagnosticsRunner.RunTransactionCoalescing();
    return;
}

if (args.Contains("--watcher-diagnostics"))
{
    GraphDiagnosticsRunner.RunWatcherPreparation();
    return;
}

if (args.Contains("--diagnostics"))
{
    GraphDiagnosticsRunner.Run();
    return;
}

GraphPerformanceCounters.Enabled = false;
BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);
