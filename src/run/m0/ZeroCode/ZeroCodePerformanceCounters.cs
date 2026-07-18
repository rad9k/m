using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using m0.Foundation;

namespace m0.ZeroCode
{
    public readonly record struct ZeroCodeInstructionPerformanceSnapshot(
        string InstructionType,
        long Calls,
        long InclusiveTimestampTicks,
        long ExclusiveTimestampTicks);

    public readonly record struct ZeroCodePerformanceSnapshot(
        long MontevideoInstructionCalls,
        long SequentialInstructionCalls,
        long ExecutableInstructionCalls,
        long ValueInstructionCalls,
        long EndpointCacheHits,
        long EndpointCacheMisses,
        long StackFramePushes,
        long StackFramePops,
        long MaximumStackFrameDepth,
        long NextExpressionTransitions,
        long InnerOperatorCalls,
        long InnerOperatorNestedExecutions,
        long ColonOperatorQueryCombinations,
        long ForVertexIterations,
        long ForEdgeIterations,
        long WhileIterations,
        long CreatedStacks,
        long OriginalStackEdgeBatches,
        long OriginalStackEdgesBatched,
        long StackParentFrameCacheHits,
        long StackParentFrameCacheMisses,
        IReadOnlyList<ZeroCodeInstructionPerformanceSnapshot> Instructions);

    internal readonly struct ZeroCodeInstructionTimingToken
    {
        internal ZeroCodeInstructionTimingToken(bool enabled)
        {
            Enabled = enabled;
        }

        internal bool Enabled { get; }
    }

    public static class ZeroCodePerformanceCounters
    {
        private sealed class InstructionCounter
        {
            internal long Calls;
            internal long InclusiveTimestampTicks;
            internal long ExclusiveTimestampTicks;
        }

        private struct TimingFrame
        {
            internal long StartTimestamp;
            internal long ChildTimestampTicks;
        }

        private static readonly ConcurrentDictionary<string, InstructionCounter>
            instructionCounters =
                new ConcurrentDictionary<string, InstructionCounter>(
                    StringComparer.Ordinal);

        [ThreadStatic]
        private static List<TimingFrame> timingFrames;

        private static int enabled;
        private static long montevideoInstructionCalls;
        private static long sequentialInstructionCalls;
        private static long executableInstructionCalls;
        private static long valueInstructionCalls;
        private static long endpointCacheHits;
        private static long endpointCacheMisses;
        private static long stackFramePushes;
        private static long stackFramePops;
        private static long maximumStackFrameDepth;
        private static long nextExpressionTransitions;
        private static long innerOperatorCalls;
        private static long innerOperatorNestedExecutions;
        private static long colonOperatorQueryCombinations;
        private static long forVertexIterations;
        private static long forEdgeIterations;
        private static long whileIterations;
        private static long createdStacks;
        private static long originalStackEdgeBatches;
        private static long originalStackEdgesBatched;
        private static long stackParentFrameCacheHits;
        private static long stackParentFrameCacheMisses;

        public static bool Enabled
        {
            get => Volatile.Read(ref enabled) != 0;
            set => Volatile.Write(ref enabled, value ? 1 : 0);
        }

        public static void Reset()
        {
            instructionCounters.Clear();
            Interlocked.Exchange(ref montevideoInstructionCalls, 0);
            Interlocked.Exchange(ref sequentialInstructionCalls, 0);
            Interlocked.Exchange(ref executableInstructionCalls, 0);
            Interlocked.Exchange(ref valueInstructionCalls, 0);
            Interlocked.Exchange(ref endpointCacheHits, 0);
            Interlocked.Exchange(ref endpointCacheMisses, 0);
            Interlocked.Exchange(ref stackFramePushes, 0);
            Interlocked.Exchange(ref stackFramePops, 0);
            Interlocked.Exchange(ref maximumStackFrameDepth, 0);
            Interlocked.Exchange(ref nextExpressionTransitions, 0);
            Interlocked.Exchange(ref innerOperatorCalls, 0);
            Interlocked.Exchange(ref innerOperatorNestedExecutions, 0);
            Interlocked.Exchange(ref colonOperatorQueryCombinations, 0);
            Interlocked.Exchange(ref forVertexIterations, 0);
            Interlocked.Exchange(ref forEdgeIterations, 0);
            Interlocked.Exchange(ref whileIterations, 0);
            Interlocked.Exchange(ref createdStacks, 0);
            Interlocked.Exchange(ref originalStackEdgeBatches, 0);
            Interlocked.Exchange(ref originalStackEdgesBatched, 0);
            Interlocked.Exchange(ref stackParentFrameCacheHits, 0);
            Interlocked.Exchange(ref stackParentFrameCacheMisses, 0);
            timingFrames?.Clear();
        }

        public static ZeroCodePerformanceSnapshot GetSnapshot()
        {
            ZeroCodeInstructionPerformanceSnapshot[] instructions =
                instructionCounters
                    .Select(
                        pair =>
                            new ZeroCodeInstructionPerformanceSnapshot(
                                pair.Key,
                                Volatile.Read(ref pair.Value.Calls),
                                Volatile.Read(
                                    ref pair.Value.InclusiveTimestampTicks),
                                Volatile.Read(
                                    ref pair.Value.ExclusiveTimestampTicks)))
                    .OrderByDescending(
                        instruction => instruction.ExclusiveTimestampTicks)
                    .ThenBy(
                        instruction => instruction.InstructionType,
                        StringComparer.Ordinal)
                    .ToArray();

            return new ZeroCodePerformanceSnapshot(
                Volatile.Read(ref montevideoInstructionCalls),
                Volatile.Read(ref sequentialInstructionCalls),
                Volatile.Read(ref executableInstructionCalls),
                Volatile.Read(ref valueInstructionCalls),
                Volatile.Read(ref endpointCacheHits),
                Volatile.Read(ref endpointCacheMisses),
                Volatile.Read(ref stackFramePushes),
                Volatile.Read(ref stackFramePops),
                Volatile.Read(ref maximumStackFrameDepth),
                Volatile.Read(ref nextExpressionTransitions),
                Volatile.Read(ref innerOperatorCalls),
                Volatile.Read(ref innerOperatorNestedExecutions),
                Volatile.Read(ref colonOperatorQueryCombinations),
                Volatile.Read(ref forVertexIterations),
                Volatile.Read(ref forEdgeIterations),
                Volatile.Read(ref whileIterations),
                Volatile.Read(ref createdStacks),
                Volatile.Read(ref originalStackEdgeBatches),
                Volatile.Read(ref originalStackEdgesBatched),
                Volatile.Read(ref stackParentFrameCacheHits),
                Volatile.Read(ref stackParentFrameCacheMisses),
                instructions);
        }

        public static string FormatReport(
            ZeroCodePerformanceSnapshot snapshot,
            int maximumInstructionRows = 30)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("ZeroCode performance diagnostics");
            report.Append("instructions: montevideo=")
                .Append(snapshot.MontevideoInstructionCalls)
                .Append(", sequential=")
                .Append(snapshot.SequentialInstructionCalls)
                .Append(", executable=")
                .Append(snapshot.ExecutableInstructionCalls)
                .Append(", values=")
                .AppendLine(snapshot.ValueInstructionCalls.ToString());
            report.Append("endpoint cache: hits=")
                .Append(snapshot.EndpointCacheHits)
                .Append(", misses=")
                .AppendLine(snapshot.EndpointCacheMisses.ToString());
            report.Append("stack frames: pushes=")
                .Append(snapshot.StackFramePushes)
                .Append(", pops=")
                .Append(snapshot.StackFramePops)
                .Append(", max-depth=")
                .AppendLine(snapshot.MaximumStackFrameDepth.ToString());
            report.Append("control flow: next=")
                .Append(snapshot.NextExpressionTransitions)
                .Append(", for-vertex=")
                .Append(snapshot.ForVertexIterations)
                .Append(", for-edge=")
                .Append(snapshot.ForEdgeIterations)
                .Append(", while=")
                .AppendLine(snapshot.WhileIterations.ToString());
            report.Append("amplification: inner-calls=")
                .Append(snapshot.InnerOperatorCalls)
                .Append(", inner-nested=")
                .Append(snapshot.InnerOperatorNestedExecutions)
                .Append(", colon-query-combinations=")
                .AppendLine(
                    snapshot.ColonOperatorQueryCombinations.ToString());
            report.Append("temporary stacks: created=")
                .Append(snapshot.CreatedStacks)
                .Append(", original-edge-batches=")
                .Append(snapshot.OriginalStackEdgeBatches)
                .Append(", original-edges-batched=")
                .AppendLine(
                    snapshot.OriginalStackEdgesBatched.ToString());
            report.Append("stack parent cache: hits=")
                .Append(snapshot.StackParentFrameCacheHits)
                .Append(", misses=")
                .AppendLine(
                    snapshot.StackParentFrameCacheMisses.ToString());
            report.AppendLine(
                "instruction type | calls | self ms | inclusive ms");

            foreach (ZeroCodeInstructionPerformanceSnapshot instruction in
                snapshot.Instructions.Take(maximumInstructionRows))
            {
                report.Append(instruction.InstructionType)
                    .Append(" | ")
                    .Append(instruction.Calls)
                    .Append(" | ")
                    .Append(
                        TimestampTicksToMilliseconds(
                            instruction.ExclusiveTimestampTicks)
                            .ToString("F3"))
                    .Append(" | ")
                    .AppendLine(
                        TimestampTicksToMilliseconds(
                            instruction.InclusiveTimestampTicks)
                            .ToString("F3"));
            }

            return report.ToString();
        }

        public static void LogReport(
            ZeroCodePerformanceSnapshot snapshot,
            int maximumInstructionRows = 30)
        {
            MinusZero.Instance.Log(
                1,
                "ZeroCodePerformance",
                FormatReport(
                    snapshot,
                    maximumInstructionRows));
        }

        internal static ZeroCodeInstructionTimingToken BeginInstruction(
            bool montevideo)
        {
            if (!Enabled)
                return default;

            if (montevideo)
                Interlocked.Increment(ref montevideoInstructionCalls);
            else
                Interlocked.Increment(ref sequentialInstructionCalls);

            List<TimingFrame> frames =
                timingFrames ??= new List<TimingFrame>(16);
            frames.Add(
                new TimingFrame
                {
                    StartTimestamp = Stopwatch.GetTimestamp()
                });
            return new ZeroCodeInstructionTimingToken(true);
        }

        internal static void EndInstruction(
            ZeroCodeInstructionTimingToken token,
            IVertex instructionType)
        {
            if (!token.Enabled)
                return;

            List<TimingFrame> frames = timingFrames;
            if (frames == null || frames.Count == 0)
                return;

            int frameIndex = frames.Count - 1;
            TimingFrame frame = frames[frameIndex];
            frames.RemoveAt(frameIndex);
            long elapsedTimestampTicks =
                Stopwatch.GetTimestamp() - frame.StartTimestamp;
            long exclusiveTimestampTicks =
                Math.Max(
                    0,
                    elapsedTimestampTicks - frame.ChildTimestampTicks);

            if (frames.Count > 0)
            {
                int parentIndex = frames.Count - 1;
                TimingFrame parentFrame = frames[parentIndex];
                parentFrame.ChildTimestampTicks += elapsedTimestampTicks;
                frames[parentIndex] = parentFrame;
            }

            string instructionTypeName =
                instructionType?.Value?.ToString() ?? "<value>";
            InstructionCounter counter =
                instructionCounters.GetOrAdd(
                    instructionTypeName,
                    _ => new InstructionCounter());
            Interlocked.Increment(ref counter.Calls);
            Interlocked.Add(
                ref counter.InclusiveTimestampTicks,
                elapsedTimestampTicks);
            Interlocked.Add(
                ref counter.ExclusiveTimestampTicks,
                exclusiveTimestampTicks);
        }

        internal static void RecordInstructionResolution(bool executable)
        {
            if (!Enabled)
                return;

            if (executable)
                Interlocked.Increment(ref executableInstructionCalls);
            else
                Interlocked.Increment(ref valueInstructionCalls);
        }

        internal static void RecordEndpointCacheLookup(bool hit)
        {
            if (!Enabled)
                return;

            if (hit)
                Interlocked.Increment(ref endpointCacheHits);
            else
                Interlocked.Increment(ref endpointCacheMisses);
        }

        internal static void RecordStackFramePush(int depth)
        {
            if (!Enabled)
                return;

            Interlocked.Increment(ref stackFramePushes);
            UpdateMaximum(ref maximumStackFrameDepth, depth);
        }

        internal static void RecordStackFramePop()
        {
            if (Enabled)
                Interlocked.Increment(ref stackFramePops);
        }

        internal static void RecordNextExpressionTransition()
        {
            if (Enabled)
                Interlocked.Increment(ref nextExpressionTransitions);
        }

        internal static void RecordInnerOperator(long nestedExecutions)
        {
            if (!Enabled)
                return;

            Interlocked.Increment(ref innerOperatorCalls);
            Interlocked.Add(
                ref innerOperatorNestedExecutions,
                nestedExecutions);
        }

        internal static void RecordColonOperatorQueryCombinations(
            long queryCombinations)
        {
            if (Enabled)
                Interlocked.Add(
                    ref colonOperatorQueryCombinations,
                    queryCombinations);
        }

        internal static void RecordForVertexIteration()
        {
            if (Enabled)
                Interlocked.Increment(ref forVertexIterations);
        }

        internal static void RecordForEdgeIteration()
        {
            if (Enabled)
                Interlocked.Increment(ref forEdgeIterations);
        }

        internal static void RecordWhileIteration()
        {
            if (Enabled)
                Interlocked.Increment(ref whileIterations);
        }

        internal static void RecordStackCreated()
        {
            if (Enabled)
                Interlocked.Increment(ref createdStacks);
        }

        internal static void RecordOriginalStackEdgeBatch(int edgeCount)
        {
            if (!Enabled)
                return;

            Interlocked.Increment(ref originalStackEdgeBatches);
            Interlocked.Add(ref originalStackEdgesBatched, edgeCount);
        }

        internal static void RecordStackParentFrameCacheLookup(bool hit)
        {
            if (!Enabled)
                return;

            if (hit)
                Interlocked.Increment(ref stackParentFrameCacheHits);
            else
                Interlocked.Increment(ref stackParentFrameCacheMisses);
        }

        private static double TimestampTicksToMilliseconds(long ticks)
        {
            return ticks * 1000.0 / Stopwatch.Frequency;
        }

        private static void UpdateMaximum(ref long target, long value)
        {
            long current = Volatile.Read(ref target);

            while (value > current)
            {
                long observed =
                    Interlocked.CompareExchange(
                        ref target,
                        value,
                        current);
                if (observed == current)
                    return;

                current = observed;
            }
        }
    }
}
