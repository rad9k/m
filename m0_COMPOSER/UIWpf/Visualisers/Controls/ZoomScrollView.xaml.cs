using m0.UIWpf;
using m0.UIWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public void SetMainContent(FrameworkElement control)
        {
            Scroll.Content = control;
        }        

        ScrollViewer HorizontalAxisDecoratorScrollViewer;
        ScrollViewer VerticalAxisDecoratorScrollViewer;
        Slider HorizontalZoomSlider;
        Slider VerticalZoomSlider;
        Grid Grid;
        AnimatedHideArea DownHideArea;

        public ScrollContentPresenter ContentPresenter;

        IZoomScrollViewerHost Host;
        
        IZoomScrollViewAxisDecorator HorizontalAxisDecorator;
        IZoomScrollViewAxisDecorator VerticalAxisDecorator;        

        enum DownContentCursorStateEnum { MouseOverUp, MouseOverDown, MouseOutside}

        DownContentCursorStateEnum DownCursorState;

        Point prevMousePosition;

        public void SetHorizontalAxisDecorator(IZoomScrollViewAxisDecorator decorator)
        {
            HorizontalAxisDecorator = decorator;
            HorizontalAxisDecoratorScrollViewer.Content = decorator;

            HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);

            Grid.RowDefinitions[0].Height = new GridLength(decorator.Size.Height);
        }

        public void SetVerticalAxisDecorator(IZoomScrollViewAxisDecorator decorator)
        {
            VerticalAxisDecorator = decorator;
            VerticalAxisDecoratorScrollViewer.Content = decorator;

            VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);

            Grid.ColumnDefinitions[0].Width = new GridLength(decorator.Size.Width);
        }

        public void SetHost(IZoomScrollViewerHost host)
        {
            Host = host;
        }

        private void HorizontalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);

            Host.VisualiserDraw();
        }

        private void VerticalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);

            Host.VisualiserDraw();
        }

        private void Scroll_Loaded(object sender, RoutedEventArgs e)
        {
            HorizontalAxisDecoratorScrollViewer = (ScrollViewer)Scroll.Template.FindName("HorizontalAxisDecoratorScrollViewer", Scroll);
            VerticalAxisDecoratorScrollViewer = (ScrollViewer)Scroll.Template.FindName("VerticalAxisDecoratorScrollViewer", Scroll);
            HorizontalZoomSlider = (Slider)Scroll.Template.FindName("HorizontalZoomSlider", Scroll);
            VerticalZoomSlider = (Slider)Scroll.Template.FindName("VerticalZoomSlider", Scroll);
            Grid = (Grid)Scroll.Template.FindName("Grid", Scroll);
            ContentPresenter = (ScrollContentPresenter)Scroll.Template.FindName("PART_ScrollContentPresenter", Scroll);
            DownHideArea = (AnimatedHideArea)Scroll.Template.FindName("DownHideArea", Scroll);
            
            Host.ChildControlsLoaded();

            DownHideArea.Loaded += DownHideArea_Loaded;
        }

        private void DownHideArea_Loaded(object sender, RoutedEventArgs e)
        {
            DownHideArea.IsExpanded = true;
        }

        double HorizontalOffset;
        double VerticalOffset;

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (HorizontalOffset != this.Scroll.HorizontalOffset)
            {
                HorizontalOffset = this.Scroll.HorizontalOffset;

                HorizontalAxisDecoratorScrollViewer.ScrollToHorizontalOffset(HorizontalOffset);
            }

            if (VerticalOffset != this.Scroll.VerticalOffset)
            {
                VerticalOffset = this.Scroll.VerticalOffset;

                VerticalAxisDecoratorScrollViewer.ScrollToVerticalOffset(VerticalOffset);
            }
        }

        private void HorizontalIn(object sender, RoutedEventArgs e)
        {
            if(HorizontalZoomSlider.Value < HorizontalZoomSlider.Maximum)
                HorizontalZoomSlider.Value = HorizontalZoomSlider.Value + 1;
        }

        private void HorizontalOut(object sender, RoutedEventArgs e)
        {
            if (HorizontalZoomSlider.Value > HorizontalZoomSlider.Minimum)
                HorizontalZoomSlider.Value = HorizontalZoomSlider.Value - 1;
        }

        private void VerticalIn(object sender, RoutedEventArgs e)
        {
            if (VerticalZoomSlider.Value < VerticalZoomSlider.Maximum)
                VerticalZoomSlider.Value = VerticalZoomSlider.Value + 1;
        }

        private void VerticalOut(object sender, RoutedEventArgs e)
        {
            if (VerticalZoomSlider.Value > VerticalZoomSlider.Minimum)
                VerticalZoomSlider.Value = VerticalZoomSlider.Value - 1;
        }

        private void DownHideAreaGrip_MouseEnter(object sender, MouseEventArgs e)
        {
            if (DownHideArea.IsExpanded)
            {
                WpfUtil.SetCursor(Cursors.SizeNS);
                DownCursorState = DownContentCursorStateEnum.MouseOverUp;
            }
            else
                WpfUtil.SetCursor(Cursors.Arrow);
        }

        private void DownHideAreaGrip_MouseLeave(object sender, MouseEventArgs e)
        {
            WpfUtil.SetCursor(Cursors.Arrow);

            DownCursorState = DownContentCursorStateEnum.MouseOutside;
        }

        private void DownHideAreaGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DownCursorState == DownContentCursorStateEnum.MouseOverUp)
            {
                DownCursorState = DownContentCursorStateEnum.MouseOverDown;

                prevMousePosition = e.GetPosition(this);
            }
        }

        private void DownHideAreaGrip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DownCursorState == DownContentCursorStateEnum.MouseOverDown)
                DownCursorState = DownContentCursorStateEnum.MouseOverUp;
        }

        private void DownHideAreaGrip_MouseMove(object sender, MouseEventArgs e)
        {
            if (DownCursorState == DownContentCursorStateEnum.MouseOverDown)
            {
                Point currentMousePosition = e.GetPosition(this);

                double deltaY = prevMousePosition.Y - currentMousePosition.Y;

                prevMousePosition = currentMousePosition;

                FrameworkElement contentElement = (FrameworkElement)DownHideArea.Content;

                double contentElementHeight = contentElement.Height + deltaY;

                if (contentElementHeight < 0)
                    contentElementHeight = 0;                

                contentElement.Height = contentElementHeight;

                double downHideAreaHeight = contentElementHeight + 100;

                Grid.RowDefinitions[6].Height = new GridLength(downHideAreaHeight);
            }
        }
    }
}
