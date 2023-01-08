using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class UXAggregator : UXItem, IUXAggregator
    {
        public Canvas TheCanvas { get; set; }

        public bool IsSelecting { get; set; }

        public bool IsDrawingLine { get; set; }

        public double ClickPositionX_ItemCordinates { get; set; }
        public double ClickPositionY_ItemCordinates { get; set; }

        public double ClickPositionX_AnchorCordinates { get; set; }
        public double ClickPositionY_AnchorCordinates { get; set; }

        public IUXItem ClickedItem { get; set; }

        public ClickTargetEnum ClickTarget { get; set; }

        public FrameworkElement ClickedAnchor { get; set; }


        public Dictionary<IVertex, List<IUXItem>> GetItemsDictionary() { return null; }
        public double LineSelectionDelta { get; }
        public void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges) { }
        public void SetFocus() { }
        public void UnselectAllSelectedEdges() { }

        public void CheckAndUpdateDiagramLinesForItem(IUXItem item) { }

        //

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\CollapsedSize");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");

        public UXAggregator(IEdge edge) : base(edge) { }

        public bool IsExpanded
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    val = Vertex.AddVertex(IsExpanded_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.Size ExpandedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size ExpandedSizeCreate()
        {
            IEdge expectedSizeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

            if (expectedSizeEdge != null)
                Vertex.DeleteEdge(expectedSizeEdge);

            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, ExpandedSize_meta));
        }

        public UX.Size CollapsedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "CollapsedSize", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size CollapsedSizeCreate()
        {
            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, CollapsedSize_meta));
        }
    }

}
