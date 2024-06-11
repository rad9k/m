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
    public partial class OvalItem : LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "ConstantLabel", "LabelQuery", "ShowMeta", "UseCodeLabel", "FormalTextLanguage", "ShowMeta", "HideLabel",  "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

        FrameworkElement LabelControl;

        public OvalItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public OvalItem(IEdge edge) : base(edge)
        {
            InitializeComponent();
        }

        public override void ItemVisualUpdate()
        {
            base.ItemVisualUpdate();

            LabelControl = GetLabelControl();

            LabelContainer.Child = LabelControl;

         /*   if (ShowMeta)
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
         */
            if (BorderSize != 0)
                this.Elipse.StrokeThickness = BorderSize;

            SetBaselineColors();
        }

        void SetBaselineColors()
        {
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Elipse.Fill = backgroundBrush;

            //this.Title.Foreground = foregroundBrush;
            GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", foregroundBrush);

            this.Foreground = foregroundBrush;            

            this.Elipse.Stroke = borderBrush;
        }

        public override void Select()
        {
            base.Select();
            
            this.Elipse.Stroke = (Brush)FindResource("0SelectionBrush");


            //this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", (Brush)FindResource("0BackgroundBrush"));

            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Elipse.Fill = (Brush)FindResource("0SelectionBrush");

            //this.Title.Cursor = Cursors.ScrollAll;
            GeneralUtil.SetPropertyIfPresent(LabelControl, "Cursor", Cursors.ScrollAll);
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBaselineColors();

            //this.Title.Cursor = Cursors.Arrow;
            GeneralUtil.SetPropertyIfPresent(LabelControl, "Cursor", Cursors.Arrow);
        }

        public override void Highlight()
        {
            base.Highlight();
            
            this.Elipse.Stroke = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush");

            this.Elipse.Fill = (Brush)FindResource("0HighlightBrush");

            //this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");
            GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", (Brush)FindResource("0HighlightForegroundBrush"));
        }

        public override void Unhighlight()
        {
            base.Unhighlight();
        }

        public override Point GetLineAnchorLocation(IUXItem _toItem, bool useToPoint, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLineNumber, bool isSelfStart)
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

        // UNDER        

       
    }
}