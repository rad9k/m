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

            IVertex edgeVertex = EdgeHelper.CreateTempEdgeVertex(BaseEdge.To.GetAll(false, @"Sequence:").FirstOrDefault());

            BaseCommands.OpenDefaultVisualiser(edgeVertex, false);
        }

        public void OpenFormVisualiser()
        {
            if (BaseEdge == null)
                return;

            IVertex edgeVertex = EdgeHelper.CreateTempEdgeVertex(BaseEdge.To.GetAll(false, @"Sequence:").FirstOrDefault());

            BaseCommands.OpenFormVisualiser(edgeVertex, false);
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
                    SelectHighlight();
                else
                    NoHighlight();
            }
        }

        Color TrackColor;

        Brush TrackBrush
        {
            get
            {              
                TrackColor = (Color)WpfUtil.FindResource("0Background");

                if (TrackVertex == null) 
                    return (Brush)WpfUtil.FindResource("0BackgroundBrush");

                IVertex colorVertex = TrackVertex.Get(false, "Color:");

                if (colorVertex == null)
                    return (Brush)WpfUtil.FindResource("0BackgroundBrush");

                TrackColor = ColorHelper_desktop.GetColorFromColorVertex(colorVertex);

                return new SolidColorBrush(TrackColor);
            }
            set
            {

            }
        }

        public void PlayHighlight() { }

        public void StopHighlight() { }

        public void SelectHighlight()
        {
            isSelected = true;

            BorderThickness = new Thickness(3);            

            BorderBrush = TrackBrush;
            
            Background = (Brush)WpfUtil.FindResource("0HighlightBrush");

            Update();
        }

        public void NoHighlight()
        {
            isSelected = false;

            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            Background = TrackBrush;

            Update();
        }

        bool showLabel;

        TextBlock labelControl;

        public SequenceEventItem(IEdge baseEdge, IZoomScrollViewerHost host, bool _showLabel)
        {
            BaseEdge = baseEdge;

            showLabel = _showLabel;

            Host = host;

            //

            labelControl = new TextBlock();

            this.Child = labelControl;            

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);            
           
            NoHighlight();

            if (showLabel)            
                Update();                            
        }        

        public void Update()
        {
            if (showLabel)
            {
                IVertex sequenceVertex = BaseEdge.To.Get(false, "Sequence:");

                if (sequenceVertex != null && sequenceVertex.Value != null)
                    Label = sequenceVertex.Value.ToString();

                labelControl.Text = " " + Label;

                labelControl.Foreground = new SolidColorBrush(WpfUtil.GetNegativeColorWhiteOrBlack(TrackColor));
            }
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
