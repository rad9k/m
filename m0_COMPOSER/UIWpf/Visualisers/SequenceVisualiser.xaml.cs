using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Controls;
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
            IVertex r = MinusZero.Instance.root;
            //Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;            

            bool dummy = false;

            showLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);
            showVelocity = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowVelocity:"), ref dummy);
            defaultVelocity = GraphUtil.GetIntegerValue(Vertex.Get(false, "DefaultVelocity:"), ref dummy);

            if (Vertex.Get(false, "SnapToGrid:") == null || Vertex.Get(false, "SnapToGrid:").Value.ToString() == "")
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Sequence\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1 bar'"));

            SnapToGridComboBox_SelectionChange();
        }

        bool showLabel;
        bool showVelocity;
        int defaultVelocity;
        
        Canvas Main;
        SelectionArea SelectionArea;

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

        double HorizontalNoteMoveLeftRightSpan = 10;

        enum CursorMode { Arrow, Pen, Eraser }

        CursorMode currentCursorMode;

        enum CursorModeDetail {
            ArrowUp,
            ArrowDown,
            ArrowUp_MoveLeft,
            ArrowUp_MoveRight,
            ArrowDown_Move,
            ArrowDown_MoveLeft,
            ArrowDown_MoveRight,
            PenUp,
            PenDown,
            Eraser }

        CursorModeDetail currentCursorModeDetail;

        Point mouseDownPoint;

        Border newNoteShape;

        AxisSegment newNoteSegment;

        enum SnapToGrid { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32, No_Snap }

        SnapToGrid currentSnapToGrid;

        double currentSnapToGridValue;

        List<FrameworkElement> items;

        bool VertexChangeOff = false;

        void SetCursorMode(CursorModeDetail modeDetail)
        {
            currentCursorModeDetail = modeDetail;

            switch (modeDetail)
            {
                case CursorModeDetail.ArrowDown:
                case CursorModeDetail.ArrowUp:
                case CursorModeDetail.ArrowUp_MoveLeft:
                case CursorModeDetail.ArrowUp_MoveRight:
                case CursorModeDetail.ArrowDown_MoveLeft:
                case CursorModeDetail.ArrowDown_MoveRight:
                case CursorModeDetail.ArrowDown_Move:
                    currentCursorMode = CursorMode.Arrow;
                    break;

                case CursorModeDetail.Eraser:
                    currentCursorMode = CursorMode.Eraser;
                    break;

                case CursorModeDetail.PenUp:                
                case CursorModeDetail.PenDown:                
                    currentCursorMode = CursorMode.Pen;
                    break;
            }
        }

        void UpdateCursorShape()
        {
            switch (currentCursorModeDetail)
            {
                case CursorModeDetail.ArrowDown:
                case CursorModeDetail.ArrowUp:
                    WpfUtil.OverrideCursor(Cursors.Arrow);
                    break;

                case CursorModeDetail.ArrowDown_Move:
                    WpfUtil.OverrideCursor(Cursors.SizeNESW);
                    break;

                case CursorModeDetail.ArrowUp_MoveLeft:
                case CursorModeDetail.ArrowUp_MoveRight:
                case CursorModeDetail.ArrowDown_MoveLeft:
                case CursorModeDetail.ArrowDown_MoveRight:
                    WpfUtil.OverrideCursor(Cursors.SizeWE);
                    break;

                case CursorModeDetail.Eraser:
                    WpfUtil.OverrideCursorFromResource("/m0;component/_resources/basic/eraser.cur");
                    break;

                case CursorModeDetail.PenDown:                
                case CursorModeDetail.PenUp:
                    WpfUtil.OverrideCursorFromResource("/m0;component/_resources/basic/pen.cur");
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

            SelectionArea = new SelectionArea(Main);
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
                case CursorModeDetail.PenDown:
                    PenMove_PenDown(sender, e);
                    break;

                case CursorModeDetail.ArrowUp:
                case CursorModeDetail.ArrowUp_MoveLeft:
                case CursorModeDetail.ArrowUp_MoveRight:
                    ArrowMove_ArrowUp(sender, e);
                    break;

                case CursorModeDetail.ArrowDown:
                case CursorModeDetail.ArrowDown_Move:
                case CursorModeDetail.ArrowDown_MoveLeft:
                case CursorModeDetail.ArrowDown_MoveRight:
                    ArrowMove_ArrowDown(sender, e);
                    break;

                default:
                    break;
            }
        } 

        private void PresenterBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorMode)
            {
                case CursorMode.Pen:
                    PenUp(sender, e);
                    break;                

                case CursorMode.Arrow:
                    ArrowUp(sender, e);
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
        
        void AddItem(IEdge noteEventEdge)
        {
            IVertex noteEventVertex = noteEventEdge.To;

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex,
                GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            NoteItem ni = new NoteItem(noteEventEdge, label, this, showLabel, showVelocity);

            AxisSegment noteSegment = GetPitchSegment(pitchVertex);

            bool dummy=false;

            double startPosition = GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "TriggerTime:"), ref dummy) * TimeSpanAD.BaseUnitSize;

            double endPosition = startPosition + (GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Length:"), ref dummy) * TimeSpanAD.BaseUnitSize);

            WpfUtil.SetPositionAbsolute(ni, startPosition, noteSegment.StartPosition, endPosition, noteSegment.EndPosition);

            items.Add(ni);

            Main.Children.Add(ni);
        }

        IEdge AddNoteEventEdge(AxisSegment noteSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge tempNoteEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddVertex(noteEvent.Get(false, @"Velocity"), defaultVelocity);
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Octave"), noteSegment.BaseVertex.Get(false, "Octave:"));
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Note"), noteSegment.BaseVertex.Get(false, "Note:"));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"TriggerTime"), (int) (startPosition / TimeSpanAD.BaseUnitSize) + 0.01);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Length"), (int) (lengthPosition / TimeSpanAD.BaseUnitSize) + 0.01);

            IEdge finalEdge = baseVertex.AddEdge(noteEvent, noteEventVertex);

            baseVertex.DeleteEdge(tempNoteEventEdge);

            return finalEdge;
        }


        void PenDown(object sender, MouseButtonEventArgs e)
        {
            SetCursorMode(CursorModeDetail.PenDown);
            

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

        void PenMove_PenDown(object sender, MouseEventArgs e)
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

            SetCursorMode(CursorModeDetail.PenUp);            
        }

        void PerformArrowUp_FromArrowDown()
        {
            UnselectAllSelectedEdges();

            foreach (FrameworkElement e in items)
                if (e is IItem)
                {
                    IItem item = (IItem)e;
                    
                    item.Unselect();
                }
            
            SetCursorMode(CursorModeDetail.ArrowUp);

            SelectionArea.HideSelectionArea();
        }

        void PenUp(object sender, MouseButtonEventArgs e)
        {
            PerformPenUp();

            VertexChangeOff = true;

            IEdge newNoteEventEdge = AddNoteEventEdge(newNoteSegment, Canvas.GetLeft(newNoteShape), newNoteShape.Width);

            AddItem(newNoteEventEdge);

            VertexChangeOff = false;
        }

        void EraserDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                IEdge eventEdge = item.BaseEdge;

                VertexChangeOff = true;

                GraphUtil.DeleteEdgeByToVertex(baseVertex, eventEdge.To);

                items.Remove(element);

                Main.Children.Remove(element);

                VertexChangeOff = false;
            }
        }

        void ArrowDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                {
                    item.Unselect();

                    Edge.DeleteVertexByEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
                }
                else
                {
                    item.Select();

                    Edge.AddEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);                    
                }

            }
            else
            {                
                SetCursorMode(CursorModeDetail.ArrowDown);

                SelectionArea.StartSelection(currentMousePosition);
            }
        }

        void ArrowMove_ArrowUp(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                double e_Left = Canvas.GetLeft(element);

                double e_Right = e_Left + element.Width;

                if (currentMousePosition.X >= e_Left && currentMousePosition.X <= (e_Left + HorizontalNoteMoveLeftRightSpan))
                { 
                    SetCursorMode(CursorModeDetail.ArrowUp_MoveLeft);
                    UpdateCursorShape();
                    return;
                }

                if (currentMousePosition.X >= (e_Right - HorizontalNoteMoveLeftRightSpan) && currentMousePosition.X <= e_Right)
                {
                    SetCursorMode(CursorModeDetail.ArrowUp_MoveRight);
                    UpdateCursorShape();
                    return;
                }
            }

            SetCursorMode(CursorModeDetail.ArrowUp);
            UpdateCursorShape();            
        }

        void ArrowMove_ArrowDown(object sender, MouseEventArgs e)
        {      
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            SelectionArea.MoveSelectionArea(currentMousePosition);

            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(items, SelectionArea.Left, SelectionArea.Top, SelectionArea.Right, SelectionArea.Bottom);

            foreach (FrameworkElement _e in items)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        item.Select();
                    else
                        item.Unselect();
                }
                             
            WpfUtil.OverrideCursor(Cursors.Arrow);
        }

        void ArrowUp(object sender, MouseButtonEventArgs e)
        {
            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(items, SelectionArea.Left, SelectionArea.Top, SelectionArea.Right, SelectionArea.Bottom);

            UnselectAllSelectedEdges();

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            UnselectAllSelectedEdges();

            foreach (FrameworkElement _e in items)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                    {
                        Edge.AddEdge(selectedEdges, item.BaseEdge);
                        item.Select();
                    }
                    else
                        item.Unselect();
                }

            currentCursorModeDetail = CursorModeDetail.ArrowUp;
            SelectionArea.HideSelectionArea();
        }

        private void PresenterBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(PresenterBackground);

            if (currentMousePosition.X >= 0 && 
                currentMousePosition.Y >= 0 && 
                currentMousePosition.X <= PresenterBackground.Width && 
                currentMousePosition.Y <= PresenterBackground.Height)
                return;

            WpfUtil.OverrideCursor(Cursors.Arrow);

            switch (currentCursorModeDetail)
            {
                case (CursorModeDetail.PenDown):
                    PerformPenUp();
                    break;

                case (CursorModeDetail.ArrowDown):
                    PerformArrowUp_FromArrowDown();
                    break;
            }
        }

        private void PresenterBackground_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateCursorShape();         
        }

        private void TurnOnSelectedEdgesFireChange()
        {
            if (Vertex.Get(false, "SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get(false, "SelectedEdges:")).CanFireChangeEvent = true;
        }

        private void TurnOffSelectedEdgesFireChange()
        {
            if (Vertex.Get(false, "SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get(false, "SelectedEdges:")).CanFireChangeEvent = false;
        }

        public void UnselectAllSelectedEdges()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            TurnOffSelectedEdgesFireChange();
            
            GraphUtil.RemoveAllEdges(sv);

            TurnOnSelectedEdgesFireChange();         
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
                AddItem(e);
        }

        void InitSequenceVisualierState()
        {
            SetCursorMode(CursorModeDetail.ArrowUp);

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

            if ((sender == Vertex.Get(false, "DefaultVelocity:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateVertexValues();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SnapToGrid")))                
                UpdateVertexValues();

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

        private void SnapToGridComboBox_SelectionChange()
        {            
            switch (Vertex.Get(false, "SnapToGrid:").Value.ToString())
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
            SetCursorMode(CursorModeDetail.PenUp);
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorModeDetail.Eraser);
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorModeDetail.ArrowUp);
        }
    }
}
