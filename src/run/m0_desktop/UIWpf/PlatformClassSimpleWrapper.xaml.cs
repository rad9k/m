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
using m0.ZeroTypes.UX;

namespace m0.UIWpf
{
    /// <summary>
    /// Interaction logic for PlatformClassSimpleWrapper.xaml
    /// </summary>
    public partial class PlatformClassSimpleWrapper : UserControl, IHasScrollViewer
    {
        bool CODE_ON_RIGHT = false;

        public bool IsIntialising;

        bool MainVisualiserHasSelectableEdges = false;

        IVertex currentSelectedEdgesFirst = null;

        IVertex BaseEdge;
        IPlatformClass platformClassObject;

        static IVertex r = m0.MinusZero.Instance.root;

        static IVertex showLineNumbers_meta = r.Get(false, @"System\Meta\Visualiser\Code\ShowLineNumbers");
        static IVertex scale_meta = r.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale");
        static IVertex baseEdge_meta = r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex metaAlignRight_meta = r.Get(false, @"System\Meta\Visualiser\Form\MetaAlignRight");

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

        Border Dummy_Down = new Border();
        Border Dummy_Right = new Border();

        ContentPresenter Content_Top;
        ScrollViewer Content_Down;
        ScrollViewer Content_Right;

        void SetContentPresenters()
        {
            Content_Top = ((ContentPresenter)((DockPanel)this.Expander_Top.Content).Children[0]);
            Content_Down = (ScrollViewer)this.Expander_Down.Content;

            Content_Right = (ScrollViewer)this.Expander_Right.Content;
        }

        public void SetContent(IPlatformClass pc){
            platformClassObject = pc;

            BaseEdge = pc.Vertex.Get(false, "BaseEdge:");

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, platformClassObject.Vertex);

            Content = platformClassObject;

            FrameworkElement fe = (FrameworkElement)platformClassObject;

            if (fe is IOwnScrolling)
            {
                this.MainContent_NoScroll.Child = fe;
                this.MainContent.Visibility = Visibility.Hidden;
            }
            else
            {
                this.MainContent.Content = fe;
                this.MainContent_NoScroll.Visibility = Visibility.Hidden;
            }

            DockPanel.SetDock(fe, Dock.Bottom);

            if (fe is IVisualiser)
            {
                IVisualiser fe_Visualiser = (IVisualiser)fe;

                if (fe_Visualiser.Vertex.Get(false, @"SelectedEdges:") != null)
                {
                    MainVisualiserHasSelectableEdges = true;
                    ((IListVisualiser)fe).SelectedEdgesChange += PlatformClassSimpleWrapper_SelectedEdgesChange;
                }
            }

            Visualiser_Top = new WrapVisualiser(baseEdgeVertex, 0.6, platformClassObject.Vertex, true);

            Content_Top.Content = Visualiser_Top;

            CheckVisibility_DownRight();
        }

        void CheckVisibility_DownRight()
        {
            if (!MainVisualiserHasSelectableEdges)
            {
                ExpanderVisible_Down = false;
                ExpanderVisible_Right = false;

                Expander_Down.Height = 0;
                Expander_Right.Width = 0;
            }
            else
            {
                if (platformClassObject is INoDownVisualiser)
                {
                    VerticalGrid.RowDefinitions[2].Height = new GridLength(0);
                    ExpanderVisible_Down = false;
                }
                else
                    ExpanderVisible_Down = true;

                ExpanderVisible_Right = true;
            }
        }

        private void PlatformClassSimpleWrapper_SelectedEdgesChange()
        {
            IVisualiser platformClassObject_IVisualiser = (IVisualiser)platformClassObject;

            IVertex selectedEdges = GraphUtil.GetQueryOutFirst(platformClassObject_IVisualiser.Vertex, "SelectedEdges", null);

            currentSelectedEdgesFirst = null;

            if (selectedEdges != null)
                currentSelectedEdgesFirst = GraphUtil.GetQueryOutFirst(selectedEdges, "Edge", null);

            if (currentSelectedEdgesFirst != null && 
                GraphUtil.GetBooleanValueOrFalse(platformClassObject_IVisualiser.Vertex.Get(false, "ShowSelectedEdgesBaseEdge:")))
                currentSelectedEdgesFirst = currentSelectedEdgesFirst.Get(false, @"To:\BaseEdge:");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            EnsureContentReady_Down();
            EnsureContentReady_Right();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        void EnsureContentReady_Down()
        {
            if (currentSelectedEdgesFirst != null && Expander_Down.IsExpanded)
                EnsureVisualiserReadyAndSetBaseEdge_Down(currentSelectedEdgesFirst);
            else
                Content_Down.Content = Dummy_Down;
        }

        void EnsureContentReady_Right()
        {
            if (currentSelectedEdgesFirst != null && Expander_Right.IsExpanded)
                EnsureVisualiserReadyAndSetBaseEdge_Right(currentSelectedEdgesFirst);
            else
                Content_Right.Content = Dummy_Right;
        }
    

        public void EnsureVisualiserReadyAndSetBaseEdge_Down(IVertex baseEdge)
        {
            if (Visualiser_Down == null)
            {               
                Visualiser_Down = new CodeVisualiser(baseEdge, platformClassObject.Vertex, true);                

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                GraphUtil.SetVertexValue(Visualiser_Down.Vertex, scale_meta, 80);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                Visualiser_Down.ScaleChange();
            }
            else 
                GraphUtil.CreateOrReplaceEdge(Visualiser_Down.Vertex, baseEdge_meta, baseEdge);

            Content_Down.Content = Visualiser_Down;
        }

        public void EnsureVisualiserReadyAndSetBaseEdge_Right(IVertex baseEdge)
        {
            if (Visualiser_Right == null)
            {                
                if (CODE_ON_RIGHT)
                {
                    Visualiser_Right = new CodeVisualiser(baseEdge, platformClassObject.Vertex, true);                    

                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////

                    GraphUtil.SetVertexValue(Visualiser_Right.Vertex, scale_meta, 80);

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////

                    Visualiser_Right.ScaleChange();
                }
                else
                {
                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////
                    
                    Visualiser_Right = new FormVisualiser(baseEdge, platformClassObject.Vertex, true);

                    GraphUtil.SetVertexValue(Visualiser_Right.Vertex, metaAlignRight_meta, "False");
                    GraphUtil.SetVertexValue(Visualiser_Right.Vertex, scale_meta, 80);

                    //////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    //////////////////////////////////////
                }
            }
            else
                GraphUtil.CreateOrReplaceEdge(Visualiser_Right.Vertex, baseEdge_meta, baseEdge);

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

        bool ExpanderVisible_Down = false;

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
            if (ExpanderVisible_Down)
            {
                Grip_Down.Height = 5;
                VerticalGrid.RowDefinitions[1].Height = new GridLength(5);

                EnsureContentReady_Down();

                if (Double.IsNaN(Content_Down.Height))
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

        bool ExpanderVisible_Right = false;

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
            if (ExpanderVisible_Right)
            {
                Grip_Right.Width = 5;
                HorizontalGrid.ColumnDefinitions[1].Width = new GridLength(5);

                EnsureContentReady_Right();

                if (Double.IsNaN(Content_Right.Height)) {
                    if (this.ActualWidth < 300)
                        Content_Right.Width = this.ActualWidth - 50;
                    else
                        Content_Right.Width = 230;
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
