using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store.FileSystem;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class FileSystemInvariantTests
{
    public FileSystemInvariantTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void FileRenameUpdatesRegistryQueriesAndRollbackState()
    {
        var directoryName = CreateTemporaryDirectory();
        var oldFileName =
            Path.Combine(directoryName, "before.txt");
        var newFileName =
            Path.Combine(directoryName, "after.txt");
        File.WriteAllText(oldFileName, "content");
        var store = CreateFileSystemStore(directoryName);
        var root = store.Root;
        _ = root.OutEdges;
        var fileEdge = Assert.Single(
            GraphUtil.GetQueryOut(
                root,
                "File",
                "before.txt"));
        var fileVertex = Assert.IsType<FileVertex>(
            fileEdge.To);
        var collision = Assert.Throws<InvalidOperationException>(
            () => new FileVertex(store, oldFileName));
        Assert.Contains(
            "collision",
            collision.Message);
        Assert.Same(
            fileVertex,
            FileSystemStore.FileVertexDictionary[
                oldFileName]);
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();

        ExecutionFlowHelper.StartTransaction();

        try
        {
            fileVertex.Value = "after.txt";

            Assert.False(File.Exists(oldFileName));
            Assert.True(File.Exists(newFileName));
            Assert.Equal(newFileName, fileVertex.Identifier);
            Assert.False(
                FileSystemStore.FileVertexDictionary
                    .ContainsKey(oldFileName));
            Assert.Same(
                fileVertex,
                FileSystemStore.FileVertexDictionary[
                    newFileName]);
            Assert.Same(
                fileVertex,
                store.GetVertexByIdentifier(newFileName));
            Assert.Empty(
                GraphUtil.GetQueryOut(
                    root,
                    "File",
                    "before.txt"));
            Assert.Equal(
                new[] { fileEdge },
                GraphUtil.GetQueryOut(
                    root,
                    "File",
                    "after.txt"));

            var transaction = Assert.IsType<Transaction>(
                MinusZero.Instance.GetTopTransaction());
            var valueChange = Assert.Single(
                transaction
                    .graphChangeTransactionAtoms_OutEdgeValueChange[
                        fileVertex],
                atom => atom.Type ==
                    AtomGraphChangeTypeEnum.ValueChange);
            Assert.Equal("before.txt", valueChange.OldValue);
            Assert.Equal("after.txt", valueChange.NewValue);

            ExecutionFlowHelper.RollbackTransaction();

            Assert.True(File.Exists(oldFileName));
            Assert.False(File.Exists(newFileName));
            Assert.Equal(oldFileName, fileVertex.Identifier);
            Assert.Same(
                fileVertex,
                FileSystemStore.FileVertexDictionary[
                    oldFileName]);
            Assert.False(
                FileSystemStore.FileVertexDictionary
                    .ContainsKey(newFileName));
            Assert.Equal(
                new[] { fileEdge },
                GraphUtil.GetQueryOut(
                    root,
                    "File",
                    "before.txt"));
            Assert.Empty(
                GraphUtil.GetQueryOut(
                    root,
                    "File",
                    "after.txt"));
        }
        finally
        {
            if (!ReferenceEquals(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper.RollbackTransaction();
            }

            RemoveFileSystemStore(
                store,
                fileVertex,
                root);
            Directory.Delete(directoryName, true);
        }
    }

    [Fact]
    public void FileRenameInvalidatesCachedOverlayMetadata()
    {
        var directoryName = CreateTemporaryDirectory();
        var oldFileName =
            Path.Combine(directoryName, "before.bin");
        var newFileName =
            Path.Combine(directoryName, "after.dat");
        File.WriteAllText(oldFileName, "content");
        var store = CreateFileSystemStore(directoryName);
        var root = store.Root;
        _ = root.OutEdges;
        var fileVertex =
            Assert.IsType<FileVertex>(
                GraphUtil.GetQueryOutFirst(
                    root,
                    "File",
                    "before.bin"));

        try
        {
            Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "before.bin"));

            fileVertex.Value = "after.dat";

            Assert.Empty(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "before.bin"));
            Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "after.dat"));
            Assert.True(File.Exists(newFileName));
        }
        finally
        {
            RemoveFileSystemStore(
                store,
                fileVertex,
                root);
            Directory.Delete(directoryName, true);
        }
    }

    [Fact]
    public void FailedFileMovePreservesIdentifierAndRegistry()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var directoryName = CreateTemporaryDirectory();
        var oldFileName =
            Path.Combine(directoryName, "locked.txt");
        var newFileName =
            Path.Combine(directoryName, "after.txt");
        File.WriteAllText(oldFileName, "content");
        var store = CreateFileSystemStore(directoryName);
        var root = store.Root;
        _ = root.OutEdges;
        var fileEdge = Assert.Single(
            GraphUtil.GetQueryOut(
                root,
                "File",
                "locked.txt"));
        var fileVertex = Assert.IsType<FileVertex>(
            fileEdge.To);

        try
        {
            using (File.Open(
                oldFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None))
            {
                Assert.Throws<IOException>(
                    () => fileVertex.Value = "after.txt");
            }

            Assert.True(File.Exists(oldFileName));
            Assert.False(File.Exists(newFileName));
            Assert.Equal(oldFileName, fileVertex.Identifier);
            Assert.Same(
                fileVertex,
                FileSystemStore.FileVertexDictionary[
                    oldFileName]);
            Assert.False(
                FileSystemStore.FileVertexDictionary
                    .ContainsKey(newFileName));
            Assert.Equal(
                new[] { fileEdge },
                GraphUtil.GetQueryOut(
                    root,
                    "File",
                    "locked.txt"));
        }
        finally
        {
            RemoveFileSystemStore(
                store,
                fileVertex,
                root);
            Directory.Delete(directoryName, true);
        }
    }

    [Fact]
    public void UnsupportedDirectoryRenameLeavesAllStateUnchanged()
    {
        var directoryName = CreateTemporaryDirectory();
        var childDirectoryName =
            Path.Combine(directoryName, "child");
        var renamedDirectoryName =
            Path.Combine(directoryName, "renamed");
        Directory.CreateDirectory(childDirectoryName);
        var store = CreateFileSystemStore(directoryName);
        var root = store.Root;
        _ = root.OutEdges;
        var childEdge = Assert.Single(
            GraphUtil.GetQueryOut(
                root,
                "Directory",
                "child"));
        var child = Assert.IsType<DirectoryVertex>(
            childEdge.To);

        try
        {
            var exception = Assert.Throws<NotSupportedException>(
                () => child.Value = "renamed");

            Assert.Contains(
                "not implemented",
                exception.Message);
            Assert.Equal(
                childDirectoryName,
                child.Identifier);
            Assert.True(Directory.Exists(childDirectoryName));
            Assert.False(
                Directory.Exists(renamedDirectoryName));
            Assert.Same(
                child,
                FileSystemStore.DirectoryVertexDictionary[
                    childDirectoryName]);
            Assert.False(
                FileSystemStore.DirectoryVertexDictionary
                    .ContainsKey(renamedDirectoryName));
        }
        finally
        {
            RemoveFileSystemStore(store, child, root);
            Directory.Delete(directoryName, true);
        }
    }

    [Fact]
    public void FileContentQueriesReadOnlyWhenContentIsRequested()
    {
        var directoryName = CreateTemporaryDirectory();
        var fileName =
            Path.Combine(
                directoryName,
                "payload.txt");
        File.WriteAllText(fileName, "Before");
        var store =
            CreateFileSystemStore(directoryName);
        var root = store.Root;
        _ = root.OutEdges;
        var fileVertex =
            Assert.IsType<FileVertex>(
                GraphUtil.GetQueryOutFirst(
                    root,
                    "File",
                    "payload.txt"));

        try
        {
            var contentEdge =
                Assert.Single(
                    GraphUtil.GetQueryOut(
                        fileVertex,
                        "Content",
                        null));

            Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "payload.txt"));

            Assert.Equal(
                new[] { contentEdge },
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Content",
                    "Before"));
            File.WriteAllText(fileName, "After");
            Assert.Empty(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Content",
                    "Before"));
            Assert.Equal(
                new[] { contentEdge },
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Content",
                    "After"));
        }
        finally
        {
            RemoveFileSystemStore(
                store,
                fileVertex,
                root);
            Directory.Delete(directoryName, true);
        }
    }

    [Fact]
    public void FileSystemOverlayExcludesInheritedNoInheritEdges()
    {
        var directoryName = CreateTemporaryDirectory();
        var fileName =
            Path.Combine(
                directoryName,
                "payload.txt");
        File.WriteAllText(fileName, "content");
        var store =
            CreateFileSystemStore(directoryName);
        var root = store.Root;
        _ = root.OutEdges;
        var fileVertex =
            Assert.IsType<FileVertex>(
                GraphUtil.GetQueryOutFirst(
                    root,
                    "File",
                    "payload.txt"));
        var tempStore = MinusZero.Instance.TempStore;
        var noInheritMeta =
            new EasyVertex(tempStore)
            {
                Value = "$NoInherit"
            };
        var blockedMeta =
            new EasyVertex(tempStore)
            {
                Value = "Blocked"
            };
        var marker =
            new EasyVertex(tempStore)
            {
                Value = "Marker"
            };
        var markerEdge =
            blockedMeta.AddEdge(
                noInheritMeta,
                marker);
        var parent =
            new EasyVertex(tempStore)
            {
                Value = "Parent"
            };
        var blockedEdge =
            parent.AddEdge(
                blockedMeta,
                new EasyVertex(tempStore)
                {
                    Value = "Target"
                });
        fileVertex.AddEdge(
            MinusZero.Instance.Inherits,
            parent);

        try
        {
            Assert.Empty(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Blocked",
                    null));
            Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "payload.txt"));
            Assert.DoesNotContain(
                blockedEdge,
                fileVertex.OutEdges);

            blockedMeta.DeleteEdge(markerEdge);

            Assert.Equal(
                new[] { blockedEdge },
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Blocked",
                    null));
            Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "payload.txt"));

            blockedMeta.AddEdge(
                noInheritMeta,
                marker);

            Assert.Empty(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Blocked",
                    null));
            Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "Filename",
                    "payload.txt"));
        }
        finally
        {
            RemoveFileSystemStore(
                store,
                fileVertex,
                root);
            Directory.Delete(directoryName, true);
        }
    }

    private static FileSystemStore CreateFileSystemStore(
        string directoryName)
    {
        return new FileSystemStore(
            directoryName,
            MinusZero.Instance,
            new[] { AccessLevelEnum.NoRestrictions });
    }

    private static string CreateTemporaryDirectory()
    {
        var directoryName = Path.Combine(
            Path.GetTempPath(),
            $"m0-fs-invariants-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryName);
        return directoryName;
    }

    private static void RemoveFileSystemStore(
        FileSystemStore store,
        params IVertex[] vertices)
    {
        foreach (var vertex in vertices)
            store.RemoveVertexIdentifier(vertex);

        MinusZero.Instance.RemoveStore(store);
    }
}
