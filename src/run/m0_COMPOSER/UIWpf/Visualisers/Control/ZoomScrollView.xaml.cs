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
        public Slider HorizontalZoomSlider_public;
        public Slider VerticalZoomSlider_public;

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
            ScrollViewer.Content = control;
        }

        void DownWidthUpdate()
        {
            if(DownMain != null && VerticalAxisDecorator != null)
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

        public ScrollViewer HorizontalAxisDecoratorScrollViewer;
        public ScrollViewer VerticalAxisDecoratorScrollViewer;
        public Slider HorizontalZoomSlider;
        public Slider VerticalZoomSlider;
        Grid Grid;
        AnimatedHideArea_HorizontalDown DownHideArea;
        Border DownGrip;
        Border DownDecorator;
        ScrollViewer DownMain;
        Border LeftDownCorner;

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
            if (ScrollViewer.Content == null)
                return;

            double half = ScrollViewer.ActualWidth / 2;

            double scrollBarPosAbstract = (this.ScrollViewer.HorizontalOffset + half) / ((FrameworkElement)ScrollViewer.Content).Width;

            //HorizontalAxisDecorator.SetZoomFactor(HorizontalZoomSlider.Value);

            Host.VisualiserDraw();

            double toScroll = (scrollBarPosAbstract * ((FrameworkElement)ScrollViewer.Content).Width) - half;

            SetHorizontalScrollPosition(toScroll);
        }

        private void VerticalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScrollViewer.Content == null)
                return;

            double half =  ScrollViewer.ActualHeight / 2;

            double scrollBarPosAbstract = (this.ScrollViewer.VerticalOffset + half) / ((FrameworkElement)ScrollViewer.Content).Height;

            //VerticalAxisDecorator.SetZoomFactor(VerticalZoomSlider.Value);

            Host.VisualiserDraw();

            SetVerticalScrollPosition((scrollBarPosAbstract * ((FrameworkElement)ScrollViewer.Content).Height) - half);            
        }

        void InitializeLocalControlVariables()
        {
            HorizontalAxisDecoratorScrollViewer = (ScrollViewer)ScrollViewer.Template.FindName("HorizontalAxisDecoratorScrollViewer", ScrollViewer);
            VerticalAxisDecoratorScrollViewer = (ScrollViewer)ScrollViewer.Template.FindName("VerticalAxisDecoratorScrollViewer", ScrollViewer);
            HorizontalZoomSlider = (Slider)ScrollViewer.Template.FindName("HorizontalZoomSlider", ScrollViewer);
            VerticalZoomSlider = (Slider)ScrollViewer.Template.FindName("VerticalZoomSlider", ScrollViewer);
            Grid = (Grid)ScrollViewer.Template.FindName("Grid", ScrollViewer);
            ContentPresenter = (ScrollContentPresenter)ScrollViewer.Template.FindName("PART_ScrollContentPresenter", ScrollViewer);

            DownHideArea = (AnimatedHideArea_HorizontalDown)ScrollViewer.Template.FindName("DownHideArea", ScrollViewer);
            DownGrip = (Border)ScrollViewer.Template.FindName("DownGrip", ScrollViewer);

            DownDecorator = (Border)((StackPanel)DownHideArea.Content).Children[0];
            DownMain = (ScrollViewer)((StackPanel)DownHideArea.Content).Children[1];

            LeftDownCorner = (Border)ScrollViewer.Template.FindName("LeftDownCorner", ScrollViewer);

            HorizontalZoomSlider_public = HorizontalZoomSlider;
            VerticalZoomSlider_public = VerticalZoomSlider;
        }

        void InitializeLocalControls()
        {
            DownHideArea.IsExpanded = downAreaIsExpanded;

            UpdateDownHideAreaVisibility();
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeLocalControlVariables();            

            InitializeLocalControls();

            Host.ChildControlsLoaded();
        }
        

        double HorizontalOffset;
        double VerticalOffset;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateScrollPositions();
        }

        private void SetHorizontalScrollPosition(double newValue)
        {
            this.ScrollViewer.ScrollToHorizontalOffset(newValue);

            HorizontalOffset = this.ScrollViewer.HorizontalOffset;
        }

        private void SetVerticalScrollPosition(double newValue)
        {
            this.ScrollViewer.ScrollToVerticalOffset(newValue);

            VerticalOffset = this.ScrollViewer.VerticalOffset;
        }

        private void UpdateScrollPositions()
        {
            if (HorizontalOffset != this.ScrollViewer.HorizontalOffset)
            {
                HorizontalOffset = this.ScrollViewer.HorizontalOffset;

                HorizontalAxisDecoratorScrollViewer.ScrollToHorizontalOffset(HorizontalOffset);

                DownMain.ScrollToHorizontalOffset(HorizontalOffset);
            }

            if (VerticalOffset != this.ScrollViewer.VerticalOffset)
            {
                VerticalOffset = this.ScrollViewer.VerticalOffset;

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

        // DOWN HIDE AREA BEG

        private void DownHideAreaGrip_MouseEnter(object sender, MouseEventArgs e) //
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

        private void DownHideAreaGrip_MouseLeave(object sender, MouseEventArgs e) //
        {
            if (DownCursorState != DownContentCursorStateEnum.MouseOverDown)
            {
                WpfUtil.SetCursor(Cursors.Arrow);
                DownCursorState = DownContentCursorStateEnum.MouseOutside;
            }
        }

        private void DownHideAreaGrip_MouseLeave_Hard(object sender, MouseEventArgs e) //
        {            
            WpfUtil.SetCursor(Cursors.Arrow);
            DownCursorState = DownContentCursorStateEnum.MouseOutside;         
        }

        private void DownHideAreaGrip_MouseDown(object sender, MouseButtonEventArgs e) //
        {
            if (DownCursorState == DownContentCursorStateEnum.MouseOverUp)
            {
                DownCursorState = DownContentCursorStateEnum.MouseOverDown;

                prevMousePosition = e.GetPosition(this);
            }
        }

        private void DownHideAreaGrip_MouseUp(object sender, MouseButtonEventArgs e) //
        {
            if (DownCursorState == DownContentCursorStateEnum.MouseOverDown)
            {
                WpfUtil.SetCursor(Cursors.Arrow);

                DownCursorState = DownContentCursorStateEnum.MouseOverUp;
            }
        }

        private void DownHideAreaGrip_MouseMove(object sender, MouseEventArgs e) //
        {
            if (DownCursorState == DownContentCursorStateEnum.MouseOverDown)
            {
                Point currentMousePosition = e.GetPosition(this);

                double deltaY = prevMousePosition.Y - currentMousePosition.Y;

                prevMousePosition = currentMousePosition;
                
                double contentElementHeight = DownMain.Height + deltaY;

                if (contentElementHeight < 0)
                    contentElementHeight = 0;

                if (contentElementHeight == 0)
                    DownHideArea.IsExpanded = false;

                if (contentElementHeight > this.ActualHeight - 200)
                    contentElementHeight = this.ActualHeight - 200;

                DownDecorator.Height = contentElementHeight;
                DownMain.Height = contentElementHeight;
            }
        }

        private void DownHideArea_Expanded(object sender, System.EventArgs e) //
        {
            if (downAreaVisible)
            {
                DownGrip.Background = (Brush)WpfUtil.FindResource("0GripBrush");

                DownGrip.Height = 5;
                Grid.RowDefinitions[5].Height = new GridLength(5);

                foreach (Ellipse el in ((StackPanel)DownGrip.Child).Children)
                    el.Fill = (Brush)WpfUtil.FindResource("0ForegroundBrush");
            }
        }

        private void DownHideArea_Collapsed(object sender, System.EventArgs e) //
        {
            DownGrip.Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");

            DownGrip.Height = 0;
            Grid.RowDefinitions[5].Height = new GridLength(0);           

            foreach (Ellipse el in ((StackPanel)DownGrip.Child).Children)
                el.Fill = (Brush)WpfUtil.FindResource("0LightBackgroundBrush");
        }

        // DOWN HIDE AREA END

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DownWidthUpdate();
        }

        public void SetLeftDownCornerControl(FrameworkElement control)
        {            
            LeftDownCorner.Child = control;
        }
    }
}
