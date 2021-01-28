using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using m0.Foundation;
using System.Windows.Controls;
using m0.UIWpf;
using System.Windows.Media;
using System.Windows;
using m0.Graph;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class NoteItem : Border, IItem
    {
        Canvas Canvas;

        public void Add(Canvas canvas)
        {
            Canvas = canvas;

            Canvas.Children.Add(this);            
        }

        public void Remove()
        {
            Canvas.Children.Remove(this);
        }

        public IEdge BaseEdge { get; set; }

        public bool IsCentered { get { return false; } }

        public String Label { get; set; }        

        public bool CanResizeHorizontally { get { return true; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        public bool IsSelected { get { return isSelected; } }

        bool showLabel;

        bool showVelocity;

        public double HiddenLeft { get; set; }

        public double HiddenRight { get; set; }

        public double HiddenHorizontalCenter { get; set; }

        public double HiddenVerticalCenter { get; set; }

        public double HiddenTop { get; set; }

        public double HiddenBottom { get; set; }

        public void SetHiddenFromReal()
        {
            HiddenLeft = Canvas.GetLeft(this);

            HiddenRight = HiddenLeft + this.Width;

            HiddenTop = Canvas.GetTop(this);

            HiddenBottom = HiddenTop + Height;
        }

        public void Select()
        {
            isSelected = true;

            BorderThickness = new Thickness(3);

            BorderBrush = (Brush)WpfUtil.FindResource("0HighlightBrush");
            
            Background = (Brush)WpfUtil.FindResource("0HighlightBrush");

            if (showLabel)
                labelControl.Background = (Brush)WpfUtil.FindResource("0HighlightBrush");
        }

        public void Unselect()
        {
            isSelected = false;

            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            if (showVelocity)
            {
                Background = velocityColorBrush;

                if (showLabel)
                    labelControl.Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");
            }
            else
            {
                Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

                if (showLabel)
                    labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");
            }
        }

        TextBlock labelControl;

        public NoteItem(IEdge baseEdge, string label, IZoomScrollViewerHost host, bool _showLabel, bool _showVelocity)
        {
            BaseEdge = baseEdge;

            Label = label;

            Host = host;

            showLabel = _showLabel;

            showVelocity = _showVelocity;

            //

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            BorderThickness = new System.Windows.Thickness(2);

            labelControl = new TextBlock();

            this.Child = labelControl;

            if (showLabel)
            {                
                labelControl.Text = " " + Label;
                
                labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");                
            }            

            Update();
           
            Unselect();

            this.SizeChanged += NoteItem_SizeChanged;
        }

        private void NoteItem_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (showLabel)
            {
                if (this.Height > 17 && this.Width > 25)
                    labelControl.Visibility = System.Windows.Visibility.Visible;
                else
                    labelControl.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        Brush velocityColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

        public void Update()
        {
            labelControl.Text = " " + Label;            

            if (showVelocity)
            {
                velocityColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

                int? velocity = GraphUtil.GetIntegerValue(BaseEdge.To.Get(false, "Velocity:"));

                if (velocity != null)
                {
                    byte color = (byte)(255 - ((int)velocity * 2));

                    velocityColorBrush = new SolidColorBrush(Color.FromRgb(color, color, color));

                    if (color > 127 && showLabel)
                        labelControl.Foreground = (Brush)WpfUtil.FindResource("0ForegroundBrush");
                }

                Background = velocityColorBrush;
            }            
        }

        public double Left {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }

        public double Right {
            get { return Left + Width; }
            set { Width = value - Left; }
        }

        public double HorizontalCenter { get; set; }

        public double VerticalCenter { get; set; }

        public double Top
        {
            get { return Canvas.GetTop(this); }
            set { Canvas.SetTop(this, value); }
        }

        public double Bottom
        {
            get { return Top + Height; }
            set { Height = value - Top; }
        }
    }
}
