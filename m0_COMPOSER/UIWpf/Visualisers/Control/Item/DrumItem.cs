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
using m0.ZeroTypes;
using m0.UIWpf.Commands;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class DrumItem : Border, IItem
    {
        Canvas Canvas;
        public void OpenDefaultVisualiser()
        {
            OpenFormVisualiser();
        }

        public void OpenFormVisualiser()
        {
            if (BaseEdge == null)
                return;

            IVertex edgeVertex = EdgeHelper.CreateTempEdgeVertex(BaseEdge);

            BaseCommands.OpenFormVisualiser(edgeVertex, false);
        }


        public void Add(Canvas canvas)
        {
            Canvas = canvas;

            Canvas.Children.Add(this);

            ItemDictionary.Add(this);
        }

        public void Remove()
        {
            Canvas.Children.Remove(this);

            ItemDictionary.Remove(this);
        }

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

        public double HiddenHorizontalCenter { get; set; }

        public double HiddenVerticalCenter { get; set; }

        public double HiddenTop { get; set; }

        public double HiddenBottom { get; set; }

        public void SetHiddenFromReal()
        {
            HiddenHorizontalCenter = HorizontalCenter;

            HiddenTop = Top;

            HiddenBottom = Bottom;
        }

        void SetBorder(Brush b)
        {
            path.Stroke = b;
        }

        void SetBackground(Brush b)
        {
            path.Fill = b;
        }

        public void PlayHighlight()
        {            
            path.StrokeThickness = 2;

            SetBorder((Brush)WpfUtil.FindResource("0HardHighlightBrush"));

            SetBackground((Brush)WpfUtil.FindResource("0HardHighlightBrush"));
        }

        public void StopHighlight()
        {
            if (isSelected)
                SelectHighlight();
            else
                NoHighlight();
        }

        public void SelectHighlight()
        {
            isSelected = true;

            path.StrokeThickness = 3;

            SetBorder((Brush)WpfUtil.FindResource("0HighlightBrush"));

            SetBackground((Brush)WpfUtil.FindResource("0HighlightBrush"));
        }

        public void NoHighlight()
        {
            isSelected = false;

            path.StrokeThickness = 2;

            SetBorder((Brush)WpfUtil.FindResource("0BlackBrush"));

            if (showVelocity)
                SetBackground(velocityColorBrush);
            else
                SetBackground((Brush)WpfUtil.FindResource("0LightForegroundBrush"));
        }

        Path path;

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



            path = new Path();

            path.Stretch = Stretch.Fill;

            path.StrokeLineJoin = PenLineJoin.Round;
            

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

            BorderThickness = new Thickness(0);


            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
           
            Update();            

            NoHighlight();            
        }

        Brush velocityColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

        public void Update()
        {            
            if (showVelocity)
            {
                velocityColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

                int? velocity = GraphUtil.GetIntegerValue(BaseEdge.To.Get(false, "Velocity:"));

                if (velocity != null)
                {
                    byte color = (byte)(255 - ((int)velocity * 2));

                    velocityColorBrush = new SolidColorBrush(Color.FromRgb(color, color, color));
                }

                if(!isSelected)
                    SetBackground(velocityColorBrush);
            }            
        }

        protected void UpdateVerticalCenter()
        {
            VerticalCenter = Top + (Height / 2.0);
        }

        public double Left { get; set; }

        public double Right { get; set; }

        double horizontalCenter;
        public double HorizontalCenter {
            get { return Canvas.GetLeft(this) + (Height / 2.0); }
            set {
                horizontalCenter = value;
                Canvas.SetLeft(this, horizontalCenter  - (Height/2.0));
            }
        }

        public double VerticalCenter { get; set; }

        public double Top
        {
            get { return Canvas.GetTop(this); }
            set {
                Canvas.SetTop(this, value);
                UpdateVerticalCenter();
            }
        }

        public double Bottom
        {
            get { return Top + Height; }
            set {
                Height = value - Top;
                Width = Height;
                Canvas.SetLeft(this, horizontalCenter - Height / 2.0);
                UpdateVerticalCenter();
            }
        }
    }
}
