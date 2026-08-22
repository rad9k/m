using System.Reflection;
using m0;
using m0.Foundation;
using m0.Store.Text;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class TextStoreContractTests
{
    public TextStoreContractTests(
        BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void NonEmptyParserQueryIsPreservedDuringLoad()
    {
        string aliasName =
            $"TextStoreParser{Guid.NewGuid():N}";
        string parserQuery = aliasName + ":";
        string fileName = Path.Combine(
            Path.GetTempPath(),
            $"m0-text-store-{Guid.NewGuid():N}.m0t");
        IVertex parser = MinusZero.Instance.Root.Get(
            false,
            @"System\FormalTextLanguage\ZeroCode_VertexAndManyLines");
        IVertex aliasMeta =
            MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                aliasName);
        IEdge aliasEdge = MinusZero.Instance.Root.AddEdge(
            aliasMeta,
            parser);
        TextStore? store = null;

        File.WriteAllText(
            fileName,
            parserQuery + Environment.NewLine +
            "\"Loaded\"");

        try
        {
            store = new TextStore(
                fileName,
                MinusZero.Instance,
                new[]
                {
                    AccessLevelEnum.NoRestrictions
                });
            FieldInfo queryField =
                typeof(TextStore).GetField(
                    "formalTextLanguageProcessing_Query",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic)!;

            Assert.Equal(
                parserQuery,
                queryField.GetValue(store));
            Assert.Equal(
                "Loaded",
                store.Root.Value);
        }
        finally
        {
            if (store != null)
                MinusZero.Instance.RemoveStore(store);

            MinusZero.Instance.Root.DeleteEdge(aliasEdge);
            File.Delete(fileName);
        }
    }
}
