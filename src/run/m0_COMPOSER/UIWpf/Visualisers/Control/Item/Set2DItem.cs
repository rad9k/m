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
    public class Set2DItem : Border, IItem
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

        }

        public void StopHighlight()
        {

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

            path.StrokeThickness = 1;

            SetBorder((Brush)WpfUtil.FindResource("0ForegroundBrush"));

            SetBackground((Brush)WpfUtil.FindResource("0ForegroundBrush"));
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

        public Set2DItem(IEdge baseEdge, IZoomScrollViewerHost host)
        {
            Width = 10;
            Height = 10;

            BaseEdge = baseEdge;

            Host = host;            

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
            if (Host is Set2DVisualiser)
            {
                Set2DVisualiser set2Dhost = (Set2DVisualiser)Host;

                string horizontalValue = GraphUtil.GetQueryOutFirst(this.BaseEdge.To, set2Dhost.SetItemHorizontalAxisMetaString, null).ToString();

                string verticalValue = GraphUtil.GetQueryOutFirst(this.BaseEdge.To, set2Dhost.SetItemVerticalAxisMetaString, null).ToString();

                this.ToolTip = "[" + 
                    set2Dhost.SetItemHorizontalAxisMetaString
                    + "] : "
                    + horizontalValue 
                    + " / [" 
                    + set2Dhost.SetItemVerticalAxisMetaString 
                    + "] : " 
                    + verticalValue;
            }
        }

        public double Left { get; set; }

        public double Right { get; set; }

        double horizontalCenter;
        public double HorizontalCenter {
            get { return Canvas.GetLeft(this) + (Height / 2.0); }
            set {
                horizontalCenter = value;
                Canvas.SetLeft(this, horizontalCenter  - (Height / 2.0));
            }
        }

        double verticalCenter;
        public double VerticalCenter {
            get { return Canvas.GetTop(this) + (Width / 2.0); }
            set
            {
                verticalCenter = value;
                Canvas.SetTop(this, verticalCenter - (Width / 2.0));
            }
        }

        public double Top { get; set; }

        public double Bottom { get; set; }
    }
}
