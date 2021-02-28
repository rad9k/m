using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using m0.Foundation;
using System.Windows.Controls;
using m0.UIWpf;
using System.Windows.Media;
using System.Windows;
using m0.Graph;
using m0.ZeroTypes;
using m0.UIWpf.Commands;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class SequenceEventItem : Border, IItem
    {
        public double PositionMark { get; set; }

        public void OpenDefaultVisualiser() {
            if (BaseEdge == null)
                return;

            IVertex edgeVertex = Edge.CreateTempEdgeVertex(BaseEdge.To.GetAll(false, @"Sequence:").FirstOrDefault());

            BaseCommands.Open(edgeVertex, null);
        }

        public void OpenFormVisualiser()
        {
            if (BaseEdge == null)
                return;

            IVertex edgeVertex = Edge.CreateTempEdgeVertex(BaseEdge.To.GetAll(false, @"Sequence:").FirstOrDefault());

            BaseCommands.OpenFormVisualiser(edgeVertex);
        }

        Canvas Canvas;

        public void Add(Canvas canvas)
        {
            Canvas = canvas;

            Canvas.Children.Add(this);
        }

        public void Remove()
        {
            Canvas.Children.Remove(this);
        }

        public IEdge BaseEdge { get; set; }

        public bool IsCentered { get { return false; } }

        public String Label { get; set; }

        public bool CanResizeHorizontally { get { return true; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        public bool IsSelected { get { return isSelected; } }

        public double HiddenLeft { get; set; }

        public double HiddenRight { get; set; }

        public double HiddenHorizontalCenter { get; set; }

        public double HiddenVerticalCenter { get; set; }

        public double HiddenTop { get; set; }

        public double HiddenBottom { get; set; }

        public void SetHiddenFromReal()
        {
            HiddenLeft = Canvas.GetLeft(this);

            HiddenRight = HiddenLeft + this.Width;

            HiddenTop = Canvas.GetTop(this);

            HiddenBottom = HiddenTop + Height;
        }


        IVertex trackVertex;

        public IVertex TrackVertex {
            get {
                return trackVertex;
            }
            set {
                trackVertex = value;

                if (isSelected)
                    Select();
                else
                    Unselect();
            }
        }

        Brush TrackBrush
        {
            get
            {
                if (TrackVertex == null)
                    return (Brush)WpfUtil.FindResource("0BackgroundBrush");

                IVertex colorVertex = TrackVertex.Get(false, "Color:");

                if (colorVertex == null)
                    return (Brush)WpfUtil.FindResource("0BackgroundBrush");

                return new SolidColorBrush(WpfUtil.GetColorFromColorVertex(colorVertex));
            }
            set
            {

            }
        }        

        public void Select()
        {
            isSelected = true;

            BorderThickness = new Thickness(3);            

            BorderBrush = TrackBrush;
            
            Background = (Brush)WpfUtil.FindResource("0HighlightBrush");                        
        }

        public void Unselect()
        {
            isSelected = false;

            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            Background = TrackBrush;
        }        

        public SequenceEventItem(IEdge baseEdge, IZoomScrollViewerHost host, bool _showLabel)
        {
            BaseEdge = baseEdge;            

            Host = host;
           
            //

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);            
           
            Unselect();          
        }        

        public void Update()
        {
            
        }

        public double Left {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }

        public double Right {
            get { return Left + Width; }
            set { Width = value - Left; }
        }

        public double HorizontalCenter { get; set; }

        public double VerticalCenter { get; set; }

        public double Top
        {
            get { return Canvas.GetTop(this); }
            set { Canvas.SetTop(this, value); }
        }

        public double Bottom
        {
            get { return Top + Height; }
            set { Height = value - Top; }
        }
    }
}
