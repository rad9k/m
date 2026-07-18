# Graph core benchmark baseline

BenchmarkDotNet 0.15.8, `ShortRun` (3 warmups, 3 measurement iterations), .NET 10.0.10 x64, Concurrent Workstation GC.

The broad `ShortRun` matrix is intended to locate scaling and allocation hotspots. Before accepting or rejecting an optimization, rerun its focused benchmark with the default BenchmarkDotNet job because three measurement iterations produce wide confidence intervals.

## Warm query lookup

| Method | Edge count | Mean | Allocated |
|---|---:|---:|---:|
| Direct meta, many | 1 | 9.194 ns | 0 B |
| Direct meta and value, singleton | 1 | 30.302 ns | 0 B |
| Direct meta miss | 1 | 6.816 ns | 0 B |
| Derived meta matched by base meta | 1 | 9.448 ns | 0 B |
| Inherited source edge | 1 | 11.678 ns | 0 B |
| Direct meta, many | 100 | 8.791 ns | 0 B |
| Direct meta and value, singleton | 100 | 31.283 ns | 0 B |
| Direct meta miss | 100 | 7.434 ns | 0 B |
| Derived meta matched by base meta | 100 | 8.332 ns | 0 B |
| Inherited source edge | 100 | 12.073 ns | 0 B |
| Direct meta, many | 1000 | 8.653 ns | 0 B |
| Direct meta and value, singleton | 1000 | 39.779 ns | 0 B |
| Direct meta miss | 1000 | 7.577 ns | 0 B |
| Derived meta matched by base meta | 1000 | 10.255 ns | 0 B |
| Inherited source edge | 1000 | 11.748 ns | 0 B |

## Target value mutation followed by query

| Edge count | Mean | Gen0 / 1000 ops | Gen1 / 1000 ops | Allocated |
|---:|---:|---:|---:|---:|
| 1 | 137.4 ns | 0.0398 | 0 | 376 B |
| 100 | 4.635 us | 0.4346 | 0.0098 | 4,120 B |
| 1000 | 56.882 us | 4.2188 | 0.8203 | 39,976 B |

## Deep inheritance

| Method | Depth | Mean | Allocated |
|---|---:|---:|---:|
| Warm query at deepest child | 1 | 11.847 ns | 0 B |
| Add root edge, query deepest child, remove | 1 | 641.633 ns | 1,408 B |
| Warm query at deepest child | 10 | 9.314 ns | 0 B |
| Add root edge, query deepest child, remove | 10 | 3.140 us | 5,560 B |
| Warm query at deepest child | 100 | 9.221 ns | 0 B |
| Add root edge, query deepest child, remove | 100 | 27.232 us | 46,056 B |

## Wide inheritance

| Method | Children | Mean | Allocated |
|---|---:|---:|---:|
| Add and remove parent edge | 1 | 274.2 ns | 648 B |
| Add, query all children, remove | 1 | 510.5 ns | 1,304 B |
| Add and remove parent edge | 100 | 10.347 us | 15,048 B |
| Add, query all children, remove | 100 | 37.196 us | 76,688 B |
| Add and remove parent edge | 1000 | 137.024 us | 146,600 B |
| Add, query all children, remove | 1000 | 429.231 us | 762,640 B |

## ZeroCode stack

| Method | Mean | Allocated |
|---|---:|---:|
| Local query | 11.44 ns | 0 B |
| Parent frame query | 39.49 ns | 0 B |
| Create stack, copy 100 edges, unregister | 3.653 us | 13,944 B |

## Transaction watchers

These two scenarios use the in-process toolchain with 256 invocations, one warmup, and three measurement iterations because the application process-exit handler intentionally waits during shutdown.

| Method | Mean | Gen0 / 1000 ops | Gen1 / 1000 ops | Allocated |
|---|---:|---:|---:|---:|
| Commit one value change | 598.9 us | 11.7188 | 3.9063 | 107.76 KB |
| Commit ten value changes | 1.020 ms | 15.6250 | 7.8125 | 169.37 KB |

The generated CSV, Markdown, and HTML reports are in `m0_graph_benchmarks/BenchmarkDotNet.Artifacts/baseline-2026-07-16/results`.

## Post-baseline: correction 2

Correction 2 fixes stale value and meta+value indexes on source vertices inheriting an edge whose target value changes. The original implementation did not produce correct inherited-query results, so only the direct workload has a valid before/after performance comparison.

### Direct target value mutation — ShortRun comparison

| Edge count | Baseline mean | Corrected mean | Mean change | Baseline allocated | Corrected allocated |
|---:|---:|---:|---:|---:|---:|
| 1 | 137.4 ns | 110.7 ns | -19.4% | 376 B | 336 B |
| 100 | 4.635 us | 4.630 us | -0.1% | 4,120 B | 4,080 B |
| 1000 | 56.882 us | 47.438 us | -16.6% | 39,976 B | 39,936 B |

### Accurate acceptance run — default job

| Workload | Size | Mean | Allocated |
|---|---:|---:|---:|
| Direct target value mutation/query | 1 edge | 72.47 ns | 336 B |
| Direct target value mutation/query | 100 edges | 2.518 us | 4,080 B |
| Direct target value mutation/query | 1000 edges | 29.058 us | 39,936 B |
| Inherited target value mutation/query | depth 1 | 130.6 ns | 552 B |
| Inherited target value mutation/query | depth 10 | 948.4 ns | 1,784 B |
| Inherited target value mutation/query | depth 100 | 8.314 us | 15,456 B |

The corrected direct fast path adds no allocation regression. The inherited path scales linearly because changing a shared target must invalidate every descendant source that logically exposes the edge.

## Post-baseline: correction 3

Correction 3 fixes stale incoming indexes during detach, collection mutation in multi-edge `InDetach`, partially attached edges after endpoint or hook failures, and a stuck removal guard. The original implementation failed all four corresponding regression scenarios, so there is no valid before/after performance comparison.

### Cross-store detach and attach — ShortRun

| Edge count | Mean | Allocated |
|---:|---:|---:|
| 1 | 337.6 ns | 504 B |
| 100 | 31.368 us | 44,066 B |
| 1000 | 490.358 us | 440,080 B |

### Accurate acceptance run — default job

| Edge count | Mean | Error | Allocated |
|---:|---:|---:|---:|
| 1 | 212.0 ns | 4.30 ns | 504 B |
| 100 | 19.983 us | 0.399 us | 44,066 B |
| 1000 | 328.200 us | 3.898 us | 440,080 B |

The workload performs a complete detach and attach cycle and preserves every physical reverse link. Runtime and allocation scale linearly with the number of cross-store edges.

## Post-baseline: correction 4

Correction 4 defines `$Inherits` as a DAG for new mutations while keeping traversals safe for cyclic legacy data. Existing hierarchy benchmarks retain exactly the same allocations as the Stage 0 baseline.

### Existing deep hierarchy — ShortRun comparison

| Workload | Depth | Baseline mean | Corrected mean | Mean change |
|---|---:|---:|---:|---:|
| Warm inherited query | 1 | 11.847 ns | 4.423 ns | -62.7% |
| Warm inherited query | 10 | 9.314 ns | 4.757 ns | -48.9% |
| Warm inherited query | 100 | 9.221 ns | 4.398 ns | -52.3% |
| Add root edge, query leaf, remove | 1 | 641.633 ns | 579.987 ns | -9.6% |
| Add root edge, query leaf, remove | 10 | 3.140 us | 2.313 us | -26.3% |
| Add root edge, query leaf, remove | 100 | 27.232 us | 20.200 us | -25.8% |

### Existing wide hierarchy — ShortRun comparison

| Workload | Children | Baseline mean | Corrected mean | Mean change |
|---|---:|---:|---:|---:|
| Add and remove parent edge | 1 | 274.2 ns | 277.9 ns | +1.3% |
| Add and remove parent edge | 100 | 10.347 us | 6.495 us | -37.2% |
| Add and remove parent edge | 1000 | 137.024 us | 77.581 us | -43.4% |
| Add, query children, remove | 1 | 510.5 ns | 530.9 ns | +4.0% |
| Add, query children, remove | 100 | 37.196 us | 26.742 us | -28.1% |
| Add, query children, remove | 1000 | 429.231 us | 338.555 us | -21.1% |

`ShortRun` variance is too wide to treat the apparent speedups as guaranteed gains, but there is no systematic regression and allocations are unchanged.

### Valid inheritance add/remove — default job

| Parent depth | Mean | Allocated |
|---:|---:|---:|
| 0 | 250.5 ns | 888 B |
| 10 | 628.7 ns | 1,984 B |
| 100 | 4.755 us | 12,200 B |

This new workload includes cycle validation, physical add, invalidation, and removal. Its cost scales linearly with the number of ancestors inspected.

## Post-baseline: correction 5

Correction 5 makes `FindEdgeByMetaVertex` require referential meta identity while retaining indexed, inheritance-aware candidate selection. The original implementation returned a wrong edge for equal-valued or inherited meta candidates, so its lower lookup time did not implement the required contract.

### Exact meta identity filtering — ShortRun

| Candidate count | Mean | Allocated |
|---:|---:|---:|
| 1 | 7.674 ns | 0 B |
| 100 | 88.040 ns | 0 B |
| 1000 | 1.161 us | 0 B |

### Accurate acceptance run — default job

| Candidate count | Mean | Error | Allocated |
|---:|---:|---:|---:|
| 1 | 6.293 ns | 0.200 ns | 0 B |
| 100 | 90.481 ns | 1.296 ns | 0 B |
| 1000 | 1.180 us | 0.023 us | 0 B |

The searched meta is deliberately the last candidate in every bucket, so this is the worst-case linear identity-filtering cost after an indexed lookup. The implementation adds no managed allocations and does not scan unrelated outgoing edges.

## Post-baseline: correction 6

Correction 6 fixes three previously incorrect workloads: inherited-type recognition, independent target cardinality, and legacy deep copy of shared or cyclic subgraphs. The old implementation did not satisfy these contracts, so the measurements below are acceptance and scaling data rather than valid faster/slower comparisons.

### Correct inherited-type lookup — ShortRun

| Inheritance depth | Mean | Allocated |
|---:|---:|---:|
| 1 | 10.90 ns | 0 B |
| 10 | 43.55 ns | 0 B |
| 100 | 423.33 ns | 0 B |

The lookup uses the warm inheritance query index and scans only its candidate bucket. The direct `QueryOutEdges` zero/one/many result avoids the former singleton wrapper allocation.

### Target-cardinality validation — ShortRun

| Existing incoming edges | Mean | Allocated |
|---:|---:|---:|
| 0 | 36.55 ns | 0 B |
| 100 | 381.72 ns | 40 B |
| 1000 | 3.448 us | 40 B |

The valid-path workload uses a finite `$MaxTargetCardinality` and scans physical `InEdgesRaw`; runtime therefore scales linearly with target fan-in.

### Accurate lookup acceptance — in-process adaptive job

The normal out-of-process default job was blocked by the application's long-lived process-exit handler, so the two lookup workloads were repeated with BenchmarkDotNet's in-process toolchain.

| Workload | Scale | Mean | Error | Median | Allocated |
|---|---:|---:|---:|---:|---:|
| Correct `IsInherited` | depth 1 | 19.45 ns | 1.630 ns | 18.06 ns | 0 B |
| Correct `IsInherited` | depth 10 | 202.86 ns | 4.627 ns | 201.07 ns | 40 B |
| Correct `IsInherited` | depth 100 | 2.122 us | 0.161 us | 1.982 us | 40 B |
| Target cardinality | 0 incoming | 42.60 ns | 2.002 ns | 41.30 ns | 0 B |
| Target cardinality | 100 incoming | 1.303 us | 0.030 us | 1.305 us | 40 B |
| Target cardinality | 1000 incoming | 13.557 us | 0.777 us | 12.761 us | 40 B |

The target-cardinality distribution at 1000 incoming edges was multimodal (`StdDev 2.243 us`), but both median and mean confirm linear fan-in scaling.

### Shared-subgraph deep-copy lifecycle — ShortRun

| Shared branches | Mean | Allocated |
|---:|---:|---:|
| 1 | 13.48 us | 4.02 KB |
| 10 | 70.27 us | 20.73 KB |
| 100 | 431.21 us | 188.88 KB |

Each operation includes destination creation, a complete map-based deep copy, and disposal. Every branch points to one copied shared vertex, cycles point back into the copy, and meta vertices inside the copied scope are remapped. Runtime and allocation scale approximately linearly with copied vertices and edges.

## Post-baseline: correction 7

Correction 7 replaces the two unbounded, unsynchronized parsed-query dictionaries with independently configurable bounded LRU caches. Cache ownership and per-execution leases are represented by separate external references, so an eviction or reset cannot dispose an AST while another thread is executing it. Parse failures are not cached.

### Controlled bootstrap diagnostics

| Metric | Result |
|---|---:|
| Parser cache lookups | 100 |
| Hits | 32 |
| Misses / unique regular queries | 68 |
| Hit rate | 32% |
| Final regular cache occupancy | 68 / 512 |
| Final `metaMode` cache occupancy | 0 / 128 |
| Maximum combined occupancy | 68 |

The controlled application bootstrap uses 13.3% of the default regular-cache capacity. This is a lifecycle and bounded-memory acceptance measurement; the Stage 0 diagnostics did not execute parsed string queries and therefore recorded zero cache lookups.

### Bounded LRU mechanics — ShortRun

| Workload | Capacity | Mean | Gen0 / 1000 ops | Gen1 / 1000 ops | Allocated |
|---|---:|---:|---:|---:|---:|
| Warm hit and LRU promotion | 16 | 45.01 ns | 0 | 0 | 0 B |
| Miss and eviction | 16 | 100.33 ns | 0.0085 | 0 | 80 B |
| Warm hit and LRU promotion | 128 | 44.09 ns | 0 | 0 | 0 B |
| Miss and eviction | 128 | 111.99 ns | 0.0085 | 0.0002 | 80 B |
| Warm hit and LRU promotion | 512 | 44.37 ns | 0 | 0 | 0 B |
| Miss and eviction | 512 | 128.15 ns | 0.0085 | 0.0010 | 80 B |

The synchronized warm path is effectively independent of cache size and allocation-free. The miss benchmark includes dictionary insertion, linked-list insertion, one LRU eviction, and balanced external-reference updates. It intentionally excludes formal-text parsing so cache lifecycle overhead remains isolated.

## Post-baseline: correction 8

Correction 8 removes TempStore registry retention from production ZeroCode stacks. Ephemeral stacks retain their store, unique identifier, edge identity, aliasing, frame inheritance, and ability to escape an operator, but are not addressable through `VertexIdentifiersDictionary`. Direct public construction remains registered for compatibility; no pooling or automatic disposal was introduced.

### TempStore lifecycle diagnostics

| Metric | Registered baseline | Ephemeral production stacks |
|---|---:|---:|
| Explicitly created stacks | 10,000 | 10,000 |
| Additional active frames | 10 | 10 |
| Total stacks observed by counters | 10,011 | 10,011 |
| Reachable active stack chain | 11 | 11 |
| TempStore registry before | 1,662 | 887 |
| TempStore registry after | 11,673 | 887 |
| TempStore registry growth | 10,011 | 0 |

The previous implementation retained exactly one dictionary entry per created stack. The corrected workload retains none while preserving the complete active frame chain. The controlled bootstrap itself also leaves 775 fewer TempStore dictionary entries (`1,662` versus `887`).

### Stack workload — ShortRun

| Workload | Stage 0 baseline | Current mean | Mean change | Allocated |
|---|---:|---:|---:|---:|
| Local stack query | 11.44 ns | 4.869 ns | -57.4% | 0 B |
| Parent-frame query | 39.49 ns | 21.870 ns | -44.6% | 0 B |
| Create stack and copy 100 edges | 3.653 us | 2.429 us | -33.5% | 13,944 B |

These are cumulative comparisons after corrections 2–8, not isolated gains attributable only to ephemeral registration.

### Registration overhead isolation — default job

| Workload | Mean | Error | Allocated |
|---|---:|---:|---:|
| Registered stack, copy 100 edges, unregister | 2.382 us | 0.0477 us | 13.62 KB |
| Ephemeral stack, copy 100 edges | 2.393 us | 0.0479 us | 13.62 KB |

The ephemeral path is `0.46%` slower by mean, well inside overlapping confidence intervals, with identical allocation and GC rates. Correction 8 is therefore a bounded-memory/lifecycle fix with no measurable construction-time regression.

## Post-baseline: correction 9

Correction 9 separates the full rollback journal from the final listener change-set. Index invalidation remains immediate. Repeated value changes are represented by the first old value and final new value, while add followed by remove of the same `IEdge` reference produces no listener event. Rollback consumes the complete journal in reverse order.

### Transaction coalescing diagnostics

| Metric | Result |
|---|---:|
| Repeated value mutations | 10 |
| Rollback journal atoms | 10 |
| Mutations merged into an existing listener entry | 9 |
| Final listener change-set entries | 1 |
| Value after rollback | `Before` |

The listener view is reduced by 90% without deleting any rollback information. `NonTransactedEvent` remains immediate in the root transaction and is not folded into this change-set.

### Transaction watcher comparison

The post-correction run uses the same in-process configuration as the Stage 0 baseline: 256 invocations, one warmup, and three measurement iterations.

| Workload | Baseline mean | Current mean | Mean change | Baseline allocated | Current allocated | Allocation change |
|---|---:|---:|---:|---:|---:|---:|
| Commit one value change | 598.9 us | 419.4 us | -30.0% | 107.76 KB | 110.62 KB | +2.7% |
| Commit ten value changes | 1.020 ms | 391.9 us | -61.6% | 169.37 KB | 119.96 KB | -29.2% |

| Workload | Baseline Gen0 / 1000 ops | Current Gen0 / 1000 ops | Baseline Gen1 / 1000 ops | Current Gen1 / 1000 ops | Listener events |
|---|---:|---:|---:|---:|---:|
| One value change | 11.7188 | 11.7188 | 3.9063 | 0 | 1 -> 1 |
| Ten value changes | 15.6250 | 11.7188 | 7.8125 | 0 | 10 -> 1 |

The three-iteration confidence intervals remain wide (`419.4 us ± 311.0 us` and `391.9 us ± 656.9 us` at 99.9%), so the relative ordering of the two current means is not significant. The material result is that ten mutations now pay for one listener call, eliminate measured Gen1 collections, and allocate only 9.34 KB more than the single-mutation case instead of 61.61 KB more at baseline.

## Post-baseline: correction 11

Correction 11 makes store identifier collisions explicit, protects every `MemoryStore` root, and keeps filesystem paths, vertex identifiers, registries, query indexes, event payloads, and transactional rollback consistent during file rename. File-backed content is explicitly read-only; a normal `FileContentVertex` now uses the standard `EasyVertex` value lifecycle.

### Store and content acceptance benchmarks

The focused in-process jobs use two warmups and ten measurement iterations. The identifier and content jobs use 262,144 invocations; idempotent re-registration performs 16 operations per invocation. These workloads had no semantically equivalent Stage 0 measurements.

| Workload | Mean | Error | StdDev | Gen0 / 1000 ops | Allocated |
|---|---:|---:|---:|---:|---:|
| Register and remove one unique vertex | 183.66 ns | 23.478 ns | 15.529 ns | 0.0610 | 584 B |
| Re-register the same vertex | 22.10 ns | 0.888 ns | 0.528 ns | 0 | 0 B |
| Change normal `FileContentVertex` and query warm source | 136.5 ns | 7.99 ns | 4.75 ns | 0.0610 | 592 B |

The collision check is part of the normal registration path. Re-registering the same reference remains allocation-free and inexpensive. The content workload includes immediate incoming-source invalidation, lazy rebuilding of the warm meta+value index, and the verifying lookup.

### End-to-end file rename

| Workload | Mean | Error | StdDev | Gen0 / 1000 ops | Gen1 / 1000 ops | Allocated |
|---|---:|---:|---:|---:|---:|---:|
| Physical rename, registry update, overlay refresh, transaction commit | 670.5 us | 188.0 us | 124.4 us | 15.6250 | 3.9063 | 155.66 KB |

This Windows filesystem workload alternates two names for the same file over 256 invocations per iteration. It includes physical I/O and therefore has materially higher variance than in-memory graph operations. Contract tests, rather than timing, are the acceptance gate: a successful rename preserves reference identity under the new key, a locked-file failure preserves the old state, and transaction rollback restores the physical path, identifier, registry, and warm query result.

## Full post-correction rebaseline

The complete post-correction `ShortRun` matrix finished successfully on 2026-07-16: 61 benchmark cases across 13 benchmark classes, with deep copy restricted to `ShortRun`. Transaction and filesystem workloads retained their controlled in-process jobs. The runtime solution builds with zero errors, 61 isolated graph tests and 32 bootstrapped integration tests pass, and no test is skipped.

Of the 35 workloads that have equivalent Stage 0 semantics, 34 are faster by mean. The sole slower mean is the depth-1 add/query/remove hierarchy case; its `ShortRun` error is more than five times the mean, while the depth-10 and depth-100 forms are both about 45% faster, so it is not treated as a demonstrated regression.

### Warm query lookup

All 15 warm lookup combinations remain allocation-free.

| Workload | Scale | Stage 0 mean | Rebaseline mean | Mean change |
|---|---|---:|---:|---:|
| Direct meta, many | 1 / 100 / 1000 edges | 9.194 / 8.791 / 8.653 ns | 4.788 / 4.821 / 4.464 ns | -47.9% / -45.2% / -48.4% |
| Direct meta+value singleton | 1 / 100 / 1000 edges | 30.302 / 31.283 / 39.779 ns | 18.146 / 19.135 / 21.079 ns | -40.1% / -38.8% / -47.0% |
| Direct meta miss | 1 / 100 / 1000 edges | 6.816 / 7.434 / 7.577 ns | 3.387 / 3.349 / 3.585 ns | -50.3% / -55.0% / -52.7% |
| Derived meta matched by base | 1 / 100 / 1000 edges | 9.448 / 8.332 / 10.255 ns | 4.079 / 4.222 / 4.309 ns | -56.8% / -49.3% / -58.0% |
| Inherited source edge | 1 / 100 / 1000 edges | 11.678 / 12.073 / 11.748 ns | 5.623 / 5.668 / 5.742 ns | -51.8% / -53.1% / -51.1% |

### Mutation and hierarchy

| Workload | Scale | Stage 0 mean | Rebaseline mean | Mean change | Allocated |
|---|---|---:|---:|---:|---:|
| Target value mutation + query | 1 / 100 / 1000 edges | 137.4 ns / 4.635 us / 56.882 us | 73.04 ns / 2.629 us / 30.620 us | -46.8% / -43.3% / -46.2% | 336 B / 4,080 B / 39,936 B |
| Warm inherited query | depth 1 / 10 / 100 | 11.847 / 9.314 / 9.221 ns | 8.668 / 3.896 / 4.057 ns | -26.8% / -58.2% / -56.0% | 0 B |
| Add root edge, query leaf, remove | depth 1 / 10 / 100 | 641.633 ns / 3.140 us / 27.232 us | 820.576 ns / 1.718 us / 14.949 us | +27.9% / -45.3% / -45.1% | 1,408 B / 5,560 B / 46,056 B |
| Add/remove parent edge | 1 / 100 / 1000 children | 274.2 ns / 10.347 us / 137.024 us | 184.5 ns / 5.515 us / 61.780 us | -32.7% / -46.7% / -54.9% | 648 B / 15,048 B / 146,600 B |
| Add/query children/remove | 1 / 100 / 1000 children | 510.5 ns / 37.196 us / 429.231 us | 422.1 ns / 23.473 us / 308.076 us | -17.3% / -36.9% / -28.2% | 1,304 B / 76,688 B / 762,640 B |

The mixed diagnostic workload records 104 logical-edge rebuilds, 104 query-meta rebuilds, 2 meta+value rebuilds, and 3,452 scanned edges. Two hierarchy invalidations traverse and invalidate exactly 200 children. This confirms that descendant invalidation, not warm lookup, remains the dominant hierarchy mutation cost.

### Stack, cache, corrected helpers, and persistence lifecycle

| Workload | Scale | Rebaseline mean | Allocated |
|---|---|---:|---:|
| Local / parent stack query | local / parent | 4.682 ns / 24.829 ns | 0 B / 0 B |
| Registered / ephemeral stack copy | 100 edges | 2.620 us / 2.578 us | 13,944 B / 13,944 B |
| Exact meta identity, last candidate | 1 / 100 / 1000 | 6.856 ns / 93.395 ns / 1.259 us | 0 B |
| Correct `IsInherited` | depth 1 / 10 / 100 | 6.038 ns / 32.012 ns / 262.914 ns | 0 B |
| Target-cardinality validation | 0 / 100 / 1000 incoming | 21.69 ns / 219.57 ns / 2.282 us | 0 B / 40 B / 40 B |
| Shared-subgraph deep copy | 1 / 10 / 100 branches | 6.903 us / 37.974 us / 339.681 us | 4.04 KB / 20.81 KB / 189.67 KB |
| Parsed-query cache hit | capacity 16 / 128 / 512 | 32.94 / 39.21 / 31.80 ns | 0 B |
| Parsed-query miss + eviction | capacity 16 / 128 / 512 | 75.69 / 81.91 / 83.55 ns | 80 B |

The 10,011-stack lifecycle diagnostic leaves TempStore at 886 entries before and after, for zero growth. Controlled bootstrap performs 100 parsed-query-cache lookups: 32 hits, 68 misses, 32% hit rate, 68/512 regular occupancy, and 0/128 meta-mode occupancy.

The initial full-rebaseline cross-store detach+attach run measured 362.2 ns, 35.095 us, and 560.947 us for 1, 100, and 1000 edges, with 504 B, 44,066 B, and 440,080 B allocated. A focused default run produced 193.5 ns, 18.922 us, and 497.937 us. The disagreement between jobs led to the phase-level follow-up recorded below rather than treating the isolated default mean as proof of an algorithmic regression.

## Selected optimization: cached GraphChange watcher definitions

The rebaseline identified transaction event preparation as the clearest remaining measured hotspot. Before optimization, a commit with no benchmark-added listener still cost 111.4 us and 91.01 KB for one change because the 12 bootstrapped trigger definitions were reparsed through ZeroCode on every commit. Direct diagnostics confirmed that `GetWatchedVertexDictionary()` alone cost 189.668 us and 89,784 B per call.

`GraphChangeTriggerWatcher` now:

- caches parsed trigger definitions while the trigger set and trigger-vertex edge snapshot remain unchanged,
- compares trigger/source/edge/meta/target references and relevant values to detect dynamic definition edits,
- still executes scope queries on every call, so graph-dependent scope membership remains current,
- deduplicates each `(watched vertex, watcher entry)` pair,
- serializes trigger-list, cache-rebuild, and dictionary-preparation operations with one synchronization root.

The new integration contract warms the cache, adds duplicate scope definitions dynamically, verifies that a child mutation produces exactly one event, removes the listener/trigger, and verifies that subsequent changes produce no callback.

### Direct preparation diagnostic

| Metric | Before | Cached | Change |
|---|---:|---:|---:|
| Triggers / watched vertices / mappings | 12 / 12 / 12 | 12 / 12 / 12 | unchanged |
| Mean preparation time | 189.668 us | 12.342 us | -93.5% |
| Allocated per preparation | 89,784 B | 2,088 B | -97.7% |

### Transaction commit impact

| Workload | Rebaseline before cache | Cached definitions | Change | Before allocated | Cached allocated | Allocation change |
|---|---:|---:|---:|---:|---:|---:|
| One value change with listener | 185.5 us | 52.28 us | -71.8% | 110.62 KB | 17.22 KB | -84.4% |
| Ten value changes with listener | 250.6 us | 42.95 us | -82.9% | 119.96 KB | 18.81 KB | -84.3% |
| One value change, no added listener | 111.4 us | 11.86 us | -89.4% | 91.01 KB | 5.37 KB | -94.1% |
| Ten value changes, no added listener | 131.3 us | 16.44 us | -87.5% | 92.16 KB | 6.52 KB | -92.9% |

Against Stage 0, listener commit time is lower by 91.3% for one change and 95.8% for ten changes. Allocations are lower by 84.0% and 88.9%. The in-process time confidence interval for the one-change listener case remains wide, but allocation reductions and the direct 100-iteration preparation diagnostic independently confirm the removed work.

## Cross-store acceptance follow-up: allocation-free store lookup

Phase diagnostics showed linear scaling rather than a new superlinear regression. At 1000 edges, detach accounted for 558.328 us and 64,071 B, while attach accounted for 700.985 us and 376,010 B in the controlled timestamp diagnostic. The attach allocation was exactly 376 B per edge at every tested scale.

`EasyEdge.Attach()` resolves target and meta stores for every edge. `MinusZero.GetStore()` previously implemented each lookup as `Where(...).FirstOrDefault()` with a captured predicate. Two such lookups dominated attach allocations. The implementation now scans the existing `List<IStore>` by index and preserves the same fallback that creates a missing store.

### Store lookup isolation — default job

| Store count | Legacy LINQ mean | Indexed loop mean | Mean change | Legacy allocated | Loop allocated |
|---:|---:|---:|---:|---:|---:|
| 2 | 57.49 ns | 27.89 ns | -51.5% | 136 B | 0 B |
| 32 | 500.06 ns | 504.57 ns | +0.9% | 136 B | 0 B |

At 32 stores the means are equivalent within their dispersion, while the loop remains allocation-free. The cross-store workload has two stores and therefore uses the 51.5% faster case.

### Attach phase after lookup change

| Edges | Before mean | Current mean | Mean change | Before allocated | Current allocated | Allocation change |
|---:|---:|---:|---:|---:|---:|---:|
| 1 | 1.193 us | 0.932 us | -21.9% | 376 B | 40 B | -89.4% |
| 100 | 80.513 us | 61.828 us | -23.2% | 37,601 B | 4,000 B | -89.4% |
| 1000 | 700.985 us | 499.124 us | -28.8% | 376,010 B | 40,000 B | -89.4% |

### End-to-end detach+attach — comparable ShortRun

| Edges | Correction 3 | Full rebaseline before lookup change | Current | Change vs correction 3 | Current allocated |
|---:|---:|---:|---:|---:|---:|
| 1 | 337.6 ns | 362.2 ns | 231.4 ns | -31.5% | 168 B |
| 100 | 31.368 us | 35.095 us | 22.713 us | -27.6% | 10,464 B |
| 1000 | 490.358 us | 560.947 us | 444.666 us | -9.3% | 104,068 B |

Relative to the immediate full rebaseline, current means are lower by 36.1%, 35.3%, and 20.7%. End-to-end allocation falls by 66.7%, 76.3%, and 76.4%. The 1000-edge scaling watch item is therefore closed: the comparable job is faster than both the correction-3 acceptance state and the immediate pre-change rebaseline, with unchanged edge identity and reverse-link contracts.

## Selected optimization: shared logical/index rebuild

The first inherited index query after a parent mutation previously rebuilt the
logical `OutEdges` snapshot and then scanned that snapshot again for the
requested index. `EasyVertex` now builds the logical snapshot and exactly one
requested direct-meta, query-meta, value, or query-meta+value index during the
same inherited-edge traversal. Other dirty indexes remain lazy. Parent order,
`OutEdgesRaw` order, duplicate edge references, diamond deduplication, and
`$NoInherit` filtering are unchanged.

The diagnostic contract with three logical edges records both completed
structures but reduces `RebuildScannedEdges` from 6 to 3 for the first query
(-50%). Building all four indexes remains lazy and records 12 scanned edges
instead of the former 15 (-20%).

### First inherited query after parent mutation — bounded DefaultJob

The matrix uses `IterationTime=100ms` to avoid the default adaptive job selecting
more than two million mutation/query operations per iteration for the smallest
case. The query-meta row includes the final focused rerun after adding its
four-edge fast path.

| Requested index | Edges | Before | Current | Mean change |
|---|---:|---:|---:|---:|
| Direct meta | 1 / 100 / 1000 | 582.5 ns / 7.671 us / 48.559 us | 642.8 ns / 4.217 us / 36.090 us | +10.4% / -45.0% / -25.7% |
| Query meta | 1 / 100 / 1000 | 557.8 ns / 5.894 us / 53.836 us | 395.6 ns / 3.151 us / 27.457 us | -29.1% / -46.5% / -49.0% |
| Value | 1 / 100 / 1000 | 562.8 ns / 4.712 us / 55.535 us | 441.4 ns / 2.724 us / 29.247 us | -21.6% / -42.2% / -47.3% |
| Query meta+value | 1 / 100 / 1000 | 690.6 ns / 4.641 us / 72.701 us | 481.0 ns / 4.455 us / 44.397 us | -30.4% / -4.0% / -38.9% |
| All indexes, lazy sequence | 1 / 100 / 1000 | 1.557 us / 10.954 us / 199.466 us | 826.3 ns / 11.512 us / 117.470 us | -46.9% / +5.1% / -41.1% |

All 15 allocation results improve:

| Requested index | Before allocated, 1 / 100 / 1000 | Current allocated, 1 / 100 / 1000 |
|---|---:|---:|
| Direct meta | 1.29 / 8.33 / 63.70 KB | 1.21 / 6.97 / 55.30 KB |
| Query meta | 1.29 / 8.33 / 63.70 KB | 1.21 / 6.97 / 55.30 KB |
| Value | 1.29 / 6.19 / 47.49 KB | 1.21 / 4.83 / 39.09 KB |
| Query meta+value | 1.31 / 7.02 / 56.11 KB | 1.23 / 5.66 / 47.71 KB |
| All indexes, lazy sequence | 2.06 / 20.59 / 179.52 KB | 1.98 / 19.23 / 171.12 KB |

The initial before/after runs showed inconsistent mid-size means under changing
machine load. A same-session legacy/shared check therefore repeated the two
affected paths. At 100 edges, query meta+value improved from 9.642 us to
6.774 us (-29.7%) and the all-index sequence improved from 18.609 us to
16.813 us (-9.7%); allocation fell from 7.02/20.59 KB to 5.66/19.23 KB.
For the one-edge direct-meta path, the paired means were 458.0 ns legacy and
447.4 ns shared, so the +10.4% result in the original full matrix was not
reproduced.

### Existing hierarchy acceptance — ShortRun

- All 15 warm lookup variants remain allocation-free.
- Warm inherited query is 4.339 / 3.943 / 3.856 ns at depth 1/10/100.
- Add-root/query-leaf/remove is 418.4 ns / 1.778 us / 14.789 us and allocates
  1,328 / 5,312 / 44,664 B.
- Wide add/remove is 184.7 ns / 5.339 us / 55.269 us for 1/100/1000 children.
- Wide add/query-all/remove is 350.3 ns / 19.831 us / 276.570 us and allocates
  1,272 / 73,488 / 730,640 B.

The final verification has 65 isolated and 33 integration tests passing with no
skips. `run.sln` builds with zero errors. Raw before, final matrix, focused
same-session checks, and acceptance reports are under
`BenchmarkDotNet.Artifacts/shared-rebuild-*`.

## Selected optimization: generation-stamped inheritance invalidation

Outgoing structure, target-value, direct-meta, and query-meta changes no longer
walk every source descendant at mutation time. A changed source increments a
granular local generation only when it has inheritance children. Each inherited
cache captures the corresponding versions of its transitive parents when it is
built. A global epoch is used only as an allocation-free fast filter; after the
epoch changes, the child compares its precise parent stamps and rebuilds before
lookup only when a dependency changed. Unrelated hierarchies therefore do not
rebuild each other, and read-your-writes remains immediate.

The post-change mixed diagnostic, with 100 wide children warmed before the
measured mutation, changes hierarchy invalidation from 2 traversals / 200
traversed / 200 invalidated descendants to zero for all three counters. It
records 2 generation bumps, 100 precise checks, 100 checked dependencies, and
100 stale checks. Logical/query-meta rebuilds are 103/103 and the scan total is
3,226 edges.

### Mutation-only scaling

| Workload | Scale | Before | Current | Mean change | Before allocated | Current allocated |
|---|---:|---:|---:|---:|---:|---:|
| Wide parent add+remove | 1 child | 184.7 ns | 147.1 ns | -20.4% | 648 B | 216 B |
| Wide parent add+remove | 100 children | 5.339 us | 143.1 ns | -97.3%, 37.3x | 15,048 B | 216 B |
| Wide parent add+remove | 1000 children | 55.269 us | 144.6 ns | -99.74%, 382x | 146,600 B | 216 B |
| Deep root add+remove, no query | depth 1 | 176.0 ns | 94.32 ns | -46.4% | 648 B | 216 B |
| Deep root add+remove, no query | depth 10 | 706.1 ns | 97.37 ns | -86.2%, 7.3x | 2,536 B | 216 B |
| Deep root add+remove, no query | depth 100 | 8.080 us | 94.13 ns | -98.84%, 85.8x | 22,968 B | 216 B |
| Mutate tree A, query independent tree B | 1 child each | 176.1 ns | 109.1 ns | -38.0% | 648 B | 216 B |
| Mutate tree A, query independent tree B | 100 children each | 4.772 us | 104.9 ns | -97.8%, 45.5x | 15,048 B | 216 B |
| Mutate tree A, query independent tree B | 1000 children each | 66.793 us | 105.6 ns | -99.84%, 632x | 146,600 B | 216 B |

The deep and independent-hierarchy rows are paired `ShortRun` measurements from
the generation baseline/final artifacts. The wide before values are the final
shared-rebuild `ShortRun`; the current short and default jobs both measure about
145 ns at 1000 children.

### Value mutation and query-time trade-off

| Workload | Scale | Before | Current | Mean change | Before allocated | Current allocated |
|---|---:|---:|---:|---:|---:|---:|
| Inherited target value + query | depth 1 | 139.9 ns | 114.3 ns | -18.3% | 552 B | 400 B |
| Inherited target value + query | depth 10 | 694.9 ns | 414.5 ns | -40.4% | 1,784 B | 1,072 B |
| Inherited target value + query | depth 100 | 7.321 us | 3.504 us | -52.1% | 15,456 B | 8,128 B |
| Warm inherited query | depth 1 | 4.339 ns | 8.310 ns | +91.5%, +3.97 ns | 0 B | 0 B |
| Warm inherited query | depth 10 | 3.943 ns | 8.100 ns | +105.4%, +4.16 ns | 0 B | 0 B |
| Warm inherited query | depth 100 | 3.856 ns | 8.557 ns | +121.9%, +4.70 ns | 0 B | 0 B |

The warm percentage is large because the original lookup was approximately
4 ns; the absolute cost of the epoch comparison is 4.0-4.7 ns and remains
allocation-free. A stable default job for add/query-all/remove measured
390.0 ns, 24.705 us, and 336.351 us at 1/100/1000 children, allocating
904 B, 65,056 B, and 648,256 B. Relative to the post-correction rebaseline
means, those times are -7.6%, +5.2%, and +9.2%, while allocation is lower by
30.7%, 15.2%, and 15.0%.

The optimization is retained because the profiled mutation-only hotspot becomes
constant-time and effectively allocation-constant, the query-all regression
stays below 10% with lower allocation, and target-value mutation/query improves
at every depth. Contracts cover alternating mutation/query, transaction
rollback, topology add/remove, meta inheritance, meta value, target value,
independent hierarchies, `$NoInherit`, `AllowInheritance=false`, diamond order,
and edge identity. Final verification has 72 isolated and 34 integration tests
passing with no skips; `run.sln` builds with zero errors. Raw runs are under
`BenchmarkDotNet.Artifacts/generation-*`.

## Selected optimization: incremental local outgoing indexes

A local `OutEdgesRaw` add/remove no longer makes every warm outgoing index
dirty. `OutList` now routes the mutation through one `EasyVertex` handler that
patches only indexes which have already been built. Query-meta aliases are
computed once, direct-meta remains a non-inherited view, and remove matches the
exact edge reference while preserving bucket order. Buckets transition
single-to-list on add and list-to-single or empty on remove.

The fast path is deliberately narrow. Source inheritance, vertices with
inheritance children, `$Inherits`, detached loader reconstruction, filesystem
overlays, stack-specific adds, and inconsistent buckets retain the full-rebuild
fallback. A vertex begins with a one-mutation budget: a second write before a
read abandons incremental state and makes all indexes lazy-dirty. If a query
then follows that burst, the vertex learns twice the observed burst size, with
a minimum of 8 and a cap of 256. Repeated query-heavy bursts therefore remain
incremental, while never-queried write-only vertices still stop maintaining
unused dictionaries after the second write.

The controlled 1000-edge diagnostic performs warm add/query/remove/query across
direct-meta, query-meta, value, and meta+value. It records one incremental add,
one incremental remove, zero fallbacks, zero rebuilds, and zero scanned edges.
It also records two key removals for the temporary value and meta+value buckets.

### Local mutation/query — bounded DefaultJob

| Workload | Edges | Before | Current | Mean change | Before allocated | Current allocated |
|---|---:|---:|---:|---:|---:|---:|
| Add/query meta/remove/query | 1 | 487.7 ns | 1.159 us | +137.6% | 816 B | 392 B |
| Add/query meta/remove/query | 100 | 7.333 us | 1.188 us | -83.8%, 6.2x | 10,936 B | 216 B |
| Add/query meta/remove/query | 1000 | 60.448 us | 7.363 us | -87.8%, 8.2x | 95,528 B | 216 B |
| Add/query all indexes/remove/query all | 1 | 1.307 us | 1.407 us | +7.7% | 2,488 B | 392 B |
| Add/query all indexes/remove/query all | 100 | 29.309 us | 1.290 us | -95.6%, 22.7x | 36,040 B | 216 B |
| Add/query all indexes/remove/query all | 1000 | 290.921 us | 6.584 us | -97.7%, 44.2x | 332,712 B | 216 B |

The one-edge cases expose the fixed cost of updating several already-warm hash
indexes and promoting singleton buckets. They are retained as an explicit
trade-off because allocation still falls by 52.0% to 84.2%, while the profiled
100/1000-edge rebuild hotspot improves by 6.2x to 44.2x. The final query run
was collected after sustained benchmark load and has 15-30% relative standard
deviation; the large-cardinality speedups and allocation reductions remain well
outside that dispersion.

### Adaptive write-only guard — bounded DefaultJob

| Edges | Before | Current | Mean change | Before allocated | Current allocated |
|---:|---:|---:|---:|---:|---:|
| 1 | 131.6 ns | 144.0 ns | +9.4% | 216 B | 216 B |
| 100 | 463.7 ns | 441.1 ns | -4.9% | 216 B | 216 B |
| 1000 | 4.342 us | 4.177 us | -3.8% | 216 B | 216 B |

The adaptive second-mutation fallback removes the initial repeated-write
regression: allocation is identical to the old lazy invalidation path and
100/1000-edge means are slightly lower. The one-edge difference is 12.4 ns and
small relative to run dispersion.

### Incremental versus rebuild burst matrix — InProcess, 10 iterations

The controlled matrix creates a fresh warmed vertex outside each measured
iteration. The query workload adds the complete burst and then queries either
query-meta or all four outgoing indexes. The write-only workload performs the
same additions without a query. These measurements compare forced
`rebuild-only`, the original fixed one-write adaptive policy, and forced
`always-incremental`.

| 1000-edge burst | Query all: rebuild | Query all: fixed-1 | Query all: incremental | No query: rebuild | No query: fixed-1 | No query: incremental |
|---:|---:|---:|---:|---:|---:|---:|
| 1 | 287.920 us | 17.760 us | 37.033 us | 9.811 us | 12.294 us | 12.780 us |
| 2 | 445.506 us | 394.611 us | 24.510 us | 10.490 us | 13.833 us | 15.840 us |
| 5 | 295.844 us | 284.961 us | 21.350 us | 9.860 us | 14.870 us | 25.000 us |
| 10 | 292.430 us | 387.320 us | 35.561 us | 10.850 us | 16.990 us | 24.060 us |
| 50 | 463.290 us | 486.140 us | 63.270 us | 18.663 us | 21.650 us | 75.889 us |
| 100 | 358.711 us | 452.220 us | 111.180 us | 21.750 us | 43.683 us | 56.500 us |

With a query after the burst, forced incremental wins every 1000-edge case
from 2 through 100 writes; at 100 writes it is 3.23x faster than rebuild-only
and 4.07x faster than the fixed-one policy. Without a query, rebuild-only wins
every case; at 50 writes forced incremental is 4.07x slower, and at 100 writes
it is 2.60x slower. The same direction holds at 100 existing edges: forced
incremental wins every measured query burst, while rebuild-only wins every
write-only burst. This proves that one static threshold cannot be optimal for
both access patterns.

At 1000 edges and a 100-write query burst, allocation is 258,136 B for
rebuild-only/fixed-one and 91,888 B for forced incremental. The corresponding
write-only allocations are 58,736 B, 54,704 B, and 91,888 B. At 50 writes,
query allocation is 247,336 B versus 79,120 B for forced incremental, while
write-only is 45,632 B versus 81,088 B.

### Learned repeated bursts — InProcess, 10 iterations

The production policy now learns only after observing that a fallback burst is
followed by a query. The first `5/query` phase raises the budget from 1 to 10;
the next two five-write phases patch warm indexes without a fallback. A
`50/query` phase raises the budget to 100, so the following 100 and 50 writes
remain incremental. The cap of 256 bounds the cost if a previously query-heavy
vertex later changes to write-only.

| Existing edges | Pattern | Query view | Rebuild-only | Learned adaptive | Always incremental |
|---:|---|---|---:|---:|---:|
| 100 | 5 / 5 / 5 | meta | 199.63 us | 72.07 us | 15.59 us |
| 100 | 5 / 5 / 5 | all indexes | 432.01 us | 188.97 us | 44.55 us |
| 100 | 50 / 100 / 50 | meta | 73.22 us | 72.93 us | 52.04 us |
| 100 | 50 / 100 / 50 | all indexes | 452.40 us | 148.60 us | 126.62 us |
| 1000 | 5 / 5 / 5 | meta | 150.70 us | 92.90 us | 12.44 us |
| 1000 | 5 / 5 / 5 | all indexes | 1,215.94 us | 320.34 us | 26.40 us |
| 1000 | 50 / 100 / 50 | meta | 301.57 us | 120.96 us | 60.91 us |
| 1000 | 50 / 100 / 50 | all indexes | 1,195.69 us | 453.99 us | 176.72 us |

For all-index queries, learned adaptive is 2.29x/3.04x faster than rebuild-only
at 100 edges and 3.80x/2.63x faster at 1000 edges for the small/large patterns.
At 1000 edges allocation falls from 495.98 KB to 171.27 KB for `5/5/5` and
from 715.70 KB to 420.32 KB for `50/100/50`. It intentionally remains slower
than forced incremental because the first observed burst pays one rebuild; that
cost is the guard which preserves cheap behavior for vertices that never query.
A final `ShortRun` hot-path guard after enabling learning measures meta
add/query/remove/query at 1.023 us / 4.335 us and all-index at
1.139 us / 5.773 us for 100/1000 edges, with 216 B in all four cases. The
write-only guard also remains at 216 B.

Contracts cover zero/one/many buckets, first/middle/last removal, exact edge
identity and order, inherited meta query keys versus direct-meta, 200 randomized
local add/remove operations checked against raw-edge results, inherited-source
fallback, stack fallback, transaction rollback, fixed-budget boundaries,
adaptive budget learning, and post-load incremental updates after both JSON
and Binary roundtrips. Final verification has 80
isolated and 35 integration tests passing with no skips; `run.sln` builds with
zero errors. Raw measurements are under
`BenchmarkDotNet.Artifacts/incremental-*`, including
`incremental-burst-strategies-2026-07-17`,
`incremental-write-only-bursts-2026-07-17`, and
`incremental-repeated-bursts-2026-07-17`; the counter report is
`incremental-index-diagnostics.json`.

## Selected optimization: batch original-edge adds for ZeroCode stacks

`NoInEdgeInOutVertexVertex.AddRangeOriginalEdges` appends original `IEdge`
instances directly to the stack out-list. It preserves exact references,
duplicates, and order, does not create reverse `InEdgesRaw` or
`MetaInEdgesRaw`, and invalidates the stack indexes once after the complete
batch. If source enumeration throws after a partial add, the `finally` path
still invalidates the indexes, so the next query sees every element that was
successfully appended.

The centralized `InstructionHelpers.AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES`
now uses the batch. This covers the many-result paths in query, colon, and
`ZeroCodeExecuter.GetAll`. The simple result-copy loops in slash, execute, and
method-call operators also use the new API. Single-edge, filtered, transformed,
and mutable-scope loops remain on the old operation because they either cannot
batch or perform required work between additions. Wrap-copy remains separate
and `Create_INoInEdgeInOutVertexVertex_FromEdgesList` still returns an existing
stack by zero-copy alias.

### Original-edge construction — paired bounded DefaultJob

| Workload | Edges | Per-edge | Batch | Mean change | Per-edge allocated | Batch allocated |
|---|---:|---:|---:|---:|---:|---:|
| Add original references | 1 | 167.2 ns | 178.7 ns | +6.9%, +11.5 ns | 760 B | 760 B |
| Add original references | 10 | 345.1 ns | 233.2 ns | -32.4%, 1.48x | 1,000 B | 808 B |
| Add original references | 1000 | 9.749 us | 783.4 ns | -92.0%, 12.44x | 17,272 B | 8,728 B |
| Add originals then query | 1 | 254.1 ns | 258.6 ns | +1.8%, +4.5 ns | 1,016 B | 1,016 B |
| Add originals then query | 10 | 546.3 ns | 427.6 ns | -21.7%, 1.28x | 1,808 B | 1,616 B |
| Add originals then query | 1000 | 55.523 us | 36.666 us | -34.0%, 1.51x | 64,928 B | 56,384 B |

The separate pre-change run measured per-edge add at 99.61 ns / 190.10 ns /
7.804 us and add+query at 153.25 ns / 443.31 ns / 25.884 us for 1/10/1000
edges. Machine load shifted before the final run, so acceptance uses the paired
old/new methods in the same process rather than comparing absolute means across
sessions. At one edge the batch has no allocation benefit and adds only
4.5-11.5 ns. At 10 and 1000 edges both time and allocation improve; the large
pure-add result substantially exceeds the original 1.2-2x hypothesis.

The controlled 1000-edge warm-stack diagnostic records one original-edge
batch containing 1000 edges, zero incremental-index operations and fallbacks,
one logical rebuild, one query-meta rebuild, and exactly 1000 scanned edges.
The query returns all 1000 original references. Existing stack lookup remains
allocation-free; a post-change guard measures 10.55 ns local and 41.93 ns
parent-frame lookup. Registered/ephemeral wrap-copy remains a separate path at
2.805/2.633 us and 7,664 B.

Contracts cover identity, order, duplicates, no reverse wiring, query after a
warm batch, empty batches, partial enumeration failure, and the migrated
multi-edge helper. Full verification has 84 isolated and 35 integration tests
passing with no skips; `run.sln` builds with zero errors. Raw reports are under
`BenchmarkDotNet.Artifacts/stack-original-batch-before-2026-07-17`,
`stack-original-batch-after-2026-07-17`, and
`stack-batch-regression-2026-07-17`; counters are in
`stack-batch-diagnostics.json`.

## Selected optimization: specialized ZeroCode stack storage

`NoInEdgeInOutVertexVertex` keeps its `EasyVertex`/`IVertex` surface and all
existing outgoing-index behavior, but its base constructor now selects a lean
storage mode. Stack instances retain one normal out-list while incoming,
meta-incoming, and ordinary-inheritance lists are created only on first use.
This preserves the mutable `IList` contract when a stack is the target or meta
of an ordinary edge. Construction also initializes the fixed empty value
directly instead of running normal vertex value-change invalidation. Ordinary
vertices still receive their original eager lists. No pooling or changed stack
ownership was introduced.

### Stack infrastructure — immediate before/after DefaultJob

| Workload | Before | Specialized | Mean change | Before allocated | Specialized allocated | Allocation change |
|---|---:|---:|---:|---:|---:|---:|
| Create empty ephemeral stack | 157.766 ns | 88.216 ns | -44.1%, 1.79x | 704 B | 512 B | -27.3% |
| Create and read empty incoming lists | 154.020 ns | 125.250 ns | -18.7%, 1.23x | 704 B | 640 B | -9.1% |
| Create stack with parent-frame edge | 201.021 ns | 133.000 ns | -33.8%, 1.51x | 864 B | 672 B | -22.2% |

The empty-stack Gen0 rate falls from 0.0746 to 0.0544 collections per 1000
operations (-27.1%). Explicitly reading both incoming lists materializes their
private storage, which intentionally retains 640 B rather than the unsafe
512 B read-only-singleton result.
The 1.5x lower-bound construction-time hypothesis is met, but the original
50-80% allocation hypothesis is not: the inherited `EasyVertex` object and its
outgoing index fields remain. Replacing that base was rejected in this cycle
because the extra compatibility risk is not justified by the remaining 512 B.

Existing stack behavior remains allocation-free for warmed lookup. Relative to
the immediately preceding stack-batch guard, local lookup is 9.643 ns versus
10.55 ns (-8.6%) and parent-frame lookup is 38.667 ns versus 41.93 ns (-7.8%).
Registered+unregister and ephemeral 100-edge wrap-copy measure 2.534/2.434 us
and 7,472 B, versus 2.805/2.633 us and 7,664 B before specialization. The
original-edge batch matrix reduces every construction allocation by exactly
192 B; its paired 1000-edge pure-add result remains 14.5x faster than per-edge
addition.

Conformance was captured before changing storage and covers registration mode,
identity, ordering, duplicates, reverse-edge suppression, local shadowing,
first-parent selection, delete/query refresh, artificial edges, and zero-copy
aliasing. A bootstrapped parsed ZeroCode function executes `FunctionCall`,
nested frame creation/removal, and `Return` end to end. Dedicated contracts also
verify lazy mutable incoming, meta-incoming, and ordinary-inheritance
bookkeeping. Final verification has 92 isolated and 36 integration tests
passing with no skips. Raw reports are under
`BenchmarkDotNet.Artifacts/stack-infrastructure-before-2026-07-17`,
`stack-infrastructure-hardened-2026-07-17`,
`stack-operations-after-2026-07-17`, and
`stack-batch-after-specialization-2026-07-17`.

## Selected optimization: direct ZeroCode parent-frame cache

Each stack frame now caches the first parent selected by the existing
`$StackFrameInherits` query. The physical edge remains the source of truth:
normal, artificial, original-reference batch, and delete paths invalidate the
cache in `finally`, including partial-failure paths. The next local miss
re-resolves through the unchanged base query, preserving query-meta semantics
and first-parent ordering. A single object field stores either the parent or a
shared no-parent sentinel, so both positive and terminal misses are cached
without adding a separate initialized flag.

### Parent-chain lookup — immediate before/final DefaultJob

| Workload | Depth | Before | Cached | Mean change | Before allocated | Cached allocated |
|---|---:|---:|---:|---:|---:|---:|
| Resolve value from root frame | 1 | 23.74 ns | 14.78 ns | -37.8%, 1.61x | 0 B | 0 B |
| Resolve value from root frame | 10 | 182.69 ns | 89.20 ns | -51.2%, 2.05x | 0 B | 0 B |
| Resolve value from root frame | 100 | 3,376.06 ns | 861.62 ns | -74.5%, 3.92x | 0 B | 0 B |
| Miss across every frame | 1 | 29.05 ns | 15.06 ns | -48.2%, 1.93x | 0 B | 0 B |
| Miss across every frame | 10 | 196.36 ns | 91.45 ns | -53.4%, 2.15x | 0 B | 0 B |
| Miss across every frame | 100 | 3,726.42 ns | 863.93 ns | -76.8%, 4.31x | 0 B | 0 B |

The focused 1000-edge stack guard reports local lookup at 5.520 ns and parent
lookup at 15.266 ns, both allocation-free. The preceding specialized-storage
guard measured 9.643 ns and 38.667 ns, so no simple-local regression was
observed. The direct depth benchmark is the primary paired comparison because
it uses the same fixture immediately before and after this optimization.

The one-field cache adds exactly 8 B to each stack: empty/read-incoming/parent
construction now allocate 520/648/680 B versus 512/640/672 B before this step.
They remain 26.1%/8.0%/21.3% below the original pre-specialization
704/704/864 B. Construction means in the regression run were
57.020/73.047/81.617 ns; these cross-run timing improvements are treated only
as a no-regression signal, while the exact +8 B is the structural trade-off.

The counter diagnostic warms a depth-100 chain, performs 1000 resolved and
1000 missing queries, and records 201,000 parent-cache hits with zero misses.
Contracts cover depths 1/10/100, local shadowing, multiple-parent ordering,
first-parent removal, original-reference batch parents, cache hit/miss
counters, and full function-call integration. Final verification has 98
isolated and 36 integration tests passing with no skips; `run.sln` builds with
zero errors.

Raw reports are under
`BenchmarkDotNet.Artifacts/stack-parent-cache-before-2026-07-17`,
`stack-parent-cache-final-2026-07-17`, and
`stack-parent-cache-regression-2026-07-17`. Counter output is in
`stack-parent-cache-diagnostics.json`.

## Selected optimization: allocation-free query results

`GraphUtil.GetQueryOutResult` and `GetQueryInResult` now return a compact
`EdgeQueryResult` that represents zero, one, or many edges without creating a
list. It exposes `IReadOnlyList<IEdge>`, a concrete struct enumerator,
`Count`, indexed access, `FirstOrDefault`, and `IsEmpty`; it does not implement
`IList<IEdge>`, so callers cannot mutate an index bucket through this API. The
existing mutable-list wrappers remain unchanged for compatibility.

Filtered results retain exact edge references and order. A full scan wraps
logical `OutEdges` or physical `InEdgesRaw` directly instead of calling the
old `ToList` path. These are non-owning read-only views intended for immediate
consumption before the next graph mutation.

### Result construction and count — paired 100 ms job

| Direction | Cardinality | Legacy list | Read-only result | Mean change | Legacy allocated | Result allocated |
|---|---|---:|---:|---:|---:|---:|
| Out | Empty | 12.235 ns | 6.794 ns | -44.5% | 32 B | 0 B |
| Out | Single | 37.422 ns | 23.405 ns | -37.5% | 88 B | 0 B |
| Out | Many (1000) | 7.793 ns | 8.529 ns | +9.4%, +0.736 ns | 0 B | 0 B |
| Out | Full scan (1000) | 520.842 ns | 2.280 ns | -99.56%, 228.4x | 8,056 B | 0 B |
| In | Empty | 12.707 ns | 6.652 ns | -47.7% | 32 B | 0 B |
| In | Single | 37.814 ns | 22.659 ns | -40.1% | 88 B | 0 B |
| In | Many (1000) | 7.388 ns | 7.498 ns | +1.5%, +0.110 ns | 0 B | 0 B |
| In | Full scan (1000) | 527.317 ns | 3.386 ns | -99.36%, 155.7x | 8,056 B | 0 B |

### Enumeration — final concrete-enumerator job

| Direction | Cardinality | Legacy list | Read-only result | Mean change | Legacy allocated | Result allocated |
|---|---|---:|---:|---:|---:|---:|
| Out | Empty | 19.371 ns | 8.248 ns | -57.4% | 32 B | 0 B |
| Out | Single | 37.496 ns | 25.915 ns | -30.9% | 88 B | 0 B |
| Out | Many (1000) | 562.204 ns | 681.355 ns | +21.2% | 0 B | 0 B |
| Out | Full scan (1000) | 1,245.275 ns | 977.063 ns | -21.5% | 8,056 B | 0 B |
| In | Empty | 13.182 ns | 6.746 ns | -48.8% | 32 B | 0 B |
| In | Single | 35.791 ns | 23.300 ns | -34.9% | 88 B | 0 B |
| In | Many (1000) | 562.917 ns | 675.712 ns | +20.0% | 0 B | 0 B |
| In | Full scan (1000) | 1,176.368 ns | 945.854 ns | -19.6% | 8,056 B | 0 B |

The filtered-many list was already allocation-free and its native
`List<IEdge>` enumerator remains about 20% faster. For that reason the rollout
is intentionally selective: existing bulk filtered-many loops remain on
`GetQueryOut/GetQueryIn`. First, existence, count, empty/single, and full-scan
callers use direct query outputs or the read-only result. This keeps the
measured many-result regression out of production hot loops while removing
32/88/8,056 B from the paths that previously allocated.

Contracts cover out/in zero-one-many-full results, exact reference identity,
order, indexing, concrete enumeration, first/empty state, and the absence of a
mutable `IList` surface. Existing graph query contracts and the ZeroCode
function-call integration remain green. Final verification has 101 isolated
and 36 integration tests passing with no skips; `run.sln` builds with zero
errors.

Raw reports are under
`BenchmarkDotNet.Artifacts/query-result-before-2026-07-17`,
`query-result-after-2026-07-17`,
`query-result-final-2026-07-17`, and
`query-result-enumerator-final-2026-07-17`.

## Selected optimization: dedicated query-meta edge buckets

The branch/cast and value-key costs were measured before changing production
indexes. The existing query-meta dictionary stored each value as `object`,
containing either one `IEdge` or a `List_VertexBase`. A prototype struct bucket
reduced a 1000-entry lookup from 5.357 to 5.288 ns for a singleton and from
5.634 to 5.273 ns for a many-edge bucket. The gain is small for singleton
buckets but repeatable for many-edge buckets, which are typical for query-meta
indexes.

Production therefore uses the dedicated `EdgeBucket` only for the private
inheritance-aware query-meta index. It keeps zero/one/many state, exact
references, order, duplicate handling, promotion, collapse, and key removal.
Lookups use `CollectionsMarshal.GetValueRefOrNullRef`, avoiding both the old
runtime type test and a struct copy. Public direct-meta, value, and
meta+value dictionaries retain their existing types and memory density; this
avoids a public API change and an extra 8 bytes for every high-cardinality value
index entry.

### Query-meta lookup — paired default jobs

| Workload | Edges | Before | Dedicated bucket | Mean change | Allocation |
|---|---:|---:|---:|---:|---:|
| Direct meta hit | 1 | 4.744 ns | 4.398 ns | -7.3% | 0 B |
| Direct meta hit | 1000 | 4.998 ns | 5.141 ns | +2.9%, within combined variance | 0 B |
| Direct meta miss | 1 | 4.043 ns | 4.026 ns | -0.4% | 0 B |
| Direct meta miss | 100 | 3.905 ns | 4.078 ns | +4.4%, within combined variance | 0 B |
| Direct meta miss | 1000 | 3.926 ns | 4.045 ns | +3.0%, within combined variance | 0 B |
| Derived meta matched by base | 1 | 4.615 ns | 4.217 ns | -8.6% | 0 B |
| Inherited source edge | 1 | 5.968 ns | 5.232 ns | -12.3% | 0 B |
| Inherited source edge | 100 | 5.718 ns | 5.246 ns | -8.3% | 0 B |
| Inherited source edge | 1000 | 5.925 ns | 5.556 ns | -6.2% | 0 B |

The 100-edge direct-hit before-run produced a 13.680 ns outlier and is excluded
from the percentage claims. The final ref-based run measured 5.156 ns.
Incremental add/query/remove remains allocation-stable at 392 B for a one-edge
index and 216 B for 100/1000-edge indexes. Its focused `ShortRun` means are
462.6 ns, 664.3 ns, and 2.504 us.

### Value-key and hash findings

Current warmed value-only lookup is 7.4-8.0 ns for short strings and 271-281 ns
for 1024-character strings. A non-cached integer value costs 22.79 ns and 32 B
at value 999; decimal costs 34.46-36.69 ns and 32 B; a custom allocating
`ToString` costs 22.70-30.71 ns and 40-80 B. Meta+value adds a second key
component and reaches 700-938 ns for the long-string case.

A conditional-weak hash-cache prototype improved repeated 1024-character
string lookup from 266.872 to 54.994 ns for one key and from 258.897 to
63.269 ns for 1000 keys. It also doubled short-key lookup from about 5.0 to
10.2 ns and requires per-string cache state. It was rejected for production:
short strings dominate graph metadata, and accepting a systematic 2x
regression there would violate the optimization threshold. `MetaAndValueKey`
precomputed hash was also not adopted because a new search key still has to
hash its strings once, while storing the hash would enlarge every dictionary
key.

Contracts now cover int/string/custom `ToString` equivalence, decimal/string
equivalence, empty values, long values, duplicates, exact reference identity,
and order. Final verification has 104 isolated and 36 integration tests passing
with no skips. Raw reports are under
`BenchmarkDotNet.Artifacts/bucket-key-baseline-2026-07-17`,
`string-hash-prototype-2026-07-17`,
`edge-bucket-before-2026-07-17`,
`edge-bucket-after-2026-07-17`,
`edge-bucket-ref-after-2026-07-17`, and
`edge-bucket-mutation-2026-07-17`.

## Selected optimization: protected file-content value indexes

File-backed `FileContentVertex` values are no longer materialized while
building out/in value or meta+value indexes. An internal target capability
keeps graph core independent of filesystem types. Indexes record that an
explicit value evaluation may be needed, and only a value-bearing query takes
an ordered fallback scan. Meta filtering happens before value evaluation, so
an unrelated meta+value query scans cheaply without opening the file.

The public `Value` contract is unchanged: an explicit read still returns the
complete current file content. Value-only and matching Content queries still
read the file and preserve order, duplicates, exact edge references, incoming
queries, and inherited outgoing-meta matching. Because the fallback evaluates
the current value, an external file edit changes the next query result
immediately; there is no stale content key.

### Unrelated meta+value rebuild — immediate before/after jobs

| File size | Before | Protected index | Speedup | Before allocated | Protected allocated |
|---:|---:|---:|---:|---:|---:|
| 1 KB | 155.814 us | 94.32 ns | 1,652x | 12,184 B | 320 B |
| 1 MB | 3.087 ms | 94.90 ns | 32,529x | 4,228,070 B | 320 B |
| 10 MB | 30.861 ms | 91.07 ns | 338,876x | 42,057,370 B | 320 B |

The 10 MB case removes 99.9992% of allocation and is independent of file size
after protection. Meta-only queries remain at 68.5-85.8 ns and 280 B with zero
file reads.

### Queries that genuinely require content

| Workload | File size | Before | Protected index | Mean change | Allocation |
|---|---:|---:|---:|---:|---:|
| Value-only equality | 1 MB | 2.403 ms | 2.215 ms | -7.8% | 4.23 MB unchanged |
| Value-only equality | 10 MB | 27.121 ms | 24.026 ms | -11.4% | 42.06 MB unchanged |
| Content meta+value equality | 1 MB | 4.471 ms | 2.487 ms | -44.4% | 4.23 MB unchanged |
| Content meta+value equality | 10 MB | 42.943 ms | 26.325 ms | -38.7% | 42.06 MB unchanged |
| Explicit `Value` read | 10 MB | 24.012 ms | 25.692 ms | +7.0%, I/O variance | 42.06 MB unchanged |

The content-equality path is faster because it no longer builds and hashes a
full-content dictionary key before performing the required comparison. Direct
`Value` still performs exactly one `ReadAllText`; its small cross-run timing
difference is not attributed to graph indexing.

Switchable counters distinguish `FileContentDiskReads`,
`FileContentCharactersRead`, `FileContentValueIndexSkips`, and
`FileContentExplicitValueQueryFallbacks`. Contracts prove zero disk reads for
meta-only and unrelated meta+value queries, one read for each explicit content
evaluation, immediate external-edit visibility, and exact out/in ordering.

A generic-value regression run retained zero allocation for string lookups.
The closest run measured 7.833 ns for a one-edge short-string value lookup,
inside the prior 7.4-8.0 ns band; the 1000-edge result was 8.966 ns versus
7.959 ns before. Meta+value measured 17.389/20.575 ns at 1/1000 edges versus
15.960/17.923 ns. This is a 0.4-2.7 ns compatibility-guard cost on the
nanosecond path, while allocations remain unchanged. Later repeated runs
showed machine-wide thermal variance and are retained as diagnostics rather
than used for percentage claims.

Final verification has 108 isolated and 37 integration tests passing with no
skips; `run.sln` builds with zero errors. Raw reports are under
`BenchmarkDotNet.Artifacts/file-content-index-before-2026-07-17`,
`file-content-index-after-2026-07-17`, and
`file-content-generic-value-regression-2026-07-17`.

## Selected optimization: consistent `$NoInherit` and filesystem overlay

`AbstractFileSystemVertex` now builds its logical overlay with the same
`$NoInherit` filtering contract as `EasyVertex`. It uses one merged list for
local, inherited, and generated filesystem edges instead of first materializing
an unfiltered inherited list and then copying it into a second list.

The shared `GraphUtil.AddRange_NoNoInherit` merge remembers the immediately
preceding meta vertex, so a run of edges with the same meta performs one
`$NoInherit` lookup rather than one lookup per edge. This is a traversal-local
optimization only; no persistent boolean cache or additional vertex state was
introduced.

Adding or removing a direct `$NoInherit` marker now follows the reverse
`MetaInEdgesRaw` links to the exact source vertices using the marked meta and
bumps their structure generation. Descendants detect the changed parent version
at their next query and rebuild before lookup. Renaming a marker-meta value to
or from `$NoInherit` applies the same rule, including marker-meta descendants.
An unrelated warmed hierarchy performs the global epoch check but does not
rebuild. New counters report `NoInheritMarkerChanges` and
`NoInheritAffectedSources`.

### Logical rebuild — immediate before/after default jobs

| Workload | Parent edges | Before | Corrected | Mean change | Before allocated | Corrected allocated |
|---|---:|---:|---:|---:|---:|---:|
| EasyVertex blocked-meta rebuild | 1 | 94.95 ns | 94.48 ns | -0.5% | 384 B | 384 B |
| EasyVertex blocked-meta rebuild | 1000 | 10.620 us | 2.515 us | -76.3%, 4.22x | 384 B | 384 B |
| Filesystem overlay rebuild | 1 | 211.73 ns, incorrect result | 131.50 ns | -37.9% | 624 B | 512 B |
| Filesystem overlay rebuild | 1000 | 2.301 us, incorrect result | 2.860 us | +24.3% | 32,520 B | 512 B |

The old filesystem rows are historical timing for incorrect semantics because
they included edges whose meta explicitly forbade inheritance. At 1000 edges,
correct filtering costs 558 ns relative to that invalid result, while removing
98.4% of allocation. The one-edge corrected path is both correct and faster
because the redundant second overlay list is gone.

### Dynamic marker acceptance cost

| Workload | Parent edges | Mean | Allocated |
|---|---:|---:|---:|
| Remove marker, query EasyVertex, add marker, query | 1 | 1.701 us | 2,024 B |
| Remove marker, query filesystem overlay, add marker, query | 1 | 1.588 us | 2,304 B |
| Remove marker, query EasyVertex, add marker, query | 1000 | 26.602 us | 19,143 B |
| Remove marker, query filesystem overlay, add marker, query | 1000 | 27.425 us | 18,688 B |

These rows have no valid old-path speedup comparison: the previous
implementation returned a stale result after the marker mutation. The measured
cost includes both mutations and two required logical rebuild/query phases.

A generic write-only control remains allocation-stable at 216 B. Focused
default-job add+remove means are 108.5 ns, 343.7 ns, and 2.610 us for
1/100/1000 existing edges, versus the preceding 144.0 ns, 441.1 ns, and
4.177 us measurements. No ordinary-mutation regression was observed.

The post-audit follow-up also disables the base shared logical/index rebuild for
filesystem vertices. That base path cannot see the separate
`FileSystemVertex.OutEdges` overlay and previously produced an index that lost
`Filename`, `Size`, and other generated metadata whenever the filesystem vertex
also inherited from another source. Filesystem vertices now take their explicit
full-rebuild fallback: the overlay-aware logical list is built first, then the
requested index. A matching `Filename:payload.txt` meta+value rebuild measures
637.7 ns at one parent edge and 3.480 us at 1000 parent edges, allocating
1.37 KB in both cases.

`FileVertex.Refresh()` and `DirectoryVertex.Refresh()` now invalidate the host
logical/index caches before replacing overlay edges. A non-text rename contract
confirmed the old stale `Filename:before.bin` result before the fix and immediate
`Filename:after.dat` visibility after it.

Final verification has 110 isolated and 39 integration tests passing with no
skips; `run.sln` builds with zero errors. Raw reports are under
`BenchmarkDotNet.Artifacts/no-inherit-overlay-before-2026-07-18`,
`no-inherit-overlay-after-2026-07-18`,
`no-inherit-incremental-control-2026-07-18`, and
`no-inherit-write-control-2026-07-18`. The overlay-index follow-up is under
`BenchmarkDotNet.Artifacts/no-inherit-fs-index-followup-2026-07-18`.

## Selected optimization: iterative graph traversals

Inheritance parent/child discovery, `InheritanceCompare`, deep-copy scope
collection, `DeepIterator`, subgraph collectors, and inheritance-level search
now keep their traversal state on explicit stacks. The copy algorithms retain
one original-to-copy map and create each destination vertex once. Legacy
cycles terminate through the existing visited/active-path sets without relying
on the process call stack.

Reverse edge pushes preserve the previous depth-first preorder, including exact
edge identity and duplicate behavior. `DeepIterator` still snapshots each
vertex's outgoing list when `canModifyOutEdges` is true, so callback mutations
do not change the current frame. The iterative inheritance-level traversal
retains path-local rather than global cycle detection, preserving maximum-path
semantics when branches converge.

### Depth scaling — immediate before/final default jobs

| Workload | Depth | Recursive | Iterative | Mean change | Recursive allocated | Iterative allocated |
|---|---:|---:|---:|---:|---:|---:|
| Rebuild inherited query | 10 | 990.8 ns | 1,095.9 ns | +10.6%, overlapping dispersion | 2.98 KB | 2,736 B |
| Rebuild inherited query | 100 | 9.854 us | 8.160 us | -17.2%, 1.21x | 25.88 KB | 22,592 B |
| Rebuild inherited query | 1000 | 158.053 us | 127.055 us | -19.6%, 1.24x | 251.70 KB | 217,832 B |
| Missing inherited type | 10 | 929.0 ns | 274.6 ns | -70.4%, 3.38x | 1.25 KB | 848 B |
| Missing inherited type | 100 | 53.949 us | 2.772 us | -94.9%, 19.46x | 11.23 KB | 7,464 B |
| Missing inherited type | 1000 | 6.891 ms | 33.635 us | -99.5%, 204.87x | 110.62 KB | 73,240 B |
| Collect non-link subgraph | 10 | 463.3 ns | 330.8 ns | -28.6%, 1.40x | 1.13 KB | 848 B |
| Collect non-link subgraph | 100 | 5.719 us | 4.131 us | -27.8%, 1.38x | 11.11 KB | 7,464 B |
| Collect non-link subgraph | 1000 | 108.764 us | 55.756 us | -48.7%, 1.95x | 110.50 KB | 73,240 B |
| `DeepIterator` miss | 10 | 374.8 ns | 303.2 ns | -19.1%, 1.24x | 1.23 KB | 880 B |
| `DeepIterator` miss | 100 | 4.968 us | 3.871 us | -22.1%, 1.28x | 11.20 KB | 7,496 B |
| `DeepIterator` miss | 1000 | 130.547 us | 39.368 us | -69.8%, 3.32x | 110.59 KB | 73,272 B |

The inherited-type workload improves superlinearly because it now traverses
physical inheritance links once instead of recursively querying already
flattened logical `$Inherits` buckets. The depth-10 inherited rebuild is the
only slower mean. Its +105 ns delta is smaller than the combined run
dispersion, allocation still falls, and the depth-100/1000 cases improve.

Contracts exercise preorder, first-result short-circuiting, mutation snapshots,
cycles, a 10,000-level inheritance and subgraph chain, and a 5,000-level deep
copy. The formerly recursive paths complete without stack overflow. The warm
`IsInherited` compatibility control remains allocation-free and unchanged in
source; its uncontended ShortRun measures 12.24/39.66/383.80 ns at depth
1/10/100.

The follow-up audit also removed recursion from stack parent-frame fallback and
ZeroCode `NextAtom` execution. Parent lookup now walks cached direct parents in
a loop, retaining allocation-free warmed lookups; malformed parent cycles are
bounded by delayed cycle tracking. `NextAtom` keeps an explicit DFS frame list,
preserves early return and branch order, and uses path-local cycle detection.
Its reusable thread-local traversal state avoids steady-state allocations while
remaining safe for nested/reentrant execution.

| Follow-up workload | Depth | Recursive | Iterative | Mean change | Recursive allocated | Iterative allocated |
|---|---:|---:|---:|---:|---:|---:|
| Parent-frame hit | 1000 | 8.872 us | 7.095 us | -20.0%, 1.25x | 0 B | 0 B |
| Parent-frame miss | 1000 | 9.436 us | 7.231 us | -23.4%, 1.30x | 0 B | 0 B |
| Execute `NextAtom` chain | 10 | 389.9 ns | 326.8 ns | -16.2%, 1.19x | 400 B | 0 B |
| Execute `NextAtom` chain | 100 | 5.466 us | 4.334 us | -20.7%, 1.26x | 4,000 B | 0 B |
| Execute `NextAtom` chain | 1000 | 73.516 us | 51.451 us | -30.0%, 1.43x | 40,000 B | 0 B |

The new contracts run both paths at depth 10,000, terminate explicit
parent-frame and `NextAtom` cycles, preserve DFS order, and preserve immediate
stack-frame return.

Final verification has 117 isolated and 42 integration tests passing with no
skips; `run.sln` builds with zero errors. Raw reports are under
`BenchmarkDotNet.Artifacts/iterative-traversal-before-2026-07-18`,
`iterative-traversal-final-2026-07-18`, and
`iterative-traversal-controls-rerun-2026-07-18`. Follow-up reports are under
`iterative-followup-before-2026-07-18`,
`iterative-followup-after-2026-07-18`, and
`iterative-execution-final2-2026-07-18`.

## Final acceptance matrix and read-only API decision

A curated final `ShortRun` gate completed 132 accepted benchmark cases across
13 classes. Rejected prototypes and exploratory strategy matrices were excluded
so the result measures production paths rather than inflating the run with
non-selected alternatives. All 132 cases completed without benchmark errors.

| Final control | Scale | Mean | Allocated |
|---|---:|---:|---:|
| Warm direct-meta query | 1 / 100 / 1000 edges | 4.611 / 5.717 / 5.418 ns | 0 B |
| Target value mutation + query | 1 / 100 / 1000 edges | 86.22 ns / 2.925 us / 31.561 us | 336 / 4,080 / 39,936 B |
| Incremental add/query-all/remove/query-all | 1000 edges | 2.748 us | 216 B |
| Deep inherited-query rebuild | depth 1000 | 105.950 us | 217,832 B |
| Missing inherited type / subgraph / `DeepIterator` | depth 1000 | 35.670 / 45.915 / 44.392 us | 73,240 / 73,240 / 73,272 B |
| Parent-frame hit / miss | depth 1000 | 7.704 / 8.137 us | 0 B |
| `NextAtom` chain | depth 1000 | 49.461 us | 0 B |
| Unrelated file-content meta+value rebuild | 10 MB | 97.19 ns | 320 B |
| Cross-store detach+attach | 1000 edges | 281.258 us | 104,068 B |

Optional optimization 13, changing public edge-list properties from
`IList<IEdge>` to `IReadOnlyList<IEdge>`, is deliberately deferred. A repeated
repository-wide inventory found no production mutation of logical `OutEdges`
and no UI/application caller mutating any public edge-list getter. Active raw
adds remain the same five controlled graph-core paths: normal `EasyVertex`
addition, JSON reconstruction, Binary reconstruction, and the two specialized
stack additions. Direct raw mutations outside graph core occur only in three
test/benchmark fixtures that deliberately construct legacy inheritance cycles
or detached states.

The migration would still be source- and binary-breaking for external
`IVertex` consumers. Internally, 23 read-only local aliases in
`BaseInstructions` are typed as `IList<IEdge>`, and all `IVertex` implementations
and overrides would need signature changes. Existing incremental indexes do
not require this migration: supported `Add`/`Remove` paths already pass through
`OutList` callbacks and retain full-rebuild fallback. `ExtandableList.Clear`
and its index setter remain theoretically unsafe because they bypass callbacks,
but the inventory found no active caller.

The decision gate is therefore **defer, with no implementation change**. Reopen
it only for a versioned public-API release, a proven external-mutation defect,
or an explicitly defined concurrent graph model. The direct performance
benefit remains zero, while compatibility risk is high. Raw reports for the
final gate are under
`BenchmarkDotNet.Artifacts/final-acceptance-after-o12-2026-07-18`.
