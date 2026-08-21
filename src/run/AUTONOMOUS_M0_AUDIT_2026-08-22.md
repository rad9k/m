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

