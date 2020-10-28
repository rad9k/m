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
    public partial class SequenceVisualiser : UserControl, IPlatformClass, IOwnScrolling, IZoomScrollViewerHost, IDisposable
    {
        // sequencer specyfic

        protected bool showVelocity;
        bool showArowLines;
        bool showSnapLines;
        int defaultVelocity;
        bool isDrum;
        int CCNumber;

        //

        protected bool isCurrentPenItemCenter;

        protected bool showLabel;

        protected Canvas Main;
        protected SelectionArea SelectionArea;

        protected IVertex BaseEdgeToVertex;

        protected IVertex baseVertex;
        protected IVertex pitchSetVertex;
        protected IVertex timeSpanVertex;

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

        protected enum CursorState {
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

        protected CursorState currentCursorState;

        protected Point mouseDownPoint;

        protected Point previousMousePosition;

        protected Border newItemShape;

        protected AxisSegment newItemSegment;

        protected enum SnapToGridEnum { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32, No_Snap }        

        protected SnapToGridEnum currentSnapToGrid;

        protected double currentSnapToGridValue;

        protected List<FrameworkElement> items;

        protected Dictionary<IVertex, IItem> itemsDictinaryHolder = new Dictionary<IVertex, IItem>();

        protected bool needToRebuildItemsDictionary = true;

        protected bool VertexChangeOff = false;

        protected FrameworkElement mouseOverItem_Element;

        protected IItem mouseOverItem;

        protected double mouseOverItem_startLeft;

        protected Line HorizontalArrowLine;
        protected Line VerticalArrowLine;

        protected Line HorizontalArrowLine_Down;
        protected Line VerticalArrowLine_Down;

        // DOWN

        protected IZoomScrollViewDownDecorator DownDecorator;

        protected Canvas Down;

        protected double Height_Down;

        protected enum WhereIsMouseEnum { MouseOnMain, MouseOnDown, MouseOutside }

        protected WhereIsMouseEnum WhereIsMouse;


        protected Dictionary<IVertex, IItem> GetItemsDictionary()
        {
            if (needToRebuildItemsDictionary)
                RebuildItemsDictionary();

            return itemsDictinaryHolder;
        }

        protected void RebuildItemsDictionary()
        {
            itemsDictinaryHolder.Clear();

            foreach (IItem i in items)
                itemsDictinaryHolder.Add(i.BaseEdge.To, i);

            needToRebuildItemsDictionary = false;
        }

        protected void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;
            //Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;            

            bool dummy = false;

            showLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);
            showVelocity = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowVelocity:"), ref dummy);
            showArowLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowArrowLines:"), ref dummy);
            showSnapLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowSnapLines:"), ref dummy);
            defaultVelocity = GraphUtil.GetIntegerValue(Vertex.Get(false, "DefaultVelocity:"), ref dummy);

            if (Vertex.Get(false, "SnapToGrid:") == null || Vertex.Get(false, "SnapToGrid:").Value.ToString() == "")
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Sequence\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1 bar'"));

            SnapToGridComboBox_SelectionChange();
        }

        protected void SetMouseOverItem(IItem item)
        {
            if (!(item is FrameworkElement))
                return;

            mouseOverItem = item;
            mouseOverItem_Element = (FrameworkElement)item;
            mouseOverItem_startLeft = item.Left;
        }

        protected void SetCursorMode(CursorState modeDetail)
        {
            UnCheckAllCursorButtons();            

            currentCursorState = modeDetail;

            switch (modeDetail)
            {
                case CursorState.ArrowDown:
                case CursorState.ArrowUp:
                case CursorState.ArrowUp_MoveOnItem_Left:
                case CursorState.ArrowUp_MoveOnItem_Right:
                case CursorState.ArrowUp_MoveOnItem:
                case CursorState.ArrowDown_MoveOnItem_Left:
                case CursorState.ArrowDown_MoveOnItem_Right:
                case CursorState.ArrowDown_MoveOnItem_MouseDown:
                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:

                    ArrowButton.IsChecked = true;

                    HideArrowLines();

                    break;

                case CursorState.Eraser:

                    EraseButton.IsChecked = true;
                    break;

                case CursorState.PenUp:                
                case CursorState.PenDown:                

                    PenButton.IsChecked = true;
                    break;
            }
        }

        protected void UpdateCursorShape()
        {
            switch (currentCursorState)
            {
                case CursorState.ArrowDown:
                case CursorState.ArrowUp:
                case CursorState.ArrowUp_MoveOnItem:
                case CursorState.ArrowDown_MoveOnItem_MouseDown:
                    WpfUtil.SetCursor(Cursors.Arrow);
                    break;

                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:
                    WpfUtil.SetCursor(Cursors.SizeAll);
                    break;

                case CursorState.ArrowUp_MoveOnItem_Left:
                case CursorState.ArrowUp_MoveOnItem_Right:
                case CursorState.ArrowDown_MoveOnItem_Left:
                case CursorState.ArrowDown_MoveOnItem_Right:
                    WpfUtil.SetCursor(Cursors.SizeWE);
                    break;

                case CursorState.Eraser:
                    WpfUtil.SetCursorFromResource("/m0;component/_resources/basic/eraser.cur");
                    break;

                case CursorState.PenDown:                
                case CursorState.PenUp:
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

                SetVertexVaribles();

                SetAxisDecorators();

                CreateMain();            

                CreateArrowLines();

                SetupScrollViewer();

                CreateDown();
            }

            DrawMain();

            DrawDown();

            InitialiseItems();

            DrawItems();

            VisuliseserDrraw_NeedsInitilisation = false;
        }

        protected void SetVertexVaribles()
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
                if(isDrum)
                    pitchSetVertex = r.Get(false, @"System\Lib\Music\Data\DefaultDrumPitchSet:");
                else
                    pitchSetVertex = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");

            timeSpanVertex = baseVertex.Get(false, "TimeSpan:");

            if (timeSpanVertex == null)
                timeSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultTimeSpanLevel:");
        }

        protected void SetupLocalVariablesFromBaseVertexVertexes()
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

        protected void SaveLength()
        {
            GraphUtil.SetVertexValue(baseVertex, BaseEdgeToVertex.Get(false, "Length"), Length);
        }

        protected void SetAxisDecorators()
        {
            VerticalAD = new PitchSetAxisDecorator();

            VerticalAD.SetBaseVertex(pitchSetVertex);


            TimeSpanAxisDecorator TimeSpanAD = new TimeSpanAxisDecorator();
            TimeSpanAD.BoldLineCount = 4;

            HorizontalAD = TimeSpanAD;

            HorizontalAD.SetBaseVertex(timeSpanVertex);

            HorizontalAD.SetLength(Length);


            ZoomScrollView.SetVerticalAxisDecorator(VerticalAD);

            ZoomScrollView.SetHorizontalAxisDecorator(HorizontalAD);
        }

        protected void CreateMain()
        {
            Main = new Canvas();

            Width = HorizontalAD.Size.Width;
            Height = VerticalAD.Size.Height;

            Main.Width = Width;
            Main.Height = Height;

            ZoomScrollView.SetMainContent(Main);                        
        }

        protected void InitialiseItems()
        {
            items = new List<FrameworkElement>();
        }

        protected void CreateArrowLines()
        {
            HorizontalArrowLine = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
            VerticalArrowLine = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));

            HorizontalArrowLine_Down = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
            VerticalArrowLine_Down = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
        }

        protected void CreateDown()
        {
            ZoomScrollView.InitialDownHeight = 100;

            DownDecorator = new ControlChangeDownDecorator();

            CCNumber = (int)DownDecorator.Selection;

            DownDecorator.SelectionChanged += DownDecorator_SelectionChanged;

            Down = new Canvas();

            Down.SizeChanged += Down_SizeChanged;
            Down.Loaded += Down_Loaded;

            ZoomScrollView.SetDownContent((FrameworkElement)DownDecorator, Down);            
        }

        private void Down_Loaded(object sender, RoutedEventArgs e)
        {
            Height_Down = Down.ActualHeight;

            DrawDown();
        }

        private void Down_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height_Down = Down.ActualHeight;

            DrawDown();
        }

        protected void DrawDownLines()
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

        protected void DrawDown()
        {             
            if (DownDecorator == null)
                return;

            Down.Children.Clear();

            DrawDownBackground();

            DrawDownLines();

            DrawArrowLines_Down();        
        }

        protected void DownDecorator_SelectionChanged(object sender, EventArgs e)
        {
            CCNumber = (int)DownDecorator.Selection;

            DrawDown();
        }

        protected void ItemsAdd(IItem i)
        {
            needToRebuildItemsDictionary = true;
            items.Add((FrameworkElement)i);

            Main.Children.Add((FrameworkElement)i);
        }

        protected void ItemsRemove(IItem i)
        {
            needToRebuildItemsDictionary = true;
            items.Remove((FrameworkElement)i);

            Main.Children.Remove((FrameworkElement)i);
        }

        protected void SetupScrollViewer()
        {
            ZoomScrollView.SetHost(this);
            ZoomScrollView.SetMainContent(Main);
        }

        protected void DrawMain()
        {
            Main.Children.Clear();

            DrawBackground();

            DrawMainSegments();

            DrawMainSnapLines();

            DrawMainLines();

            DrawArrowLines();

            AddEventHandlers();            


            SelectionArea = new SelectionArea(Main);
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

            //

            Down.MouseEnter += MouseEnterHandler_Down;

            Down.MouseLeave += MouseLeaveHandler_Down;

            Down.MouseDown += MouseDownHandler_Down;

            Down.MouseUp += MouseUpHandler_Down;

            Down.MouseMove += MouseMoveHandler_Down;
        }

        protected void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            UpdateArrowLines(e);

            switch (currentCursorState)
            {
                case CursorState.PenDown:
                    PenMove_PenDown(sender, e);
                    break;

                case CursorState.ArrowUp:
                case CursorState.ArrowUp_MoveOnItem_Left:
                case CursorState.ArrowUp_MoveOnItem_Right:
                case CursorState.ArrowUp_MoveOnItem:
                    ArrowMove_ArrowUp(sender, e);
                    break;

                case CursorState.ArrowDown:
                    ArrowMove_ArrowDown(sender, e);
                    break;

                case CursorState.ArrowDown_MoveOnItem_Left:
                case CursorState.ArrowDown_MoveOnItem_Right:
                case CursorState.ArrowDown_MoveOnItem_MouseDown:
                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowMove_DownMoveOnItemLeftRight(sender, e);
                    break;

                default:
                    break;
            }
        }        

        protected void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorState)
            {
                case CursorState.PenDown:
                    PenUp(sender, e);
                    break;                

                case CursorState.ArrowDown:
                    ArrowUp_FromDown(sender, e);
                    break;

                case CursorState.ArrowDown_MoveOnItem_Left:
                case CursorState.ArrowDown_MoveOnItem_Right:
                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowUp_FromMove(sender, e);
                    break;

                case CursorState.ArrowDown_MoveOnItem_MouseDown:
                    ArrowUp_FromMoveOnItem_MouseDown(sender, e);
                    break;

                default:
                    break;
            }
        }

        protected void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorState)
            {
                case CursorState.PenUp:
                    PenDown(sender, e);
                    break;

                case CursorState.Eraser:
                    EraserDown(sender, e);
                    break;

                case CursorState.ArrowUp:
                    ArrowDown(sender, e);
                    break;

                case CursorState.ArrowUp_MoveOnItem_Left:
                case CursorState.ArrowUp_MoveOnItem_Right:
                case CursorState.ArrowUp_MoveOnItem:
                    ArrowDown_FromUpMove(sender, e);
                    break;
            }
        }

        protected void UpdateArrowLines(MouseEventArgs e)
        {
            if (!showArowLines || 
                !(currentCursorState == CursorState.Eraser || 
                currentCursorState == CursorState.PenDown || 
                currentCursorState == CursorState.PenUp))
            {
                HideArrowLines();

                return;
            }

            double x, y;

            switch (WhereIsMouse)
            {
                case WhereIsMouseEnum.MouseOnMain:

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

                case WhereIsMouseEnum.MouseOnDown:

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
            SetCursorMode(CursorState.PenDown);
            
            mouseDownPoint = GetMainContentMousePosition(e);

            previousMousePosition = mouseDownPoint;

            newItemSegment = FindVerticalSegment(mouseDownPoint.Y);

            //

            if (isCurrentPenItemCenter)
                return;

            newItemShape = new Border();

            newItemShape.Background = (Brush)FindResource("0HighlightBrush");

            newItemShape.BorderThickness = new Thickness(0);

            double snappedMouseX = GetSnappedPosition(mouseDownPoint.X);

            WpfUtil.SetPositionAbsolute(newItemShape, snappedMouseX, newItemSegment.StartPosition, snappedMouseX, newItemSegment.EndPosition);

            Main.Children.Add(newItemShape);
        }

        protected void PenMove_PenDown(object sender, MouseEventArgs e)
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

            WpfUtil.SetPositionAbsolute(newItemShape, left, newItemSegment.StartPosition, right, newItemSegment.EndPosition);
        }

        protected void PerformPenUp()
        {
            Main.Children.Remove(newItemShape);

            SetCursorMode(CursorState.PenUp);            
        }

        protected void PenUp(object sender, MouseButtonEventArgs e)
        {
            PerformPenUp();

            VertexChangeOff = true;

            IEdge newItemEventEdge;

            if (isCurrentPenItemCenter)
                newItemEventEdge = AddItemEdge(newItemSegment, GetSnappedPosition(mouseDownPoint.X), 0);
            else
            {
                if(newItemShape.Width == 0)
                {
                    VertexChangeOff = false;
                    return;
                }

                newItemEventEdge = AddItemEdge(newItemSegment, Canvas.GetLeft(newItemShape), newItemShape.Width);
            }

            AddItem(newItemEventEdge, null);

            VertexChangeOff = false;
        }

        protected void EraserDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                IEdge eventEdge = item.BaseEdge;

                VertexChangeOff = true;

                GraphUtil.DeleteEdgeByToVertex(baseVertex, eventEdge.To);

                ItemsRemove(item);                

                VertexChangeOff = false;
            }
        }

        protected void ArrowDown(object sender, MouseButtonEventArgs e)
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
                SetCursorMode(CursorState.ArrowDown);

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

            foreach (IItem i in items)
                if (selectedVertexes.Contains(i.BaseEdge.To))
                    selectedItems.Add(i);

            return selectedItems;
        }

        protected List<IItem> GetSelectedAndMouseOverItems()
        {
            List<IItem> selectedItems = GetSelectedItems();

            if (!selectedItems.Contains(mouseOverItem))
                selectedItems.Add(mouseOverItem);

            return selectedItems;
        }

        protected void InitMouseOverElementAndSelected()
        {
            List<IItem> selectedItems = GetSelectedItems();

            selectedItems.Add(mouseOverItem);

            foreach (IItem i in selectedItems)
                i.SetHiddenFromReal();
        }

        protected void ArrowDown_FromUpMove(object sender, MouseButtonEventArgs e)
        {
            if (mouseOverItem == null)
                return;

            InitMouseOverElementAndSelected();

            mouseDownPoint = GetMainContentMousePosition(e);

            previousMousePosition = mouseDownPoint;

            if (currentCursorState == CursorState.ArrowUp_MoveOnItem_Left)
                SetCursorMode(CursorState.ArrowDown_MoveOnItem_Left);

            if (currentCursorState == CursorState.ArrowUp_MoveOnItem_Right)
                SetCursorMode(CursorState.ArrowDown_MoveOnItem_Right);

            if (currentCursorState == CursorState.ArrowUp_MoveOnItem)            
                SetCursorMode(CursorState.ArrowDown_MoveOnItem_MouseDown);                            
        }

        protected enum LeftRightEnum { Left, Right }

        protected void ItemTryMoveLeftRight(IItem item, double delta, LeftRightEnum LeftRight)
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

        protected void ItemTryMove(IItem item, double deltaX, double deltaY)
        {
            if (!(item is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)item;


            if (item.IsCentered)
            {
                // centered

                item.HiddenCenter += deltaX;

                double newValue = GetSnappedPosition(item.HiddenCenter);

                if (newValue != item.Center)
                    item.Center = newValue;
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

            AxisSegment newSegment = FindVerticalSegment(item.HiddenTop + ( (item.HiddenBottom - item.HiddenTop) / 2 ));

            if(newSegment != null)
                item.Top = newSegment.StartPosition;            
        }

        protected void ArrowMove_DownMoveOnItemLeftRight(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            double deltaX = currentMousePosition.X - previousMousePosition.X;
            double deltaY = currentMousePosition.Y - previousMousePosition.Y;

            switch (currentCursorState)
            {
                case CursorState.ArrowDown_MoveOnItem_Left:

                    foreach (IItem i in GetSelectedAndMouseOverItems())
                        ItemTryMoveLeftRight(i, deltaX, LeftRightEnum.Left);
                    
                    break;

                case CursorState.ArrowDown_MoveOnItem_Right:

                    foreach (IItem i in GetSelectedAndMouseOverItems())
                        ItemTryMoveLeftRight(i, deltaX, LeftRightEnum.Right);

                    break;

                case CursorState.ArrowDown_MoveOnItem_MouseDown:

                    double horizontalDelta = Math.Abs(mouseDownPoint.X - currentMousePosition.X);
                    double verticalDelta = Math.Abs(mouseDownPoint.Y - currentMousePosition.Y);

                    double delta = Math.Sqrt(horizontalDelta * horizontalDelta + verticalDelta * verticalDelta);

                    if (delta > ArrowDown_MoveOnItem_MouseDown_Delta)
                    {
                        SetCursorMode(CursorState.ArrowDown_MoveOnItem_MouseDownAndMove);
                        UpdateCursorShape();
                    }

                    break;

                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:

                    foreach (IItem i in GetSelectedAndMouseOverItems())
                        ItemTryMove(i, deltaX, deltaY);

                    break;
            }

            previousMousePosition = currentMousePosition;
        }

        protected void ArrowMove_ArrowUp_SetMouseCurrentItem(IItem item, CursorState cursorModeDetail)
        {
            SetCursorMode(cursorModeDetail);
            SetMouseOverItem(item);
            UpdateCursorShape();
        }

        protected void ArrowMove_ArrowUp(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(items, currentMousePosition);

            if (element != null && element is IItem)
            {
                double HorizontalItemMoveLeftRightSpan = HorizontalItemMoveLeftRightSpan_Big;

                if(element.Width < HorizontalItemMoveLeftRightSpan_ItemSizeMiddleBoundary)
                    HorizontalItemMoveLeftRightSpan = HorizontalItemMoveLeftRightSpan_Small;

                if (element.Width < HorizontalItemMoveLeftRightSpan_ItemSizeSmallBoundary)
                    HorizontalItemMoveLeftRightSpan = 0;

                IItem item = (IItem)element;                

                if (!isCurrentPenItemCenter && currentMousePosition.X >= item.Left && currentMousePosition.X <= (item.Left + HorizontalItemMoveLeftRightSpan))
                {
                    ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorState.ArrowUp_MoveOnItem_Left);
                    return;
                }

                if (!isCurrentPenItemCenter && currentMousePosition.X >= (item.Right - HorizontalItemMoveLeftRightSpan) && currentMousePosition.X <= item.Right)
                {
                    ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorState.ArrowUp_MoveOnItem_Right);
                    return;
                }

                ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorState.ArrowUp_MoveOnItem);
                return;
            }

            SetCursorMode(CursorState.ArrowUp);
            UpdateCursorShape();            
        }

        protected void ArrowMove_ArrowDown(object sender, MouseEventArgs e)
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
                             
            WpfUtil.SetCursor(Cursors.Arrow);
        }

        protected void ArrowUp_FromDown(object sender, MouseButtonEventArgs e)
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

            currentCursorState = CursorState.ArrowUp;
            SelectionArea.HideSelectionArea();
        }

        protected void ArrowUp_FromMove(object sender, MouseEventArgs e)
        {
            if(currentCursorState == CursorState.ArrowDown_MoveOnItem_Left || currentCursorState == CursorState.ArrowDown_MoveOnItem_Right)
            {
                SetCursorMode(CursorState.ArrowUp);

                foreach(IItem i in GetSelectedAndMouseOverItems())
                    UpdateItem_HorizontalPosition(i);
            }

            if (currentCursorState == CursorState.ArrowDown_MoveOnItem_MouseDownAndMove)
            {
                SetCursorMode(CursorState.ArrowUp);

                foreach (IItem i in GetSelectedAndMouseOverItems())
                {
                    UpdateItem_HorizontalPosition(i);

                    UpdateItem_VerticalPosition(i);
                }
            }
        }

        protected void SelectItem(IItem item)
        {
            item.Select();

            Edge.AddEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
        }

        protected void UnselectItem(IItem item)
        {
            item.Unselect();

            Edge.DeleteVertexByEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
        }

        protected void ArrowUp_FromMoveOnItem_MouseDown(object sender, MouseButtonEventArgs e)
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

            SetCursorMode(CursorState.ArrowUp);
        }

        protected void PerformArrowUp_FromArrowDown_WhileMouseLeave()
        {
            UnselectAllSelectedEdges();

            foreach (FrameworkElement e in items)
                if (e is IItem)
                {
                    IItem item = (IItem)e;

                    item.Unselect();
                }

            SetCursorMode(CursorState.ArrowUp);

            SelectionArea.HideSelectionArea();
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
            if (currentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double positionInBars = (position / HorizontalAD.BaseUnitSize) / HorizontalAD.BarLength;

            double reminder = positionInBars % currentSnapToGridValue;

            if (reminder < (currentSnapToGridValue / 2.0))
                return (positionInBars - reminder) * HorizontalAD.BarLength * HorizontalAD.BaseUnitSize;
            else
                return (positionInBars - reminder + currentSnapToGridValue) * HorizontalAD.BarLength * HorizontalAD.BaseUnitSize;
        }

        protected double getSnapMinmalWidth()
        {
            if (currentSnapToGridValue == 0)
                return 1;

            return currentSnapToGridValue * HorizontalAD.BarLength * HorizontalAD.BaseUnitSize;
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

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex,
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            FrameworkElement newElement;

            if (isDrum)
                newElement = new DrumItem(itemEdge, this, showVelocity);
            else                            
                newElement = new NoteItem(itemEdge, label, this, showLabel, showVelocity);            

            IItem newItem = (IItem)newElement;

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
                newItem.Select();

            AxisSegment itemSegment = GetPitchSegment(pitchVertex);


            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            double endPosition = startPosition + (length * HorizontalAD.BaseUnitSize);


            if (isDrum)
            {
                newItem.Center = startPosition;
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
                TriggerTime = (int)(item.Center / HorizontalAD.BaseUnitSize);

                Length = 0;
            }
            else
            {
                TriggerTime = (int)(item.Left / HorizontalAD.BaseUnitSize);

                Length = (int)(itemWidth / HorizontalAD.BaseUnitSize);
            }

            GraphUtil.SetVertexValue(itemVertex, metaTriggerTime, TriggerTime);
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

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex, octave, note);

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
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Velocity"), defaultVelocity);
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

        protected Point GetDownContentMousePosition(MouseButtonEventArgs e)
        {
            return e.GetPosition(Down);
        }

        protected Point GetDownContentMousePosition(MouseEventArgs e)
        {
            return e.GetPosition(Down);
        }

        protected void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            HideArrowLines();            

            WpfUtil.SetCursor(Cursors.Arrow);

            switch (currentCursorState)
            {
                case (CursorState.PenDown):
                    PerformPenUp();
                    break;

                case (CursorState.ArrowDown):
                    PerformArrowUp_FromArrowDown_WhileMouseLeave();
                    break;
            }

            WhereIsMouse = WhereIsMouseEnum.MouseOutside;
        }

        protected void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            UpdateCursorShape();

            WhereIsMouse = WhereIsMouseEnum.MouseOnMain;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /////////////// DOWN START /////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////        

        protected void MouseEnterHandler_Down(object sender, MouseEventArgs e)
        {
            UpdateCursorShape();

            WhereIsMouse = WhereIsMouseEnum.MouseOnDown;
        }

        protected void MouseLeaveHandler_Down(object sender, MouseEventArgs e)
        {
            HideArrowLines();


            WpfUtil.SetCursor(Cursors.Arrow);

            /*            switch (currentCursorState)
                        {
                            case (CursorState.PenDown):
                                PerformPenUp();
                                break;

                            case (CursorState.ArrowDown):
                                PerformArrowUp_FromArrowDown_WhileMouseLeave();
                                break;
                        }*/

            WhereIsMouse = WhereIsMouseEnum.MouseOutside;
        }

        protected void MouseDownHandler_Down(object sender, MouseButtonEventArgs e)
        {
            switch (currentCursorState)
            {
                case CursorState.PenUp:
                    PenDown_Down(sender, e);
                    break;

             /*   case CursorState.Eraser:
                    EraserDown(sender, e);
                    break;

                case CursorState.ArrowUp:
                    ArrowDown(sender, e);
                    break;

                case CursorState.ArrowUp_MoveOnItem_Left:
                case CursorState.ArrowUp_MoveOnItem_Right:
                case CursorState.ArrowUp_MoveOnItem:
                    ArrowDown_FromUpMove(sender, e);
                    break;*/
            }
        }

        protected void MouseUpHandler_Down(object sender, MouseButtonEventArgs e)
        {
        /*    switch (currentCursorState)
            {
                case CursorState.PenDown:
                    PenUp(sender, e);
                    break;

                case CursorState.ArrowDown:
                    ArrowUp_FromDown(sender, e);
                    break;

                case CursorState.ArrowDown_MoveOnItem_Left:
                case CursorState.ArrowDown_MoveOnItem_Right:
                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowUp_FromMove(sender, e);
                    break;

                case CursorState.ArrowDown_MoveOnItem_MouseDown:
                    ArrowUp_FromMoveOnItem_MouseDown(sender, e);
                    break;

                default:
                    break;
            }*/
        }

        protected void MouseMoveHandler_Down(object sender, MouseEventArgs e)
        {
            UpdateArrowLines(e);

            /*switch (currentCursorState)
            {
                case CursorState.PenDown:
                    PenMove_PenDown(sender, e);
                    break;

                case CursorState.ArrowUp:
                case CursorState.ArrowUp_MoveOnItem_Left:
                case CursorState.ArrowUp_MoveOnItem_Right:
                case CursorState.ArrowUp_MoveOnItem:
                    ArrowMove_ArrowUp(sender, e);
                    break;

                case CursorState.ArrowDown:
                    ArrowMove_ArrowDown(sender, e);
                    break;

                case CursorState.ArrowDown_MoveOnItem_Left:
                case CursorState.ArrowDown_MoveOnItem_Right:
                case CursorState.ArrowDown_MoveOnItem_MouseDown:
                case CursorState.ArrowDown_MoveOnItem_MouseDownAndMove:
                    ArrowMove_DownMoveOnItemLeftRight(sender, e);
                    break;

                default:
                    break;
            }*/
        }

        protected void PenDown_Down(object sender, MouseButtonEventArgs e)
        {            
            mouseDownPoint = GetDownContentMousePosition(e);

            previousMousePosition = mouseDownPoint;

            double mouseY = mouseDownPoint.Y;

            VertexChangeOff = true;

            IEdge newItemEventEdge = AddItemEdge_Down(mouseY, GetSnappedPosition(mouseDownPoint.X));

            AddItem_Down(newItemEventEdge, null);         

            VertexChangeOff = false;
        }

        protected int getValueFromMouseY_Down(double mouseY)
        {
            return 127 - (int)((mouseY / Height_Down) * 127);
        }

        protected IEdge AddItemEdge_Down(double mouseY, double startPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex ControlChangeEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");

            IEdge tempControlChangeEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteControlChangeVertex = tempControlChangeEventEdge.To;

            noteControlChangeVertex.AddEdge(MinusZero.Instance.Is, ControlChangeEvent);
            noteControlChangeVertex.AddVertex(ControlChangeEvent.Get(false, @"Attribute:Number"), CCNumber);
            noteControlChangeVertex.AddVertex(ControlChangeEvent.Get(false, @"Attribute:Value"), getValueFromMouseY_Down(mouseY));
            noteControlChangeVertex.AddVertex(ControlChangeEvent.Get(false, @"Attribute:TriggerTime"), (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01));
            

            IEdge finalEdge = baseVertex.AddEdge(Event, noteControlChangeVertex);

            baseVertex.DeleteEdge(tempControlChangeEventEdge);

            return finalEdge;
        }

        protected void AddItem_Down(IEdge itemEdge, List<IVertex> selectedVertexes)
        {
            return;
            IVertex itemEventVertex = itemEdge.To;

            bool dummy = false;

            int triggerTime = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "TriggerTime:"), ref dummy);

            int length = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Length:"), ref dummy);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(pitchSetVertex,
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            FrameworkElement newElement;

            if (isDrum)
                newElement = new DrumItem(itemEdge, this, showVelocity);
            else
                newElement = new NoteItem(itemEdge, label, this, showLabel, showVelocity);

            IItem newItem = (IItem)newElement;

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
                newItem.Select();

            AxisSegment itemSegment = GetPitchSegment(pitchVertex);


            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            double endPosition = startPosition + (length * HorizontalAD.BaseUnitSize);


            if (isDrum)
            {
                newItem.Center = startPosition;
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

        protected void UnselectAllSelectedEdges()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            TurnOffSelectedEdgesFireChange();
            
            GraphUtil.RemoveAllEdges(sv);

            TurnOnSelectedEdgesFireChange();         
        }

        protected void DrawMainSnapLines()
        {            
            if (currentSnapToGrid == SnapToGridEnum.No_Snap || showSnapLines == false)
                    return;

            double snapWidth = getSnapMinmalWidth();

            Brush lb = (Brush)FindResource("0VeryLightForegroundBrush");

            for (double x = 0 ; x < Width ; x+= snapWidth)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, x, 0, x, Height);

                l.Stroke = lb;

                l.StrokeThickness = 1;

                Main.Children.Add(l);
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
            {
                if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                    AddItem(e, selectedVertexes);

                if (GraphUtil.ExistQueryOut(e.To, "$Is", "ControlChangeEvent"))
                    AddItem_Down(e, selectedVertexes);
            }
            
        }

        protected void InitSequenceVisualierState()
        {
            SetCursorMode(CursorState.ArrowUp);

            currentSnapToGrid = SnapToGridEnum.Bar1;

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

                SetVertexVaribles();

                // SnapToGridComboBox_SelectionChange(); in UpdateVertexValues() does this;
                // VisualiserDraw(); 
            }
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (VertexChangeOff)
                return;            

            if ((sender == Vertex.Get(false, "ShowArrowLines:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            if ((sender == Vertex.Get(false, "ShowSnapLines:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

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
                    currentSnapToGrid = SnapToGridEnum.Bar1;
                    currentSnapToGridValue = 1;
                    break;

                case "1/2 bar":
                    currentSnapToGrid = SnapToGridEnum.Bar1_2;
                    currentSnapToGridValue = 1.0/2;
                    break;

                case "1/4 bar":
                    currentSnapToGrid = SnapToGridEnum.Bar1_4;
                    currentSnapToGridValue = 1.0/4;
                    break;

                case "1/8 bar":
                    currentSnapToGrid = SnapToGridEnum.Bar1_8;
                    currentSnapToGridValue = 1.0/8;
                    break;

                case "1/16 bar":
                    currentSnapToGrid = SnapToGridEnum.Bar1_16;
                    currentSnapToGridValue = 1.0/16;
                    break;

                case "1/32 bar":
                    currentSnapToGrid = SnapToGridEnum.Bar1_32;
                    currentSnapToGridValue = 1.0/32;
                    break;

                case "no snap":
                    currentSnapToGrid = SnapToGridEnum.No_Snap;
                    currentSnapToGridValue = 0;
                    break;
                }

            VisualiserDraw();
        }

        protected void ExtendButton_Click(object sender, RoutedEventArgs e)
        {
            Length += ExtendTimeLength;            

            SaveLength();

            HorizontalAD.SetLength(Length);

            VisualiserDraw();
        }

        protected void PenButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorState.PenUp);
        }

        protected void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorState.Eraser);
        }

        protected void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorState.ArrowUp);
        }

        protected void UnCheckAllCursorButtons()
        {
            EraseButton.IsChecked = false;
            PenButton.IsChecked = false;
            ArrowButton.IsChecked = false;
        }

        protected void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {                
                Dictionary<IVertex, IItem> itemsDictionary = GetItemsDictionary();

                foreach (IVertex v in GetSelectedVertexes())
                {
                    IItem i = itemsDictionary[v];

                    ItemsRemove(i);

                    GraphUtil.DeleteEdgeByToVertex(baseVertex, v);
                }

                UnselectAllSelectedEdges();
            }
            
        }
    }
}
