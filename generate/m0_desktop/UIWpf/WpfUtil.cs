using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using m0.Foundation;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;
using m0.UIWpf.Commands;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Controls;
using m0.Graph;
using m0.UIWpf.Visualisers;
using System.Windows.Shapes;
using System.Globalization;
using m0.UIWpf.Visualisers.Helper;
using m0.ZeroTypes;
using System.Windows.Threading;
using m0.ZeroTypes.UX;
using System.Windows.Forms;

namespace m0.UIWpf
{
    public class WpfUtil
    {
        public static FontWeight MetaWeight = FontWeights.Normal; // FontWeight.FromOpenTypeWeight(550);
        public static FontWeight BoldWeight = FontWeight.FromOpenTypeWeight(550);
        public static FontWeight ValueWeight = FontWeight.FromOpenTypeWeight(550); // FontWeights.Normal;

        public static double IconSize = 15;        

        public static void InitializeUIWpf()
        {
            Dnd.MinimumHorizontalDragDistance = SystemParameters.MinimumHorizontalDragDistance * 2;
            Dnd.MinimumVerticalDragDistance = SystemParameters.MinimumVerticalDragDistance * 2;
        }

        public static void DecorateWithCustomCursor(FrameworkElement e, System.Windows.Input.Cursor cursor)
        {
            e.Tag = cursor;

            e.MouseEnter += DecorateWithCustomCursor_MouseEnter;
            e.MouseLeave += DecorateWithCustomCursor_MouseLeave;
        }

        private static void DecorateWithCustomCursor_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            WpfUtil.SetCursor(System.Windows.Input.Cursors.Arrow);
        }

        private static void DecorateWithCustomCursor_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;

            if (fe.Tag is System.Windows.Input.Cursor)
                WpfUtil.SetCursor((System.Windows.Input.Cursor)fe.Tag);
        }

        public static Line CreateLine(double thickness, Brush stroke)
        {
            Line l = new Line();

            l.StrokeThickness = thickness;

            l.Stroke = stroke;

            return l;
        }

        public static FrameworkElement GetElementAtFromList_StartFromEnd(List<FrameworkElement> list, Point point)
        {
            for(int x=list.Count - 1; x >= 0; x--)
            {
                FrameworkElement e = list[x];

                if (Canvas.GetLeft(e) <= point.X &&
                    point.X <= (Canvas.GetLeft(e) + e.Width) &&
                    Canvas.GetTop(e) <= point.Y &&
                    point.Y <= (Canvas.GetTop(e) + e.Height))
                    return e;
            }                         

            return null;
        }

        public static FrameworkElement GetElementAtFromList(List<FrameworkElement> list, Point point)
        {
            foreach (FrameworkElement e in list)
                if (Canvas.GetLeft(e) <= point.X &&
                    point.X <= (Canvas.GetLeft(e) + e.Width) &&
                    Canvas.GetTop(e) <= point.Y &&
                    point.Y <= (Canvas.GetTop(e) + e.Height))
                    return e;

            return null;
        }

        public static List<FrameworkElement> GetElementsAtFromListByArea(List<FrameworkElement> list, double AreaLeft, double AreaTop, double AreaRight, double AreaDown)
        {
            List<FrameworkElement> match = new List<FrameworkElement>();

            foreach (FrameworkElement e in list)
            {
                if (e is ICentered && ((ICentered)e).IsCentered)
                {
                    ICentered i = (ICentered)e;

                    if (i.HorizontalCenter >= AreaLeft &&
                        AreaRight >= i.HorizontalCenter &&
                        i.VerticalCenter >= AreaTop &&
                        AreaDown >= i.VerticalCenter)
                            match.Add((FrameworkElement)i);
                }
                else
                {
                    if (Canvas.GetLeft(e) >= AreaLeft &&
                        AreaRight >= (Canvas.GetLeft(e) + e.Width) &&
                        Canvas.GetTop(e) >= AreaTop &&
                        AreaDown >= (Canvas.GetTop(e) + e.Height))
                        match.Add(e);
                }
            }

            return match;
        }

        public static List<FrameworkElement> GetElementsAtFromListByArea_OnlyHorizontal(List<FrameworkElement> list, double AreaLeft, double AreaTop, double AreaRight, double AreaDown)
        {
            List<FrameworkElement> match = new List<FrameworkElement>();

            foreach (FrameworkElement e in list)
            {
                if (e is ICentered && ((ICentered)e).IsCentered)
                {
                    ICentered i = (ICentered)e;

                    if (i.HorizontalCenter >= AreaLeft &&
                        AreaRight >= i.HorizontalCenter)
                        match.Add((FrameworkElement)i);
                }
                else
                {
                    if (Canvas.GetLeft(e) >= AreaLeft &&
                        AreaRight >= (Canvas.GetLeft(e) + e.Width))
                        match.Add(e);
                }
            }

            return match;
        }

        public static void SetCursorFromResource(string resourceName)
        {
            System.Windows.Resources.StreamResourceInfo info = System.Windows.Application.GetResourceStream(new Uri(resourceName, UriKind.Relative));


            Mouse.OverrideCursor = new System.Windows.Input.Cursor(info.Stream);
        }

        public static void SetCursor(System.Windows.Input.Cursor cursor)
        {
            Mouse.OverrideCursor = cursor;
        }

        public static System.Windows.Size MeasureTextBlock(TextBlock tb)
        {
            var formattedText = new FormattedText(
                tb.Text,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black,
                new NumberSubstitution(), TextFormattingMode.Display);

            return new System.Windows.Size(formattedText.Width, formattedText.Height);
        }

        public static System.Windows.Media.Color GetNegativeColor(System.Windows.Media.Color inColor)
        {
            System.Windows.Media.Color c = new System.Windows.Media.Color();

            c.A = inColor.A;

            c.R = (byte) (255 - inColor.R);
            c.G = (byte)(255 - inColor.G);
            c.B = (byte)(255 - inColor.B);

            return c;
        }

        public static System.Windows.Media.Color GetNegativeColorWhiteOrBlack(System.Windows.Media.Color inColor)
        {
            int sum = inColor.R + inColor.G + inColor.B;

            if (sum > (256 * 3.0) / 2.0)
                return Colors.Black;

            return Colors.White;            
        }

        public static void SetPosition(FrameworkElement e, double x, double y)
        {
            Canvas.SetLeft(e, x);
            Canvas.SetTop(e, y);            
        }

        public static void SetPosition(FrameworkElement e, double x, double y, double width, double height)
        {
            Canvas.SetLeft(e, x);
            Canvas.SetTop(e, y);

            e.Width = width;
            e.Height = height;
        }

        public static void SetPositionAbsolute(FrameworkElement e, double x1, double y1, double x2, double y2)
        {
            Canvas.SetLeft(e, x1);
            Canvas.SetTop(e, y1);

            e.Width = x2 - x1;
            e.Height = y2 - y1;
        }

        public static void SetLinePosition(Line e, double x1, double y1, double x2, double y2)
        {
            e.X1 = x1;
            e.Y1 = y1;

            e.X2 = x2;
            e.Y2 = y2;
        }

        public static Line DrawLine(System.Windows.Controls.Panel c, double x1, double y1, double x2, double y2, double thickness, Brush brush)
        {
            Line lr = new Line();

            WpfUtil.SetLinePosition(lr, x1, y1, x2, y2);

            lr.StrokeThickness = thickness;

            lr.Stroke = brush;

            c.Children.Add(lr);

            return lr;
        }

        public static Line DrawLine(System.Windows.Controls.Panel canvas, double X1, double Y1, double X2, double Y2, Brush brush)
        {
            Line l = new Line();

            l.StrokeThickness = 1;

            l.Stroke = brush;

            WpfUtil.SetLinePosition(l, X1, Y1, X2, Y2);

            canvas.Children.Add(l);

            return l;
        }

        public static void Print(Canvas canvas, string text, double x, double y, string fontName, double size, Brush brush)
        {
            System.Windows.Controls.Label l = new System.Windows.Controls.Label();

            l.Foreground = brush;

            l.Margin = new Thickness(0);

            l.Padding = new Thickness(0);

            l.VerticalAlignment = VerticalAlignment.Top;

            canvas.Children.Add(l);

            l.Content = text;

            SetPosition(l, x, y);

            l.FontSize = size;

            if (fontName != null)
                l.FontFamily = new FontFamily(fontName);
        }

        public static object FindResource(string name)
        {
            return m0Main.Instance.FindResource(name);
        }

        public static DependencyObject GetParentFormVisualiser(DependencyObject e)
        {
            if (e == null)
                return null;

            if (e is FormVisualiser)
                return e;

            return GetParentFormVisualiser(VisualTreeHelper.GetParent(e));
        }

        public static Brush GetBrushFromColorVertex(IVertex colorVertex)
        {
            return new SolidColorBrush(ColorHelper_desktop.GetColorFromColorVertex(colorVertex));
        }

        public static double GetHorizontalSizeOfCharacterString(int Characters)
        {
            return (Characters * 11) + 10;
        }

        public static bool IsMouseOverScrollbar(object sender, Point mousePosition)
        {
            if (sender is Visual)
            {
                HitTestResult hit = VisualTreeHelper.HitTest(sender as Visual, mousePosition);

                if (hit == null) return false;

                DependencyObject dObj = hit.VisualHit;
                while (dObj != null)
                {
                    if (dObj is System.Windows.Controls.Primitives.ScrollBar) return true;

                    if ((dObj is Visual) || (dObj is Visual3D)) dObj = VisualTreeHelper.GetParent(dObj);
                    else dObj = LogicalTreeHelper.GetParent(dObj);
                }
            }

            return false;
        }

        public static bool HasParentsGotContextMenu(FrameworkElement e)
        {
            if (e.ContextMenu != null)
                return true;

            DependencyObject Parent = VisualTreeHelper.GetParent(e);

            if (Parent == null)
                return false;
            else
                if (Parent is FrameworkElement)
                    return (HasParentsGotContextMenu((FrameworkElement)Parent));
                else
                    return false;
        }

        public static IVisualiser GetParentVisualiser(FrameworkElement e)
        {
            object Parent = VisualTreeHelper.GetParent(e);

            if (Parent == null)
                Parent = e.Parent;

            if (Parent == null)
                return null;

            if (Parent is IVisualiser)
                return (IVisualiser)Parent;

            if (Parent is FrameworkElement)
                return GetParentVisualiser((FrameworkElement)Parent);
           
            return null;
        } 

        public static T FindVisualChild<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null) return null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(current, i);
                if (child is T) return (T)child;
                T result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
                
        public static Point GetMousePosition()
        {
            Point p = new Point();

            p.X = Mouse.GetPosition(m0Main.Instance).X + m0Main.Instance.Left;
            p.Y = Mouse.GetPosition(m0Main.Instance).Y + m0Main.Instance.Top;

            return p;
        }

        public static Point GetMousePositionDnd(System.Windows.DragEventArgs e)
        {
            Point p = new Point();

            p.X = e.GetPosition(m0Main.Instance).X + m0Main.Instance.Left;
            p.Y = e.GetPosition(m0Main.Instance).Y + m0Main.Instance.Top;

            return p;
        }

        public static System.Windows.Size GetWpfScreenSizeFromPoint(Point wpfPoint)
        {
            // Znajdź źródło (dla DPI)
            var source = PresentationSource.FromVisual(m0Main.Instance);
            if (source == null)
                return new System.Windows.Size(0, 0); // fallback

            // Macierz przekształcenia z WPF -> fizyczne piksele
            var transformToDevice = source.CompositionTarget.TransformToDevice;
            var transformFromDevice = source.CompositionTarget.TransformFromDevice;

            // Konwersja WPF → fizyczne piksele
            int physicalX = (int)(wpfPoint.X * transformToDevice.M11);
            int physicalY = (int)(wpfPoint.Y * transformToDevice.M22);

            // Znajdź monitor zawierający ten punkt
            var screen = Screen.FromPoint(new System.Drawing.Point(physicalX, physicalY));
            var bounds = screen.WorkingArea;

            // Konwersja granic ekranu z pikseli → WPF
            Point topLeft = transformFromDevice.Transform(new Point(bounds.Left, bounds.Top));
            Point bottomRight = transformFromDevice.Transform(new Point(bounds.Right, bounds.Bottom));

            // Oblicz rozmiary
            double widthWpf = bottomRight.X - topLeft.X;
            double heightWpf = bottomRight.Y - topLeft.Y;

            return new System.Windows.Size(widthWpf, heightWpf);
        }

        public static void SetWindowPosition(Window window, Point position)
        {
            System.Windows.Size size = GetWpfScreenSizeFromPoint(position);

            Point position_tobe = new Point(position.X, position.Y);
            
            if (position.X + window.ActualWidth > size.Width)
                position_tobe.X = size.Width - window.ActualWidth;

            if (position.Y + window.ActualHeight > size.Height)
                position_tobe.Y = size.Height - window.ActualHeight;

            window.Left = position_tobe.X;
            window.Top = position_tobe.Y;
        }        
    }
}
