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
    public partial class ContainerItem : UXItem, IUXContainer
    {
        public Canvas Canvas { get; set; }

        public ContainerItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public ContainerItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        public override void VertexSetedUp()
        {
            if (Canvas == null) {
                Canvas = new Canvas();
                TheGrid.Children.Add(Canvas);
            }

            Canvas.ClipToBounds = true;

            Grid.SetRow(Canvas, 2);


            base.VertexSetedUp();
        }

        public override void ItemVisualUpdate()
        {
            base.ItemVisualUpdate();

            if (ShowMeta)
            {
                IEdge baseEdge = BaseEdge;
                IVertex baseEdgeTo = baseEdge.To;
                IVertex baseEdgeMeta = baseEdge.Meta;

                string meta_text, to_text;

                if (baseEdgeMeta != null)
                    meta_text = baseEdgeMeta.Value.ToString();
                else
                    meta_text = "Ø";

                if (baseEdgeTo != null)
                    to_text = baseEdgeTo.Value.ToString();
                else
                    to_text = "Ø";

                if (meta_text != "$Empty" && meta_text != "")
                    this.Title.Text = meta_text + " : " + to_text;
                else
                    this.Title.Text = to_text;
            }
            else
            {
                IVertex baseEdgeTo = BaseEdgeTo;

                if (baseEdgeTo != null)
                    this.Title.Text = baseEdgeTo.Value.ToString();
                else
                    this.Title.Text = "Ø";
            }

            double roundEdgeSize = RoundEdgeSize;

            if (roundEdgeSize != 0)
            {
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);

                this.Title.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 0);

                Canvas.Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

                TheGrid.RowDefinitions[0].Height = new GridLength(18 + RoundEdgeSize);
            }

            double borderSize = BorderSize;

            if (borderSize != 0)
            {
                this.Frame.BorderThickness = new Thickness(borderSize);
                this.InternalFrame.BorderThickness = new Thickness(borderSize / 2);

                this.TheGrid.RowDefinitions[1].Height = new GridLength(borderSize);
            }
            else
            {
                this.Frame.BorderThickness = new Thickness(1);
                this.InternalFrame.BorderThickness = new Thickness(1);
            }

            //

            NormalColorsUpdate();

            //
        }

        void NormalColorsUpdate()
        {
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();

            this.Foreground = foregroundBrush;

            this.Frame.Background = backgroundBrush;
            this.Frame.BorderBrush = borderBrush;

            this.InternalFrame.BorderBrush = borderBrush;
            this.Title.Foreground = foregroundBrush;
        }

        public override void Select()
        {
            base.Select();

            
            this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");

            this.Title.Cursor = Cursors.ScrollAll;
        }

        public override void Unselect()
        {
            base.Unselect();

            NormalColorsUpdate();

            this.Title.Cursor = Cursors.Arrow;
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 

            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");

            this.InternalFrame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");            
        }

        public override void Unhighlight()
        {
            NormalColorsUpdate();
            
            base.Unhighlight();
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "RoundEdgeSize", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "ShowMeta", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "BorderSize", null))
                {
                    ItemVisualUpdate();
                    return exe.Stack;
                }
            }

            //return exe.Stack;

            return base.VertexChange(exe);
        }
        
        // ContainerItem     

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem\ShowMeta");
        static IVertex RoundEdgeSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem\RoundEdgeSize");
        
        public bool ShowMeta
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    val = Vertex.AddVertex(ShowMeta_meta, value);
                else
                    val.Value = value;
            }
        }

        public double RoundEdgeSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    val = Vertex.AddVertex(RoundEdgeSize_meta, value);
                else
                    val.Value = value;
            }
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