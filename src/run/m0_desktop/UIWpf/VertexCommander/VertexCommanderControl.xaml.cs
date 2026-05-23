using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

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
        private KeyboardHighlightSection currentKeyboardHighlightSection = KeyboardHighlightSection.InEdges;

        private enum KeyboardHighlightPane
        {
            Left,
            Right
        }

        private enum KeyboardHighlightSection
        {
            InEdges,
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
            SetDefaultVisualiserClasses();
            SetVisualiserSelectors();
            AddVisualiserSelectorEventHandlers();
            CreateAllVisualiserInstances();
            UpdateBottomCommandButtonsFontSize();
        }

        private void VertexCommanderControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBottomCommandButtonsFontSize();
        }

        private void UpdateBottomCommandButtonsFontSize()
        {
            double buttonWidth = ActualWidth / 9.0;
            double fontSize = Math.Max(7.0, Math.Min(10.0, buttonWidth / 11.0));

            SetBottomCommandButtonFontSize(ViewButton, fontSize);
            SetBottomCommandButtonFontSize(EditButton, fontSize);
            SetBottomCommandButtonFontSize(CopyButton, fontSize);
            SetBottomCommandButtonFontSize(MoveButton, fontSize);
            SetBottomCommandButtonFontSize(NewVertexButton, fontSize);
            SetBottomCommandButtonFontSize(DeleteButton, fontSize);
            SetBottomCommandButtonFontSize(NewEdgeButton, fontSize);
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
            SetupCodeControlDropTarget(RightQueryStringCodeControlHost, RightQueryStringCodeControl_Drop);
            SetupCodeControlDropTarget(RightQueryStringCodeControl, RightQueryStringCodeControl_Drop);
        }

        private static void SetupCodeControlDropTarget(UIElement element, DragEventHandler dropHandler)
        {
            element.AllowDrop = true;
            element.Drop += dropHandler;
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
            IEdge droppedEdge = GetDroppedEdge(e);

            if (droppedEdge == null)
                return;

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(droppedEdge);

            if (pane == KeyboardHighlightPane.Left)
            {
                LeftBaseEdge = baseEdgeVertex;
                SetCodeControlQueryText(LeftQueryStringCodeControl, LeftBaseEdge);
                RecreateLeftInEdgesVisualiser();
                RecreateLeftOutEdgesVisualiser();
            }
            else
            {
                RightBaseEdge = baseEdgeVertex;
                SetCodeControlQueryText(RightQueryStringCodeControl, RightBaseEdge);
                RecreateRightInEdgesVisualiser();
                RecreateRightOutEdgesVisualiser();
            }

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
            SetCodeControlQueryText();
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
            RecreateLeftInEdgesVisualiser();
            RecreateLeftOutEdgesVisualiser();
        }

        private void RightQueryStringCodeControl_EnterSubmitted(object sender, System.EventArgs e)
        {
            IVertex baseEdgeTo = MinusZero.Instance.Root.Get(false, RightQueryStringCodeControl.editor.Text);

            if (baseEdgeTo == null || RightBaseEdge == null)
                return;

            GraphUtil.ReplaceEdge(RightBaseEdge, "To", baseEdgeTo);
            RecreateRightInEdgesVisualiser();
            RecreateRightOutEdgesVisualiser();
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
            if (e.Key == System.Windows.Input.Key.Tab)
            {
                e.Handled = true;
                SwitchKeyboardHighlightPane();
                return;
            }

            if (HandleCommandKey(e.Key))
            {
                e.Handled = true;
                return;
            }

            if (e.Key != System.Windows.Input.Key.Up
                && e.Key != System.Windows.Input.Key.Down
                && e.Key != System.Windows.Input.Key.Space
                && e.Key != System.Windows.Input.Key.Enter)
                return;

            e.Handled = true;

            IKeyboardHighlight keyboardHighlight = GetKeyboardHighlight(currentKeyboardHighlightPane, currentKeyboardHighlightSection);

            if (keyboardHighlight == null)
                return;

            if (e.Key == System.Windows.Input.Key.Up)
                keyboardHighlight.MoveKeyboardHighlight(-1);
            else if (e.Key == System.Windows.Input.Key.Down)
                keyboardHighlight.MoveKeyboardHighlight(1);
            else if (e.Key == System.Windows.Input.Key.Space)
                keyboardHighlight.ToggleKeyboardHighlightedEdgeSelection();
            else
                HandleKeyboardHighlightEnter(currentKeyboardHighlightPane, currentKeyboardHighlightSection, keyboardHighlight.KeyboardHighlightedEdge);
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

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NewVertexButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NewEdgeButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MasterToDetailButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DetailToMasterButton_Click(object sender, RoutedEventArgs e)
        {
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

            IKeyboardHighlight inEdgesKeyboardHighlight = GetKeyboardHighlight(pane, KeyboardHighlightSection.InEdges);

            if (inEdgesKeyboardHighlight != null)
            {
                currentKeyboardHighlightSection = KeyboardHighlightSection.InEdges;
                SetKeyboardHighlightPosition(pane, KeyboardHighlightSection.InEdges, true);
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
            currentKeyboardHighlightSection = KeyboardHighlightSection.InEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.InEdges, true);
        }

        private void LeftInEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.OutEdges, true);
        }

        private void LeftOutEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.InEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.InEdges, false);
        }

        private void LeftOutEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Left;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Left, KeyboardHighlightSection.OutEdges, false);
        }

        private void RightInEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.InEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.InEdges, true);
        }

        private void RightInEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.OutEdges, true);
        }

        private void RightOutEdgesKeyboardHighlight_GoneBeforeFirstPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.InEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.InEdges, false);
        }

        private void RightOutEdgesKeyboardHighlight_GoneAfterLastPosition(object sender, System.EventArgs e)
        {
            currentKeyboardHighlightPane = KeyboardHighlightPane.Right;
            currentKeyboardHighlightSection = KeyboardHighlightSection.OutEdges;
            SetKeyboardHighlightPosition(KeyboardHighlightPane.Right, KeyboardHighlightSection.OutEdges, false);
        }

        private void LeftInEdgesKeyboardHighlight_KeyboardHighlightEnterPressed(object sender, System.EventArgs e)
        {
            HandleKeyboardHighlightEnter(KeyboardHighlightPane.Left, KeyboardHighlightSection.InEdges, leftInEdgesKeyboardHighlight?.KeyboardHighlightedEdge);
        }

        private void RightInEdgesKeyboardHighlight_KeyboardHighlightEnterPressed(object sender, System.EventArgs e)
        {
            HandleKeyboardHighlightEnter(KeyboardHighlightPane.Right, KeyboardHighlightSection.InEdges, rightInEdgesKeyboardHighlight?.KeyboardHighlightedEdge);
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

            IVertex newBaseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(newBaseEdge);

            if (pane == KeyboardHighlightPane.Left)
            {
                LeftBaseEdge = newBaseEdgeVertex;
                SetCodeControlQueryText(LeftQueryStringCodeControl, LeftBaseEdge);
                RecreateLeftInEdgesVisualiser();
                RecreateLeftOutEdgesVisualiser();
            }
            else
            {
                RightBaseEdge = newBaseEdgeVertex;
                SetCodeControlQueryText(RightQueryStringCodeControl, RightBaseEdge);
                RecreateRightInEdgesVisualiser();
                RecreateRightOutEdgesVisualiser();
            }

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
        }

        private IKeyboardHighlight GetKeyboardHighlight(KeyboardHighlightPane pane, KeyboardHighlightSection section)
        {
            if (pane == KeyboardHighlightPane.Left)
                return section == KeyboardHighlightSection.InEdges
                    ? leftInEdgesKeyboardHighlight
                    : leftOutEdgesKeyboardHighlight;

            return section == KeyboardHighlightSection.InEdges
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
                host = section == KeyboardHighlightSection.InEdges
                    ? LeftInEdgesVisualiserHost
                    : LeftOutEdgesVisualiserHost;
            else
                host = section == KeyboardHighlightSection.InEdges
                    ? RightInEdgesVisualiserHost
                    : RightOutEdgesVisualiserHost;

            DataGrid dataGrid = FindVisualChild<DataGrid>(host);

            if (dataGrid != null)
                dataGrid.Focus();
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
            EventHandler keyboardHighlightEnterPressedHandler,
            ref IKeyboardHighlight keyboardHighlight,
            ref IVertex visualiserInstance)
        {
            DisposeVisualiser(
                visualiserHost,
                wrapVisualiserHost,
                goneBeforeFirstPositionHandler,
                goneAfterLastPositionHandler,
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
                    visualiserAsVisualiser.SelectionProphibited = isInEdgesVisualiser;
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

        private static void SetKeyboardHighlight(
            object visualiser,
            EventHandler goneBeforeFirstPositionHandler,
            EventHandler goneAfterLastPositionHandler,
            EventHandler keyboardHighlightEnterPressedHandler,
            ref IKeyboardHighlight keyboardHighlight)
        {
            keyboardHighlight = visualiser as IKeyboardHighlight;

            if (keyboardHighlight == null)
                return;

            keyboardHighlight.GoneBeforeFirstPosition += goneBeforeFirstPositionHandler;
            keyboardHighlight.GoneAfterLastPosition += goneAfterLastPositionHandler;
            keyboardHighlight.KeyboardHighlightEnterPressed += keyboardHighlightEnterPressedHandler;
        }

        private static void DisposeVisualiser(
            ContentControl visualiserHost,
            ContentControl wrapVisualiserHost,
            EventHandler goneBeforeFirstPositionHandler,
            EventHandler goneAfterLastPositionHandler,
            EventHandler keyboardHighlightEnterPressedHandler,
            ref IKeyboardHighlight keyboardHighlight,
            ref IVertex visualiserInstance)
        {
            if (keyboardHighlight != null)
            {
                keyboardHighlight.GoneBeforeFirstPosition -= goneBeforeFirstPositionHandler;
                keyboardHighlight.GoneAfterLastPosition -= goneAfterLastPositionHandler;
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

            if (!(visualiser is GraphVisualiser) && !(visualiser is GraphVisualiser3D))
                return;

            double viewportWidth = sectionScrollViewer.ViewportWidth;
            double viewportHeight = sectionScrollViewer.ViewportHeight;

            if (double.IsNaN(viewportWidth) || viewportWidth <= 0)
                viewportWidth = sectionScrollViewer.ActualWidth;

            if (double.IsNaN(viewportHeight) || viewportHeight <= 0)
                viewportHeight = sectionScrollViewer.ActualHeight;

            frameworkElement.Width = System.Math.Max(100, viewportWidth - 4);
            frameworkElement.Height = System.Math.Max(100, viewportHeight - 40);
        }
    }
}
