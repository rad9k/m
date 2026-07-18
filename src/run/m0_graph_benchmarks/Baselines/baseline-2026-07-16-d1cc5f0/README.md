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

The full post-correction rebaseline completed 61 `ShortRun` cases across 13 classes. Of 35 semantically equivalent Stage 0 comparisons, 34 are faster by mean. The complete runtime solution, including WPF UI projects, builds with zero errors; 61 isolated and 33 integration tests pass with no skips.

The first profile-selected optimization caches GraphChange watcher definitions while retaining dynamic scope evaluation and definition-change detection. Direct watcher preparation falls from 189.668 us and 89,784 B to 12.342 us and 2,088 B. Stage 0 listener commits are now 91.3% faster for one mutation and 95.8% faster for ten, with allocation reductions of 84.0% and 88.9%.

The cross-store acceptance follow-up found no superlinear detach/attach regression. Replacing the two per-edge LINQ store lookups with an allocation-free indexed scan makes the two-store lookup 51.5% faster and removes 136 B per lookup. Comparable `ShortRun` detach+attach means are 231.4 ns, 22.713 us, and 444.666 us for 1/100/1000 edges; allocation is 168 B, 10,464 B, and 104,068 B.

The shared logical/index rebuild optimization reduces the first inherited query
from two edge scans to one: the diagnostic contract falls from 6 to 3 scanned
edges while reporting both the logical snapshot and requested index rebuild.
At 1000 edges, final default-job means improve by 25.7% to 49.0% for the four
individual index paths and by 41.1% for the lazy all-index sequence. All 15
allocation measurements are lower. Final verification has 65 isolated and 33
integration tests passing with no skips, and `run.sln` builds with zero errors.

Generation-stamped inheritance invalidation replaces eager DFS over every source
descendant. A 1000-child add+remove mutation falls from 55.269 us and 146,600 B
to approximately 145 ns and 216 B. A depth-100 mutation without query falls
from 8.080 us to 94.13 ns, and mutating one 1000-child hierarchy while querying
an independent hierarchy falls from 66.793 us to 105.6 ns. Inherited target
value mutation+query improves by 18.3% to 52.1% across depth 1/10/100.

The explicit trade-off is a 4.0-4.7 ns allocation-free epoch check on warm
inherited lookups. Add/query-all/remove is 7.6% faster at one child and 5.2% /
9.2% slower at 100/1000 children, with 15-31% lower allocation. Diagnostics now
record zero descendant traversals and invalidations, 2 generation bumps, and
100 precise stale dependency checks for the warmed 100-child workload. Final
verification has 72 isolated and 34 integration tests passing with no skips,
and `run.sln` builds with zero errors.

Incremental local outgoing-index maintenance now patches already-warm
direct-meta, query-meta, value, and meta+value buckets for ordinary leaf
vertices. A 1000-edge warm add/query/remove/query diagnostic performs zero
rebuilds and scans zero edges. Default-job add/query-all/remove/query-all falls
from 29.309 us to 1.290 us at 100 edges and from 290.921 us to 6.584 us at 1000
edges (22.7x and 44.2x), while allocation falls to 216 B in both cases.
Meta-only mutation/query improves by 6.2x and 8.2x at those scales.

Inheritance, filesystem, loader reconstruction, stack, and inconsistent-state
paths retain a full-rebuild fallback. A new burst matrix confirms that
incremental wins when a query follows the writes, while lazy rebuild wins for
write-only series. Vertices therefore start with a one-write budget and learn
2x the observed burst size, capped at 256, only after a fallback is followed by
a query. At 1000 edges, learned `5/query` repeated three times is 320.34 us
versus 1,215.94 us rebuild-only (3.80x); `50/100/50` with queries is 453.99 us
versus 1,195.69 us (2.63x). A never-queried vertex retains the early
second-write fallback. Final verification has 80 isolated and 35 integration
tests passing with no skips; `run.sln` builds with zero errors.

ZeroCode stacks now batch-copy original edge references without reverse-edge
wiring and invalidate their indexes once after the complete batch. In a paired
DefaultJob, adding 1000 originals falls from 9.749 us and 17,272 B to 783.4 ns
and 8,728 B (12.44x); add followed by query falls from 55.523 us and 64,928 B
to 36.666 us and 56,384 B (1.51x). At 10 edges the gains are 1.48x for add and
1.28x for add+query. One-edge cost increases by only 4.5-11.5 ns with unchanged
allocation. Identity, order, duplicates, zero-copy aliasing, partial failure,
and no reverse wiring are covered; final verification has 84 isolated and 35
integration tests passing.

ZeroCode stacks now use specialized internal storage without changing
`IVertex`, ownership, outgoing indexes, or operator-facing APIs. Incoming,
meta-incoming, and ordinary-inheritance lists are created lazily, preserving
their mutable contract when a stack is used as an ordinary edge endpoint.
Ordinary graph vertices retain eager storage. Empty ephemeral stack creation
falls from 157.766 ns and 704 B to 88.216 ns and 512 B (1.79x, -27.3%
allocation). Creating a stack with a parent frame falls from 201.021 ns and
864 B to 133.000 ns and 672 B. Warm local and parent lookups remain
allocation-free at 9.643 ns and 38.667 ns. A parsed executable function-call
test covers nested frames and return end to end; final verification has
92 isolated and 36 integration tests passing.

ZeroCode stack parent fallback now caches the first frame selected by the
unchanged `$StackFrameInherits` query and invalidates that cache on every
sanctioned stack-edge mutation. At frame depths 1/10/100, resolved root lookup
falls from 23.74/182.69/3,376.06 ns to 14.78/89.20/861.62 ns; a miss across the
whole chain falls from 29.05/196.36/3,726.42 ns to
15.06/91.45/863.93 ns. All six cases remain allocation-free. Warm local lookup
shows no regression at 5.520 ns. A single cache slot adds 8 B per stack
(empty stack: 512 to 520 B), leaving empty construction allocation 26.1% below
the original pre-specialization baseline. A depth-100 diagnostic records
201,000 warm cache hits and zero misses. Final verification has 98 isolated
and 36 integration tests passing; `run.sln` builds with zero errors.

Graph query helpers now offer allocation-free `EdgeQueryResult` views for
zero/one/many and full out/in scans without changing the legacy list API.
Empty and singleton result construction is 37.5–47.7% faster and removes
32/88 B. Counting a 1000-edge full scan improves by 155.7–228.4x and removes
8,056 B; enumerating that scan improves by 19.6–21.5% with zero allocation.
The existing filtered-many list enumerator remains about 20% faster, so bulk
many-result loops deliberately stay on the legacy API while first/exist/count,
empty/single, and full-scan paths use direct or read-only results. The new
surface preserves order and edge identity without exposing `IList` mutation.
Final verification has 101 isolated and 36 integration tests passing;
`run.sln` builds with zero errors.

The private inheritance-aware query-meta index now uses a dedicated
zero/one/many `EdgeBucket` instead of an `object` union. It preserves order,
duplicates, and exact edge identity while removing the lookup type test.
Representative inherited-source hits improve by 6.2-12.3%; a direct singleton
hit improves by 7.3%. Direct 1000-edge hit and miss differences are within
combined run variance, and every lookup remains allocation-free. The rollout is
intentionally limited to the low-key-count query-meta index: converting public
high-cardinality value indexes would enlarge every entry and change exposed
types.

Value-key profiling records 7.4-8.0 ns for short strings, 271-281 ns for
1024-character strings, and 32 B per non-cached integer/decimal query. A
conditional weak hash prototype made repeated long-string lookups 75.6-79.4%
faster but doubled short-string cost, so it is documented and rejected rather
than shipped. Current `ToString` equivalence remains unchanged. Final
verification has 104 isolated and 36 integration tests passing.

File-backed content is now excluded from value and meta+value index keys.
Queries that do not request file content therefore perform no hidden I/O,
while value-only and Content equality retain compatibility through an ordered
fallback that reads the current file. An unrelated meta+value rebuild for a
10 MB file falls from 30.861 ms and 42.06 MB to 91.07 ns and 320 B
(338,876x, -99.9992% allocation). The same workload improves by 1,652x at
1 KB and 32,529x at 1 MB. Required Content equality is also 38.7% faster at
10 MB because the full content is no longer first materialized as a dictionary
key. Counters and contracts verify zero reads for meta-only/unrelated queries,
one read per explicit evaluation, external-edit visibility, inherited-meta
matching, out/in ordering, duplicates, and exact references. Final
verification has 108 isolated and 37 integration tests passing; `run.sln`
builds with zero errors.

`$NoInherit` now has one logical-merge contract across ordinary and filesystem
vertices. Marker add/remove and marker-meta rename invalidate only exact source
dependencies through generation stamps; unrelated warmed hierarchies do not
rebuild. The merge reuses the immediately preceding meta decision and the
filesystem overlay materializes one list instead of two. At 1000 repeated-meta
edges, ordinary rebuild falls from 10.620 us to 2.515 us (4.22x), while the
corrected filesystem rebuild is 2.860 us and 512 B. Its old 2.301 us result was
semantically invalid and allocated 32,520 B, so correctness costs 558 ns while
removing 98.4% of allocation. The audit follow-up keeps generated overlay
metadata in inherited query indexes and invalidates host caches on every
`Refresh`; matching `Filename` rebuilds cost 637.7 ns / 3.480 us for 1/1000
parent edges. Final verification has 110 isolated and 39 integration tests
passing; `run.sln` builds with zero errors.

Core graph traversals now use explicit stacks for inheritance, subgraph
collection, deep iteration, inheritance-level search, and deep-copy scope
mapping. Depth-first preorder, first-result behavior, mutation snapshots,
cycle handling, and exact copy maps remain unchanged. Contracts complete
10,000-level inheritance/subgraph traversals and a 5,000-level deep copy
without stack overflow.

At depth 1000, missing inherited-type comparison falls from 6.891 ms to
33.635 us (204.87x), non-link subgraph collection from 108.764 us to
55.756 us (1.95x), and a `DeepIterator` miss from 130.547 us to 39.368 us
(3.32x). A deep inherited-query rebuild improves from 158.053 us to
127.055 us while allocation falls from 251.70 KB to 217,832 B. The depth-10
rebuild mean is 105 ns slower within overlapping dispersion; all other
traversal rows improve and every measured allocation is lower. Final
follow-up coverage also makes parent-frame fallback and ZeroCode `NextAtom`
execution iterative. At depth 1000, parent-frame hit/miss improve by
20.0%/23.4% and remain allocation-free; `NextAtom` improves by 30.0% while
falling from 40,000 B to 0 B. Its depth-10/100 rows improve by 16.2%/20.7% and
also reach 0 B. Contracts verify depth 10,000, DFS order, early return, and
cycle termination. Final verification has 117 isolated and 42 integration
tests passing; `run.sln` builds with zero errors.

The final curated acceptance gate completed 132 production benchmark cases
across 13 classes without errors. Warm direct-meta lookup remains
allocation-free at 4.6–5.7 ns; depth-1000 parent-frame hit/miss is
7.704/8.137 us at 0 B, `NextAtom` is 49.461 us at 0 B, and the 1000-edge
incremental all-index lifecycle is 2.748 us and 216 B. Optional read-only
public edge views are deferred: no production caller mutates the exposed
lists, the five raw-add paths remain controlled, incremental indexes are
already callback-safe, and changing `IVertex` would break external source and
binary compatibility for no direct performance gain.

## Commands

```powershell
dotnet test m0_graph_tests/m0_graph_tests.csproj --configuration Release
dotnet test m0_graph_integration_tests/m0_graph_integration_tests.csproj --configuration Release
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --query-cache-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --stack-lifecycle-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --stack-batch-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --stack-parent-cache-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --transaction-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --watcher-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --detach-attach-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --incremental-index-diagnostics
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*InheritedIndexRebuildBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*IncrementalOutgoingIndexBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*IncrementalOutgoingBurstBenchmarks*"
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*IncrementalOutgoingWriteOnlyBurstBenchmarks*"
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*IncrementalOutgoingRepeatedBurstBenchmarks*"
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*StackOriginalEdgeBatchBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*StackInfrastructureBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "m0_graph_benchmarks.StackBenchmarks.*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*StackParentFrameCacheBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*QueryResultBenchmarks*"
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*ValueKeyLookupBenchmarks*" "*EdgeBucketRepresentationBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*StringHashCacheBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*FileContentIndexBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*NoInheritOverlayBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*IterativeTraversalBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*IterativeExecutionBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*WideHierarchyMutationBenchmarks*" --job default
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*DeepHierarchyMutationBenchmarks*" "*IndependentHierarchyMutationBenchmarks*" "*InheritedTargetValueMutationBenchmarks*" --job short
dotnet run --project m0_graph_benchmarks/m0_graph_benchmarks.csproj --configuration Release -- --filter "*" --job short
```

## Current coverage boundary

This first baseline, its correction runs, and the optimization cycles cover edge lifecycle, physical incoming edges, query indexes, source/meta inheritance, dynamic `$NoInherit`, filesystem overlay inheritance, generation-stamped dependency validation, read-your-writes, full LIFO rollback, immediate non-transacted delivery, coalesced transactional listeners, cached watcher definitions, dynamic watcher scope updates and deduplication, nested suppression, JSON/Binary roundtrip, cross-store detach/attach, identifier collisions, store roots, filesystem rename and rollback, file-content value lifecycle, simple, shared-child, cyclic, and 5,000-level deep-copy contracts, 10,000-level iterative traversals, stack identity/aliasing, frame add/remove and shadowing, hierarchy scaling, allocations, GC, rebuild scans, and invalidation counts.

No graph contract tests remain skipped. Parsed function call, nested frame, and
return are now executable integration contracts; a future replacement of the
entire `EasyVertex` base would still require differential coverage of every
`BaseInstructions` operator.
