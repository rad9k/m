using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class RectangleItem : UXItem
    {
        public DiagramRectangleItemBase(IVertex baseEdgeVertex) : base(baseEdgeVertex)
        {
            this.MouseLeftButtonDown += MouseLeftButtonDownHandler;
        }

        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            double? width = GraphUtil.GetDoubleValue(Vertex.Get(false, @"SizeX:"));
            double? height = GraphUtil.GetDoubleValue(Vertex.Get(false, @"SizeY:"));

            if (width != null && height != null)
            {
                this.Width = (double)width;
                this.Height = (double)height;
            }
        }

        public override Point GetLineAnchorLocation(DiagramItemBase toItem, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart)
        {
            Point p = new Point();

            Point pTo = new Point();

            if (toItem != null)
            {
                pTo.X = Canvas.GetLeft(toItem) + toItem.ActualWidth / 2;
                pTo.Y = Canvas.GetTop(toItem) + toItem.ActualHeight / 2;
            }
            else
                pTo = toPoint;

            double tX = Canvas.GetLeft(this) + this.ActualWidth / 2;
            double tY = Canvas.GetTop(this) + this.ActualHeight / 2;

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
                        p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = Canvas.GetTop(this) + (((double)(toItemDiagramLinesCount - toItemDiagramLinesNumber)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        return p;
                    }
                }

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY - this.ActualHeight / 2;
                }

                if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }
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

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - (this.ActualHeight / 2 * testX / testY);
                    p.Y = tY - this.ActualHeight / 2;
                }

                if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + (this.ActualHeight / 2 * testX / testY);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = tY + (this.ActualWidth / 2 * testY / testX);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = tY - (this.ActualWidth / 2 * testY / testX);
                }
            }

            return p;
        }

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
