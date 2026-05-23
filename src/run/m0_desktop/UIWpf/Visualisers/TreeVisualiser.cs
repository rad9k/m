using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static bool HideMetaNameIfEmpty = true;

        public bool IsFilled;        

        private bool ignoreNextMouseLeftButtonUp;
        private bool isExpandCollapseAnimationInProgress;
        private ToggleButton expanderToggleButton;

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
                _IsSelected = value;

                MetaToEdgeControl headerControl = Header as MetaToEdgeControl;

                if (headerControl != null)
                    headerControl.IsSelected = value;
            }
        }

        public TreeVisualiser ParentVisualiser {get; set;}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

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
                    e.Handled = true;
                    return;
                }

                BaseCommands.OpenDefaultVisualiser(EdgeHelper.CreateTempEdgeVertex(GetEdge()), false);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs a)
        {
            if (IsInExpandCollapseClickArea(a.GetPosition(this)))
            {
                ToggleExpandedWithAnimation();
                ignoreNextMouseLeftButtonUp = true;
                a.Handled = true;
                return;
            }

            a.Handled = true;
        }

        IEdge GetEdge()
        {
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

            bool IsCtrl = false;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                IsCtrl = true;

            bool WasSelected = IsSelected;            

            if (!IsCtrl)            
                ParentVisualiser.ClearAllSelectedItems();

            if (WasSelected)
                Unselect(IsCtrl);
            else
                Select(IsCtrl);

            a.Handled = true;

            //base.OnMouseLeftButtonDown(a);
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
            bool wasSelected = _IsSelected;

            MetaToEdgeControl headerControl = Header as MetaToEdgeControl;

            if (headerControl == null)
            {
                headerControl = new MetaToEdgeControl();
                headerControl.MouseEnter += HeaderControlMouseEnter;
                headerControl.MouseLeave += HeaderControlMouseLeave;
                Header = headerControl;
            }

            headerControl.ShowIcon = ParentVisualiser.ShowIcons;
            headerControl.BaseEdge = GetEdge();
            headerControl.RefreshVisuals();
            headerControl.IsSelected = wasSelected;
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
            MetaToEdgeControl headerControl = Header as MetaToEdgeControl;

            if (headerControl != null)
                headerControl.IsHighlighted = isHighlighted;
        }

        public INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
           // ExecutionFlowHelper.DebugStackStraceAsEvents(exe.Stack);

            if (ParentVisualiser.VisualiserHelper.IsDisposed)
                return exe.Stack;

            if (GetEdge().To.DisposedState != DisposeStateEnum.Live)
                return exe.Stack;

            // will do this

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

        public bool SelectionProphibited { get; set; }

        private TreeVisualiserViewItem keyboardHighlightedItem;
        private bool isBeforeFirstKeyboardPosition;
        private bool isAfterLastKeyboardPosition;


        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

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

            // THIS REDUCES PERFORMANCE ON LARGE TREES SO commented out
            //VirtualizingStackPanel.SetIsVirtualizing(this, true); 
            //VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);

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
            IList l = GeneralUtil.CreateAndCopyList(Items);

            foreach (TreeVisualiserViewItem i in l)
                if (EdgeHelper.CompareIEdges((IEdge)i.Tag, edge))
                    Items.Remove(i);
        }

        private void EdgeAdded(IEdge edge)
        {
            Items.Add(CreateTreeViewItem(edge, true, null));
        }

        private void EdgeDisposed(IEdge edge)
        {
            BaseEdgeToUpdated();
        }

        private void UpdateShowIconOnAllItems()
        {
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

            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (TreeViewItem i in Items)
                SelectedVerticesUpdated_Reccurent(i,sv);

            TurnOffSelectedVerticesUpdate = false;
        }

        private void SelectedVerticesUpdated_Reccurent(TreeViewItem i,IVertex sv)
        {
            if (i is TreeVisualiserViewItem)
            {
                TreeVisualiserViewItem ii = (TreeVisualiserViewItem)i;

                
                if (EdgeHelper.FindEdgeVertexByToVertex(sv, ((IEdge)ii.Tag).To)!=null)                
                    ii.IsSelected = true;
                else
                    ii.IsSelected = false;
            }

            foreach (TreeViewItem ii in i.Items)
                SelectedVerticesUpdated_Reccurent(ii, sv);
        }

        public void UpdateSelectedVertices(bool IsCtrl, TreeVisualiserViewItem item)
        {
            if (SelectionProphibited)
                return;

            if (TurnOffSelectedVerticesUpdate)
                return;

            TurnOffSelectedItemsUpdate = true;

            IVertex sv = Vertex.Get(false, @"SelectedEdges:");            

            IEdge e=(IEdge)item.Tag;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////            

            if (!IsCtrl)
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            if (item.IsSelected)
                EdgeHelper.AddEdgeVertex(sv, e);
            else
                EdgeHelper.DeleteVertexByEdgeOnlyToVertex(sv, e);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            // LEGACY
            //
            // currently there is no support for same vertex in two places in tree begin selected / unselected
            // this is due to performance
            //
            /*IVertex sv = Vertex.Get(false, "SelectedVertices:");

            GraphUtil.RemoveAllEdges(sv);

            foreach (TreeViewItem i in Items)
                UpdateSelectedVertices_Reccurent(i, sv);
             */

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
            foreach (TreeViewItem i in Items)
                ClearAllSelectedItems_Reccurent(i);            
        }

        public void ClearKeyboardHighlight()
        {
            if (keyboardHighlightedItem != null)
                keyboardHighlightedItem.SetHeaderHighlight(false);

            keyboardHighlightedItem = null;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
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
            if (SelectionProphibited || keyboardHighlightedItem == null)
                return;

            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool wasSelected = keyboardHighlightedItem.IsSelected;

            if (!isCtrl)
                ClearAllSelectedItems();

            keyboardHighlightedItem.IsSelected = !wasSelected;
            UpdateSelectedVertices(isCtrl, keyboardHighlightedItem);
        }

        public bool ActivateKeyboardHighlightItem(TreeVisualiserViewItem item)
        {
            if (item == null)
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
            ClearKeyboardHighlight();

            keyboardHighlightedItem = item;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
            keyboardHighlightedItem.SetHeaderHighlight(true);
            keyboardHighlightedItem.BringIntoView();
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

            AddKeyboardHighlightItems(Items, result);

            return result;
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

            i.Tag = e;

            i.UpdateHeader();

            

            TurnOffSelectedVerticesUpdate = true;

            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            if (EdgeHelper.FindIEdgeVertexByIEdge(sv, e)!=null)
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

                DisposeTreeViewItems(this.Items);
            }
        }

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByPoint(Point p)
        {
            vertexByLocationToReturn = null;

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
            double bottomY = GetMaxBottomYOfVisibleHeaders(this.Items);

            // No visible header at all -> the whole tree area counts as "below the last header".
            if (bottomY <= 0)
                return true;

            return p.Y >= bottomY;
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

        protected IVertex GetVertexByLocation_Reccurent(ItemCollection items,Point p){
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
