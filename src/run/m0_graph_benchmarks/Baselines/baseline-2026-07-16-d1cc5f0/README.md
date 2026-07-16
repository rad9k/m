# Graph core baseline — 2026-07-16

## Source state

- Repository commit: `d1cc5f00e15a605842b37cf117db917d23eb44b5`
- State label: `d1cc5f0 + uncommitted graph-stage0 infrastructure and diagnostics`
- Configuration: `Release`
- Process architecture: x64
- Benchmark project: `m0_graph_benchmarks`
- Contract test project: `m0_graph_tests`
- Bootstrapped integration test project: `m0_graph_integration_tests`
- Shared deterministic fixture: `m0_graph_test_support`

The baseline represents the working tree after adding Stage 0 tests, benchmarks, and disabled-by-default diagnostic counters. It is not a measurement of bare commit `d1cc5f0`.

## Environment

- OS: Windows 11 Home, `10.0.26200`
- CPU: Intel Core i7-1260P, 12 physical / 16 logical cores
- Memory: 15.61 GB
- .NET SDK: `10.0.302`
- Runtime: `.NET 10.0.10`
- JIT: x64 RyuJIT x86-64-v3
- GC: Concurrent Workstation
- BenchmarkDotNet: `0.15.8`
- Benchmark job: `ShortRun`, 1 launch, 3 warmups, 3 measurement iterations

## Deterministic graph sizes

- Query lookup: 1, 100, and 1000 outgoing edges
- Mutation/query: 1, 100, and 1000 outgoing edges
- Deep inheritance: 1, 10, and 100 `$Inherits` levels
- Wide inheritance: 1, 100, and 1000 direct children
- Stack lookup: 1000 copied edges
- Stack construction: 100 copied edges per stack
- Diagnostic workload: 1000 direct edges, 100 children, depth 25, 100 created stacks

Graph construction runs in `GlobalSetup` and is excluded from lookup measurements. Construction is included only by benchmarks whose names explicitly describe creation or mutation.

## Verification

- Stage 0 isolated contract tests: 24 passed, 0 failed, 1 known-defect test skipped.
- Bootstrapped integration tests: 8 passed, 0 failed, 2 known-defect tests skipped.
- Stage 0 benchmark matrix: 35 cases completed.
- Production diagnostics are disabled during BenchmarkDotNet measurements.
- Detailed diagnostic workload is recorded separately in `diagnostics.json`.

Post-baseline correction 2 verification has 25 passing isolated tests and adds three inherited target-value mutation cases. Its `ShortRun` comparison and default-job acceptance measurements are appended to `benchmark-results.md`.

Post-baseline correction 3 verification has 30 passing isolated tests, retains all 8 passing bootstrapped integration tests, and adds three cross-store detach/attach benchmark cases. Its `ShortRun` and default-job acceptance measurements are appended to `benchmark-results.md`.

Post-baseline correction 4 verification has 36 passing isolated tests, retains all 8 passing bootstrapped integration tests, and adds three inheritance-validation benchmark cases. Existing deep/wide hierarchy comparisons and the default-job acceptance measurements are appended to `benchmark-results.md`.

Post-baseline correction 5 verification has 41 passing isolated tests, retains all 8 passing bootstrapped integration tests, and adds three exact-meta identity-filtering benchmark cases. Its `ShortRun` and default-job acceptance measurements are appended to `benchmark-results.md`.

Post-baseline correction 6 verification has 47 passing isolated tests and 18 passing bootstrapped integration tests. The shared-child deep-copy test is enabled, and nine inherited-type, cardinality, and map-based deep-copy benchmark cases are appended to `benchmark-results.md`.

Post-baseline correction 7 verification has 53 passing isolated tests and 21 passing bootstrapped integration tests, with the same 2 correction-9 tests skipped. Controlled bootstrap diagnostics recorded 100 parsed-query cache lookups, 68 unique regular queries, 32 hits, and 68/512 final occupancy. Six bounded-LRU hit/eviction benchmark cases are appended to `benchmark-results.md`.

Post-baseline correction 8 verification has 54 passing isolated tests and 24 passing bootstrapped integration tests, with the same 2 correction-9 tests skipped. A 10,011-stack lifecycle workload changes TempStore registry growth from 10,011 entries to zero while retaining all 11 active frames. Focused registered-versus-ephemeral construction measurements are appended to `benchmark-results.md`.

Post-baseline correction 9 verification has 57 passing isolated tests and 28 passing bootstrapped integration tests, with no skipped tests. Ten repeated value mutations retain 10 rollback atoms but produce one final listener entry. The transaction watcher comparison and coalescing diagnostics are appended to `benchmark-results.md`.

Post-baseline correction 11 verification has 61 passing isolated tests and 31 passing bootstrapped integration tests, with no skipped tests. Collision enforcement, root protection, atomic filesystem rename and rollback, directory no-mutation failure, and both `FileContentVertex` modes are covered. Four focused acceptance measurements are appended to `benchmark-results.md`.

The full post-correction rebaseline completed 61 `ShortRun` cases across 13 classes. Of 35 semantically equivalent Stage 0 comparisons, 34 are faster by mean. The complete runtime solution, including WPF UI projects, builds with zero errors; 61 isolated and 32 integration tests pass with no skips.

The first profile-selected optimization caches GraphChange watcher definitions while retaining dynamic scope evaluation and definition-change detection. Direct watcher preparation falls from 189.668 us and 89,784 B to 12.342 us and 2,088 B. Stage 0 listener commits are now 91.3% faster for one mutation and 95.8% faster for ten, with allocation reductions of 84.0% and 88.9%.

## Commands

```powershell
dotnet test m0_graph_tests/m0_graph_tests.csproj --configuration Release
dotnet test m0_graph_integration_tests/m0_graph_integration_tests.csproj --configuration Release
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --query-cache-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --stack-lifecycle-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --transaction-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --watcher-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*" --job short
```

## Current coverage boundary

This first baseline, its correction runs, and the full rebaseline cover edge lifecycle, physical incoming edges, query indexes, source/meta inheritance, read-your-writes, full LIFO rollback, immediate non-transacted delivery, coalesced transactional listeners, cached watcher definitions, dynamic watcher scope updates and deduplication, nested suppression, JSON/Binary roundtrip, cross-store detach/attach, identifier collisions, store roots, filesystem rename and rollback, file-content value lifecycle, simple and shared-child deep-copy contracts, stack identity/aliasing, frame add/remove and shadowing, hierarchy scaling, allocations, GC, rebuild scans, and invalidation counts.

No graph contract tests remain skipped. Full executable ZeroCode function-call conformance remains required before any stack redesign.
