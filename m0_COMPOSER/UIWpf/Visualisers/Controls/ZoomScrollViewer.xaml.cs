using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    /// <summary>
    /// Interaction logic for ZoomScrollViewer.xaml
    /// </summary>
    public partial class ZoomScrollViewer : UserControl, IZoomScrollView
    {
        public ZoomScrollViewer()
        {
            InitializeComponent();
        }

        public void SetContent(Control control)
        {
            Scroll.Content = control;
        }

        ScrollViewer HorizontalAxisDecorator;
        ScrollViewer VerticalAxisDecorator;

        public override void OnApplyTemplate()
        {
            HorizontalAxisDecorator = (ScrollViewer) Template.FindName("HorizontalAxisDecorator", this);

            VerticalAxisDecorator = (ScrollViewer)Template.FindName("VerticalAxisDecorator", this);
        }

        public void SetHorizontalAxisDecorator(IZoomScrollViewerAxisDecorator decorator)
        {
            HorizontalAxisDecorator.Content = decorator;            
        }

        public void SetVerticalAxisDecorator(IZoomScrollViewerAxisDecorator decorator)
        {
            VerticalAxisDecorator.Content = decorator;
        }
    }
}
