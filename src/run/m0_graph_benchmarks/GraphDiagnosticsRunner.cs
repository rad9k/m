using System.Text.Json;
using System.Diagnostics;
using m0;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Foundation;
using m0.Store;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

internal static class GraphDiagnosticsRunner
{
    public static void RunStackLifecycle()
    {
        const int createdStackCount = 10000;
        const int activeFrameDepth = 10;

        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();

        var tempStore = (m0.Store.StoreBase)
            MinusZero.Instance.TempStore;
        var initialStoreVertexCount =
            tempStore.VertexIdentifiersDictionary.Count;

        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        var execution = new ZeroCodeExecution();

        for (var depth = 0;
             depth < activeFrameDepth;
             depth++)
        {
            execution.AddStackFrame();
        }

        var reachableActiveStackCount =
            CountReachableStackFrames(execution.Stack);

        for (var index = 0;
             index < createdStackCount;
             index++)
        {
            _ = InstructionHelpers.CreateStack();
        }

        GraphPerformanceCounters.Enabled = false;
        var finalStoreVertexCount =
            tempStore.VertexIdentifiersDictionary.Count;
        var snapshot = GraphPerformanceCounters.GetSnapshot();
        var report = new
        {
            Workload = new
            {
                CreatedStackCount = createdStackCount,
                ActiveFrameDepth = activeFrameDepth
            },
            TempStore = new
            {
                Before = initialStoreVertexCount,
                After = finalStoreVertexCount,
                Growth =
                    finalStoreVertexCount -
                    initialStoreVertexCount
            },
            ReachableActiveStacks =
                reachableActiveStackCount,
            snapshot.CreatedStacks,
            snapshot.CreatedTempStoreStacks
        };

        Console.WriteLine(JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void RunQueryParseCache()
    {
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        EasyVertex.ResetQueryParseCaches();
        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        MinusZero.Instance.Initialize();

        GraphPerformanceCounters.Enabled = false;
        var snapshot = GraphPerformanceCounters.GetSnapshot();
        var lookupCount =
            snapshot.QueryParseCacheHits +
            snapshot.QueryParseCacheMisses;
        var report = new
        {
            Workload = "Controlled application bootstrap",
            RegularEntries =
                EasyVertex.QueryParseCacheEntryCount,
            RegularCapacity =
                EasyVertex.QueryParseCacheCapacity,
            MetaModeEntries =
                EasyVertex.MetaQueryParseCacheEntryCount,
            MetaModeCapacity =
                EasyVertex.MetaQueryParseCacheCapacity,
            snapshot.QueryParseCacheHits,
            snapshot.QueryParseCacheMisses,
            HitRate = lookupCount == 0
                ? 0
                : (double)snapshot.QueryParseCacheHits /
                  lookupCount,
            snapshot.MaximumQueryParseCacheSize
        };

        Console.WriteLine(JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void RunTransactionCoalescing()
    {
        const int mutationCount = 10;

        var fixture = new GraphFixture();
        var vertex = fixture.CreateVertex("Before");
        var execution = new GraphTestExecution(fixture);
        var previousTransaction =
            MinusZero.Instance.GetTopTransaction();
        var transaction =
            new Transaction(previousTransaction);
        transaction.Start();
        MinusZero.Instance.SetTopTransaction(transaction);

        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        GraphPerformanceSnapshot snapshot;
        var listenerChangeCount = 0;
        try
        {
            for (var index = 1;
                 index <= mutationCount;
                 index++)
            {
                vertex.Value = $"Value-{index}";
            }

            listenerChangeCount = transaction
                .graphChangeTransactionAtoms_OutEdgeValueChange
                .Values
                .Sum(changes => changes.Count);
            snapshot =
                GraphPerformanceCounters.GetSnapshot();

            transaction.Rollback(execution);
        }
        finally
        {
            GraphPerformanceCounters.Enabled = false;
            MinusZero.Instance.SetTopTransaction(
                previousTransaction);
        }

        var report = new
        {
            Workload = new
            {
                RepeatedValueMutations = mutationCount
            },
            snapshot.RollbackJournalAtoms,
            snapshot.CoalescedGraphChangeAtoms,
            FinalListenerChangeSetEntries =
                listenerChangeCount,
            ValueAfterRollback = vertex.Value
        };

        Console.WriteLine(JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void RunWatcherPreparation()
    {
        const int iterationCount = 100;

        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();

        _ = GraphChangeTriggerWatcher
            .GetWatchedVertexDictionary();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long allocatedBefore =
            GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        Dictionary<IVertex, List<WatcherEntry>>
            watchedVertexDictionary = null!;

        for (var index = 0;
             index < iterationCount;
             index++)
        {
            watchedVertexDictionary =
                GraphChangeTriggerWatcher
                    .GetWatchedVertexDictionary();
        }

        stopwatch.Stop();
        long allocatedBytes =
            GC.GetAllocatedBytesForCurrentThread() -
            allocatedBefore;
        int watcherMappings =
            watchedVertexDictionary.Values.Sum(
                entries => entries.Count);
        var report = new
        {
            Workload = new
            {
                Iterations = iterationCount,
                GraphChangeTriggerWatcher.TriggerCount
            },
            LastResult = new
            {
                WatchedVertices =
                    watchedVertexDictionary.Count,
                WatcherMappings = watcherMappings
            },
            TotalElapsedMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds,
            MeanMicroseconds =
                stopwatch.Elapsed.TotalMicroseconds /
                iterationCount,
            AllocatedBytes = allocatedBytes,
            AllocatedBytesPerIteration =
                allocatedBytes / iterationCount
        };

        Console.WriteLine(JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void RunDetachAttachPhases()
    {
        var reports = new List<object>();

        foreach (var edgeCount in
            new[] { 1, 100, 1000 })
        {
            int iterationCount = edgeCount switch
            {
                1 => 10000,
                100 => 1000,
                _ => 100
            };
            var identifierSuffix =
                Guid.NewGuid().ToString("N");
            var accessLevels = new[]
            {
                AccessLevelEnum.NoRestrictions
            };
            var sourceStore = new MemoryStore(
                $"diagnostic-source-{identifierSuffix}",
                MinusZero.Instance,
                accessLevels,
                true);
            var targetStore = new MemoryStore(
                $"diagnostic-target-{identifierSuffix}",
                MinusZero.Instance,
                accessLevels,
                true);
            var meta = new EasyVertex(sourceStore)
            {
                Value = "Meta"
            };
            var target = new EasyVertex(targetStore)
            {
                Value = "Target"
            };

            for (var index = 0;
                 index < edgeCount;
                 index++)
            {
                var source = new EasyVertex(sourceStore)
                {
                    Value = $"Source-{index}"
                };
                source.AddEdge(meta, target);
            }

            for (var index = 0; index < 10; index++)
            {
                sourceStore.Detach();
                sourceStore.Attach();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long detachTimestampTicks = 0;
            long attachTimestampTicks = 0;
            long detachAllocatedBytes = 0;
            long attachAllocatedBytes = 0;

            for (var index = 0;
                 index < iterationCount;
                 index++)
            {
                long allocatedBefore =
                    GC.GetAllocatedBytesForCurrentThread();
                long timestampBefore =
                    Stopwatch.GetTimestamp();
                sourceStore.Detach();
                detachTimestampTicks +=
                    Stopwatch.GetTimestamp() -
                    timestampBefore;
                detachAllocatedBytes +=
                    GC.GetAllocatedBytesForCurrentThread() -
                    allocatedBefore;

                allocatedBefore =
                    GC.GetAllocatedBytesForCurrentThread();
                timestampBefore =
                    Stopwatch.GetTimestamp();
                sourceStore.Attach();
                attachTimestampTicks +=
                    Stopwatch.GetTimestamp() -
                    timestampBefore;
                attachAllocatedBytes +=
                    GC.GetAllocatedBytesForCurrentThread() -
                    allocatedBefore;
            }

            reports.Add(new
            {
                EdgeCount = edgeCount,
                Iterations = iterationCount,
                StoreCount =
                    MinusZero.Instance.Stores.Count,
                Detach = new
                {
                    MeanMicroseconds =
                        TicksToMeanMicroseconds(
                            detachTimestampTicks,
                            iterationCount),
                    AllocatedBytesPerOperation =
                        detachAllocatedBytes /
                        iterationCount
                },
                Attach = new
                {
                    MeanMicroseconds =
                        TicksToMeanMicroseconds(
                            attachTimestampTicks,
                            iterationCount),
                    AllocatedBytesPerOperation =
                        attachAllocatedBytes /
                        iterationCount
                },
                FinalIncomingEdgeCount =
                    target.InEdgesRaw.Count
            });

            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
        }

        Console.WriteLine(JsonSerializer.Serialize(
            reports,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    private static double TicksToMeanMicroseconds(
        long timestampTicks,
        int operationCount)
    {
        return timestampTicks * 1000000d /
            Stopwatch.Frequency /
            operationCount;
    }

    public static void Run()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var edges = fixture.AddEdges(source, meta, 1000);
        var changedTarget = edges[0].To;

        var hierarchyParent = fixture.CreateVertex("HierarchyParent");
        var hierarchyChildren = fixture.CreateInheritanceChildren(hierarchyParent, 100);
        var hierarchyMeta = fixture.CreateVertex("HierarchyMeta");
        var hierarchyTarget = fixture.CreateVertex("HierarchyTarget");

        var deepHierarchy = fixture.CreateInheritanceChain(25, "Deep");
        deepHierarchy[0].AddEdge(hierarchyMeta, hierarchyTarget);

        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        Query(source, "Meta", null);
        Query(source, "Meta", "Target-0");
        changedTarget.Value = "ChangedTarget";
        Query(source, "Meta", "ChangedTarget");

        var hierarchyEdge = hierarchyParent.AddEdge(hierarchyMeta, hierarchyTarget);

        foreach (var child in hierarchyChildren)
            Query(child, "HierarchyMeta", null);

        Query(deepHierarchy[^1], "HierarchyMeta", null);
        hierarchyParent.DeleteEdge(hierarchyEdge);

        for (var index = 0; index < 100; index++)
            _ = new NoInEdgeInOutVertexVertex(fixture.Store);

        RecordRollbackWorkload(fixture);

        GraphPerformanceCounters.Enabled = false;
        var snapshot = GraphPerformanceCounters.GetSnapshot();
        var report = new
        {
            Workload = new
            {
                DirectEdgeCount = 1000,
                WideHierarchyChildCount = 100,
                DeepHierarchyDepth = 25,
                CreatedStackCount = 100
            },
            Counters = snapshot,
            CacheSizes = new
            {
                ValueIndexEntries = source.OutEdgesByValue?.Count ?? 0,
                MetaAndValueIndexEntries = source.OutEdgesByMetaAndValue?.Count ?? 0,
                StoreVertexCount = fixture.Store.VertexIdentifiersDictionary.Count
            }
        };

        Console.WriteLine(JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RecordRollbackWorkload(GraphFixture fixture)
    {
        var execution = new GraphTestExecution(fixture);
        var vertex = fixture.CreateVertex("BeforeTransaction");
        var previousTransaction = MinusZero.Instance.GetTopTransaction();
        var transaction = new Transaction(previousTransaction);
        transaction.Start();
        MinusZero.Instance.SetTopTransaction(transaction);

        try
        {
            vertex.Value = "DuringTransaction";
            transaction.Rollback(execution);
        }
        finally
        {
            MinusZero.Instance.SetTopTransaction(previousTransaction);
        }
    }

    private static int Query(m0.Foundation.IVertex vertex, object meta, object? value)
    {
        vertex.QueryOutEdges(meta, value!, out var result, out var results);
        return result != null ? 1 : results?.Count ?? 0;
    }

    private static int CountReachableStackFrames(
        IVertex stack)
    {
        var visited = new HashSet<IVertex>();
        var current = stack;

        while (current is INoInEdgeInOutVertexVertex &&
               visited.Add(current))
        {
            var parentEdge =
                GraphUtil.GetQueryOutFirstEdge(
                    current,
                    "$StackFrameInherits",
                    null);

            if (parentEdge?.To is not
                INoInEdgeInOutVertexVertex parentStack)
            {
                break;
            }

            current = parentStack;
        }

        return visited.Count;
    }
}
