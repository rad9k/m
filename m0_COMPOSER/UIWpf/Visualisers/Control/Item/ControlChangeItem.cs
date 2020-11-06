using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using System.Windows.Controls;
using m0.UIWpf;
using System.Windows.Media;
using System.Windows;
using m0.Graph;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class ControlChangeItem : Border, IItem
    {
        public IEdge BaseEdge { get; set; }

        public bool IsCentered { get { return true; } }

        public String Label { get; set; }

        public bool CanResizeHorizontally { get { return false; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        public bool IsSelected { get { return isSelected; } }

        

        public double HiddenLeft { get; set; }

        public double HiddenRight { get; set; }

        public double HiddenHorizontalCenter { get; set; }

        public double HiddenVerticalCenter { get; set; }

        public double HiddenTop { get; set; }

        public double HiddenBottom { get; set; }

        public void SetHiddenFromReal()
        {
            HiddenHorizontalCenter = HorizontalCenter;

            HiddenVerticalCenter = VerticalCenter;

            //HiddenTop = Top;

            //HiddenBottom = Bottom;
        }

        public void Select()
        {
            isSelected = true;

            BorderThickness = new Thickness(2);

            Background = (Brush)WpfUtil.FindResource("0HighlightBrush");            

            BorderBrush = (Brush)WpfUtil.FindResource("0HighlightBrush");            
        }

        public void Unselect()
        {
            isSelected = false;

            BorderThickness = new Thickness(1);

            Background = (Brush)WpfUtil.FindResource("0BlackBrush");

            BorderBrush = (Brush)WpfUtil.FindResource("0BlackBrush");
        }

        public ControlChangeItem(IEdge baseEdge, IZoomScrollViewerHost host)
        {
            BaseEdge = baseEdge;

            Host = host;

            //

            Width = 6;
            

            BorderThickness = new Thickness(1);


            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            Brush backColorBrush = (Brush)WpfUtil.FindResource("0BlackBrush");

            Background = backColorBrush;

            Unselect();            
        }        

        public void Update()
        {

        }

        public double Left { get; set; }

        public double Right { get; set; }

        double horitzontalCenter;
        public double HorizontalCenter {
            get { return horitzontalCenter; }
            set {
                horitzontalCenter = value;

                Canvas.SetLeft(this, horitzontalCenter - (Width / 2));
            }
        }

        double verticalCenter;
        public double VerticalCenter
        {
            get { return verticalCenter; }
            set
            {
                verticalCenter = value;

                Canvas.SetTop(this, verticalCenter);

                Height = Host.Height_Down - verticalCenter;
            }
        }

        public double Top { get; set; }

        public double Bottom { get; set; }
        
    }
}
