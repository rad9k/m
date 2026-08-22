using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using m0;
using m0.Foundation;
using m0.Graph;
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

    [Fact]
    public void AddedWrapperIsSizedAndCenteredBeforeLinesUseIt()
    {
        StaTestHost.Run(
            () =>
            {
                const double centerX = 200;
                const double centerY = 150;
                IVertex vertex =
                    CreateRetainedVertex("LayoutVertex");
                var visualiser =
                    TestableGraphVisualiser.CreateForLayout(
                        new EasyEdge(
                            null,
                            null,
                            vertex));
                var child = new Border
                {
                    Width = 80,
                    Height = 40
                };
                SimpleVisualiserWrapper wrapper =
                    visualiser.AddWrapper(
                        centerX,
                        centerY,
                        child,
                        vertex);

                try
                {
                    Assert.True(wrapper.ActualWidth > 0);
                    Assert.True(wrapper.ActualHeight > 0);
                    Assert.Equal(
                        centerX - wrapper.ActualWidth / 2,
                        Canvas.GetLeft(wrapper),
                        6);
                    Assert.Equal(
                        centerY - wrapper.ActualHeight / 2,
                        Canvas.GetTop(wrapper),
                        6);
                }
                finally
                {
                    wrapper.Dispose();
                    visualiser.Children.Clear();
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

        private TestableGraphVisualiser(
            IEdge edge)
            : base(edge)
        {
        }

        internal static TestableGraphVisualiser Create()
        {
            return (TestableGraphVisualiser)
                RuntimeHelpers.GetUninitializedObject(
                    typeof(TestableGraphVisualiser));
        }

        internal static TestableGraphVisualiser
            CreateForLayout(IEdge edge)
        {
            var visualiser =
                new TestableGraphVisualiser(edge);
            FieldInfo displayedVerticesField =
                typeof(GraphVisualiser).GetField(
                    "DisplayedVerticesUIElements",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic)!;
            displayedVerticesField.SetValue(
                visualiser,
                new Dictionary<
                    IVertex,
                    SimpleVisualiserWrapper>());
            return visualiser;
        }

        internal SimpleVisualiserWrapper AddWrapper(
            double x,
            double y,
            FrameworkElement child,
            IVertex vertex)
        {
            return Add(
                x,
                y,
                child,
                vertex);
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
