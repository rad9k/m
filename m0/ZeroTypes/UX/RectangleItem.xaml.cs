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
using System.Xml.Linq;
using m0.User.Process.UX;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class RectangleItem : UXItem
    {
        IPlatformClass ContentVisualiser;

        public RectangleItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public RectangleItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        public override void VertexSetedUp()
        {
            if (VisualiserClass != null)        
            {                
                ContentVisualiser = PlatformClass.CreatePlatformObject(VisualiserClass, BaseEdge);                

                Grid.SetRow((UIElement)ContentVisualiser, 2);
                
                TheGrid.Children.Add((UIElement)ContentVisualiser);

                //

                //Button b = new Button();

                //Grid.SetRow(b, 3);

                //TheGrid.Children.Add(b);
            }
            else
            {
                InternalFrame.BorderThickness = new Thickness(0);
                ContentVisualiser = null;
            }
                    
            if (VisualiserVertex != null && ContentVisualiser != null)
                Diagram.AddEdgesFromDefintion(ContentVisualiser.Vertex, VisualiserVertex);

            base.VertexSetedUp();
        }
        
        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            if(ShowMeta)            
            {
                IVertex baseEdgeTo = BaseEdgeTo;

                if (baseEdgeTo != null)
                    this.Title.Text = baseEdgeTo.Value.ToString();
                else
                    this.Title.Text = "Ø";
            }
            else
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


            double roundEdgeSize = RoundEdgeSize;

            if (roundEdgeSize != 0)
            {                
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);

                if (VisualiserClass != null)
                {
                    this.Title.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 0);

                    ((FrameworkElement)this.ContentVisualiser).Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

                    TheGrid.RowDefinitions[0].Height = new GridLength(18 + RoundEdgeSize);
                }
                else
                {
                    this.Title.Margin = new Thickness(RoundEdgeSize);

                    this.Title.TextWrapping = TextWrapping.Wrap;

                    TheGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);

                    TheGrid.Children.Remove(InternalFrame);
                }
            }

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();
            

            this.Frame.Background = backgroundBrush;

            this.Title.Foreground = foregroundBrush;

            this.InternalFrame.BorderBrush = foregroundBrush;

            this.Frame.BorderBrush = foregroundBrush;

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", foregroundBrush);
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", backgroundBrush);
            }

            if (BorderSize != 0)
            {
                this.Frame.BorderThickness = new Thickness(BorderSize);

                if (ContentVisualiser != null)
                {
                    this.InternalFrame.BorderThickness = new Thickness(BorderSize / 2);

                    this.TheGrid.RowDefinitions[1].Height = new GridLength(BorderSize);
                }
            }
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

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();


            this.Frame.Background = backgroundBrush;

            this.Title.Foreground = foregroundBrush;
            this.Foreground = foregroundBrush;

            this.InternalFrame.BorderBrush = foregroundBrush;

            this.Frame.BorderBrush = foregroundBrush;

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
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            this.Foreground = foregroundBrush; 

            this.Frame.Background = backgroundBrush;
            this.Frame.BorderBrush = foregroundBrush;

            this.InternalFrame.BorderBrush = foregroundBrush;
            this.Title.Foreground = foregroundBrush;
            
            base.Unhighlight();
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "RoundEdgeSize", null))
                {
                    VisualiserUpdate();
                    return exe.Stack;
                }
            }

            //return exe.Stack;

            return base.VertexChange(exe);
        }
        
        // UNDER        

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\ShowMeta");
        static IVertex RoundEdgeSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\RoundEdgeSize");
        static IVertex VisualiserClass_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserClass");
        static IVertex VisualiserVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserVertex");

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

        public IVertex VisualiserClass
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserClass", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserClass", null);

                if (val == null)
                    val = Vertex.AddVertex(VisualiserClass_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex VisualiserVertex
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserVertex", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserVertex", null);

                if (val == null)
                    val = Vertex.AddVertex(VisualiserVertex_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}