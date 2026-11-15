using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using m0.Foundation;

namespace m0.Graph
{
    public enum GraphScaleListenerChannel
    {
        Out,
        In,
        Meta
    }

    public readonly struct GraphScaleSnapshot
    {
        public GraphScaleSnapshot(
            long coalesceOutCalls,
            long coalesceOutSteps,
            long coalesceOutMaxList,
            long coalesceInCalls,
            long coalesceInSteps,
            long coalesceInMaxList,
            long coalesceMetaCalls,
            long coalesceMetaSteps,
            long coalesceMetaMaxList,
            long addEdgeCalls,
            long outIndexRebuilds,
            long outIndexRebuildEdges,
            long inIndexRebuilds,
            long inIndexRebuildEdges,
            long plusLessCalls,
            long plusLessFanout,
            long plusLessMaxDestinationOut,
            long plusLessMaxMetaIn,
            long newVertexOperations,
            long linkOperations,
            long linkSameTargetCount,
            long linkIncoming0,
            long linkIncoming1,
            long linkIncoming2To15,
            long linkIncoming16To255,
            long linkIncoming256To4095,
            long linkIncoming4096OrMore)
        {
            CoalesceOutCalls = coalesceOutCalls;
            CoalesceOutSteps = coalesceOutSteps;
            CoalesceOutMaxList = coalesceOutMaxList;
            CoalesceInCalls = coalesceInCalls;
            CoalesceInSteps = coalesceInSteps;
            CoalesceInMaxList = coalesceInMaxList;
            CoalesceMetaCalls = coalesceMetaCalls;
            CoalesceMetaSteps = coalesceMetaSteps;
            CoalesceMetaMaxList = coalesceMetaMaxList;
            AddEdgeCalls = addEdgeCalls;
            OutIndexRebuilds = outIndexRebuilds;
            OutIndexRebuildEdges = outIndexRebuildEdges;
            InIndexRebuilds = inIndexRebuilds;
            InIndexRebuildEdges = inIndexRebuildEdges;
            PlusLessCalls = plusLessCalls;
            PlusLessFanout = plusLessFanout;
            PlusLessMaxDestinationOut = plusLessMaxDestinationOut;
            PlusLessMaxMetaIn = plusLessMaxMetaIn;
            NewVertexOperations = newVertexOperations;
            LinkOperations = linkOperations;
            LinkSameTargetCount = linkSameTargetCount;
            LinkIncoming0 = linkIncoming0;
            LinkIncoming1 = linkIncoming1;
            LinkIncoming2To15 = linkIncoming2To15;
            LinkIncoming16To255 = linkIncoming16To255;
            LinkIncoming256To4095 = linkIncoming256To4095;
            LinkIncoming4096OrMore = linkIncoming4096OrMore;
        }

        public long CoalesceOutCalls { get; }
        public long CoalesceOutSteps { get; }
        public long CoalesceOutMaxList { get; }
        public long CoalesceInCalls { get; }
        public long CoalesceInSteps { get; }
        public long CoalesceInMaxList { get; }
        public long CoalesceMetaCalls { get; }
        public long CoalesceMetaSteps { get; }
        public long CoalesceMetaMaxList { get; }
        public long AddEdgeCalls { get; }
        public long OutIndexRebuilds { get; }
        public long OutIndexRebuildEdges { get; }
        public long InIndexRebuilds { get; }
        public long InIndexRebuildEdges { get; }
        public long PlusLessCalls { get; }
        public long PlusLessFanout { get; }
        public long PlusLessMaxDestinationOut { get; }
        public long PlusLessMaxMetaIn { get; }
        public long NewVertexOperations { get; }
        public long LinkOperations { get; }
        public long LinkSameTargetCount { get; }
        public long LinkIncoming0 { get; }
        public long LinkIncoming1 { get; }
        public long LinkIncoming2To15 { get; }
        public long LinkIncoming16To255 { get; }
        public long LinkIncoming256To4095 { get; }
        public long LinkIncoming4096OrMore { get; }

        public long CoalesceSteps =>
            CoalesceOutSteps + CoalesceInSteps + CoalesceMetaSteps;
    }

    public static class GraphScaleDiagnostics
    {
        private static int enabled;
        private static long coalesceOutCalls;
        private static long coalesceOutSteps;
        private static long coalesceOutMaxList;
        private static long coalesceInCalls;
        private static long coalesceInSteps;
        private static long coalesceInMaxList;
        private static long coalesceMetaCalls;
        private static long coalesceMetaSteps;
        private static long coalesceMetaMaxList;
        private static long addEdgeCalls;
        private static long outIndexRebuilds;
        private static long outIndexRebuildEdges;
        private static long inIndexRebuilds;
        private static long inIndexRebuildEdges;
        private static long plusLessCalls;
        private static long plusLessFanout;
        private static long plusLessMaxDestinationOut;
        private static long plusLessMaxMetaIn;
        private static long newVertexOperations;
        private static long linkOperations;
        private static long linkSameTargetCount;
        private static long linkIncoming0;
        private static long linkIncoming1;
        private static long linkIncoming2To15;
        private static long linkIncoming16To255;
        private static long linkIncoming256To4095;
        private static long linkIncoming4096OrMore;
        private static long coalesceOutTicks;
        private static long coalesceInTicks;
        private static long coalesceMetaTicks;
        private static long outIndexTicks;
        private static long inIndexTicks;
        private static long triggerFromCalls;
        private static long triggerFromTicks;
        private static long triggerFromMaxIn;
        private static long triggerValueCalls;
        private static long triggerValueTicks;
        private static long triggerValueMaxIn;
        private static long triggerMetaRebuilds;
        private static long triggerMetaRebuildTicks;
        private static long otherMetaRebuilds;
        private static long otherMetaRebuildTicks;
        private static long largeInRebuildCount;
        private static int repeatedTargetInCount;
        private static string repeatedTargetValue;
        private static string repeatedTargetMetaValue;
        private static string dominantInRebuildCaller;

        private static readonly ConcurrentDictionary<string, long>
            inRebuildCallers =
                new ConcurrentDictionary<string, long>(
                    StringComparer.Ordinal);

        [ThreadStatic]
        private static IVertex previousLinkTarget;

        public static bool Enabled
        {
            get => Volatile.Read(ref enabled) != 0;
            set => Volatile.Write(ref enabled, value ? 1 : 0);
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref coalesceOutCalls, 0);
            Interlocked.Exchange(ref coalesceOutSteps, 0);
            Interlocked.Exchange(ref coalesceOutMaxList, 0);
            Interlocked.Exchange(ref coalesceInCalls, 0);
            Interlocked.Exchange(ref coalesceInSteps, 0);
            Interlocked.Exchange(ref coalesceInMaxList, 0);
            Interlocked.Exchange(ref coalesceMetaCalls, 0);
            Interlocked.Exchange(ref coalesceMetaSteps, 0);
            Interlocked.Exchange(ref coalesceMetaMaxList, 0);
            Interlocked.Exchange(ref addEdgeCalls, 0);
            Interlocked.Exchange(ref outIndexRebuilds, 0);
            Interlocked.Exchange(ref outIndexRebuildEdges, 0);
            Interlocked.Exchange(ref inIndexRebuilds, 0);
            Interlocked.Exchange(ref inIndexRebuildEdges, 0);
            Interlocked.Exchange(ref plusLessCalls, 0);
            Interlocked.Exchange(ref plusLessFanout, 0);
            Interlocked.Exchange(ref plusLessMaxDestinationOut, 0);
            Interlocked.Exchange(ref plusLessMaxMetaIn, 0);
            Interlocked.Exchange(ref newVertexOperations, 0);
            Interlocked.Exchange(ref linkOperations, 0);
            Interlocked.Exchange(ref linkSameTargetCount, 0);
            Interlocked.Exchange(ref linkIncoming0, 0);
            Interlocked.Exchange(ref linkIncoming1, 0);
            Interlocked.Exchange(ref linkIncoming2To15, 0);
            Interlocked.Exchange(ref linkIncoming16To255, 0);
            Interlocked.Exchange(ref linkIncoming256To4095, 0);
            Interlocked.Exchange(ref linkIncoming4096OrMore, 0);
            Interlocked.Exchange(ref coalesceOutTicks, 0);
            Interlocked.Exchange(ref coalesceInTicks, 0);
            Interlocked.Exchange(ref coalesceMetaTicks, 0);
            Interlocked.Exchange(ref outIndexTicks, 0);
            Interlocked.Exchange(ref inIndexTicks, 0);
            Interlocked.Exchange(ref triggerFromCalls, 0);
            Interlocked.Exchange(ref triggerFromTicks, 0);
            Interlocked.Exchange(ref triggerFromMaxIn, 0);
            Interlocked.Exchange(ref triggerValueCalls, 0);
            Interlocked.Exchange(ref triggerValueTicks, 0);
            Interlocked.Exchange(ref triggerValueMaxIn, 0);
            Interlocked.Exchange(ref triggerMetaRebuilds, 0);
            Interlocked.Exchange(ref triggerMetaRebuildTicks, 0);
            Interlocked.Exchange(ref otherMetaRebuilds, 0);
            Interlocked.Exchange(ref otherMetaRebuildTicks, 0);
            Interlocked.Exchange(ref largeInRebuildCount, 0);
            repeatedTargetInCount = 0;
            repeatedTargetValue = null;
            repeatedTargetMetaValue = null;
            dominantInRebuildCaller = null;
            inRebuildCallers.Clear();
            previousLinkTarget = null;
        }

        public static GraphScaleSnapshot GetSnapshot()
        {
            return new GraphScaleSnapshot(
                Volatile.Read(ref coalesceOutCalls),
                Volatile.Read(ref coalesceOutSteps),
                Volatile.Read(ref coalesceOutMaxList),
                Volatile.Read(ref coalesceInCalls),
                Volatile.Read(ref coalesceInSteps),
                Volatile.Read(ref coalesceInMaxList),
                Volatile.Read(ref coalesceMetaCalls),
                Volatile.Read(ref coalesceMetaSteps),
                Volatile.Read(ref coalesceMetaMaxList),
                Volatile.Read(ref addEdgeCalls),
                Volatile.Read(ref outIndexRebuilds),
                Volatile.Read(ref outIndexRebuildEdges),
                Volatile.Read(ref inIndexRebuilds),
                Volatile.Read(ref inIndexRebuildEdges),
                Volatile.Read(ref plusLessCalls),
                Volatile.Read(ref plusLessFanout),
                Volatile.Read(ref plusLessMaxDestinationOut),
                Volatile.Read(ref plusLessMaxMetaIn),
                Volatile.Read(ref newVertexOperations),
                Volatile.Read(ref linkOperations),
                Volatile.Read(ref linkSameTargetCount),
                Volatile.Read(ref linkIncoming0),
                Volatile.Read(ref linkIncoming1),
                Volatile.Read(ref linkIncoming2To15),
                Volatile.Read(ref linkIncoming16To255),
                Volatile.Read(ref linkIncoming256To4095),
                Volatile.Read(ref linkIncoming4096OrMore));
        }

        public static string FormatReport(GraphScaleSnapshot snapshot)
        {
            StringBuilder report = new StringBuilder();
            report.Append("coalesce out calls=")
                .Append(snapshot.CoalesceOutCalls)
                .Append(" steps=")
                .Append(snapshot.CoalesceOutSteps)
                .Append(" max=")
                .Append(snapshot.CoalesceOutMaxList);
            report.Append(" | in calls=")
                .Append(snapshot.CoalesceInCalls)
                .Append(" steps=")
                .Append(snapshot.CoalesceInSteps)
                .Append(" max=")
                .Append(snapshot.CoalesceInMaxList);
            report.Append(" | meta calls=")
                .Append(snapshot.CoalesceMetaCalls)
                .Append(" steps=")
                .Append(snapshot.CoalesceMetaSteps)
                .Append(" max=")
                .AppendLine(snapshot.CoalesceMetaMaxList.ToString());
            report.Append("addEdge=")
                .Append(snapshot.AddEdgeCalls)
                .Append(" outRebuilds=")
                .Append(snapshot.OutIndexRebuilds)
                .Append(" outEdgesScanned=")
                .Append(snapshot.OutIndexRebuildEdges)
                .Append(" inRebuilds=")
                .Append(snapshot.InIndexRebuilds)
                .Append(" inEdgesScanned=")
                .AppendLine(snapshot.InIndexRebuildEdges.ToString());
            report.Append("plusLess calls=")
                .Append(snapshot.PlusLessCalls)
                .Append(" fanout=")
                .Append(snapshot.PlusLessFanout)
                .Append(" maxDestinationOut=")
                .Append(snapshot.PlusLessMaxDestinationOut)
                .Append(" maxMetaIn=")
                .AppendLine(snapshot.PlusLessMaxMetaIn.ToString());
            report.Append("newVertex=")
                .Append(snapshot.NewVertexOperations)
                .Append(" link=")
                .Append(snapshot.LinkOperations)
                .Append(" linkSameTarget=")
                .Append(snapshot.LinkSameTargetCount)
                .Append(" linkIncomingBuckets 0=")
                .Append(snapshot.LinkIncoming0)
                .Append(" 1=")
                .Append(snapshot.LinkIncoming1)
                .Append(" 2-15=")
                .Append(snapshot.LinkIncoming2To15)
                .Append(" 16-255=")
                .Append(snapshot.LinkIncoming16To255)
                .Append(" 256-4095=")
                .Append(snapshot.LinkIncoming256To4095)
                .Append(" 4096+=")
                .AppendLine(snapshot.LinkIncoming4096OrMore.ToString());
            report.Append("timeMs coalesceOut=")
                .Append(TicksToMilliseconds(coalesceOutTicks).ToString("F1"))
                .Append(" coalesceIn=")
                .Append(TicksToMilliseconds(coalesceInTicks).ToString("F1"))
                .Append(" coalesceMeta=")
                .Append(TicksToMilliseconds(coalesceMetaTicks).ToString("F1"))
                .Append(" outIndex=")
                .Append(TicksToMilliseconds(outIndexTicks).ToString("F1"))
                .Append(" inIndex=")
                .AppendLine(TicksToMilliseconds(inIndexTicks).ToString("F1"));
            report.Append("repeatedLinkTarget value=")
                .Append(repeatedTargetValue ?? "<none>")
                .Append(" meta=")
                .Append(repeatedTargetMetaValue ?? "<none>")
                .Append(" inEdges=")
                .AppendLine(repeatedTargetInCount.ToString());
            report.Append("triggerQuery fromCalls=")
                .Append(triggerFromCalls)
                .Append(" fromMs=")
                .Append(TicksToMilliseconds(triggerFromTicks).ToString("F1"))
                .Append(" fromMaxIn=")
                .Append(triggerFromMaxIn)
                .Append(" valueCalls=")
                .Append(triggerValueCalls)
                .Append(" valueMs=")
                .Append(TicksToMilliseconds(triggerValueTicks).ToString("F1"))
                .Append(" valueMaxIn=")
                .AppendLine(triggerValueMaxIn.ToString());
            report.Append("metaInRebuild triggerCount=")
                .Append(triggerMetaRebuilds)
                .Append(" triggerMs=")
                .Append(TicksToMilliseconds(triggerMetaRebuildTicks).ToString("F1"))
                .Append(" otherCount=")
                .Append(otherMetaRebuilds)
                .Append(" otherMs=")
                .AppendLine(TicksToMilliseconds(otherMetaRebuildTicks).ToString("F1"));
            report.Append("inIndexCallers");
            if (inRebuildCallers.IsEmpty)
                report.Append(" <none>");
            foreach (System.Collections.Generic.KeyValuePair<string, long> caller
                in inRebuildCallers)
            {
                report.Append(' ')
                    .Append(caller.Key)
                    .Append('=')
                    .Append(caller.Value);
            }
            return report.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IndexScope BeginOutIndex()
        {
            if (Volatile.Read(ref enabled) == 0)
                return default;

            return new IndexScope(Stopwatch.GetTimestamp(), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IndexScope BeginInIndex()
        {
            if (Volatile.Read(ref enabled) == 0)
                return default;

            return new IndexScope(Stopwatch.GetTimestamp(), 1);
        }

        internal static void RecordTriggerInQuery(
            bool queriedEdgeFrom,
            int incomingEdgeCount,
            long elapsedTicks)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            if (queriedEdgeFrom)
            {
                Interlocked.Increment(ref triggerFromCalls);
                Interlocked.Add(ref triggerFromTicks, elapsedTicks);
                UpdateMaximum(ref triggerFromMaxIn, incomingEdgeCount);
                return;
            }

            Interlocked.Increment(ref triggerValueCalls);
            Interlocked.Add(ref triggerValueTicks, elapsedTicks);
            UpdateMaximum(ref triggerValueMaxIn, incomingEdgeCount);
        }

        internal static void RecordMetaOnlyInRebuild(
            bool graphChangeTrigger,
            long elapsedTicks)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            if (graphChangeTrigger)
            {
                Interlocked.Increment(ref triggerMetaRebuilds);
                Interlocked.Add(ref triggerMetaRebuildTicks, elapsedTicks);
                return;
            }

            Interlocked.Increment(ref otherMetaRebuilds);
            Interlocked.Add(ref otherMetaRebuildTicks, elapsedTicks);
        }

        internal static bool TryReadTimestamp(out long timestamp)
        {
            if (Volatile.Read(ref enabled) == 0)
            {
                timestamp = 0;
                return false;
            }

            timestamp = Stopwatch.GetTimestamp();
            return true;
        }

        internal static void AddCoalesceTicks(
            GraphScaleListenerChannel channel,
            long elapsedTicks)
        {
            switch (channel)
            {
                case GraphScaleListenerChannel.Out:
                    Interlocked.Add(ref coalesceOutTicks, elapsedTicks);
                    break;
                case GraphScaleListenerChannel.In:
                    Interlocked.Add(ref coalesceInTicks, elapsedTicks);
                    break;
                default:
                    Interlocked.Add(ref coalesceMetaTicks, elapsedTicks);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RecordCoalesce(
            GraphScaleListenerChannel channel,
            int listLength,
            int scannedSteps)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            switch (channel)
            {
                case GraphScaleListenerChannel.Out:
                    Interlocked.Increment(ref coalesceOutCalls);
                    Interlocked.Add(ref coalesceOutSteps, scannedSteps);
                    UpdateMaximum(ref coalesceOutMaxList, listLength);
                    break;
                case GraphScaleListenerChannel.In:
                    Interlocked.Increment(ref coalesceInCalls);
                    Interlocked.Add(ref coalesceInSteps, scannedSteps);
                    UpdateMaximum(ref coalesceInMaxList, listLength);
                    break;
                default:
                    Interlocked.Increment(ref coalesceMetaCalls);
                    Interlocked.Add(ref coalesceMetaSteps, scannedSteps);
                    UpdateMaximum(ref coalesceMetaMaxList, listLength);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RecordAddEdge()
        {
            if (Volatile.Read(ref enabled) != 0)
                Interlocked.Increment(ref addEdgeCalls);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RecordOutIndexRebuild(int edgeCount)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            Interlocked.Increment(ref outIndexRebuilds);
            Interlocked.Add(ref outIndexRebuildEdges, edgeCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RecordInIndexRebuild(int edgeCount)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            Interlocked.Increment(ref inIndexRebuilds);
            Interlocked.Add(ref inIndexRebuildEdges, edgeCount);
            if (edgeCount > 100)
                RecordLargeInRebuildCaller();
        }

        internal static void RecordPlusLessFanout(
            int leftCount,
            int rightCount)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            Interlocked.Increment(ref plusLessCalls);
            Interlocked.Add(
                ref plusLessFanout,
                (long)leftCount * rightCount);
        }

        internal static void RecordPlusLessTarget(
            IVertex destination,
            IVertex meta)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            if (destination != null)
                UpdateMaximum(
                    ref plusLessMaxDestinationOut,
                    destination.OutEdgesRaw.Count);

            if (meta != null)
                UpdateMaximum(
                    ref plusLessMaxMetaIn,
                    meta.MetaInEdgesRaw.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RecordNewVertex()
        {
            if (Volatile.Read(ref enabled) != 0)
                Interlocked.Increment(ref newVertexOperations);
        }

        internal static void RecordLinkTarget(
            IVertex meta,
            IVertex target)
        {
            if (Volatile.Read(ref enabled) == 0)
                return;

            Interlocked.Increment(ref linkOperations);
            int incoming = target == null
                ? 0
                : target.InEdgesRaw.Count;

            if (incoming <= 0)
                Interlocked.Increment(ref linkIncoming0);
            else if (incoming == 1)
                Interlocked.Increment(ref linkIncoming1);
            else if (incoming <= 15)
                Interlocked.Increment(ref linkIncoming2To15);
            else if (incoming <= 255)
                Interlocked.Increment(ref linkIncoming16To255);
            else if (incoming <= 4095)
                Interlocked.Increment(ref linkIncoming256To4095);
            else
                Interlocked.Increment(ref linkIncoming4096OrMore);

            if (ReferenceEquals(target, previousLinkTarget))
            {
                Interlocked.Increment(ref linkSameTargetCount);
                repeatedTargetInCount = incoming;
                if (repeatedTargetValue == null)
                {
                    repeatedTargetValue =
                        target?.Value?.ToString() ?? "<null>";
                    repeatedTargetMetaValue =
                        meta?.Value?.ToString() ?? "<null>";
                }
            }

            previousLinkTarget = target;
        }

        private static void RecordLargeInRebuildCaller()
        {
            long seen = Interlocked.Increment(ref largeInRebuildCount);
            string caller;
            if (seen <= 32 || (seen & 1023) == 0)
                caller = ReadInRebuildCaller();
            else
                caller = dominantInRebuildCaller ?? "<unsampled>";

            if (seen <= 32)
                dominantInRebuildCaller = caller;

            inRebuildCallers.AddOrUpdate(
                caller,
                1,
                (_, count) => count + 1);
        }

        private static string ReadInRebuildCaller()
        {
            StackTrace trace = new StackTrace(false);
            for (int frameIndex = 2;
                frameIndex < trace.FrameCount && frameIndex < 16;
                frameIndex++)
            {
                MethodBase method = trace.GetFrame(frameIndex)?.GetMethod();
                Type declaringType = method?.DeclaringType;
                if (declaringType == null ||
                    declaringType == typeof(EasyVertex) ||
                    declaringType == typeof(GraphScaleDiagnostics))
                {
                    continue;
                }

                return declaringType.Name + "." + method.Name;
            }

            return "<unknown>";
        }

        private static double TicksToMilliseconds(long ticks)
        {
            return ticks * 1000.0 / Stopwatch.Frequency;
        }

        internal readonly struct IndexScope : IDisposable
        {
            private readonly long start;
            private readonly byte kind;
            private readonly bool active;

            internal IndexScope(long start, byte kind)
            {
                this.start = start;
                this.kind = kind;
                active = true;
            }

            public void Dispose()
            {
                if (!active)
                    return;

                long elapsed = Stopwatch.GetTimestamp() - start;
                if (kind == 0)
                    Interlocked.Add(ref outIndexTicks, elapsed);
                else
                    Interlocked.Add(ref inIndexTicks, elapsed);
            }
        }

        private static void UpdateMaximum(ref long target, long value)
        {
            long current = Volatile.Read(ref target);

            while (value > current)
            {
                long observed = Interlocked.CompareExchange(
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
