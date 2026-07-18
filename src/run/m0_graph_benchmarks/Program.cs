using BenchmarkDotNet.Running;
using m0_graph_benchmarks;

if (args.Contains("--zerocode-diagnostics"))
{
    string[] requestedPrograms = args
        .Where(
            argument => argument.StartsWith(
                "--program=",
                StringComparison.Ordinal))
        .Select(
            argument => argument[
                "--program=".Length..])
        .Where(programName => programName.Length > 0)
        .ToArray();
    ZeroCodeDiagnosticsRunner.Run(requestedPrograms);
    return;
}

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);
