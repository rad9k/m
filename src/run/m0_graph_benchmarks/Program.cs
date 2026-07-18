using BenchmarkDotNet.Running;
using m0_graph_benchmarks;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);
