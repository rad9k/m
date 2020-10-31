using m0.UIWpf;
using m0.UIWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    /// <summary>
    /// Interaction logic for ZoomScrollViewer.xaml
    /// </summary>
    public partial class ZoomScrollView : UserControl, IZoomScrollView
    {
        bool downAreaVisible = true;

        public bool DownAreaVisible {
            get
            {
                return downAreaVisible;
            }
            set
            {
                downAreaVisible = value;

                UpdateDownHideAreaVisibility();
            }
        }

        bool downAreaIsExpanded = true;

        public bool DownAreaIsExpanded
        {
            get
            {
                if (DownHideArea != null)
                    return DownHideArea.IsExpanded;

                return false;
            }
            set
            {
                downAreaIsExpanded = value;

                if (DownHideArea != null)
                    DownHideArea.IsExpanded = downAreaIsExpanded;

            }
        }

        void UpdateDownHideAreaVisibility()
        {
            if (Grid == null)
                return;

            if (downAreaVisible)
            {
                Grid.RowDefinitions[5].Height = new GridLength(5, GridUnitType.Pixel);
                Grid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Auto);
            }
            else
            {
                Grid.RowDefinitions[5].Height = new GridLength(0, GridUnitType.Pixel);
                Grid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Pixel);
            }
        }

        public ZoomScrollView()
        {
            InitializeComponent();
        }

        public void SetMainContent(FrameworkElement control)
        {
            Scroll.Content = control;
        }

        void DownWidthUpdate()
        {
            if(DownMain != null)
                DownMain.Width = DownHideArea.ActualWidth - VerticalAxisDecorator.Size.Width;
        }

        public void SetDownContent(FrameworkElement downDecoratorContent, FrameworkElement downMainContent)
        {
            DownMain.Content = downMainContent;
            DownDecorator.Child = downDecoratorContent;

            DownMain.Height = InitialDownHeight;
            DownDecorator.Height = InitialDownHeight;

            ((FrameworkElement)DownMain.Content).Width = HorizontalAxisDecorator.Size.Width;
        }

        ScrollViewer HorizontalAxisDecoratorScrollViewer;
        ScrollViewer VerticalAxisDecoratorScrollViewer;
        Slider HorizontalZoomSlider;
        Slider VerticalZoomSlider;
        Grid Grid;
        AnimatedHideArea DownHideArea;
        Border DownGrip;
        Border DownDecorator;
        ScrollViewer DownMain;

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

            double width = decorator.Size.Width;

            Grid.ColumnDefinitions[0].Width = new GridLength(width);

            //

            DownDecorator.Width = width;
            DownWidthUpdate();
        }

        public void SetHost(IZoomScrollViewerHost host)
        {
            Host = host;
        }

        public double InitialDownHeight { get; set; }

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

        void InitializeLocalControlVariables()
        {
            HorizontalAxisDecoratorScrollViewer = (ScrollViewer)Scroll.Template.FindName("HorizontalAxisDecoratorScrollViewer", Scroll);
            VerticalAxisDecoratorScrollViewer = (ScrollViewer)Scroll.Template.FindName("VerticalAxisDecoratorScrollViewer", Scroll);
            HorizontalZoomSlider = (Slider)Scroll.Template.FindName("HorizontalZoomSlider", Scroll);
            VerticalZoomSlider = (Slider)Scroll.Template.FindName("VerticalZoomSlider", Scroll);
            Grid = (Grid)Scroll.Template.FindName("Grid", Scroll);
            ContentPresenter = (ScrollContentPresenter)Scroll.Template.FindName("PART_ScrollContentPresenter", Scroll);

            DownHideArea = (AnimatedHideArea)Scroll.Template.FindName("DownHideArea", Scroll);
            DownGrip = (Border)Scroll.Template.FindName("DownGrip", Scroll);

            DownDecorator = (Border)((StackPanel)DownHideArea.Content).Children[0];
            DownMain = (ScrollViewer)((StackPanel)DownHideArea.Content).Children[1];
        }

        void InitializeLocalControls()
        {
            DownHideArea.IsExpanded = downAreaIsExpanded;

            UpdateDownHideAreaVisibility();
        }

        private void Scroll_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeLocalControlVariables();

            Host.ChildControlsLoaded();

            InitializeLocalControls();
        }
        

        double HorizontalOffset;
        double VerticalOffset;

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (HorizontalOffset != this.Scroll.HorizontalOffset)
            {
                HorizontalOffset = this.Scroll.HorizontalOffset;

                HorizontalAxisDecoratorScrollViewer.ScrollToHorizontalOffset(HorizontalOffset);

                DownMain.ScrollToHorizontalOffset(HorizontalOffset);
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

                if (DownCursorState != DownContentCursorStateEnum.MouseOverDown)
                    DownCursorState = DownContentCursorStateEnum.MouseOverUp;
            }
            else
                WpfUtil.SetCursor(Cursors.Arrow);
        }

        private void DownHideAreaGrip_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DownCursorState != DownContentCursorStateEnum.MouseOverDown)
            {
                WpfUtil.SetCursor(Cursors.Arrow);
                DownCursorState = DownContentCursorStateEnum.MouseOutside;
            }
        }

        private void DownHideAreaGrip_MouseLeave_Hard(object sender, MouseEventArgs e)
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
                

                double contentElementHeight = DownMain.Height + deltaY;

                //double contentElementHeight = Grid.RowDefinitions[6].Height.Value + deltaY;

                if (contentElementHeight < 0)
                    contentElementHeight = 0;

                if (contentElementHeight > this.ActualHeight - 200)
                    contentElementHeight = this.ActualHeight - 200;

                DownDecorator.Height = contentElementHeight;
                DownMain.Height = contentElementHeight;
                
                //double downHideAreaHeight = contentElementHeight;

                //Grid.RowDefinitions[6].Height = new GridLength(downHideAreaHeight);
            }
        }

        private void DownHideArea_Expanded(object sender, System.EventArgs e)
        {
            DownGrip.Background = (Brush)WpfUtil.FindResource("0VeryLightHighlightBrush");

            foreach (Ellipse el in ((StackPanel)DownGrip.Child).Children)
                el.Fill = (Brush)WpfUtil.FindResource("0ForegroundBrush");

        }

        private void DownHideArea_Collapsed(object sender, System.EventArgs e)
        {
            DownGrip.Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");

            foreach (Ellipse el in ((StackPanel)DownGrip.Child).Children)
                el.Fill = (Brush)WpfUtil.FindResource("0LightBackgroundBrush");
        }

        private void Scroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DownWidthUpdate();
        }
    }
}
