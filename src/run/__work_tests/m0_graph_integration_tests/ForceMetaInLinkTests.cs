using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class ForceMetaInLinkTests
{
    public ForceMetaInLinkTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void ForceMetaMarkerGeneratesMetaOnlyLinkWhenMetaIsUnique()
    {
        IVertex baseVertex =
            MinusZero.Instance.CreateTempVertex();
        IVertex metaVertex =
            baseVertex.AddVertex(null, "ForceMeta");
        IVertex targetVertex =
            baseVertex.AddVertex(null, "PhysicalRootName");
        IVertex markerVertex =
            baseVertex.AddVertex(null, "$ForceMetaInLink");
        IEdge markerEdge =
            metaVertex.AddVertexAndReturnEdge(
                markerVertex,
                "");
        IEdge edge =
            baseVertex.AddEdge(metaVertex, targetVertex);

        try
        {
            FormalTextLanguageDictinaries dictionaries =
                DictionariesForFormalTextLanguageFactory.Get(
                    MinusZero.Instance.DefaultFormalTextLanguage);

            Assert.True(
                GraphUtil.HasForceMetaInLinkMarker(metaVertex));
            Assert.True(
                GraphUtil.IsMetaOnlyEnoughToIdentifyEdge(edge));
            Assert.Equal(
                "ForceMeta:",
                GraphUtil.GetIdentyfyingQuerySubString_MetaMode(
                    dictionaries,
                    edge));
            Assert.Equal(
                "ForceMeta:",
                getLinkStringProcessing.GetStringFromEdgesList(
                    dictionaries,
                    null,
                    new List<IEdge> { edge },
                    false));
        }
        finally
        {
            metaVertex.DeleteEdge(markerEdge);
            baseVertex.DeleteEdgesList(
                baseVertex.OutEdgesRaw.ToList());
        }
    }

    [Fact]
    public void ForceMetaMarkerFallsBackToMetaAndTargetWhenMetaIsNotUnique()
    {
        IVertex baseVertex =
            MinusZero.Instance.CreateTempVertex();
        IVertex metaVertex =
            baseVertex.AddVertex(null, "ForceMeta");
        IVertex firstTarget =
            baseVertex.AddVertex(null, "FirstTarget");
        IVertex secondTarget =
            baseVertex.AddVertex(null, "SecondTarget");
        IVertex markerVertex =
            baseVertex.AddVertex(null, "$ForceMetaInLink");
        IEdge markerEdge =
            metaVertex.AddVertexAndReturnEdge(
                markerVertex,
                "");
        IEdge firstEdge =
            baseVertex.AddEdge(metaVertex, firstTarget);
        IEdge secondEdge =
            baseVertex.AddEdge(metaVertex, secondTarget);

        try
        {
            FormalTextLanguageDictinaries dictionaries =
                DictionariesForFormalTextLanguageFactory.Get(
                    MinusZero.Instance.DefaultFormalTextLanguage);

            Assert.False(
                GraphUtil.IsMetaOnlyEnoughToIdentifyEdge(firstEdge));
            Assert.Equal(
                "ForceMeta:FirstTarget",
                GraphUtil.GetIdentyfyingQuerySubString_MetaMode(
                    dictionaries,
                    firstEdge));
            Assert.Equal(
                "ForceMeta:FirstTarget",
                getLinkStringProcessing.GetStringFromEdgesList(
                    dictionaries,
                    null,
                    new List<IEdge> { firstEdge },
                    false));
        }
        finally
        {
            metaVertex.DeleteEdge(markerEdge);
            baseVertex.DeleteEdgesList(
                baseVertex.OutEdgesRaw.ToList());
        }
    }
}
