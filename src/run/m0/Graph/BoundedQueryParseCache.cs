using m0.Foundation;
using System;
using System.Collections.Generic;

namespace m0.Graph
{
    internal readonly record struct QueryParseCacheValue(
        IVertex Vertex,
        bool Cacheable);

    internal readonly record struct QueryParseCacheState(
        int Count,
        int Capacity);

    internal readonly struct QueryParseCacheLease : IDisposable
    {
        private readonly IVertex vertex;

        internal QueryParseCacheLease(
            IVertex vertex,
            bool hit,
            bool retainedByCache)
        {
            this.vertex = vertex;
            Hit = hit;
            RetainedByCache = retainedByCache;
        }

        public IVertex Vertex
        {
            get { return vertex; }
        }

        public bool Hit { get; }

        public bool RetainedByCache { get; }

        public void Dispose()
        {
            vertex.RemoveExternalReference();
        }
    }

    internal sealed class BoundedQueryParseCache
    {
        private sealed class Entry
        {
            internal Entry(string query, IVertex vertex)
            {
                Query = query;
                Vertex = vertex;
            }

            internal string Query { get; }

            internal IVertex Vertex { get; }
        }

        private readonly object synchronizationRoot;
        private readonly Dictionary<string, LinkedListNode<Entry>> entries =
            new Dictionary<string, LinkedListNode<Entry>>(
                StringComparer.Ordinal);
        private readonly LinkedList<Entry> usageOrder =
            new LinkedList<Entry>();
        private int capacity;

        internal BoundedQueryParseCache(int capacity)
            : this(capacity, new object())
        {
        }

        internal BoundedQueryParseCache(
            int capacity,
            object synchronizationRoot)
        {
            ValidateCapacity(capacity);
            this.synchronizationRoot =
                synchronizationRoot ??
                throw new ArgumentNullException(
                    nameof(synchronizationRoot));
            this.capacity = capacity;
        }

        internal int Capacity
        {
            get
            {
                lock (synchronizationRoot)
                    return capacity;
            }
            set
            {
                ValidateCapacity(value);

                lock (synchronizationRoot)
                {
                    capacity = value;
                    EvictOverflow();
                }
            }
        }

        internal QueryParseCacheState State
        {
            get
            {
                lock (synchronizationRoot)
                    return new QueryParseCacheState(
                        entries.Count,
                        capacity);
            }
        }

        internal QueryParseCacheLease GetOrCreate(
            string query,
            Func<QueryParseCacheValue> valueFactory)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            lock (synchronizationRoot)
            {
                if (entries.TryGetValue(
                    query,
                    out LinkedListNode<Entry> existingNode))
                {
                    usageOrder.Remove(existingNode);
                    usageOrder.AddFirst(existingNode);
                    existingNode.Value.Vertex.AddExternalReference();

                    return new QueryParseCacheLease(
                        existingNode.Value.Vertex,
                        true,
                        true);
                }

                QueryParseCacheValue created = valueFactory();

                if (created.Vertex == null)
                    throw new InvalidOperationException(
                        "A query parse cache factory returned a null vertex.");

                created.Vertex.AddExternalReference();

                if (!created.Cacheable || capacity == 0)
                    return new QueryParseCacheLease(
                        created.Vertex,
                        false,
                        false);

                created.Vertex.AddExternalReference();

                LinkedListNode<Entry> newNode =
                    usageOrder.AddFirst(
                        new Entry(query, created.Vertex));
                entries.Add(query, newNode);
                EvictOverflow();

                return new QueryParseCacheLease(
                    created.Vertex,
                    false,
                    true);
            }
        }

        internal void Clear()
        {
            lock (synchronizationRoot)
            {
                foreach (Entry entry in usageOrder)
                    entry.Vertex.RemoveExternalReference();

                entries.Clear();
                usageOrder.Clear();
            }
        }

        private void EvictOverflow()
        {
            while (entries.Count > capacity)
            {
                LinkedListNode<Entry> nodeToEvict =
                    usageOrder.Last;
                usageOrder.RemoveLast();
                entries.Remove(nodeToEvict.Value.Query);
                nodeToEvict.Value.Vertex.RemoveExternalReference();
            }
        }

        private static void ValidateCapacity(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Query parse cache capacity cannot be negative.");
        }
    }
}
