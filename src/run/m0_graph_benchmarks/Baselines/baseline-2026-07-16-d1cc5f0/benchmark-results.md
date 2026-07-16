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

Cross-store detach+attach currently measures 193.5 ns, 18.922 us, and 497.937 us for 1, 100, and 1000 edges under the default job, with unchanged allocation of 504 B, 44,066 B, and 440,080 B. The 1- and 100-edge cases are 8.7% and 5.3% faster than the correction-3 default acceptance run; the 1000-edge case is 51.7% slower by mean. Its current result is close to the earlier correction-3 `ShortRun` mean of 490.358 us, so this is retained as a scaling watch item rather than attributed to a later semantic change.

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
