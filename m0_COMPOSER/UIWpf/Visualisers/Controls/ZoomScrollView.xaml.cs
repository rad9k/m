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
    public partial class ZoomScrollView : UserControl, IZoomScrollView
    {
        public ZoomScrollView()
        {
            InitializeComponent();
        }

        public void SetContent(object control)
        {
            Scroll.Content = control;
        }

        ScrollViewer HorizontalAxisDecoratorScrollViewer;
        ScrollViewer VerticalAxisDecoratorScrollViewer;
        Slider HorizontalZoomSlider;
        Slider VerticalZoomSlider;

        IZoomScrollViewerHost Host;
        
        IZoomScrollViewAxisDecorator HorizontalAxisDecorator;
        IZoomScrollViewAxisDecorator VerticalAxisDecorator;

        public void SetHorizontalAxisDecorator(IZoomScrollViewAxisDecorator decorator)
        {
            HorizontalAxisDecorator = decorator;
            HorizontalAxisDecoratorScrollViewer.Content = decorator;

            HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);
        }

        public void SetVerticalAxisDecorator(IZoomScrollViewAxisDecorator decorator)
        {
            VerticalAxisDecorator = decorator;
            VerticalAxisDecoratorScrollViewer.Content = decorator;

            VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);
        }

        public void SetHost(IZoomScrollViewerHost host)
        {
            Host = host;
        }

        private void HorizontalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);
        }

        private void VerticalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);
        }

        private void Scroll_Loaded(object sender, RoutedEventArgs e)
        {
            HorizontalAxisDecoratorScrollViewer = (ScrollViewer)Scroll.Template.FindName("HorizontalAxisDecoratorScrollViewer", Scroll);
            VerticalAxisDecoratorScrollViewer = (ScrollViewer)Scroll.Template.FindName("VerticalAxisDecoratorScrollViewer", Scroll);
            HorizontalZoomSlider = (Slider)Scroll.Template.FindName("HorizontalZoomSlider", Scroll);
            VerticalZoomSlider = (Slider)Scroll.Template.FindName("VerticalZoomSlider", Scroll);

            Host.ChildControlsLoaded();
        }
    }
}
