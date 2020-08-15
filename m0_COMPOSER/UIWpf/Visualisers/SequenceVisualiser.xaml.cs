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
        // sequencer specyfic

        bool showVelocity;
        int defaultVelocity;
        bool isDrum;

        //

        bool isCurrentPenItemCenter;

        bool showLabel;
                        
        Canvas Main;
        SelectionArea SelectionArea;

        IVertex BaseEdgeToVertex;

        IVertex baseVertex;
        IVertex pitchSetVertex;
        IVertex timeSpanVertex;

        PitchSetAxisDecorator PitchSetAD;
        TimeSpanAxisDecorator TimeSpanAD;

        int Length;
        int ExtendTimeLength;

        double Width;
        double Height;

        double HorizontalNoteMoveLeftRightSpan_Big = 10;
        double HorizontalNoteMoveLeftRightSpan_ItemSizeMiddleBoundary = 20;
        double HorizontalNoteMoveLeftRightSpan_Small = 3;
        double HorizontalNoteMoveLeftRightSpan_ItemSizeSmallBoundary = 9;

        double ArrowDown_MoveOnItem_MouseDown_Delta = 3;

        enum CursorMode { Arrow, Pen, Eraser }

        CursorMode currentCursorMode;

        enum CursorModeDetail {
            ArrowUp,
            ArrowDown,
            ArrowUp_MoveOnItem_Left,
            ArrowUp_MoveOnItem_Right,
            ArrowUp_MoveOnItem,
            ArrowDown_MoveOnItem_Left,
            ArrowDown_MoveOnItem_Right,
            ArrowDown_MoveOnItem_MouseDown,
            ArrowDown_MoveOnItem_MouseDownAndMove,
            PenUp,
            PenDown,
            Eraser }

        CursorModeDetail currentCursorModeDetail;

        Point mouseDownPoint;

        Point previousMousePosition;

        Border newNoteShape;

        AxisSegment newNoteSegment;

        enum SnapToGrid { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32, No_Snap }

        SnapToGrid currentSnapToGrid;

        double currentSnapToGridValue;

        List<FrameworkElement> items;

        bool VertexChangeOff = false;

        FrameworkElement mouseOverItem_Element;

        IItem mouseOverItem;

        double mouseOverItem_startLeft;

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

        void SetMouseOverItem(IItem item)
        {
            if (!(item is FrameworkElement))
                return;

            mouseOverItem = item;
            mouseOverItem_Element = (FrameworkElement)item;
            mouseOverItem_startLeft = item.Left;
        }

        void SetCursorMode(CursorModeDetail modeDetail)
        {
            UnCheckAllCursorButtons();

            currentCursorModeDetail = modeDetail;

            switch (modeDetail)
            {
                case CursorModeDetail.ArrowDown:
                case CursorModeDetail.ArrowUp:
                case CursorModeDetail.ArrowUp_MoveOnItem_Left:
                case CursorModeDetail.ArrowUp_MoveOnItem_Right:
                case CursorModeDetail.ArrowUp_MoveOnItem:
                case CursorModeDetail.ArrowDown_MoveOnItem_Left:
                case CursorModeDetail.ArrowDown_MoveOnItem_Right:
                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDown:
                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove:
                    currentCursorMode = CursorMode.Arrow;

                    ArrowButton.IsChecked = true;
                    break;

                case CursorModeDetail.Eraser:
                    currentCursorMode = CursorMode.Eraser;

                    EraseButton.IsChecked = true;
                    break;

                case CursorModeDetail.PenUp:                
                case CursorModeDetail.PenDown:                
                    currentCursorMode = CursorMode.Pen;

                    PenButton.IsChecked = true;
                    break;
            }
        }

        void UpdateCursorShape()
        {
            switch (currentCursorModeDetail)
            {
                case CursorModeDetail.ArrowDown:
                case CursorModeDetail.ArrowUp:
                case CursorModeDetail.ArrowUp_MoveOnItem:
                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDown:
                    WpfUtil.OverrideCursor(Cursors.Arrow);
                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove:
                    WpfUtil.OverrideCursor(Cursors.SizeAll);
                    break;

                case CursorModeDetail.ArrowUp_MoveOnItem_Left:
                case CursorModeDetail.ArrowUp_MoveOnItem_Right:
                case CursorModeDetail.ArrowDown_MoveOnItem_Left:
                case CursorModeDetail.ArrowDown_MoveOnItem_Right:
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

            SetupLocalVariablesFromBaseVertexVertexes();

            SetAxisDecorators();

            CreateMain();

            SetupScrollViewer();

            DrawMain();
        }

        void SetVertexeVaribles()
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

        void SetupLocalVariablesFromBaseVertexVertexes()
        {
            if (baseVertex.Get(false, "Length:") != null)
                ExtendTimeLength = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "ExtendTimeLength:"));
            else
                ExtendTimeLength = 96 * 16; // default

            if (baseVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "Length:"));
            else
                Length = ExtendTimeLength;

            
            bool dummy = false;

            isDrum = GraphUtil.GetBooleanValue(baseVertex.Get(false, "IsDrum:"), ref dummy);

            if (isDrum)
                isCurrentPenItemCenter = true;
        }

        void SaveLength()
        {
            GraphUtil.SetVertexValue(baseVertex, BaseEdgeToVertex.Get(false, "Length"), Length);
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

            AddEventHandlers();

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

        void AddEventHandlers()
        {
            Main.MouseEnter += MouseEnterHandler;

            Main.MouseLeave += MouseLeaveHandler;

            Main.MouseDown += MouseDownHandler;

            Main.MouseUp += MouseUpHandler;

            Main.MouseMove += MouseMoveHandler;
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            switch (currentCursorModeDetail)
            {
                case CursorModeDetail.PenDown:
                    PenMove_PenDown(sender, e);
                    break;

                case CursorModeDetail.ArrowUp:
                case CursorModeDetail.ArrowUp_MoveOnItem_Left:
                case CursorModeDetail.ArrowUp_MoveOnItem_Right:
                case CursorModeDetail.ArrowUp_MoveOnItem:
                    ArrowMove_ArrowUp(sender, e);
                    break;

                case CursorModeDetail.ArrowDown:
                    ArrowMove_ArrowDown(sender, e);
                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_Left:
                case CursorModeDetail.ArrowDown_MoveOnItem_Right:
                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDown:
                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowMove_DownMoveOnItemLeftRight(sender, e);
                    break;

                default:
                    break;
            }
        } 

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorModeDetail)
            {
                case CursorModeDetail.PenDown:
                    PenUp(sender, e);
                    break;                

                case CursorModeDetail.ArrowDown:
                    ArrowUp_FromDown(sender, e);
                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_Left:
                case CursorModeDetail.ArrowDown_MoveOnItem_Right:
                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowUp_FromMove(sender, e);
                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDown:
                    ArrowUp_FromMoveOnItem_MouseDown(sender, e);
                    break;

                default:
                    break;
            }
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorModeDetail)
            {
                case CursorModeDetail.PenUp:
                    PenDown(sender, e);
                    break;

                case CursorModeDetail.Eraser:
                    EraserDown(sender, e);
                    break;

                case CursorModeDetail.ArrowUp:
                    ArrowDown(sender, e);
                    break;

                case CursorModeDetail.ArrowUp_MoveOnItem_Left:
                case CursorModeDetail.ArrowUp_MoveOnItem_Right:
                case CursorModeDetail.ArrowUp_MoveOnItem:
                    ArrowDown_FromUpMove(sender, e);
                    break;
            }
        }

        void PenDown(object sender, MouseButtonEventArgs e)
        {
            SetCursorMode(CursorModeDetail.PenDown);
            
            mouseDownPoint = GetMainContentMousePosition(e);

            previousMousePosition = mouseDownPoint;

            newNoteSegment = FindVerticalSegment(mouseDownPoint.Y);

            //

            if (isCurrentPenItemCenter)
                return;

            newNoteShape = new Border();

            newNoteShape.Background = (Brush)FindResource("0HighlightBrush");

            newNoteShape.BorderThickness = new Thickness(0);

            double snappedMouseX = GetSnappedPosition(mouseDownPoint.X);

            WpfUtil.SetPositionAbsolute(newNoteShape, snappedMouseX, newNoteSegment.StartPosition, snappedMouseX, newNoteSegment.EndPosition);

            Main.Children.Add(newNoteShape);
        }

        void PenMove_PenDown(object sender, MouseEventArgs e)
        {
            if (isCurrentPenItemCenter)
                return;

            double left, right;

            Point currentMousePosition = GetMainContentMousePosition(e);

            double snappedCurrentMousePositionX = GetSnappedPosition(currentMousePosition.X);

            double snappedMouseDownPointX = GetSnappedPosition(mouseDownPoint.X);

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
        
        void PenUp(object sender, MouseButtonEventArgs e)
        {
            PerformPenUp();

            VertexChangeOff = true;

            IEdge newNoteEventEdge = AddNoteEventEdge(newNoteSegment, Canvas.GetLeft(newNoteShape), newNoteShape.Width);

            AddItem(newNoteEventEdge, null);

            VertexChangeOff = false;
        }

        void EraserDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

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
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                    UnselectItem(item);
                else
                    SelectItem(item);

            }else{           
                SetCursorMode(CursorModeDetail.ArrowDown);

                SelectionArea.StartSelection(currentMousePosition);
            }
        }

        List<IVertex> GetSelectedVertexes()
        {
            List<IVertex> selectedVertexes = new List<IVertex>();

            foreach (IEdge e in Vertex.GetAll(false, @"SelectedEdges:\\To:"))
                selectedVertexes.Add(e.To);

            return selectedVertexes;
        }

            List<IItem> GetSelectedItems()
        {
            List<IVertex> selectedVertexes = new List<IVertex>();

            foreach (IEdge e in Vertex.GetAll(false, @"SelectedEdges:\\To:"))
                selectedVertexes.Add(e.To);

            List<IItem> selectedItems = new List<IItem>();

            foreach (IItem i in items)
                if (selectedVertexes.Contains(i.BaseEdge.To))
                    selectedItems.Add(i);

            return selectedItems;
        }

        List<IItem> GetSelectedAndMouseOverItems()
        {
            List<IItem> selectedItems = GetSelectedItems();

            if (!selectedItems.Contains(mouseOverItem))
                selectedItems.Add(mouseOverItem);

            return selectedItems;
        }

        void InitMouseOverElementAndSelected()
        {
            List<IItem> selectedItems = GetSelectedItems();

            selectedItems.Add(mouseOverItem);

            foreach (IItem i in selectedItems)
                i.SetHiddenFromReal();
        }

        void ArrowDown_FromUpMove(object sender, MouseButtonEventArgs e)
        {
            if (mouseOverItem == null)
                return;

            InitMouseOverElementAndSelected();

            mouseDownPoint = GetMainContentMousePosition(e);

            previousMousePosition = mouseDownPoint;

            if (currentCursorModeDetail == CursorModeDetail.ArrowUp_MoveOnItem_Left)
                SetCursorMode(CursorModeDetail.ArrowDown_MoveOnItem_Left);

            if (currentCursorModeDetail == CursorModeDetail.ArrowUp_MoveOnItem_Right)
                SetCursorMode(CursorModeDetail.ArrowDown_MoveOnItem_Right);

            if (currentCursorModeDetail == CursorModeDetail.ArrowUp_MoveOnItem)            
                SetCursorMode(CursorModeDetail.ArrowDown_MoveOnItem_MouseDown);                            
        }

        enum LeftRightEnum { Left, Right }

        void ItemTryMoveLeftRight(IItem item, double delta, LeftRightEnum LeftRight)
        {
            if (!(item is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)item;

            if (LeftRight == LeftRightEnum.Left)
            {
                if (delta + getSnapMinmalWidth() >= element.Width)
                    return;

                double orginalX = item.Left;

                item.HiddenLeft += delta;

                double newX = GetSnappedPosition(item.HiddenLeft);

                if(newX != orginalX)
                {
                    item.Left = newX;                    

                    element.Width = item.HiddenRight - newX; 
                }
            }
            else
            {
                if (element.Width + delta - getSnapMinmalWidth() <= 0)
                    return;

                double orginalX = item.Right;

                item.HiddenRight += delta;

                double newX = GetSnappedPosition(item.HiddenRight);

                if (newX != orginalX)
                    element.Width = newX - item.Left;
            }
        }

        void ItemTryMove(IItem item, double deltaX, double deltaY)
        {
            if (!(item is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)item;

            // left

            item.HiddenLeft += deltaX;

            double newValue = GetSnappedPosition(item.HiddenLeft);

            if (newValue != item.Left)
                item.Left = newValue;                

            // right

            item.HiddenRight += deltaX;

            newValue = GetSnappedPosition(item.HiddenRight);

            if (newValue != item.Right)
                item.Right = newValue;

            // top / bottom

            item.HiddenTop += deltaY;
            item.HiddenBottom += deltaY;

            AxisSegment newSegment = FindVerticalSegment(item.HiddenTop);

            item.Top = newSegment.StartPosition;            
        }

        void ArrowMove_DownMoveOnItemLeftRight(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            double deltaX = currentMousePosition.X - previousMousePosition.X;
            double deltaY = currentMousePosition.Y - previousMousePosition.Y;

            switch (currentCursorModeDetail)
            {
                case CursorModeDetail.ArrowDown_MoveOnItem_Left:

                    foreach (IItem i in GetSelectedAndMouseOverItems())
                        ItemTryMoveLeftRight(i, deltaX, LeftRightEnum.Left);
                    
                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_Right:

                    foreach (IItem i in GetSelectedAndMouseOverItems())
                        ItemTryMoveLeftRight(i, deltaX, LeftRightEnum.Right);

                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDown:

                    double horizontalDelta = Math.Abs(mouseDownPoint.X - currentMousePosition.X);
                    double verticalDelta = Math.Abs(mouseDownPoint.Y - currentMousePosition.Y);

                    double delta = Math.Sqrt(horizontalDelta * horizontalDelta + verticalDelta * verticalDelta);

                    if (delta > ArrowDown_MoveOnItem_MouseDown_Delta)
                    {
                        SetCursorMode(CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove);
                        UpdateCursorShape();
                    }

                    break;

                case CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove:

                    foreach (IItem i in GetSelectedAndMouseOverItems())
                        ItemTryMove(i, deltaX, deltaY);

                    break;
            }

            previousMousePosition = currentMousePosition;
        }

        void ArrowMove_ArrowUp_SetMouseCurrentItem(IItem item, CursorModeDetail cursorModeDetail)
        {
            SetCursorMode(cursorModeDetail);
            SetMouseOverItem(item);
            UpdateCursorShape();
        }

        void ArrowMove_ArrowUp(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {
                double HorizontalNoteMoveLeftRightSpan = HorizontalNoteMoveLeftRightSpan_Big;

                if(element.Width < HorizontalNoteMoveLeftRightSpan_ItemSizeMiddleBoundary)
                    HorizontalNoteMoveLeftRightSpan = HorizontalNoteMoveLeftRightSpan_Small;

                if (element.Width < HorizontalNoteMoveLeftRightSpan_ItemSizeSmallBoundary)
                    HorizontalNoteMoveLeftRightSpan = 0;

                IItem item = (IItem)element;                

                if (currentMousePosition.X >= item.Left && currentMousePosition.X <= (item.Left + HorizontalNoteMoveLeftRightSpan))
                {
                    ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorModeDetail.ArrowUp_MoveOnItem_Left);
                    return;
                }

                if (currentMousePosition.X >= (item.Right - HorizontalNoteMoveLeftRightSpan) && currentMousePosition.X <= item.Right)
                {
                    ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorModeDetail.ArrowUp_MoveOnItem_Right);
                    return;
                }

                ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorModeDetail.ArrowUp_MoveOnItem);
                return;
            }

            SetCursorMode(CursorModeDetail.ArrowUp);
            UpdateCursorShape();            
        }

        void ArrowMove_ArrowDown(object sender, MouseEventArgs e)
        {      
            Point currentMousePosition = GetMainContentMousePosition(e);

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

        void ArrowUp_FromDown(object sender, MouseButtonEventArgs e)
        {
            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(items, SelectionArea.Left - 1, SelectionArea.Top - 1, SelectionArea.Right + 1, SelectionArea.Bottom + 1);

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

        void ArrowUp_FromMove(object sender, MouseEventArgs e)
        {
            if(currentCursorModeDetail == CursorModeDetail.ArrowDown_MoveOnItem_Left || currentCursorModeDetail == CursorModeDetail.ArrowDown_MoveOnItem_Right)
            {
                SetCursorMode(CursorModeDetail.ArrowUp);

                foreach(IItem i in GetSelectedAndMouseOverItems())
                    UpdateItem_HorizontalPosition(i);
            }

            if (currentCursorModeDetail == CursorModeDetail.ArrowDown_MoveOnItem_MouseDownAndMove)
            {
                SetCursorMode(CursorModeDetail.ArrowUp);

                foreach (IItem i in GetSelectedAndMouseOverItems())
                {
                    UpdateItem_HorizontalPosition(i);

                    UpdateItem_VerticalPosition(i);
                }
            }
        }

        void SelectItem(IItem item)
        {
            item.Select();

            Edge.AddEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
        }

        void UnselectItem(IItem item)
        {
            item.Unselect();

            Edge.DeleteVertexByEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
        }

        void ArrowUp_FromMoveOnItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                    UnselectItem(item);
                else
                    SelectItem(item);
            }

            SetCursorMode(CursorModeDetail.ArrowUp);
        }

        void PerformArrowUp_FromArrowDown_WhileMouseLeave()
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

        AxisSegment FindVerticalSegment(double position)
        {
            foreach (AxisSegment s in PitchSetAD.Segments)
                if (s.StartPosition <= position && position <= s.EndPosition)
                    return s;

            return null;
        }

        double GetSnappedPosition(double position)
        {
            if (currentSnapToGrid == SnapToGrid.No_Snap)
                return position;

            double positionInBars = (position / TimeSpanAD.BaseUnitSize) / TimeSpanAD.BarLength;

            double reminder = positionInBars % currentSnapToGridValue;

            if (reminder < (currentSnapToGridValue / 2.0))
                return (positionInBars - reminder) * TimeSpanAD.BarLength * TimeSpanAD.BaseUnitSize;
            else
                return (positionInBars - reminder + currentSnapToGridValue) * TimeSpanAD.BarLength * TimeSpanAD.BaseUnitSize;
        }

        double getSnapMinmalWidth()
        {
            if (currentSnapToGridValue == 0)
                return 1;

            return currentSnapToGridValue * TimeSpanAD.BarLength * TimeSpanAD.BaseUnitSize;
        }

        AxisSegment GetPitchSegment(IVertex pitchVertex)
        {
            foreach (AxisSegment s in PitchSetAD.Segments)
                if (s.BaseVertex == pitchVertex)
                    return s;

            return null;
        }

        void AddItem(IEdge noteEventEdge, List<IVertex> selectedVertexes)
        {
            IVertex noteEventVertex = noteEventEdge.To;

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex,
                GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            FrameworkElement newItem;
            
            if(isDrum)
                newItem = new DrumItem(noteEventEdge, this, showVelocity);
            else
                newItem = new NoteItem(noteEventEdge, label, this, showLabel, showVelocity);

            if (selectedVertexes != null && selectedVertexes.Contains(noteEventVertex))
                ((IItem)newItem).Select();

            AxisSegment noteSegment = GetPitchSegment(pitchVertex);

            bool dummy = false;

            double startPosition = GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "TriggerTime:"), ref dummy) * TimeSpanAD.BaseUnitSize;

            double endPosition = startPosition + (GraphUtil.GetIntegerValue(noteEventVertex.Get(false, "Length:"), ref dummy) * TimeSpanAD.BaseUnitSize);

            WpfUtil.SetPositionAbsolute(newItem, startPosition, noteSegment.StartPosition, endPosition, noteSegment.EndPosition);

            items.Add(newItem);

            Main.Children.Add(newItem);
        }

        void UpdateItem_HorizontalPosition(IItem item)
        {
            IVertex r = MinusZero.Instance.root;
            IVertex metaTriggerTime = r.Get(false, @"System\Lib\Music\Event\TriggerTime");
            IVertex metaLength = r.Get(false, @"System\Lib\Music\HasLength\Length");

            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex noteEventVertex = item.BaseEdge.To;

            double itemWidth = element.Width;

            int TriggerTime = (int) (item.Left / TimeSpanAD.BaseUnitSize);

            int Length = (int) (itemWidth / TimeSpanAD.BaseUnitSize);

            GraphUtil.SetVertexValue(noteEventVertex, metaTriggerTime, TriggerTime);
            GraphUtil.SetVertexValue(noteEventVertex, metaLength, Length);
        }

        void UpdateItem_VerticalPosition(IItem item)
        {
            IVertex r = MinusZero.Instance.root;
            IVertex metaOctave = r.Get(false, @"System\Lib\Music\Pitch\Octave");
            IVertex metaNote = r.Get(false, @"System\Lib\Music\Pitch\Note");

            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex noteEventVertex = item.BaseEdge.To;

            AxisSegment segment = FindVerticalSegment(item.Top + 1);

            IVertex octaveVertex = segment.BaseVertex.Get(false, "Octave:");
            IVertex noteVertex = segment.BaseVertex.Get(false, "Note:");

            GraphUtil.CreateOrReplaceEdge(noteEventVertex, metaOctave, octaveVertex);
            GraphUtil.CreateOrReplaceEdge(noteEventVertex, metaNote, noteVertex);            

            int? octave = GraphUtil.GetIntegerValue(octaveVertex);
            int? note = GraphUtil.GetIntegerValue(noteVertex);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex, octave, note);

            string label = pitchVertex.Value.ToString();

            item.Label = label;

            item.Update();
        }
        
        IEdge AddNoteEventEdge(AxisSegment noteSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge tempNoteEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Velocity"), defaultVelocity);
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Attribute:Octave"), noteSegment.BaseVertex.Get(false, "Octave:"));
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Attribute:Note"), noteSegment.BaseVertex.Get(false, "Note:"));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:TriggerTime"), (int)((startPosition / TimeSpanAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Length"), (int)((lengthPosition / TimeSpanAD.BaseUnitSize) + 0.01));

            IEdge finalEdge = baseVertex.AddEdge(noteEvent, noteEventVertex);

            baseVertex.DeleteEdge(tempNoteEventEdge);

            return finalEdge;
        }

        Point GetMainContentMousePosition(MouseButtonEventArgs e)
        {
            return e.GetPosition(Main);
        }

        Point GetMainContentMousePosition(MouseEventArgs e)
        {
            return e.GetPosition(Main);
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            WpfUtil.OverrideCursor(Cursors.Arrow);

            switch (currentCursorModeDetail)
            {
                case (CursorModeDetail.PenDown):
                    PerformPenUp();
                    break;

                case (CursorModeDetail.ArrowDown):
                    PerformArrowUp_FromArrowDown_WhileMouseLeave();
                    break;
            }
        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
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
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            foreach (IEdge e in baseVertex.GetAll(false, "NoteEvent:"))
                AddItem(e, selectedVertexes);
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

            BaseEdgeToVertex = mz.root.Get(false, @"System\Lib\Music\Class:Sequence");

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

                SetVertexeVaribles();

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

        private void PenButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorModeDetail.PenUp);
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorModeDetail.Eraser);
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorModeDetail.ArrowUp);
        }

        void UnCheckAllCursorButtons()
        {
            EraseButton.IsChecked = false;
            PenButton.IsChecked = false;
            ArrowButton.IsChecked = false;
        }
    }
}
