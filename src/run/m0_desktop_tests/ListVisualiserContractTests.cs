using System.Runtime.CompilerServices;
using System.Windows.Controls;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
using m0.ZeroTypes;

namespace m0_desktop_tests;

[Collection(DesktopGraphCollection.Name)]
public sealed class ListVisualiserContractTests
{
    public ListVisualiserContractTests(
        DesktopBootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void DataRefreshPreservesExistingColumnTemplates()
    {
        StaTestHost.Run(
            () =>
            {
                IVertex target = CreateRetainedVertex(
                    "ListRefreshTarget");
                IVertex itemMeta = CreateRetainedVertex(
                    "ListRefreshItem");
                target.AddVertex(itemMeta, "First");
                IVertex visualiserState =
                    CreateListVisualiserState(target);
                var visualiser =
                    TestableListVisualiser.Create(
                        visualiserState);

                visualiser.BaseEdgeToUpdated();
                DataGridColumn[] originalColumns =
                    visualiser.Columns.ToArray();

                target.AddVertex(itemMeta, "Second");
                visualiser.BaseEdgeToUpdated();

                Assert.Equal(
                    originalColumns.Length,
                    visualiser.Columns.Count);
                for (int index = 0;
                    index < originalColumns.Length;
                    index++)
                {
                    Assert.Same(
                        originalColumns[index],
                        visualiser.Columns[index]);
                }
            });
    }

    private static IVertex CreateListVisualiserState(
        IVertex target)
    {
        IVertex state = CreateRetainedVertex(
            "ListVisualiserState");
        IVertex baseEdgeMeta = MinusZero.Instance.Root.Get(
            false,
            @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        IVertex baseEdgeVertex =
            EdgeHelper.CreateTempEdgeVertex(
                null,
                null,
                target);
        state.AddEdge(baseEdgeMeta, baseEdgeVertex);
        AddSetting(state, "IsMetaRightAlign", "False");
        AddSetting(state, "IsAllVisualisersEdit", "False");
        AddSetting(state, "ShowMeta", "True");
        AddSetting(state, "ShowIcons", "False");
        AddSetting(state, "ShowHeader", "True");
        AddSetting(state, "GridStyle", "None");
        return state;
    }

    private static void AddSetting(
        IVertex state,
        string name,
        object value)
    {
        state.AddVertex(
            CreateRetainedVertex(name),
            value);
    }

    private static IVertex CreateRetainedVertex(
        string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }

    private sealed class TestableListVisualiser :
        ListVisualiser
    {
        private TestableListVisualiser()
            : base((IEdge)null!)
        {
        }

        internal static TestableListVisualiser Create(
            IVertex visualiserState)
        {
            var visualiser =
                (TestableListVisualiser)
                    RuntimeHelpers.GetUninitializedObject(
                        typeof(TestableListVisualiser));
            var helper =
                (ListVisualiserHelper)
                    RuntimeHelpers.GetUninitializedObject(
                        typeof(ListVisualiserHelper));
            helper.Vertex = visualiserState;
            visualiser.VisualiserHelper = helper;
            visualiser.ThisDataGrid = new DataGrid();
            return visualiser;
        }

        internal IList<DataGridColumn> Columns =>
            ThisDataGrid.Columns;
    }
}
