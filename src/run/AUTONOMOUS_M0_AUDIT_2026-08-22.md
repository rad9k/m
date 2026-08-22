# Autonomous m0 performance and correctness audit

## Scope and operating rules

- Started: 2026-08-22 01:00 UTC+2.
- Production audit scope: compiled sources and XAML in `m0`, `m0_console`, and `m0_desktop`.
- Tests and benchmarks are verification infrastructure, not production optimization targets.
- Accepted production changes must be preceded by characterization, regression, conformance, or differential coverage.
- Performance changes must have a recorded baseline and an acceptance measurement.
- Suspicious behavior without a reliable oracle is documented but not changed.
- Changes are grouped into local commits only after the corresponding tests are green. No push is permitted.
- Existing unrelated work must never be included in a commit.
- High-risk graph behavior follows `.ai/rules.md`; diagnostic logging uses `MinusZero.Instance.Log`.

## Repository baseline

- Branch: `master`, tracking `origin/master`.
- Starting commit: `5601a67c` (`tests`).
- Starting working tree: clean.
- Recent commits:
  - `5601a67c tests`
  - `1cd47389 logi usuniete`
  - `b531e2b0 ponowna poprawka rozmiarow dla commandera`
  - `b575e6c7 poprawwa size w commanderze`
  - `66c45e2c co za zjeb`
- .NET SDK: `10.0.302`.
- Runtime: `.NET 10.0.10`, x64.
- OS: Windows `10.0.26200`.
- MSBuild: `18.6.11`.
- Compiled-item manifests were obtained through MSBuild for all three production projects. The final coverage section will contain the reviewed-area inventory.

## Baseline build

Command:

```powershell
dotnet build run.sln --configuration Release --property:Platform=x64
```

Result:

- Build succeeded.
- Errors: 0.
- Warnings: 167.
- Elapsed: 60.69 seconds.
- Most warnings are pre-existing legacy and `m0_COMPOSER` warnings outside the authorized production-change scope.

## Baseline tests

An initial `--no-build --property:Platform=x64` invocation could not find test assemblies because the project-level test output layout differs from the solution x64 output assumption. This was an invocation issue, not a product failure. Baseline tests were rerun with each project building its normal Release output.

### Isolated graph contracts

Command:

```powershell
dotnet test m0_graph_tests/m0_graph_tests.csproj --configuration Release
```

Result: **114 passed, 2 failed, 0 skipped, 116 total**.

Pre-existing failures on the clean starting commit:

1. `TransactionContractTests.RepeatedValueChangesCoalesceButRollbackFullJournal`
   - Expected one listener atom representing `Before -> Second`.
   - Actual listener collection contains two atoms: `Before -> First` and `First -> Second`.
2. `TransactionContractTests.AddedThenRemovedEdgeProducesNoNetChangeEvent`
   - Expected no listener change-set entry for adding and removing the same edge instance.
   - Actual change-set retains an entry.

### Bootstrapped integration contracts

Command:

```powershell
dotnet test m0_graph_integration_tests/m0_graph_integration_tests.csproj --configuration Release
```

Result: **77 passed, 4 failed, 1 skipped, 82 total**.

Pre-existing failures:

1. `TransactionGarbageCollectionFailureModeTests.GarbageCollectionRequestWithoutTransactionIsNotLost`
   - Orphan remains `Live` after the following transaction instead of becoming `Disposed`.
2. `GenerationCounterIntegrationTests.InheritedQuerySeesWritesAndRollbackInsideTransaction`
   - Rollback leaves the first inherited edge visible.
3. `EventContractTests.AddedThenRemovedEdgeProducesNoListenerEvent`
   - Listener receives one event instead of zero.
4. `EventContractTests.TransactionalListenerReceivesOneNetValueChange`
   - Listener receives `NewValue == First` instead of `Second`.

Skipped known failure:

- `TransactionGarbageCollectionFailureModeTests.ListenerExceptionDoesNotPinEventOrLeaveChildTransactionCurrent`.

### Desktop contracts

Command:

```powershell
dotnet test m0_desktop_tests/m0_desktop_tests.csproj --configuration Release
```

Result: **1 passed, 1 failed, 0 skipped, 2 total**.

Pre-existing failure:

- `InEdgesListVisualiserContractTests.ItemsSourceMatchesPhysicalIncomingEdgesAfterCommit`
  - Expected 3 rows matching physical incoming edges.
  - Actual row count: 6.

## Baseline interpretation

The starting revision builds but is not test-green. These failures are not attributed to new audit work. They are being investigated as correctness candidates because they are expressed by repository-owned contract tests and several align with invariants documented in `TO-DO/Graph core optimization plan.md`. No production changes have been made yet.

The latest commit adds desktop and garbage-collection tests. Its predecessor removes code from five desktop visualizer/helper files. This history is diagnostic context only; no older code version will be checked out or treated as an implementation source.

## Audit work in progress

Read-only subsystem audits started in parallel for:

- ZeroCode, ZeroUML, parser/generator, and execution hot paths.
- Store, persistence, bootstrap, network, and standard views.
- WPF desktop startup, visualizers, layout, event handling, and graph synchronization.
- Console startup, project configuration, and cross-project integration.

No optimization candidate will be implemented until baseline profiling and contract analysis identify an evidence-backed target.

## Broad ZeroCode diagnostic baseline

All persisted ZeroCode workload programs (`Code0` through `Code14`, including `Code7b`) were freshly parsed and executed with production diagnostics. Every program completed with a non-null result and zero reported execution errors.

Largest measured executions:

- `Code7`: 707.337 ms, 4,013,848 B allocated.
  - 4,700 `for vertex` frame pushes/pops.
  - 9,574,510 collapsed-assignment query edges.
  - `Query`: 638.325 ms self time.
- `Code8`: 518.264 ms, 40,814,160 B allocated.
  - 100,001 while iterations and frame pushes/pops.
  - `While`: 253.166 ms self time.
  - `RedirectLeftEdgesToRightVertices`: 221.710 ms self time.
- `Code2`: 50.134 ms, 2,779,568 B allocated.
  - `AddLeftEdgesToRightVertices`: 46.865 ms self time.
- Remaining programs completed between 0.056 ms and 11.450 ms in this diagnostic run.

These are causal diagnostics rather than stable acceptance benchmarks. `Code7`, `Code8`, and `Code2` are retained as broad workload candidates for targeted profiling.

## Accepted change group 1 — transaction correctness, cleanup, and coalescing

### Evidence before implementation

The clean baseline already contained failing contract tests for:

- non-LIFO rollback of repeated changes;
- stale inherited query state after rollback;
- multiple listener events for repeated value changes;
- an add/remove pair of the same edge producing a false net event.

Two additional existing/documented contracts were activated before production changes:

1. `GraphChangeSuppressionBelongsToTransactionInstance` was added to the isolated suite.
   - It failed because `GraphChangeWatchActive` was backed by one static field shared by all transaction instances.
2. `ListenerExceptionDoesNotPinEventOrLeaveChildTransactionCurrent` was enabled.
   - It failed because listener exceptions bypassed event external-reference cleanup and restoration of the parent transaction.

### Performance baseline

BenchmarkDotNet transaction baseline:

- No watcher, one value change: 13.72 us, 4.31 KB.
- No watcher, ten value changes: 18.87 us, 6.11 KB.
- One watcher, one value change: 71.20 us, 12.50 KB.
- One watcher, ten value changes: 516.49 us, 55.39 KB.

The near-linear event cost for ten writes confirmed that intermediate listener events were being materialized instead of one net event.

### Implementation

Changed `m0/Graph/ExecutionFlow/Transaction.cs`:

- `GraphChangeWatchActive` is now transaction-instance state.
- The atom list is the complete rollback journal.
- Rollback executes the journal in reverse order.
- Listener dictionaries are a separate net change-set.
- Repeated value changes retain the first old value and latest new value.
- Add/remove of the same physical edge cancels from outgoing, incoming, and meta listener change-sets.
- Trigger infrastructure mutations remain excluded from listener delivery but remain rollback-capable.
- Graph-change suppression is restored with `finally`.
- Event external references are removed with `finally`.
- Second-stage cleanup runs even when listener dispatch throws.

Changed `m0/Lib/Sys.cs`:

- Parent transaction restoration now occurs in `finally`, including listener-exception paths.

Changed second-stage orphan scheduling:

- `ExecutionFlowHelper` now defers orphan cleanup requested without a current transaction.
- The next explicit or ambient transaction adopts and executes those actions.
- `GraphLifecycleLog` records this state as `SecondStageDeferred`, rather than incorrectly reporting that the action was dropped.

### Verification

- Isolated graph contracts: **117 passed, 0 failed, 0 skipped**.
- Bootstrapped integration contracts: **82 passed, 0 failed, 0 skipped**.
- This includes the formerly skipped listener-exception cleanup contract and the no-transaction orphan-collection contract.
- IDE diagnostics: no new linter errors in changed files.
- Desktop suite remains at its independent baseline of 1 pass / 1 failure; its existing incoming-edge snapshot defect is not masked by the transaction repair.

### Acceptance performance

Final same-harness BenchmarkDotNet measurements:

- No watcher, one value change: 17.03 us, approximately 4.3 KB.
- No watcher, ten value changes: 21.45 us, approximately 6.1 KB.
- One watcher, one value change: 91.81 us, 12.53 KB.
- One watcher, ten value changes: 117.14 us, 14.75 KB.

The short in-process job is noisy for single-change latency and shows a small absolute correctness cost from retaining a full rollback journal. The repeated-write watcher path improves from 516.49 us to 117.14 us (**77.3% faster**) and from 55.39 KB to 14.75 KB (**73.4% less allocation**). The change is accepted because it repairs six active contract failures, enables one previously skipped failure-mode contract, and removes the measured repeated-event amplification.

## Accepted change group 2 — stable incoming-edge visualizer snapshots

### Evidence before implementation

`InEdgesListVisualiserContractTests.ItemsSourceMatchesPhysicalIncomingEdgesAfterCommit` failed on the clean baseline and remained red after transaction repair:

- Physical incoming-edge count after commit: 3.
- DataGrid snapshot count: 6.
- A manual refresh after commit already made the second contract pass.

The synchronous graph-change listener refreshed `ItemsSource` while temporary graph-change event and edge-payload vertices were still externally retained. The DataGrid therefore held a stale snapshot containing transient edges even though second-stage cleanup correctly removed those edges before `CommitTransaction` returned.

### Implementation

`InEdgesListVisualiser` now participates in second-stage commit cleanup:

- The existing immediate refresh remains unchanged for compatibility with synchronous listener behavior.
- A post-commit refresh is deduplicated per commit.
- The refresh is deliberately requeued to the second cleanup wave, after event/payload orphan actions from the first wave have run.
- The post-cleanup refresh calls the non-scheduling core directly, preventing an infinite reschedule loop.
- A visualizer disposed before the cleanup wave does not refresh.

This avoids identifying transient graph structures by fragile meta names and preserves the documented contract that the visualizer represents physical `InEdgesRaw`.

### Verification

- Desktop contracts: **2 passed, 0 failed, 0 skipped**.
- The original manual-refresh contract remains green.
- The formerly failing automatic commit-refresh contract now matches physical edge count, exact edge membership, and live source state.

## Accepted change group 3 — enable compiler optimization for Release/x86

### Evidence before implementation

MSBuild property evaluation showed:

- Default Release (`AnyCPU`): `Optimize=true`.
- Explicit Release/x64: `Optimize=true`.
- Desktop default Release: `Optimize=true`.
- Explicit m0 Release/x86: `Optimize=false`.

The x86 override was introduced in 2020 without a project comment or a related correctness constraint. BenchmarkDotNet independently rejected the x86 dependency with: `assembly ... references non-optimized m0`.

Before changing the setting, the complete x86 Release contract suites passed:

- Isolated graph: 117/117.
- Integration: 82/82.

Direct x86 diagnostics with the same executable harness:

- `Code7`: 1,402.710 ms, 4,203,296 B.
- `Code8`: 5,771.181 ms, 24,008,364 B.

### Implementation

`m0/m0.csproj` now sets `Optimize=true` for `Release|x86`, matching every other Release configuration. No runtime code, API, data format, architecture target, or package reference changed.

### Verification

Optimized x86 Release:

- Isolated graph contracts: **117 passed**.
- Integration contracts: **82 passed**.
- Desktop contracts: **2 passed**.
- All suites have zero failures and zero skips.
- Full `run.sln` Release/x86 build: 0 errors (159 pre-existing warnings).

Direct same-harness x86 diagnostics:

- `Code7`: 1,343.816 ms, 4,180,068 B — 4.2% faster in this diagnostic run.
- `Code8`: 984.611 ms, 24,008,392 B — 82.9% faster (5.86x).
- Both results retained their expected graph result sizes and reported zero execution errors.
- The isolated graph test execution phase decreased from 814 ms to 415 ms; this is corroborating harness data rather than a standalone benchmark.

BenchmarkDotNet's normal out-of-process x86 child could not be launched by the installed x64 `dotnet` host, so no fabricated BenchmarkDotNet result is reported. The direct before/after runner used the same x86 process, workload, diagnostics, SDK, and machine.

## Rejected experiment — bypass QueryOperator for simple assignment targets

### Hypothesis

`Code8` executes a simple `A = A + 1` assignment 100,001 times. A trial fast path queried a single left-hand stack edge directly instead of constructing and executing the normal `QueryOperator` result stack.

### Verification

- Isolated graph contracts: 117/117.
- Integration contracts: 82/82.
- Desktop contracts: 2/2.
- x64 BenchmarkDotNet indicated a possible improvement for `Code8` from the stable 82.549 ms baseline to 70.445 ms, and diagnostics reduced executed Query instructions from 100,002 to 91.

The cross-architecture acceptance gate rejected the change:

- Optimized Release/x86 `Code8` diagnostic baseline: 984.611 ms.
- Trial fast path: 5,619.574 ms.
- This is a 5.71x x86 regression despite correct output.

The complete trial was manually reverted before any commit. No production code from this experiment remains. The result demonstrates why optimizations are checked across supported configurations rather than accepted from one x64 workload.

## Accepted change group 4 — parser, numeric, HTTP, and graph-hover contracts

### Test-first defects

Three independent production bugs received focused red tests before correction:

1. Decimal boolean conversion:
   - `GetBolleanValue` selected `NumericTypeEnum.Decimal` and then cast the boxed decimal to `double`.
   - Positive, zero, and negative decimal tests all failed with `InvalidCastException`.
2. TextStore parser selection:
   - A valid non-empty parser query from the first `.m0t` line was always replaced by the default query.
   - The contract expected the alias query to remain; the baseline loaded the default instead.
3. HTTP OPTIONS dispatch:
   - Upper- and lower-case OPTIONS contracts expected `HttpActionEnum.OPTIONS`.
   - The baseline returned `HEAD`.

Corrections:

- Decimal truth conversion now compares the original decimal value, matching `IsTrue_Vertex`.
- TextStore defaults its parser query only when the stored line is null or empty.
- HTTP method mapping is isolated in a tested helper and preserves OPTIONS.

### Graph hover performance

`GraphVisualiser.GetVertexWrapperByEventSource` previously performed two full `DisplayedVerticesUIElements.FirstOrDefault` scans at every visual-tree level for every mouse move. With `V` displayed vertices and hierarchy depth `D`, hover lookup cost was `O(V * D)`.

`SimpleVisualiserWrapper` now verifies its owning `GraphVisualiser` directly. Lookup walks only the visual/logical parent hierarchy and returns the owning wrapper in `O(D)`, without scanning the displayed-vertex dictionary. The ownership check deliberately skips wrappers belonging to nested graph visualizers, preserving the old outer-wrapper behavior.

A new STA contract builds nested inner/outer wrappers and confirms that an event originating in the inner child resolves to the correct wrapper owned by the outer visualizer. The old scan-based implementation failed the isolated oracle; the new hierarchy lookup passes.

### Verification

- Isolated graph/contracts: **122 passed, 0 failed, 0 skipped**.
- Bootstrapped integration: **83 passed, 0 failed, 0 skipped**.
- Desktop STA contracts: **3 passed, 0 failed, 0 skipped**.
- Full Release/x64 solution build: 0 errors.
- IDE diagnostics: no new linter errors.

## Production audit coverage and deferred findings

The compiled production manifests and subsystem audits covered:

- m0 graph/query/execution infrastructure;
- ZeroCode, ZeroUML, parser and generator;
- JSON, Binary, Text, memory and filesystem stores;
- bootstrap, network/REST and standard views;
- all m0_console code and shared startup paths;
- all m0_desktop C# and XAML, including visualizers, UX, controls, commands and startup.

High-value candidates intentionally left unchanged because their required oracle or redesign exceeds a safe autonomous patch:

- lazy loading of nested `.m0j/.m0t/.m0x` stores during filesystem refresh;
- global detach/attach of all stores during commit;
- incremental `UXVisualiser.Paint` and `FormVisualiser` rebuilds;
- separating ListVisualiser row refresh from column/template recreation;
- batching GraphVisualiser layout instead of calling `UpdateLayout` per node;
- wiring the existing redirect-assignment cache into `=`/`+=`;
- Text2Graph keyword memoization keyed only by a weak hash;
- Graph2Text recursive link resolution and repeated dictionaries;
- asynchronous icon-directory preload and console/bootstrap phase redesign.

These remain in the final candidate ledger with file/member references and required tests. They were not modified merely because static inspection made them look expensive.

## Accepted change group 5 — synchronized visualizer listener lifecycle

Both `FirstSelectedEdgeSynchronisedHelper` and `SellectedSelectedSynchronisedHelper` registered:

- a listener on the master's `SelectedEdges` vertex;
- a disposal listener on the detail visualizer's host.

When the detail visualizer was removed, each callback deleted only the selected-edge listener. The disposal listener remained attached to the long-lived host, retaining the helper and allowing stale callbacks to accumulate across repeated open/close cycles.

Test-first STA coverage creates both helper variants, verifies both listeners exist, removes the detail edge in a transaction, and requires both `$GraphChangeTrigger` edges to disappear. The baseline left one host trigger for both variants.

Each disposal callback now removes both listener edges. Verification:

- focused lifecycle contracts: 2/2 passed;
- full desktop suite: **5 passed, 0 failed, 0 skipped**;
- no new IDE diagnostics.

This is a long-session memory and responsiveness correction: disposed detail panes no longer leave event infrastructure or callbacks behind.

## Final validation

Final Release test gate, run sequentially to avoid concurrent MSBuild output locking:

- `m0_graph_tests`: **122 passed, 0 failed, 0 skipped**.
- `m0_graph_integration_tests`: **83 passed, 0 failed, 0 skipped**.
- `m0_desktop_tests`: **5 passed, 0 failed, 0 skipped**.
- Total: **210 passed, 0 failed, 0 skipped**.

Final solution builds:

- Release/x64: 0 errors, 125 pre-existing warnings in the incremental build.
- Release/x86: 0 errors, 65 pre-existing warnings in the incremental build.
- The initial clean full build reported 167 warnings; no warning-cleanup campaign was mixed into this work.

Final broad ZeroCode diagnostics:

- `Code0` through `Code14`, including `Code7b`, all parsed and executed.
- Every program reported zero execution errors and retained its expected result shape.
- Final diagnostic `Code7`: 600.979 ms, 4,013,848 B, 4,669 result edges.
- Final diagnostic `Code8`: 550.266 ms, 40,814,160 B, 4 result edges.
- These counter-enabled values are diagnostic, not a claim that unrelated workloads improve by the same percentage.

## Prioritized remaining candidate ledger

### Performance candidates requiring a dedicated follow-up

1. **Filesystem nested-store loading**
   - `m0/Store/FileSystem/FileVertex.cs`, `Refresh`.
   - Accessing filesystem overlays can construct and load complete nested `.m0j`, `.m0t`, or `.m0x` stores.
   - Required gate: lazy-open contract plus directory matrices with 0/100/1000 store files.
2. **Global store detach/attach on commit**
   - `m0/m0.cs`, global `CommitTransaction`.
   - Potentially touches every store for an unrelated mutation.
   - Very high risk: requires JSON/Binary/filesystem/cross-store differential tests.
3. **Bootstrap and drive enumeration**
   - `m0/Bootstrap/LoadFromBootstrap.cs`, `m0.cs Initialize`, `AddDrives`, `CreateAutostart`.
   - Required first step: phase timing through `MinusZero.Instance.Log`, including slow/network drives.
4. **Completed — GraphVisualiser per-node layout**
   - `m0_desktop/UIWpf/Visualisers/GraphVisualiser.cs`, `Add` and `AddCircle`.
   - Local measure/arrange now supplies geometry; one global layout remains per paint.
5. **Completed — List/Form/UX complete rebuilds**
   - `ListVisualiser.BaseEdgeToUpdated`, `FormVisualiser.BaseEdgeToUpdated`, `UXVisualiser.Paint`.
   - List schemas persist across row refreshes; hosted UX value changes bypass parent repaint; existing Form guards were retained.
6. **Completed — Code7 collapsed assignment query amplification**
   - `BaseInstructions.AddDistinctFromMetaQueryMatch` and `AddLeftEdgesToRightVertices`.
   - A specialized stack cache now preserves first-edge identity/order without rescanning accumulated values.
7. **Completed — Text2Graph hash-only memoization**
   - `m0/ZeroCode/Text2GraphProcessing.cs`, `_tryIsKeyword`.
   - Structural equality now distinguishes deliberately colliding parser states.
8. **Completed — Graph2Text link traversal**
   - `m0/ZeroCode/Graph2TextProcessing.cs`, `GetLinkString_Recurrect`, `AppendAsLink`, `MatchKeywords`.
   - Reverse imports, allocation reductions and processor reuse are guarded by an exact output hash and benchmark.
9. **VertexToJson visited and repeated dictionary work**
   - `m0/Lib/StdView/VertexToJson.cs`.
   - Required gate: golden outputs from `.ai/01 Json.md` before changing traversal structures.
10. **HTTP concurrency and synchronous I/O**
    - `m0/Network/Server/Server.cs`, `CallHandler` lock and request-body `.Result`.
    - Required gate: random-port integration server with concurrent GET/POST load.

### High-confidence concerns not changed without a complete oracle

- `m0/m0.cs`: graceful `Dispose/Finalize` reaches `Environment.Exit(-1)`.
- `m0_console/console/ConsoleRunner.cs`: process lifetime depends on blocking autostart code; non-blocking/no-autostart paths may exit immediately.
- `m0.cs Initialize_AfterPossibleUXInitialized`: autostart can block while its transaction is still open.
- `m0_desktop/UIWpf/Visualisers/GraphVisualiser3D.cs`: a mid-animation dispose may leave a `CompositionTarget.Rendering` handler.
- `m0_desktop/UIWpf/PlatformClassSimpleWrapper.xaml.cs`: `SelectedEdgesChange` subscription has no proven symmetric unsubscribe path.
- `m0/Store/Json` primitive converter: unsupported CLR values are serialized as an empty string, risking silent data loss.
- Binary persistence does not visibly mirror JSON's `$GraphChangeTrigger` exclusion.
- `StoreBase.GetRootIdentifier` falls back to the first dictionary key, which is not a semantic root contract.

These are deliberately reported rather than patched. Several could be intentional compatibility behavior; each needs an executable product-level oracle.

## Local commits and rollback map

Functional commits created by this audit:

1. `490966e6` — transaction rollback, listener coalescing, exception cleanup and deferred orphan collection.
2. `dab7c52f` — incoming-edge visualizer post-event snapshot correction.
3. `fb3ca4f9` — enable compiler optimization for explicit Release/x86.
4. `24aef31b` — TextStore, decimal, OPTIONS and graph-hover corrections.
5. `63af24e4` — synchronized visualizer listener cleanup.
6. `f2e57328` — simple ZeroCode assignment-target fast path.
7. `0c770435` — GraphVisualiser layout batching.
8. `c4bf8bfa` — List and UX rebuild suppression.
9. `ef8212d9` — collapsed Code7 assignment-query cache.
10. `eed07bdf` — collision-safe Text2Graph memoization.
11. `3a1330fd` — Graph2Text traversal and golden-output benchmark.

Rollback notes:

- `fb3ca4f9` is independent and can be reverted alone.
- `dab7c52f` relies on the corrected second-stage lifecycle in `490966e6`; revert `dab7c52f` before reverting `490966e6`.
- `63af24e4` is behaviorally independent but was verified against the transaction event lifecycle from `490966e6`.
- Use normal `git revert <commit>` rather than rewriting history.
- No commit was pushed.

The assignment fast path was initially rejected under the original x86 gate. It was later accepted in a follow-up after the product decision that x86 is not supported; see the follow-up section below.

## Final outcome

- Starting state: clean build, but 7 active test failures and 1 skipped known failure across the three suites.
- Final state after follow-ups: 215 tests green, zero skipped, Release/x64 build green.
- Accepted changes: five initial functional commits plus the follow-up optimization commits.
- Performance evidence:
  - repeated watcher transaction: 516.49 us to 117.14 us, with allocation 55.39 KB to 14.75 KB;
  - explicit Release/x86 `Code8` diagnostic: 5,771.181 ms to 984.611 ms after enabling optimization;
  - graph hover lookup complexity: `O(V * D)` to `O(D)`;
  - Graph2Text generation: 71.586 ms to 56.815 ms;
  - Code7 query self time: 548.634 ms to about 11 ms in counter-enabled diagnostics.
- The simple assignment fast path is accepted for supported x64 builds; x86 is explicitly outside the product acceptance gate.

## Follow-up — accepted simple assignment fast path for x64

The product decision established that x86 is not supported:

- published desktop and console packages target x64 and ARM64;
- solution `Release|x86` maps m0, m0_console and m0_desktop to `Any CPU`;
- true x86 requires an explicit project-level `-p:Platform=x86` build.

The earlier x86 veto was therefore removed. The fast path was reintroduced unchanged:

- only a simple left `Query` with no next expression and exactly one target is eligible;
- dynamic/parenthesized, missing, multiple-target and non-meta-mode cases retain the full QueryOperator path;
- scalar evaluation and target replacement semantics are unchanged;
- the optimization skips left QueryOperator dispatch and its temporary result stack.

Same-session x64 BenchmarkDotNet (`10` iterations, `3` warmups):

- `Code7`: 127.083 ms -> 94.951 ms (**25.3% faster**);
- `Code8`: 181.450 ms -> 122.577 ms (**32.4% faster**);
- measured allocation remained 3.81 MB and 38.92 MB respectively.

Verification:

- isolated graph contracts: 122/122;
- integration contracts, including every persisted `Code0`–`Code14` workload: 83/83;
- desktop STA contracts: 5/5;
- no new IDE diagnostics.

ARM64 performance remains unmeasured on the current x64 machine; semantic coverage is architecture-independent, but an ARM64 performance run remains recommended.

## Follow-up — GraphVisualiser layout batching

`GraphVisualiser.Add` previously called `UpdateLayout()` after every wrapper was appended, and the root wrapper was updated a second time. Painting `N` vertices could therefore request approximately `N + 1` synchronous global layout updates while the canvas was still being constructed.

The new path:

- locally measures and arranges each new wrapper so `ActualWidth` and `ActualHeight` are immediately available for centering and line geometry;
- removes the duplicate root `UpdateLayout`;
- performs one final canvas `UpdateLayout` after all circles, wrappers and lines are constructed.

This preserves immediate node dimensions without repeatedly laying out all previously added siblings.

Test-first STA coverage verifies that an added wrapper has non-zero dimensions and is centered from those exact dimensions before line creation. The isolated old implementation could not satisfy this contract without a connected global layout root; local measure/arrange now does.

Verification:

- GraphVisualiser focused contracts: 2/2;
- full desktop suite: 6/6;
- full Release/x64 solution build: 0 errors;
- no new IDE diagnostics.

## Follow-up — Code7 collapsed assignment query cache

`Code7` repeatedly executes `+=` against variables whose stack buckets contain every previously appended value. QueryOperator must return one representative per exact `(From, Meta)` assignment target, but the old collapse path enumerated the entire growing bucket on every iteration. Diagnostics counted 9,574,510 discarded duplicate edges.

The specialized ZeroCode stack now maintains a lazy collapsed-meta cache:

- cache entries retain the first exact edge for every reference-identical `(From, Meta)` group and preserve insertion order;
- successful local adds incrementally update warm entries;
- removals, batches and pool resets invalidate entries conservatively;
- meta-inheritance generations force rebuilds when query aliases may have changed;
- misses in temporary child frames are not cached, avoiding per-frame dictionaries;
- ordinary `EasyVertex`, value queries, dynamic queries and non-collapse paths are unchanged.

QueryOperator uses this path only when collapse mode requests a meta-only query from `NoInEdgeInOutVertexVertex`. Representatives are still passed through the existing final deduplication, preserving overlap semantics across expanded query values.

Tests cover:

- duplicate values under one `(From, Meta)`;
- multiple sources and two different meta vertices with the same value;
- first-representative order;
- incremental duplicate and new-group additions;
- representative removal and rebuild.

Measured effects:

- counter-enabled `Code7` diagnostic: 600.979 ms -> 93.962 ms;
- Query self time: 548.634 ms -> 11.038 ms;
- BenchmarkDotNet measured allocation: 3.81 MB -> 3.63 MB;
- 10-iteration x64 run before the cache: 94.951 ms; first 10-iteration cache run: 82.493 ms (timing variance is high, so diagnostics provide the stronger causal evidence);
- `Code7` retained 4,669 result edges and zero execution errors.

Verification:

- isolated contracts: 123/123;
- integration contracts: 83/83;
- full Release/x64 solution build: 0 errors;
- no new IDE diagnostics.

The structural layout-pass reduction is from per-node global updates to one global update per paint. Local subtree measure/arrange remains per wrapper because geometry requires each wrapper's desired size.

## Follow-up — suppress unnecessary List and UX rebuilds

### ListVisualiser

`BaseEdgeToUpdated` previously called `ResetView` for every data refresh. `ResetView` clears and recreates every DataGrid column and both view/edit templates, even when only row data changed.

The view/schema lifecycle is now separate from row refresh:

- the constructor initializes columns and view styling once;
- explicit view-attribute changes still call `ResetView`;
- the first refresh of an uninitialized visualizer still builds the view;
- subsequent BaseEdge data refreshes preserve existing column and template instances and only replace row data.

A red STA contract captured column references across two data refreshes; the old path returned new columns, while the new path preserves every reference.

### UXVisualiser

Each hosted `UXItem` already owns a graph-change listener that updates its value and visual attributes. The parent UXVisualiser nevertheless repainted the entire canvas for the same value-only events.

The parent helper now classifies event batches:

- if every event is a `ValueChange` for a currently hosted item's data vertex or view vertex, the parent full `Paint` is skipped;
- empty, unknown, edge, add/remove/dispose, or mixed batches retain the complete existing rebuild path;
- individual UXItem listeners remain responsible for the local update.

Focused tests verify hosted value batches are suppressible and unknown vertices are not.

### FormVisualiser

The current production code already contained the intended optimization: `FormVertexChange` ignores value-only changes of the BaseEdge target and compares cached BaseEdge definitions and layout options before allowing a rebuild. No second competing fast path was added. This existing guard was retained and covered by the full desktop suite.

Verification:

- focused List/UX contracts: 4/4;
- full desktop suite: 8/8;
- full Release/x64 solution build: 0 errors;
- no new IDE diagnostics.

## Follow-up — collision-safe Text2Graph keyword memoization

`Text2GraphProcessing._tryIsKeyword` previously stored memoized parser results in `Dictionary<int, ...>` using only a hand-computed hash. Two distinct parser states with the same 32-bit hash therefore shared one result, potentially selecting the wrong keyword branch.

The memo now uses an immutable structural key:

- equality covers exactly the fields that the old memo hash treated as semantic inputs;
- `parentKeyword` uses reference identity;
- `keywordsFilter` uses ordinal string equality;
- hash collisions are resolved by normal dictionary equality;
- lookup uses one `TryGetValue` rather than `ContainsKey` followed by a second lookup;
- the old hash-only parameter override was removed to prevent accidental reuse.

A deterministic contract constructs two distinct parser states whose legacy polynomial hashes are equal (`start/previous = 0/397` and `1/0`). Both now coexist in the dictionary and return their own values.

Verification:

- collision contract: passed;
- isolated contracts: 124/124;
- all `Code0`–`Code14` parser/execution integration contracts: 83/83;
- full Release/x64 solution build: 0 errors;
- no new IDE diagnostics.

This is primarily a parser-correctness fix. It also removes duplicate dictionary probing, but no broad performance percentage is claimed without a dedicated Text2Graph parse benchmark.

## Follow-up — Graph2Text link traversal

Graph2Text generation repeatedly searched every import list while walking incoming edges, copied each incoming-edge collection, allocated a replacement `EdgeBase` per traversed path edge, and allocated a temporary one-element list when resolving indexed links.

The optimized traversal:

- builds a lazy reverse `imported vertex -> ordered import metas` dictionary once per generation input;
- preserves original import and meta insertion order, including multiple import metas for one vertex;
- iterates stable `InEdgesRaw` directly and keeps exact original edge references in the temporary path;
- reuses one link-string processor for the generation operation;
- handles singleton query results without allocating a temporary list;
- uses `TryGetValue` and `List.Count` on repeated dictionary/list paths.

### Golden output gate

Before changing production code, the exact generated workload output was hashed with SHA-256:

`DCCBA90691D50C4D800F47F5B75C24681259082C0035EFA83D915EAA9C70B5BF`

The integration test now asserts this hash before reparsing and also retains the existing generate -> parse -> generate equality assertion. This is the “golden output” gate: the optimized generator must emit byte-for-byte identical text, not merely output that can parse successfully.

BenchmarkDotNet (`5` iterations, `3` warmups):

- generation mean: 71.586 ms -> 56.815 ms (**20.6% faster**);
- allocation: 9.31 MB -> 9.23 MB;
- output hash remained exact.

Verification:

- golden round-trip contract: passed;
- full integration suite: 83/83;
- full Release/x64 solution build: 0 errors;
- no new IDE diagnostics.

## Final follow-up validation

After completing requested candidates 4–8:

- isolated graph contracts: **124 passed**;
- bootstrapped integration contracts: **83 passed**;
- desktop STA contracts: **8 passed**;
- total: **215 passed, 0 failed, 0 skipped**;
- full Release/x64 solution build: 0 errors;
- IDE diagnostics for every changed file: no new errors.

The final `Code0`–`Code14` diagnostic matrix parsed and executed every program with zero reported execution errors. `Code7` retained 4,669 result edges; `Code8` retained its expected scalar result shape.

True x86 is no longer an acceptance target by explicit product decision. Published x64 and ARM64 semantics are covered by the same managed tests; performance measurements in this follow-up were collected on x64, while an ARM64 performance run remains recommended.

