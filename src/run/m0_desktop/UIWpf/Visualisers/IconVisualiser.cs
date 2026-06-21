using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Commands;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Controls;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers
{
    public class IconVisualiser : Grid, IListVisualiser, ITypedEdge, IKeyboardHighlight, IOwnScrolling
    {
        private const double DefaultIconSize = 64;
        private const double ItemMargin = 6;

        private readonly ScrollViewer scrollViewer;
        private readonly WrapPanel itemsPanel;

        private readonly Dictionary<IEdge, IconVisualiserItem> displayedEdgeItems = new Dictionary<IEdge, IconVisualiserItem>();

        private IEdge keyboardHighlightedEdge;
        private bool isBeforeFirstKeyboardPosition;
        private bool isAfterLastKeyboardPosition;

        private IEdge pendingMouseDownSelectionEdge;
        private bool pendingMouseDownSelectionIsCtrl;
        private bool pendingWasInSelectionAtMouseDown;
        private bool suppressNextMouseUpSelection;

        private Point dndStartPoint;
        private bool hasButtonBeenDown;
        private bool isDragging;

        private ScrollViewer wrappingScrollViewer;

        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public bool SelectionProhibited { get; set; }

        public double Scale { get; set; }

        static string[] metaTriggeringUpdateVertex = new string[] { "FilterQuery" };
        public string[] MetaTriggeringUpdateVertex { get { return metaTriggeringUpdateVertex; } }

        static string[] metaTriggeringUpdateView = new string[] { "IconSize" };
        public string[] MetaTriggeringUpdateView { get { return metaTriggeringUpdateView; } }

        public IconVisualiser(IVertex baseEdgeVertex, IVertex parentVertex, bool isVolatile)
        {
            Scale = 1.0;
            Focusable = true;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Background = (Brush)FindResource("0BackgroundBrush");

            itemsPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Background = (Brush)FindResource("0BackgroundBrush")
            };

            scrollViewer = new ScrollViewer
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = itemsPanel,
                Background = (Brush)FindResource("0BackgroundBrush")
            };

            Children.Add(scrollViewer);

            Loaded += IconVisualiser_Loaded;
            Unloaded += IconVisualiser_Unloaded;
            SizeChanged += IconVisualiser_SizeChanged;
            scrollViewer.SizeChanged += IconVisualiser_SizeChanged;

            IVertex iconMetaVertex = MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Icon");

            new ListVisualiserHelper(parentVertex,
                isVolatile,
                iconMetaVertex,
                this,
                "IconVisualiser",
                this,
                false,
                new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            PreviewKeyDown += OnPreviewKeyDown;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            PreviewMouseMove += OnPreviewMouseMoveForDnd;
            Drop += OnDropForDnd;
            MouseEnter += OnMouseEnterForDnd;
            AllowDrop = true;

            SetVertexDefaultValues();
            ScaleChange();
        }

        private void IconVisualiser_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshWrapLayout();
        }

        private void IconVisualiser_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachFromWrappingScrollViewer();
        }

        private void IconVisualiser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsPanelWidth();
        }

        private void WrappingScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsPanelWidth();
        }

        private void AttachToWrappingScrollViewer()
        {
            DetachFromWrappingScrollViewer();

            DependencyObject current = Parent;

            while (current != null)
            {
                if (current is ScrollViewer ancestorScrollViewer && ancestorScrollViewer != scrollViewer)
                {
                    wrappingScrollViewer = ancestorScrollViewer;
                    wrappingScrollViewer.SizeChanged += WrappingScrollViewer_SizeChanged;
                    return;
                }

                DependencyObject visualParent = VisualTreeHelper.GetParent(current);

                if (visualParent != null)
                    current = visualParent;
                else
                    current = LogicalTreeHelper.GetParent(current);
            }
        }

        private void DetachFromWrappingScrollViewer()
        {
            if (wrappingScrollViewer == null)
                return;

            wrappingScrollViewer.SizeChanged -= WrappingScrollViewer_SizeChanged;
            wrappingScrollViewer = null;
        }

        private void UpdateItemsPanelWidth()
        {
            SyncHostWidthToWrappingViewport();

            double availableWidth = GetItemsPanelWrapWidth();

            if (availableWidth <= 0)
                return;

            itemsPanel.Width = availableWidth;
            itemsPanel.InvalidateMeasure();
            itemsPanel.InvalidateArrange();
            scrollViewer.InvalidateMeasure();
            scrollViewer.InvalidateArrange();
        }

        public void RefreshWrapLayout()
        {
            AttachToWrappingScrollViewer();
            UpdateItemsPanelWidth();
        }

        private void SyncHostWidthToWrappingViewport()
        {
            if (wrappingScrollViewer == null)
                return;

            double viewportWidth = wrappingScrollViewer.ViewportWidth;

            if (double.IsNaN(viewportWidth) || viewportWidth <= 0)
                viewportWidth = wrappingScrollViewer.ActualWidth;

            if (viewportWidth <= 0)
                return;

            double hostWidth = Math.Max(100, viewportWidth - 4);
            MaxWidth = hostWidth;
            Width = double.NaN;
        }

        private double GetItemsPanelWrapWidth()
        {
            scrollViewer.UpdateLayout();

            double innerViewportWidth = scrollViewer.ViewportWidth;

            if (!double.IsNaN(innerViewportWidth) && innerViewportWidth > 0)
                return innerViewportWidth;

            double actualWidth = ActualWidth;

            if (actualWidth > 0)
                return actualWidth;

            return GetAvailableWrapWidth();
        }

        private double GetAvailableWrapWidth()
        {
            if (wrappingScrollViewer != null)
            {
                double viewportWidth = wrappingScrollViewer.ViewportWidth;

                if (double.IsNaN(viewportWidth) || viewportWidth <= 0)
                    viewportWidth = wrappingScrollViewer.ActualWidth;

                if (viewportWidth > 0)
                    return viewportWidth;
            }

            double actualWidth = ActualWidth;

            if (actualWidth > 0)
                return actualWidth;

            if (scrollViewer.ViewportWidth > 0)
                return scrollViewer.ViewportWidth;

            return scrollViewer.ActualWidth;
        }

        public IconVisualiser(IEdge edge)
        {
            Edge = edge;
            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }

        public int CurrentHighlightPosition
        {
            get
            {
                List<IEdge> edges = GetKeyboardHighlightEdges();

                if (keyboardHighlightedEdge == null)
                    return -1;

                return edges.IndexOf(keyboardHighlightedEdge);
            }
        }

        public bool IsBeforeFirstPosition { get { return isBeforeFirstKeyboardPosition; } }

        public bool IsAfterLastPosition { get { return isAfterLastKeyboardPosition; } }

        public bool IsFirstPosition
        {
            get { return CurrentHighlightPosition == 0 && !isBeforeFirstKeyboardPosition && !isAfterLastKeyboardPosition; }
            set { if (value) SetKeyboardHighlightToFirst(); }
        }

        public bool IsLastPosition
        {
            get
            {
                List<IEdge> edges = GetKeyboardHighlightEdges();
                return edges.Count > 0
                    && CurrentHighlightPosition == edges.Count - 1
                    && !isBeforeFirstKeyboardPosition
                    && !isAfterLastKeyboardPosition;
            }
            set { if (value) SetKeyboardHighlightToLast(); }
        }

        public bool CanGoBeforeFirstPosition { get { return true; } }

        public bool CanGoAfterLastPosition { get { return true; } }

        public bool HasKeyboardHighlightItems { get { return GetKeyboardHighlightEdges().Count > 0; } }

        public bool IsVertexCommanderKeyboardHighlightEnabled { get; set; }

        public IEdge KeyboardHighlightedEdge { get { return keyboardHighlightedEdge; } }

        public event EventHandler KeyboardHighlightActivated;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
            BaseEdgeToUpdated();
        }

        public void ViewAttributesUpdated()
        {
            ApplyIconSizeToAllItems();
            UpdateItemsPanelWidth();
            itemsPanel.InvalidateMeasure();
            itemsPanel.InvalidateArrange();
            scrollViewer.InvalidateMeasure();
            scrollViewer.InvalidateArrange();
        }

        public void ScaleChange()
        {
            double scale = ((double)(GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:")) ?? 100)) / 100.0;
            Scale = scale;

            if (scale != 1.0)
                LayoutTransform = new ScaleTransform(scale, scale);
            else
                LayoutTransform = null;
        }

        public void BaseEdgeToUpdated()
        {
            UnselectAllSelectedEdges();

            IVertex baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");
            IVertex meta = Vertex.Get(false, @"BaseEdge:\To:\$Is:");

            if (baseEdgeTo == null)
                return;

            itemsPanel.Children.Clear();
            displayedEdgeItems.Clear();

            double iconSize = GetIconSize();

            IList<IEdge> edgesToDisplay = GetEdgesToDisplay(baseEdgeTo, meta);

            foreach (IEdge edge in edgesToDisplay)
            {
                if (edge == null)
                    continue;

                if (!VisualiserUtil.FilterEdge(edge, Vertex))
                    continue;

                AddEdge(edge, iconSize);
            }

            SelectedVerticesUpdated();
            RefreshKeyboardHighlightAfterItemsChanged();
            UpdateItemsPanelWidth();
            itemsPanel.InvalidateMeasure();
            itemsPanel.InvalidateArrange();
        }

        public void UnselectAllSelectedEdges()
        {
            Interaction.BeginInteractionWithGraph();

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");
            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);

            Interaction.EndInteractionWithGraph();

            UnselectAllItems();
        }

        public void SelectedVerticesUpdated()
        {
            UnselectAllItems();

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            if (selectedEdges == null)
                return;

            foreach (IEdge selectedEdgeVertexEdge in selectedEdges.GetAll(false, @"{$Is:Edge}"))
            {
                IEdge selectedEdge = EdgeHelper.GetIEdgeByEdgeVertex(selectedEdgeVertexEdge.To);

                if (selectedEdge != null && displayedEdgeItems.ContainsKey(selectedEdge))
                    displayedEdgeItems[selectedEdge].SetSelected(true);
            }

            if (keyboardHighlightedEdge != null && displayedEdgeItems.ContainsKey(keyboardHighlightedEdge)
                && IsVertexCommanderKeyboardHighlightEnabled)
                displayedEdgeItems[keyboardHighlightedEdge].SetKeyboardHighlighted(true);

            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
        }

        public void ClearKeyboardHighlight()
        {
            if (keyboardHighlightedEdge != null && displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
                displayedEdgeItems[keyboardHighlightedEdge].SetKeyboardHighlighted(false);

            keyboardHighlightedEdge = null;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            List<IEdge> edges = GetKeyboardHighlightEdges();
            int currentIndex = CurrentHighlightPosition;

            if (edges.Count == 0)
            {
                if (positionDelta < 0)
                    GoBeforeFirstKeyboardHighlightPosition();
                else
                    GoAfterLastKeyboardHighlightPosition();

                return;
            }

            if (currentIndex < 0)
                currentIndex = positionDelta < 0 ? edges.Count : -1;

            int newIndex = currentIndex + positionDelta;

            if (newIndex < 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else if (newIndex >= edges.Count)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightEdge(edges[newIndex]);
        }

        public void MoveKeyboardHighlight(KeyboardHighlightMoveDirection direction)
        {
            bool moved = MoveKeyboardHighlightDirectional(direction);

            if (moved
                || KeyboardHighlightActivated == null
                || (direction != KeyboardHighlightMoveDirection.Up
                    && direction != KeyboardHighlightMoveDirection.Down))
                return;

            if (direction == KeyboardHighlightMoveDirection.Up)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                GoAfterLastKeyboardHighlightPosition();
        }

        public void ToggleKeyboardHighlightedEdgeSelection()
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled || SelectionProhibited || keyboardHighlightedEdge == null)
                return;

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");
            IEdge selectedEdgeVertexEdge = EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, keyboardHighlightedEdge);

            Interaction.BeginInteractionWithGraph();

            if (selectedEdgeVertexEdge != null)
            {
                selectedEdges.DeleteEdge(selectedEdgeVertexEdge);

                if (displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
                    displayedEdgeItems[keyboardHighlightedEdge].SetSelected(false);
            }
            else
            {
                IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

                if (baseVertex != null)
                {
                    EdgeHelper.AddEdgeVertex(selectedEdges, baseVertex, keyboardHighlightedEdge.Meta, keyboardHighlightedEdge.To);

                    if (displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
                        displayedEdgeItems[keyboardHighlightedEdge].SetSelected(true);
                }
            }

            Interaction.EndInteractionWithGraph();

            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            VisualiserHelper.Dispose();
        }

        public IVertex GetEdgeByPoint(Point point)
        {
            foreach (KeyValuePair<IEdge, IconVisualiserItem> pair in displayedEdgeItems)
            {
                IconVisualiserItem item = pair.Value;

                if (item == null || pair.Key == null)
                    continue;

                Point pointInItem = TranslatePoint(point, item);

                if (pointInItem.X < 0 || pointInItem.Y < 0
                    || pointInItem.X > item.ActualWidth || pointInItem.Y > item.ActualHeight)
                    continue;

                IVertex edgeVertex = MinusZero.Instance.CreateTempVertex();
                EdgeHelper.AddEdgeVertexEdges(edgeVertex, pair.Key);
                return edgeVertex;
            }

            if (GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                return Vertex.Get(false, "BaseEdge:");

            return null;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex edge)
        {
            throw new NotImplementedException();
        }

        private bool isDisposed;

        private IList<IEdge> GetEdgesToDisplay(IVertex baseEdgeTo, IVertex meta)
        {
            IList<IEdge> schemaEdges = GetSchemaMatchedEdges(baseEdgeTo, meta);

            if (schemaEdges.Count > 0)
                return schemaEdges;

            return GetDirectOutEdges(baseEdgeTo);
        }

        private IList<IEdge> GetSchemaMatchedEdges(IVertex baseEdgeTo, IVertex meta)
        {
            IList<IEdge> result = new List<IEdge>();

            if (meta == null)
                return result;

            foreach (IEdge definitionEdge in VertexOperations.GetChildEdges(meta))
            {
                object definitionMetaValue = definitionEdge.To?.Value;
                IEdge edge = GraphUtil.GetQueryOutFirstEdge(baseEdgeTo, definitionMetaValue, null);

                if (edge == null)
                    continue;

                result.Add(edge);
            }

            return result;
        }

        private IList<IEdge> GetDirectOutEdges(IVertex baseEdgeTo)
        {
            IList<IEdge> result = new List<IEdge>();
            IVertex filterQueryVertex = Vertex.Get(false, @"FilterQuery:");

            if (filterQueryVertex != null && filterQueryVertex.Value != null)
            {
                IVertex filteredData = VertexOperations.DoFilter(baseEdgeTo, filterQueryVertex);

                if (filteredData == null)
                    return result;

                foreach (IEdge edge in filteredData)
                    result.Add(edge);

                return result;
            }

            foreach (IEdge edge in baseEdgeTo)
                result.Add(edge);

            return result;
        }

        private void SetVertexDefaultValues()
        {
            IVertex iconSizeVertex = Vertex.Get(false, "IconSize:");
            IVertex scaleVertex = Vertex.Get(false, "Scale:");

            if (iconSizeVertex != null)
                iconSizeVertex.Value = (int)DefaultIconSize;

            if (scaleVertex != null)
                scaleVertex.Value = 100;
        }

        private void ApplyIconSizeToAllItems()
        {
            double iconSize = GetIconSize();

            foreach (IconVisualiserItem item in displayedEdgeItems.Values)
                item.UpdateIconSize(iconSize);
        }

        private double GetIconSize()
        {
            int? iconSize = GraphUtil.GetIntegerValue(Vertex.Get(false, "IconSize:"));

            if (iconSize == null || iconSize <= 0)
                return DefaultIconSize;

            return iconSize.Value;
        }

        private void AddEdge(IEdge edge, double iconSize)
        {
            IconVisualiserItem item = new IconVisualiserItem();
            item.Margin = new Thickness(ItemMargin);
            item.Initialize(edge, iconSize);

            item.MouseLeftButtonDown += Item_MouseLeftButtonDown;

            displayedEdgeItems.Add(edge, item);
            itemsPanel.Children.Add(item);
        }

        private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IconVisualiserItem item = sender as IconVisualiserItem;

            if (item == null || item.BaseEdge == null)
                return;

            if (e.ClickCount == 2)
            {
                if (!IsVertexCommanderKeyboardHighlightEnabled)
                    return;

                SetKeyboardHighlightEdge(item.BaseEdge);
                ActivateKeyboardHighlightedItem();
                e.Handled = true;
                return;
            }

            if (SelectionProhibited)
                return;

            pendingMouseDownSelectionEdge = item.BaseEdge;
            pendingMouseDownSelectionIsCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            pendingWasInSelectionAtMouseDown = SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(
                Vertex,
                pendingMouseDownSelectionEdge);
            suppressNextMouseUpSelection = false;
            e.Handled = true;
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IEdge clickedEdge = GetDisplayedEdgeAtPoint(e.GetPosition(this));

            if (clickedEdge == null || SelectionProhibited)
                return;

            SelectedEdgesInteractionHelper.ApplyForContextMenu(Vertex, clickedEdge);
            SelectedVerticesUpdated();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);

            if (KeyboardHighlightActivated == null)
            {
                Focus();
                UpdateLayout();
            }

            hasButtonBeenDown = true;
            isDragging = false;
            ClearPendingMouseDown();
            suppressNextMouseUpSelection = false;
            MinusZero.Instance.IsGUIDragging = false;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IEdge edgeAtUp = GetDisplayedEdgeAtPoint(e.GetPosition(this));

            if (!suppressNextMouseUpSelection && pendingMouseDownSelectionEdge != null)
            {
                if (edgeAtUp == pendingMouseDownSelectionEdge)
                    TryApplyPendingMouseClick();
                else
                    ClearPendingMouseDown();
            }
            else
                ClearPendingMouseDown();

            suppressNextMouseUpSelection = false;
            hasButtonBeenDown = false;
        }

        private void TryApplyPendingMouseClick()
        {
            if (SelectionProhibited || pendingMouseDownSelectionEdge == null)
            {
                ClearPendingMouseDown();
                return;
            }

            PendingEdgeMouseGesture pendingGesture = new PendingEdgeMouseGesture
            {
                ClickedEdge = pendingMouseDownSelectionEdge,
                IsCtrl = pendingMouseDownSelectionIsCtrl,
                WasInSelectionAtMouseDown = pendingWasInSelectionAtMouseDown
            };

            SelectedEdgesInteractionHelper.ApplyForClick(Vertex, pendingGesture);
            SelectedVerticesUpdated();
            ClearPendingMouseDown();
        }

        private void ClearPendingMouseDown()
        {
            pendingMouseDownSelectionEdge = null;
            pendingMouseDownSelectionIsCtrl = false;
            pendingWasInSelectionAtMouseDown = false;
        }

        private IEdge GetDisplayedEdgeAtPoint(Point point)
        {
            foreach (KeyValuePair<IEdge, IconVisualiserItem> pair in displayedEdgeItems)
            {
                IconVisualiserItem item = pair.Value;

                if (item == null || pair.Key == null)
                    continue;

                Point pointInItem = TranslatePoint(point, item);

                if (pointInItem.X < 0 || pointInItem.Y < 0
                    || pointInItem.X > item.ActualWidth || pointInItem.Y > item.ActualHeight)
                    continue;

                return pair.Key;
            }

            return null;
        }

        private void OnPreviewMouseMoveForDnd(object sender, MouseEventArgs e)
        {
            if (!hasButtonBeenDown || isDragging)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (WpfUtil.IsMouseOverScrollbar(sender, dndStartPoint))
                return;

            Point mousePosition = e.GetPosition(this);
            Vector controlDiff = dndStartPoint - mousePosition;

            if (Math.Abs(controlDiff.X) <= Dnd.MinimumHorizontalDragDistance
                && Math.Abs(controlDiff.Y) <= Dnd.MinimumVerticalDragDistance)
                return;

            isDragging = true;
            suppressNextMouseUpSelection = true;

            IVertex dndVertex = TryPrepareDragAndBuildDndVertex();

            if (dndVertex != null && dndVertex.Count() > 0)
            {
                dndVertex.AddExternalReference();

                DataObject dragData = new DataObject("Vertex", dndVertex);
                dragData.SetData("DragSource", this);

                Dnd.DoDragDrop(this, dragData);

                e.Handled = true;
            }

            isDragging = false;
            hasButtonBeenDown = false;
        }

        private IVertex TryPrepareDragAndBuildDndVertex()
        {
            IEdge clickedEdge = pendingMouseDownSelectionEdge;
            bool isCtrl = pendingMouseDownSelectionIsCtrl;
            bool wasInSelectionAtMouseDown = pendingWasInSelectionAtMouseDown;
            IVertex fallbackEdgeVertex = GetEdgeByPoint(dndStartPoint);

            if (clickedEdge == null)
            {
                if (fallbackEdgeVertex == null)
                    return null;

                clickedEdge = EdgeHelper.GetIEdgeByEdgeVertex(fallbackEdgeVertex);

                if (clickedEdge == null)
                {
                    return SelectedEdgesInteractionHelper.BuildDndVertexFromSelectedEdges(
                        Vertex,
                        fallbackEdgeVertex);
                }

                isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                wasInSelectionAtMouseDown = SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, clickedEdge);
            }

            PendingEdgeMouseGesture pendingGesture = new PendingEdgeMouseGesture
            {
                ClickedEdge = clickedEdge,
                IsCtrl = isCtrl,
                WasInSelectionAtMouseDown = wasInSelectionAtMouseDown
            };

            SelectedEdgesInteractionHelper.ApplyForDrag(Vertex, pendingGesture);
            SelectedVerticesUpdated();

            IVertex dndVertex = SelectedEdgesInteractionHelper.BuildDndVertexFromSelectedEdges(
                Vertex,
                fallbackEdgeVertex);

            ClearPendingMouseDown();

            return dndVertex;
        }

        private void OnDropForDnd(object sender, DragEventArgs e)
        {
            IVertex edgeVertex = GetEdgeByPoint(e.GetPosition(this));

            if (edgeVertex == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                edgeVertex = Vertex.Get(false, "BaseEdge:");

            if (edgeVertex != null)
                Dnd.DoDrop(null, edgeVertex.Get(false, "To:"), e);
        }

        private void OnMouseEnterForDnd(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Left && e.Key != Key.Right
                && e.Key != Key.Space && e.Key != Key.Enter)
                return;

            e.Handled = true;

            if (keyboardHighlightedEdge == null && e.Key != Key.Enter && e.Key != Key.Space)
            {
                if (GetKeyboardHighlightEdges().Count > 0)
                    SetKeyboardHighlightToFirst();

                return;
            }

            if (e.Key == Key.Up)
                MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Up);
            else if (e.Key == Key.Down)
                MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Down);
            else if (e.Key == Key.Left)
                MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Left);
            else if (e.Key == Key.Right)
                MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Right);
            else if (e.Key == Key.Space)
                ToggleKeyboardHighlightedEdgeSelection();
            else
                ActivateKeyboardHighlightedItem();
        }

        private void ActivateKeyboardHighlightedItem()
        {
            if (keyboardHighlightedEdge == null)
                return;

            IVertex targetVertex = keyboardHighlightedEdge.To;

            if (targetVertex == null)
                return;

            if (KeyboardHighlightActivated != null)
            {
                RaiseKeyboardHighlightActivated();
                return;
            }

            GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", targetVertex);

            IVertex updatedBaseTo = Vertex.Get(false, @"BaseEdge:\To:");

            if (updatedBaseTo == targetVertex)
                BaseEdgeToUpdated();
        }

        private void RaiseKeyboardHighlightActivated()
        {
            if (KeyboardHighlightedEdge == null)
                return;

            if (KeyboardHighlightActivated != null)
                KeyboardHighlightActivated(this, EventArgs.Empty);

            if (KeyboardHighlightEnterPressed != null)
                KeyboardHighlightEnterPressed(this, EventArgs.Empty);
        }

        private void SetKeyboardHighlightToFirst()
        {
            List<IEdge> edges = GetKeyboardHighlightEdges();

            if (edges.Count == 0)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightEdge(edges[0]);
        }

        private void SetKeyboardHighlightToLast()
        {
            List<IEdge> edges = GetKeyboardHighlightEdges();

            if (edges.Count == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightEdge(edges[edges.Count - 1]);
        }

        private void SetKeyboardHighlightEdge(IEdge edge)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            ClearKeyboardHighlight();

            if (edge == null || !displayedEdgeItems.ContainsKey(edge))
                return;

            keyboardHighlightedEdge = edge;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;

            IconVisualiserItem item = displayedEdgeItems[edge];
            item.SetKeyboardHighlighted(true);

            item.BringIntoView();
            UpdateLayout();
        }

        private void GoBeforeFirstKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();
            isBeforeFirstKeyboardPosition = true;

            if (GoneBeforeFirstPosition != null)
                GoneBeforeFirstPosition(this, EventArgs.Empty);
        }

        private void GoAfterLastKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();
            isAfterLastKeyboardPosition = true;

            if (GoneAfterLastPosition != null)
                GoneAfterLastPosition(this, EventArgs.Empty);
        }

        private List<IEdge> GetKeyboardHighlightEdges()
        {
            return displayedEdgeItems
                .Where(pair => pair.Key != null && pair.Value != null)
                .OrderBy(pair => GetItemCenter(pair.Value).Y)
                .ThenBy(pair => GetItemCenter(pair.Value).X)
                .Select(pair => pair.Key)
                .ToList();
        }

        private bool MoveKeyboardHighlightDirectional(KeyboardHighlightMoveDirection direction)
        {
            if (keyboardHighlightedEdge == null)
            {
                SetKeyboardHighlightToFirst();
                return true;
            }

            if (!displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
                return false;

            Point current = GetItemCenter(displayedEdgeItems[keyboardHighlightedEdge]);
            IEdge bestEdge = null;
            double bestScore = double.MaxValue;

            foreach (KeyValuePair<IEdge, IconVisualiserItem> pair in displayedEdgeItems)
            {
                if (pair.Key == null || pair.Key == keyboardHighlightedEdge || pair.Value == null)
                    continue;

                Point candidate = GetItemCenter(pair.Value);
                double dx = candidate.X - current.X;
                double dy = candidate.Y - current.Y;

                if (!IsCandidateInDirection(direction, dx, dy))
                    continue;

                double score = (dx * dx) + (dy * dy);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestEdge = pair.Key;
                }
            }

            if (bestEdge != null)
            {
                SetKeyboardHighlightEdge(bestEdge);
                return true;
            }

            return false;
        }

        private static bool IsCandidateInDirection(KeyboardHighlightMoveDirection direction, double dx, double dy)
        {
            if (direction == KeyboardHighlightMoveDirection.Left)
                return dx < 0 && Math.Abs(dx) >= Math.Abs(dy) * 0.35;

            if (direction == KeyboardHighlightMoveDirection.Right)
                return dx > 0 && Math.Abs(dx) >= Math.Abs(dy) * 0.35;

            if (direction == KeyboardHighlightMoveDirection.Up)
                return dy < 0 && Math.Abs(dy) >= Math.Abs(dx) * 0.35;

            return dy > 0 && Math.Abs(dy) >= Math.Abs(dx) * 0.35;
        }

        private Point GetItemCenter(IconVisualiserItem item)
        {
            item.UpdateLayout();

            Point topLeft = item.TransformToAncestor(itemsPanel).Transform(new Point(0, 0));

            return new Point(topLeft.X + item.ActualWidth / 2, topLeft.Y + item.ActualHeight / 2);
        }

        private void RefreshKeyboardHighlightAfterItemsChanged()
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled || keyboardHighlightedEdge == null)
                return;

            if (displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
            {
                displayedEdgeItems[keyboardHighlightedEdge].SetKeyboardHighlighted(true);
                return;
            }

            ClearKeyboardHighlight();
        }

        private void UnselectAllItems()
        {
            foreach (IconVisualiserItem item in displayedEdgeItems.Values)
                item.SetSelected(false);
        }
    }
}
