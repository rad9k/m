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

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    public class NoteItem : Border, IItem
    {
        public IEdge BaseEdge { get; set; }

        public String Label { get; set; }

        public bool CanResizeHorizontally { get { return true; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        public bool IsSelected { get { return isSelected; } }

        bool showLabel;

        bool showVelocity;

        public void Select()
        {
            isSelected = true;

         //   BorderThickness = new Thickness(3);

            BorderBrush = (Brush)WpfUtil.FindResource("0HighlightBrush");

            if (!showVelocity)
            {
                Background = (Brush)WpfUtil.FindResource("0HighlightBrush");

                if (showLabel)
                    labelControl.Background = (Brush)WpfUtil.FindResource("0HighlightBrush");
            }
            else
            {
                if (showLabel)
                    labelControl.Foreground = (Brush)WpfUtil.FindResource("0HighlightBrush");
            }
        }

        public void Unselect()
        {
            isSelected = false;

            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            if (!showVelocity)
            {
                Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

                if (showLabel)
                    labelControl.Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");
            }
            else
            {
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

            if (showLabel)
            {
                labelControl = new TextBlock();                

                labelControl.Text = " " + Label;
                
                labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");

                this.Child = labelControl;
            }

            Brush backColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

            if (showVelocity)
            {
                int? velocity = GraphUtil.GetIntegerValue(BaseEdge.To.Get(false, "Velocity:"));

                if (velocity != null)
                {
                    byte color = (byte)(255 - ((int)velocity * 2));

                    backColorBrush = new SolidColorBrush(Color.FromRgb(color, color, color));

                    if(color > 127 && showLabel)
                        labelControl.Foreground = (Brush)WpfUtil.FindResource("0ForegroundBrush");
                }
            }

            Background = backColorBrush;

            if (showLabel)
                labelControl.Background = backColorBrush;            

            Unselect();

            this.SizeChanged += NoteItem_SizeChanged;
        }

        private void NoteItem_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (showLabel)
            {
                if (this.Height > 18 && this.Width > 25)
                    labelControl.Visibility = System.Windows.Visibility.Visible;
                else
                    labelControl.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void Update()
        {
            labelControl.Text = " " + Label;
        }
    }
}
