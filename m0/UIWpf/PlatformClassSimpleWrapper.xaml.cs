using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using m0.ZeroTypes;
using m0.Foundation;
using System.Globalization;
using m0.UIWpf.Visualisers;
using m0.Graph;
using Xceed.Wpf.AvalonDock.Layout;
using m0.User.Process.UX;
using m0.UIWpf.Visualisers.Helper;
using m0.UIWpf.Controls;

namespace m0.UIWpf
{
    /// <summary>
    /// Interaction logic for PlatformClassSimpleWrapper.xaml
    /// </summary>
    public partial class PlatformClassSimpleWrapper : UserControl, IHasScrollViewer
    {
        public bool IsIntialising;

        IVertex BaseEdge;
        IPlatformClass pcObject;

        static IVertex r = m0.MinusZero.Instance.root;

        static IVertex showLineNumbers_meta = r.Get(false, @"System\Meta\Visualiser\Code\ShowLineNumbers");

        public PlatformClassSimpleWrapper()
        {
            InitializeComponent();
            
            this.PreviewMouseWheel += PlatformClassSimpleWrapper_PreviewMouseWheel;

            SetContentPresenters();
        }

        private void PlatformClassSimpleWrapper_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Content != null && Content is IMouseWheelHandler)
                ((IMouseWheelHandler)Content).MouseWheelAction(e);

            e.Handled = true;
        }

        private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public void HideEventHandler(object sender, EventArgs e)        
        {
            if (sender is LayoutAnchorable && ((LayoutAnchorable)sender).IsHidden == true)
                if (!IsIntialising)
                    CloseContent();
        }

        public void ClosedEventHandler(object sender, EventArgs e)
        {
            if(!IsIntialising)
                CloseContent();
        }

        private void CloseContent()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            if (Content is IDisposable)
                ((IDisposable)Content).Dispose();

            if (this.Expander_Top.Content is IDisposable)
                ((IDisposable)this.Expander_Top.Content).Dispose();

            if (this.Expander_Down.Content is IDisposable)
                ((IDisposable)this.Expander_Down.Content).Dispose();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        object Content;

        WrapVisualiser Visualiser_Top;
        CodeVisualiser Visualiser_Down;
        FormVisualiser Visualiser_Right;

        ContentPresenter Content_Top;
        ContentPresenter Content_Down;
        ContentPresenter Content_Right;

        void SetContentPresenters()
        {
            Content_Top = ((ContentPresenter)((DockPanel)this.Expander_Top.Content).Children[0]);
            Content_Down = (ContentPresenter)this.Expander_Down.Content;
            //ContentRight = ((ContentPresenter)((DockPanel)this.ExpanderRight.Content).Children[0]);
        }

        public void SetContent(IPlatformClass pc){
            pcObject = pc;

            BaseEdge = pc.Vertex.Get(false, "BaseEdge:");

            SetContent_Main();
        }

        public void SetContent_Main()
        {
            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, pcObject.Vertex);

            Content = pcObject;

            FrameworkElement fe = (FrameworkElement)pcObject;

            if (fe is IOwnScrolling)
            {
                this.ParentCont.Children.Add(fe);
            }
            else
                this.MainContent.Content = fe;

            DockPanel.SetDock(fe, Dock.Bottom);

            
            Visualiser_Top = new WrapVisualiser(baseEdgeVertex, 0.6, pcObject.Vertex);

            Content_Top.Content = Visualiser_Top;
        }

        public void SetContent_Down()
        {
            Visualiser_Down = new CodeVisualiser(BaseEdge, pcObject.Vertex);

            GraphUtil.SetVertexValue(Visualiser_Down.Vertex, showLineNumbers_meta, "False"); 

            Content_Down.Content = Visualiser_Down;
        }

        public void SetContent_Right()
        {
        }

        public ScrollViewer GetScrollViewer()
        {
            return this.MainContent;
        }

        // general grip beg

        Point prevMousePosition;

        enum ContentCursorStateEnum { MouseOverUp, MouseOverDown, MouseOutside }

        // general grip end

        // DOWN BEG

        ContentCursorStateEnum DownCursorState;

        bool ExpanderDownVisible = true;

        private void Down_MouseEnter(object sender, MouseEventArgs e) //
        {
            if (Expander_Down.IsExpanded)
            {
                WpfUtil.SetCursor(Cursors.SizeNS);

                if (DownCursorState != ContentCursorStateEnum.MouseOverDown)
                    DownCursorState = ContentCursorStateEnum.MouseOverUp;
            }
            else
                WpfUtil.SetCursor(Cursors.Arrow);
        }

        private void Down_MouseLeave(object sender, MouseEventArgs e) //
        {
            if (DownCursorState != ContentCursorStateEnum.MouseOverDown)
            {
                WpfUtil.SetCursor(Cursors.Arrow);
                DownCursorState = ContentCursorStateEnum.MouseOutside;
            }
        }

        private void Down_MouseLeave_Hard(object sender, MouseEventArgs e) //
        {
            WpfUtil.SetCursor(Cursors.Arrow);
            DownCursorState = ContentCursorStateEnum.MouseOutside;
        }

        private void Down_MouseDown(object sender, MouseButtonEventArgs e) //
        {
            if (DownCursorState == ContentCursorStateEnum.MouseOverUp)
            {
                DownCursorState = ContentCursorStateEnum.MouseOverDown;

                prevMousePosition = e.GetPosition(this);
            }
        }

        private void Down_MouseUp(object sender, MouseButtonEventArgs e) //
        {
            if (DownCursorState == ContentCursorStateEnum.MouseOverDown)
            {
                DownCursorState = ContentCursorStateEnum.MouseOverUp;

                WpfUtil.SetCursor(Cursors.Arrow);
            }
        }

        private void Down_MouseMove(object sender, MouseEventArgs e) //
        {
            if (DownCursorState == ContentCursorStateEnum.MouseOverDown)
            {
                Point currentMousePosition = e.GetPosition(this);

                double deltaY = prevMousePosition.Y - currentMousePosition.Y;

                prevMousePosition = currentMousePosition;

                double contentElementHeight = Content_Down.Height + deltaY;

                if (contentElementHeight < 0)
                    contentElementHeight = 0;

                if (contentElementHeight == 0)
                    Expander_Down.IsExpanded = false;

                if (contentElementHeight > this.ActualHeight - 200)
                    contentElementHeight = this.ActualHeight - 200;

                Content_Down.Height = contentElementHeight;
            }
        }

        private void Down_Expanded(object sender, System.EventArgs e) //
        {
            if (ExpanderDownVisible)
            {
                DownGrip.Height = 5;
                VerticalGrid.RowDefinitions[1].Height = new GridLength(5);

                if (Content_Down.Content == null)
                    SetContent_Down();

                if(Double.IsNaN(Content_Down.Height))
                    Content_Down.Height = this.ActualHeight / 5;
            }
        }

        private void Down_Collapsed(object sender, System.EventArgs e) //
        {
            DownGrip.Height = 0;
            VerticalGrid.RowDefinitions[1].Height = new GridLength(0);
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Content_Down.Height -= e.VerticalChange;
        }

        // DOWN END
    }
}
