using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
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
    public class IconVisualiser : WrapPanel, IListVisualiser, ITypedEdge, IKeyboardHighlight
    {
        private const double DefaultIconSize = 64;
        private const double ItemMargin = 6;

        private readonly Dictionary<IEdge, IconVisualiserItem> displayedEdgeItems = new Dictionary<IEdge, IconVisualiserItem>();

        private IEdge keyboardHighlightedEdge;
        private bool isBeforeFirstKeyboardPosition;
        private bool isAfterLastKeyboardPosition;

        private IEdge pendingMouseDownSelectionEdge;
        private bool pendingMouseDownSelectionIsCtrl;
        private bool suppressNextMouseUpSelection;

        private IVertex tempSelectedVertices;

        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public bool SelectionProphibited { get; set; }

        public double Scale { get; set; }

        static string[] metaTriggeringUpdateVertex = new string[0];
        public string[] MetaTriggeringUpdateVertex { get { return metaTriggeringUpdateVertex; } }

        static string[] metaTriggeringUpdateView = new string[] { "IconSize" };
        public string[] MetaTriggeringUpdateView { get { return metaTriggeringUpdateView; } }

        public IconVisualiser(IVertex baseEdgeVertex, IVertex parentVertex, bool isVolatile)
        {
            Scale = 1.0;
            Background = (Brush)FindResource("0BackgroundBrush");
            Orientation = Orientation.Horizontal;
            Focusable = true;

            new ListVisualiserHelper(parentVertex,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Icon"),
                this,
                "IconVisualiser",
                this,
                false,
                new List<string> { @"BaseEdge:\To:" },
                "Visualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            ((ListVisualiserHelper)VisualiserHelper).CustomVertexChangeEvent += CustomVertexChange;

            PreviewKeyDown += OnPreviewKeyDown;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;

            SetVertexDefaultValues();
            ScaleChange();
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

        public void OnLoad(object sender, RoutedEventArgs e) { }

        public void ViewAttributesUpdated()
        {
            BaseEdgeToUpdated();
        }

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100.0;
            Scale = scale;

            foreach (IconVisualiserItem item in displayedEdgeItems.Values)
                item.ApplyScaleTransform(scale);
        }

        public void BaseEdgeToUpdated()
        {
            IVertex baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");
            IVertex meta = Vertex.Get(false, @"BaseEdge:\To:\$Is:");

            if (baseEdgeTo == null || meta == null)
                return;

            Children.Clear();
            displayedEdgeItems.Clear();

            double iconSize = GetIconSize();
            double scale = Scale > 0 ? Scale : 1.0;

            foreach (IEdge definitionEdge in VertexOperations.GetChildEdges(meta))
            {
                IEdge edge = GraphUtil.GetQueryOutFirstEdge(baseEdgeTo, definitionEdge.To.Value, null);

                if (edge != null && VisualiserUtil.FilterEdge(edge, Vertex))
                    AddEdge(edge, iconSize, scale);
            }

            SelectedVerticesUpdated();
            RefreshKeyboardHighlightAfterItemsChanged();
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

            if (keyboardHighlightedEdge != null && displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
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
            List<IEdge> edges = GetKeyboardHighlightEdges();

            if (edges.Count == 0)
            {
                if (positionDelta < 0)
                    GoBeforeFirstKeyboardHighlightPosition();
                else
                    GoAfterLastKeyboardHighlightPosition();

                return;
            }

            int currentIndex = CurrentHighlightPosition;

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
            MoveKeyboardHighlightDirectional(direction);
        }

        public void ToggleKeyboardHighlightedEdgeSelection()
        {
            if (SelectionProphibited || keyboardHighlightedEdge == null)
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

        protected virtual INoInEdgeInOutVertexVertex CustomVertexChange(IExecution exe)
        {
            if (ExecutionFlowHelper.AllEventChildVisualiser(exe.Stack))
                return exe.Stack;

            BaseEdgeToUpdated();

            return exe.Stack;
        }

        private void SetVertexDefaultValues()
        {
            Vertex.Get(false, "IconSize:").Value = (int)DefaultIconSize;
            Vertex.Get(false, "Scale:").Value = 100;
        }

        private double GetIconSize()
        {
            int? iconSize = GraphUtil.GetIntegerValue(Vertex.Get(false, "IconSize:"));

            if (iconSize == null || iconSize <= 0)
                return DefaultIconSize;

            return iconSize.Value;
        }

        private void AddEdge(IEdge edge, double iconSize, double scale)
        {
            IconVisualiserItem item = new IconVisualiserItem();
            item.Margin = new Thickness(ItemMargin);
            item.Initialize(edge, iconSize);
            item.ApplyScaleTransform(scale);

            item.MouseLeftButtonDown += Item_MouseLeftButtonDown;

            displayedEdgeItems.Add(edge, item);
            Children.Add(item);
        }

        private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IconVisualiserItem item = sender as IconVisualiserItem;

            if (item == null || item.BaseEdge == null)
                return;

            if (e.ClickCount == 2)
            {
                SetKeyboardHighlightEdge(item.BaseEdge);
                ActivateKeyboardHighlightedItem();
                e.Handled = true;
                return;
            }

            if (SelectionProphibited)
                return;

            CopySelectedVerticesToTemp();

            pendingMouseDownSelectionEdge = item.BaseEdge;
            pendingMouseDownSelectionIsCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            suppressNextMouseUpSelection = false;
            e.Handled = true;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!suppressNextMouseUpSelection && pendingMouseDownSelectionEdge != null)
                ApplyPendingMouseSelection();

            pendingMouseDownSelectionEdge = null;
            suppressNextMouseUpSelection = false;
        }

        private void ApplyPendingMouseSelection()
        {
            if (pendingMouseDownSelectionEdge == null || !displayedEdgeItems.ContainsKey(pendingMouseDownSelectionEdge))
                return;

            IconVisualiserItem item = displayedEdgeItems[pendingMouseDownSelectionEdge];
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");
            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            Interaction.BeginInteractionWithGraph();

            if (pendingMouseDownSelectionIsCtrl)
            {
                IEdge selectedEdgeVertexEdge = EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, pendingMouseDownSelectionEdge);

                if (selectedEdgeVertexEdge != null)
                {
                    selectedEdges.DeleteEdge(selectedEdgeVertexEdge);
                    item.SetSelected(false);
                }
                else if (baseVertex != null)
                {
                    EdgeHelper.AddEdgeVertex(selectedEdges, baseVertex, pendingMouseDownSelectionEdge.Meta, pendingMouseDownSelectionEdge.To);
                    item.SetSelected(true);
                }
            }
            else
            {
                UnselectAllItems();
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);

                if (baseVertex != null)
                {
                    EdgeHelper.AddEdgeVertex(selectedEdges, baseVertex, pendingMouseDownSelectionEdge.Meta, pendingMouseDownSelectionEdge.To);
                    item.SetSelected(true);
                }
            }

            Interaction.EndInteractionWithGraph();

            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
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

            RestoreSelectedVertices();

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
            ClearKeyboardHighlight();

            if (edge == null || !displayedEdgeItems.ContainsKey(edge))
                return;

            keyboardHighlightedEdge = edge;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;

            IconVisualiserItem item = displayedEdgeItems[edge];
            item.SetKeyboardHighlighted(true);
            item.BringIntoView();
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

        private void MoveKeyboardHighlightDirectional(KeyboardHighlightMoveDirection direction)
        {
            if (keyboardHighlightedEdge == null)
            {
                SetKeyboardHighlightToFirst();
                return;
            }

            if (!displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
                return;

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
                SetKeyboardHighlightEdge(bestEdge);
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

            Point topLeft = item.TransformToAncestor(this).Transform(new Point(0, 0));

            return new Point(topLeft.X + item.ActualWidth / 2, topLeft.Y + item.ActualHeight / 2);
        }

        private void RefreshKeyboardHighlightAfterItemsChanged()
        {
            if (keyboardHighlightedEdge == null)
                return;

            if (!displayedEdgeItems.ContainsKey(keyboardHighlightedEdge))
            {
                List<IEdge> edges = GetKeyboardHighlightEdges();

                if (edges.Count == 0)
                    ClearKeyboardHighlight();
                else
                    SetKeyboardHighlightEdge(edges[Math.Min(CurrentHighlightPosition, edges.Count - 1)]);
            }
            else
            {
                displayedEdgeItems[keyboardHighlightedEdge].SetKeyboardHighlighted(true);
            }
        }

        private void UnselectAllItems()
        {
            foreach (IconVisualiserItem item in displayedEdgeItems.Values)
                item.SetSelected(false);
        }

        private void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();
            tempSelectedVertices.AddExternalReference();
            GraphUtil.CopyShallow(Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"), tempSelectedVertices);
        }

        private void RestoreSelectedVertices()
        {
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            if (tempSelectedVertices != null)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                GraphUtil.CopyShallow(tempSelectedVertices, selectedEdges);
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(tempSelectedVertices);
                tempSelectedVertices.RemoveExternalReference();
                tempSelectedVertices = null;
            }
        }
    }
}
