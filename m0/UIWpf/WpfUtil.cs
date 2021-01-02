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

        public static void SetCursorFromResource(string resourceName)
        {
            System.Windows.Resources.StreamResourceInfo info = Application.GetResourceStream(new Uri(resourceName, UriKind.Relative));


            Mouse.OverrideCursor = new System.Windows.Input.Cursor(info.Stream);
        }

        public static void SetCursor(Cursor cursor)
        {
            Mouse.OverrideCursor = cursor;
        }

        public static Size MeasureTextBlock(TextBlock tb)
        {
            var formattedText = new FormattedText(
                tb.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black,
                new NumberSubstitution(), TextFormattingMode.Display);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public static Color GetNegativeColor(Color inColor)
        {
            Color c = new Color();

            c.A = inColor.A;

            c.R = (byte) (255 - inColor.R);
            c.G = (byte)(255 - inColor.G);
            c.B = (byte)(255 - inColor.B);

            return c;
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


        public static void DrawLine(Panel c, double x1, double y1, double x2, double y2, double thickness, Brush brush)
        {
            Line lr = new Line();

            WpfUtil.SetLinePosition(lr, x1, y1, x2, y2);

            lr.StrokeThickness = thickness;

            lr.Stroke = brush;

            c.Children.Add(lr);
        }

        public static void DrawLine(Panel canvas, double X1, double Y1, double X2, double Y2, Brush brush)
        {
            Line l = new Line();

            l.StrokeThickness = 1;

            l.Stroke = brush;

            WpfUtil.SetLinePosition(l, X1, Y1, X2, Y2);

            canvas.Children.Add(l);
        }

        public static void Print(Canvas canvas, string text, double x, double y, string fontName, double size, Brush brush)
        {
            Label l = new Label();

            l.Foreground = brush;

            l.Margin = new Thickness(0);

            l.Padding = new Thickness(0);

            l.VerticalAlignment = VerticalAlignment.Top;

            canvas.Children.Add(l);

            l.Content = text;

            SetPosition(l, x, y);

            l.FontSize = size;

            if(fontName != null)
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
            if (GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))==null ||
                GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:")) == null ||
                GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")) == null)

                return new SolidColorBrush(Colors.Black);

            if (colorVertex.Get(false, "Opacity:")==null)
                return new SolidColorBrush(Color.FromArgb(255,(byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))
                    ,(byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:"))
                    ,(byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:"))));
            else
                return new SolidColorBrush(Color.FromArgb((byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Opacity:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:"))));
        }

        public static Color GetColorFromColorVertex(IVertex colorVertex)
        {
            if (GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:")) == null ||
                GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:")) == null ||
                GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")) == null)

                return Colors.Black;

            if (colorVertex.Get(false, "Opacity:") == null)
                return Color.FromArgb(255, (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")));
            else
                return Color.FromArgb((byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Opacity:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")));
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
                    if (dObj is ScrollBar) return true;

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
          Point p=new Point();

          p.X=Mouse.GetPosition(m0Main.Instance).X+m0Main.Instance.Left;
          p.Y = Mouse.GetPosition(m0Main.Instance).Y + m0Main.Instance.Top;

          return p;
        }

        public static Point GetMousePositionDnd(DragEventArgs e)
        {
            Point p = new Point();

            p.X = e.GetPosition(m0Main.Instance).X + m0Main.Instance.Left;
            p.Y = e.GetPosition(m0Main.Instance).Y + m0Main.Instance.Top;

            return p;
        }

        public static void SetWindowPosition(Window control, Point position)
        { 
            if (position!=null)
            {
                double screenWidth = 0;
                double screenHeight = 0;

                foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                    if(s.WorkingArea.Left <= position.X && s.WorkingArea.Right >= position.X && // XXX why it is not working for second screen?
                       s.WorkingArea.Top <= position.Y && s.WorkingArea.Bottom >= position.Y)
                        {
                            screenWidth = s.WorkingArea.Width;
                            screenHeight = s.WorkingArea.Height;
                        }

                if(screenWidth == 0)
                {
                    System.Windows.Forms.Screen lastScreen = System.Windows.Forms.Screen.AllScreens[System.Windows.Forms.Screen.AllScreens.Length - 1];

                    screenWidth = lastScreen.WorkingArea.Width;
                    screenHeight = lastScreen.WorkingArea.Height;
                }

                if (position.X + control.ActualWidth > screenWidth)
                    control.Left = screenWidth - control.ActualWidth;
                else
                    control.Left = position.X;

                if (position.Y + control.ActualHeight > screenHeight)
                    control.Top = screenHeight - control.ActualHeight - 75; // 50 is for taskbar that used to be on the bottom
                else
                    control.Top = position.Y;
            }
        }

    }
}
