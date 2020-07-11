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

        public void Select()
        {
            isSelected = true;

            Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");

            labelControl.Foreground = (Brush)WpfUtil.FindResource("0ForegroundBrush");
        }

        public void Unselect()
        {
            isSelected = false;

            Background = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");
        }

        TextBlock labelControl;

        public NoteItem(IVertex baseVertex, string label, IZoomScrollViewerHost host)
        {
            BaseVertex = baseVertex;

            Label = label;

            Host = host;

            //

            labelControl = new TextBlock();

            labelControl.Text = " " + Label;

            this.Child = labelControl;

            Unselect();

            this.SizeChanged += NoteItem_SizeChanged;
        }

        private void NoteItem_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.Height > 12)
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
