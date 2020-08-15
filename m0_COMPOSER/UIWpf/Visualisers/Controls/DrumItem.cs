using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using System.Windows.Controls;
using m0.UIWpf;
using System.Windows.Media;
using System.Windows;
using m0.Graph;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    public class DrumItem : Border, IItem
    {
        public IEdge BaseEdge { get; set; }

        public bool IsCentered { get { return true; } }

        public String Label { get; set; }

        public bool CanResizeHorizontally { get { return false; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        public bool IsSelected { get { return isSelected; } }

        bool showVelocity;

        public double HiddenLeft { get; set; }

        public double HiddenRight { get; set; }

        public double HiddenCenter { get; set; }

        public double HiddenTop { get; set; }

        public double HiddenBottom { get; set; }

        public void SetHiddenFromReal()
        {
            HiddenLeft = Canvas.GetLeft(this);

            HiddenRight = HiddenLeft + this.Width;

            HiddenTop = Canvas.GetTop(this);

            HiddenBottom = HiddenTop + Height;
        }

        public void Select()
        {
            isSelected = true;

            //   BorderThickness = new Thickness(3);

            BorderBrush = (Brush)WpfUtil.FindResource("0HighlightBrush");

            if (!showVelocity)
                Background = (Brush)WpfUtil.FindResource("0HighlightBrush");
        }

        public void Unselect()
        {
            isSelected = false;

            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            if (!showVelocity)
                Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

        }
        
        void AddPath()
        {
            PathGeometry pathGeometry = new PathGeometry();

            pathGeometry.FillRule = FillRule.Nonzero;



            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(50, 0);

            pathFigure.IsClosed = true;

            pathGeometry.Figures.Add(pathFigure);



            LineSegment lineSegment1 = new LineSegment();

            lineSegment1.Point = new Point(100, 50);

            pathFigure.Segments.Add(lineSegment1);


            LineSegment lineSegment2 = new LineSegment();

            lineSegment2.Point = new Point(50, 100);

            pathFigure.Segments.Add(lineSegment2);


            LineSegment lineSegment3 = new LineSegment();

            lineSegment3.Point = new Point(0, 50);

            pathFigure.Segments.Add(lineSegment3);


            LineSegment lineSegment4 = new LineSegment();

            lineSegment4.Point = new Point(50, 0);

            pathFigure.Segments.Add(lineSegment4);







            Path path = new Path();

            path.Stretch = Stretch.Fill;

            path.StrokeLineJoin = PenLineJoin.Round;

            path.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            path.Fill = new SolidColorBrush(Color.FromRgb(170, 87, 170));

            path.StrokeThickness = 2;

            path.Data = pathGeometry;

            this.Child = path;
        }

        public DrumItem(IEdge baseEdge, IZoomScrollViewerHost host, bool _showVelocity)
        {
            BaseEdge = baseEdge;

            Host = host;

            showVelocity = _showVelocity;

            //

            AddPath();

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

  /*          BorderThickness = new System.Windows.Thickness(2);

            Brush backColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

            if (showVelocity)
            {
                int? velocity = GraphUtil.GetIntegerValue(BaseEdge.To.Get(false, "Velocity:"));

                if (velocity != null)
                {
                    byte color = (byte)(255 - ((int)velocity * 2));

                    backColorBrush = new SolidColorBrush(Color.FromRgb(color, color, color));
                }
            }

            Background = backColorBrush;

            Unselect();

            this.SizeChanged += NoteItem_SizeChanged;*/
        }

        private void NoteItem_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {

        }

        public void Update()
        {

        }

        public double Left { get; set; }

        public double Right { get; set; }

        public double Center {
            get { return Canvas.GetLeft(this) + Height / 2.0; }
            set { Canvas.SetLeft(this, value - Height/2.0); }
        }

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
