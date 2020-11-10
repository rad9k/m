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
using m0;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class ControlChangeItem : Border, IItem
    {
        Canvas Canvas;

        public void Add(Canvas canvas)
        {
            Canvas = canvas;

            Canvas.Children.Add(this);

            Canvas.Children.Add(CCTop);
        }

        public void Remove()
        {
            Canvas.Children.Remove(this);

            Canvas.Children.Remove(CCTop);
        }        

        public bool IsNote;

        IEdge baseEdge;

        public IEdge BaseEdge {
            get {
                return baseEdge;
            }
            set {
                baseEdge = value;

                if (baseEdge.To.Get(false, "$Is:NoteEvent") != null)
                    IsNote = true;
            }
        }

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

            CCTop.Background = (Brush)WpfUtil.FindResource("0HighlightBrush");

            CCTop.BorderBrush = (Brush)WpfUtil.FindResource("0HighlightBrush");
        }

        public void Unselect()
        {
            isSelected = false;

            BorderThickness = new Thickness(1);

            Background = (Brush)WpfUtil.FindResource("0BlackBrush");

            BorderBrush = (Brush)WpfUtil.FindResource("0BlackBrush");

            CCTop.Background = (Brush)WpfUtil.FindResource("0BlackBrush");

            CCTop.BorderBrush = (Brush)WpfUtil.FindResource("0BlackBrush");
        }

        Border CCTop = new Border();

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

            CCTop.Background = backColorBrush;

            Unselect();            
        }        

        public void Update()
        {

        }

        public double Left { get; set; }

        public double Right { get; set; }

        void CCTopPositionUpdate()
        {
            Canvas.SetLeft(CCTop, Canvas.GetLeft(this) - 2);
            Canvas.SetTop(CCTop, Canvas.GetTop(this));

            CCTop.Width = 10;
            CCTop.Height = 2;

            if (Canvas != null)
                if (!Canvas.Children.Contains(CCTop))
                    Canvas.Children.Add(CCTop);         
        }

        public static int getValueFromMouseY_Down(double mouseY, double Height_Down)
        {
            return 127 - (int)((mouseY / Height_Down) * 127);
        }

        protected void updateVertexValueByVerticalCenter(double y)
        {
            int newValue = getValueFromMouseY_Down(y, Host.Height_Down);

            IVertex r = MinusZero.Instance.Root;

            if (IsNote)
            {
                IVertex NoteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

                GraphUtil.SetVertexValue(BaseEdge.To,
                    NoteEvent.Get(false, @"Attribute:Velocity"), newValue);
            }
            else
            {
                IVertex ControlChangeEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");

                GraphUtil.SetVertexValue(BaseEdge.To,
                    ControlChangeEvent.Get(false, @"Attribute:Value"), newValue);
            }
        }

        double horitzontalCenter;
        public double HorizontalCenter {
            get { return horitzontalCenter; }
            set {
                horitzontalCenter = value;

                Canvas.SetLeft(this, horitzontalCenter - (Width / 2));

                CCTopPositionUpdate();
            }
        }        

        double verticalCenter;
        public double VerticalCenter
        {
            get { return verticalCenter; }
            set
            {
                verticalCenter = value;

                if (verticalCenter < 0)
                    verticalCenter = 0;

                if (verticalCenter > Host.Height_Down)
                    verticalCenter = Host.Height_Down;

                Canvas.SetTop(this, verticalCenter);

                Height = Host.Height_Down - verticalCenter;

                updateVertexValueByVerticalCenter(verticalCenter);

                CCTopPositionUpdate();
            }
        }

        public double Top { get; set; }

        public double Bottom { get; set; }
        
    }
}
