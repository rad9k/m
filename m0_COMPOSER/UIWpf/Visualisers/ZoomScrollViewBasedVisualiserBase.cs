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
using m0_COMPOSER.UIWpf.Visualisers.Control;
using m0_COMPOSER.UIWpf.Visualisers.Control.Item;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public class ZoomScrollViewBasedVisualiserBase : UserControl, IPlatformClass, IOwnScrolling, IZoomScrollViewerHost, IDisposable
    {
        // sequencer specyfic

        protected bool ShowVelocity;
        protected bool ShowArowLines;
        protected bool ShowSnapLines;
        protected int DefaultVelocity;
        protected bool IsDrum;
        protected int CurrentControlChangeNumber;

        // XAML objects

        protected ToggleButton PenButton;
        protected ToggleButton ArrowButton;
        protected ToggleButton EraseButton;
        protected ToggleButton GlueButton;
        protected ToggleButton ScissorsButton;
        protected Button TruncateButton;
        protected Button ExtendButton;

        protected ZoomScrollView ZoomScrollView;

        //

        protected bool IsCurrentPenItemCenter;

        protected bool ShowLabel;

        protected Canvas Main;
        protected SelectionArea SelectionArea;

        protected IVertex BaseEdgeToMetaVertex;
        protected IVertex VisualiserMetaVertex;

        protected IVertex baseVertex;
        protected IVertex verticalSpanVertex;
        protected IVertex horizontalSpanVertex;

        protected IZoomScrollViewAxisDecorator VerticalAD;
        protected IZoomScrollViewAxisDecorator HorizontalAD;

        protected double Width;
        protected double Height;

        protected int Length;
        protected int ExtendTimeLength;

        protected double HorizontalItemMoveLeftRightSpan_Big = 10;
        protected double HorizontalItemMoveLeftRightSpan_ItemSizeMiddleBoundary = 20;
        protected double HorizontalItemMoveLeftRightSpan_Small = 3;
        protected double HorizontalItemMoveLeftRightSpan_ItemSizeSmallBoundary = 9;

        protected double ArrowDown_MoveOnItem_MouseDown_Delta = 3;

        protected enum CursorStateEnum
        {
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
            Eraser,
            Glue,
            Scissors
        }

        protected CursorStateEnum CurrentCursorState;

        protected Point MouseDownPoint;

        protected Point PreviousMousePosition;

        protected Border NewItemShape;

        protected AxisSegment NewItemSegment;

        protected enum SnapToGridEnum { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32, No_Snap }

        protected SnapToGridEnum CurrentSnapToGrid;

        protected double CurrentSnapToGridValue;

        protected List<FrameworkElement> Items;

        protected enum MainDownEnum
        {
            Undefined,
            Main,
            Down
        }

        protected MainDownEnum PreviousSelectedItemContext;

        protected Dictionary<IVertex, IItem> ItemsDictinaryHolder = new Dictionary<IVertex, IItem>();

        protected bool NeedToRebuildItemsDictionary = true;

        protected bool VertexChangeOff = false;

        protected FrameworkElement MuseOverItem_Element;

        protected IItem MouseOverItem;

        protected double MouseOverItem_startLeft;

        protected Line HorizontalArrowLine;
        protected Line VerticalArrowLine;

        protected Line HorizontalArrowLine_Down;
        protected Line VerticalArrowLine_Down;

        // DOWN

        public bool HasDown;

        protected IZoomScrollViewAxisDecorator DownDecorator;

        protected Canvas Down;

        public double Height_Down { get; set; }

        protected MainDownEnum WhereIsMouse;

        protected List<FrameworkElement> Items_Down;

        protected Dictionary<IVertex, IItem> ItemsDictinaryHolder_Down = new Dictionary<IVertex, IItem>();

        protected Dictionary<int, Dictionary<int, List<IItem>>> ItemsDictinaryHolder_Number_TriggerTime_Down = new Dictionary<int, Dictionary<int, List<IItem>>>();

        protected bool NeedToRebuildItemsDictionary_Down = true;

        protected SelectionArea SelectionArea_Down;

        protected bool AllowHorizontalItemMove_Down;

        protected bool MainItemsSyncedWithDown;

        //
     
        protected Dictionary<IVertex, IItem> GetItemsDictionary()
        {
            if (NeedToRebuildItemsDictionary)
                RebuildItemsDictionary();

            return ItemsDictinaryHolder;
        }

        protected void RebuildItemsDictionary()
        {
            ItemsDictinaryHolder.Clear();

            foreach (IItem i in Items)
                ItemsDictinaryHolder.Add(i.BaseEdge.To, i);

            NeedToRebuildItemsDictionary = false;
        }

        protected virtual void UpdateVertexValues() { }

        protected void SetMouseOverItem(IItem item)
        {
            if (!(item is FrameworkElement))
                return;

            MouseOverItem = item;
            MuseOverItem_Element = (FrameworkElement)item;
            MouseOverItem_startLeft = item.Left;
        }

        protected void SetCursorMode(CursorStateEnum modeDetail)
        {
            UnCheckAllCursorButtons();

            CurrentCursorState = modeDetail;

            switch (modeDetail)
            {
                case CursorStateEnum.ArrowDown:
                case CursorStateEnum.ArrowUp:
                case CursorStateEnum.ArrowUp_MoveOnItem_Left:
                case CursorStateEnum.ArrowUp_MoveOnItem_Right:
                case CursorStateEnum.ArrowUp_MoveOnItem:
                case CursorStateEnum.ArrowDown_MoveOnItem_Left:
                case CursorStateEnum.ArrowDown_MoveOnItem_Right:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:

                    SetChecked(ArrowButton, true);                    

                    HideArrowLines();
                    break;

                case CursorStateEnum.Eraser:

                    SetChecked(EraseButton, true);                    
                    break;

                case CursorStateEnum.PenUp:
                case CursorStateEnum.PenDown:

                    SetChecked(PenButton, true);                    
                    break;

                case CursorStateEnum.Glue:

                    SetChecked(GlueButton, true);
                    break;

                case CursorStateEnum.Scissors:

                    SetChecked(ScissorsButton, true);
                    break;
            }
        }

        protected void UpdateCursorShape()
        {
            switch (CurrentCursorState)
            {
                case CursorStateEnum.ArrowDown:
                case CursorStateEnum.ArrowUp:
                case CursorStateEnum.ArrowUp_MoveOnItem:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:
                    WpfUtil.SetCursor(Cursors.Arrow);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:
                    WpfUtil.SetCursor(Cursors.SizeAll);
                    break;

                case CursorStateEnum.ArrowUp_MoveOnItem_Left:
                case CursorStateEnum.ArrowUp_MoveOnItem_Right:
                case CursorStateEnum.ArrowDown_MoveOnItem_Left:
                case CursorStateEnum.ArrowDown_MoveOnItem_Right:
                    WpfUtil.SetCursor(Cursors.SizeWE);
                    break;

                case CursorStateEnum.Eraser:
                    WpfUtil.SetCursorFromResource("/m0;component/_resources/basic/eraser.cur");
                    break;

                case CursorStateEnum.PenDown:
                case CursorStateEnum.PenUp:
                    WpfUtil.SetCursorFromResource("/m0;component/_resources/basic/pen.cur");
                    break;
            }
        }

        bool VisuliseserDrraw_NeedsInitilisation = true;

        public void VisualiserDraw()
        {
            if (baseVertex == null || isLoaded == false)
                return;

            if (VisuliseserDrraw_NeedsInitilisation)
            {
                SetupLocalVariablesFromBaseVertexVertexes();

                UpdateVariablesFromBaseVertex();
            }

            SetAxisDecorators();

            if (VisuliseserDrraw_NeedsInitilisation)
            {
                CreateMain();

                CreateArrowLines();

                SetupScrollViewer();

                CreateDown();

                AddEventHandlers();

                AddEventHandlers_Down();
            }
            else
            {
                ResetDown();
            }

            UpdateMainSize();

            InitialiseItems();

            DrawMain();

            Draw_Down();

            VisuliseserDrraw_NeedsInitilisation = false;
        }

        protected virtual void UpdateVariablesFromBaseVertex() { }

        protected virtual void SetupLocalVariablesFromBaseVertexVertexes() { }

        protected void SaveLength()
        {
            GraphUtil.SetVertexValue(baseVertex, BaseEdgeToMetaVertex.Get(false, "Length"), Length);
        }

        protected virtual void SetAxisDecorators() { }        

        protected void CreateMain()
        {
            Main = new Canvas();

            ZoomScrollView.SetMainContent(Main);
        }

        protected void InitialiseItems()
        {
            Items = new List<FrameworkElement>();

            Items_Down = new List<FrameworkElement>();
        }

        protected void CreateArrowLines()
        {
            HorizontalArrowLine = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
            VerticalArrowLine = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));

            HorizontalArrowLine_Down = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
            VerticalArrowLine_Down = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
        }

        protected void ItemsAdd(IItem i)
        {
            NeedToRebuildItemsDictionary = true;
            Items.Add((FrameworkElement)i);

            i.Add(Main);
        }

        protected void ItemsRemoveAndRemoveAllEdges(IItem i)
        {
            IEdge eventEdge = i.BaseEdge;

            GraphUtil.DeleteEdgeByToVertex(baseVertex, eventEdge.To);

            Edge.DeleteVertexByEdgeTo(Vertex.Get(false, "SelectedEdges:"), eventEdge.To);

            NeedToRebuildItemsDictionary = true;

            Items.Remove((FrameworkElement)i);

            i.Remove();
        }

        protected void SetupScrollViewer()
        {
            ZoomScrollView.SetHost(this);
            ZoomScrollView.SetMainContent(Main);
        }

        protected void UpdateMainSize()
        {
            Width = HorizontalAD.Size.Width;
            Height = VerticalAD.Size.Height;

            Main.Width = Width;
            Main.Height = Height;
        }

        protected void DrawMain()
        {            
            Main.Children.Clear();

            SelectionArea = new SelectionArea(Main);

            DrawBackground();

            DrawMainSegments();

            DrawSnapLines();

            DrawMainLines();

            DrawArrowLines();

            DrawItems();
        }

        protected void DrawBackground()
        {
            Border Background = new Border();

            Background.Background = (Brush)FindResource("0LightBackgroundBrush");

            WpfUtil.SetPosition(Background, 0, 0, Main.Width, Main.Height);

            Main.Children.Add(Background);
        }

        protected void AddEventHandlers()
        {
            Main.MouseEnter += MouseEnterHandler;

            Main.MouseLeave += MouseLeaveHandler;

            Main.MouseDown += MouseDownHandler;

            Main.MouseUp += MouseUpHandler;

            Main.MouseMove += MouseMoveHandler;
        }

        protected void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            UpdateArrowLines(e);

            switch (CurrentCursorState)
            {
                case CursorStateEnum.PenDown:
                    PenMove_PenDown(sender, e);
                    break;

                case CursorStateEnum.ArrowUp:
                case CursorStateEnum.ArrowUp_MoveOnItem_Left:
                case CursorStateEnum.ArrowUp_MoveOnItem_Right:
                case CursorStateEnum.ArrowUp_MoveOnItem:
                    ArrowMove_ArrowUp(sender, e);
                    break;

                case CursorStateEnum.ArrowDown:
                    ArrowMove_ArrowDown(sender, e);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_Left:
                case CursorStateEnum.ArrowDown_MoveOnItem_Right:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowMove_DownMoveOnItemLeftRight(sender, e);
                    break;

                default:
                    break;
            }
        }

        protected void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentCursorState)
            {
                case CursorStateEnum.PenDown:
                    PenUp(sender, e);
                    break;

                case CursorStateEnum.ArrowDown:
                    ArrowUp_FromDown(sender, e);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_Left:
                case CursorStateEnum.ArrowDown_MoveOnItem_Right:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowUp_FromMove(sender, e);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:
                    ArrowUp_FromMoveOnItem_MouseDown(sender, e);
                    break;

                default:
                    break;
            }
        }

        protected void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentCursorState)
            {
                case CursorStateEnum.PenUp:
                    PenDown(sender, e);
                    break;

                case CursorStateEnum.Eraser:
                    EraserDown(sender, e);
                    break;

                case CursorStateEnum.ArrowUp:
                    ArrowDown(sender, e);
                    break;

                case CursorStateEnum.ArrowUp_MoveOnItem_Left:
                case CursorStateEnum.ArrowUp_MoveOnItem_Right:
                case CursorStateEnum.ArrowUp_MoveOnItem:
                    ArrowDown_FromUpMove(sender, e);
                    break;
            }
        }

        protected void UpdateArrowLines(MouseEventArgs e)
        {
            if (!ShowArowLines ||
                !(CurrentCursorState == CursorStateEnum.Eraser ||
                CurrentCursorState == CursorStateEnum.PenDown ||
                CurrentCursorState == CursorStateEnum.PenUp))
            {
                HideArrowLines();

                return;
            }

            double x, y;

            switch (WhereIsMouse)
            {
                case MainDownEnum.Main:

                    HorizontalArrowLine.Visibility = Visibility.Visible;
                    HorizontalArrowLine_Down.Visibility = Visibility.Hidden;

                    VerticalArrowLine.Visibility = Visibility.Visible;
                    VerticalArrowLine_Down.Visibility = Visibility.Visible;

                    Point currentMousePosition = GetMainContentMousePosition(e);

                    x = currentMousePosition.X;
                    y = currentMousePosition.Y;

                    WpfUtil.SetLinePosition(HorizontalArrowLine, 0, y, Width, y);
                    WpfUtil.SetLinePosition(VerticalArrowLine, x, 0, x, Height);
                    WpfUtil.SetLinePosition(VerticalArrowLine_Down, x, 0, x, Height_Down);

                    break;

                case MainDownEnum.Down:

                    HorizontalArrowLine.Visibility = Visibility.Hidden;
                    HorizontalArrowLine_Down.Visibility = Visibility.Visible;

                    VerticalArrowLine.Visibility = Visibility.Visible;
                    VerticalArrowLine_Down.Visibility = Visibility.Visible;

                    Point currentMousePosition2 = GetDownContentMousePosition(e);

                    x = currentMousePosition2.X;
                    y = currentMousePosition2.Y;

                    WpfUtil.SetLinePosition(HorizontalArrowLine_Down, 0, y, Width, y);
                    WpfUtil.SetLinePosition(VerticalArrowLine, x, 0, x, Height);
                    WpfUtil.SetLinePosition(VerticalArrowLine_Down, x, 0, x, Height_Down);

                    break;
            }
        }

        protected void HideArrowLines()
        {
            if (HorizontalArrowLine == null)
                return;

            HorizontalArrowLine.Visibility = Visibility.Hidden;
            VerticalArrowLine.Visibility = Visibility.Hidden;

            HorizontalArrowLine_Down.Visibility = Visibility.Hidden;
            VerticalArrowLine_Down.Visibility = Visibility.Hidden;
        }

        protected void PenDown(object sender, MouseButtonEventArgs e)
        {
            SetCursorMode(CursorStateEnum.PenDown);

            MouseDownPoint = GetMainContentMousePosition(e);

            PreviousMousePosition = MouseDownPoint;

            NewItemSegment = FindVerticalSegment(MouseDownPoint.Y);

            //

            if (IsCurrentPenItemCenter)
                return;

            NewItemShape = new Border();

            NewItemShape.Background = (Brush)FindResource("0HighlightBrush");

            NewItemShape.BorderThickness = new Thickness(0);

            double snappedMouseX = GetSnappedPosition(MouseDownPoint.X);

            WpfUtil.SetPositionAbsolute(NewItemShape, snappedMouseX, NewItemSegment.StartPosition, snappedMouseX, NewItemSegment.EndPosition);

            Main.Children.Add(NewItemShape);
        }

        protected void PenMove_PenDown(object sender, MouseEventArgs e)
        {
            if (IsCurrentPenItemCenter)
                return;

            double left, right;

            Point currentMousePosition = GetMainContentMousePosition(e);

            double snappedCurrentMousePositionX = GetSnappedPosition(currentMousePosition.X);

            double snappedMouseDownPointX = GetSnappedPosition(MouseDownPoint.X);

            if (snappedCurrentMousePositionX > snappedMouseDownPointX)
            {
                left = snappedMouseDownPointX;
                right = snappedCurrentMousePositionX;
            }
            else
            {
                left = snappedCurrentMousePositionX;
                right = snappedMouseDownPointX;
            }

            WpfUtil.SetPositionAbsolute(NewItemShape, left, NewItemSegment.StartPosition, right, NewItemSegment.EndPosition);
        }

        protected void PerformPenUp()
        {
            Main.Children.Remove(NewItemShape);

            SetCursorMode(CursorStateEnum.PenUp);
        }

        protected void PenUp(object sender, MouseButtonEventArgs e)
        {
            PerformPenUp();

            VertexChangeOff = true;

            IEdge newItemEventEdge;

            if (IsCurrentPenItemCenter)
                newItemEventEdge = AddItemEdge(NewItemSegment, GetSnappedPosition(MouseDownPoint.X), 0);
            else
            {
                if (NewItemShape.Width == 0)
                {
                    VertexChangeOff = false;
                    return;
                }

                newItemEventEdge = AddItemEdge(NewItemSegment, Canvas.GetLeft(NewItemShape), NewItemShape.Width);
            }

            AddItem(newItemEventEdge, null);

            VertexChangeOff = false;
        }

        protected void EraserDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                VertexChangeOff = true;

                if (MainItemsSyncedWithDown)
                {
                    IItem item_down = GetItemsDictionary_Down()[item.BaseEdge.To];

                    ItemsRemoveAndRemoveAllEdges_Down(item_down);
                }

                ItemsRemoveAndRemoveAllEdges(item);

                VertexChangeOff = false;
            }
        }

        protected void ArrowDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                    UnselectItem(item);
                else
                    SelectItem(item);

            }
            else
            {
                SetCursorMode(CursorStateEnum.ArrowDown);

                UnselectAllSelectedItems();

                PreviousSelectedItemContext = MainDownEnum.Main;

                SelectionArea.StartSelection(currentMousePosition);
            }
        }

        protected List<IVertex> GetSelectedVertexes()
        {
            List<IVertex> selectedVertexes = new List<IVertex>();

            foreach (IEdge e in Vertex.GetAll(false, @"SelectedEdges:\\To:"))
                selectedVertexes.Add(e.To);

            return selectedVertexes;
        }

        protected List<IItem> GetSelectedItems()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            List<IItem> selectedItems = new List<IItem>();

            foreach (IItem i in Items)
                if (selectedVertexes.Contains(i.BaseEdge.To))
                    selectedItems.Add(i);

            if (WhereIsMouse == MainDownEnum.Down || !MainItemsSyncedWithDown)
                foreach (IItem i in Items_Down)
                    if (selectedVertexes.Contains(i.BaseEdge.To))
                        selectedItems.Add(i);

            return selectedItems;
        }

        protected List<IItem> GetSelectedAndMouseOverItems(MainDownEnum actualContext)
        {
            if (actualContext != PreviousSelectedItemContext)
                UnselectAllSelectedItems();

            List<IItem> selectedItems = GetSelectedItems();

            if (!selectedItems.Contains(MouseOverItem))
                selectedItems.Add(MouseOverItem);

            return selectedItems;
        }

        protected void InitMouseOverElementAndSelected()
        {
            List<IItem> selectedItems = GetSelectedItems();

            selectedItems.Add(MouseOverItem);

            foreach (IItem i in selectedItems)
                i.SetHiddenFromReal();
        }

        protected void ArrowDown_FromUpMove(object sender, MouseButtonEventArgs e)
        {
            if (MouseOverItem == null)
                return;

            InitMouseOverElementAndSelected();

            MouseDownPoint = GetMainContentMousePosition(e);

            PreviousMousePosition = MouseDownPoint;

            if (CurrentCursorState == CursorStateEnum.ArrowUp_MoveOnItem_Left)
                SetCursorMode(CursorStateEnum.ArrowDown_MoveOnItem_Left);

            if (CurrentCursorState == CursorStateEnum.ArrowUp_MoveOnItem_Right)
                SetCursorMode(CursorStateEnum.ArrowDown_MoveOnItem_Right);

            if (CurrentCursorState == CursorStateEnum.ArrowUp_MoveOnItem)
                SetCursorMode(CursorStateEnum.ArrowDown_MoveOnItem_MouseDown);
        }

        protected enum LeftRightEnum { Left, Right }

        protected void ItemTryMoveLeftRight(IItem item, double delta, LeftRightEnum LeftRight)
        {
            if (!(item is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)item;

            if (LeftRight == LeftRightEnum.Left)
            {
                if (delta + GetSnapMinmalWidth() >= element.Width)
                    return;

                double orginalX = item.Left;

                item.HiddenLeft += delta;

                double newX = GetSnappedPosition(item.HiddenLeft);

                if (newX != orginalX)
                {
                    item.Left = newX;

                    element.Width = item.HiddenRight - newX;
                }
            }
            else
            {
                if (element.Width + delta - GetSnapMinmalWidth() <= 0)
                    return;

                double orginalX = item.Right;

                item.HiddenRight += delta;

                double newX = GetSnappedPosition(item.HiddenRight);

                if (newX != orginalX)
                    element.Width = newX - item.Left;
            }
        }

        protected void ItemTryMove(IItem item, double deltaX, double deltaY)
        {
            if (!(item is FrameworkElement))
                return;

            if (WhereIsMouse == MainDownEnum.Down && GetItemContext(item) == MainDownEnum.Main)
                return;

            FrameworkElement element = (FrameworkElement)item;


            if (item.IsCentered)
            {
                // centered

                item.HiddenHorizontalCenter += deltaX;

                double newValue_X = GetSnappedPosition(item.HiddenHorizontalCenter);

                if (newValue_X != item.HorizontalCenter)
                    item.HorizontalCenter = newValue_X;

                item.VerticalCenter += deltaY; // currently only down itens are vertially centered, so we can leave this like this
            }
            else
            {
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
            }

            // top / bottom

            item.HiddenTop += deltaY;
            item.HiddenBottom += deltaY;

            AxisSegment newSegment = FindVerticalSegment(item.HiddenTop + ((item.HiddenBottom - item.HiddenTop) / 2));

            if (newSegment != null)
                item.Top = newSegment.StartPosition;
        }

        protected void ArrowMove_DownMoveOnItemLeftRight(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            double deltaX = currentMousePosition.X - PreviousMousePosition.X;
            double deltaY = currentMousePosition.Y - PreviousMousePosition.Y;

            switch (CurrentCursorState)
            {
                case CursorStateEnum.ArrowDown_MoveOnItem_Left:

                    foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                        ItemTryMoveLeftRight(i, deltaX, LeftRightEnum.Left);

                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_Right:

                    foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                        ItemTryMoveLeftRight(i, deltaX, LeftRightEnum.Right);

                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:

                    double horizontalDelta = Math.Abs(MouseDownPoint.X - currentMousePosition.X);
                    double verticalDelta = Math.Abs(MouseDownPoint.Y - currentMousePosition.Y);

                    double delta = Math.Sqrt(horizontalDelta * horizontalDelta + verticalDelta * verticalDelta);

                    if (delta > ArrowDown_MoveOnItem_MouseDown_Delta)
                    {
                        SetCursorMode(CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove);
                        UpdateCursorShape();
                    }

                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:

                    foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                        ItemTryMove(i, deltaX, deltaY);

                    break;
            }

            PreviousMousePosition = currentMousePosition;
        }

        protected void ArrowMove_ArrowUp_SetMouseCurrentItem(IItem item, CursorStateEnum cursorModeDetail)
        {
            SetCursorMode(cursorModeDetail);
            SetMouseOverItem(item);
            UpdateCursorShape();
        }

        protected void ArrowMove_ArrowUp(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items, currentMousePosition);

            if (element != null && element is IItem)
            {
                double HorizontalItemMoveLeftRightSpan = HorizontalItemMoveLeftRightSpan_Big;

                if (element.Width < HorizontalItemMoveLeftRightSpan_ItemSizeMiddleBoundary)
                    HorizontalItemMoveLeftRightSpan = HorizontalItemMoveLeftRightSpan_Small;

                if (element.Width < HorizontalItemMoveLeftRightSpan_ItemSizeSmallBoundary)
                    HorizontalItemMoveLeftRightSpan = 0;

                IItem item = (IItem)element;

                if (!IsCurrentPenItemCenter && currentMousePosition.X >= item.Left && currentMousePosition.X <= (item.Left + HorizontalItemMoveLeftRightSpan))
                {
                    ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorStateEnum.ArrowUp_MoveOnItem_Left);
                    return;
                }

                if (!IsCurrentPenItemCenter && currentMousePosition.X >= (item.Right - HorizontalItemMoveLeftRightSpan) && currentMousePosition.X <= item.Right)
                {
                    ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorStateEnum.ArrowUp_MoveOnItem_Right);
                    return;
                }

                ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorStateEnum.ArrowUp_MoveOnItem);
                return;
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
            UpdateCursorShape();
        }

        protected void ArrowMove_ArrowDown(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            SelectionArea.MoveSelectionArea(currentMousePosition);

            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(Items, SelectionArea.Left - 2, SelectionArea.Top - 2, SelectionArea.Right + 2, SelectionArea.Bottom + 2);

            foreach (FrameworkElement _e in Items)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        item.Select();
                    else
                        item.Unselect();
                }

            WpfUtil.SetCursor(Cursors.Arrow);
        }

        protected void SaveSelectionArea()
        {
            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(Items, SelectionArea.Left - 2, SelectionArea.Top - 2, SelectionArea.Right + 2, SelectionArea.Bottom + 2);

            UnselectAllSelectedEdges();

            foreach (FrameworkElement _e in Items)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        SelectItem(item);
                    else
                        item.Unselect();
                }

            CurrentCursorState = CursorStateEnum.ArrowUp;
            SelectionArea.HideSelectionArea();
        }

        protected void ArrowUp_FromDown(object sender, MouseButtonEventArgs e)
        {
            SaveSelectionArea();
        }

        protected void ArrowUp_FromMove(object sender, MouseEventArgs e)
        {
            if (CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_Left || CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_Right)
            {
                SetCursorMode(CursorStateEnum.ArrowUp);

                foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                    UpdateItem_HorizontalPosition(i);
            }

            if (CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove)
            {
                SetCursorMode(CursorStateEnum.ArrowUp);

                foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                {
                    UpdateItem_HorizontalPosition(i);

                    UpdateItem_VerticalPosition(i);
                }
            }

            if (MainItemsSyncedWithDown)
                Draw_Down();
        }

        protected MainDownEnum GetItemContext(IItem item)
        {
            if (item is NoteItem || item is DrumItem)
                return MainDownEnum.Main;

            if (item is ControlChangeItem)
                return MainDownEnum.Down;

            return MainDownEnum.Undefined; // fallback
        }

        protected void SelectItem(IItem item)
        {
            MainDownEnum ic = GetItemContext(item);

            if (PreviousSelectedItemContext != ic)
                UnselectAllSelectedItems();

            item.Select();

            Edge.AddEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);

            PreviousSelectedItemContext = ic;
        }

        protected void UnselectItem(IItem item)
        {
            item.Unselect();

            Edge.DeleteVertexByEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
        }

        protected void ArrowUp_FromMoveOnItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                    UnselectItem(item);
                else
                    SelectItem(item);
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
        }

        protected void PerformArrowUp_FromArrowDown_WhileMouseLeave()
        {
            SaveSelectionArea();
        }

        protected AxisSegment FindVerticalSegment(double position)
        {
            foreach (AxisSegment s in VerticalAD.Segments)
                if (s.StartPosition < position && position < s.EndPosition)
                    return s;

            return null;
        }

        protected double GetSnappedPosition(double position)
        {
            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double positionInBars = (position / HorizontalAD.BaseUnitSize) / HorizontalAD.BarLength;

            double reminder = positionInBars % CurrentSnapToGridValue;

            if (reminder < (CurrentSnapToGridValue / 2.0))
                return (positionInBars - reminder) * HorizontalAD.BarLength * HorizontalAD.BaseUnitSize;
            else
                return (positionInBars - reminder + CurrentSnapToGridValue) * HorizontalAD.BarLength * HorizontalAD.BaseUnitSize;
        }

        protected double GetSnapMinmalWidth()
        {
            if (CurrentSnapToGridValue == 0)
                return 1;

            return CurrentSnapToGridValue * HorizontalAD.BarLength * HorizontalAD.BaseUnitSize;
        }

        protected AxisSegment GetPitchSegment(IVertex pitchVertex)
        {
            foreach (AxisSegment s in VerticalAD.Segments)
                if (s.BaseVertex == pitchVertex)
                    return s;

            return null;
        }

        protected void AddItem(IEdge itemEdge, List<IVertex> selectedVertexes)
        {
            IVertex itemEventVertex = itemEdge.To;

            bool dummy = false;

            int triggerTime = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "TriggerTime:"), ref dummy);

            int length = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Length:"), ref dummy);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex,
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            FrameworkElement newElement;

            if (IsDrum)
                newElement = new DrumItem(itemEdge, this, ShowVelocity);
            else
                newElement = new NoteItem(itemEdge, label, this, ShowLabel, ShowVelocity);

            IItem newItem = (IItem)newElement;

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                newItem.Select();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }

            AxisSegment itemSegment = GetPitchSegment(pitchVertex);


            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            double endPosition = startPosition + (length * HorizontalAD.BaseUnitSize);


            if (IsDrum)
            {
                newItem.HorizontalCenter = startPosition;
                newItem.Top = itemSegment.StartPosition;
                newItem.Bottom = itemSegment.EndPosition;
            }
            else
            {
                newItem.Left = startPosition;
                newItem.Top = itemSegment.StartPosition;
                newItem.Right = endPosition;
                newItem.Bottom = itemSegment.EndPosition;
            }

            ItemsAdd(newItem);

            if (MainItemsSyncedWithDown)
                AddItem_Down(itemEdge, selectedVertexes, false, true);
        }

        protected void UpdateItem_HorizontalPosition(IItem item)
        {
            IVertex r = MinusZero.Instance.root;
            IVertex metaTriggerTime = r.Get(false, @"System\Lib\Music\Event\TriggerTime");
            IVertex metaLength = r.Get(false, @"System\Lib\Music\HasLength\Length");

            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            double itemWidth = element.Width;

            int TriggerTime;

            int Length;

            if (item.IsCentered)
            {
                TriggerTime = (int)(item.HorizontalCenter / HorizontalAD.BaseUnitSize);

                Length = 0;
            }
            else
            {
                TriggerTime = (int)(item.Left / HorizontalAD.BaseUnitSize);

                Length = (int)(itemWidth / HorizontalAD.BaseUnitSize);
            }

            GraphUtil.SetVertexValue(itemVertex, metaTriggerTime, TriggerTime);

            if (Length != 0)
                GraphUtil.SetVertexValue(itemVertex, metaLength, Length);
        }

        protected void UpdateItem_VerticalPosition(IItem item)
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

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex, octave, note);

            string label = pitchVertex.Value.ToString();

            item.Label = label;

            item.Update();
        }

        protected IEdge AddItemEdge(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge tempNoteEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, noteEvent);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Velocity"), DefaultVelocity);
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Attribute:Octave"), itemSegment.BaseVertex.Get(false, "Octave:"));
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Attribute:Note"), itemSegment.BaseVertex.Get(false, "Note:"));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:TriggerTime"), (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Length"), (int)((lengthPosition / HorizontalAD.BaseUnitSize) + 0.01));

            IEdge finalEdge = baseVertex.AddEdge(Event, noteEventVertex);

            baseVertex.DeleteEdge(tempNoteEventEdge);

            return finalEdge;
        }

        protected Point GetMainContentMousePosition(MouseButtonEventArgs e)
        {
            return e.GetPosition(Main);
        }

        protected Point GetMainContentMousePosition(MouseEventArgs e)
        {
            return e.GetPosition(Main);
        }

        protected void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            HideArrowLines();

            WpfUtil.SetCursor(Cursors.Arrow);

            switch (CurrentCursorState)
            {
                case (CursorStateEnum.PenDown):
                    PerformPenUp();
                    break;

                case (CursorStateEnum.ArrowDown):
                case (CursorStateEnum.ArrowDown_MoveOnItem_Left):
                case (CursorStateEnum.ArrowDown_MoveOnItem_MouseDown):
                case (CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove):
                case (CursorStateEnum.ArrowDown_MoveOnItem_Right):
                    PerformArrowUp_FromArrowDown_WhileMouseLeave();
                    break;
            }

            WhereIsMouse = MainDownEnum.Undefined;
        }

        protected void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            UpdateCursorShape();

            WhereIsMouse = MainDownEnum.Main;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /////////////// DOWN START /////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////        

        protected bool ApplyFilter_Down(IVertex baseVertex)
        {
            AxisSegment s;

            if (VerticalAD.Selection == null)
                return true;

            s = (AxisSegment)VerticalAD.Selection;

            IVertex noteVertex = s.BaseVertex;

            if (baseVertex.Get(false, @"Note:") == noteVertex.Get(false, @"Note:")
                && baseVertex.Get(false, @"Octave:") == noteVertex.Get(false, @"Octave:"))
                return true;

            return false;
        }        

        protected Point GetDownContentMousePosition(MouseButtonEventArgs e)
        {
            return e.GetPosition(Down);
        }

        protected Point GetDownContentMousePosition(MouseEventArgs e)
        {
            return e.GetPosition(Down);
        }

        protected void SetCurrentControlChangeNumber(int ccnum)
        {
            CurrentControlChangeNumber = ccnum;

            if (CurrentControlChangeNumber == -1)
            {
                MainItemsSyncedWithDown = true;
                AllowHorizontalItemMove_Down = false;
            }
            else
            {
                MainItemsSyncedWithDown = false;
                AllowHorizontalItemMove_Down = true;
            }
        }

        protected void AddEventHandlers_Down()
        {
            if (HasDown)
            {
                Down.MouseEnter += MouseEnterHandler_Down;

                Down.MouseLeave += MouseLeaveHandler_Down;

                Down.MouseDown += MouseDownHandler_Down;

                Down.MouseUp += MouseUpHandler_Down;

                Down.MouseMove += MouseMoveHandler_Down;
            }
        }

        protected void CreateDown()
        {
            if (!HasDown)
                return;

            ZoomScrollView.InitialDownHeight = 100;

            DownDecorator = new ControlChangeDownDecorator();

            SetCurrentControlChangeNumber((int)DownDecorator.Selection);

            DownDecorator.SelectionChanged += DownDecorator_SelectionChanged;

            Down = new Canvas();

            Down.SizeChanged += Down_SizeChanged;
            Down.Loaded += Down_Loaded;

            ZoomScrollView.SetDownContent((FrameworkElement)DownDecorator, Down);

            VerticalAD.SelectionChanged += VerticalAD_SelectionChanged_Down;
        }

        private void VerticalAD_SelectionChanged_Down(object sender, EventArgs e)
        {
            Draw_Down();
        }

        protected void ResetDown()
        {
            if (!HasDown)
                return;

            ZoomScrollView.InitialDownHeight = Height_Down;

            ZoomScrollView.SetDownContent((FrameworkElement)DownDecorator, Down);
        }

        private void Down_Loaded(object sender, RoutedEventArgs e)
        {
            Height_Down = Down.ActualHeight;

            Draw_Down();
        }

        private void Down_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Height_Down != Down.ActualHeight)
            {
                Height_Down = Down.ActualHeight;

                Draw_Down();
            }
        }

        protected void DrawLines_Down()
        {
            foreach (AxisSegment s in DownDecorator.Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

                Down.Children.Add(l);
            }

            foreach (AxisSegment s in HorizontalAD.Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, s.StartPosition, 0, s.StartPosition, Height_Down);

                s.LineStyle.SetStyle(l);

                Down.Children.Add(l);
            }
        }

        protected void DrawDownBackground()
        {
            Border Background = new Border();

            Background.Background = (Brush)FindResource("0LightBackgroundBrush");

            WpfUtil.SetPosition(Background, 0, 0, Main.Width, Height_Down);

            Down.Children.Add(Background);
        }

        protected void Draw_Down()
        {
            if (!HasDown || DownDecorator == null || Height_Down == 0)
                return;            
            
            Down.Children.Clear();

            SelectionArea_Down = new SelectionArea(Down);

            Items_Down = new List<FrameworkElement>();

            DrawDownBackground();

            DrawSnapLines_Down();

            DrawLines_Down();

            DrawArrowLines_Down();

            DrawItems_Down();
        }

        protected void DownDecorator_SelectionChanged(object sender, EventArgs e)
        {
            SetCurrentControlChangeNumber((int)DownDecorator.Selection);

            Draw_Down();
        }

        protected void MouseEnterHandler_Down(object sender, MouseEventArgs e)
        {
            UpdateCursorShape();

            WhereIsMouse = MainDownEnum.Down;
        }

        protected void MouseLeaveHandler_Down(object sender, MouseEventArgs e)
        {
            HideArrowLines();

            WpfUtil.SetCursor(Cursors.Arrow);

            switch (CurrentCursorState)
            {
                case (CursorStateEnum.PenDown):
                    WpfUtil.SetCursor(Cursors.Arrow);
                    break;

                case (CursorStateEnum.ArrowDown):
                case (CursorStateEnum.ArrowDown_MoveOnItem_MouseDown):
                case (CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove):
                    PerformArrowUp_FromArrowDown_WhileMouseLeave_Down();
                    break;
            }

            WhereIsMouse = MainDownEnum.Undefined;
        }

        protected void MouseDownHandler_Down(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentCursorState)
            {
                case CursorStateEnum.PenUp:
                    PenDown_Down(sender, e);
                    break;

                case CursorStateEnum.Eraser:
                    EraserDown_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowUp:
                    ArrowDown_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowUp_MoveOnItem:
                    ArrowDown_FromUpMove_Down(sender, e);
                    break;
            }
        }

        protected void MouseUpHandler_Down(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentCursorState)
            {
                case CursorStateEnum.PenDown:
                    SetCursorMode(CursorStateEnum.PenUp);
                    break;

                case CursorStateEnum.ArrowDown:
                    ArrowUp_FromDown_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowUp_FromMove_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:
                    ArrowUp_FromMoveOnItem_MouseDown_Down(sender, e);
                    break;

                default:
                    break;
            }
        }

        protected void MouseMoveHandler_Down(object sender, MouseEventArgs e)
        {
            UpdateArrowLines(e);

            switch (CurrentCursorState)
            {
                case CursorStateEnum.PenDown:
                    PenMove_PenDown_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowUp:
                case CursorStateEnum.ArrowUp_MoveOnItem:
                    ArrowMove_ArrowUp_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowDown:
                    ArrowMove_ArrowDown_Down(sender, e);
                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowMove_DownMoveOnItemLeftRight_Down(sender, e);
                    break;

                default:
                    break;
            }
        }

        protected void PenDown_Down(object sender, MouseButtonEventArgs e)
        {
            MouseDownPoint = GetDownContentMousePosition(e);

            PenDown_Down_internal(sender, MouseDownPoint);
        }

        protected void PenDown_Down_internal(object sender, Point mouseDownPoint)
        {
            SetCursorMode(CursorStateEnum.PenDown);

            PreviousMousePosition = mouseDownPoint;

            double mouseY = mouseDownPoint.Y;

            VertexChangeOff = true;

            bool isUpdate = false;

            bool isNoteEvent = false;

            IEdge newItemEventEdge = AddItemEdge_Down(mouseY, GetSnappedPosition(mouseDownPoint.X), out isUpdate, out isNoteEvent);

            if (isNoteEvent)
                GetItemsDictionary()[newItemEventEdge.To].Update();

            if (newItemEventEdge != null)
                AddItem_Down(newItemEventEdge, null, isUpdate, isNoteEvent);

            VertexChangeOff = false;
        }

        protected List<IItem> GetDownItemFromNumberTriggerTimeDictionary(int number, int triggerTime)
        {
            Dictionary<int, Dictionary<int, List<IItem>>> itemsDictinary_Number_TriggerTime_Down = GetItemsDictionary_Number_TriggerTime_Down();

            if (itemsDictinary_Number_TriggerTime_Down.ContainsKey(number))
            {
                Dictionary<int, List<IItem>> itemsDictinary_TriggerTime_Down = itemsDictinary_Number_TriggerTime_Down[number];

                if (itemsDictinary_TriggerTime_Down.ContainsKey(triggerTime))
                    return itemsDictinary_TriggerTime_Down[triggerTime];
            }

            return null;
        }

        protected IEdge AddItemEdge_Down(double mouseY, double startPosition, out bool isUpdate, out bool isNoteEvent)
        {
            isUpdate = false;

            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex ControlChangeEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");
            IVertex NoteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            int triggerTime = (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01);

            IEdge tempEventEdge = null;

            isNoteEvent = false;

            List<IItem> existingItems = GetDownItemFromNumberTriggerTimeDictionary(CurrentControlChangeNumber, triggerTime);

            if (existingItems == null && MainItemsSyncedWithDown)
                return null;

            if (existingItems != null)
            {
                IItem item = existingItems[0];

                tempEventEdge = item.BaseEdge;

                isUpdate = true;

                if (tempEventEdge.To.Get(false, @"$Is:NoteEvent") != null)
                    isNoteEvent = true;
            }
            else
                tempEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex eventVertex = tempEventEdge.To;

            if (!isUpdate)
                eventVertex.AddEdge(MinusZero.Instance.Is, ControlChangeEvent);

            if (isNoteEvent)
                GraphUtil.SetVertexValue(eventVertex, NoteEvent.Get(false, @"Attribute:Velocity"), ControlChangeItem.getValueFromMouseY_Down(mouseY, Height_Down));
            else
            {
                GraphUtil.SetVertexValue(eventVertex, ControlChangeEvent.Get(false, @"Attribute:Number"), CurrentControlChangeNumber);
                GraphUtil.SetVertexValue(eventVertex, ControlChangeEvent.Get(false, @"Attribute:Value"), ControlChangeItem.getValueFromMouseY_Down(mouseY, Height_Down));
                GraphUtil.SetVertexValue(eventVertex, ControlChangeEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            }

            IEdge finalEdge = tempEventEdge;

            if (!isUpdate)
            {
                baseVertex.AddEdge(Event, eventVertex);
                baseVertex.DeleteEdge(tempEventEdge);
            }

            return finalEdge;
        }

        protected void AddItem_Down(IEdge itemEdge, List<IVertex> selectedVertexes, bool isUpdate, bool isNoteEvent)
        {
            if (Height_Down == 0)
                return;

            IVertex itemEventVertex = itemEdge.To;

            bool dummy = false;


            ControlChangeItem item = null;

            if (isUpdate)
                item = (ControlChangeItem)GetItemsDictionary_Down()[itemEdge.To];
            else
                item = new ControlChangeItem(itemEdge, this);

            IVertex itemVertex = item.BaseEdge.To;

            int triggerTime = GraphUtil.GetIntegerValue(itemVertex.Get(false, "TriggerTime:"), ref dummy);

            int value;

            if (isNoteEvent)
                value = GraphUtil.GetIntegerValue(itemVertex.Get(false, "Velocity:"), ref dummy);
            else
                value = GraphUtil.GetIntegerValue(itemVertex.Get(false, "Value:"), ref dummy);

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                item.Select();
                PreviousSelectedItemContext = MainDownEnum.Down;
            }

            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            if (!isUpdate)
                ItemsAdd_Down(item); // need this as item.Canvas needs to be set for the cc top mark

            item.HorizontalCenter = startPosition;
            item.VerticalCenter = Height_Down - (((double)value / 127) * Height_Down);
        }

        protected void ItemsAdd_Down(IItem i)
        {
            NeedToRebuildItemsDictionary_Down = true;
            Items_Down.Add((FrameworkElement)i);

            i.Add(Down);
        }

        protected void ItemsRemoveAndRemoveAllEdges_Down(IItem i)
        {
            IEdge eventEdge = i.BaseEdge;

            GraphUtil.DeleteEdgeByToVertex(baseVertex, eventEdge.To);

            Edge.DeleteVertexByEdgeTo(Vertex.Get(false, "SelectedEdges:"), eventEdge.To);

            NeedToRebuildItemsDictionary_Down = true;

            Items_Down.Remove((FrameworkElement)i);

            i.Remove();
        }

        protected Dictionary<IVertex, IItem> GetItemsDictionary_Down()
        {
            if (NeedToRebuildItemsDictionary_Down)
                RebuildItemsDictionary_Down();

            return ItemsDictinaryHolder_Down;
        }

        protected Dictionary<int, Dictionary<int, List<IItem>>> GetItemsDictionary_Number_TriggerTime_Down()
        {
            if (NeedToRebuildItemsDictionary_Down)
                RebuildItemsDictionary_Down();

            return ItemsDictinaryHolder_Number_TriggerTime_Down;
        }

        protected void RebuildItemsDictionary_Down()
        {
            ItemsDictinaryHolder_Down.Clear();

            ItemsDictinaryHolder_Number_TriggerTime_Down.Clear();

            foreach (IItem i in Items_Down)
            {
                IVertex v = i.BaseEdge.To;
                ItemsDictinaryHolder_Down.Add(v, i);

                //

                int triggerTime = (int)GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"));
                int number;

                if (MainItemsSyncedWithDown)
                    number = -1;
                else
                    number = (int)GraphUtil.GetIntegerValue(v.Get(false, "Number:"));

                Dictionary<int, List<IItem>> itemsDictinaryHolder_TriggerTime_Down;

                if (ItemsDictinaryHolder_Number_TriggerTime_Down.ContainsKey(number))
                    itemsDictinaryHolder_TriggerTime_Down = ItemsDictinaryHolder_Number_TriggerTime_Down[number];
                else
                {
                    itemsDictinaryHolder_TriggerTime_Down = new Dictionary<int, List<IItem>>();
                    ItemsDictinaryHolder_Number_TriggerTime_Down.Add(number, itemsDictinaryHolder_TriggerTime_Down);
                }

                if (itemsDictinaryHolder_TriggerTime_Down.ContainsKey(triggerTime))
                    itemsDictinaryHolder_TriggerTime_Down[triggerTime].Add(i);
                else
                {
                    List<IItem> list = new List<IItem>();
                    list.Add(i);
                    itemsDictinaryHolder_TriggerTime_Down.Add(triggerTime, list);
                }
            }

            NeedToRebuildItemsDictionary_Down = false;
        }

        protected void DrawItems_Down()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            if (CurrentControlChangeNumber == -1)
            {
                foreach (IEdge e in baseVertex.GetAll(false, "Event:"))
                    if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                        if(ApplyFilter_Down(e.To))
                            AddItem_Down(e, selectedVertexes, false, true);
            }
            else
            {
                foreach (IEdge e in baseVertex.GetAll(false, "Event:"))
                    if (GraphUtil.ExistQueryOut(e.To, "$Is", "ControlChangeEvent")
                        && GraphUtil.GetIntegerValue(e.To.Get(false, @"Number:")) == CurrentControlChangeNumber)
                        AddItem_Down(e, selectedVertexes, false, false);
            }
        }

        private void PenMove_PenDown_Down(object sender, MouseEventArgs e)
        {
            MouseDownPoint = GetDownContentMousePosition(e);

            PenDown_Down_internal(sender, MouseDownPoint);
        }

        protected void EraserDown_Down(object sender, MouseButtonEventArgs e)
        {
            if (MainItemsSyncedWithDown)
                return;

            Point currentMousePosition = GetDownContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items_Down, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                IEdge eventEdge = item.BaseEdge;

                VertexChangeOff = true;

                ItemsRemoveAndRemoveAllEdges_Down(item);

                VertexChangeOff = false;
            }
        }

        protected void ArrowDown_Down(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetDownContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items_Down, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                    UnselectItem(item);
                else
                    SelectItem(item);
            }
            else
            {
                SetCursorMode(CursorStateEnum.ArrowDown);

                UnselectAllSelectedItems();

                PreviousSelectedItemContext = MainDownEnum.Down;

                SelectionArea_Down.StartSelection(currentMousePosition);
            }
        }

        protected void ArrowMove_ArrowDown_Down(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetDownContentMousePosition(e);

            SelectionArea_Down.MoveSelectionArea(currentMousePosition);

            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(Items_Down,
                SelectionArea_Down.Left - 2,
                SelectionArea_Down.Top - 2,
                SelectionArea_Down.Right + 2,
                SelectionArea_Down.Bottom + 2);

            foreach (FrameworkElement _e in Items_Down)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        item.Select();
                    else
                        item.Unselect();
                }

            WpfUtil.SetCursor(Cursors.Arrow);
        }

        protected void SaveSelectionArea_Down()
        {
            IList<FrameworkElement> matched = WpfUtil.GetElementsAtFromListByArea(Items_Down,
                SelectionArea_Down.Left - 2,
                SelectionArea_Down.Top - 2,
                SelectionArea_Down.Right + 2,
                SelectionArea_Down.Bottom + 2);

            UnselectAllSelectedEdges();

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            foreach (FrameworkElement _e in Items_Down)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        SelectItem(item);
                    else
                        item.Unselect();
                }

            CurrentCursorState = CursorStateEnum.ArrowUp;
            SelectionArea_Down.HideSelectionArea();
        }

        protected void ArrowUp_FromDown_Down(object sender, MouseButtonEventArgs e)
        {
            SaveSelectionArea_Down();
        }

        protected void PerformArrowUp_FromArrowDown_WhileMouseLeave_Down()
        {
            SaveSelectionArea_Down();
        }

        protected void ArrowDown_FromUpMove_Down(object sender, MouseButtonEventArgs e)
        {
            if (MouseOverItem == null)
                return;

            InitMouseOverElementAndSelected();

            MouseDownPoint = GetDownContentMousePosition(e);

            PreviousMousePosition = MouseDownPoint;

            if (CurrentCursorState == CursorStateEnum.ArrowUp_MoveOnItem)
                SetCursorMode(CursorStateEnum.ArrowDown_MoveOnItem_MouseDown);
        }

        protected void ArrowUp_FromMove_Down(object sender, MouseEventArgs e)
        {
            if (CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove)
            {
                SetCursorMode(CursorStateEnum.ArrowUp);

                foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Down))
                    UpdateItem_HorizontalPosition(i);
            }

            RemoveDuplicatedDownItems_SelectedEdgesFirst();
        }

        protected void ArrowUp_FromMoveOnItem_MouseDown_Down(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetDownContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items_Down, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (item.IsSelected)
                    UnselectItem(item);
                else
                    SelectItem(item);
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
        }

        protected void RemoveDuplicatedDownItems_SelectedEdgesFirst()
        {
            IVertex selectedEdges = Vertex.GetAll(false, @"SelectedEdges:\");

            VertexChangeOff = true;

            foreach (IEdge e in selectedEdges)
            {
                IVertex v = e.To.Get(false, @"To:");

                if (v.Get(false, @"$Is:ControlChangeEvent") != null)
                {
                    int triggerTime = (int)GraphUtil.GetIntegerValue(v.Get(false, @"TriggerTime:"));
                    int number = (int)GraphUtil.GetIntegerValue(v.Get(false, @"Number:"));

                    List<IItem> existingItems = GetDownItemFromNumberTriggerTimeDictionary(number, triggerTime);

                    if (existingItems != null)
                        foreach (IItem i in existingItems)
                            if (i.BaseEdge.To != v)
                                ItemsRemoveAndRemoveAllEdges_Down(i);
                }
            }

            VertexChangeOff = false;
        }

        protected void ArrowMove_ArrowUp_Down(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetDownContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items_Down, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorStateEnum.ArrowUp_MoveOnItem);
                return;
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
            UpdateCursorShape();
        }

        protected void ArrowMove_DownMoveOnItemLeftRight_Down(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetDownContentMousePosition(e);

            double deltaX = currentMousePosition.X - PreviousMousePosition.X;
            double deltaY = currentMousePosition.Y - PreviousMousePosition.Y;

            switch (CurrentCursorState)
            {
                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDown:

                    double horizontalDelta = Math.Abs(MouseDownPoint.X - currentMousePosition.X);
                    double verticalDelta = Math.Abs(MouseDownPoint.Y - currentMousePosition.Y);

                    double delta = Math.Sqrt(horizontalDelta * horizontalDelta + verticalDelta * verticalDelta);

                    if (delta > ArrowDown_MoveOnItem_MouseDown_Delta)
                    {
                        SetCursorMode(CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove);
                        UpdateCursorShape();
                    }

                    break;

                case CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove:

                    foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Down))
                        if (AllowHorizontalItemMove_Down)
                            ItemTryMove(i, deltaX, deltaY);
                        else
                            ItemTryMove(i, 0, deltaY);

                    break;
            }

            PreviousMousePosition = currentMousePosition;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /////////////// DOWN END ///////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////        

        protected void TurnOnSelectedEdgesFireChange()
        {
            if (Vertex.Get(false, "SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get(false, "SelectedEdges:")).CanFireChangeEvent = true;
        }

        protected void TurnOffSelectedEdgesFireChange()
        {
            if (Vertex.Get(false, "SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get(false, "SelectedEdges:")).CanFireChangeEvent = false;
        }

        protected void UnselectAllSelectedItems()
        {
            foreach (IItem i in Items)
                i.Unselect();

            foreach (IItem i in Items_Down)
                i.Unselect();

            UnselectAllSelectedEdges();
        }

        protected void UnselectAllSelectedEdges()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            TurnOffSelectedEdgesFireChange();

            GraphUtil.RemoveAllEdges(sv);

            TurnOnSelectedEdgesFireChange();
        }

        protected void DrawSnapLines()
        {
            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap || ShowSnapLines == false)
                return;

            double snapWidth = GetSnapMinmalWidth();

            Brush lb = (Brush)FindResource("0VeryLightForegroundBrush");

            for (double x = 0; x < Width; x += snapWidth)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, x, 0, x, Height);

                l.Stroke = lb;

                l.StrokeThickness = 1;

                Main.Children.Add(l);
            }
        }

        protected void DrawSnapLines_Down()
        {
            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap || ShowSnapLines == false)
                return;

            double snapWidth = GetSnapMinmalWidth();

            Brush lb = (Brush)FindResource("0VeryLightForegroundBrush");

            for (double x = 0; x < Width; x += snapWidth)
            {
                Line ld = new Line();

                WpfUtil.SetLinePosition(ld, x, 0, x, Height_Down);

                ld.Stroke = lb;

                ld.StrokeThickness = 1;

                Down.Children.Add(ld);
            }
        }

        protected void DrawMainSegments()
        {
            foreach (AxisSegment s in VerticalAD.Segments)
            {
                if (s.UseBackgroundColor)
                {
                    Border b = new Border();

                    b.Background = new SolidColorBrush(s.BackgroundColor);

                    WpfUtil.SetPosition(b, 0, s.StartPosition, Width, s.EndPosition - s.StartPosition);

                    Main.Children.Add(b);
                }
            }
        }

        protected void DrawMainLines()
        {
            foreach (AxisSegment s in VerticalAD.Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

                Main.Children.Add(l);

            }

            foreach (AxisSegment s in HorizontalAD.Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, s.StartPosition, 0, s.StartPosition, Height);

                s.LineStyle.SetStyle(l);

                Main.Children.Add(l);
            }
        }

        protected void DrawArrowLines()
        {
            Main.Children.Add(HorizontalArrowLine);
            Main.Children.Add(VerticalArrowLine);
        }

        protected void DrawArrowLines_Down()
        {
            Down.Children.Add(HorizontalArrowLine_Down);
            Down.Children.Add(VerticalArrowLine_Down);
        }

        protected void DrawItems()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            foreach (IEdge e in baseVertex.GetAll(false, "Event:"))
                if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                    AddItem(e, selectedVertexes);
        }

        protected void InitSequenceVisualierState()
        {
            SetCursorMode(CursorStateEnum.ArrowUp);

            CurrentSnapToGrid = SnapToGridEnum.Bar1;

            CurrentSnapToGridValue = 1;
        }

        protected string VisualiserName = "NAME";        

        public void ZoomScrollViewBasedVisualiserBase_Init()
        {                        
            MinusZero mz = MinusZero.Instance;
            
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
                Vertex.Value = VisualiserName + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, VisualiserMetaVertex);

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                UpdateVertexValues();

                /*this.ContextMenu = new m0ContextMenu(this);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;
                this.Drop += dndDrop;

                this.MouseEnter += dndMouseEnter;*/

                ZoomScrollView.SetHost(this);

                InitSequenceVisualierState();

                if (HasDown == false)
                    ZoomScrollView.DownAreaVisible = false;
            }
        }

        protected void UpdateBaseEdge()
        {
            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (bas != null)
            {
                UpdateVertexValues();

                UpdateVariablesFromBaseVertex();

                // SnapToGridComboBox_SelectionChange(); in UpdateVertexValues() does this;
                // VisualiserDraw(); 
            }
        }

        protected virtual void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (VertexChangeOff)
                return;

            if ((sender == Vertex.Get(false, "ShowArrowLines:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            if ((sender == Vertex.Get(false, "ShowSnapLines:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            if ((sender == Vertex.Get(false, "ShowLabel:")) && (e.Type == VertexChangeType.ValueChanged))
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

        protected IVertex _Vertex;

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

        protected bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                DispachAllSubVisualisers();

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        protected void DispachAllSubVisualisers()
        {
            if (ZoomScrollView is IDisposable)
                ((IDisposable)ZoomScrollView).Dispose();
        }

        protected bool isLoaded = false;
        public void ChildControlsLoaded()
        {
            isLoaded = true;

            VisualiserDraw();
        }

        protected void SnapToGridComboBox_SelectionChange()
        {
            switch (Vertex.Get(false, "SnapToGrid:").Value.ToString())
            {
                case "1 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1;
                    CurrentSnapToGridValue = 1;
                    break;

                case "1/2 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_2;
                    CurrentSnapToGridValue = 1.0 / 2;
                    break;

                case "1/4 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_4;
                    CurrentSnapToGridValue = 1.0 / 4;
                    break;

                case "1/8 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_8;
                    CurrentSnapToGridValue = 1.0 / 8;
                    break;

                case "1/16 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_16;
                    CurrentSnapToGridValue = 1.0 / 16;
                    break;

                case "1/32 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_32;
                    CurrentSnapToGridValue = 1.0 / 32;
                    break;

                case "no snap":
                    CurrentSnapToGrid = SnapToGridEnum.No_Snap;
                    CurrentSnapToGridValue = 0;
                    break;
            }

            VisualiserDraw();
        }        

        protected void PenButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorStateEnum.PenUp);
        }

        protected void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorStateEnum.Eraser);
        }

        protected void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorStateEnum.ArrowUp);
        }

        protected void SetChecked(ToggleButton button, bool state)
        {
            if (button != null)
                button.IsChecked = state;
        }

        protected void UnCheckAllCursorButtons()
        {
            SetChecked(EraseButton, false);
            SetChecked(PenButton, false);
            SetChecked(ArrowButton, false);
            SetChecked(GlueButton, false);
            SetChecked(ScissorsButton, false);
        }

        protected void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Dictionary<IVertex, IItem> itemsDictionary = GetItemsDictionary();

                Dictionary<IVertex, IItem> itemsDictionary_Down = GetItemsDictionary_Down();

                foreach (IVertex v in GetSelectedVertexes())
                {
                    IItem i = null;

                    if (itemsDictionary.ContainsKey(v))
                    {
                        i = itemsDictionary[v];

                        ItemsRemoveAndRemoveAllEdges(i);
                    }

                    if (itemsDictionary_Down.ContainsKey(v))
                    {
                        i = itemsDictionary_Down[v];

                        ItemsRemoveAndRemoveAllEdges_Down(i);
                    }
                }

                UnselectAllSelectedEdges();
            }

        }
        protected virtual void TruncateButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Length - ExtendTimeLength) <= 0)
                return;

            Length -= ExtendTimeLength;

            SaveLength();

            HorizontalAD.SetLength(Length);

            VisualiserDraw();
        }

        protected virtual void ExtendButton_Click(object sender, RoutedEventArgs e)
        {
            Length += ExtendTimeLength;

            SaveLength();

            HorizontalAD.SetLength(Length);

            VisualiserDraw();
        }
    }
}
