using System.Runtime.CompilerServices;
using System.Windows.Controls;
using m0;
using m0.Foundation;
using m0.UIWpf.Visualisers;

namespace m0_desktop_tests;

[Collection(DesktopGraphCollection.Name)]
public sealed class GraphVisualiserContractTests
{
    public GraphVisualiserContractTests(
        DesktopBootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void EventSourceLookupUsesOwningWrapperHierarchy()
    {
        StaTestHost.Run(
            () =>
            {
                var outerVisualiser =
                    TestableGraphVisualiser.Create();
                var innerVisualiser =
                    TestableGraphVisualiser.Create();
                IVertex outerVertex =
                    CreateRetainedVertex("OuterVertex");
                IVertex innerVertex =
                    CreateRetainedVertex("InnerVertex");
                var leaf = new TextBlock();
                var innerWrapper =
                    new SimpleVisualiserWrapper(
                        leaf,
                        innerVertex,
                        innerVisualiser);
                var container = new Grid();
                container.Children.Add(innerWrapper);
                var outerWrapper =
                    new SimpleVisualiserWrapper(
                        container,
                        outerVertex,
                        outerVisualiser);

                try
                {
                    KeyValuePair<
                        IVertex,
                        SimpleVisualiserWrapper> match =
                            outerVisualiser.FindWrapper(
                                leaf);

                    Assert.Same(outerVertex, match.Key);
                    Assert.Same(outerWrapper, match.Value);
                }
                finally
                {
                    outerWrapper.Dispose();
                    innerWrapper.Dispose();
                }
            });
    }

    private static IVertex CreateRetainedVertex(
        string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }

    private sealed class TestableGraphVisualiser :
        GraphVisualiser
    {
        private TestableGraphVisualiser()
            : base((IEdge)null!)
        {
        }

        internal static TestableGraphVisualiser Create()
        {
            return (TestableGraphVisualiser)
                RuntimeHelpers.GetUninitializedObject(
                    typeof(TestableGraphVisualiser));
        }

        internal KeyValuePair<
            IVertex,
            SimpleVisualiserWrapper> FindWrapper(
            object eventSource)
        {
            return GetVertexWrapperByEventSource(
                eventSource);
        }
    }
}
