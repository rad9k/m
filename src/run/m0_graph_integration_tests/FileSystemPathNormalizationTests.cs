using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class FileSystemPathNormalizationTests
{
    public FileSystemPathNormalizationTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void TrailingSeparatorAndParentListingShareTheSameDirectoryVertex()
    {
        var parentDirectory = CreateTemporaryDirectory();
        var nestedName = "net10.0-windows7.0";
        var nestedDirectory = Path.Combine(parentDirectory, nestedName);
        Directory.CreateDirectory(nestedDirectory);
        Directory.CreateDirectory(Path.Combine(nestedDirectory, "autostart"));
        File.WriteAllText(Path.Combine(nestedDirectory, "app.dll"), "x");

        var store = CreateFileSystemStore(parentDirectory);

        try
        {
            string nestedWithSlash =
                nestedDirectory + Path.DirectorySeparatorChar;

            var startVertex = store.GetVertexByIdentifier(nestedWithSlash);
            _ = startVertex.OutEdges;

            Assert.Equal(nestedName, startVertex.Value);
            Assert.Equal(
                FileSystemUtil.NormalizeFileSystemIdentifier(
                    nestedDirectory),
                startVertex.Identifier);
            Assert.Same(
                startVertex,
                store.GetVertexByIdentifier(nestedDirectory));
            Assert.NotNull(
                GraphUtil.GetQueryOutFirst(
                    startVertex,
                    "Directory",
                    "autostart"));
            Assert.NotNull(
                GraphUtil.GetQueryOutFirst(
                    startVertex,
                    "File",
                    "app.dll"));
            Assert.NotNull(
                GraphUtil.GetQueryOutFirst(
                    startVertex,
                    "Basename",
                    nestedName));
            Assert.Null(
                GraphUtil.GetQueryOutFirst(
                    startVertex,
                    "Extension",
                    "0"));

            var root = store.Root;
            _ = root.OutEdges;
            var nestedFromParent = GraphUtil.GetQueryOutFirst(
                root,
                "Directory",
                nestedName);

            Assert.Same(startVertex, nestedFromParent);
            Assert.NotNull(
                GraphUtil.GetQueryOutFirst(
                    nestedFromParent,
                    "Directory",
                    "autostart"));
        }
        finally
        {
            RemoveFileSystemStore(store, store.Root);
            Directory.Delete(parentDirectory, true);
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
            $"m0-fs-path-norm-{Guid.NewGuid():N}");
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
