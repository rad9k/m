using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace m0.UIWpf.Controls
{
    public class SelectionArea
    {
        public Rectangle Shape;

        public Canvas Host;

        public bool IsSelecting;

        public void HideSelectionArea()
        {
            Canvas.SetLeft(Shape, 0);
            Canvas.SetTop(Shape, 0);
            Shape.Width = 0;
            Shape.Height = 0;

            IsSelecting = false;
        }

        public double Left, Right, Top, Bottom;

        public Point StartPosition;
        public Point CurrentPosition;

        public void StartSelection(Point position)
        {
            IsSelecting = true;            

            StartPosition = position;

            SetSelectionArea(position.X, position.Y, position.X, position.Y);
        }

        public void MoveSelectionArea(Point position)
        {
            CurrentPosition = position;

            SetSelectionArea(StartPosition.X, StartPosition.Y, position.X, position.Y);
        }

        private void SetSelectionArea(double left, double top, double right, double bottom)
        {
            SelectionArea_RemapCordinates(left, top, right, bottom, out Left, out Right, out Top, out Bottom);

            Canvas.SetLeft(Shape, Left);
            Canvas.SetTop(Shape, Top);
            Shape.Width = Right - Left;
            Shape.Height = Bottom - Top;

            IsSelecting = true;
        }

        private static void SelectionArea_RemapCordinates(double left, double top, double right, double bottom, out double _left, out double _right, out double _top, out double _bottom)
        {
            if (left > right)
            {
                _left = right;
                _right = left;
            }
            else
            {
                _left = left;
                _right = right;
            }

            if (top > bottom)
            {
                _top = bottom;
                _bottom = top;
            }
            else
            {
                _top = top;
                _bottom = bottom;
            }
        }

        public SelectionArea(Canvas _host)
        {
            Host = _host;

            Shape = new Rectangle();

            Host.Children.Add(Shape);

            Shape.Stroke = (Brush)Shape.FindResource("0HighlightBrush");
            Shape.StrokeDashArray = new DoubleCollection(new double[] { 3, 3 });
            Shape.StrokeThickness = 2;

            Canvas.SetZIndex(Shape, 1000);

            HideSelectionArea();

        }
    }
}
