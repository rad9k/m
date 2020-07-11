using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using m0_COMPOSER.UIWpf.Visualisers.Controls;
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

namespace m0_COMPOSER.UIWpf.Visualisers
{
    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class SequenceVisualiser : UserControl, IPlatformClass, IOwnScrolling, IZoomScrollViewerHost
    {                
        protected void SetVertexDefaultValues()
        {
            //Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;
        }

        Canvas Main;

        IVertex SequenceVertex;

        IVertex baseVertex;
        IVertex pitchSetVertex;
        IVertex timeSpanVertex;

        PitchSetAxisDecorator PitchSetAD;
        TimeSpanAxisDecorator TimeSpanAD;

        Border PresenterBackground;

        int Length;
        int ExtendTimeLength;

        double Width;
        double Height;

        enum CursorMode { Pen, Arrow, Eraser}

        CursorMode currentCursorMode;

        bool penMode_IsInTheMiddleOfDrawing = false;

        enum SnapToGrid { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32}

        SnapToGrid currentSnapToGrid;

        void SetCursorMode(CursorMode mode)
        {
            switch (mode)
            {
                case CursorMode.Arrow:
                    currentCursorMode = CursorMode.Arrow;
                    break;

                case CursorMode.Pen:
                    currentCursorMode = CursorMode.Pen;
                    break;

                case CursorMode.Eraser:
                    currentCursorMode = CursorMode.Eraser;
                    break;
            }
        }

        public void VisualiserDraw()
        {
            if (baseVertex == null || isLoaded == false)
                return;

            SetupParameters();

            SetAxisDecorators();

            CreateMain();

            SetupScrollViewer();

            DrawMain();
        }

        void SetVertexes()
        {            
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return;

            if (baseVertex.Get(false, "$Is:Sequence") == null)
            {
                baseVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;            

            pitchSetVertex = baseVertex.Get(false, "PitchSet:");

            if (pitchSetVertex == null)
                pitchSetVertex = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");

            timeSpanVertex = baseVertex.Get(false, "TimeSpan:");

            if (timeSpanVertex  == null)
                timeSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultTimeSpanLevel:");

        }

        void SetupParameters()
        {
            if (baseVertex.Get(false, "Length:") != null)
                ExtendTimeLength = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "ExtendTimeLength:"));
            else
                ExtendTimeLength = 96 * 16; // default

            if (baseVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "Length:"));
            else
                Length = ExtendTimeLength;
        }

        void SaveLength()
        {
            GraphUtil.SetVertexValue(baseVertex, SequenceVertex.Get(false, "Length"), Length);
        }

        void SetAxisDecorators()
        {
            PitchSetAD = new PitchSetAxisDecorator();

            PitchSetAD.SetBaseVertex(pitchSetVertex);


            TimeSpanAD = new TimeSpanAxisDecorator();

            TimeSpanAD.SetBaseVertex(timeSpanVertex);

            TimeSpanAD.SetLength(Length);


            ZoomScrollView.SetVerticalAxisDecorator(PitchSetAD);

            ZoomScrollView.SetHorizontalAxisDecorator(TimeSpanAD);   
           
        }

        public void CreateMain()
        {
            Main = new Canvas();

            Width = TimeSpanAD.Size.Width;
            Height = PitchSetAD.Size.Height;

            Main.Width = Width;
            Main.Height = Height;            

            ZoomScrollView.SetContent(Main);
        }

        public void SetupScrollViewer()
        {
            ZoomScrollView.SetHost(this);
            ZoomScrollView.SetContent(Main);                        
        }

        public void DrawMain()
        {
            DrawBackground();

            DrawLines();

            DrawPresenterBackground();

            DrawNotes();
        }

        void DrawBackground()
        {
            Border Background = new Border();

            Background.Background = (Brush)FindResource("0LightBackgroundBrush");
            
            WpfUtil.SetPosition(Background, 0, 0, Main.Width, Main.Height);

            Main.Children.Add(Background);
        }

        void DrawPresenterBackground()
        {
            PresenterBackground = new Border();

            PresenterBackground.Background = (Brush)FindResource("0LightBackgroundBrush");

            PresenterBackground.MouseEnter += PresenterBackground_MouseEnter;

            PresenterBackground.MouseLeave += PresenterBackground_MouseLeave;

            PresenterBackground.MouseDown += PresenterBackground_MouseDown;

            PresenterBackground.Opacity = 0.01;

            WpfUtil.SetPosition(PresenterBackground, 0, 0, Main.Width, Main.Height);

            Main.Children.Add(PresenterBackground);
        }

        private void PresenterBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorMode)
            {
                case (CursorMode.Pen):
                    PenDown(sender, e);
                    break;

                case (CursorMode.Eraser):
                    EraserDown(sender, e);
                    break;

                case (CursorMode.Arrow):
                    ArrowDown(sender, e);
                    break;
            }
        }

        void PenDown(object sender, MouseButtonEventArgs e)
        {

        }

        void EraserDown(object sender, MouseButtonEventArgs e)
        {

        }

        void ArrowDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void PresenterBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            WpfUtil.OverrideCursor(Cursors.Arrow);            
        }

        private void PresenterBackground_MouseEnter(object sender, MouseEventArgs e)
        {
            switch (currentCursorMode)
            {
                case CursorMode.Arrow:
                    WpfUtil.OverrideCursor(Cursors.Arrow);
                    break;

                case CursorMode.Eraser:
                    WpfUtil.OverrideCursorFromResource("/m0;component/_resources/basic/eraser.cur");
                    break;

                case CursorMode.Pen:
                    WpfUtil.OverrideCursorFromResource("/m0;component/_resources/basic/pen.cur");
                    break;
            }

            
        }

        public void DrawLines()
        {
            foreach (AxisSegment s in PitchSetAD.Segments)
            {
                if(s.UseBackgroundColor)
                {
                    Border b = new Border();

                    b.Background = new SolidColorBrush(s.BackgroundColor);

                    WpfUtil.SetPosition(b, 0, s.StartPosition, Width, s.EndPosition - s.StartPosition);

                    Main.Children.Add(b);
                }


                Line l = new Line();
                
                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

                Main.Children.Add(l);
                
            }

            foreach (AxisSegment s in TimeSpanAD.Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, s.StartPosition, 0, s.StartPosition, Height);

                s.LineStyle.SetStyle(l);

                Main.Children.Add(l);
            }
        }

        public void DrawNotes()
        {

        }

        void InitSequenceVisualierState()
        {
            SetCursorMode(CursorMode.Arrow);

            currentSnapToGrid = SnapToGrid.Bar1;
        }

        public SequenceVisualiser()
        {
            InitializeComponent();

            MinusZero mz = MinusZero.Instance;

            SequenceVertex = mz.root.Get(false, @"System\Lib\Music\Class:Sequence");

            this.Foreground = (Brush)FindResource("0ForegroundBrush");
            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.BorderThickness = new Thickness(0);
            this.Padding = new Thickness(0);
            this.AllowDrop = true;

            // THIS REDUCES PERFORMANCE ON LARGE TREES SO commented out
            //VirtualizingStackPanel.SetIsVirtualizing(this, true); 
            //VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);

            if (mz != null && mz.IsInitialized)
            {
                //Vertex = mz.Root.Get(false, @"System\Session\Visualisers").AddVertex(null, "TreeVisualiser" + this.GetHashCode());

                Vertex = mz.CreateTempVertex();
                Vertex.Value = "SequenceVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\Sequence"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                SetVertexDefaultValues();

                /*this.ContextMenu = new m0ContextMenu(this);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;
                this.Drop += dndDrop;

                this.MouseEnter += dndMouseEnter;*/

                ZoomScrollView.SetHost(this);

                InitSequenceVisualierState();
            }
        }


        protected void UpdateBaseEdge()
        {
            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (bas != null)
            {
                SetVertexes();

                VisualiserDraw();
            }
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();

            if ((sender == Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))
                UpdateBaseEdge();

            if (sender == Vertex.Get(false, @"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                MinusZero mz = MinusZero.Instance;

                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(false, @"System\Session\Visualisers"), Vertex);

                /*foreach (UIElement e in Children)
                {
                    if (e is StackPanel)
                        foreach (UIElement ee in ((StackPanel)e).Children)
                            if (ee is IDisposable)
                                ((IDisposable)ee).Dispose();
                }*/
            }
        }

        bool isLoaded = false;
        public void ChildControlsLoaded()
        {
            isLoaded = true;

            VisualiserDraw();
        }        

        private void SnapToGridComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SnapToGridComboBox.SelectedItem != null && ((ComboBoxItem)SnapToGridComboBox.SelectedItem).Content != null)
            switch (((ComboBoxItem)SnapToGridComboBox.SelectedItem).Content.ToString())
            {
                case "1 bar":
                    currentSnapToGrid = SnapToGrid.Bar1;
                    break;

                case "1/2 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_2;
                    break;

                case "1/4 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_4;
                    break;

                case "1/8 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_8;
                    break;

                case "1/16 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_16;
                    break;

                case "1/32 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_32;
                    break;
            }
        }

        private void ExtendButton_Click(object sender, RoutedEventArgs e)
        {
            Length += ExtendTimeLength;            

            SaveLength();

            TimeSpanAD.SetLength(Length);

            VisualiserDraw();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorMode.Pen);
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorMode.Eraser);
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorMode.Arrow);
        }

        public void ItemMouseDown(IItem item)
        {

        }
    }
}
