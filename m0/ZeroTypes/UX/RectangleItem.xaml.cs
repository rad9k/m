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
    public partial class RectangleItem : UXItem
    {
        IPlatformClass ContentVisualiser;

        public RectangleItem() : base(new ZeroTypes.Edge(null))
        {

        }


        //  public RectangleItem(IVertex baseEdgeVertex, IVertex parentVisualiser) : base(baseEdgeVertex)  {
        //    InitializeComponent();
        //}
/*
    
        public override void VertexSetedUp()
        {
            if (VisualiserClass != null)
            if (Vertex.Get(false, "VisualiserClass:") != null)
            {
                ContentVisualiser = PlatformClass.CreatePlatformObject(VisualiserClass, BaseEdge);                

                Grid.SetRow((UIElement)ContentVisualiser, 2);

                TheGrid.Children.Add((UIElement)ContentVisualiser);
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
        */
        /*
        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            if (Vertex.Get(false, "ShowMeta:False") != null)
            {
                if (Vertex.Get(false, @"BaseEdge:\To:").Value != null)
                    this.Title.Text = Vertex.Get(false, @"BaseEdge:\To:").Value.ToString();
                else
                    this.Title.Text = "Ø";
            }
            else
            {
                string mtext, ttext;

                if (Vertex.Get(false, @"BaseEdge:\Meta:").Value != null)
                    mtext = Vertex.Get(false, @"BaseEdge:\Meta:").Value.ToString();
                else
                    mtext = "Ø";

                if (Vertex.Get(false, @"BaseEdge:\To:").Value != null)
                    ttext = Vertex.Get(false, @"BaseEdge:\To:").Value.ToString();
                else
                    ttext = "Ø";

                this.Title.Text = mtext + " : " + ttext;
            }


            int? _esize = GraphUtil.GetIntegerValue(Vertex.Get(false, "RoundEdgeSize:"));

            if (_esize != null)
            {
                int esize = (int)_esize;

                this.Frame.CornerRadius = new CornerRadius(esize);

                if (Vertex.Get(false, "VisualiserClass:") != null)
                {
                    this.Title.Margin = new Thickness(esize, esize, esize, 0);

                    ((FrameworkElement)this.ContentVisualiser).Margin = new Thickness(esize, 0, esize, esize);

                    TheGrid.RowDefinitions[0].Height = new GridLength(18 + esize);
                }
                else
                {
                    this.Title.Margin = new Thickness(esize);

                    this.Title.TextWrapping = TextWrapping.Wrap;

                    TheGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);

                    TheGrid.Children.Remove(InternalFrame);
                }
            }

            this.Frame.Background = BackgroundColor;

            this.Title.Foreground = ForegroundColor;

            this.InternalFrame.BorderBrush = ForegroundColor;

            this.Frame.BorderBrush = ForegroundColor;

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", ForegroundColor);
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", BackgroundColor);
            }

            if (LineWidth != 0)
            {
                this.Frame.BorderThickness = new Thickness(LineWidth);

                if (ContentVisualiser != null)
                {
                    this.InternalFrame.BorderThickness = new Thickness(LineWidth / 2);

                    this.TheGrid.RowDefinitions[1].Height = new GridLength(LineWidth);
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

            this.Frame.Background = BackgroundColor;

            this.Title.Foreground = ForegroundColor;
            this.Foreground = ForegroundColor;

            this.InternalFrame.BorderBrush = ForegroundColor;

            this.Frame.BorderBrush = ForegroundColor;

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
            this.Foreground = ForegroundColor; 

            this.Frame.Background = BackgroundColor;
            this.Frame.BorderBrush = ForegroundColor;

            this.InternalFrame.BorderBrush = ForegroundColor;           
            this.Title.Foreground = ForegroundColor;
            
            base.Unhighlight();
        }

        protected virtual INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
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

            return exe.Stack;

            //return base.VertexChange(exe);
        }
        */
        // UNDER        

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\ShowMeta");
        static IVertex RoundEdgeSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\RoundEdgeSize");
        static IVertex VisualiserClass_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserClass");
        static IVertex VisualiserVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserVertex");

        public RectangleItem(IEdge edge) : base(edge) { }

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