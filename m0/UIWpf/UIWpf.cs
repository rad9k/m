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


namespace m0.UIWpf
{
    public class UIWpf
    {
        public static FontWeight MetaWeight = FontWeights.Normal; // FontWeight.FromOpenTypeWeight(550);
        public static FontWeight BoldWeight = FontWeight.FromOpenTypeWeight(550);
        public static FontWeight ValueWeight = FontWeight.FromOpenTypeWeight(550); // FontWeights.Normal;

        public static void InitializeUIWpf()
        {
            Dnd.MinimumHorizontalDragDistance = SystemParameters.MinimumHorizontalDragDistance * 2;
            Dnd.MinimumVerticalDragDistance = SystemParameters.MinimumVerticalDragDistance * 2;
        }

        public static DependencyObject getParentFormVisualiser(DependencyObject e)
        {
            if (e == null)
                return null;

            if (e is FormVisualiser)
                return e;

            return getParentFormVisualiser(VisualTreeHelper.GetParent(e));
        }

        public static Brush GetBrushFromColorVertex(IVertex colorVertex)
        {
            if(colorVertex.Get("Opacity:")==null)
                return new SolidColorBrush(Color.FromArgb(255,(byte)GraphUtil.GetIntegerValue(colorVertex.Get("Red:"))
                    ,(byte)GraphUtil.GetIntegerValue(colorVertex.Get("Green:"))
                    ,(byte)GraphUtil.GetIntegerValue(colorVertex.Get("Blue:"))));
            else
                return new SolidColorBrush(Color.FromArgb((byte)GraphUtil.GetIntegerValue(colorVertex.Get("Opacity:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get("Red:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get("Green:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get("Blue:"))));
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

        public static void SetWindowPosition(Window control, Point position){
            if (position!=null)
            {
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

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
