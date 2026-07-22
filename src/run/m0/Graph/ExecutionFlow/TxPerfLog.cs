using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace m0.Graph.ExecutionFlow
{
    /// <summary>
    /// Aggregated performance logging for Transaction.Commit hot paths.
    /// Flushes a summary every FlushIntervalMs to avoid flooding the log.
    /// Disable with TxPerfLog.Enabled = false when finished measuring.
    /// </summary>
    public static class TxPerfLog
    {
        public static bool Enabled = true;

        public static int FlushIntervalMs = 500;

        const string Where = "TxPerf";

        class Bucket
        {
            public long CallCount;
            public long TotalTicks;
            public long MaxTicks;
            public long ExtraSum;
            public string ExtraLabel;
        }

        static readonly Dictionary<string, Bucket> buckets = new Dictionary<string, Bucket>();
        static readonly object sync = new object();
        static long lastFlushTimestamp = Stopwatch.GetTimestamp();

        public static long Timestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        public static double TicksToMs(long ticks)
        {
            return ticks * 1000.0 / Stopwatch.Frequency;
        }

        public static void Record(string key, long elapsedTicks)
        {
            Record(key, elapsedTicks, 0, null);
        }

        public static void Record(string key, long elapsedTicks, long extra, string extraLabel)
        {
            if (!Enabled)
                return;

            lock (sync)
            {
                if (!buckets.TryGetValue(key, out Bucket b))
                {
                    b = new Bucket();
                    buckets[key] = b;
                }

                b.CallCount++;
                b.TotalTicks += elapsedTicks;
                if (elapsedTicks > b.MaxTicks)
                    b.MaxTicks = elapsedTicks;
                b.ExtraSum += extra;
                if (extraLabel != null)
                    b.ExtraLabel = extraLabel;

                MaybeFlushUnlocked();
            }
        }

        public static void Count(string key, long n = 1)
        {
            if (!Enabled)
                return;

            lock (sync)
            {
                if (!buckets.TryGetValue(key, out Bucket b))
                {
                    b = new Bucket();
                    buckets[key] = b;
                }

                b.CallCount += n;
                MaybeFlushUnlocked();
            }
        }

        public static void CountWithExtra(string key, long n, long extra, string extraLabel)
        {
            if (!Enabled)
                return;

            lock (sync)
            {
                if (!buckets.TryGetValue(key, out Bucket b))
                {
                    b = new Bucket();
                    buckets[key] = b;
                }

                b.CallCount += n;
                b.ExtraSum += extra;
                if (extraLabel != null)
                    b.ExtraLabel = extraLabel;

                MaybeFlushUnlocked();
            }
        }

        public static void FlushNow(string reason)
        {
            if (!Enabled)
                return;

            lock (sync)
            {
                FlushUnlocked(reason);
            }
        }

        static void MaybeFlushUnlocked()
        {
            long now = Stopwatch.GetTimestamp();
            double sinceFlushMs = TicksToMs(now - lastFlushTimestamp);
            if (sinceFlushMs < FlushIntervalMs)
                return;

            FlushUnlocked("interval");
        }

        static void FlushUnlocked(string reason)
        {
            if (buckets.Count == 0)
            {
                lastFlushTimestamp = Stopwatch.GetTimestamp();
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("flush=").Append(reason);

            List<string> keys = new List<string>(buckets.Keys);
            keys.Sort(StringComparer.Ordinal);

            foreach (string key in keys)
            {
                Bucket b = buckets[key];
                sb.Append(" | ").Append(key);
                sb.Append(" n=").Append(b.CallCount);

                if (b.TotalTicks > 0)
                {
                    double totalMs = TicksToMs(b.TotalTicks);
                    double avgMs = totalMs / b.CallCount;
                    double maxMs = TicksToMs(b.MaxTicks);
                    sb.Append(" totalMs=").Append(totalMs.ToString("F2"));
                    sb.Append(" avgMs=").Append(avgMs.ToString("F3"));
                    sb.Append(" maxMs=").Append(maxMs.ToString("F3"));
                }

                if (b.ExtraSum != 0 || b.ExtraLabel != null)
                {
                    sb.Append(" ").Append(b.ExtraLabel ?? "extra");
                    sb.Append("=").Append(b.ExtraSum);
                }
            }

            buckets.Clear();
            lastFlushTimestamp = Stopwatch.GetTimestamp();

            MinusZero.Instance.Log(1, Where, sb.ToString());
        }
    }
}
