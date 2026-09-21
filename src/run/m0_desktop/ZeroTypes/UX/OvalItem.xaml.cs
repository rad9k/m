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
using m0.UIWpf;
using m0.UIWpf.Controls;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class OvalItem : LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "ConstantLabel", "LabelQuery", "ShowMeta", "ShowIcons", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel",  "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

        public OvalItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public OvalItem(IEdge edge) : base(edge)
        {
            InitializeComponent();
        }

        protected override void UpdateLabelControl(FrameworkElement LabelControl)
        {
            LabelContainer.Child = LabelControl;
        }

        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();        

            if (BorderSize != 0)
                this.Elipse.StrokeThickness = BorderSize;

            SetBaselineColors();
        }

        protected override void SetBaselineColors()
        {
            base.SetBaselineColors();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Elipse.Fill = backgroundBrush;

            this.Foreground = foregroundBrush;            

            this.Elipse.Stroke = borderBrush;
        }

        public override void Select()
        {
            base.Select();
            
            this.Elipse.Stroke = (Brush)FindResource("0SelectionBrush");

            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Elipse.Fill = (Brush)FindResource("0SelectionBrush");
        }

        public override void Highlight()
        {
            base.Highlight();
            
            this.Elipse.Stroke = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush");

            this.Elipse.Fill = (Brush)FindResource("0HighlightBrush");
        }

        public override Point GetLineAnchorLocation(IUXItem _toItem, bool useToPoint, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLineNumber, bool isSelfStart)
        {
            if (OwningVisualiser == null)
                return new Point();

            if (!(_toItem is FrameworkElement))
                return new Point();

            FrameworkElement toItem = (FrameworkElement)_toItem;

            //

            Point toItemLeftTop = new Point();

            Point thisLeftTop = new Point();


            toItemLeftTop = toItem.TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            thisLeftTop = TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            //

            Point p = new Point();
            Point p2 = new Point();

            Point pTo = new Point();

            Line2D firstLineSelf;

            if (!useToPoint && toItem != null)
            {
                pTo.X = toItemLeftTop.X + toItem.ActualWidth / 2;
                pTo.Y = toItemLeftTop.Y + toItem.ActualHeight / 2;
            }
            else
                pTo = toPoint;

            double tX = thisLeftTop.X + this.ActualWidth / 2;
            double tY = thisLeftTop.Y + this.ActualHeight / 2;

            double testX = pTo.X - tX;
            double testY = pTo.Y - tY;

            if (testX == 0) testX = 0.001;
            if (testY == 0) testY = 0.001;

            double ovalX = thisLeftTop.X + this.ActualWidth / 2;
            double ovalY = thisLeftTop.Y + this.ActualHeight / 2;
            double ovalR2 = this.ActualWidth / 2;
            double ovalR1 = this.ActualHeight / 2;

            Oval o = new Oval(ovalX, ovalY, ovalR1, ovalR2);

            if (toItemDiagramLinesCount > 1)
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = thisLeftTop.X + ((((double)toItemDiagramLineNumber) / 2 + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p.Y = tY - this.ActualHeight / 2;

                        p2.X = thisLeftTop.X + ((((double)toItemDiagramLineNumber) / 2 + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p2.Y = tY - this.ActualHeight;

                        firstLineSelf = Geometry2D.GetLine2DFromPoints(p, p2);

                        return Geometry2D.GetOvalLineCross(firstLineSelf, o)[0];
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = thisLeftTop.Y + (((double)(toItemDiagramLinesCount - ((double)toItemDiagramLineNumber) / 2)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        p2.X = tX + this.ActualWidth;
                        p2.Y = thisLeftTop.Y + (((double)(toItemDiagramLinesCount - ((double)toItemDiagramLineNumber) / 2)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        firstLineSelf = Geometry2D.GetLine2DFromPoints(p, p2);

                        return Geometry2D.GetOvalLineCross(firstLineSelf, o)[1];
                    }
                }

                Point pFrom = new Point();

                pFrom.X = tX;

                if (testY <= 0)
                    pFrom.Y = thisLeftTop.Y + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight / 2);
                else
                    pFrom.Y = tY + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight / 2);

                Line2D firstLine = Geometry2D.GetLine2DFromPoints(pTo, pFrom);

                // we have two:
                // - line from pTo to pFrom (firstLine)
                // - oval

                if (testY <= 0 && testX <= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[0];

                if (testY <= 0 && testX >= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[1];

                if (testY >= 0 && testX >= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[1];

                if (testY >= 0 && testX <= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[0];
            }
            else
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = tX;
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = tY;

                        return p;
                    }
                }

                Point pFrom = new Point();

                pFrom.X = tX;
                pFrom.Y = tY;

                Line2D firstLine = Geometry2D.GetLine2DFromPoints(pTo, pFrom);

                if (testY <= 0 && testX <= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[0];

                if (testY <= 0 && testX >= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[1];

                if (testY >= 0 && testX >= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[1];

                if (testY >= 0 && testX <= 0)
                    return Geometry2D.GetOvalLineCross(firstLine, o)[0];
            }

            return p;
        }

        public override bool UsesRectangularLinePorts
        {
            get { return false; }
        }

        internal override bool SegmentCrossesVisibleInterior(
            Point start,
            Point end)
        {
            if (OwningVisualiser == null ||
                OwningVisualiser.Canvas == null)
            {
                return false;
            }

            Rect bounds;
            if (!DiagramLineRouter.TryGetVisibleBounds(
                    this,
                    OwningVisualiser.Canvas,
                    out bounds))
            {
                return false;
            }

            return DiagramLineRouter
                .SegmentCrossesEllipseInterior(
                    start,
                    end,
                    bounds);
        }

        // Ellipse equivalent of UXItem.GetLineEdgeIntersection.
        // Solves the ray <-> ellipse intersection (2 candidates) and returns
        // the one strictly forward along the ray (closest to fromPoint).
        public override Point GetLineEdgeIntersection(Point fromPoint, Vector direction)
        {
            if (OwningVisualiser == null)
                return fromPoint;

            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
                return fromPoint;

            if (direction.Length < 0.0001)
                return fromPoint;

            Point thisLeftTop = TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            double ovalX = thisLeftTop.X + this.ActualWidth / 2;
            double ovalY = thisLeftTop.Y + this.ActualHeight / 2;
            // Same Oval ctor convention as the existing GetLineAnchorLocation
            // path above (a = height/2, b = width/2)
            Oval o = new Oval(ovalX, ovalY, this.ActualHeight / 2, this.ActualWidth / 2);

            Point rayEnd = new Point(fromPoint.X + direction.X, fromPoint.Y + direction.Y);
            Line2D ray = Geometry2D.GetLine2DFromPoints(fromPoint, rayEnd);

            Point[] crosses = Geometry2D.GetOvalLineCross(ray, o);

            Point best = fromPoint;
            double bestDistance = double.MaxValue;

            foreach (Point cand in crosses)
            {
                if (double.IsNaN(cand.X) || double.IsNaN(cand.Y) || double.IsInfinity(cand.X) || double.IsInfinity(cand.Y))
                    continue;

                double dx = cand.X - fromPoint.X;
                double dy = cand.Y - fromPoint.Y;

                if (dx * direction.X + dy * direction.Y <= 0)
                    continue;

                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = cand;
                }
            }

            if (bestDistance == double.MaxValue)
                return fromPoint;

            return best;
        }
    }
}