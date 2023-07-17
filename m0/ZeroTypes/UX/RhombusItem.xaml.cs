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
using m0.UIWpf.Visualisers.Diagram;
using m0.UIWpf;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class RhombusItem : UXItem
    {
        public RhombusItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public RhombusItem(IEdge edge) : base(edge)
        {
            InitializeComponent();
        }

        public override void ItemVisualUpdate()
        {
            base.ItemVisualUpdate();

            if (ShowMeta)
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
            else
            {
                IVertex baseEdgeTo = BaseEdgeTo;

                if (baseEdgeTo != null)
                    this.Title.Text = baseEdgeTo.Value.ToString();
                else
                    this.Title.Text = "Ø";
            }

            if (BorderSize != 0)
                this.Rhombus.StrokeThickness = BorderSize;

            SetBaselineColors();
        }

        void SetBaselineColors()
        {
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Rhombus.Fill = backgroundBrush;

            this.Title.Foreground = foregroundBrush;
            this.Foreground = foregroundBrush;

            this.Rhombus.Stroke = borderBrush;
        }

        public override void Select()
        {
            base.Select();

            this.Rhombus.Stroke = (Brush)FindResource("0SelectionBrush");


            this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Rhombus.Fill = (Brush)FindResource("0SelectionBrush");

            this.Title.Cursor = Cursors.ScrollAll;
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBaselineColors();

            this.Title.Cursor = Cursors.Arrow;
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Rhombus.Stroke = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush");

            this.Rhombus.Fill = (Brush)FindResource("0HighlightBrush");

            this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");
        }

        public override void Unhighlight()
        {
            base.Unhighlight();
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "RoundEdgeSize", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "ShowMeta", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "BorderSize", null))
                {
                    ItemVisualUpdate();
                    return exe.Stack;
                }
            }

            //return exe.Stack;

            return base.VertexChange(exe);
        }

        public override Point GetLineAnchorLocation(IUXItem _toItem, int toItemDiagramLinesCount, int toItemDiagramLineNumber, bool isSelfStart)        
        {
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

            if (toItem != null)
            {
                pTo.X = toItemLeftTop.X + toItem.ActualWidth / 2;
                pTo.Y = toItemLeftTop.Y + toItem.ActualHeight / 2;
            }
            else
                pTo = new Point();

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

        // UNDER        

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RhombusItem\ShowMeta");

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
    }
}