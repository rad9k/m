using m0;
using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class SelfImportResolutionTests
{
    public SelfImportResolutionTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void DirectMetaImportFromCurrentTextStoreResolvesAfterParse()
    {
        string directoryName = Path.Combine(
            MinusZero.Instance.ApplicationPath,
            $"SelfImportTests{Guid.NewGuid():N}");
        string fileName = Path.Combine(
            directoryName,
            "self-import.m0t");
        Directory.CreateDirectory(directoryName);

        string canonicalIdentifier =
            FileSystemUtil.GetCanonicalFileIdentifier(fileName);
        string fileQuery = CreateStartFileQuery(
            canonicalIdentifier);
        string source =
            "System\\FormalTextLanguage\\" +
            "ZeroCode_VertexAndManyLines" +
            Environment.NewLine +
            "\"\"" + Environment.NewLine +
            "\t<@Structure\\System :: \"SelfImportTest\">" +
            Environment.NewLine +
            "\t\t<@$Is :: @Structure\\System>" +
            Environment.NewLine +
            "\t\t<@Structure\\Model :: \"LocalModel\">" +
            Environment.NewLine +
            "\t\t\t<@$Is :: @Structure\\Model>" +
            Environment.NewLine +
            "\t\tpackage \"Consumer\"" +
            Environment.NewLine +
            "\t\t\timport @" + fileQuery +
            "\\$Store:\\Model:LocalModel direct meta";
        File.WriteAllText(fileName, source);

        IVertex? fileVertex = null;
        IStore? textStore = null;

        try
        {
            fileVertex =
                FileSystemUtil.GetFileSystemVertex(fileName);
            Assert.NotNull(fileVertex);

            IVertex storeRoot = Assert.Single(
                GraphUtil.GetQueryOut(
                    fileVertex,
                    "$Store",
                    null)).To;
            textStore = storeRoot.Store;

            IVertex localModel = Assert.IsAssignableFrom<IVertex>(
                GraphUtil.DeepFindOneByValue(
                    storeRoot,
                    "LocalModel",
                    false));
            IVertex consumer = Assert.IsAssignableFrom<IVertex>(
                GraphUtil.DeepFindOneByValue(
                    storeRoot,
                    "Consumer",
                    false));
            IEdge? importEdge = consumer.OutEdgesRaw
                .FirstOrDefault(edge =>
                    string.Equals(
                        edge.Meta?.Value?.ToString(),
                        "$ImportDirectMeta",
                        StringComparison.Ordinal));
            Assert.True(
                importEdge != null,
                "Consumer edges: " +
                string.Join(
                    ", ",
                    consumer.OutEdgesRaw.Select(edge =>
                        (edge.Meta?.Value?.ToString() ?? "<null>") +
                        " -> " +
                        (edge.To?.Value?.ToString() ?? "<null>"))));
            IVertex importedTarget = importEdge.To;

            Assert.Same(localModel, importedTarget);
            Assert.NotSame(
                MinusZero.Instance.Empty,
                importedTarget);
        }
        finally
        {
            if (textStore != null)
                MinusZero.Instance.RemoveStore(textStore);

            if (fileVertex != null)
                fileVertex.Store.RemoveVertexIdentifier(
                    fileVertex);

            if (Directory.Exists(directoryName))
                Directory.Delete(directoryName, true);
        }
    }

    private static string CreateStartFileQuery(
        string canonicalIdentifier)
    {
        int lastSeparator =
            canonicalIdentifier.LastIndexOf('/');
        string directoryPart = canonicalIdentifier
            .Substring(0, lastSeparator)
            .Replace('/', '\\');
        string filePart = canonicalIdentifier.Substring(
            lastSeparator + 1);

        return directoryPart + "\\'" + filePart + "'";
    }
}
