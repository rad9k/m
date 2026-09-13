using m0.Foundation;
using m0.Graph;
using m0.Store;
using m0.Store.FileSystem;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class StoreInvariantContractTests
{
    [Fact]
    public void ThreeArgumentMemoryStoreMarksRootAsRoot()
    {
        var universe = new GraphTestStoreUniverse();
        var store = new MemoryStore(
            "three-argument-memory-store",
            universe,
            new[] { AccessLevelEnum.NoRestrictions });

        Assert.True(store.Root.IsRoot);
        Assert.Same(
            store.Root,
            store.GetVertexByIdentifier(
                store.Root.Identifier));
    }

    [Fact]
    public void DuplicateExplicitIdentifierThrowsAndKeepsFirstVertex()
    {
        var fixture = new GraphFixture();
        var firstVertex =
            new EasyVertex(fixture.Store, "duplicate-id");

        var exception = Assert.Throws<InvalidOperationException>(
            () => new EasyVertex(
                fixture.Store,
                "duplicate-id"));

        Assert.Contains(
            fixture.Store.Identifier,
            exception.Message);
        Assert.Contains("duplicate-id", exception.Message);
        Assert.Same(
            firstVertex,
            fixture.Store.GetVertexByIdentifier(
                "duplicate-id"));

        fixture.Store.StoreVertexIdentifier(firstVertex);
        Assert.Same(
            firstVertex,
            fixture.Store.GetVertexByIdentifier(
                "duplicate-id"));
    }

    [Fact]
    public void NormalFileContentValueInvalidatesWarmSourceIndexes()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Content");
        var content = new FileContentVertex(fixture.Store)
        {
            Value = "Before"
        };
        var edge = source.AddEdge(meta, content);

        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                source,
                "Content",
                "Before"));

        content.Value = "After";

        Assert.Empty(
            GraphUtil.GetQueryOut(
                source,
                "Content",
                "Before"));
        Assert.Equal(
            new[] { edge },
            GraphUtil.GetQueryOut(
                source,
                "Content",
                "After"));
    }

    [Fact]
    public void FileBackedContentValueIsExplicitlyReadOnly()
    {
        var fixture = new GraphFixture();
        var fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-file-content-{Guid.NewGuid():N}.txt");
        File.WriteAllText(fileName, "Original");

        try
        {
            var content = new FileContentVertex(
                fileName,
                fixture.Store);

            var exception =
                Assert.Throws<NotSupportedException>(
                    () => content.Value = "Changed");

            Assert.Contains("read-only", exception.Message);
            Assert.Equal("Original", content.Value);
            Assert.Equal(
                "Original",
                File.ReadAllText(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void FileBackedContentIndexBuildSkipsReadForUnrelatedQuery()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var contentMeta =
            fixture.CreateVertex("Content");
        var fileNameMeta =
            fixture.CreateVertex("Filename");
        var fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-file-content-index-{Guid.NewGuid():N}.txt");
        const string fileContent = "Expensive content";
        File.WriteAllText(
            fileName,
            fileContent);

        try
        {
            var fileContentEdge =
                source.AddEdge(
                    contentMeta,
                    new FileContentVertex(
                        fileName,
                        fixture.Store));
            var fileNameEdge =
                source.AddEdge(
                    fileNameMeta,
                    fixture.CreateVertex(
                        "payload.txt"));

            Assert.Equal(
                new[] { fileContentEdge },
                GraphUtil.GetQueryOut(
                    source,
                    "Content",
                    null));

            var result =
                GraphUtil.GetQueryOut(
                    source,
                    "Filename",
                    "payload.txt");

            Assert.Equal(
                new[] { fileNameEdge },
                result);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void FileBackedContentFallbackTracksExternalChangesAndOrder()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var contentMeta =
            fixture.CreateVertex("Content");
        var normalBeforeEdge =
            source.AddEdge(
                contentMeta,
                fixture.CreateVertex("Before"));
        var fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-file-content-fallback-{Guid.NewGuid():N}.txt");
        File.WriteAllText(fileName, "Before");

        try
        {
            var fileContentEdge =
                source.AddEdge(
                    contentMeta,
                    new FileContentVertex(
                        fileName,
                        fixture.Store));

            Assert.Equal(
                new[]
                {
                    normalBeforeEdge,
                    fileContentEdge
                },
                GraphUtil.GetQueryOut(
                    source,
                    "Content",
                    "Before"));

            File.WriteAllText(fileName, "After");

            Assert.Equal(
                new[] { normalBeforeEdge },
                GraphUtil.GetQueryOut(
                    source,
                    "Content",
                    "Before"));
            Assert.Equal(
                new[] { fileContentEdge },
                GraphUtil.GetQueryOut(
                    source,
                    null,
                    "After"));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void IncomingFileBackedContentFallbackPreservesExactSource()
    {
        var fixture = new GraphFixture();
        var relationMeta =
            fixture.CreateVertex("Relation");
        var target = fixture.CreateVertex("Target");
        var normalSource =
            fixture.CreateVertex("Payload");
        var normalEdge =
            normalSource.AddEdge(
                relationMeta,
                target);
        var fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-file-content-incoming-{Guid.NewGuid():N}.txt");
        File.WriteAllText(fileName, "Payload");

        try
        {
            var fileContent =
                new FileContentVertex(
                    fileName,
                    fixture.Store);
            var fileEdge =
                fileContent.AddEdge(
                    relationMeta,
                    target);

            Assert.Equal(
                new[]
                {
                    normalEdge,
                    fileEdge
                },
                GraphUtil.GetQueryIn(
                    target,
                    "Relation",
                    "Payload"));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void FileBackedContentFallbackPreservesInheritedMetaMatching()
    {
        var fixture = new GraphFixture();
        var baseContentMeta =
            fixture.CreateVertex("BaseContent");
        var derivedContentMeta =
            fixture.CreateVertex("DerivedContent");
        fixture.AddInheritance(
            derivedContentMeta,
            baseContentMeta);
        var source = fixture.CreateVertex("Source");
        var fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-file-content-meta-{Guid.NewGuid():N}.txt");
        File.WriteAllText(fileName, "Payload");

        try
        {
            var edge =
                source.AddEdge(
                    derivedContentMeta,
                    new FileContentVertex(
                        fileName,
                        fixture.Store));

            Assert.Equal(
                new[] { edge },
                GraphUtil.GetQueryOut(
                    source,
                    "BaseContent",
                    "Payload"));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
