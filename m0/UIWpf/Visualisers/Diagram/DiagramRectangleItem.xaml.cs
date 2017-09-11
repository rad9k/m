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

namespace m0.UIWpf.Visualisers.Diagram
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class DiagramRectangleItem : DiagramRectangleItemBase
    {
        IPlatformClass ContentVisualiser;

        public override IVertex Vertex
        {
            get { return base.Vertex; }
            set
            {
                base.Vertex = value;
            }
        }

        public override void VertexSetedUp()
        {
            if (Vertex.Get("VisualiserClass:") != null)
            {
                ContentVisualiser = PlatformClass.CreatePlatformObject(Vertex.Get("VisualiserClass:"));

                GraphUtil.ReplaceEdge(ContentVisualiser.Vertex, "BaseEdge", Vertex.Get("BaseEdge:"));

                Grid.SetRow((UIElement)ContentVisualiser, 2);

                TheGrid.Children.Add((UIElement)ContentVisualiser);
            }
            else
            {
                InternalFrame.BorderThickness = new Thickness(0);
                ContentVisualiser = null;
            }

            //VisualiserUpdate(); done in base.VertexSetedUp();
           
            if (Vertex.Get("VisualiserVertex:") != null && ContentVisualiser != null)
                Diagram.AddEdgesFromDefintion(ContentVisualiser.Vertex, Vertex.Get("VisualiserVertex:"));

            base.VertexSetedUp();
        }

        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            if (Vertex.Get("ShowMeta:False") != null)
            {
                if (Vertex.Get(@"BaseEdge:\To:").Value != null)
                    this.Title.Text = Vertex.Get(@"BaseEdge:\To:").Value.ToString();
                else
                    this.Title.Text = "Ø";
            }
            else
            {
                string mtext, ttext;

                if (Vertex.Get(@"BaseEdge:\Meta:").Value != null)
                    mtext = Vertex.Get(@"BaseEdge:\Meta:").Value.ToString();
                else
                    mtext = "Ø";

                if (Vertex.Get(@"BaseEdge:\To:").Value != null)
                    ttext = Vertex.Get(@"BaseEdge:\To:").Value.ToString();
                else
                    ttext = "Ø";

                this.Title.Text = mtext + " : " + ttext;
            }



            if (Vertex.Get("RoundEdgeSize:") != null)
            {
                int esize = GraphUtil.GetIntegerValue(Vertex.Get("RoundEdgeSize:"));

                
                this.Frame.CornerRadius = new CornerRadius(esize);

                if (Vertex.Get("VisualiserClass:") != null)
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

      

        public DiagramRectangleItem()
        {
            InitializeComponent();
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

        public override void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (sender == Vertex.Get(@"RoundEdgeSize:"))
                VisualiserUpdate();

            base.VertexChange(sender, e);
        }
    }
}
