using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using m0.Graph;
using m0.Foundation;
using m0.ZeroTypes;
using m0.Util;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerSubItem : UXItem, IUXContainer, IMultiContainerSubItem
    {
        public Canvas Canvas { 
            get { return SubCanvas; }
            set { }
        }        

        public MultiContainerSubItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public MultiContainerSubItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        public override void VertexSetedUp()
        {            
            Canvas.ClipToBounds = true;                       

            base.VertexSetedUp();
        }

        protected Brush GetParentBackgroundBrush()
        {
            if (ParentItem == null || !(ParentItem is UXItem))
                return null;

            Color backgroundColor_parent = ((UXItem)ParentItem).BackgroundColor;

            if (backgroundColor_parent != null)
                return backgroundColor_parent.GetBrush();
            else
                return (Brush)FindResource("0BackgroundBrush");
        }

        protected Brush GetParentForegroundBrush()
        {
            if (ParentItem == null || !(ParentItem is UXItem))
                return null;

            Color foregroundColor_parent = ((UXItem)ParentItem).BackgroundColor;

            if (foregroundColor_parent != null)
                return foregroundColor_parent.GetBrush();
            else
                return (Brush)FindResource("0BackgroundBrush");
        }

        public override void VisualiserUpdate()
        {
            // base.VisualiserUpdate();            

            Brush backgroundBrush = GetParentBackgroundBrush();
        }

        public override void Select() {}

        public override void Unselect() {}

        public override void Highlight() {}

        public override void Unhighlight() {}

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            return exe.Stack;
            // return base.VertexChange(exe);
        }
        
        // UXContainer

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeQuery");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");        

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

        public string ContainerEdgeQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    return "";

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ContainerEdgeQuery_meta, value);
                else
                    val.Value = value;
            }
        }

    }
}