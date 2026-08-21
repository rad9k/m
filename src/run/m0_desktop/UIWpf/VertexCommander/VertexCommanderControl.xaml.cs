using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Commands;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace m0.UIWpf.VertexCommander
{
    /// <summary>
    /// Interaction logic for VertexCommanderControl.xaml
    /// </summary>
    public partial class VertexCommanderControl : UserControl
    {
        private const string InEdgesVisualiserQuery = @"System\Meta\Visualiser\Class:{$Inherits:ShowesInEdges,BaseEdgeTarget:Any}";
        private const string OutEdgesVisualiserQuery = @"System\Meta\Visualiser\Class:{$Inherits:ShowesOutEdges,BaseEdgeTarget:Any}";
        private const string DefaultInEdgesVisualiserClassQuery = @"System\Meta\Visualiser\InEdgesList";
        private const string DefaultOutEdgesVisualiserClassQuery = @"System\Meta\Visualiser\List";

        private static readonly IVertex BaseEdgeMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");

        private IVertex leftInEdgesVisuliserInstance;
        private IVertex rightInEdgesVisuliserInstance;
        private IVertex leftOutEdgesVisuliserInstance;
        private IVertex rightOutEdgesVisuliserInstance;
        private IKeyboardHighlight leftInEdgesKeyboardHighlight;
        private IKeyboardHighlight rightInEdgesKeyboardHighlight;
        private IKeyboardHighlight leftOutEdgesKeyboardHighlight;
        private IKeyboardHighlight rightOutEdgesKeyboardHighlight;

        private KeyboardHighlightPane currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
        private KeyboardHighlightSection currentKeyboardHighlightSection = KeyboardHighlightSection.IncomingEdges;

        private const int QueryHistoryMax = 10;
        private const int KeyboardHighlightPageStep = 10;

        private readonly ObservableCollection<QueryHistoryEntry> leftQueryHistory = new ObservableCollection<QueryHistoryEntry>();
        private readonly ObservableCollection<QueryHistoryEntry> rightQueryHistory = new ObservableCollection<QueryHistoryEntry>();
        private bool isApplyingHistorySelection;
        private System.DateTime leftQueryHistoryPopupClosed;
        private System.DateTime rightQueryHistoryPopupClosed;
        private bool initialKeyboardHighlightSet;

        private KeyboardHighlightPane? liveSyncMasterPane;
        private bool isLiveSyncing;
        private System.Windows.Threading.DispatcherTimer liveSyncTimer;

        private class QueryHistoryEntry
        {
            public string QueryText { get; set; }

            public IVertex BaseEdgeTo { get; set; }
        }

        private enum KeyboardHighlightPane
        {
            Left,
            Right
        }

        private enum KeyboardHighlightSection
        {
            IncomingEdges,
            OutEdges
        }

        public IVertex LeftBaseEdge { get; private set; }

        public IVertex RightBaseEdge { get; private set; }

        public CodeControl LeftQueryStringCodeControl { get; private set; }

        public CodeControl RightQueryStringCodeControl { get; private set; }

        public IVertex LeftInEdgesVisuliserClass { get; private set; }

        public IVertex RightInEdgesVisuliserClass { get; private set; }

        public IVertex LeftOutEdgesVisuliserClass { get; private set; }

        public IVertex RightOutEdgesVisuliserClass { get; private set; }

        public IVertex LeftInEdgesVisuliserInstance { get { return leftInEdgesVisuliserInstance; } }

        public IVertex RightInEdgesVisuliserInstance { get { return rightInEdgesVisuliserInstance; } }

        public IVertex LeftOutEdgesVisuliserInstance { get { return leftOutEdgesVisuliserInstance; } }

        public IVertex RightOutEdgesVisuliserInstance { get { return rightOutEdgesVisuliserInstance; } }

        public override string ToString()
        {
            return "[][]";
        }

        public VertexCommanderControl()
        {
            InitializeComponent();
            SetStaticControls();
        }

        public VertexCommanderControl(IVertex leftBaseEdge, IVertex rightBaseEdge)
        {
            LeftBaseEdge = leftBaseEdge;
            RightBaseEdge = rightBaseEdge;

            InitializeComponent();
            SetStaticControls();
        }

        private void SetStaticControls()
        {
            Focusable = true;
            PreviewKeyDown += VertexCommanderControl_PreviewKeyDown;
            SizeChanged += VertexCommanderControl_SizeChanged;

            LeftQueryStringCodeControl = CreateQueryStringCodeControl();
            RightQueryStringCodeControl = CreateQueryStringCodeControl();

            LeftQueryStringCodeControlHost.Content = LeftQueryStringCodeControl;
            RightQueryStringCodeControlHost.Content = RightQueryStringCodeControl;
            SetupCodeControlDropTargets();

            SetCodeControlQueryText();
            Loaded += VertexCommanderControl_Loaded;
            AddCodeControlEventHandlers();
            SetupQueryHistoryControls();
            SetDefaultVisualiserClasses();
            SetVisualiserSelectors();
            AddVisualiserSelectorEventHandlers();
            CreateAllVisualiserInstances();
            UpdateBottomCommandButtonsFontSize();
        }

        private void VertexCommanderControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBottomCommandButtonsFontSize();
            RefreshIconVisualiserLayouts();
        }

        private void RefreshIconVisualiserLayouts()
        {
            RefreshIconVisualiserLayout(LeftInEdgesVisualiserHost, LeftInEdgesSectionScrollViewer);
            RefreshIconVisualiserLayout(RightInEdgesVisualiserHost, RightInEdgesSectionScrollViewer);
            RefreshIconVisualiserLayout(LeftOutEdgesVisualiserHost, LeftOutEdgesSectionScrollViewer);
            RefreshIconVisualiserLayout(RightOutEdgesVisualiserHost, RightOutEdgesSectionScrollViewer);
        }

        private static void RefreshIconVisualiserLayout(ContentControl visualiserHost, ScrollViewer sectionScrollViewer)
        {
            if (visualiserHost?.Content is IconVisualiser)
                SetViewportSizeIfNeeded(visualiserHost.Content, sectionScrollViewer);
        }

        private void UpdateBottomCommandButtonsFontSize()
        {
            double buttonWidth = ActualWidth / 10.0;
            double fontSize = Math.Max(7.0, Math.Min(10.0, buttonWidth / 11.0));

            SetBottomCommandButtonFontSize(ViewButton, fontSize);
            SetBottomCommandButtonFontSize(EditButton, fontSize);
            SetBottomCommandButtonFontSize(CopyButton, fontSize);
            SetBottomCommandButtonFontSize(MoveButton, fontSize);
            SetBottomCommandButtonFontSize(NewVertexButton, fontSize);
            SetBottomCommandButtonFontSize(DeleteButton, fontSize);
            SetBottomCommandButtonFontSize(NewEdgeButton, fontSize);
            SetBottomCommandButtonFontSize(ReplaceButton, fontSize);
            SetBottomCommandButtonFontSize(MasterToDetailButton, fontSize);
            SetBottomCommandButtonFontSize(DetailToMasterButton, fontSize);
        }

        private static void SetBottomCommandButtonFontSize(Button button, double fontSize)
        {
            if (button != null)
                button.FontSize = fontSize;
        }

        private static CodeControl CreateQueryStringCodeControl()
        {
            IVertex codeControlVertex = MinusZero.Instance.CreateTempVertex();
            CodeControl codeControl = new CodeControl(codeControlVertex);
            codeControl.HandleEnterAsSubmit = true;
            codeControl.HandleEscapeAsParse = false;
            codeControl.Height = 20;
            codeControl.editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

            return codeControl;
        }

        private void SetupCodeControlDropTargets()
        {
            SetupCodeControlDropTarget(LeftQueryStringCodeControlHost, LeftQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(LeftQueryStringCodeControl, LeftQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(LeftQueryStringCodeControl.editor, LeftQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(LeftQueryStringCodeControl.editor.TextArea, LeftQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(RightQueryStringCodeControlHost, RightQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(RightQueryStringCodeControl, RightQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(RightQueryStringCodeControl.editor, RightQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(RightQueryStringCodeControl.editor.TextArea, RightQueryStringCodeControl_Drop);
        }

        private static void SetupCodeControlDropTarget(UIElement element, DragEventHandler dropHandler)
        {
            element.AllowDrop = true;
            element.AddHandler(UIElement.PreviewDragOverEvent, new DragEventHandler(CodeControl_PreviewDragOver), true);
            element.AddHandler(UIElement.PreviewDropEvent, dropHandler, true);
            element.Drop += dropHandler;
        }

        private static void CodeControl_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (GetDroppedEdge(e) == null)
                return;

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void LeftQueryStringCodeControl_Drop(object sender, DragEventArgs e)
        {
            HandleCodeControlDrop(KeyboardHighlightPane.Left, e);
        }

        private void RightQueryStringCodeControl_Drop(object sender, DragEventArgs e)
        {
            HandleCodeControlDrop(KeyboardHighlightPane.Right, e);
        }

        private void HandleCodeControlDrop(KeyboardHighlightPane pane, DragEventArgs e)
        {
            if (e.Handled)
                return;

            IEdge droppedEdge = GetDroppedEdge(e);

            if (droppedEdge == null)
                return;

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(droppedEdge);

            ApplyPaneBaseEdgeUpdate(pane, baseEdgeVertex);
            SetKeyboardHighlightAfterBaseEdgeChange(pane);
            RecordPaneQueryHistory(pane);

            object dragSource = e.Data.GetData("DragSource");

            if (dragSource is IHasSelectableEdges selectableEdges)
                selectableEdges.UnselectAllSelectedEdges();

            if (e.Data.GetData("Vertex") is IVertex dndVertex)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(dndVertex);
                dndVertex.RemoveExternalReference();
            }

            MinusZero.Instance.IsGUIDragging = false;
            e.Handled = true;
        }

        private static IEdge GetDroppedEdge(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Vertex"))
                return null;

            IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

            if (dndVertex == null)
                return null;

            IEdge firstEdgeVertexEdge = dndVertex.FirstOrDefault();

            if (firstEdgeVertexEdge == null || firstEdgeVertexEdge.To == null)
                return null;

            return EdgeHelper.CreateIEdgeFromEdgeVertex(firstEdgeVertexEdge.To);
        }

        private void VertexCommanderControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (initialKeyboardHighlightSet)
                return;

            initialKeyboardHighlightSet = true;
            SetCodeControlQueryText();
            RecordPaneQueryHistory(KeyboardHighlightPane.Left);
            RecordPaneQueryHistory(KeyboardHighlightPane.Right);
            Dispatcher.BeginInvoke(new Action(SetInitialKeyboardHighlight), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetCodeControlQueryText()
        {
            SetCodeControlQueryText(LeftQueryStringCodeControl, LeftBaseEdge);
            SetCodeControlQueryText(RightQueryStringCodeControl, RightBaseEdge);
        }

        private static void SetCodeControlQueryText(CodeControl codeControl, IVertex baseEdgeVertex)
        {
            if (baseEdgeVertex == null)
                return;

            IVertex baseEdgeTo = baseEdgeVertex.Get(false, @"To:");

            if (baseEdgeTo == null)
                return;

            string queryText = GraphUtil.GetQueryBetweenVertexes_byInEdges(baseEdgeTo, MinusZero.Instance.Root);

            codeControl.editor.Text = queryText ?? "";
            codeControl.editor.Background = null;
        }

        private void AddCodeControlEventHandlers()
        {
            LeftQueryStringCodeControl.EnterSubmitted += LeftQueryStringCodeControl_EnterSubmitted;
            RightQueryStringCodeControl.EnterSubmitted += RightQueryStringCodeControl_EnterSubmitted;
        }

        private void LeftQueryStringCodeControl_EnterSubmitted(object sender, System.EventArgs e)
        {
            IVertex baseEdgeTo = MinusZero.Instance.Root.Get(false, LeftQueryStringCodeControl.editor.Text);

            if (baseEdgeTo == null || LeftBaseEdge == null)
                return;

            GraphUtil.ReplaceEdge(LeftBaseEdge, "To", baseEdgeTo);
            ApplyPaneBaseEdgeUpdate(KeyboardHighlightPane.Left, LeftBaseEdge);
            SetKeyboardHighlightAfterBaseEdgeChange(KeyboardHighlightPane.Left);
            RecordPaneQueryHistory(KeyboardHighlightPane.Left);
        }

        private void RightQueryStringCodeControl_EnterSubmitted(object sender, System.EventArgs e)
        {
            IVertex baseEdgeTo = MinusZero.Instance.Root.Get(false, RightQueryStringCodeControl.editor.Text);

            if (baseEdgeTo == null || RightBaseEdge == null)
                return;

            GraphUtil.ReplaceEdge(RightBaseEdge, "To", baseEdgeTo);
            ApplyPaneBaseEdgeUpdate(KeyboardHighlightPane.Right, RightBaseEdge);
            SetKeyboardHighlightAfterBaseEdgeChange(KeyboardHighlightPane.Right);
            RecordPaneQueryHistory(KeyboardHighlightPane.Right);
        }

        private void SetupQueryHistoryControls()
        {
            LeftQueryHistoryList.ItemsSource = leftQueryHistory;
            RightQueryHistoryList.ItemsSource = rightQueryHistory;

            LeftQueryHistoryList.SelectionChanged += LeftQueryHistoryList_SelectionChanged;
            RightQueryHistoryList.SelectionChanged += RightQueryHistoryList_SelectionChanged;

            LeftQueryHistoryPopup.Closed += (s, e) => leftQueryHistoryPopupClosed = System.DateTime.Now;
            RightQueryHistoryPopup.Closed += (s, e) => rightQueryHistoryPopupClosed = System.DateTime.Now;
        }

        private void RecordPaneQueryHistory(KeyboardHighlightPane pane)
        {
            if (isApplyingHistorySelection)
                return;

            CodeControl codeControl = pane == KeyboardHighlightPane.Left
                ? LeftQueryStringCodeControl
                : RightQueryStringCodeControl;

            IVertex baseEdge = pane == KeyboardHighlightPane.Left ? LeftBaseEdge : RightBaseEdge;

            if (codeControl == null || baseEdge == null)
                return;

            string queryText = codeControl.editor.Text;

            if (string.IsNullOrWhiteSpace(queryText))
                return;

            ObservableCollection<QueryHistoryEntry> history = pane == KeyboardHighlightPane.Left
                ? leftQueryHistory
                : rightQueryHistory;

            for (int i = history.Count - 1; i >= 0; i--)
                if (history[i].QueryText == queryText)
                    history.RemoveAt(i);

            history.Insert(0, new QueryHistoryEntry
            {
                QueryText = queryText,
                BaseEdgeTo = baseEdge.Get(false, @"To:")
            });

            while (history.Count > QueryHistoryMax)
                history.RemoveAt(history.Count - 1);
        }

        private void LeftQueryHistoryToggle_Click(object sender, RoutedEventArgs e)
        {
            if ((System.DateTime.Now - leftQueryHistoryPopupClosed).TotalMilliseconds < 250)
                return;

            LeftQueryHistoryPopup.IsOpen = true;
        }

        private void RightQueryHistoryToggle_Click(object sender, RoutedEventArgs e)
        {
            if ((System.DateTime.Now - rightQueryHistoryPopupClosed).TotalMilliseconds < 250)
                return;

            RightQueryHistoryPopup.IsOpen = true;
        }

        private void LeftQueryHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyHistorySelection(KeyboardHighlightPane.Left, LeftQueryHistoryList, LeftQueryHistoryPopup);
        }

        private void RightQueryHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyHistorySelection(KeyboardHighlightPane.Right, RightQueryHistoryList, RightQueryHistoryPopup);
        }

        private void ApplyHistorySelection(KeyboardHighlightPane pane, ListBox historyList, System.Windows.Controls.Primitives.Popup popup)
        {
            if (isApplyingHistorySelection)
                return;

            if (!(historyList.SelectedItem is QueryHistoryEntry entry))
                return;

            isApplyingHistorySelection = true;

            try
            {
                ApplyHistoryEntry(pane, entry);
                historyList.SelectedItem = null;
                popup.IsOpen = false;
            }
            finally
            {
                isApplyingHistorySelection = false;
            }
        }

        private void ApplyHistoryEntry(KeyboardHighlightPane pane, QueryHistoryEntry entry)
        {
            if (entry == null)
                return;

            IVertex baseEdgeTo = entry.BaseEdgeTo;

            if (baseEdgeTo == null && !string.IsNullOrWhiteSpace(entry.QueryText))
                baseEdgeTo = MinusZero.Instance.Root.Get(false, entry.QueryText);

            if (baseEdgeTo == null)
                return;

            if (pane == KeyboardHighlightPane.Left)
            {
                if (LeftBaseEdge == null)
                    return;

                GraphUtil.ReplaceEdge(LeftBaseEdge, "To", baseEdgeTo);
                ApplyPaneBaseEdgeUpdate(KeyboardHighlightPane.Left, LeftBaseEdge, entry.QueryText);
            }
            else
            {
                if (RightBaseEdge == null)
                    return;

                GraphUtil.ReplaceEdge(RightBaseEdge, "To", baseEdgeTo);
                ApplyPaneBaseEdgeUpdate(KeyboardHighlightPane.Right, RightBaseEdge, entry.QueryText);
            }

            SetKeyboardHighlightAfterBaseEdgeChange(pane);
        }

        private void SetVisualiserSelectors()
        {
            IList<IVertex> inEdgesVisualisers = GetQueryToVertexes(InEdgesVisualiserQuery);
            IList<IVertex> outEdgesVisualisers = GetQueryToVertexes(OutEdgesVisualiserQuery);

            SetVisualiserSelectorItems(LeftInEdgesVisualiserSelector, inEdgesVisualisers, LeftInEdgesVisuliserClass);
            SetVisualiserSelectorItems(RightInEdgesVisualiserSelector, inEdgesVisualisers, RightInEdgesVisuliserClass);
            SetVisualiserSelectorItems(LeftOutEdgesVisualiserSelector, outEdgesVisualisers, LeftOutEdgesVisuliserClass);
            SetVisualiserSelectorItems(RightOutEdgesVisualiserSelector, outEdgesVisualisers, RightOutEdgesVisuliserClass);
        }

        private void SetDefaultVisualiserClasses()
        {
            IVertex defaultInEdgesVisualiserClass = MinusZero.Instance.Root.Get(false, DefaultInEdgesVisualiserClassQuery);
            IVertex defaultOutEdgesVisualiserClass = MinusZero.Instance.Root.Get(false, DefaultOutEdgesVisualiserClassQuery);

            LeftInEdgesVisuliserClass = defaultInEdgesVisualiserClass;
            RightInEdgesVisuliserClass = defaultInEdgesVisualiserClass;
            LeftOutEdgesVisuliserClass = defaultOutEdgesVisualiserClass;
            RightOutEdgesVisuliserClass = defaultOutEdgesVisualiserClass;
        }

        private void AddVisualiserSelectorEventHandlers()
        {
            LeftInEdgesVisualiserSelector.SelectionChanged += LeftInEdgesVisualiserSelector_SelectionChanged;
            RightInEdgesVisualiserSelector.SelectionChanged += RightInEdgesVisualiserSelector_SelectionChanged;
            LeftOutEdgesVisualiserSelector.SelectionChanged += LeftOutEdgesVisualiserSelector_SelectionChanged;
            RightOutEdgesVisualiserSelector.SelectionChanged += RightOutEdgesVisualiserSelector_SelectionChanged;

            LeftInEdgesExpander.Expanded += LeftInEdgesExpander_Expanded;
            LeftInEdgesExpander.Collapsed += LeftInEdgesExpander_Collapsed;
            RightInEdgesExpander.Expanded += RightInEdgesExpander_Expanded;
            RightInEdgesExpander.Collapsed += RightInEdgesExpander_Collapsed;
            LeftOutEdgesExpander.Expanded += LeftOutEdgesExpander_Expanded;
            LeftOutEdgesExpander.Collapsed += LeftOutEdgesExpander_Collapsed;
            RightOutEdgesExpander.Expanded += RightOutEdgesExpander_Expanded;
            RightOutEdgesExpander.Collapsed += RightOutEdgesExpander_Collapsed;
        }

        private void VertexCommanderControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsQueryCodeControlKeyboardEvent(e))
                return;

            System.Windows.Input.Key key = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;

            if (key == System.Windows.Input.Key.Tab)
            {
                e.Handled = true;
                SwitchKeyboardHighlightPane();
                return;
            }

            if (HandleCommandKey(key))
            {
                e.Handled = true;
                return;
            }

            if (key != System.Windows.Input.Key.Up
                && key != System.Windows.Input.Key.Down
                && key != System.Windows.Input.Key.Left
                && key != System.Windows.Input.Key.Right
                && key != System.Windows.Input.Key.PageUp
                && key != System.Windows.Input.Key.PageDown
                && key != System.Windows.Input.Key.Space
                && key != System.Windows.Input.Key.Enter)
                return;

            e.Handled = true;

            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(currentKeyboardHighlightPane, currentKeyboardHighlightSection);

            if (keyboardHighlight == null)
                return;

            if (key == System.Windows.Input.Key.Up)
                keyboardHighlight.MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Up);
            else if (key == System.Windows.Input.Key.Down)
                keyboardHighlight.MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Down);
            else if (key == System.Windows.Input.Key.Left)
                keyboardHighlight.MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Left);
            else if (key == System.Windows.Input.Key.Right)
                keyboardHighlight.MoveKeyboardHighlight(KeyboardHighlightMoveDirection.Right);
            else if (key == System.Windows.Input.Key.PageUp)
                keyboardHighlight.MoveKeyboardHighlight(-KeyboardHighlightPageStep);
            else if (key == System.Windows.Input.Key.PageDown)
                keyboardHighlight.MoveKeyboardHighlight(KeyboardHighlightPageStep);
            else if (key == System.Windows.Input.Key.Space)
                keyboardHighlight.ToggleKeyboardHighlightedEdgeSelection();
            else
                HandleKeyboardHighlightEnter(currentKeyboardHighlightPane, currentKeyboardHighlightSection, keyboardHighlight.KeyboardHighlightedEdge);

            if (key == System.Windows.Input.Key.Up
                || key == System.Windows.Input.Key.Down
                || key == System.Windows.Input.Key.Left
                || key == System.Windows.Input.Key.Right
                || key == System.Windows.Input.Key.PageUp
                || key == System.Windows.Input.Key.PageDown
                || key == System.Windows.Input.Key.Space)
                RunLiveSyncIfActive();
        }

        private bool IsQueryCodeControlKeyboardEvent(System.Windows.Input.KeyEventArgs e)
        {
            DependencyObject originalSource = e.OriginalSource as DependencyObject;

            return IsDescendantOf(LeftQueryStringCodeControl, originalSource)
                || IsDescendantOf(RightQueryStringCodeControl, originalSource);
        }

        private static bool IsDescendantOf(DependencyObject parent, DependencyObject child)
        {
            DependencyObject current = child;

            while (current != null)
            {
                if (current == parent)
                    return true;

                DependencyObject next = null;

                if (current is Visual || current is System.Windows.Media.Media3D.Visual3D)
                    next = VisualTreeHelper.GetParent(current);

                if (next == null)
                    next = LogicalTreeHelper.GetParent(current);

                current = next;
            }

            return false;
        }

        private bool HandleCommandKey(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.F3:
                    ViewButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F4:
                    EditButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F5:
                    CopyButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F6:
                    MoveButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F7:
                    NewVertexButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F8:
                    DeleteButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F9:
                    NewEdgeButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F10:
                    ReplaceButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F11:
                    MasterToDetailButton_Click(this, new RoutedEventArgs());
                    return true;
                case System.Windows.Input.Key.F12:
                    DetailToMasterButton_Click(this, new RoutedEventArgs());
                    return true;
                default:
                    return false;
            }
        }

        private IVertex GetActivePaneBaseEdge()
        {
            return currentKeyboardHighlightPane == KeyboardHighlightPane.Left ? LeftBaseEdge : RightBaseEdge;
        }

        private IVertex GetOtherPaneBaseEdge()
        {
            return currentKeyboardHighlightPane == KeyboardHighlightPane.Left ? RightBaseEdge : LeftBaseEdge;
        }

        private IVertex GetPaneBaseEdge(KeyboardHighlightPane pane)
        {
            return pane == KeyboardHighlightPane.Left ? LeftBaseEdge : RightBaseEdge;
        }

        private IVertex GetActivePaneOutEdgesInstance()
        {
            return currentKeyboardHighlightPane == KeyboardHighlightPane.Left
                ? leftOutEdgesVisuliserInstance
                : rightOutEdgesVisuliserInstance;
        }

        private IVertex GetPaneOutEdgesInstance(KeyboardHighlightPane pane)
        {
            return pane == KeyboardHighlightPane.Left
                ? leftOutEdgesVisuliserInstance
                : rightOutEdgesVisuliserInstance;
        }

        private IList<IEdge> GetActivePaneSelectedIEdges()
        {
            List<IEdge> result = new List<IEdge>();

            IVertex instance = GetActivePaneOutEdgesInstance();

            if (instance == null)
                return result;

            IVertex selectedEdges = instance.Get(false, "SelectedEdges:");

            if (selectedEdges == null)
                return result;

            foreach (IEdge e in selectedEdges.GetAll(false, @"{$Is:Edge}"))
                result.Add(EdgeHelper.CreateIEdgeFromEdgeVertex(e.To));

            return result;
        }

        private IList<IEdge> GetActivePaneOperationIEdges()
        {
            IList<IEdge> selectedEdges = GetActivePaneSelectedIEdges();

            if (selectedEdges.Count > 0)
                return selectedEdges;

            IEdge highlightedOutEdge = GetActivePaneHighlightedOutEdge();

            if (highlightedOutEdge == null)
                return new List<IEdge>();

            return new List<IEdge> { highlightedOutEdge };
        }

        private bool EnsureActivePaneDeleteSelection()
        {
            if (GetActivePaneSelectedIEdges().Count > 0)
                return true;

            IEdge highlightedOutEdge = GetActivePaneHighlightedOutEdge();

            if (highlightedOutEdge == null)
                return false;

            IVertex instance = GetActivePaneOutEdgesInstance();

            if (instance == null)
                return false;

            IVertex selectedEdges = instance.Get(false, "SelectedEdges:");

            if (selectedEdges == null)
                return false;

            EdgeHelper.AddEdgeVertex(selectedEdges, highlightedOutEdge);

            return true;
        }

        private IEdge GetActivePaneHighlightedOutEdge()
        {
            if (currentKeyboardHighlightSection != KeyboardHighlightSection.OutEdges)
                return null;

            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(currentKeyboardHighlightPane, KeyboardHighlightSection.OutEdges);

            return keyboardHighlight?.KeyboardHighlightedEdge;
        }

        private int GetKeyboardHighlightPosition(KeyboardHighlightPane pane, KeyboardHighlightSection section)
        {
            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(pane, section);

            if (keyboardHighlight == null)
                return -1;

            return keyboardHighlight.CurrentHighlightPosition;
        }

        private void RestoreKeyboardHighlightPosition(
            KeyboardHighlightPane pane,
            KeyboardHighlightSection section,
            int position,
            bool recreateHappened)
        {
            Action restore = () =>
            {
                currentKeyboardHighlightPane = pane;
                currentKeyboardHighlightSection = section;

                IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(pane, section);

                if (keyboardHighlight == null)
                    return;

                ClearOtherKeyboardHighlights(keyboardHighlight);

                if (position >= 0)
                {
                    keyboardHighlight.IsFirstPosition = true;

                    if (position > 0)
                        keyboardHighlight.MoveKeyboardHighlight(position);
                }
                else if (keyboardHighlight.KeyboardHighlightedEdge == null)
                {
                    keyboardHighlight.IsFirstPosition = true;
                }

                FocusKeyboardHighlightVisualiser(pane, section);
                RunLiveSyncIfActive();
            };

            if (recreateHappened)
                Dispatcher.BeginInvoke(restore, System.Windows.Threading.DispatcherPriority.Loaded);
            else
                restore();
        }

        private void RestoreSingleOutEdgesKeyboardHighlight(KeyboardHighlightPane pane, bool recreateHappened)
        {
            currentKeyboardHighlightPane = pane;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;

            if (recreateHappened)
            {
                Dispatcher.BeginInvoke(
                    new Action(() => SetKeyboardHighlightPosition(pane, KeyboardHighlightSection.OutEdges, true)),
                    System.Windows.Threading.DispatcherPriority.Loaded);
                return;
            }

            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(pane, KeyboardHighlightSection.OutEdges);

            if (keyboardHighlight != null && keyboardHighlight.KeyboardHighlightedEdge != null)
                ActivateKeyboardHighlight(pane, KeyboardHighlightSection.OutEdges, keyboardHighlight);
            else
                SetKeyboardHighlightPosition(pane, KeyboardHighlightSection.OutEdges, true);
        }

        private void RecreatePaneOutEdgesVisualiser(KeyboardHighlightPane pane)
        {
            if (pane == KeyboardHighlightPane.Left)
                RecreateLeftOutEdgesVisualiser();
            else
                RecreateRightOutEdgesVisualiser();
        }

        private void RestorePaneAfterFloatingDialog(
            KeyboardHighlightPane pane,
            KeyboardHighlightSection section,
            int position,
            bool committed)
        {
            if (!committed)
                return;

            Dispatcher.BeginInvoke(
                new Action(() => RestoreKeyboardHighlightPosition(pane, section, position, false)),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void OpenFloatingAtomVisualiser(bool editable)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            KeyboardHighlightSection sourceSection = currentKeyboardHighlightSection;
            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(currentKeyboardHighlightPane, currentKeyboardHighlightSection);

            IEdge highlightedEdge = keyboardHighlight?.KeyboardHighlightedEdge;

            if (highlightedEdge == null)
                return;

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(highlightedEdge);

            FrameworkElement visualiser = editable
                ? (FrameworkElement)new StringVisualiser(baseEdgeVertex, null, false)
                : new StringViewVisualiser(baseEdgeVertex, null, false);

            visualiser.Focusable = true;
            visualiser.PreviewKeyDown += FloatingAtomVisualiser_PreviewKeyDown;
            visualiser.Loaded += (s, args) =>
                visualiser.Dispatcher.BeginInvoke(
                    new Action(() => visualiser.Focus()),
                    System.Windows.Threading.DispatcherPriority.Input);
            visualiser.Unloaded += (s, args) =>
                Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        currentKeyboardHighlightPane = sourcePane;
                        currentKeyboardHighlightSection = sourceSection;
                        Focus();
                        FocusKeyboardHighlightVisualiser(sourcePane, sourceSection);
                    }),
                    System.Windows.Threading.DispatcherPriority.Input);

            MinusZero.Instance.UserInteraction.ShowContentFloating(visualiser, FloatingWindowSize.Large);
        }

        private void FloatingAtomVisualiser_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Escape)
                return;

            e.Handled = true;
            MinusZero.Instance.UserInteraction.CloseWindowByContent(sender);
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFloatingAtomVisualiser(false);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFloatingAtomVisualiser(true);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            IVertex copyTo = GetOtherPaneBaseEdge()?.Get(false, "To:");

            if (copyTo == null)
                return;

            IList<IEdge> selected = GetActivePaneOperationIEdges();

            if (selected.Count == 0)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            VertexOperations.CopyEdgesSet(selected, copyTo);

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////

            RestoreSingleOutEdgesKeyboardHighlight(sourcePane, false);
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            IVertex moveTo = GetOtherPaneBaseEdge()?.Get(false, "To:");

            if (moveTo == null)
                return;

            IList<IEdge> selected = GetActivePaneOperationIEdges();

            if (selected.Count == 0)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            VertexOperations.MoveEdgesSet(selected, moveTo);

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////

            RecreateLeftOutEdgesVisualiser();
            RecreateRightOutEdgesVisualiser();
            RestoreSingleOutEdgesKeyboardHighlight(sourcePane, true);
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            IVertex replaceTo = GetOtherPaneBaseEdge()?.Get(false, "To:");

            if (replaceTo == null)
                return;

            IList<IEdge> selected = GetActivePaneOperationIEdges();

            if (selected.Count == 0)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            VertexOperations.MoveAndReplaceEdgesSet(selected, replaceTo);

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////

            RecreateLeftOutEdgesVisualiser();
            RecreateRightOutEdgesVisualiser();
            RestoreSingleOutEdgesKeyboardHighlight(sourcePane, true);
        }

        private void NewVertexButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            KeyboardHighlightSection sourceSection = currentKeyboardHighlightSection;
            int sourcePosition = GetKeyboardHighlightPosition(sourcePane, sourceSection);
            IVertex baseEdge = GetPaneBaseEdge(sourcePane);

            if (baseEdge == null)
                return;

            IVertex baseEdgeTo = baseEdge.Get(false, "To:");

            if (baseEdgeTo == null)
                return;

            m0.UIWpf.Dialog.NewVertex dialog = new m0.UIWpf.Dialog.NewVertex(baseEdgeTo);
            dialog.Unloaded += (s, args) => RestorePaneAfterFloatingDialog(sourcePane, sourceSection, sourcePosition, dialog.IsCommitted);

            MinusZero.Instance.UserInteraction.ShowContentFloating(dialog, FloatingWindowSize.Micro);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            IVertex baseEdge = GetActivePaneBaseEdge();
            IVertex instance = GetActivePaneOutEdgesInstance();

            if (baseEdge == null || instance == null)
                return;

            if (!EnsureActivePaneDeleteSelection())
                return;

            BaseCommands.Delete(baseEdge, instance);

            RecreatePaneOutEdgesVisualiser(sourcePane);
            RestoreSingleOutEdgesKeyboardHighlight(sourcePane, true);
        }

        private void NewEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHighlightPane sourcePane = currentKeyboardHighlightPane;
            KeyboardHighlightSection sourceSection = currentKeyboardHighlightSection;
            int sourcePosition = GetKeyboardHighlightPosition(sourcePane, sourceSection);
            IVertex baseEdge = GetPaneBaseEdge(sourcePane);

            if (baseEdge == null)
                return;

            IVertex baseEdgeTo = baseEdge.Get(false, "To:");

            if (baseEdgeTo == null)
                return;

            m0.UIWpf.Dialog.NewEdge dialog = new m0.UIWpf.Dialog.NewEdge(baseEdgeTo);
            dialog.Unloaded += (s, args) => RestorePaneAfterFloatingDialog(sourcePane, sourceSection, sourcePosition, dialog.IsCommitted);

            MinusZero.Instance.UserInteraction.ShowContentFloating(dialog, FloatingWindowSize.Micro);
        }

        private void MasterToDetailButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleLiveSync(KeyboardHighlightPane.Left);
        }

        private void DetailToMasterButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleLiveSync(KeyboardHighlightPane.Right);
        }

        private void ToggleLiveSync(KeyboardHighlightPane masterPane)
        {
            if (liveSyncMasterPane == masterPane)
            {
                liveSyncMasterPane = null;
                liveSyncTimer?.Stop();
                UpdateLiveSyncButtonsVisualState();
                return;
            }

            liveSyncMasterPane = masterPane;
            currentKeyboardHighlightPane = masterPane;
            UpdateLiveSyncButtonsVisualState();
            RunLiveSyncIfActive();
        }

        private void UpdateLiveSyncButtonsVisualState()
        {
            SetCommandButtonPressed(MasterToDetailButton, liveSyncMasterPane == KeyboardHighlightPane.Left);
            SetCommandButtonPressed(DetailToMasterButton, liveSyncMasterPane == KeyboardHighlightPane.Right);
        }

        private void SetCommandButtonPressed(Button button, bool pressed)
        {
            if (button == null)
                return;

            if (pressed)
            {
                button.Background = (Brush)FindResource("0ForegroundBrush");
                button.Foreground = (Brush)FindResource("0BackgroundBrush");
            }
            else
            {
                button.Background = (Brush)FindResource("0BackgroundBrush");
                button.Foreground = (Brush)FindResource("0ForegroundBrush");
            }
        }

        // Coalesces rapid master navigation: only the last move within the debounce window
        // triggers the (expensive) detail-pane refresh.
        private void RunLiveSyncIfActive()
        {
            if (isLiveSyncing || liveSyncMasterPane == null)
                return;

            if (liveSyncTimer == null)
            {
                liveSyncTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = System.TimeSpan.FromMilliseconds(120)
                };

                liveSyncTimer.Tick += (s, e) =>
                {
                    liveSyncTimer.Stop();
                    PerformLiveSync();
                };
            }

            liveSyncTimer.Stop();
            liveSyncTimer.Start();
        }

        private void PerformLiveSync()
        {
            if (isLiveSyncing || liveSyncMasterPane == null)
                return;

            KeyboardHighlightPane masterPane = liveSyncMasterPane.Value;

            if (currentKeyboardHighlightPane != masterPane)
                return;

            // master tracks OutEdges only; ignore when the highlight sits on incoming edges
            if (currentKeyboardHighlightSection != KeyboardHighlightSection.OutEdges)
                return;

            IKeyboardHighlight masterHighlight = GetKeyboardHighlight(masterPane, KeyboardHighlightSection.OutEdges);

            IEdge masterEdge = masterHighlight?.KeyboardHighlightedEdge;

            if (masterEdge == null || masterEdge.To == null)
                return;

            KeyboardHighlightPane detailPane = masterPane == KeyboardHighlightPane.Left
                ? KeyboardHighlightPane.Right
                : KeyboardHighlightPane.Left;

            isLiveSyncing = true;

            try
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                try
                {
                    ApplyLiveSyncDetailBaseEdge(detailPane, EdgeHelper.CreateTempEdgeVertex(masterEdge));
                }
                finally
                {
                    //////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    //////////////////////////////////////
                }
            }
            finally
            {
                isLiveSyncing = false;
            }
        }

        private void ApplyLiveSyncDetailBaseEdge(KeyboardHighlightPane detailPane, IVertex baseEdgeVertex)
        {
            ApplyPaneBaseEdgeUpdate(detailPane, baseEdgeVertex);
        }

        private void ApplyPaneBaseEdgeUpdate(KeyboardHighlightPane pane, IVertex baseEdgeVertex, string queryTextOverride = null)
        {
            if (baseEdgeVertex == null)
                return;

            CodeControl queryCodeControl = pane == KeyboardHighlightPane.Left
                ? LeftQueryStringCodeControl
                : RightQueryStringCodeControl;

            if (pane == KeyboardHighlightPane.Left)
                LeftBaseEdge = baseEdgeVertex;
            else
                RightBaseEdge = baseEdgeVertex;

            if (!string.IsNullOrWhiteSpace(queryTextOverride))
            {
                queryCodeControl.editor.Text = queryTextOverride;
                queryCodeControl.editor.Background = null;
            }
            else
                SetCodeControlQueryText(queryCodeControl, baseEdgeVertex);

            IVertex inEdgesVisualiserInstance = pane == KeyboardHighlightPane.Left
                ? leftInEdgesVisuliserInstance
                : rightInEdgesVisuliserInstance;

            IVertex outEdgesVisualiserInstance = pane == KeyboardHighlightPane.Left
                ? leftOutEdgesVisuliserInstance
                : rightOutEdgesVisuliserInstance;

            if (inEdgesVisualiserInstance == null && outEdgesVisualiserInstance == null)
            {
                if (pane == KeyboardHighlightPane.Left)
                {
                    RecreateLeftInEdgesVisualiser();
                    RecreateLeftOutEdgesVisualiser();
                }
                else
                {
                    RecreateRightInEdgesVisualiser();
                    RecreateRightOutEdgesVisualiser();
                }

                return;
            }

            Interaction.BeginInteractionWithGraph();

            try
            {
                if (inEdgesVisualiserInstance != null)
                    GraphUtil.ReplaceEdge(inEdgesVisualiserInstance, "BaseEdge", baseEdgeVertex);

                if (outEdgesVisualiserInstance != null)
                    GraphUtil.ReplaceEdge(outEdgesVisualiserInstance, "BaseEdge", baseEdgeVertex);
            }
            finally
            {
                Interaction.EndInteractionWithGraph();
            }
        }

        private void SwitchKeyboardHighlightPane()
        {
            IKeyboardHighlight currentKeyboardHighlight = GetKeyboardHighlight(currentKeyboardHighlightPane, currentKeyboardHighlightSection);

            if (currentKeyboardHighlight != null)
                currentKeyboardHighlight.ClearKeyboardHighlight();

            currentKeyboardHighlightPane = currentKeyboardHighlightPane == KeyboardHighlightPane.Left
                ? KeyboardHighlightPane.Right
                : KeyboardHighlightPane.Left;

            SetKeyboardHighlightPosition(currentKeyboardHighlightPane, currentKeyboardHighlightSection, true);
        }

        private void SetInitialKeyboardHighlight()
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            SetFirstAvailableKeyboardHighlight(KeyboardHighlightPane.Left);
        }

        private void SetKeyboardHighlightAfterBaseEdgeChange(KeyboardHighlightPane pane)
        {
            Dispatcher.BeginInvoke(
                new Action(() => SetFirstAvailableKeyboardHighlight(pane)),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetFirstAvailableKeyboardHighlight(KeyboardHighlightPane pane)
        {
            currentKeyboardHighlightPane = pane;

            IKeyboardHighlight outEdgesKeyboardHighlight = GetKeyboardHighlight(pane, KeyboardHighlightSection.OutEdges);

            if (outEdgesKeyboardHighlight != null)
            {
                currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
                SetKeyboardHighlightPosition(pane, KeyboardHighlightSection.OutEdges, true);
                return;
            }

            IKeyboardHighlight inEdgesKeyboardHighlight = GetKeyboardHighlight(pane, KeyboardHighlightSection.IncomingEdges);

            if (inEdgesKeyboardHighlight != null)
            {
                currentKeyboardHighlightSection = KeyboardHighlightSection.IncomingEdges;
                SetKeyboardHighlightPosition(pane, KeyboardHighlightSection.IncomingEdges, true);
            }
        }

        private static IList<IVertex> GetQueryToVertexes(string query)
        {
            IList<IVertex> vertexes = new List<IVertex>();

            foreach (IEdge edge in MinusZero.Instance.Root.GetAll(false, query))
                vertexes.Add(edge.To);

            return vertexes;
        }

        private static void SetVisualiserSelectorItems(ComboBox selector, IList<IVertex> visualisers, IVertex defaultVisualiserClass)
        {
            selector.ItemsSource = visualisers;

            IVertex selectedVisualiser = visualisers.FirstOrDefault(visualiser => visualiser == defaultVisualiserClass);

            if (selectedVisualiser != null)
                selector.SelectedItem = selectedVisualiser;
            else if (visualisers.Count > 0)
                selector.SelectedIndex = 0;
        }

        private void CreateAllVisualiserInstances()
        {
            RecreateLeftInEdgesVisualiser();
            RecreateRightInEdgesVisualiser();
            RecreateLeftOutEdgesVisualiser();
            RecreateRightOutEdgesVisualiser();
        }

        private void LeftInEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftInEdgesVisuliserClass = (IVertex)LeftInEdgesVisualiserSelector.SelectedItem;
            RecreateLeftInEdgesVisualiser();
        }

        private void RightInEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightInEdgesVisuliserClass = (IVertex)RightInEdgesVisualiserSelector.SelectedItem;
            RecreateRightInEdgesVisualiser();
        }

        private void LeftOutEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftOutEdgesVisuliserClass = (IVertex)LeftOutEdgesVisualiserSelector.SelectedItem;
            RecreateLeftOutEdgesVisualiser();
        }

        private void RightOutEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightOutEdgesVisuliserClass = (IVertex)RightOutEdgesVisualiserSelector.SelectedItem;
            RecreateRightOutEdgesVisualiser();
        }

        private void LeftInEdgesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ShowVisualiserSelector(LeftInEdgesVisualiserSelector);
        }

        private void LeftInEdgesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            HideVisualiserSelector(LeftInEdgesVisualiserSelector);
        }

        private void RightInEdgesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ShowVisualiserSelector(RightInEdgesVisualiserSelector);
        }

        private void RightInEdgesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            HideVisualiserSelector(RightInEdgesVisualiserSelector);
        }

        private void LeftOutEdgesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ShowVisualiserSelector(LeftOutEdgesVisualiserSelector);
        }

        private void LeftOutEdgesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            HideVisualiserSelector(LeftOutEdgesVisualiserSelector);
        }

        private void RightOutEdgesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ShowVisualiserSelector(RightOutEdgesVisualiserSelector);
        }

        private void RightOutEdgesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            HideVisualiserSelector(RightOutEdgesVisualiserSelector);
        }

        private static void ShowVisualiserSelector(ComboBox selector)
        {
            selector.IsHitTestVisible = true;
            AnimateVisualiserSelector(selector, 1);
        }

        private static void HideVisualiserSelector(ComboBox selector)
        {
            selector.IsHitTestVisible = false;
            AnimateVisualiserSelector(selector, 0);
        }

        private static void AnimateVisualiserSelector(ComboBox selector, double opacity)
        {
            DoubleAnimation animation = new DoubleAnimation(opacity, new Duration(System.TimeSpan.FromMilliseconds(200)));
            selector.BeginAnimation(OpacityProperty, animation);
        }

        private void LeftInEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.IncomingEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.IncomingEdges, true);
        }

        private void LeftInEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.OutEdges, true);
        }

        private void LeftInEdgesKeyboardHighlight_KeyboardHighlightActivated(object sender, System.EventArgs e)
        {
            ActivateKeyboardHighlight(KeyboardHighlightPane.Left, KeyboardHighlightSection.IncomingEdges, leftInEdgesKeyboardHighlight);
        }

        private void LeftOutEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.IncomingEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.IncomingEdges, false);
        }

        private void LeftOutEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.OutEdges, false);
        }

        private void LeftOutEdgesKeyboardHighlight_KeyboardHighlightActivated(object sender, System.EventArgs e)
        {
            ActivateKeyboardHighlight(KeyboardHighlightPane.Left, KeyboardHighlightSection.OutEdges, leftOutEdgesKeyboardHighlight);
        }

        private void RightInEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.IncomingEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.IncomingEdges, true);
        }

        private void RightInEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.OutEdges, true);
        }

        private void RightInEdgesKeyboardHighlight_KeyboardHighlightActivated(object sender, System.EventArgs e)
        {
            ActivateKeyboardHighlight(KeyboardHighlightPane.Right, KeyboardHighlightSection.IncomingEdges, rightInEdgesKeyboardHighlight);
        }

        private void RightOutEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.IncomingEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.IncomingEdges, false);
        }

        private void RightOutEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.OutEdges, false);
        }

        private void RightOutEdgesKeyboardHighlight_KeyboardHighlightActivated(object sender, System.EventArgs e)
        {
            ActivateKeyboardHighlight(KeyboardHighlightPane.Right, KeyboardHighlightSection.OutEdges, rightOutEdgesKeyboardHighlight);
        }

        private void ActivateKeyboardHighlight(
            KeyboardHighlightPane pane,
            KeyboardHighlightSection section,
            IKeyboardHighlight keyboardHighlight)
        {
            if (keyboardHighlight == null)
                return;

            currentKeyboardHighlightPane = pane;
            currentKeyboardHighlightSection = section;
            ClearOtherKeyboardHighlights(keyboardHighlight);
            RunLiveSyncIfActive();
        }

        private void LeftInEdgesKeyboardHighlight_KeyboardHighlightEnterPressed(object sender, System.EventArgs e)
        {
            HandleKeyboardHighlightEnter(KeyboardHighlightPane.Left, KeyboardHighlightSection.IncomingEdges, leftInEdgesKeyboardHighlight?.KeyboardHighlightedEdge);
        }

        private void RightInEdgesKeyboardHighlight_KeyboardHighlightEnterPressed(object sender, System.EventArgs e)
        {
            HandleKeyboardHighlightEnter(KeyboardHighlightPane.Right, KeyboardHighlightSection.IncomingEdges, rightInEdgesKeyboardHighlight?.KeyboardHighlightedEdge);
        }

        private void LeftOutEdgesKeyboardHighlight_KeyboardHighlightEnterPressed(object sender, System.EventArgs e)
        {
            HandleKeyboardHighlightEnter(KeyboardHighlightPane.Left, KeyboardHighlightSection.OutEdges, leftOutEdgesKeyboardHighlight?.KeyboardHighlightedEdge);
        }

        private void RightOutEdgesKeyboardHighlight_KeyboardHighlightEnterPressed(object sender, System.EventArgs e)
        {
            HandleKeyboardHighlightEnter(KeyboardHighlightPane.Right, KeyboardHighlightSection.OutEdges, rightOutEdgesKeyboardHighlight?.KeyboardHighlightedEdge);
        }

        private void HandleKeyboardHighlightEnter(
            KeyboardHighlightPane pane,
            KeyboardHighlightSection section,
            IEdge highlightedEdge)
        {
            if (highlightedEdge == null)
                return;

            IEdge newBaseEdge = null;

            if (section == KeyboardHighlightSection.OutEdges)
            {
                newBaseEdge = highlightedEdge;
            }
            else if (highlightedEdge.From != null && highlightedEdge.From.InEdgesRaw.Count > 0)
            {
                newBaseEdge = highlightedEdge.From.InEdgesRaw[0];
            }

            if (newBaseEdge == null)
                return;

            Dispatcher.BeginInvoke(
                new Action(() => ApplyKeyboardHighlightEnter(pane, newBaseEdge)),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void ApplyKeyboardHighlightEnter(KeyboardHighlightPane pane, IEdge newBaseEdge)
        {
            IVertex newBaseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(newBaseEdge);

            ApplyPaneBaseEdgeUpdate(pane, newBaseEdgeVertex);
            RecordPaneQueryHistory(pane);

            currentKeyboardHighlightPane = pane;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            Dispatcher.BeginInvoke(
                new Action(() => SetKeyboardHighlightPosition(pane, KeyboardHighlightSection.OutEdges, true)),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetKeyboardHighlightPosition(
            KeyboardHighlightPane pane,
            KeyboardHighlightSection section,
            bool isFirstPosition)
        {
            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(pane, section);

            if (keyboardHighlight == null)
                return;

            ClearOtherKeyboardHighlights(keyboardHighlight);

            if (isFirstPosition)
                keyboardHighlight.IsFirstPosition = true;
            else
                keyboardHighlight.IsLastPosition = true;

            FocusKeyboardHighlightVisualiser(pane, section);

            RunLiveSyncIfActive();
        }

        private IKeyboardHighlight GetKeyboardHighlight(KeyboardHighlightPane pane, KeyboardHighlightSection section)
        {
            if (pane == KeyboardHighlightPane.Left)
                return section == KeyboardHighlightSection.IncomingEdges
                    ? leftInEdgesKeyboardHighlight
                    : leftOutEdgesKeyboardHighlight;

            return section == KeyboardHighlightSection.IncomingEdges
                ? rightInEdgesKeyboardHighlight
                : rightOutEdgesKeyboardHighlight;
        }

        private void ClearOtherKeyboardHighlights(IKeyboardHighlight keyboardHighlightToKeep)
        {
            ClearKeyboardHighlightIfOther(leftInEdgesKeyboardHighlight, keyboardHighlightToKeep);
            ClearKeyboardHighlightIfOther(leftOutEdgesKeyboardHighlight, keyboardHighlightToKeep);
            ClearKeyboardHighlightIfOther(rightInEdgesKeyboardHighlight, keyboardHighlightToKeep);
            ClearKeyboardHighlightIfOther(rightOutEdgesKeyboardHighlight, keyboardHighlightToKeep);
        }

        private static void ClearKeyboardHighlightIfOther(
            IKeyboardHighlight keyboardHighlight,
            IKeyboardHighlight keyboardHighlightToKeep)
        {
            if (keyboardHighlight != null && keyboardHighlight != keyboardHighlightToKeep)
                keyboardHighlight.ClearKeyboardHighlight();
        }

        private void FocusKeyboardHighlightVisualiser(KeyboardHighlightPane pane, KeyboardHighlightSection section)
        {
            ContentControl host;

            if (pane == KeyboardHighlightPane.Left)
                host = section == KeyboardHighlightSection.IncomingEdges
                    ? LeftInEdgesVisualiserHost
                    : LeftOutEdgesVisualiserHost;
            else
                host = section == KeyboardHighlightSection.IncomingEdges
                    ? RightInEdgesVisualiserHost
                    : RightOutEdgesVisualiserHost;

            DataGrid dataGrid = FindVisualChild<DataGrid>(host);

            if (dataGrid != null)
                dataGrid.Focus();
            else if (host?.Content is UIElement contentElement)
                contentElement.Focus();
        }

        private static T FindVisualChild<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(dependencyObject, i);

                if (child is T)
                    return (T)child;

                T descendant = FindVisualChild<T>(child);

                if (descendant != null)
                    return descendant;
            }

            return null;
        }

        private void RecreateLeftInEdgesVisualiser()
        {
            RecreateVisualiser(
                LeftInEdgesVisuliserClass,
                LeftBaseEdge,
                LeftInEdgesVisualiserHost,
                LeftInEdgesWrapVisualiserHost,
                LeftInEdgesSectionScrollViewer,
                true,
                LeftInEdgesKeyboardHighlight_GoneBeforeFirstPosition,
                LeftInEdgesKeyboardHighlight_GoneAfterLastPosition,
                LeftInEdgesKeyboardHighlight_KeyboardHighlightActivated,
                LeftInEdgesKeyboardHighlight_KeyboardHighlightEnterPressed,
                ref leftInEdgesKeyboardHighlight,
                ref leftInEdgesVisuliserInstance);
        }

        private void RecreateRightInEdgesVisualiser()
        {
            RecreateVisualiser(
                RightInEdgesVisuliserClass,
                RightBaseEdge,
                RightInEdgesVisualiserHost,
                RightInEdgesWrapVisualiserHost,
                RightInEdgesSectionScrollViewer,
                true,
                RightInEdgesKeyboardHighlight_GoneBeforeFirstPosition,
                RightInEdgesKeyboardHighlight_GoneAfterLastPosition,
                RightInEdgesKeyboardHighlight_KeyboardHighlightActivated,
                RightInEdgesKeyboardHighlight_KeyboardHighlightEnterPressed,
                ref rightInEdgesKeyboardHighlight,
                ref rightInEdgesVisuliserInstance);
        }

        private void RecreateLeftOutEdgesVisualiser()
        {
            RecreateVisualiser(
                LeftOutEdgesVisuliserClass,
                LeftBaseEdge,
                LeftOutEdgesVisualiserHost,
                LeftOutEdgesWrapVisualiserHost,
                LeftOutEdgesSectionScrollViewer,
                false,
                LeftOutEdgesKeyboardHighlight_GoneBeforeFirstPosition,
                LeftOutEdgesKeyboardHighlight_GoneAfterLastPosition,
                LeftOutEdgesKeyboardHighlight_KeyboardHighlightActivated,
                LeftOutEdgesKeyboardHighlight_KeyboardHighlightEnterPressed,
                ref leftOutEdgesKeyboardHighlight,
                ref leftOutEdgesVisuliserInstance);
        }

        private void RecreateRightOutEdgesVisualiser()
        {
            RecreateVisualiser(
                RightOutEdgesVisuliserClass,
                RightBaseEdge,
                RightOutEdgesVisualiserHost,
                RightOutEdgesWrapVisualiserHost,
                RightOutEdgesSectionScrollViewer,
                false,
                RightOutEdgesKeyboardHighlight_GoneBeforeFirstPosition,
                RightOutEdgesKeyboardHighlight_GoneAfterLastPosition,
                RightOutEdgesKeyboardHighlight_KeyboardHighlightActivated,
                RightOutEdgesKeyboardHighlight_KeyboardHighlightEnterPressed,
                ref rightOutEdgesKeyboardHighlight,
                ref rightOutEdgesVisuliserInstance);
        }

        private static void RecreateVisualiser(
            IVertex visualiserClass,
            IVertex baseVertex,
            ContentControl visualiserHost,
            ContentControl wrapVisualiserHost,
            ScrollViewer sectionScrollViewer,
            bool isInEdgesVisualiser,
            EventHandler goneBeforeFirstPositionHandler,
            EventHandler goneAfterLastPositionHandler,
            EventHandler keyboardHighlightActivatedHandler,
            EventHandler keyboardHighlightEnterPressedHandler,
            ref IKeyboardHighlight keyboardHighlight,
            ref IVertex visualiserInstance)
        {
            DisposeVisualiser(
                visualiserHost,
                wrapVisualiserHost,
                goneBeforeFirstPositionHandler,
                goneAfterLastPositionHandler,
                keyboardHighlightActivatedHandler,
                keyboardHighlightEnterPressedHandler,
                ref keyboardHighlight,
                ref visualiserInstance);

            if (visualiserClass == null)
                return;

            Interaction.BeginInteractionWithGraph();

            try
            {
                IPlatformClass visualiser = PlatformClass.CreatePlatformObject(visualiserClass, null as IVertex);
                IVisualiser visualiserAsVisualiser = visualiser as IVisualiser;

                if (visualiserAsVisualiser != null)
                {
                    visualiserAsVisualiser.SelectionProhibited = isInEdgesVisualiser;
                    ConfigureVertexCommanderVisualiser(visualiser);
                    ConfigureEdgesVisibility(visualiserAsVisualiser, isInEdgesVisualiser);

                    if (baseVertex != null)
                        GraphUtil.CreateOrReplaceEdge(visualiserAsVisualiser.Vertex, BaseEdgeMeta, baseVertex);

                    visualiserAsVisualiser.BaseEdgeToUpdated();
                }

                visualiserInstance = visualiser.Vertex;
                SetClipToBoundsIfPossible(visualiser);
                SetViewportSizeIfNeeded(visualiser, sectionScrollViewer);
                visualiserHost.Content = visualiser;
                SetWrapVisualiser(wrapVisualiserHost, visualiserInstance);
                SetKeyboardHighlight(
                    visualiser,
                    goneBeforeFirstPositionHandler,
                    goneAfterLastPositionHandler,
                    keyboardHighlightActivatedHandler,
                    keyboardHighlightEnterPressedHandler,
                    ref keyboardHighlight);
            }
            finally
            {
                Interaction.EndInteractionWithGraph();
            }
        }

        private static void SetWrapVisualiser(ContentControl wrapVisualiserHost, IVertex visualiserInstance)
        {
            DisposeContent(wrapVisualiserHost);

            if (visualiserInstance == null)
                return;

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, visualiserInstance);
            WrapVisualiser wrapVisualiser = new WrapVisualiser(baseEdgeVertex, 0.6, visualiserInstance, true);
            wrapVisualiser.Background = (Brush)wrapVisualiser.FindResource("0VeryLightGrayBrush");
            wrapVisualiser.ClipToBounds = true;
            wrapVisualiser.HorizontalAlignment = HorizontalAlignment.Left;
            wrapVisualiserHost.Content = wrapVisualiser;
        }

        private static void ConfigureEdgesVisibility(IVisualiser visualiser, bool isInEdgesVisualiser)
        {
            IVertex showOutEdges = visualiser.Vertex.Get(false, "ShowOutEdges:");

            if (showOutEdges != null)
                showOutEdges.Value = isInEdgesVisualiser ? "False" : "True";

            IVertex showInEdges = visualiser.Vertex.Get(false, "ShowInEdges:");

            if (showInEdges != null)
                showInEdges.Value = isInEdgesVisualiser ? "True" : "False";
        }

        private static void ConfigureVertexCommanderVisualiser(IPlatformClass visualiser)
        {
            TreeVisualiser treeVisualiser = visualiser as TreeVisualiser;

            if (treeVisualiser != null)
                treeVisualiser.FullWidthSelectionHighlight = true;
        }

        private static void SetKeyboardHighlight(
            object visualiser,
            EventHandler goneBeforeFirstPositionHandler,
            EventHandler goneAfterLastPositionHandler,
            EventHandler keyboardHighlightActivatedHandler,
            EventHandler keyboardHighlightEnterPressedHandler,
            ref IKeyboardHighlight keyboardHighlight)
        {
            keyboardHighlight = visualiser as IKeyboardHighlight;

            if (keyboardHighlight == null)
                return;

            keyboardHighlight.IsVertexCommanderKeyboardHighlightEnabled = true;

            FormVisualiser formVisualiser = visualiser as FormVisualiser;

            if (formVisualiser != null)
                formVisualiser.EnableNestedVertexCommanderKeyboardHighlight();

            keyboardHighlight.GoneBeforeFirstPosition += goneBeforeFirstPositionHandler;
            keyboardHighlight.GoneAfterLastPosition += goneAfterLastPositionHandler;
            keyboardHighlight.KeyboardHighlightActivated += keyboardHighlightActivatedHandler;
            keyboardHighlight.KeyboardHighlightEnterPressed += keyboardHighlightEnterPressedHandler;
        }

        private static void DisposeVisualiser(
            ContentControl visualiserHost,
            ContentControl wrapVisualiserHost,
            EventHandler goneBeforeFirstPositionHandler,
            EventHandler goneAfterLastPositionHandler,
            EventHandler keyboardHighlightActivatedHandler,
            EventHandler keyboardHighlightEnterPressedHandler,
            ref IKeyboardHighlight keyboardHighlight,
            ref IVertex visualiserInstance)
        {
            if (keyboardHighlight != null)
            {
                keyboardHighlight.IsVertexCommanderKeyboardHighlightEnabled = false;
                keyboardHighlight.ClearKeyboardHighlight();
                keyboardHighlight.GoneBeforeFirstPosition -= goneBeforeFirstPositionHandler;
                keyboardHighlight.GoneAfterLastPosition -= goneAfterLastPositionHandler;
                keyboardHighlight.KeyboardHighlightActivated -= keyboardHighlightActivatedHandler;
                keyboardHighlight.KeyboardHighlightEnterPressed -= keyboardHighlightEnterPressedHandler;
                keyboardHighlight = null;
            }

            DisposeContent(wrapVisualiserHost);
            DisposeContent(visualiserHost);

            if (visualiserInstance != null)
            {
                visualiserInstance.Dispose();
                visualiserInstance = null;
            }
        }

        private static void DisposeContent(ContentControl host)
        {
            if (host.Content is System.IDisposable disposableContent)
                disposableContent.Dispose();

            host.Content = null;
        }

        private static void SetClipToBoundsIfPossible(object visualiser)
        {
            if (visualiser is UIElement element)
                element.ClipToBounds = true;
        }

        private static void SetViewportSizeIfNeeded(object visualiser, ScrollViewer sectionScrollViewer)
        {
            FrameworkElement frameworkElement = visualiser as FrameworkElement;

            if (frameworkElement == null)
                return;

            double viewportWidth = sectionScrollViewer.ViewportWidth;
            double viewportHeight = sectionScrollViewer.ViewportHeight;

            if (double.IsNaN(viewportWidth) || viewportWidth <= 0)
                viewportWidth = sectionScrollViewer.ActualWidth;

            if (double.IsNaN(viewportHeight) || viewportHeight <= 0)
                viewportHeight = sectionScrollViewer.ActualHeight;

            if (visualiser is IconVisualiser iconVisualiser)
            {
                frameworkElement.MaxWidth = System.Math.Max(100, viewportWidth - 4);
                frameworkElement.Width = double.NaN;
                iconVisualiser.RefreshWrapLayout();
                return;
            }

            if (visualiser is ListVisualiser || visualiser is InEdgesListVisualiser)
            {
                frameworkElement.MaxWidth = System.Math.Max(100, viewportWidth - 4);
                frameworkElement.MaxHeight = System.Math.Max(100, viewportHeight - 4);
                frameworkElement.Width = double.NaN;
                frameworkElement.Height = double.NaN;
                return;
            }

            if (!(visualiser is GraphVisualiser) && !(visualiser is GraphVisualiser3D))
                return;

            frameworkElement.Width = System.Math.Max(100, viewportWidth - 4);
            frameworkElement.Height = System.Math.Max(100, viewportHeight - 40);
        }
    }
}
