using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class IterativeTraversalContractTests
{
    [Fact]
    public void DeepIteratorPreservesDepthFirstOrderAndFirstMatch()
    {
        var fixture = new GraphFixture();
        var childMeta =
            fixture.CreateVertex("Child");
        var root = fixture.CreateVertex("Root");
        var first = fixture.CreateVertex("First");
        var firstChild =
            fixture.CreateVertex("FirstChild");
        var second = fixture.CreateVertex("Second");
        root.AddEdge(childMeta, first);
        root.AddEdge(childMeta, second);
        first.AddEdge(childMeta, firstChild);
        var visited = new List<string>();

        var allMatches =
            GraphUtil.DeepIterator(
                    root,
                    edge =>
                    {
                        visited.Add(
                            edge.To.Value.ToString()!);
                        return true;
                    },
                    false,
                    false,
                    true)
                .ToList();

        Assert.Equal(
            new[]
            {
                "First",
                "FirstChild",
                "Second"
            },
            visited);
        Assert.Equal(
            new[]
            {
                first,
                firstChild,
                second
            },
            allMatches);

        visited.Clear();
        var firstMatch =
            GraphUtil.DeepIterator(
                    root,
                    edge =>
                    {
                        visited.Add(
                            edge.To.Value.ToString()!);
                        return ReferenceEquals(
                            edge.To,
                            firstChild) ||
                            ReferenceEquals(
                                edge.To,
                                second);
                    },
                    true,
                    false,
                    true)
                .ToList();

        Assert.Equal(
            new[] { "First", "FirstChild" },
            visited);
        Assert.Equal(
            new[] { firstChild },
            firstMatch);
    }

    [Fact]
    public void DeepIteratorKeepsPerVertexSnapshotWhenMutationIsAllowed()
    {
        var fixture = new GraphFixture();
        var childMeta =
            fixture.CreateVertex("Child");
        var root = fixture.CreateVertex("Root");
        var first = fixture.CreateVertex("First");
        var second = fixture.CreateVertex("Second");
        var added = fixture.CreateVertex("Added");
        root.AddEdge(childMeta, first);
        root.AddEdge(childMeta, second);
        var visited = new List<IVertex>();

        _ = GraphUtil.DeepIterator(
                root,
                edge =>
                {
                    visited.Add(edge.To);

                    if (ReferenceEquals(
                        edge.To,
                        first))
                        root.AddEdge(
                            childMeta,
                            added);

                    return false;
                },
                false,
                true,
                true)
            .ToList();

        Assert.Equal(
            new[] { first, second },
            visited);
        Assert.Contains(
            root.OutEdgesRaw,
            edge => ReferenceEquals(
                edge.To,
                added));
    }

    [Fact]
    public void IterativeCollectorsPreserveDepthFirstOrderAndCycles()
    {
        var fixture = new GraphFixture();
        var childMeta =
            fixture.CreateVertex("Child");
        var holder = fixture.CreateVertex("Holder");
        var root = fixture.CreateVertex("Root");
        var first = fixture.CreateVertex("First");
        var firstChild =
            fixture.CreateVertex("FirstChild");
        var second = fixture.CreateVertex("Second");
        var rootEdge =
            holder.AddEdge(childMeta, root);
        var firstEdge =
            root.AddEdge(childMeta, first);
        var secondEdge =
            root.AddEdge(childMeta, second);
        var firstChildEdge =
            first.AddEdge(
                childMeta,
                firstChild);
        firstChild.AddEdge(
            childMeta,
            root);

        Assert.Equal(
            new[]
            {
                root,
                first,
                firstChild,
                second
            },
            GraphUtil
                .GetSubGraphWithoutLinksAsList(
                    root)
                .ToArray());
        Assert.Equal(
            new[]
            {
                rootEdge,
                firstEdge,
                firstChildEdge,
                firstChild.OutEdgesRaw[0],
                secondEdge
            },
            GraphUtil
                .GetSubGraphAsEdgesWithoutLinksAsList(
                    rootEdge));
    }

    [Fact]
    public void CoreTraversalsHandleDepthTenThousand()
    {
        const int depth = 10_000;
        var fixture = new GraphFixture();
        var inheritance =
            new EasyVertex[depth + 1];

        for (var index = 0;
            index < inheritance.Length;
            index++)
            inheritance[index] =
                (EasyVertex)fixture.CreateVertex(
                    $"Type-{index}");

        inheritance[0].Value = "RootType";

        for (var index = 1;
            index < inheritance.Length;
            index++)
            AttachInheritanceWithoutValidation(
                inheritance[index],
                inheritance[index - 1],
                fixture.InheritsMeta);

        Assert.True(
            VertexOperations.InheritanceCompare(
                inheritance[^1],
                "RootType"));
        Assert.Equal(
            depth,
            inheritance[^1]
                .OutEdges
                .Count);

        var childMeta =
            fixture.CreateVertex("Child");
        var graphRoot =
            fixture.CreateVertex("GraphRoot");
        IVertex current = graphRoot;

        for (var index = 0;
            index < depth;
            index++)
        {
            var next =
                fixture.CreateVertex(
                    $"Node-{index}");
            current.AddEdge(
                childMeta,
                next);
            current = next;
        }

        Assert.Equal(
            depth + 1,
            GraphUtil
                .GetSubGraphWithoutLinksAsList(
                    graphRoot)
                .Count());
        Assert.Empty(
            GraphUtil.DeepIterator(
                graphRoot,
                _ => false,
                false,
                false,
                true));
    }

    [Fact]
    public void DeepCopyHandlesDepthFiveThousand()
    {
        const int depth = 5_000;
        var fixture = new GraphFixture();
        var childMeta =
            fixture.CreateVertex("Child");
        var original =
            fixture.CreateVertex("Original");
        IVertex current = original;

        for (var index = 0;
            index < depth;
            index++)
        {
            var next =
                fixture.CreateVertex(
                    $"Node-{index}");
            current.AddEdge(
                childMeta,
                next);
            current = next;
        }

        var copy =
            fixture.CreateVertex(
                "CopyPlaceholder");

        GraphUtil.DeepCopyByVertex(
            original,
            copy);

        Assert.Equal(
            depth + 1,
            GraphUtil
                .GetSubGraphWithoutLinksAsList(
                    copy)
                .Count());
    }

    private static void
        AttachInheritanceWithoutValidation(
            IVertex child,
            IVertex parent,
            IVertex inheritsMeta)
    {
        var edge =
            new EasyEdge(
                child,
                inheritsMeta,
                parent);
        child.OutEdgesRaw.Add(edge);
        child.AttachEdge(edge);
        parent.AttachInEdge(edge);
    }
}
