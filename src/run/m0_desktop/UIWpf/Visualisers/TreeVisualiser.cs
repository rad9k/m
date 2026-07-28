using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using m0.Foundation;
using System.Collections.ObjectModel;
using System.Windows;
using m0.Util;
using m0.ZeroUML;
using m0.ZeroTypes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using System.Collections;
using m0.UIWpf.Commands;
using m0.UIWpf.Visualisers.Helper;
using m0.Graph.ExecutionFlow;
using static m0.Graph.ExecutionFlow.ExecutionFlowHelper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class TreeVisualiserViewItem : TreeViewItem, IDisposable
    {
        public bool doNotTrackGraphChanges = false;

        public IEdge vertexChangeListenerEdge;

        public TreeEdgeNode Node { get; set; }

        public static bool HideMetaNameIfEmpty = true;

        public bool IsFilled;        

        private bool ignoreNextMouseLeftButtonUp;
        private bool isExpandCollapseAnimationInProgress;
        private ToggleButton expanderToggleButton;
        private bool isKeyboardHighlighted;
        private bool isMouseHoverHighlighted;

        private void Select(bool IsCtrl)
        {
            IsSelected = true;

            ParentVisualiser.UpdateSelectedVertices(IsCtrl, this);
        }

        private void Unselect(bool IsCtrl)
        {
            IsSelected = false;

            ParentVisualiser.UpdateSelectedVertices(IsCtrl, this);
        }

        private bool _IsSelected;

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (_IsSelected == value)
                    return;

                _IsSelected = value;
                ApplyEdgeVisualState();
            }
        }

        public TreeVisualiser ParentVisualiser {get; set;}

        public bool IsMouseHoverHighlighted
        {
            get { return isMouseHoverHighlighted; }
            set
            {
                if (isMouseHoverHighlighted == value)
                    return;

                isMouseHoverHighlighted = value;
                ApplyEdgeVisualState();
            }
        }

        public bool IsKeyboardHighlighted
        {
            get { return isKeyboardHighlighted; }
            set
            {
                if (isKeyboardHighlighted == value)
                    return;

                isKeyboardHighlighted = value;
                ApplyEdgeVisualState();
            }
        }

        private void ApplyEdgeVisualState()
        {
            if (ParentVisualiser == null)
                return;

            MetaToEdgeControl headerControl = Header as MetaToEdgeControl;

            if (headerControl != null)
            {
                headerControl.ExternalBackgroundMode = true;
                headerControl.IsKeyboardHighlightedSelected = IsSelected && isKeyboardHighlighted;
                headerControl.IsSelected = _IsSelected;
                headerControl.IsHighlighted = isKeyboardHighlighted;
                headerControl.IsMouseHoverHighlighted = isMouseHoverHighlighted;
                headerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                headerControl.ClearValue(FrameworkElement.MarginProperty);
                headerControl.ClearValue(FrameworkElement.WidthProperty);
                headerControl.RefreshSelectionState();
            }

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ClearValue(Control.BackgroundProperty);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ParentVisualiser == null)
                return;

            Brush backgroundBrush = null;

            if (isKeyboardHighlighted)
                backgroundBrush = (Brush)FindResource("0HighlightBrush");
            else if (IsSelected)
                backgroundBrush = (Brush)FindResource("0SelectionBrush");

            if (backgroundBrush == null)
                return;

            FrameworkElement headerElement = Header as FrameworkElement;

            if (headerElement == null || headerElement.ActualHeight <= 0)
                return;

            double backgroundWidth = ParentVisualiser.GetFullWidthBackgroundWidth();

            if (backgroundWidth <= 0)
                return;

            Point itemPositionInTree;
            Point headerPositionInItem;

            try
            {
                itemPositionInTree = TranslatePoint(new Point(0, 0), ParentVisualiser);
                headerPositionInItem = headerElement.TranslatePoint(new Point(0, 0), this);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            Rect backgroundRect = new Rect(
                -itemPositionInTree.X,
                headerPositionInItem.Y,
                backgroundWidth,
                headerElement.ActualHeight);

            drawingContext.DrawRectangle(backgroundBrush, null, backgroundRect);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Focusable = false;
            FocusVisualStyle = null;

            if (expanderToggleButton != null)
            {
                expanderToggleButton.PreviewMouseLeftButtonDown -= ExpanderToggleButtonPreviewMouseLeftButtonDown;
                expanderToggleButton.MouseEnter -= ExpanderToggleButtonMouseEnter;
                expanderToggleButton.MouseLeave -= ExpanderToggleButtonMouseLeave;
            }

            expanderToggleButton = GetTemplateChild("Expander") as ToggleButton;

            if (expanderToggleButton != null)
            {
                expanderToggleButton.Focusable = false;
                expanderToggleButton.Width = 48;
                expanderToggleButton.Height = 16;
                expanderToggleButton.Margin = new Thickness(0, 0, -32, 0);
                expanderToggleButton.Template = TreeVisualiser.CreateFilledExpandCollapseToggleTemplate();
                expanderToggleButton.PreviewMouseLeftButtonDown += ExpanderToggleButtonPreviewMouseLeftButtonDown;
                expanderToggleButton.MouseEnter += ExpanderToggleButtonMouseEnter;
                expanderToggleButton.MouseLeave += ExpanderToggleButtonMouseLeave;
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            base.IsSelected = false;
            e.Handled = true;
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            e.Handled = true;
        }

        private void ExpanderToggleButtonMouseEnter(object sender, MouseEventArgs e)
        {
            SetHeaderHighlight(true);
        }

        private void ExpanderToggleButtonMouseLeave(object sender, MouseEventArgs e)
        {
            SetHeaderHighlight(false);
        }

        private void ExpanderToggleButtonPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleExpandedWithAnimation();
            ignoreNextMouseLeftButtonUp = true;
            e.Handled = true;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (ParentVisualiser.ActivateKeyboardHighlightItem(this))
                {
                    ignoreNextMouseLeftButtonUp = true;
                    e.Handled = true;
                    return;
                }

                ignoreNextMouseLeftButtonUp = true;
                BaseCommands.OpenDefaultVisualiser(EdgeHelper.CreateTempEdgeVertex(GetEdge()), false);
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs a)
        {
            Point position = a.GetPosition(this);
            bool inExpandArea = IsInExpandCollapseClickArea(position);

            if (inExpandArea)
            {
                ToggleExpandedWithAnimation();
                ignoreNextMouseLeftButtonUp = true;
                a.Handled = true;
                return;
            }

            a.Handled = true;

            ignoreNextMouseLeftButtonUp = false;
            ParentVisualiser.RegisterPendingMouseDownFromItem(this);
        }

        IEdge GetEdge()
        {
            if (Node != null)
                return Node.Edge;

            return (IEdge)Tag;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs a)
        {
            if (ignoreNextMouseLeftButtonUp)
            {
                ignoreNextMouseLeftButtonUp = false;
                a.Handled = true;
                return;
            }

            ParentVisualiser.TryApplyPendingMouseClickFromItem(this);

            a.Handled = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs a)
        {
            if (IsInExpandCollapseClickArea(a.GetPosition(this)))
                return;

            ParentVisualiser.TryApplyContextMenuSelectionFromItem(this);
            a.Handled = true;
        }

        private bool IsInExpandCollapseClickArea(Point position)
        {
            if (HasItems == false)
                return false;

            FrameworkElement headerElement = Header as FrameworkElement;

            if (headerElement == null || headerElement.IsVisible == false)
                return false;

            Point headerPosition = headerElement.TranslatePoint(new Point(0, 0), this);
            double headerBottom = headerPosition.Y + headerElement.ActualHeight;

            return position.X >= 0 &&
                position.X < headerPosition.X &&
                position.Y >= headerPosition.Y &&
                position.Y <= headerBottom;
        }

        void Fill()
        {
            if (Node != null && ParentVisualiser.UseDataVirtualization)
            {
                ParentVisualiser.FillVirtualNode(this);
                return;
            }

            TreeVisualiser.ClearAllItems_Reccurent(this);

            IEnumerable<IEdge> filteredList = VisualiserUtil.FilterEdges(GetEdge().To, ParentVisualiser.Vertex);

            foreach (IEdge ee in filteredList)
                Items.Add(ParentVisualiser.CreateTreeViewItem(ee, true, this));
        }

        protected override void OnExpanded(RoutedEventArgs ea)
        {
            if (IsFilled == false)
                Fill();

            IsFilled = true;

            if (ParentVisualiser.UseDataVirtualization)
                return;

            if (isExpandCollapseAnimationInProgress == false)
                BeginExpandAnimation();
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            ResetChildItemAnimations();
        }

        private void ToggleExpandedWithAnimation()
        {
            if (HasItems == false || isExpandCollapseAnimationInProgress)
                return;

            if (ParentVisualiser.UseDataVirtualization)
            {
                IsExpanded = !IsExpanded;
                return;
            }

            isExpandCollapseAnimationInProgress = true;

            if (IsExpanded)
            {
                AnimateChildItems(false, delegate
                {
                    IsExpanded = false;
                    ResetChildItemAnimations();
                    isExpandCollapseAnimationInProgress = false;
                });
            }
            else
            {
                IsExpanded = true;
                BeginExpandAnimation();
            }
        }

        private void BeginExpandAnimation()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                AnimateChildItems(true, delegate
                {
                    isExpandCollapseAnimationInProgress = false;
                });
            }), DispatcherPriority.Render);
        }

        private void AnimateChildItems(bool expand, EventHandler completed)
        {
            List<FrameworkElement> childElements = Items
                .OfType<FrameworkElement>()
                .ToList();

            if (childElements.Count == 0)
            {
                completed?.Invoke(this, EventArgs.Empty);
                return;
            }

            TimeSpan duration = TimeSpan.FromMilliseconds(expand ? 85 : 65);
            IEasingFunction easingFunction = new QuadraticEase { EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn };
            double startOpacity = expand ? 0.65 : 1;
            double endOpacity = expand ? 1 : 0;
            double startTranslateY = expand ? -5 : 0;
            double endTranslateY = expand ? 0 : -5;

            for (int index = 0; index < childElements.Count; index++)
            {
                FrameworkElement childElement = childElements[index];
                TranslateTransform translateTransform = childElement.RenderTransform as TranslateTransform;

                if (translateTransform == null)
                {
                    translateTransform = new TranslateTransform();
                    childElement.RenderTransform = translateTransform;
                }

                childElement.BeginAnimation(OpacityProperty, null);
                translateTransform.BeginAnimation(TranslateTransform.YProperty, null);

                childElement.Opacity = startOpacity;
                translateTransform.Y = startTranslateY;

                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = startOpacity,
                    To = endOpacity,
                    Duration = new Duration(duration),
                    EasingFunction = easingFunction,
                    FillBehavior = FillBehavior.Stop
                };

                DoubleAnimation translateAnimation = new DoubleAnimation
                {
                    From = startTranslateY,
                    To = endTranslateY,
                    Duration = new Duration(duration),
                    EasingFunction = easingFunction,
                    FillBehavior = FillBehavior.Stop
                };

                if (index == childElements.Count - 1)
                    opacityAnimation.Completed += delegate
                    {
                        CompleteChildItemAnimations(childElements, endOpacity, endTranslateY);
                        completed?.Invoke(this, EventArgs.Empty);
                    };

                childElement.BeginAnimation(OpacityProperty, opacityAnimation);
                translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            }
        }

        private void CompleteChildItemAnimations(List<FrameworkElement> childElements, double opacity, double translateY)
        {
            foreach (FrameworkElement childElement in childElements)
            {
                childElement.BeginAnimation(OpacityProperty, null);
                childElement.Opacity = opacity;

                TranslateTransform translateTransform = childElement.RenderTransform as TranslateTransform;

                if (translateTransform != null)
                {
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                    translateTransform.Y = translateY;
                }
            }
        }

        private void ResetChildItemAnimations()
        {
            foreach (FrameworkElement childElement in Items.OfType<FrameworkElement>())
            {
                childElement.BeginAnimation(OpacityProperty, null);
                childElement.Opacity = 1;

                TranslateTransform translateTransform = childElement.RenderTransform as TranslateTransform;

                if (translateTransform != null)
                {
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                    translateTransform.Y = 0;
                }
            }
        }

        public void UpdateHeader()
        {
            MetaToEdgeControl headerControl = Header as MetaToEdgeControl;

            if (headerControl == null)
            {
                headerControl = new MetaToEdgeControl();
                headerControl.MouseEnter += HeaderControlMouseEnter;
                headerControl.MouseLeave += HeaderControlMouseLeave;
                Header = headerControl;
            }

            headerControl.UpdateEdgeAndIcon(GetEdge(), ParentVisualiser.ShowIcons);
            ApplyEdgeVisualState();
        }

        private void HeaderControlMouseEnter(object sender, MouseEventArgs e)
        {
            SetHeaderHighlight(true);
        }

        private void HeaderControlMouseLeave(object sender, MouseEventArgs e)
        {
            SetHeaderHighlight(false);
        }

        public void SetHeaderHighlight(bool isHighlighted)
        {
            IsMouseHoverHighlighted = isHighlighted;
        }

        public INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
           // ExecutionFlowHelper.DebugStackStraceAsEvents(exe.Stack);

            if (ParentVisualiser.VisualiserHelper.IsDisposed)
                return exe.Stack;

            if (GetEdge().To.DisposedState != DisposeStateEnum.Live)
                return exe.Stack;

            if (Node != null && ParentVisualiser.UseDataVirtualization)
                return ParentVisualiser.VirtualItemVertexChange(this, exe);

            UpdateHeader();
            Fill();

            return exe.Stack;

            // instead of this
            // NEED TO DO:
            // exe.Stack.GetAll(false, @"event:\Type:OutputEdgeAdded")
            // exe.Stack.GetAll(false, @"event:\Type:OutputEdgeRemoved")
            // exe.Stack.GetAll(false, @"event:\Type:OutputEdgeDisposed")
            /*
            IVertex edgeVertex = exe.Stack.Get(false, @"event:\Edge:");

            if (edgeVertex != null)
            {           
                IVertex eventType = exe.Stack.Get(false, @"event:\Type:");

                if (eventType != null)
                {
                    if (GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeAdded"))
                    {
                        //EdgeAdded(Edge.CreateIEdgeFromEdgeVertex(edgeVertex));
                        Fill();
                        return exe.Stack;
                    }

                    if (GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeRemoved"))
                    {
                        //EdgeRemoved(Edge.CreateIEdgeFromEdgeVertex(edgeVertex));
                        Fill();
                        return exe.Stack;
                    }

                    if (GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeDisposed"))
                    {
                        //EdgeDisposed(Edge.CreateIEdgeFromEdgeVertex(edgeVertex));
                        Fill(); 
                        return exe.Stack;
                    }
                    
                }
            }

            UpdateHeader();

            return exe.Stack;*/
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeVisualiserViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeVisualiserViewItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            TreeVisualiserViewItem childItem = element as TreeVisualiserViewItem;
            TreeEdgeNode childNode = item as TreeEdgeNode;

            if (childItem != null && childNode != null && ParentVisualiser != null)
                ParentVisualiser.PrepareVirtualTreeViewItem(childNode, childItem);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            TreeVisualiserViewItem childItem = element as TreeVisualiserViewItem;

            if (childItem != null && ParentVisualiser != null)
                ParentVisualiser.ClearVirtualTreeViewItem(childItem);

            base.ClearContainerForItemOverride(element, item);
        }

        private void EdgeRemoved(IEdge edge)
        {
            if (IsFilled)
            {
                IList l = GeneralUtil.CreateAndCopyList(Items);
                foreach (TreeVisualiserViewItem i in l)
                    if (EdgeHelper.CompareIEdges(((IEdge)i.Tag), edge))
                        Items.Remove(i);
            }
        }

        private void EdgeAdded(IEdge edge)
        {
            if (!IsFilled)                        
                TreeVisualiser.ClearAllItems_Reccurent(this);            

            Items.Add(ParentVisualiser.CreateTreeViewItem(edge, true, this));
        }


        private bool IsDisposed;

        public void Dispose()
        {
            if (!IsDisposed)
            {                
                if(vertexChangeListenerEdge != null)
                    ExecutionFlowHelper.RemoveGraphChangeListener(vertexChangeListenerEdge);

                IsDisposed = true;
            }
        }
    }

    public class TreeVisualiser: TreeView, IListVisualiser, IHasSelectableEdges, ITypedEdge, IKeyboardHighlight
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProhibited { get; set; }

        private bool fullWidthSelectionHighlight = true;
        private readonly ObservableCollection<TreeEdgeNode> virtualRootNodes =
            new ObservableCollection<TreeEdgeNode>();
        private readonly Dictionary<TreeEdgeNode, TreeVisualiserViewItem> virtualContainers =
            new Dictionary<TreeEdgeNode, TreeVisualiserViewItem>();
        private bool useDataVirtualization = true;

        public bool UseDataVirtualization
        {
            get { return useDataVirtualization; }
            set
            {
                if (useDataVirtualization == value)
                    return;

                useDataVirtualization = value;

                ApplyDataVirtualizationSettings();
            }
        }

        private void ApplyDataVirtualizationSettings()
        {
            if (useDataVirtualization)
                ItemsPanel = new ItemsPanelTemplate(
                    new FrameworkElementFactory(typeof(VirtualizingStackPanel)));

            VirtualizingStackPanel.SetIsVirtualizing(this, useDataVirtualization);
            VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            ScrollViewer.SetCanContentScroll(this, useDataVirtualization);
        }

        public bool FullWidthSelectionHighlight
        {
            get { return fullWidthSelectionHighlight; }
            set
            {
                fullWidthSelectionHighlight = value;
                ApplyFullWidthSelectionHighlightToAllItems();
                ScheduleFullWidthVisualRefresh();
            }
        }

        private TreeVisualiserViewItem keyboardHighlightedItem;
        private bool isBeforeFirstKeyboardPosition;
        private bool isAfterLastKeyboardPosition;


        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

        private TreeVisualiserViewItem pendingMouseDownItem;
        private IEdge pendingMouseDownEdge;
        private bool pendingMouseDownIsCtrl;
        private bool pendingWasInSelectionAtMouseDown;
        private bool suppressNextMouseUpSelection;

        public TreeVisualiser() : this(null, null, false) { }


        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowIcons" };
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

        public int CurrentHighlightPosition
        {
            get
            {
                List<TreeVisualiserViewItem> items = GetKeyboardHighlightItems();

                if (keyboardHighlightedItem == null)
                    return -1;

                return items.IndexOf(keyboardHighlightedItem);
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
                List<TreeVisualiserViewItem> items = GetKeyboardHighlightItems();
                return items.Count > 0 && CurrentHighlightPosition == items.Count - 1 && !isBeforeFirstKeyboardPosition && !isAfterLastKeyboardPosition;
            }
            set { if (value) SetKeyboardHighlightToLast(); }
        }

        public bool CanGoBeforeFirstPosition { get { return true; } }

        public bool CanGoAfterLastPosition { get { return true; } }

        public bool HasKeyboardHighlightItems
        {
            get { return GetKeyboardHighlightItems().Count > 0; }
        }

        public bool IsVertexCommanderKeyboardHighlightEnabled { get; set; }

        public IEdge KeyboardHighlightedEdge
        {
            get
            {
                if (keyboardHighlightedItem == null)
                    return null;

                return keyboardHighlightedItem.Tag as IEdge;
            }
        }

        public event EventHandler KeyboardHighlightActivated;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        public bool ShowIcons
        {
            get { return GraphUtil.GetBooleanValueOrFalse(Vertex.Get(false, "ShowIcons:")); }
        }

        // TypedEdge START

        public TreeVisualiser(IEdge _edge)
        {
            ApplyDataVirtualizationSettings();

            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public TreeVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            MinusZero mz = MinusZero.Instance;

            this.Foreground = (Brush)FindResource("0ForegroundBrush");
            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.BorderThickness = new Thickness(0);
            this.Padding = new Thickness(0);
            this.AllowDrop = true;
            this.SizeChanged += TreeVisualiser_SizeChanged;
            ApplyDataVirtualizationSettings();

            if (mz != null && mz.IsInitialized)
            {
                new ListVisualiserHelper(parentVisualiser,
                    isVolatile,
                    MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Tree"),
                    this,
                    "TreeVisualiser",
                    this,
                    true,
                    new List<string> { @"", @"BaseEdge:\To:" },
                    "AtomVisualiserFull",
                    baseEdgeVertex,
                    UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

                ((ListVisualiserHelper)VisualiserHelper).CustomVertexChangeEvent += CustomVertexChange;

                SetVertexDefaultValues();
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeVisualiserViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeVisualiserViewItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            TreeEdgeNode node = item as TreeEdgeNode;
            TreeVisualiserViewItem treeItem = element as TreeVisualiserViewItem;

            if (UseDataVirtualization && node != null && treeItem != null)
                PrepareVirtualTreeViewItem(node, treeItem);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            TreeVisualiserViewItem treeItem = element as TreeVisualiserViewItem;

            if (UseDataVirtualization && treeItem != null)
                ClearVirtualTreeViewItem(treeItem);

            base.ClearContainerForItemOverride(element, item);
        }

        internal void PrepareVirtualTreeViewItem(TreeEdgeNode node, TreeVisualiserViewItem treeItem)
        {
            ClearVirtualTreeViewItem(treeItem);

            treeItem.Node = node;
            treeItem.Tag = node.Edge;
            treeItem.ParentVisualiser = this;
            treeItem.doNotTrackGraphChanges = node.DoNotTrackGraphChanges;
            treeItem.IsFilled = node.ChildrenLoaded;
            treeItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            treeItem.IsSelected = IsEdgeSelected(node.Edge);
            treeItem.UpdateHeader();

            virtualContainers[node] = treeItem;

            if (node.ChildrenLoaded)
            {
                treeItem.ItemsSource = node.Children;
            }
            else if (node.HasChildren)
            {
                treeItem.Items.Add(new TreeViewItem());
            }

            if (!treeItem.doNotTrackGraphChanges)
            {
                treeItem.vertexChangeListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(node.Edge.To,
                    new List<string> { },
                    new List<GraphChangeFilterEnum> {
                        GraphChangeFilterEnum.ValueChange,
                        GraphChangeFilterEnum.OutputEdgeAdded,
                        GraphChangeFilterEnum.OutputEdgeRemoved,
                        GraphChangeFilterEnum.OutputEdgeDisposed
                    },
                    "VirtualTreeViewItem",
                    treeItem.VertexChange);
            }
        }

        internal void ClearVirtualTreeViewItem(TreeVisualiserViewItem treeItem)
        {
            if (treeItem == null)
                return;

            if (treeItem.vertexChangeListenerEdge != null)
            {
                ExecutionFlowHelper.RemoveGraphChangeListener(treeItem.vertexChangeListenerEdge);
                treeItem.vertexChangeListenerEdge = null;
            }

            if (treeItem.Node != null)
                virtualContainers.Remove(treeItem.Node);

            treeItem.ItemsSource = null;
            treeItem.Items.Clear();
            treeItem.Node = null;
            treeItem.Tag = null;
            treeItem.IsFilled = false;
            treeItem.IsSelected = false;
            treeItem.IsKeyboardHighlighted = false;
        }

        internal void FillVirtualNode(TreeVisualiserViewItem treeItem)
        {
            if (treeItem == null || treeItem.Node == null)
                return;

            TreeEdgeNode node = treeItem.Node;
            LoadVirtualChildren(node, true);

            treeItem.ItemsSource = null;
            treeItem.Items.Clear();
            treeItem.ItemsSource = node.Children;
            treeItem.IsFilled = true;
        }

        internal INoInEdgeInOutVertexVertex VirtualItemVertexChange(
            TreeVisualiserViewItem treeItem,
            IExecution exe)
        {
            if (treeItem == null || treeItem.Node == null)
                return exe.Stack;

            treeItem.UpdateHeader();

            if (treeItem.Node.ChildrenLoaded)
                LoadVirtualChildren(treeItem.Node, true);

            return exe.Stack;
        }

        private int LoadVirtualChildren(TreeEdgeNode node, bool reload)
        {
            if (node == null || node.Edge == null || node.Edge.To == null)
                return 0;

            if (node.ChildrenLoaded && !reload)
                return node.Children.Count;

            IEnumerable<IEdge> filteredEdges = VisualiserUtil.FilterEdges(node.Edge.To, Vertex);

            node.Children.Clear();

            foreach (IEdge edge in filteredEdges)
                node.Children.Add(CreateVirtualNode(edge, node));

            node.ChildrenLoaded = true;
            node.UpdateHasChildren(node.Children.Count > 0);

            return node.Children.Count;
        }

        private TreeEdgeNode CreateVirtualNode(IEdge edge, TreeEdgeNode parent)
        {
            bool doNotTrackGraphChanges = parent != null && parent.DoNotTrackGraphChanges;

            if (edge.Meta != null && GeneralUtil.CompareStrings(edge.Meta.Value, "$GraphChangeTrigger"))
                doNotTrackGraphChanges = true;

            if (edge.Meta != null && GeneralUtil.CompareStrings(edge.Meta.Value, "FormalTextLanguage"))
                doNotTrackGraphChanges = true;

            bool hasChildren = edge.To != null && edge.To.Count() > 0;

            return new TreeEdgeNode(edge, parent, hasChildren, doNotTrackGraphChanges);
        }

        private bool IsEdgeSelected(IEdge edge)
        {
            IVertex selectedEdges = Vertex.Get(false, @"SelectedEdges:");

            return selectedEdges != null
                && EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, edge) != null;
        }

        public static ControlTemplate CreateFilledExpandCollapseToggleTemplate()
        {
            const double expandedTriangleTranslateX = -1.75;

            ControlTemplate template = new ControlTemplate(typeof(ToggleButton));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            grid.SetValue(Panel.BackgroundProperty, Brushes.Transparent);

            FrameworkElementFactory triangle = new FrameworkElementFactory(typeof(Path));
            triangle.Name = "ExpandPath";
            triangle.SetValue(Path.DataProperty, Geometry.Parse("M 0 0 L 4.5 3.5 L 0 7 Z"));
            triangle.SetValue(Path.StretchProperty, Stretch.None);
            triangle.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            triangle.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);
            triangle.SetValue(FrameworkElement.MarginProperty, new Thickness(6, 0, 0, 0));
            triangle.SetValue(RenderTransformOriginProperty, new Point(0.5, 0.5));
            TransformGroup triangleTransform = new TransformGroup();
            triangleTransform.Children.Add(new RotateTransform(0));
            triangleTransform.Children.Add(new TranslateTransform(0, 0));
            triangle.SetValue(RenderTransformProperty, triangleTransform);
            triangle.SetResourceReference(Path.FillProperty, "0GrayBrush");
            triangle.SetResourceReference(Path.StrokeProperty, "0GrayBrush");

            grid.AppendChild(triangle);
            template.VisualTree = grid;

            Trigger expandedTrigger = new Trigger();
            expandedTrigger.Property = ToggleButton.IsCheckedProperty;
            expandedTrigger.Value = true;
            expandedTrigger.EnterActions.Add(new BeginStoryboard { Storyboard = CreateTriangleRotationStoryboard(90, expandedTriangleTranslateX) });
            expandedTrigger.ExitActions.Add(new BeginStoryboard { Storyboard = CreateTriangleRotationStoryboard(0, 0) });
            template.Triggers.Add(expandedTrigger);

            Trigger disabledTrigger = new Trigger();
            disabledTrigger.Property = IsEnabledProperty;
            disabledTrigger.Value = false;
            disabledTrigger.Setters.Add(new Setter(Path.FillProperty, Brushes.Transparent, "ExpandPath"));
            disabledTrigger.Setters.Add(new Setter(Path.StrokeProperty, Brushes.Transparent, "ExpandPath"));
            template.Triggers.Add(disabledTrigger);

            return template;
        }

        private static Storyboard CreateTriangleRotationStoryboard(double angle, double translateX)
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation(angle, new Duration(TimeSpan.FromMilliseconds(80)));
            rotationAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            Storyboard.SetTargetName(rotationAnimation, "ExpandPath");
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)"));

            DoubleAnimation translateAnimation = new DoubleAnimation(translateX, new Duration(TimeSpan.FromMilliseconds(80)));
            translateAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            Storyboard.SetTargetName(translateAnimation, "ExpandPath");
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(rotationAnimation);
            storyboard.Children.Add(translateAnimation);

            return storyboard;
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void BaseEdgeToUpdated()
        {
            if (UseDataVirtualization)
            {
                BaseEdgeToUpdatedVirtualized();
                return;
            }

            UnselectAllSelectedEdges();

            ClearAllItems();

            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (bas != null)
            {
                IEnumerable<IEdge> filteredList = VisualiserUtil.FilterEdges(bas, Vertex);

                foreach (IEdge e in filteredList)
                    Items.Add(CreateTreeViewItem(e, true, null));
            }

            // The TreeVisualiser owns a direct graph-change trigger registered straight on the
            // current root_tree_base vertex (BaseEdge:\To:). When BaseEdge:\To: is rebound to a
            // different vertex (initial bind from the helper, or a later BaseEdge change), we
            // must re-register so that the trigger always observes the currently visualised
            // root vertex.
            RegisterRootTreeBaseListener();
            ScheduleFullWidthVisualRefresh();
        }

        private void BaseEdgeToUpdatedVirtualized()
        {
            UnselectAllSelectedEdges();
            ClearVirtualTreeModel();

            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex != null)
            {
                foreach (IEdge edge in VisualiserUtil.FilterEdges(baseVertex, Vertex))
                    virtualRootNodes.Add(CreateVirtualNode(edge, null));
            }

            ItemsSource = virtualRootNodes;

            RegisterRootTreeBaseListener();

            ScheduleFullWidthVisualRefresh();
        }

        private void ClearVirtualTreeModel()
        {
            foreach (TreeVisualiserViewItem treeItem in virtualContainers.Values.ToList())
                ClearVirtualTreeViewItem(treeItem);

            virtualContainers.Clear();
            ItemsSource = null;
            virtualRootNodes.Clear();
        }

        internal double GetFullWidthBackgroundWidth()
        {
            if (ActualWidth > 0)
                return ActualWidth;

            if (RenderSize.Width > 0)
                return RenderSize.Width;

            return 0;
        }

        private void TreeVisualiser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!fullWidthSelectionHighlight || e.NewSize.Width <= 0)
                return;

            InvalidateFullWidthItemVisuals();
        }

        private void ScheduleFullWidthVisualRefresh()
        {
            if (!fullWidthSelectionHighlight)
                return;

            Dispatcher.BeginInvoke(new Action(RefreshFullWidthItemVisuals), DispatcherPriority.Loaded);
        }

        private void RefreshFullWidthItemVisuals()
        {
            if (!fullWidthSelectionHighlight)
                return;

            ApplyFullWidthSelectionHighlightToAllItems();
            InvalidateFullWidthItemVisuals();
        }

        private void InvalidateFullWidthItemVisuals()
        {
            if (UseDataVirtualization)
            {
                foreach (TreeVisualiserViewItem treeItem in virtualContainers.Values)
                    treeItem.InvalidateVisual();

                return;
            }

            InvalidateFullWidthItemVisuals(Items);
        }

        private void InvalidateFullWidthItemVisuals(ItemCollection items)
        {
            foreach (object item in items)
            {
                TreeVisualiserViewItem treeItem = item as TreeVisualiserViewItem;

                if (treeItem == null)
                    continue;

                treeItem.InvalidateVisual();
                InvalidateFullWidthItemVisuals(treeItem.Items);
            }
        }

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        protected INoInEdgeInOutVertexVertex CustomVertexChange(IExecution exe)
        {
            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Scale"))
                ScaleChange();

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "ShowIcons"))
                UpdateShowIconOnAllItems();

            if (IsEdgeAddedRemovedDiscardedFrom(exe.Stack, Vertex.Get(false, @"SelectedEdges:")))
                SelectedVerticesUpdated();

            if (IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:")))
            {
                BaseEdgeToUpdated();
                return exe.Stack;
            }

            // Incremental add/remove/dispose of children of root_tree_base is handled by the
            // dedicated direct trigger (RootTreeBaseVertexChange), so it is intentionally not
            // dispatched from here anymore.

            return exe.Stack;
        }

        // Direct graph-change listener registered straight on the current root_tree_base
        // (BaseEdge:\To:) vertex. This complements the helper-driven CustomVertexChange path
        // and guarantees that incremental Add/Remove/Dispose of root-level children always
        // reach the tree, even in scenarios where the helper-based trigger does not fire
        // (e.g. drag-and-drop of an unselected top-level item with CopyOnDragAndDrop=False).
        private IEdge rootTreeBaseListenerEdge;
        private IVertex rootTreeBaseListenerVertex;

        private void RegisterRootTreeBaseListener()
        {
            IVertex newRootTreeBase = Vertex == null ? null : Vertex.Get(false, @"BaseEdge:\To:");

            if (rootTreeBaseListenerEdge != null && rootTreeBaseListenerVertex == newRootTreeBase)
                return;

            UnregisterRootTreeBaseListener();

            if (newRootTreeBase == null || newRootTreeBase.DisposedState != DisposeStateEnum.Live)
                return;

            rootTreeBaseListenerVertex = newRootTreeBase;

            rootTreeBaseListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(newRootTreeBase,
                new List<string> { },
                new List<GraphChangeFilterEnum> {
                    GraphChangeFilterEnum.OutputEdgeAdded,
                    GraphChangeFilterEnum.OutputEdgeRemoved,
                    GraphChangeFilterEnum.OutputEdgeDisposed
                },
                "TreeVisualiserRootTreeBase",
                RootTreeBaseVertexChange);
        }

        private void UnregisterRootTreeBaseListener()
        {
            if (rootTreeBaseListenerEdge != null)
            {
                ExecutionFlowHelper.RemoveGraphChangeListener(rootTreeBaseListenerEdge);
                rootTreeBaseListenerEdge = null;
            }

            rootTreeBaseListenerVertex = null;
        }

        private INoInEdgeInOutVertexVertex RootTreeBaseVertexChange(IExecution exe)
        {
            if (VisualiserHelper.IsDisposed)
                return exe.Stack;

            foreach (IEdge eventEdge in exe.Stack.GetAll(false, @"event:"))
            {
                IVertex eventType = eventEdge.To.Get(false, @"Type:");
                IVertex edgeVertex = GraphUtil.GetQueryOutFirst(eventEdge.To, "Edge", null);

                if (eventType == null || edgeVertex == null || eventType.Value == null)
                    continue;

                IEdge edge = EdgeHelper.CreateIEdgeFromEdgeVertex(edgeVertex);

                switch (eventType.Value.ToString())
                {
                    case "OutputEdgeAdded":
                        EdgeAdded(edge);
                        break;

                    case "OutputEdgeRemoved":
                        EdgeRemoved(edge);
                        break;

                    case "OutputEdgeDisposed":
                        EdgeDisposed(edge);
                        break;
                }
            }

            return exe.Stack;
        }

        private void EdgeRemoved(IEdge edge)
        {
            if (UseDataVirtualization)
            {
                TreeEdgeNode node = virtualRootNodes.FirstOrDefault(
                    currentNode => EdgeHelper.CompareIEdges(currentNode.Edge, edge));

                if (node != null)
                    virtualRootNodes.Remove(node);

                return;
            }

            IList l = GeneralUtil.CreateAndCopyList(Items);

            foreach (TreeVisualiserViewItem i in l)
                if (EdgeHelper.CompareIEdges((IEdge)i.Tag, edge))
                    Items.Remove(i);
        }

        private void EdgeAdded(IEdge edge)
        {
            if (UseDataVirtualization)
            {
                virtualRootNodes.Add(CreateVirtualNode(edge, null));
                return;
            }

            Items.Add(CreateTreeViewItem(edge, true, null));
        }

        private void EdgeDisposed(IEdge edge)
        {
            BaseEdgeToUpdated();
        }

        private void UpdateShowIconOnAllItems()
        {
            if (UseDataVirtualization)
            {
                foreach (TreeVisualiserViewItem item in virtualContainers.Values.ToList())
                    item.UpdateHeader();

                return;
            }

            UpdateShowIconOnItems(Items, ShowIcons);
        }

        private void UpdateShowIconOnItems(ItemCollection items, bool showIcons)
        {
            foreach (TreeViewItem item in items)
            {
                MetaToEdgeControl headerControl = item.Header as MetaToEdgeControl;

                if (headerControl != null)
                    headerControl.ShowIcon = showIcons;

                UpdateShowIconOnItems(item.Items, showIcons);
            }
        }

        public void SelectedVerticesUpdated()
        {
            if (SelectedEdgesChange != null)
                SelectedEdgesChange();

            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVerticesUpdate = true;

            IVertex selectedEdges = Vertex.Get(false, @"SelectedEdges:");

            if (UseDataVirtualization)
            {
                foreach (TreeVisualiserViewItem item in virtualContainers.Values.ToList())
                    item.IsSelected = selectedEdges != null
                        && EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, (IEdge)item.Tag) != null;

                TurnOffSelectedVerticesUpdate = false;
                return;
            }

            if (selectedEdges == null)
            {
                foreach (TreeViewItem i in Items)
                    ClearAllSelectedItems_Reccurent(i);

                TurnOffSelectedVerticesUpdate = false;
                return;
            }

            foreach (TreeViewItem i in Items)
                SelectedVerticesUpdated_Reccurent(i, selectedEdges);

            TurnOffSelectedVerticesUpdate = false;
        }

        private void SelectedVerticesUpdated_Reccurent(TreeViewItem i, IVertex selectedEdges)
        {
            if (i is TreeVisualiserViewItem)
            {
                TreeVisualiserViewItem ii = (TreeVisualiserViewItem)i;

                bool isSelected = selectedEdges != null
                    && EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, (IEdge)ii.Tag) != null;

                ii.IsSelected = isSelected;
            }

            foreach (TreeViewItem ii in i.Items)
                SelectedVerticesUpdated_Reccurent(ii, selectedEdges);
        }

        public void UpdateSelectedVertices(bool IsCtrl, TreeVisualiserViewItem item)
        {
            if (SelectionProhibited)
                return;

            if (TurnOffSelectedVerticesUpdate)
                return;

            TurnOffSelectedItemsUpdate = true;

            IVertex sv = Vertex.Get(false, @"SelectedEdges:");

            if (sv == null)
            {
                TurnOffSelectedItemsUpdate = false;
                return;
            }

            IEdge e=(IEdge)item.Tag;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////            

            if (!IsCtrl)
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            if (item.IsSelected)
                EdgeHelper.AddEdgeVertex(sv, e);
            else
                EdgeHelper.DeleteVertexByEdge(sv, e);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            TurnOffSelectedItemsUpdate = false;
        }

        public void UnselectAllSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            VisualiserUtil.RemoveAllSelectedEdges(this);            

            ClearAllSelectedItems();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        public void ClearAllSelectedItems()
        {            
            if (UseDataVirtualization)
            {
                foreach (TreeVisualiserViewItem item in virtualContainers.Values.ToList())
                    item.IsSelected = false;

                return;
            }

            foreach (TreeViewItem i in Items)
                ClearAllSelectedItems_Reccurent(i);            
        }

        internal void RegisterPendingMouseDownFromItem(TreeVisualiserViewItem item)
        {
            if (SelectionProhibited || item == null)
                return;

            pendingMouseDownItem = item;
            pendingMouseDownEdge = (IEdge)item.Tag;
            pendingMouseDownIsCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            pendingWasInSelectionAtMouseDown = SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(
                Vertex,
                pendingMouseDownEdge);
            suppressNextMouseUpSelection = false;
        }

        internal void TryApplyContextMenuSelectionFromItem(TreeVisualiserViewItem item)
        {
            if (SelectionProhibited || item == null)
                return;

            SelectedEdgesInteractionHelper.ApplyForContextMenu(Vertex, (IEdge)item.Tag);
            SelectedVerticesUpdated();
        }

        internal void TryApplyPendingMouseClickFromItem(TreeVisualiserViewItem item)
        {
            if (suppressNextMouseUpSelection)
            {
                ClearPendingMouseDown();
                return;
            }

            if (SelectionProhibited
                || pendingMouseDownItem == null
                || pendingMouseDownEdge == null
                || pendingMouseDownItem != item)
            {
                ClearPendingMouseDown();
                return;
            }

            PendingEdgeMouseGesture pendingGesture = new PendingEdgeMouseGesture
            {
                ClickedEdge = pendingMouseDownEdge,
                IsCtrl = pendingMouseDownIsCtrl,
                WasInSelectionAtMouseDown = pendingWasInSelectionAtMouseDown
            };

            SelectedEdgesInteractionHelper.ApplyForClick(Vertex, pendingGesture);
            SelectedVerticesUpdated();

            ClearPendingMouseDown();
        }

        internal IVertex TryPrepareDragAndBuildDndVertex(Point dndStartPoint)
        {
            IEdge clickedEdge = pendingMouseDownEdge;
            bool isCtrl = pendingMouseDownIsCtrl;
            bool wasInSelectionAtMouseDown = pendingWasInSelectionAtMouseDown;
            IVertex fallbackEdgeVertex = null;

            if (clickedEdge == null)
            {
                fallbackEdgeVertex = GetEdgeByPoint(dndStartPoint);

                if (fallbackEdgeVertex == null)
                    return null;

                clickedEdge = EdgeHelper.GetIEdgeByEdgeVertex(fallbackEdgeVertex);
                isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                wasInSelectionAtMouseDown = SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, clickedEdge);
            }
            else
                fallbackEdgeVertex = GetEdgeByPoint(dndStartPoint);

            PendingEdgeMouseGesture pendingGesture = new PendingEdgeMouseGesture
            {
                ClickedEdge = clickedEdge,
                IsCtrl = isCtrl,
                WasInSelectionAtMouseDown = wasInSelectionAtMouseDown
            };

            SelectedEdgesInteractionHelper.ApplyForDrag(Vertex, pendingGesture);
            SelectedVerticesUpdated();
            suppressNextMouseUpSelection = true;

            IVertex dndVertex = SelectedEdgesInteractionHelper.BuildDndVertexFromSelectedEdges(
                Vertex,
                fallbackEdgeVertex);

            ClearPendingMouseDown();

            return dndVertex;
        }

        internal void ClearPendingMouseDown()
        {
            pendingMouseDownItem = null;
            pendingMouseDownEdge = null;
            pendingMouseDownIsCtrl = false;
            pendingWasInSelectionAtMouseDown = false;
        }

        private void ApplyFullWidthSelectionHighlightToAllItems()
        {
            if (UseDataVirtualization)
            {
                foreach (TreeVisualiserViewItem treeItem in virtualContainers.Values.ToList())
                {
                    treeItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    treeItem.InvalidateVisual();
                }

                return;
            }

            ApplyFullWidthSelectionHighlightToItems(Items);
        }

        private void ApplyFullWidthSelectionHighlightToItems(ItemCollection items)
        {
            foreach (object item in items)
            {
                TreeVisualiserViewItem treeItem = item as TreeVisualiserViewItem;

                if (treeItem == null)
                    continue;

                treeItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                treeItem.UpdateHeader();

                ApplyFullWidthSelectionHighlightToItems(treeItem.Items);
            }
        }

        public void ClearKeyboardHighlight()
        {
            if (keyboardHighlightedItem != null)
                keyboardHighlightedItem.IsKeyboardHighlighted = false;

            keyboardHighlightedItem = null;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            List<TreeVisualiserViewItem> items = GetKeyboardHighlightItems();

            if (items.Count == 0)
            {
                if (positionDelta < 0)
                    GoBeforeFirstKeyboardHighlightPosition();
                else
                    GoAfterLastKeyboardHighlightPosition();

                return;
            }

            int currentIndex = CurrentHighlightPosition;

            if (currentIndex < 0)
                currentIndex = positionDelta < 0 ? items.Count : -1;

            int newIndex = currentIndex + positionDelta;

            if (newIndex < 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else if (newIndex >= items.Count)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightItem(items[newIndex]);
        }

        public void MoveKeyboardHighlight(KeyboardHighlightMoveDirection direction)
        {
            if (direction == KeyboardHighlightMoveDirection.Up)
                MoveKeyboardHighlight(-1);
            else if (direction == KeyboardHighlightMoveDirection.Down)
                MoveKeyboardHighlight(1);
            else if (direction == KeyboardHighlightMoveDirection.Left && keyboardHighlightedItem != null)
                keyboardHighlightedItem.IsExpanded = false;
            else if (direction == KeyboardHighlightMoveDirection.Right && keyboardHighlightedItem != null)
                keyboardHighlightedItem.IsExpanded = true;
        }

        public void ToggleKeyboardHighlightedEdgeSelection()
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled || SelectionProhibited || keyboardHighlightedItem == null)
                return;

            bool wasSelected = keyboardHighlightedItem.IsSelected;

            keyboardHighlightedItem.IsSelected = !wasSelected;
            UpdateSelectedVertices(true, keyboardHighlightedItem);
        }

        public bool ActivateKeyboardHighlightItem(TreeVisualiserViewItem item)
        {
            if (item == null || !IsVertexCommanderKeyboardHighlightEnabled)
                return false;

            SetKeyboardHighlightItem(item);
            RaiseKeyboardHighlightActivated();

            return KeyboardHighlightActivated != null;
        }

        private void SetKeyboardHighlightToFirst()
        {
            List<TreeVisualiserViewItem> items = GetKeyboardHighlightItems();

            if (items.Count == 0)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightItem(items[0]);
        }

        private void SetKeyboardHighlightToLast()
        {
            List<TreeVisualiserViewItem> items = GetKeyboardHighlightItems();

            if (items.Count == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightItem(items[items.Count - 1]);
        }

        private void SetKeyboardHighlightItem(TreeVisualiserViewItem item)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            ClearKeyboardHighlight();

            keyboardHighlightedItem = item;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
            keyboardHighlightedItem.IsKeyboardHighlighted = true;
            keyboardHighlightedItem.BringIntoView();
            ScheduleFullWidthVisualRefresh();
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

        private List<TreeVisualiserViewItem> GetKeyboardHighlightItems()
        {
            List<TreeVisualiserViewItem> result = new List<TreeVisualiserViewItem>();

            if (UseDataVirtualization)
            {
                AddVirtualKeyboardHighlightItems(virtualRootNodes, result);
                return result;
            }

            AddKeyboardHighlightItems(Items, result);

            return result;
        }

        private void AddVirtualKeyboardHighlightItems(
            IEnumerable<TreeEdgeNode> nodes,
            IList<TreeVisualiserViewItem> result)
        {
            foreach (TreeEdgeNode node in nodes)
            {
                if (!virtualContainers.TryGetValue(node, out TreeVisualiserViewItem treeItem))
                    continue;

                result.Add(treeItem);

                if (treeItem.IsExpanded && node.ChildrenLoaded)
                    AddVirtualKeyboardHighlightItems(node.Children, result);
            }
        }

        private void AddKeyboardHighlightItems(ItemCollection items, IList<TreeVisualiserViewItem> result)
        {
            foreach (object item in items)
            {
                TreeVisualiserViewItem treeItem = item as TreeVisualiserViewItem;

                if (treeItem == null)
                    continue;

                result.Add(treeItem);

                if (treeItem.IsExpanded)
                    AddKeyboardHighlightItems(treeItem.Items, result);
            }
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

        private void ClearAllSelectedItems_Reccurent(TreeViewItem i)
        {
            if(i is TreeVisualiserViewItem)
                ((TreeVisualiserViewItem)i).IsSelected = false;

            foreach (TreeViewItem ii in i.Items)
                ClearAllSelectedItems_Reccurent(ii);
        }

        public TreeViewItem CreateTreeViewItem(IEdge e, bool generateDeeperLevel, TreeViewItem parent){
            TreeVisualiserViewItem i = new TreeVisualiserViewItem();            

            if(parent is TreeVisualiserViewItem)
            {
                TreeVisualiserViewItem parent_tvvi = (TreeVisualiserViewItem)parent;

                if (parent_tvvi.doNotTrackGraphChanges)
                    i.doNotTrackGraphChanges = true;
            }

            // DO NOT TRACK GRAPH CHANGE BEG

            if (e.Meta != null && GeneralUtil.CompareStrings(e.Meta.Value, "$GraphChangeTrigger"))
                i.doNotTrackGraphChanges = true;

            if (e.Meta != null && GeneralUtil.CompareStrings(e.Meta.Value, "FormalTextLanguage"))
                i.doNotTrackGraphChanges = true;

            // DO NOT TRACK GRAPH CHANGE END

            i.ParentVisualiser = this;
            i.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            i.Tag = e;

            i.UpdateHeader();
            

            TurnOffSelectedVerticesUpdate = true;

            IVertex selectedEdges = Vertex.Get(false, @"SelectedEdges:");

            if (selectedEdges != null && EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, e) != null)
                i.IsSelected = true;

            TurnOffSelectedVerticesUpdate = false;

            if(generateDeeperLevel)
                if (e.To.Count() > 0)
                {
                    TreeViewItem tvi = new TreeViewItem();
                    i.Items.Add(tvi);
                }

            if (!i.doNotTrackGraphChanges)
                i.vertexChangeListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(e.To,
                    new List<string> { },
                    new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                     GraphChangeFilterEnum.OutputEdgeAdded,
                     GraphChangeFilterEnum.OutputEdgeRemoved,
                     GraphChangeFilterEnum.OutputEdgeDisposed},
                     "TreeViewItem",
                     i.VertexChange);

            return i;
        }

        private void ClearAllItems()
        {
            ClearAllItems_Reccurent(this);           
        }

        public static void ClearAllItems_Reccurent(ItemsControl c)
        {
            foreach (object o in c.Items)
            {
                if (o is IDisposable)
                {
                    ((IDisposable)o).Dispose();
                }

                if (o is ItemsControl)
                    ClearAllItems_Reccurent((ItemsControl)o);                
            }

            c.Items.Clear();
        }

        protected void SetVertexDefaultValues()
        {         
            Vertex.Get(false, "Scale:").Value = 100;
        }

        public void SelectAllInBaseEdge()
        {
            TurnOffSelectedItemsUpdate=true;

            IVertex selectedEdges = Vertex.Get(false, @"SelectedEdges:");

            //if (selectedEdges is VertexBase)
              //  ((VertexBase)selectedEdges).CanFireChangeEvent = false;                        

            foreach (IEdge ee in Vertex.Get(false, @"BaseEdge:\To:"))
                EdgeHelper.AddEdgeVertex(selectedEdges, ee);

            //if (selectedEdges is VertexBase)
              //  ((VertexBase)selectedEdges).CanFireChangeEvent = true;            

            TurnOffSelectedItemsUpdate = false;

            if (UseDataVirtualization)
            {
                foreach (TreeVisualiserViewItem item in virtualContainers.Values.ToList())
                    item.IsSelected = true;

                return;
            }

            foreach (TreeViewItem i in Items)            
                if (i is TreeVisualiserViewItem)
                {
                    TreeVisualiserViewItem ii = (TreeVisualiserViewItem)i;
                    ii.IsSelected = true;
                }      
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        void DisposeTreeViewItems(ItemCollection list)
        {
            foreach (TreeViewItem i in list)
            {
                DisposeTreeViewItems(i.Items);

                if (i is IDisposable)
                    ((IDisposable)i).Dispose();
            }
        }


        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                UnregisterRootTreeBaseListener();

                VisualiserHelper.Dispose();

                if (UseDataVirtualization)
                    ClearVirtualTreeModel();
                else
                    DisposeTreeViewItems(this.Items);
            }
        }

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByPoint(Point p)
        {
            vertexByLocationToReturn = null;

            if (UseDataVirtualization)
                GetVirtualVertexByLocation(p);
            else
                GetVertexByLocation_Reccurent(this.Items, p);

            // Fallback to the root (BaseEdge:) is only allowed when the point is below the
            // last visible tree item header, not when it merely falls into the empty space
            // to the right of some leaf header. Otherwise dragging from the empty area on
            // the right side of any leaf would incorrectly initiate DnD for the tree root.
            if (vertexByLocationToReturn == null
                && IsPointBelowLastVisibleHeader(p)
                && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
            {
                vertexByLocationToReturn = Vertex.Get(false, @"BaseEdge:");
            }

            return vertexByLocationToReturn;
        }

        private bool IsPointBelowLastVisibleHeader(Point p)
        {
            double bottomY = UseDataVirtualization
                ? GetMaxBottomYOfVirtualHeaders()
                : GetMaxBottomYOfVisibleHeaders(this.Items);

            // No visible header at all -> the whole tree area counts as "below the last header".
            if (bottomY <= 0)
                return true;

            return p.Y >= bottomY;
        }

        private double GetMaxBottomYOfVirtualHeaders()
        {
            double max = 0;

            foreach (TreeVisualiserViewItem item in virtualContainers.Values)
            {
                FrameworkElement headerElement = item.Header as FrameworkElement;

                if (headerElement == null || !headerElement.IsVisible || headerElement.ActualHeight <= 0)
                    continue;

                try
                {
                    Point bottomLeftInTree = headerElement.TranslatePoint(
                        new Point(0, headerElement.ActualHeight), this);

                    if (bottomLeftInTree.Y > max)
                        max = bottomLeftInTree.Y;
                }
                catch (InvalidOperationException)
                {
                    // Header is not connected to this visual tree (yet), skip it.
                }
            }

            return max;
        }

        private void GetVirtualVertexByLocation(Point point)
        {
            foreach (TreeVisualiserViewItem item in virtualContainers.Values)
            {
                if (!IsPointOverTreeViewItemHeaderText(item, point))
                    continue;

                IVertex edgeVertex = MinusZero.Instance.CreateTempVertex();
                EdgeHelper.AddEdgeVertexEdges(edgeVertex, (IEdge)item.Tag);
                vertexByLocationToReturn = edgeVertex;
                return;
            }
        }

        private double GetMaxBottomYOfVisibleHeaders(ItemCollection items)
        {
            double max = 0;

            foreach (TreeViewItem i in items)
            {
                FrameworkElement headerElement = i.Header as FrameworkElement;

                if (headerElement != null && headerElement.IsVisible && headerElement.ActualHeight > 0)
                {
                    try
                    {
                        Point bottomLeftInTree = headerElement.TranslatePoint(
                            new Point(0, headerElement.ActualHeight), this);

                        if (bottomLeftInTree.Y > max)
                            max = bottomLeftInTree.Y;
                    }
                    catch (InvalidOperationException)
                    {
                        // Header is not connected to this visual tree (yet), skip it.
                    }
                }

                if (i.IsExpanded)
                {
                    double childMax = GetMaxBottomYOfVisibleHeaders(i.Items);
                    if (childMax > max)
                        max = childMax;
                }
            }

            return max;
        }

        protected IVertex GetVertexByLocation_Reccurent(ItemCollection items, Point p)
        {
            foreach (TreeViewItem i in items)
            {
                if (IsPointOverTreeViewItemHeaderText(i, p))
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    EdgeHelper.AddEdgeVertexEdges(v, (IEdge)i.Tag);
                    vertexByLocationToReturn = v;
                }

                // Only recurse into expanded items whose children are actually realized and visible.
                // This avoids spurious hits on collapsed subtrees when doing bounds-based hit testing.
                if (i.IsExpanded)
                    GetVertexByLocation_Reccurent(i.Items, p);
            }

            return null;
        }

        // Drag-and-drop on a tree item should only start when the mouse is really over the
        // visible header content (icon + meta + value text), not over the empty area that the
        // TreeViewItem stretches into on the right side of the row.
        private bool IsPointOverTreeViewItemHeaderText(TreeViewItem item, Point pointInTreeVisualiser)
        {
            FrameworkElement headerElement = item.Header as FrameworkElement;

            if (headerElement == null || headerElement.IsVisible == false)
                return false;

            if (headerElement.ActualWidth <= 0 || headerElement.ActualHeight <= 0)
                return false;

            Point pointInHeader;

            try
            {
                pointInHeader = TranslatePoint(pointInTreeVisualiser, headerElement);
            }
            catch (InvalidOperationException)
            {
                // Header is not connected to this visual tree (yet), treat as no hit.
                return false;
            }

            return pointInHeader.X >= 0
                && pointInHeader.Y >= 0
                && pointInHeader.X < headerElement.ActualWidth
                && pointInHeader.Y < headerElement.ActualHeight;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
    }
}
