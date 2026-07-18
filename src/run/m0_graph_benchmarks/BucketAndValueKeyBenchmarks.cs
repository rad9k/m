using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using m0;
using m0.Foundation;
using m0.Graph;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

public enum QueryValueKeyKind
{
    ShortString,
    LongString,
    Integer,
    Decimal,
    AllocatingCustomValue
}

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
public class ValueKeyLookupBenchmarks
{
    private GraphFixture fixture = null!;
    private IVertex source = null!;
    private object lookupValue = null!;

    [Params(1, 1000)]
    public int EdgeCount { get; set; }

    [ParamsAllValues]
    public QueryValueKeyKind KeyKind { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        fixture = new GraphFixture();
        source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Relation");

        for (var index = 0;
            index < EdgeCount;
            index++)
            source.AddEdge(
                meta,
                fixture.CreateVertex(
                    CreateValue(index)));

        lookupValue =
            CreateValue(EdgeCount - 1);

        source.QueryOutEdges(
            null,
            lookupValue,
            out _,
            out _);
        source.QueryOutEdges(
            "Relation",
            lookupValue,
            out _,
            out _);
    }

    [Benchmark(Baseline = true)]
    public int ValueOnlyLookup()
    {
        source.QueryOutEdges(
            null,
            lookupValue,
            out var single,
            out var multiple);

        return single == null
            ? multiple?.Count ?? 0
            : 1;
    }

    [Benchmark]
    public int MetaAndValueLookup()
    {
        source.QueryOutEdges(
            "Relation",
            lookupValue,
            out var single,
            out var multiple);

        return single == null
            ? multiple?.Count ?? 0
            : 1;
    }

    private object CreateValue(int index)
    {
        return KeyKind switch
        {
            QueryValueKeyKind.ShortString =>
                $"Value-{index}",
            QueryValueKeyKind.LongString =>
                string.Concat(
                    new string('x', 1024),
                    "-",
                    index),
            QueryValueKeyKind.Integer =>
                index,
            QueryValueKeyKind.Decimal =>
                index + 0.5m,
            QueryValueKeyKind.AllocatingCustomValue =>
                new AllocatingQueryValue(index),
            _ =>
                throw new System.ArgumentOutOfRangeException()
        };
    }

    private sealed class AllocatingQueryValue
    {
        private readonly int index;

        public AllocatingQueryValue(int index)
        {
            this.index = index;
        }

        public override string ToString()
        {
            return string.Concat(
                "Custom-",
                index);
        }
    }
}

public enum EdgeBucketCardinality
{
    Single,
    Many
}

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
public class EdgeBucketRepresentationBenchmarks
{
    private Dictionary<string, object>
        objectUnion = null!;
    private Dictionary<string, PrototypeEdgeBucket>
        structBuckets = null!;
    private string lookupKey = null!;

    [Params(1, 1000)]
    public int BucketCount { get; set; }

    [ParamsAllValues]
    public EdgeBucketCardinality Cardinality
        { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixture = new GraphFixture();
        var meta = fixture.CreateVertex("Relation");
        var first =
            fixture
                .CreateVertex("First")
                .AddEdge(
                    meta,
                    fixture.CreateVertex("Target"));
        var second =
            fixture
                .CreateVertex("Second")
                .AddEdge(
                    meta,
                    fixture.CreateVertex("Target"));

        objectUnion =
            new Dictionary<string, object>(
                BucketCount);
        structBuckets =
            new Dictionary<
                string,
                PrototypeEdgeBucket>(
                    BucketCount);

        for (var index = 0;
            index < BucketCount;
            index++)
        {
            var key = $"Key-{index}";

            if (Cardinality ==
                EdgeBucketCardinality.Single)
            {
                objectUnion.Add(key, first);
                structBuckets.Add(
                    key,
                    new PrototypeEdgeBucket(
                        first));
                continue;
            }

            var list =
                new List_VertexBase
                {
                    first,
                    second
                };
            objectUnion.Add(key, list);
            structBuckets.Add(
                key,
                new PrototypeEdgeBucket(
                    list));
        }

        lookupKey =
            $"Key-{BucketCount - 1}";
    }

    [Benchmark(Baseline = true)]
    public int ObjectUnionLookup()
    {
        object value =
            objectUnion[lookupKey];

        return value is List_VertexBase list
            ? list.Count
            : 1;
    }

    [Benchmark]
    public int StructBucketLookup()
    {
        return structBuckets[lookupKey].Count;
    }

    private readonly struct PrototypeEdgeBucket
    {
        private readonly IEdge? single;
        private readonly IList<IEdge>? multiple;

        public PrototypeEdgeBucket(IEdge single)
        {
            this.single = single;
            multiple = null;
        }

        public PrototypeEdgeBucket(
            IList<IEdge> multiple)
        {
            single = null;
            this.multiple = multiple;
        }

        public int Count =>
            single == null
                ? multiple?.Count ?? 0
                : 1;
    }
}

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
public class StringHashCacheBenchmarks
{
    private Dictionary<string, int>
        ordinalDictionary = null!;
    private Dictionary<string, int>
        cachedHashDictionary = null!;
    private string lookupKey = null!;

    [Params(8, 1024)]
    public int KeyLength { get; set; }

    [Params(1, 1000)]
    public int KeyCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        ordinalDictionary =
            new Dictionary<string, int>(
                KeyCount,
                StringComparer.Ordinal);
        cachedHashDictionary =
            new Dictionary<string, int>(
                KeyCount,
                CachedLongStringComparer.Instance);

        for (var index = 0;
            index < KeyCount;
            index++)
        {
            var key =
                CreateKey(
                    index,
                    KeyLength);
            ordinalDictionary.Add(
                key,
                index);
            cachedHashDictionary.Add(
                key,
                index);
        }

        lookupKey =
            new string(
                CreateKey(
                        KeyCount - 1,
                        KeyLength)
                    .ToCharArray());

        _ = cachedHashDictionary[lookupKey];
    }

    [Benchmark(Baseline = true)]
    public int OrdinalLookup()
    {
        return ordinalDictionary[lookupKey];
    }

    [Benchmark]
    public int CachedLongHashLookup()
    {
        return cachedHashDictionary[lookupKey];
    }

    private static string CreateKey(
        int index,
        int keyLength)
    {
        var suffix = $"-{index}";

        return string.Concat(
            new string(
                'x',
                keyLength - suffix.Length),
            suffix);
    }
}

internal sealed class CachedLongStringComparer
    : IEqualityComparer<string>
{
    private const int MinimumCachedLength = 64;
    private readonly ConditionalWeakTable<
        string,
        CachedHash> hashes = new();

    public static CachedLongStringComparer Instance
        { get; } = new();

    private CachedLongStringComparer()
    {
    }

    public bool Equals(
        string? left,
        string? right)
    {
        return StringComparer.Ordinal.Equals(
            left,
            right);
    }

    public int GetHashCode(string value)
    {
        if (value.Length <
            MinimumCachedLength)
            return StringComparer
                .Ordinal
                .GetHashCode(value);

        return hashes.GetValue(
            value,
            static key =>
                new CachedHash(
                    StringComparer
                        .Ordinal
                        .GetHashCode(key)))
            .Value;
    }

    private sealed class CachedHash
    {
        public CachedHash(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }
}
