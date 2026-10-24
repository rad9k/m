using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store.Binary;
using m0.Store.FileSystem;
using m0.Store.Json;
using m0.Store.Text;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class StartPathIntegrationTests
{
    public StartPathIntegrationTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void PhysicalAndStartIdentifiersReturnTheSameStore()
    {
        string testDirectory = CreateTestDirectory();
        string physicalFileName =
            Path.Combine(testDirectory, "model.m0j");
        string canonicalIdentifier =
            FileSystemUtil.GetCanonicalFileIdentifier(
                physicalFileName);
        IStore? store = null;

        try
        {
            store = FileSystemUtil.GetFileStore(
                MinusZero.Instance,
                typeof(JsonSerializationStore)
                    .AssemblyQualifiedName!,
                physicalFileName);

            IStore storeFromStartIdentifier =
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    typeof(JsonSerializationStore)
                        .AssemblyQualifiedName!,
                    canonicalIdentifier);
            IStore storeFromLegacyStartIdentifier =
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    typeof(JsonSerializationStore)
                        .AssemblyQualifiedName!,
                    canonicalIdentifier.Replace(
                        '/',
                        '\\'));

            Assert.Same(store, storeFromStartIdentifier);
            Assert.Same(
                store,
                storeFromLegacyStartIdentifier);
            Assert.Equal(
                canonicalIdentifier,
                store.Identifier);
            Assert.StartsWith(
                FileSystemUtil.StartIdentifierPrefix,
                store.Identifier);
        }
        finally
        {
            if (store != null)
                MinusZero.Instance.RemoveStore(store);

            DeleteTestDirectory(testDirectory);
        }
    }

    [Fact]
    public void FileVertexAndFullFilenameUseStartIdentifier()
    {
        string testDirectory = CreateTestDirectory();
        string physicalFileName =
            Path.Combine(testDirectory, "data.txt");
        File.WriteAllText(physicalFileName, "content");
        IVertex? fileVertex = null;

        try
        {
            fileVertex =
                FileSystemUtil.GetFileSystemVertex(
                    physicalFileName);

            Assert.NotNull(fileVertex);

            string canonicalIdentifier =
                FileSystemUtil.GetCanonicalFileIdentifier(
                    physicalFileName);

            Assert.Equal(
                canonicalIdentifier,
                fileVertex.Identifier);
            Assert.Same(
                fileVertex,
                fileVertex.Store.GetVertexByIdentifier(
                    physicalFileName));
            Assert.Same(
                fileVertex,
                fileVertex.Store.GetVertexByIdentifier(
                    canonicalIdentifier));

            IVertex fullFilenameVertex =
                GraphUtil.GetQueryOutFirst(
                    fileVertex,
                    "FullFilename",
                    null);

            Assert.Equal(
                canonicalIdentifier,
                GraphUtil.GetStringValue(
                    fullFilenameVertex));
            Assert.Equal(
                FileSystemUtil.NormalizeFileSystemIdentifier(
                    physicalFileName),
                FileSystemUtil.GetPhysicalFileName(
                    GraphUtil.GetStringValue(
                        fullFilenameVertex)));
        }
        finally
        {
            if (fileVertex != null)
                fileVertex.Store.RemoveVertexIdentifier(
                    fileVertex);

            DeleteTestDirectory(testDirectory);
        }
    }

    [Fact]
    public void TextStoreReadsThroughStartIdentifier()
    {
        string testDirectory = CreateTestDirectory();
        string physicalFileName =
            Path.Combine(testDirectory, "model.m0t");
        File.WriteAllText(
            physicalFileName,
            "System\\FormalTextLanguage\\" +
            "ZeroCode_VertexAndManyLines" +
            Environment.NewLine +
            "\"Loaded\"");
        IStore? store = null;

        try
        {
            store = FileSystemUtil.GetFileStore(
                MinusZero.Instance,
                typeof(TextStore).AssemblyQualifiedName!,
                physicalFileName);

            Assert.StartsWith(
                FileSystemUtil.StartIdentifierPrefix,
                store.Identifier);
            Assert.Equal("Loaded", store.Root.Value);
            Assert.Same(
                store,
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    typeof(TextStore)
                        .AssemblyQualifiedName!,
                    store.Identifier));
        }
        finally
        {
            if (store != null)
                MinusZero.Instance.RemoveStore(store);

            DeleteTestDirectory(testDirectory);
        }
    }

    [Theory]
    [InlineData("json")]
    [InlineData("binary")]
    public void CrossStoreReferenceRoundtripKeepsStartIdentifier(
        string format)
    {
        string testDirectory = CreateTestDirectory();
        string extension =
            format == "json" ? ".m0j" : ".m0x";
        string sourcePhysicalFileName =
            Path.Combine(
                testDirectory,
                "source" + extension);
        string targetPhysicalFileName =
            Path.Combine(
                testDirectory,
                "target" + extension);
        string targetCanonicalIdentifier =
            FileSystemUtil.GetCanonicalFileIdentifier(
                targetPhysicalFileName);
        string storeTypeName =
            format == "json"
                ? typeof(JsonSerializationStore)
                    .AssemblyQualifiedName!
                : typeof(BinaryStore).AssemblyQualifiedName!;
        bool previousNoBackup =
            MinusZero.Instance.CommandLineParameters.NoBackup;
        var storesToRemove = new HashSet<IStore>();

        MinusZero.Instance.CommandLineParameters.NoBackup =
            true;

        try
        {
            IStore targetStore =
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    storeTypeName,
                    targetPhysicalFileName);
            storesToRemove.Add(targetStore);
            SaveStore(targetStore);

            IStore sourceStore =
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    storeTypeName,
                    sourcePhysicalFileName);
            storesToRemove.Add(sourceStore);

            sourceStore.Root.AddEdge(
                MinusZero.Instance.Empty,
                targetStore.Root);
            SaveStore(sourceStore);

            if (format == "json")
            {
                Assert.Contains(
                    targetCanonicalIdentifier,
                    File.ReadAllText(
                        sourcePhysicalFileName));
            }

            MinusZero.Instance.RemoveStore(sourceStore);
            MinusZero.Instance.RemoveStore(targetStore);
            storesToRemove.Clear();

            IStore reloadedSourceStore =
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    storeTypeName,
                    sourcePhysicalFileName);
            storesToRemove.Add(reloadedSourceStore);

            IEdge reloadedEdge =
                Assert.Single(
                    reloadedSourceStore.Root.OutEdgesRaw);
            storesToRemove.Add(reloadedEdge.To.Store);

            Assert.Equal(
                targetCanonicalIdentifier,
                reloadedEdge.To.Store.Identifier);
            Assert.Equal(
                targetCanonicalIdentifier,
                ((IDetachableEdge)reloadedEdge)
                    .ToStoreIdentifier);
            Assert.Same(
                reloadedEdge.To.Store,
                FileSystemUtil.GetFileStore(
                    MinusZero.Instance,
                    storeTypeName,
                    targetPhysicalFileName));
        }
        finally
        {
            foreach (IStore store in storesToRemove)
                MinusZero.Instance.RemoveStore(store);

            MinusZero.Instance.CommandLineParameters.NoBackup =
                previousNoBackup;
            DeleteTestDirectory(testDirectory);
        }
    }

    private static void SaveStore(
        IStore store)
    {
        store.Detach();
        store.CommitTransaction();
        store.Attach();
    }

    private static string CreateTestDirectory()
    {
        string directoryName = Path.Combine(
            MinusZero.Instance.ApplicationPath,
            $"start-path-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryName);
        return directoryName;
    }

    private static void DeleteTestDirectory(
        string directoryName)
    {
        if (Directory.Exists(directoryName))
            Directory.Delete(directoryName, true);
    }
}
