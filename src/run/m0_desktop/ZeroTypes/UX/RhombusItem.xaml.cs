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
    public partial class RhombusItem : LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "ConstantLabel", "LabelQuery", "ShowMeta", "ShowIcons", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel", "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

        public RhombusItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public RhombusItem(IEdge edge) : base(edge)
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
                this.Rhombus.StrokeThickness = BorderSize;

            SetBaselineColors();
        }

        protected override void SetBaselineColors()
        {
            base.SetBaselineColors();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Rhombus.Fill = backgroundBrush;

            this.Foreground = foregroundBrush;

            this.Rhombus.Stroke = borderBrush;
        }

        public override void Select()
        {
            base.Select();

            this.Rhombus.Stroke = (Brush)FindResource("0SelectionBrush");

            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Rhombus.Fill = (Brush)FindResource("0SelectionBrush");
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Rhombus.Stroke = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush");

            this.Rhombus.Fill = (Brush)FindResource("0HighlightBrush");
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

            Point pRhombus1 = new Point();
            Point pRhombus2 = new Point();

            Line2D secondLine, firstLineSelf;

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

                        pRhombus1.X = thisLeftTop.X;
                        pRhombus1.Y = tY;

                        pRhombus2.X = tX;
                        pRhombus2.Y = thisLeftTop.Y;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = thisLeftTop.Y + (((double)(toItemDiagramLinesCount - ((double)toItemDiagramLineNumber) / 2)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        p2.X = tX + this.ActualWidth;
                        p2.Y = thisLeftTop.Y + (((double)(toItemDiagramLinesCount - ((double)toItemDiagramLineNumber) / 2)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        firstLineSelf = Geometry2D.GetLine2DFromPoints(p, p2);

                        pRhombus1.X = thisLeftTop.X + this.ActualWidth;
                        pRhombus1.Y = tY;

                        pRhombus2.X = tX;
                        pRhombus2.Y = thisLeftTop.Y + this.ActualHeight;
                    }

                    secondLine = Geometry2D.GetLine2DFromPoints(pRhombus1, pRhombus2);

                    return Geometry2D.FindLineCross(firstLineSelf, secondLine);
                }

                Point pFrom = new Point();

                pFrom.X = tX;

                if (testY <= 0)
                    pFrom.Y = thisLeftTop.Y + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight / 2);
                else
                    pFrom.Y = tY + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight / 2);

                Line2D firstLine = Geometry2D.GetLine2DFromPoints(pTo, pFrom);

                // we have two lines:
                // - from pTo to pFrom (firstLine)
                // - from pRhombus1 to pRhombus2 (secondLine - depending on the rhombus side)

                if (testY <= 0 && testX <= 0)
                {
                    pRhombus1.X = thisLeftTop.X;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y;
                }

                if (testY <= 0 && testX >= 0)
                {
                    pRhombus1.X = thisLeftTop.X + this.ActualWidth;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y;
                }

                if (testY >= 0 && testX >= 0)
                {
                    pRhombus1.X = thisLeftTop.X + this.ActualWidth;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y + this.ActualHeight;
                }

                if (testY >= 0 && testX <= 0)
                {
                    pRhombus1.X = thisLeftTop.X;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y + this.ActualHeight;
                }

                secondLine = Geometry2D.GetLine2DFromPoints(pRhombus1, pRhombus2);

                return Geometry2D.FindLineCross(firstLine, secondLine);
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

                pRhombus1 = new Point();
                pRhombus2 = new Point();

                // we have two lines:
                // - from pTo to pFrom (firstLine)
                // - from pRhombus1 to pRhombus2 (secondLine - depending on the rhombus side)

                if (testY <= 0 && testX <= 0)
                {
                    pRhombus1.X = thisLeftTop.X;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y;
                }

                if (testY <= 0 && testX >= 0)
                {
                    pRhombus1.X = thisLeftTop.X + this.ActualWidth;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y;
                }

                if (testY >= 0 && testX >= 0)
                {
                    pRhombus1.X = thisLeftTop.X + this.ActualWidth;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y + this.ActualHeight;
                }

                if (testY >= 0 && testX <= 0)
                {
                    pRhombus1.X = thisLeftTop.X;
                    pRhombus1.Y = tY;

                    pRhombus2.X = tX;
                    pRhombus2.Y = thisLeftTop.Y + this.ActualHeight;
                }

                secondLine = Geometry2D.GetLine2DFromPoints(pRhombus1, pRhombus2);

                return Geometry2D.FindLineCross(firstLine, secondLine);
            }

            return p;
        }

        // Rhombus equivalent of UXItem.GetLineEdgeIntersection.
        // The rhombus is inscribed in the item bounding box, with vertices
        // at the midpoints of the bounding box sides. Ray is intersected
        // against each of the 4 diagonal sides as a segment; the closest
        // forward hit is returned.
        public override Point GetLineEdgeIntersection(Point fromPoint, Vector direction)
        {
            if (OwningVisualiser == null)
                return fromPoint;

            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
                return fromPoint;

            if (direction.Length < 0.0001)
                return fromPoint;

            Point thisLeftTop = TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            double left = thisLeftTop.X;
            double top = thisLeftTop.Y;
            double right = left + this.ActualWidth;
            double bottom = top + this.ActualHeight;
            double cx = left + this.ActualWidth / 2;
            double cy = top + this.ActualHeight / 2;

            // 4 vertices (top, right, bottom, left)
            Point vTop = new Point(cx, top);
            Point vRight = new Point(right, cy);
            Point vBottom = new Point(cx, bottom);
            Point vLeft = new Point(left, cy);

            Point rayEnd = new Point(fromPoint.X + direction.X, fromPoint.Y + direction.Y);
            Line2D ray = Geometry2D.GetLine2DFromPoints(fromPoint, rayEnd);

            Point best = fromPoint;
            double bestDistance = double.MaxValue;

            ConsiderRhombusSide(ray, vTop, vRight, fromPoint, direction, ref best, ref bestDistance);
            ConsiderRhombusSide(ray, vRight, vBottom, fromPoint, direction, ref best, ref bestDistance);
            ConsiderRhombusSide(ray, vBottom, vLeft, fromPoint, direction, ref best, ref bestDistance);
            ConsiderRhombusSide(ray, vLeft, vTop, fromPoint, direction, ref best, ref bestDistance);

            if (bestDistance == double.MaxValue)
                return fromPoint;

            return best;
        }

        // Intersects ray with the infinite line through (a, b), then accepts
        // the crossing only if it falls within the [a, b] segment bounding box
        // (with tolerance) AND lies strictly forward along the ray. Updates
        // best/bestDistance if this hit is the closest so far.
        static void ConsiderRhombusSide(Line2D ray, Point a, Point b, Point fromPoint, Vector direction,
            ref Point best, ref double bestDistance)
        {
            Line2D side = Geometry2D.GetLine2DFromPoints(a, b);
            Point cand = Geometry2D.FindLineCross(ray, side);

            if (double.IsNaN(cand.X) || double.IsNaN(cand.Y) || double.IsInfinity(cand.X) || double.IsInfinity(cand.Y))
                return;

            const double tol = 0.5;
            double minX = Math.Min(a.X, b.X) - tol;
            double maxX = Math.Max(a.X, b.X) + tol;
            double minY = Math.Min(a.Y, b.Y) - tol;
            double maxY = Math.Max(a.Y, b.Y) + tol;

            if (cand.X < minX || cand.X > maxX || cand.Y < minY || cand.Y > maxY)
                return;

            double dx = cand.X - fromPoint.X;
            double dy = cand.Y - fromPoint.Y;

            if (dx * direction.X + dy * direction.Y <= 0)
                return;

            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                best = cand;
            }
        }

        // UNDER        
   
    }
}