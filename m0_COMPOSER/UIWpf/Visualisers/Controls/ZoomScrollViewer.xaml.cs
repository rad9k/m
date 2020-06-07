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

        ScrollViewer HorizontalAxisDecoratorScrollViewer;
        ScrollViewer VerticalAxisDecoratorScrollViewer;
        Slider HorizontalZoomSlider;
        Slider VerticalZoomSlider;

        public override void OnApplyTemplate()
        {
            HorizontalAxisDecoratorScrollViewer = (ScrollViewer) Template.FindName("HorizontalAxisDecorator", this);

            VerticalAxisDecoratorScrollViewer = (ScrollViewer)Template.FindName("VerticalAxisDecorator", this);

            HorizontalZoomSlider = (Slider)Template.FindName("HorizontalZoomSlider", this);

            VerticalZoomSlider = (Slider)Template.FindName("VerticalZoomSlider", this);
        }

        IZoomScrollViewerAxisDecorator HorizontalAxisDecorator;
        IZoomScrollViewerAxisDecorator VerticalAxisDecorator;

        public void SetHorizontalAxisDecorator(IZoomScrollViewerAxisDecorator decorator)
        {
            HorizontalAxisDecorator = decorator;
            HorizontalAxisDecoratorScrollViewer.Content = decorator;

            HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);
        }

        public void SetVerticalAxisDecorator(IZoomScrollViewerAxisDecorator decorator)
        {
            VerticalAxisDecorator = decorator;
            VerticalAxisDecoratorScrollViewer.Content = decorator;

            VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);
        }

        private void HorizontalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);
        }

        private void VerticalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);
        }
    }
}
