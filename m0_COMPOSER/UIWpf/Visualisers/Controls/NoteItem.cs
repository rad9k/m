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
        public IVertex BaseVertex { get; set; }

        public String Label { get; set; }

        public bool CanResizeHorizontally { get { return true; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        bool showLabel;

        bool showVelocity;

        public void Select()
        {
            isSelected = true;

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");

            if (showLabel)
            {
                labelControl.Foreground = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                labelControl.Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");
            }
        }

        public void Unselect()
        {
            isSelected = false;

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            Brush backColorBrush = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

            if (showVelocity) {
                int? velocity = GraphUtil.GetIntegerValue(BaseVertex.Get(false, "Velocity:"));                

                if (velocity != null) {
                    byte color = (byte) (255 - ((int)velocity * 2));

                    backColorBrush = new SolidColorBrush(Color.FromRgb(color, color, color));
                }
            } 

            Background = backColorBrush;

            if (showLabel)
            {
                labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");

                labelControl.Background = backColorBrush;
            }
        }

        TextBlock labelControl;

        public NoteItem(IVertex baseVertex, string label, IZoomScrollViewerHost host, bool _showLabel, bool _showVelocity)
        {
            BaseVertex = baseVertex;

            Label = label;

            Host = host;

            showLabel = _showLabel;

            showVelocity = _showVelocity;

            //

            BorderThickness = new System.Windows.Thickness(2);

            if (showLabel)
            {
                labelControl = new TextBlock();

                labelControl.Text = " " + Label;

                this.Child = labelControl;
            }

            Unselect();

            this.SizeChanged += NoteItem_SizeChanged;
        }

        private void NoteItem_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.Height > 18 && this.Width > 25)
                labelControl.Visibility = System.Windows.Visibility.Visible;
            else
                labelControl.Visibility = System.Windows.Visibility.Hidden;
        }

        public void Update()
        {
            labelControl.Text = " " + Label;
        }
    }
}
