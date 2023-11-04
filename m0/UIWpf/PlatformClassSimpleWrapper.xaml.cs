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
        static IVertex scale_meta = r.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale");

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
        IVisualiser Visualiser_Right;

        ContentPresenter Content_Top;
        ContentPresenter Content_Down;
        ScrollViewer Content_Right;

        void SetContentPresenters()
        {
            Content_Top = ((ContentPresenter)((DockPanel)this.Expander_Top.Content).Children[0]);
            Content_Down = (ContentPresenter)this.Expander_Down.Content;
            //Content_Right = (ContentPresenter)this.Expander_Right.Content;
            Content_Right = (ScrollViewer)this.Expander_Right.Content;
        }

        public void SetContent(IPlatformClass pc){
            pcObject = pc;

            BaseEdge = pc.Vertex.Get(false, "BaseEdge:");

            SetContent_Main();
            //SetContent_Right();
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

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

  

            //GraphUtil.SetVertexValue(Visualiser_Down.Vertex, showLineNumbers_meta, "False");
            GraphUtil.SetVertexValue(Visualiser_Down.Vertex, scale_meta, 50);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            Content_Down.Content = Visualiser_Down;
        }

        public void SetContent_Right()
        {


            IVertex b = EdgeHelper.CreateTempEdgeVertex(m0.MinusZero.Instance.empty, m0.MinusZero.Instance.empty, m0.MinusZero.Instance.root);



            VisualisersList.x = true;

            //Visualiser_Right = (TreeVisualiser)PlatformClass.CreatePlatformObject(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Tree"), BaseEdge);

            //Visualiser_Right = new CodeVisualiser(BaseEdge, pcObject.Vertex);
            //Visualiser_Right = new TreeVisualiser(BaseEdge, pcObject.Vertex);
            //Visualiser_Right = new GraphVisualiser(BaseEdge, pcObject.Vertex);
            //Visualiser_Right = new FormVisualiser(BaseEdge, pcObject.Vertex);
            Visualiser_Right = new StringVisualiser(BaseEdge, pcObject.Vertex);
            //Visualiser_Right = new ListVisualiser(BaseEdge, pcObject.Vertex);
            //Visualiser_Right = new WrapVisualiser(BaseEdge, pcObject.Vertex);

            //Visualiser_Right = new CodeVisualiser(b, pcObject.Vertex);
            //Visualiser_Right = new TreeVisualiser(b, pcObject.Vertex);
            //Visualiser_Right = new GraphVisualiser(b, pcObject.Vertex);
            //Visualiser_Right = new FormVisualiser(b, pcObject.Vertex);
            //Visualiser_Right = new StringVisualiser(b, pcObject.Vertex);
            //Visualiser_Right = new ListVisualiser(b, pcObject.Vertex);
            //Visualiser_Right = new WrapVisualiser(b, pcObject.Vertex);


            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            GraphUtil.SetVertexValue(Visualiser_Right.Vertex, scale_meta, 50);

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////

            Content_Right.Content = Visualiser_Right;
        }

        public ScrollViewer GetScrollViewer()
        {
            return this.MainContent;
        }

        // general grip beg

        Point prevMousePosition;

        enum ContentCursorStateEnum { MouseOverUp_Down, MouseOverDown_Down, MouseOverUp_Right, MouseOverDown_Right, MouseOutside }

        ContentCursorStateEnum CursorState;

        private void MouseUp_All(object sender, MouseButtonEventArgs e)
        {
            MouseUp_Down(sender, e);
            MouseUp_Right(sender, e);
        }

        private void MouseMove_All(object sender, MouseEventArgs e)
        {
            MouseMove_Down(sender, e);
            MouseMove_Right(sender, e);
        }

        // general grip end

        // DOWN BEG

        bool ExpanderRightVisible = true;

        private void MouseEnter_Down(object sender, MouseEventArgs e) //
        {
            if (Expander_Down.IsExpanded)
            {
                WpfUtil.SetCursor(Cursors.SizeNS);

                if (CursorState != ContentCursorStateEnum.MouseOverDown_Down)
                    CursorState = ContentCursorStateEnum.MouseOverUp_Down;
            }
            else
                WpfUtil.SetCursor(Cursors.Arrow);
        }

        private void MouseLeave_Down(object sender, MouseEventArgs e) //
        {
            if (CursorState != ContentCursorStateEnum.MouseOverDown_Down)
            {
                WpfUtil.SetCursor(Cursors.Arrow);
                CursorState = ContentCursorStateEnum.MouseOutside;
            }
        }

        private void MouseLeave_Hard(object sender, MouseEventArgs e) //
        {
            WpfUtil.SetCursor(Cursors.Arrow);
            CursorState = ContentCursorStateEnum.MouseOutside;
        }

        private void MouseDown_Down(object sender, MouseButtonEventArgs e) //
        {
            if (CursorState == ContentCursorStateEnum.MouseOverUp_Down)
            {
                CursorState = ContentCursorStateEnum.MouseOverDown_Down;

                prevMousePosition = e.GetPosition(this);
            }
        }

        private void MouseUp_Down(object sender, MouseButtonEventArgs e) //
        {
            if (CursorState == ContentCursorStateEnum.MouseOverDown_Down)
            {
                CursorState = ContentCursorStateEnum.MouseOverUp_Down;

                WpfUtil.SetCursor(Cursors.Arrow);
            }
        }

        private void MouseMove_Down(object sender, MouseEventArgs e) //
        {
            if (CursorState == ContentCursorStateEnum.MouseOverDown_Down)
            {
                Point currentMousePosition = e.GetPosition(this);

                double deltaY = prevMousePosition.Y - currentMousePosition.Y;

                prevMousePosition = currentMousePosition;

                double contentElementHeight = Content_Down.Height + deltaY;

                if (contentElementHeight < 0)
                    contentElementHeight = 0;

                if (contentElementHeight == 0)
                    Expander_Down.IsExpanded = false;

                Content_Down.Height = contentElementHeight;
            }
        }

        private void Expanded_Down(object sender, System.EventArgs e) //
        {
            if (ExpanderDownVisible)
            {
                Grip_Down.Height = 5;
                VerticalGrid.RowDefinitions[1].Height = new GridLength(5);

                if (Content_Down.Content == null)
                    SetContent_Down();

                if(Double.IsNaN(Content_Down.Height))
                    Content_Down.Height = this.ActualHeight / 5;
            }
        }

        private void Collapsed_Down(object sender, System.EventArgs e) //
        {
            Grip_Down.Height = 0;
            VerticalGrid.RowDefinitions[1].Height = new GridLength(0);
        }

        // DOWN END

        // RIGHT BEG

        bool ExpanderDownVisible = true;

        private void MouseEnter_Right(object sender, MouseEventArgs e) //
        {
            if (Expander_Right.IsExpanded)
            {
                WpfUtil.SetCursor(Cursors.SizeWE);

                if (CursorState != ContentCursorStateEnum.MouseOverDown_Right)
                    CursorState = ContentCursorStateEnum.MouseOverUp_Right;
            }
            else
                WpfUtil.SetCursor(Cursors.Arrow);
        }

        private void MouseLeave_Right(object sender, MouseEventArgs e) //
        {
            if (CursorState != ContentCursorStateEnum.MouseOverDown_Right)
            {
                WpfUtil.SetCursor(Cursors.Arrow);
                CursorState = ContentCursorStateEnum.MouseOutside;
            }
        }

        private void MouseDown_Right(object sender, MouseButtonEventArgs e) //
        {
            if (CursorState == ContentCursorStateEnum.MouseOverUp_Right)
            {
                CursorState = ContentCursorStateEnum.MouseOverDown_Right;

                prevMousePosition = e.GetPosition(this);
            }
        }

        private void MouseUp_Right(object sender, MouseButtonEventArgs e) //
        {
            if (CursorState == ContentCursorStateEnum.MouseOverDown_Right)
            {
                CursorState = ContentCursorStateEnum.MouseOverUp_Right;

                WpfUtil.SetCursor(Cursors.Arrow);
            }
        }

        private void MouseMove_Right(object sender, MouseEventArgs e) //
        {
            if (CursorState == ContentCursorStateEnum.MouseOverDown_Right)
            {
                Point currentMousePosition = e.GetPosition(this);

                double deltaX = prevMousePosition.X - currentMousePosition.X;

                prevMousePosition = currentMousePosition;

                double contentElementWidth = Content_Right.Width + deltaX;

                if (contentElementWidth < 0)
                    contentElementWidth = 0;

                if (contentElementWidth == 0)
                    Expander_Right.IsExpanded = false;

                Content_Right.Width = contentElementWidth;
            }
        }

        private void Expanded_Right(object sender, System.EventArgs e) //
        {
            if (ExpanderRightVisible)
            {
                Grip_Right.Width = 5;
                HorizontalGrid.ColumnDefinitions[1].Width = new GridLength(5);

                if (Content_Right.Content == null)
                    SetContent_Right();

                if (Double.IsNaN(Content_Right.Height)) {
                    if (this.ActualWidth < 200)
                        Content_Right.Width = 60;
                    else
                        Content_Right.Width = 150;
                }
            }
        }

        private void Collapsed_Right(object sender, System.EventArgs e) //
        {
            Grip_Right.Width = 0;
            HorizontalGrid.ColumnDefinitions[1].Width = new GridLength(0);
        }

        // RIGHT END
    }
}
