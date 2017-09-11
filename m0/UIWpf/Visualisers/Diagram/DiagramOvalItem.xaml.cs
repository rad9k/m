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

namespace m0.UIWpf.Visualisers.Diagram
{
    /// <summary>
    /// Interaction logic for DiagramOvalItem.xaml
    /// </summary>
    public partial class DiagramOvalItem : DiagramRectangleItemBase
    {
        public DiagramOvalItem()
        {
            InitializeComponent();
        }


        public override void SetBackAndForeground()
        {
            this.Text.Foreground = ForegroundColor;
            this.Foreground = ForegroundColor;
            this.Elipse.Stroke = ForegroundColor;
            this.Elipse.Fill = BackgroundColor;
        }

        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            if (Vertex.Get(@"BaseEdge:\To:").Value != null)
                this.Text.Text = Vertex.Get(@"BaseEdge:\To:").Value.ToString();
            else
                this.Text.Text = "Ø";

            if (LineWidth != -1 && LineWidth != 0)
                this.Elipse.StrokeThickness = LineWidth;
        }

        public override void Select()
        {
            base.Select();

            this.Text.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Elipse.Stroke = (Brush)FindResource("0BackgroundBrush");
            this.Elipse.Fill = (Brush)FindResource("0SelectionBrush");

            this.Elipse.Cursor = Cursors.ScrollAll;
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBackAndForeground();

            this.Elipse.Cursor = Cursors.Arrow;
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Text.Foreground = (Brush)FindResource("0HighlightForegroundBrush");
            this.Elipse.Stroke = (Brush)FindResource("0HighlightForegroundBrush");
            this.Elipse.Fill = (Brush)FindResource("0HighlightBrush");
        }

        public override void Unhighlight()
        {
            SetBackAndForeground();

            base.Unhighlight();
        }

        public override Point GetLineAnchorLocation(DiagramItemBase toItem, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart)
        {
            Point p = new Point();
            Point p2 = new Point();

            Point pTo = new Point();            

            Line2D firstLineSelf;

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

            double ovalX = Canvas.GetLeft(this) + this.Width / 2;
            double ovalY = Canvas.GetTop(this) + this.Height / 2;
            double ovalR2 = this.Width / 2;
            double ovalR1 = this.Height / 2;

            Oval o = new Oval(ovalX, ovalY, ovalR1, ovalR2);

            if (toItemDiagramLinesCount > 1)
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = Canvas.GetLeft(this) + ((((double)toItemDiagramLinesNumber) / 2 + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p.Y = tY - this.ActualHeight / 2;

                        p2.X = Canvas.GetLeft(this) + ((((double)toItemDiagramLinesNumber) / 2 + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p2.Y = tY - this.ActualHeight;

                        firstLineSelf = Geometry2D.GetLine2DFromPoints(p, p2);

                        return Geometry2D.GetOvalLineCross(firstLineSelf, o)[0];
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = Canvas.GetTop(this) + (((double)(toItemDiagramLinesCount - ((double)toItemDiagramLinesNumber) / 2)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        p2.X = tX + this.ActualWidth;
                        p2.Y = Canvas.GetTop(this) + (((double)(toItemDiagramLinesCount - ((double)toItemDiagramLinesNumber) / 2)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        firstLineSelf = Geometry2D.GetLine2DFromPoints(p, p2);

                        return Geometry2D.GetOvalLineCross(firstLineSelf, o)[1];
                    }                    
                }

                Point pFrom = new Point();

                pFrom.X = tX;

                if (testY <= 0)
                    pFrom.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight / 2);
                else
                    pFrom.Y = tY + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight / 2);

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

    }
}
