using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace m0.UIWpf.Visualisers.Diagram
{
    public class DiagramRectangleItemBase: DiagramItemBase
    {
        public DiagramRectangleItemBase()
        {
            this.MouseLeftButtonDown += MouseLeftButtonDownHandler;                        
        }

        public override void VisualiserUpdate() {

            base.VisualiserUpdate();

            if (GraphUtil.GetDoubleValue(Vertex.Get(@"SizeX:")) != GraphUtil.NullInt && GraphUtil.GetDoubleValue(Vertex.Get(@"SizeY:")) != GraphUtil.NullInt)
            {
                this.Width = GraphUtil.GetDoubleValue(Vertex.Get(@"SizeX:"));
                this.Height = GraphUtil.GetDoubleValue(Vertex.Get(@"SizeY:"));
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
                        p.Y = Canvas.GetTop(this) + (((double)(toItemDiagramLinesCount-toItemDiagramLinesNumber)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        return p;
                    }
                }

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this)+( ((double)toItemDiagramLinesNumber+1)/((double)toItemDiagramLinesCount+1)*this.ActualWidth );
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
                        p.X = tX + this.ActualWidth/2;
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

       
    
    }
}
