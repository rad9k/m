using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using m0_COMPOSER.Lib;
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
        protected void UpdateVertexValues()
        {
            //Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;            

            bool dummy = false;

            showLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);
            showVelocity = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowVelocity:"), ref dummy);
        }

        bool showLabel;
        bool showVelocity;
        
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

        enum CursorMode { Pen, Arrow, Eraser }

        CursorMode currentCursorMode;

        enum CursorModeDetail { PenUp, PenDown, ArrowUp, ArrowDown, ArrowDown_Move, ArrowDown_MoveLeft, ArrowDown_MoveRight, Eraser }

        CursorModeDetail currentCursorModeDetail;

        Point mouseDownPoint;

        Border newNoteShape;

        AxisSegment newNoteSegment;

        enum SnapToGrid { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32, No_Snap }

        SnapToGrid currentSnapToGrid;

        double currentSnapToGridValue;

        List<FrameworkElement> items;

        bool VertexChangeOff = false;

        void SetCursorMode(CursorMode mode)
        {
            switch (mode)
            {
                case CursorMode.Arrow:
                    currentCursorMode = CursorMode.Arrow;
                    currentCursorModeDetail = CursorModeDetail.ArrowUp;
                    break;

                case CursorMode.Pen:
                    currentCursorMode = CursorMode.Pen;
                    currentCursorModeDetail = CursorModeDetail.PenUp;
                    break;

                case CursorMode.Eraser:
                    currentCursorMode = CursorMode.Eraser;
                    currentCursorModeDetail = CursorModeDetail.Eraser;
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

            if (timeSpanVertex == null)
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

            items = new List<FrameworkElement>();
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
            
            PresenterBackground.MouseUp += PresenterBackground_MouseUp;

            PresenterBackground.MouseMove += PresenterBackground_MouseMove;

            PresenterBackground.Opacity = 0.01;

            Panel.SetZIndex(PresenterBackground, 100);

            WpfUtil.SetPosition(PresenterBackground, 0, 0, Main.Width, Main.Height);

            Main.Children.Add(PresenterBackground);
        }

        private void PresenterBackground_MouseMove(object sender, MouseEventArgs e)
        {
            switch (currentCursorModeDetail)
            {
                case (CursorModeDetail.PenDown):
                    PenDownMove(sender, e);
                    break;

                case (CursorModeDetail.ArrowUp):
                    ArrowMove(sender, e);
                    break;

                default:
                    break;
            }
        } 

        private void PresenterBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorModeDetail)
            {
                case (CursorModeDetail.PenDown):
                    PenUp(sender, e);
                    break;

                default:
                    break;
            }
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

        AxisSegment FindVerticalSegment(double position)
        {
            foreach (AxisSegment s in PitchSetAD.Segments)
                if (s.StartPosition <= position && position <= s.EndPosition)
                    return s;

            return null;
        }

        double GetSnapped(double position)
        {
            if (currentSnapToGrid == SnapToGrid.No_Snap)
                return position;

            double positionInBars = (position / TimeSpanAD.BaseUnitSize) / TimeSpanAD.BarLength;

            double reminder = positionInBars % currentSnapToGridValue;

            if (reminder < (currentSnapToGridValue / 2))
                return (positionInBars - reminder) * TimeSpanAD.BarLength * TimeSpanAD.BaseUnitSize;
            else
                return (positionInBars - reminder + currentSnapToGridValue) * TimeSpanAD.BarLength * TimeSpanAD.BaseUnitSize;            
        }

        AxisSegment GetPitchSegment(IVertex pitchVertex)
        {
            foreach (AxisSegment s in PitchSetAD.Segments)
                if (s.BaseVertex == pitchVertex)
                    return s;

            return null;
        }
        
        void AddItem(IVertex noteEventVertex)
        {
            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex,
                GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            NoteItem ni = new NoteItem(noteEventVertex, label, this, showLabel, showVelocity);

            AxisSegment noteSegment = GetPitchSegment(pitchVertex);

            bool dummy=false;

            double startPosition = GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "TriggerTime:"), ref dummy) * TimeSpanAD.BaseUnitSize;

            double endPosition = startPosition + (GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Length:"), ref dummy) * TimeSpanAD.BaseUnitSize);

            WpfUtil.SetPositionAbsolute(ni, startPosition, noteSegment.StartPosition, endPosition, noteSegment.EndPosition);

            items.Add(ni);

            Main.Children.Add(ni);
        }

        IVertex AddNoteEventVertex(AxisSegment noteSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge tempNoteEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddVertex(noteEvent.Get(false, @"Velocity"), 127);
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Octave"), noteSegment.BaseVertex.Get(false, "Octave:"));
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Note"), noteSegment.BaseVertex.Get(false, "Note:"));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"TriggerTime"), (int) (startPosition / TimeSpanAD.BaseUnitSize) + 0.01);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Length"), (int) (lengthPosition / TimeSpanAD.BaseUnitSize) + 0.01);

            baseVertex.AddEdge(noteEvent, noteEventVertex);

            baseVertex.DeleteEdge(tempNoteEventEdge);

            return noteEventVertex;
        }


        void PenDown(object sender, MouseButtonEventArgs e)
        {
            currentCursorModeDetail = CursorModeDetail.PenDown;

            mouseDownPoint = e.GetPosition(PresenterBackground);

            newNoteSegment = FindVerticalSegment(mouseDownPoint.Y);

            //

            newNoteShape = new Border();

            newNoteShape.Background = (Brush)FindResource("0HighlightBrush");

            newNoteShape.BorderThickness = new Thickness(0);

            double snappedMouseX = GetSnapped(mouseDownPoint.X);

            WpfUtil.SetPositionAbsolute(newNoteShape, snappedMouseX, newNoteSegment.StartPosition, snappedMouseX, newNoteSegment.EndPosition);

            Main.Children.Add(newNoteShape);
        }

        void PenDownMove(object sender, MouseEventArgs e)
        {
            double left, right;

            Point currentMousePosition = e.GetPosition(PresenterBackground);

            double snappedCurrentMousePositionX = GetSnapped(currentMousePosition.X);

            double snappedMouseDownPointX = GetSnapped(mouseDownPoint.X);

            if(snappedCurrentMousePositionX > snappedMouseDownPointX)
            {
                left = snappedMouseDownPointX;
                right = snappedCurrentMousePositionX;
            }
            else
            {
                left = snappedCurrentMousePositionX;
                right = snappedMouseDownPointX;
            }

            WpfUtil.SetPositionAbsolute(newNoteShape, left, newNoteSegment.StartPosition, right, newNoteSegment.EndPosition);
        }

        void PerformPenUp()
        {
            Main.Children.Remove(newNoteShape);

            currentCursorModeDetail = CursorModeDetail.PenUp;
        }

        void PenUp(object sender, MouseButtonEventArgs e)
        {
            PerformPenUp();

            VertexChangeOff = true;

            IVertex newNoteEventVertex = AddNoteEventVertex(newNoteSegment, Canvas.GetLeft(newNoteShape), newNoteShape.Width);

            AddItem(newNoteEventVertex);

            VertexChangeOff = false;
        }

        void EraserDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                IVertex eventVertex = item.BaseVertex;

                VertexChangeOff = true;

                GraphUtil.DeleteEdgeByToVertex(baseVertex, eventVertex);

                items.Remove(element);

                Main.Children.Remove(element);

                VertexChangeOff = false;
            }
        }

        void ArrowDown(object sender, MouseButtonEventArgs e)
        {

        }

        void ArrowMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {

            }else
                WpfUtil.OverrideCursor(Cursors.Arrow);
        }

        private void PresenterBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            WpfUtil.OverrideCursor(Cursors.Arrow);

            switch (currentCursorModeDetail)
            {
                case (CursorModeDetail.PenDown):
                    PerformPenUp();
                    break;
            }
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
            foreach (IEdge e in baseVertex.GetAll(false, "NoteEvent:"))
                AddItem(e.To);
        }

        void InitSequenceVisualierState()
        {
            SetCursorMode(CursorMode.Arrow);

            currentSnapToGrid = SnapToGrid.Bar1;

            currentSnapToGridValue = 1;
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

                UpdateVertexValues();

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
                UpdateVertexValues();

                SetVertexes();

                VisualiserDraw();
            }
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (VertexChangeOff)
                return;

            if ((sender == Vertex.Get(false, "ShowLabel:")) && (e.Type == VertexChangeType.ValueChanged) )
                UpdateBaseEdge();

            if ((sender == Vertex.Get(false, "ShowVelocity:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

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
                    currentSnapToGridValue = 1;
                    break;

                case "1/2 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_2;
                    currentSnapToGridValue = 1.0/2;
                    break;

                case "1/4 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_4;
                    currentSnapToGridValue = 1.0/4;
                    break;

                case "1/8 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_8;
                    currentSnapToGridValue = 1.0/8;
                    break;

                case "1/16 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_16;
                    currentSnapToGridValue = 1.0/16;
                    break;

                case "1/32 bar":
                    currentSnapToGrid = SnapToGrid.Bar1_32;
                    currentSnapToGridValue = 1.0/32;
                    break;

                case "no snap":
                    currentSnapToGrid = SnapToGrid.No_Snap;
                    currentSnapToGridValue = 0;
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
    }
}
