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
using m0.User;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Foundation;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public class ZoomScrollViewBasedVisualiserBase : UserControl, IListVisualiser, IOwnScrolling, IZoomScrollViewerHost        
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public bool ZoomSliderZero;

        static string[] _MetaTriggeringUpdateVertex = new string[] { };
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] {  };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        protected bool WasThereEdgeAddedRemovedDisposed = false;

        // sequencer specyfic

        protected bool ShowVelocity;
        protected bool ShowArowLines;
        protected bool ShowSnapLines;
        protected int DefaultVelocity;
        protected bool IsDrum;
        protected int CurrentControlChangeNumber;

        protected ISet<IVertex> SelectedVertexes;

        // behavior

        public bool NewItemWidthOneSnapLimit = false;

        // XAML objects

        protected ToggleButton PenButton;
        protected ToggleButton ArrowButton;
        protected ToggleButton EraseButton;
        protected ToggleButton GlueButton;
        protected ToggleButton RazorButton;

        protected Button CutButton;
        protected Button CopyButton;
        protected Button PasteButton;

        protected Button TruncateButton;
        protected Button ExtendButton;

        public ZoomScrollView ZoomScrollView;

        //

        protected bool IsCurrentPenItemCenter;

        protected bool ShowLabel;

        public Canvas Main;
        protected SelectionArea SelectionArea;

        protected IVertex BaseEdgeToMetaVertex;
        protected IVertex VisualiserMetaVertex;

        static IList<string> _listenerScopeQueries = new List<string> { @"", @"BaseEdge:\To:", @"BaseEdge:\To:\", @"BaseEdge:\To:\\" };
        protected virtual IList<string> listenerScopeQueries { get { return _listenerScopeQueries; } }

        public IVertex VisualizedVertex;
        protected IVertex verticalSpanVertex;
        protected IVertex horizontalSpanVertex;
        protected IVertex verticalSpanVertex_Down;


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
            Razor
        }

        protected CursorStateEnum CurrentCursorState;

        protected Point MouseDownPoint;

        protected Point PreviousMousePosition;

        protected Border NewItemShape;

        protected AxisSegment NewItemSegment;

        protected enum SnapToGridEnum { Bar1, Bar1_2, Bar1_4, Bar1_8, Bar1_16, Bar1_32, Bar1_64, Bar1_128, Bar1_256, Bar1_512, No_Snap }

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

        //protected bool VertexChangeOff = false;

        protected FrameworkElement MuseOverItem_Element;

        protected IItem MouseOverItem;

        protected double MouseOverItem_startLeft;

        protected Line HorizontalArrowLine;
        protected Line VerticalArrowLine;

        protected Line HorizontalArrowLine_Down;
        protected Line VerticalArrowLine_Down;

        // DOWN

        public bool HasDown;

        protected ControlChangeDownDecorator DownDecorator;

        public bool ShowCCList = true;

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

        public virtual void ViewAttributesUpdated() {
            BaseEdgeToUpdated();
        }

        public void ScaleChange() { }

        public void SelectedVerticesUpdated() {
            //  if (SelectedEdgesChange != null) // THIS IS TEMPORARY IN ArrowUp_FromMoveOnItem_MouseDown
            //    SelectedEdgesChange();
        }

        public void OnLoad(object sender, RoutedEventArgs e) {  }

        public IVertex GetEdgeByPoint(Point point) { return null; }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement) { return null; }

        public FrameworkElement GetVisualElementByEdge(IVertex edge) { return null; }

        protected string VisualiserName = "NAME";

        public void ZoomScrollViewBasedVisualiserBase_Init(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            MinusZero mz = MinusZero.Instance;

            this.Foreground = (Brush)FindResource("0ForegroundBrush");
            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.BorderThickness = new Thickness(0);
            this.Padding = new Thickness(0);
            this.AllowDrop = true;


            if (mz != null && mz.IsInitialized)
            {
                new ListVisualiserHelper(parentVisualiser,
                    false,
                    VisualiserMetaVertex,
                    this,
                    VisualiserName,
                    this,
                    false,
                    listenerScopeQueries,
                    VisualiserName,
                    baseEdgeVertex,
                    UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

                ((ListVisualiserHelper)VisualiserHelper).CustomVertexChangeEvent += CustomVertexChange;

                ((ListVisualiserHelper)VisualiserHelper).BaseEdgeToEventTriggeringUpdateVertex = false;

                /*ertex = mz.CreateTempVertex();
                Vertex.Value = VisualiserName;//+ this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, VisualiserMetaVertex);

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));
                */
                UpdateVertexValues();

                ZoomScrollView.SetHost(this);

                InitSequenceVisualierState();

                if (HasDown == false)
                    ZoomScrollView.DownAreaVisible = false;
            }
        }

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

            if (Items != null)
            {
                foreach (IItem i in Items)
                    ItemsDictinaryHolder.Add(i.BaseEdge.To, i);

                NeedToRebuildItemsDictionary = false;
            }
        }

        protected virtual void UpdateVertexValues() { }

        protected void SetButtonComponentName(ContentControl c, string text)
        {
            StackPanel s = (StackPanel)c.Content;

            TextBlock t = (TextBlock)s.Children[1];

            t.Text = text;
        }

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

                case CursorStateEnum.Razor:

                    SetChecked(RazorButton, true);
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
                    WpfUtil.SetCursorFromResource("/m0_desktop;component/_resources/basic/eraser.cur");
                    break;

                case CursorStateEnum.PenDown:
                case CursorStateEnum.PenUp:
                    WpfUtil.SetCursorFromResource("/m0_desktop;component/_resources/basic/pen.cur");
                    break;

                case CursorStateEnum.Glue:                
                    WpfUtil.SetCursorFromResource("/m0_desktop;component/_resources/basic/glue.cur");
                    break;

                case CursorStateEnum.Razor:
                    WpfUtil.SetCursorFromResource("/m0_desktop;component/_resources/basic/razor.cur");
                    break;
            }
        }

        bool VisuliseserDraw_NeedsInitilisation = true;

        public void VisualiserDraw()
        {
            if (VisualizedVertex == null || isLoaded == false)
                return;

            int PositionMark_Copy = PositionMark;

            if (VisuliseserDraw_NeedsInitilisation)
            {
                SetupLocalVariablesFromBaseVertexVertexes();

                UpdateVariablesFromBaseVertex();
            }

            SetAxisDecorators();

            if (VisuliseserDraw_NeedsInitilisation)
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

            DrawMain();

            Draw_Down();            

            VisuliseserDraw_NeedsInitilisation = false;

            PositionMark = PositionMark_Copy;
        }

        protected virtual void UpdateVariablesFromBaseVertex() { }

        protected virtual void SetupLocalVariablesFromBaseVertexVertexes() { }

        protected void SaveLength()
        {
            if (GraphUtil.GetIntegerValueOr0(VisualizedVertex.Get(false, "Length:")) != Length)
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                GraphUtil.SetVertexValue(VisualizedVertex, BaseEdgeToMetaVertex.Get(false, "Length"), Length);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }
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

            Canvas.SetZIndex(HorizontalArrowLine, 999);
            Canvas.SetZIndex(VerticalArrowLine, 999);

            HorizontalArrowLine_Down = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));
            VerticalArrowLine_Down = WpfUtil.CreateLine(1, (Brush)FindResource("0LightHighlightBrush"));

            Canvas.SetZIndex(HorizontalArrowLine_Down, 999);
            Canvas.SetZIndex(VerticalArrowLine_Down, 999);
        }

        protected void ItemsAdd(IItem i)
        {
            NeedToRebuildItemsDictionary = true;
            Items.Add((FrameworkElement)i);

            i.Add(Main);
        }

        protected void ItemsRemove(IItem i)
        {
            Items.Remove((FrameworkElement)i);

            i.Remove();
        }

        protected virtual void RemoveItemVertex(IItem i)
        {
            IEdge eventEdge = i.BaseEdge;

            GraphUtil.DeleteEdgeByToVertex(VisualizedVertex, eventEdge.To);

            EdgeHelper.DeleteVertexByEdgeTo(Vertex.Get(false, "SelectedEdges:"), eventEdge.To);            
        }

        protected void SetupScrollViewer()
        {
            ZoomScrollView.SetHost(this);
            ZoomScrollView.SetMainContent(Main);
        }

        protected void UpdateMainSize()
        {
            if (HorizontalAD != null && VerticalAD != null)
            {
                Width = HorizontalAD.Size.Width;
                Height = VerticalAD.Size.Height;

                Main.Width = Width;
                Main.Height = Height;
            }
        }

        protected void DrawMain()
        {
            if (Main == null)
                return;

            Main.Children.Clear();

            InitialiseItems();

            SelectionArea = new SelectionArea(Main);

            DrawBackground();

            DrawMainSegments();

            DrawSnapLines();

            DrawMainLines();

            DrawBox();

            DrawItems();

            DrawArrowLines();
            

            CreateAndDrawPositionMark();
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
                    if(!CheckIfOpenActionAndPerform(e))
                        ArrowDown_FromUpMove(sender, e);
                    break;

                case CursorStateEnum.Razor:
                    RazorDown(sender, e);
                    break;

                case CursorStateEnum.Glue:
                    GlueDown(sender, e);
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

            if (IsCurrentPenItemCenter || NewItemWidthOneSnapLimit)
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
            if (IsCurrentPenItemCenter || NewItemWidthOneSnapLimit)
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

            if(NewItemShape != null)
                WpfUtil.SetPositionAbsolute(NewItemShape, left, NewItemSegment.StartPosition, right, NewItemSegment.EndPosition);
        }

        protected void PerformPenUp()
        {
            PerformPenUp_part1();

            PerformPenUp_part2();
        }

        protected virtual void PerformPenUp_part1()
        {
            Main.Children.Remove(NewItemShape);
        }

        protected virtual void PerformPenUp_part2()
        {
            SetCursorMode(CursorStateEnum.PenUp);
        }

        protected virtual void PenUp(object sender, MouseButtonEventArgs e)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            PerformPenUp_part1();

            //VertexChangeOff = true;

            IEdge newItemEventEdge;

            if (IsCurrentPenItemCenter)
                newItemEventEdge = AddItemVertex(NewItemSegment, GetSnappedPosition(MouseDownPoint.X), 0);
            else if (NewItemWidthOneSnapLimit)
                newItemEventEdge = AddItemVertex(NewItemSegment, GetSnappedPosition(MouseDownPoint.X), GetSnapMinimalWidth());
            else
            {
                if (NewItemShape != null && NewItemShape.Width == 0)
                {
                    //VertexChangeOff = false;

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////
                    
                    return;
                }

                newItemEventEdge = AddItemVertex(NewItemSegment, Canvas.GetLeft(NewItemShape), NewItemShape.Width);
            }
            
            //AddItem(newItemEventEdge, null);
            // to be done automatically by events

            

            PerformPenUp_part2();

            //VertexChangeOff = false;

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        protected void EraserDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items, currentMousePosition);

            if (element != null && element is IItem)
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                IItem item = (IItem)element;

                //VertexChangeOff = true;

                /*if (MainItemsSyncedWithDown)
                {
                    var dict_down = GetItemsDictionary_Down();

                    IItem item_down = null;
                    
                    if(dict_down.ContainsKey(item.BaseEdge.To))
                        item_down = dict_down[item.BaseEdge.To];

                    if (item_down != null)                                        
                        ItemsRemoveAndRemoveAllEdges_Down(item_down);                    
                }*/

                RemoveItemVertex(item);

                //VertexChangeOff = false;

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }
        }

        bool NoControlKeyPressed()
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                return true;

            return false;
        }

        protected bool CheckIfOpenActionAndPerform(MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (e.ClickCount == 2 && NoControlKeyPressed())
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                        item.OpenFormVisualiser();
                    else
                        item.OpenDefaultVisualiser();

                    UnselectAllSelectedEdges();

                    return true;
                }
            }

            return false;
        }

        protected void ArrowDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (e.ClickCount == 2 && NoControlKeyPressed())
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                        item.OpenFormVisualiser();
                    else
                        item.OpenDefaultVisualiser();

                    UnselectAllSelectedEdges();
                }
                else
                {
                    if (NoControlKeyPressed())
                    {
                        UnselectAllSelectedItems();
                        SelectItem(item);
                    }
                    else
                    {
                        if (item.IsSelected)
                            UnselectItem(item);
                        else
                            SelectItem(item);
                    }
                }
            }
            else
            {
                SetCursorMode(CursorStateEnum.ArrowDown);

                UnselectAllSelectedItems();

                PreviousSelectedItemContext = MainDownEnum.Main;

                SelectionArea.StartSelection(currentMousePosition);
            }
        }

        protected List<IItem> GetSelectedItems()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

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

        protected virtual List<IItem> GetSelectedAndMouseOverItems(MainDownEnum actualContext)
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

            if(!GetSelectedItems().Contains(MouseOverItem) && NoControlKeyPressed())
                UnselectAllSelectedItems();

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
                if (delta + GetSnapMinimalWidth_Screen() >= element.Width)
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
                if (element.Width + delta - GetSnapMinimalWidth_Screen() <= 0)
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

        protected virtual void ArrowMove_ArrowUp(object sender, MouseEventArgs e)
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

        protected virtual IList<FrameworkElement> GetElementsAtFromListByArea(List<FrameworkElement> Items, double left, double top, double right, double bottom)
        {
            return WpfUtil.GetElementsAtFromListByArea(Items, left, top, right, bottom);
        }

        protected void ArrowMove_ArrowDown(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            SelectionArea.MoveSelectionArea(currentMousePosition);

            IList<FrameworkElement> matched = GetElementsAtFromListByArea(Items, SelectionArea.Left - 2, SelectionArea.Top - 2, SelectionArea.Right + 2, SelectionArea.Bottom + 2);

            foreach (FrameworkElement _e in Items)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        item.SelectHighlight();
                    else
                        item.NoHighlight();
                }

            WpfUtil.SetCursor(Cursors.Arrow);
        }

        protected void SaveSelectionArea()
        {
            IList<FrameworkElement> matched = GetElementsAtFromListByArea(Items, SelectionArea.Left - 2, SelectionArea.Top - 2, SelectionArea.Right + 2, SelectionArea.Bottom + 2);

            UnselectAllSelectedEdges();

            foreach (FrameworkElement _e in Items)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        SelectItem(item);
                    else
                        item.NoHighlight();
                }

            CurrentCursorState = CursorStateEnum.ArrowUp;
            SelectionArea.HideSelectionArea();
        }

        protected void ArrowUp_FromDown(object sender, MouseButtonEventArgs e)
        {
            SaveSelectionArea();
        }

        protected virtual void After_ArrowUp_FromMove() { }        

        protected void ArrowUp_FromMove(object sender, MouseEventArgs e)
        {
            if (CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_Left || CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_Right)
            {
                SetCursorMode(CursorStateEnum.ArrowUp);

                bool ForceVertexChangeOff_history = VisualiserHelper.ForceVertexChangeOff;
                VisualiserHelper.ForceVertexChangeOff = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                    UpdateItem_HorizontalPosition(i);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
                
                VisualiserHelper.ForceVertexChangeOff = ForceVertexChangeOff_history;

                After_ArrowUp_FromMove();
            }

            if (CurrentCursorState == CursorStateEnum.ArrowDown_MoveOnItem_MouseDownAndMove)
            {
                SetCursorMode(CursorStateEnum.ArrowUp);

                bool ForceVertexChangeOff_history = VisualiserHelper.ForceVertexChangeOff;
                VisualiserHelper.ForceVertexChangeOff = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Main))
                {
                    UpdateItem_VerticalPosition(i);

                    UpdateItem_HorizontalPosition(i);                 
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////               
                
                VisualiserHelper.ForceVertexChangeOff = ForceVertexChangeOff_history;

                After_ArrowUp_FromMove();
            }

            if (MainItemsSyncedWithDown)
                Draw_Down();
        }

        protected virtual void RazorDown(object sender, MouseButtonEventArgs e) { }

        protected virtual void GlueDown(object sender, MouseButtonEventArgs e) { }

        protected MainDownEnum GetItemContext(IItem item)
        {
            if (item is NoteItem || item is DrumItem || item is SequenceEventItem || item is Set2DItem)
                return MainDownEnum.Main;

            if (item is ControlChangeItem)
                return MainDownEnum.Down;

            return MainDownEnum.Undefined; // fallback
        }

        protected void AddToSelectedEdges(IEdge edge)
        {
            EdgeHelper.AddEdgeVertex(Vertex.Get(false, "SelectedEdges:"), edge);
        }

        protected void SelectItem(IItem item)
        {
            MainDownEnum ic = GetItemContext(item);

            if (PreviousSelectedItemContext != ic)
                UnselectAllSelectedItems();

            item.SelectHighlight();

            AddToSelectedEdges(item.BaseEdge);

            PreviousSelectedItemContext = ic;
        }

        protected void UnselectItem(IItem item)
        {
            item.NoHighlight();

            EdgeHelper.DeleteVertexByEdge(Vertex.Get(false, "SelectedEdges:"), item.BaseEdge);
        }

        protected void ArrowUp_FromMoveOnItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (NoControlKeyPressed())
                {                    
                    UnselectAllSelectedItems();
                    SelectItem(item);                    
                }
                else
                {
                    if (item.IsSelected)
                        UnselectItem(item);
                    else
                        SelectItem(item);
                }

                // 
                if (SelectedEdgesChange != null) // THIS SHOULD BE IN SelectedVerticesUpdated()
                    SelectedEdgesChange();
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
        }

        protected void PerformArrowUp_FromArrowDown_WhileMouseLeave()
        {
            SaveSelectionArea();
        }

        protected virtual AxisSegment FindVerticalSegment(double position)
        {
            foreach (AxisSegment s in VerticalAD.Segments)
                if (s.StartPosition < position && position < s.EndPosition)
                    return s;

            return null;
        }

        public virtual double GetSnappedPosition(double position)
        {
            double CurrentSnapToGridValue_corrected = CurrentSnapToGridValue * 16;

            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double positionInBars = (position / HorizontalAD.BaseUnitSize) / HorizontalAD.SegmentLength;

            double reminder = positionInBars % CurrentSnapToGridValue_corrected;

            if (reminder < (CurrentSnapToGridValue_corrected / 2.0))
                return (positionInBars - reminder) * HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;
            else
                return (positionInBars - reminder + CurrentSnapToGridValue_corrected) * HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;
        }

        protected virtual double GetSnapMinimalWidth_Screen()
        {
            if (CurrentSnapToGridValue == 0)
                return 1;

            double CurrentSnapToGridValue_corrected = CurrentSnapToGridValue * 16;

            return CurrentSnapToGridValue_corrected * HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;
        }

        protected virtual int GetSnapMinimalWidth()
        {
            if (CurrentSnapToGridValue == 0)
                return 1;

            return (int)(CurrentSnapToGridValue * Midi.Standard.MidiTicksPerBar);
        }

        protected AxisSegment GetVerticalSegment(IVertex baseVertex)
        {
            foreach (AxisSegment s in VerticalAD.Segments)
                if (s.BaseVertex == baseVertex)
                    return s;

            return null;
        }

        protected virtual void AddItemByEdge(IEdge itemEdge, ISet<IVertex> selectedVertexes) { }

        protected virtual void RemoveItemByEdge(IEdge itemEdge) {
            Dictionary<IVertex, IItem> ItemsDictinary = GetItemsDictionary();
            Dictionary<IVertex, IItem> ItemsDictinary_Down = GetItemsDictionary_Down();

            IItem item = null;
            
            if(ItemsDictinary.ContainsKey(itemEdge.To))
                item = ItemsDictinary[itemEdge.To];

            if (item != null)
                ItemsRemove(item);

            IItem item_Down = null;
            
            if(ItemsDictinary_Down.ContainsKey(itemEdge.To))
                item_Down = ItemsDictinary_Down[itemEdge.To];

            if (item_Down != null)
                ItemsRemove_Down(item_Down);
        }

        protected virtual void UpdateItem_HorizontalPosition(IItem item) {}

        protected virtual void UpdateItem_VerticalPosition(IItem item) {}

        protected virtual IEdge AddItemVertex(AxisSegment itemSegment, double startPosition, double lengthPosition) { return null; }

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

            if (GraphUtil.GetIntegerValueOr0(baseVertex.Get(false, @"Note:")) == GraphUtil.GetIntegerValueOr0(noteVertex.Get(false, @"Note:"))
                && GraphUtil.GetIntegerValueOr0(baseVertex.Get(false, @"Octave:")) == GraphUtil.GetIntegerValueOr0(noteVertex.Get(false, @"Octave:")))
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

            if (ShowCCList)
            {
                DownDecorator.ShowCCList = true;
                DownDecorator.SetBaseVertex(verticalSpanVertex_Down);
            }
            else
                DownDecorator.ShowCCList = false;

            SetCurrentControlChangeNumber((int)DownDecorator.Selection);

            DownDecorator.SelectionChanged += DownDecorator_SelectionChanged;

            Down = new Canvas();

            Down.SizeChanged += Down_SizeChanged;
            Down.Loaded += Down_Loaded;

            ZoomScrollView.SetDownContent((FrameworkElement)DownDecorator, Down);

            Height_Down = ZoomScrollView.InitialDownHeight;

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

        protected void DrawMainLines_Down()
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

                l.Stroke = (Brush)FindResource("0VeryLightForegroundBrush");                 

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

            DrawMainLines_Down();

            DrawArrowLines_Down();

            DrawItems_Down();

            CreateAndDrawPositionMark_Down();
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

            //VertexChangeOff = true;

            bool isUpdate = false;

            bool isNoteEvent = false;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            AddItemVertex_Down(mouseY, GetSnappedPosition(mouseDownPoint.X), out isUpdate, out isNoteEvent);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            /*if (isNoteEvent)
                GetItemsDictionary()[newItemEventEdge.To].Update();

            if (newItemEventEdge != null)
                AddItemByEdge_Down(newItemEventEdge, null, isUpdate, isNoteEvent);*/

            //VertexChangeOff = false;
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

        protected virtual bool IsVelocityHavingVertex(IVertex v)
        {
            if (v.Get(false, @"$Is:NoteEvent") != null)
                return true;

            return false;
        }

        static IVertex r = MinusZero.Instance.Root;

        static IVertex musicEvent = r.Get(false, @"System\Lib\Music\Event");
        static IVertex musicControlChangeEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");
        static IVertex musicNoteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

        protected virtual IEdge AddItemVertex_Down(double mouseY, double startPosition, out bool isUpdate, out bool isVelocityHavingEvent) {
            isUpdate = false;


            int triggerTime = (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01);

            IEdge eventEdge = null;

            isVelocityHavingEvent = false;

            List<IItem> existingItems = GetDownItemFromNumberTriggerTimeDictionary(CurrentControlChangeNumber, triggerTime);

            if (existingItems == null && MainItemsSyncedWithDown)
                return null;

            if (existingItems != null)
            {
                IItem item = existingItems[0];

                eventEdge = item.BaseEdge;

                isUpdate = true;

                if (IsVelocityHavingVertex(eventEdge.To))
                    isVelocityHavingEvent = true;
            }
            else
                eventEdge = VisualizedVertex.AddVertexAndReturnEdge(musicEvent, null);

            IVertex eventVertex = eventEdge.To;

            if (!isUpdate)
                eventVertex.AddEdge(MinusZero.Instance.Is, musicControlChangeEvent);

            if (isVelocityHavingEvent)
                GraphUtil.SetVertexValue(eventVertex, musicNoteEvent.Get(false, @"Attribute:Velocity"), ControlChangeItem.getValueFromMouseY_Down(mouseY, Height_Down));
            else
            {
                GraphUtil.SetVertexValue(eventVertex, musicControlChangeEvent.Get(false, @"Attribute:Number"), CurrentControlChangeNumber);
                GraphUtil.SetVertexValue(eventVertex, musicControlChangeEvent.Get(false, @"Attribute:Value"), ControlChangeItem.getValueFromMouseY_Down(mouseY, Height_Down));
                GraphUtil.SetVertexValue(eventVertex, musicControlChangeEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            }

            return eventEdge;
        }
        
        protected virtual void AddItemByEdge_Down(IEdge itemEdge, ISet<IVertex> selectedVertexes, bool isUpdate, bool isNoteEvent)
        {
            if (Height_Down == 0)
                return;

            IVertex itemEventVertex = itemEdge.To;


            ControlChangeItem item = null;

            if (isUpdate)
                item = (ControlChangeItem)GetItemsDictionary_Down()[itemEventVertex];
            else
                item = new ControlChangeItem(itemEdge, this);


           UpdateItem_Down(itemEdge, item);


            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex) && !isNoteEvent)
            {
                item.SelectHighlight();
                PreviousSelectedItemContext = MainDownEnum.Down;
            }


            if (!isUpdate)
                ItemsAdd_Down(item); // need this as item.Canvas needs to be set for the cc top mark
        }

        protected virtual void UpdateItem_Down(IEdge itemEdge, IItem _item)
        {
            if (Height_Down == 0)
                return;

            IVertex itemEventVertex = itemEdge.To;


            bool isNoteEvent = true;

            if (GraphUtil.ExistQueryOut(itemEventVertex, "$Is", "ControlChangeEvent"))
                isNoteEvent = false;


            bool dummy = false;

            ControlChangeItem item = (ControlChangeItem)_item;

            IVertex itemVertex = item.BaseEdge.To;

            int triggerTime = GraphUtil.GetIntegerValue(itemVertex.Get(false, "TriggerTime:"), ref dummy);

            int value;

            if (isNoteEvent)
                value = GraphUtil.GetIntegerValue(itemVertex.Get(false, "Velocity:"), ref dummy);
            else
                value = GraphUtil.GetIntegerValue(itemVertex.Get(false, "Value:"), ref dummy);

            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            item.HorizontalCenter = startPosition;
            item.VerticalCenter = Height_Down - (((double)value / 127) * Height_Down);
        }

        protected void ItemsAdd_Down(IItem i)
        {
            NeedToRebuildItemsDictionary_Down = true;
            Items_Down.Add((FrameworkElement)i);

            i.Add(Down);
        }

        protected void ItemsRemove_Down(IItem i)
        {
            Items_Down.Remove((FrameworkElement)i);

            i.Remove();
        }

        protected void ItemsRemoveAndRemoveAllEdges_Down(IItem i)
        {
            IEdge eventEdge = i.BaseEdge;

            GraphUtil.DeleteEdgeByToVertex(VisualizedVertex, eventEdge.To);

            EdgeHelper.DeleteVertexByEdgeTo(Vertex.Get(false, "SelectedEdges:"), eventEdge.To);

            //NeedToRebuildItemsDictionary_Down = true;

            Items_Down.Remove((FrameworkElement)i);

            //i.Remove();
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

        protected virtual void RebuildItemsDictionary_Down()
        {
            ItemsDictinaryHolder_Down.Clear();

            ItemsDictinaryHolder_Number_TriggerTime_Down.Clear();

            if(Items_Down != null)
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

        protected virtual void DrawItems_Down() { }

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

                //VertexChangeOff = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                ItemsRemoveAndRemoveAllEdges_Down(item);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                //VertexChangeOff = false;
            }
        }

        protected void ArrowDown_Down(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetDownContentMousePosition(e);

            FrameworkElement elementFound = WpfUtil.GetElementAtFromList(Items_Down, currentMousePosition);

            if (elementFound != null && elementFound is IItem)
            {
                IItem item = (IItem)elementFound;

                if (e.ClickCount == 2 && NoControlKeyPressed())
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                        item.OpenFormVisualiser();
                    else
                        item.OpenDefaultVisualiser();
                }
                else
                {
                    if (NoControlKeyPressed())
                    {
                        UnselectAllSelectedItems();
                        SelectItem(item);
                    }
                    else
                    {
                        if (item.IsSelected)
                            UnselectItem(item);
                        else
                            SelectItem(item);
                    }
                }
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

            IList<FrameworkElement> matched = GetElementsAtFromListByArea(Items_Down,
                SelectionArea_Down.Left - 2,
                SelectionArea_Down.Top - 2,
                SelectionArea_Down.Right + 2,
                SelectionArea_Down.Bottom + 2);

            foreach (FrameworkElement _e in Items_Down)
                if (_e is IItem)
                {
                    IItem item = (IItem)_e;

                    if (matched.Contains(_e))
                        item.SelectHighlight();
                    else
                        item.NoHighlight();
                }

            WpfUtil.SetCursor(Cursors.Arrow);
        }

        protected void SaveSelectionArea_Down()
        {
            IList<FrameworkElement> matched = GetElementsAtFromListByArea(Items_Down,
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
                        item.NoHighlight();
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

                bool ForceVertexChangeOff_history = VisualiserHelper.ForceVertexChangeOff;
                VisualiserHelper.ForceVertexChangeOff = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                foreach (IItem i in GetSelectedAndMouseOverItems(MainDownEnum.Down))
                    UpdateItem_HorizontalPosition(i);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                VisualiserHelper.ForceVertexChangeOff = ForceVertexChangeOff_history;
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

                if (NoControlKeyPressed())
                {
                    UnselectAllSelectedItems();
                    SelectItem(item);
                }
                else
                {
                    if (item.IsSelected)
                        UnselectItem(item);
                    else
                        SelectItem(item);
                }
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
        }

        protected void RemoveDuplicatedDownItems_SelectedEdgesFirst()
        {
            IVertex selectedEdges = Vertex.GetAll(false, @"SelectedEdges:\");

            //VertexChangeOff = true;

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

            //VertexChangeOff = false;
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

        protected void UnselectAllSelectedItems()
        {
            foreach (IItem i in Items)
                i.NoHighlight();

            foreach (IItem i in Items_Down)
                i.NoHighlight();

            UnselectAllSelectedEdges();
        }

        public void UnselectAllSelectedEdges()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            //TurnOffSelectedEdgesFireChange();

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);
            //GraphUtil.RemoveAllEdges(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            //TurnOnSelectedEdgesFireChange();
        }

        protected void DrawSnapLines()
        {
            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap || ShowSnapLines == false)
                return;

            double snapWidth = GetSnapMinimalWidth_Screen();

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

            double snapWidth = GetSnapMinimalWidth_Screen();

            Brush lb = (Brush)FindResource("0VeryVeryLightForegroundBrush");

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

        protected void DrawBox()
        {
            Rectangle r = new Rectangle();

            WpfUtil.SetPosition(r, 0, 0, Width, Height);

            r.Stroke = (Brush)FindResource("0ForegroundBrush");

            Main.Children.Add(r);
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

        protected virtual void DrawItems()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                    AddItemByEdge(e, selectedVertexes);
        }

        protected void InitSequenceVisualierState()
        {
            SetCursorMode(CursorStateEnum.ArrowUp);

            CurrentSnapToGrid = SnapToGridEnum.Bar1_16;

            CurrentSnapToGridValue = 1.0/16;
        }        

        public virtual void BaseEdgeToUpdated()
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

        protected virtual INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe) { return null; }

        protected virtual void EdgeRemoved(IEdge edge)
        {            
            RemoveItemByEdge(edge);

            WasThereEdgeAddedRemovedDisposed = true;
        }

        protected virtual void EdgeAdded(IEdge edge)
        {
            AddItemByEdge(edge, SelectedVertexes);

            WasThereEdgeAddedRemovedDisposed = true;
        }

        protected virtual void EdgeDisposed(IEdge edge)
        {
            BaseEdgeToUpdated();

            WasThereEdgeAddedRemovedDisposed = true;
        }

        protected virtual INoInEdgeInOutVertexVertex CustomVertexChange(IExecution exe)
        {
            ((ListVisualiserHelper)VisualiserHelper).VertexChangeLogic(exe);

            SelectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            return CheckBaseEdgeChange(exe);
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        protected bool IsDisposed = false;

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                VisualiserHelper.Dispose();

                DispachSubControls();

                IsDisposed = true;
            }
        }

        protected void DispachSubControls()
        {
            if (ZoomScrollView is IDisposable)
                ((IDisposable)ZoomScrollView).Dispose();
        }

        protected bool isLoaded = false;
        public virtual void ChildControlsLoaded()
        {
            isLoaded = true;

            if (ZoomSliderZero)
            {
                ZoomScrollView.HorizontalZoomSlider_public.Value = 0;
                ZoomScrollView.VerticalZoomSlider_public.Value = 0;
            }

            VisualiserDraw();
        }

        protected virtual void SnapToGridComboBox_SelectionChange()
        {
            switch (Vertex.Get(false, "SnapToGrid:").Value.ToString())
            {
                case "1/16 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_16;
                    CurrentSnapToGridValue = 1.0 / 16;
                    break;

                case "1/32 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_32;
                    CurrentSnapToGridValue = 1.0 / 32;
                    break;

                case "1/64 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_64;
                    CurrentSnapToGridValue = 1.0 / 64;
                    break;

                case "1/128 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_128;
                    CurrentSnapToGridValue = 1.0 / 128;
                    break;

                case "1/256 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_256;
                    CurrentSnapToGridValue = 1.0 / 256;
                    break;

                case "1/512 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_512;
                    CurrentSnapToGridValue = 1.0 / 512;
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
            SetChecked(RazorButton, false);
        }

        protected void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                Delete();

            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                Copy();

            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                Paste();
        }

        protected void Delete()
        {
            Dictionary<IVertex, IItem> itemsDictionary = GetItemsDictionary();

            Dictionary<IVertex, IItem> itemsDictionary_Down = GetItemsDictionary_Down();

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            foreach (IVertex v in ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes())
            {
                IItem i = null;

                if (itemsDictionary.ContainsKey(v))
                {
                    i = itemsDictionary[v];

                    RemoveItemVertex(i);
                }

                if (itemsDictionary_Down.ContainsKey(v))
                {
                    i = itemsDictionary_Down[v];

                    ItemsRemoveAndRemoveAllEdges_Down(i);
                }
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            UnselectAllSelectedEdges();
        }

        protected virtual void TruncateButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Length - ExtendTimeLength) <= 0)
                return;

            Length -= ExtendTimeLength;

            SaveLength();

            HorizontalAD.ValueSpaceMax = Length;

            VisualiserDraw();
        }

        protected virtual void ExtendButton_Click(object sender, RoutedEventArgs e)
        {
            Length += ExtendTimeLength;

            SaveLength();

            HorizontalAD.ValueSpaceMax = Length;

            VisualiserDraw();
        }

        protected virtual int MusicTimeSnapCorrect(int toCorrect)
        {
            if (CurrentSnapToGridValue == 0)
                return toCorrect;

            double snapSizeInMusicTime = CurrentSnapToGridValue * (double)Midi.Standard.MidiTicksPerBar;

            double numberOfSnaps = ((double)toCorrect) / snapSizeInMusicTime;

            double numberOfSnapsFloor = Math.Floor(numberOfSnaps);

            double rest = ((double)toCorrect) - numberOfSnapsFloor * snapSizeInMusicTime;

            if (rest < snapSizeInMusicTime / 2)
                return (int)(numberOfSnapsFloor * snapSizeInMusicTime);
            else
                return (int)((numberOfSnapsFloor + 1) * snapSizeInMusicTime);
        }

        protected virtual int MusicTimeSnapCorrect_Up(int toCorrect)
        {
            toCorrect = toCorrect - 1; // hacky :)

            if (CurrentSnapToGridValue == 0)
                return toCorrect;

            double snapSizeInMusicTime = CurrentSnapToGridValue * (double)Midi.Standard.MidiTicksPerBar;

            double numberOfSnaps = ((double)toCorrect) / snapSizeInMusicTime;

            double numberOfSnapsFloor = Math.Floor(numberOfSnaps);

            double rest = ((double)toCorrect) - numberOfSnapsFloor * snapSizeInMusicTime;

            
            return (int)((numberOfSnapsFloor + 1) * snapSizeInMusicTime);
        }

        Line PositionMarkLine;

        Line PositionMarkLine_Down;

        Line PositionMarkPrimLine_Beg;

        Line PositionMarkPrimLine_End;

        Line PositionMarkPrimLine_Beg_Down;

        Line PositionMarkPrimLine_End_Down;

        protected bool PositionMarkEnabled = false;

        protected bool PositionMarkPrimEnabled = false;

        public void CreateAndDrawPositionMark()
        {
            if (PositionMarkEnabled)
                PositionMarkLine = Common.CreatePositionMark(this.Main, PositionMark_Screen, Height);

            if (PositionMarkPrimEnabled)
            {
                PositionMarkPrimLine_Beg = Common.CreatePositionMarkPrim(this.Main, PositionMarkPrim_Beg_Screen, Height);
                PositionMarkPrimLine_End = Common.CreatePositionMarkPrim(this.Main, PositionMarkPrim_End_Screen, Height);
            }
        }

        public void CreateAndDrawPositionMark_Down()
        {
            if (!HasDown)
                return;

            if (PositionMarkEnabled)                
                PositionMarkLine_Down = Common.CreatePositionMark(this.Down, PositionMark_Screen, Height_Down);

            if (PositionMarkPrimEnabled)
            {
                PositionMarkPrimLine_Beg_Down = Common.CreatePositionMarkPrim(this.Main, PositionMarkPrim_Beg_Screen, Height);
                PositionMarkPrimLine_End_Down = Common.CreatePositionMarkPrim(this.Main, PositionMarkPrim_End_Screen, Height);
            }
        }

        public virtual void UpdatePositionMark()
        {
            bool needToPerformHorizontalADPositionMarkUpdate = false;

            if (PositionMarkEnabled && PositionMarkLine != null)
            {
                Common.UpdatePositionMark(PositionMarkLine, PositionMark_Screen, Height);

                if (HasDown)
                    Common.UpdatePositionMark(PositionMarkLine_Down, PositionMark_Screen, Height_Down);

                needToPerformHorizontalADPositionMarkUpdate = true;
            }

            if (PositionMarkPrimEnabled && PositionMarkPrimLine_Beg != null)
            {
                Common.UpdatePositionMark(PositionMarkPrimLine_Beg, PositionMarkPrim_Beg_Screen, Height);
                Common.UpdatePositionMark(PositionMarkPrimLine_End, PositionMarkPrim_End_Screen, Height);

                if (HasDown)
                {
                    Common.UpdatePositionMark(PositionMarkPrimLine_Beg, PositionMarkPrim_Beg_Screen, Height_Down);
                    Common.UpdatePositionMark(PositionMarkPrimLine_End, PositionMarkPrim_End_Screen, Height_Down);
                }

                needToPerformHorizontalADPositionMarkUpdate = true;
            }

            if (needToPerformHorizontalADPositionMarkUpdate)
                HorizontalAD.PositionMarkUpdate();
        }

        protected virtual int ScreenPositionToMusicTime(double position, bool performSnapCorrection) { return 0; }

        protected virtual double MusicTimeToScreenPosition(int musicTime, bool performSnapCorrection) { return 0; }

        protected double positionMark_Screen;

        public virtual double PositionMark_Screen
        {
            get
            {
                return positionMark_Screen;
            }
            set
            {                
                positionMark = ScreenPositionToMusicTime(value, true);

                positionMark_Screen = MusicTimeToScreenPosition(positionMark, true);

                UpdatePositionMark();
            }
        }

        protected int positionMark;

        public virtual int PositionMark
        {
            get
            {
                return positionMark;
            }

            set
            {                
                positionMark_Screen = MusicTimeToScreenPosition(value, true);

                positionMark = ScreenPositionToMusicTime(PositionMark_Screen, true);

                UpdatePositionMark();
            }
        }        

        static IVertex loopBegMeta = r.Get(false, @"System\Lib\Music\Song\LoopBeg");
        static IVertex loopEndMeta = r.Get(false, @"System\Lib\Music\Song\LoopEnd");


        double positionMarkPrim_Beg_Screen = -1;

        public double PositionMarkPrim_Beg_Screen
        {
            get
            {
                if(positionMarkPrim_Beg_Screen == -1 || HorizontalAD != null)
                    positionMarkPrim_Beg_Screen = MusicTimeToScreenPosition(positionMarkPrim_Beg, true);

                return positionMarkPrim_Beg_Screen;
            }
            set
            {
                positionMarkPrim_Beg = ScreenPositionToMusicTime(value, true);
                GraphUtil.SetVertexValue(VisualizedVertex, loopBegMeta, positionMarkPrim_Beg);

                positionMarkPrim_Beg_Screen = MusicTimeToScreenPosition(positionMarkPrim_Beg, true);

                UpdatePositionMark();
            }
        }

        protected int positionMarkPrim_Beg;

        public int PositionMarkPrim_Beg
        {
            get
            {
                return positionMarkPrim_Beg;
            }

            set
            {
                positionMarkPrim_Beg_Screen = MusicTimeToScreenPosition(value, true);

                positionMarkPrim_Beg = ScreenPositionToMusicTime(PositionMarkPrim_Beg_Screen, true);
                GraphUtil.SetVertexValue(VisualizedVertex, loopBegMeta, positionMarkPrim_Beg);

                UpdatePositionMark();
            }
        }

        double positionMarkPrim_End_Screen;

        public double PositionMarkPrim_End_Screen
        {
            get
            {
                if (positionMarkPrim_End_Screen == -1 || HorizontalAD != null)
                    positionMarkPrim_End_Screen = MusicTimeToScreenPosition(positionMarkPrim_End, true);

                return positionMarkPrim_End_Screen;
            }
            set
            {
                positionMarkPrim_End = ScreenPositionToMusicTime(value, true);
                GraphUtil.SetVertexValue(VisualizedVertex, loopEndMeta, positionMarkPrim_End);

                positionMarkPrim_End_Screen = MusicTimeToScreenPosition(positionMarkPrim_End, true);

                UpdatePositionMark();
            }
        }

        protected int positionMarkPrim_End;

        public int PositionMarkPrim_End
        {
            get
            {
                return positionMarkPrim_End;
            }

            set
            {
                positionMarkPrim_End_Screen = MusicTimeToScreenPosition(value, true);

                positionMarkPrim_End = ScreenPositionToMusicTime(PositionMarkPrim_End_Screen, true);
                GraphUtil.SetVertexValue(VisualizedVertex, loopEndMeta, positionMarkPrim_End);

                UpdatePositionMark();
            }
        }


        protected virtual int FindLastPosition(IEnumerable<IEdge> edges)
        {
            return 0;
        }

        protected virtual IEnumerable<IEdge> GetEdgesForClipboard()
        {
            return Vertex.Get(false, "SelectedEdges:");
        }

        protected void Cut()
        {
            IEnumerable<IEdge> selectedEdges = GetEdgesForClipboard();

            m0.User.Clipboard.ClearClipboard();

            m0.User.Clipboard.PutToClipboard(selectedEdges, true);

            PositionMark = MusicTimeSnapCorrect_Up(FindLastPosition(selectedEdges));
        }

        protected void Copy()
        {
            IEnumerable<IEdge> selectedEdges = GetEdgesForClipboard();

            m0.User.Clipboard.ClearClipboard();

            m0.User.Clipboard.PutToClipboard(selectedEdges, false);

            PositionMark = MusicTimeSnapCorrect_Up(FindLastPosition(selectedEdges));
        }

        protected virtual void PasteEdgesFromClipboard(IEnumerable<IEdge> edges)
        {

        }

        protected void Paste()
        {
            IEnumerable<IEdge> edges = m0.User.Clipboard.GetFromClipboard();

            //VertexChangeOff = true;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            UnselectAllSelectedItems();

            PasteEdgesFromClipboard(edges);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            //VertexChangeOff = false;

            //VisualiserDraw();
        }

        protected void CutButton_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }

        protected void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        protected void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }
    }
}
