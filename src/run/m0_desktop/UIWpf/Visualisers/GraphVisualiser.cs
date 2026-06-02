using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Commands;
using m0.UIWpf.Controls.Fast;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using m0.ZeroUML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace m0.UIWpf.Visualisers
{
    public class LineTagStore
    {
        public SimpleVisualiserWrapper ToWrapper;
        public SimpleVisualiserWrapper FromWrapper;
        public TextBlock MetaLabel;
        public bool IsAnimationOverlay;
    }

    public class SimpleVisualiserWrapper : Border, IDisposable
    {
        GraphVisualiser ParentVisualiser;

        public IVertex baseVertex;

        public List<Shape> Lines=new List<Shape>();

        public bool IsSelected;

        IEdge listenerEdge;

        public void Select()
        {
            IsSelected = true;

            this.Background = (Brush)FindResource("0SelectionBrush");

            SetChildForeground((Brush)FindResource("0BackgroundBrush"));
        }

        public void Unselect()
        {
            IsSelected = false;

            this.Background = (Brush)FindResource("0BackgroundBrush");

            if(IsHighlighted)
                SetChildForeground((Brush)FindResource("0HighlightBrush"));
            else
                SetChildForeground((Brush)FindResource("0ForegroundBrush"));
        }


        public bool IsHighlighted;

        public void HighlightThisAndDescendants()
        {
            IsHighlighted = true;

            HighlightThis();

            foreach(Shape e in Lines){
                ParentVisualiser.ApplyEdgeStyle(e, true);

                LineTagStore edgeTag = e.Tag as LineTagStore;
                Panel.SetZIndex(e, edgeTag != null && edgeTag.IsAnimationOverlay ? 99997 : 99996);

                if (e.Tag != null)
                {
                    LineTagStore lineTag = (LineTagStore)e.Tag;

                    if (lineTag.MetaLabel != null)
                    {
                        lineTag.MetaLabel.Foreground = (Brush)FindResource("0HighlightBrush");
                        Panel.SetZIndex(lineTag.MetaLabel, 99998);
                    }

                    lineTag.ToWrapper.HighlightThis();

                    if (lineTag.FromWrapper != this)
                        lineTag.FromWrapper.HighlightThis();
                }
            }                            
        }

        public void HighlightThis()
        {            
            Panel.SetZIndex(this, 99999);

            this.BorderBrush = (Brush)FindResource("0HighlightBrush");

            SetChildForeground((Brush)FindResource("0HighlightBrush"));
        }

        public void UnhighlightThisAndDescendants()
        {            
            IsHighlighted = false;

            UnhighlightThis();

            foreach(Shape e in Lines){
                ParentVisualiser.ApplyEdgeStyle(e, false);

                Panel.SetZIndex(e, 0);

                if (e.Tag != null)
                {
                    LineTagStore lineTag = (LineTagStore)e.Tag;

                    if (lineTag.MetaLabel != null)
                    {
                        lineTag.MetaLabel.Foreground = (Brush)FindResource("0LightGrayBrush");
                        Panel.SetZIndex(lineTag.MetaLabel, 1);
                    }

                    lineTag.ToWrapper.UnhighlightThis();

                    if (lineTag.FromWrapper != this)
                        lineTag.FromWrapper.UnhighlightThis();
                }
            }                                    
        }

        public void UnhighlightThis()
        {
            Panel.SetZIndex(this, 1);

            this.BorderBrush = (Brush)FindResource("0LightGrayBrush");

            if(IsSelected)
                SetChildForeground((Brush)FindResource("0BackgroundBrush"));
            else
                SetChildForeground((Brush)FindResource("0ForegroundBrush"));
        }

        private void SetChildForeground(Brush brush)
        {
            SetForegroundIfPresent(this.Child, brush);
        }

        private void SetForegroundIfPresent(DependencyObject element, Brush brush)
        {
            if (element == null)
                return;

            GeneralUtil.SetPropertyIfPresent(element, "Foreground", brush);

            if (element is Panel)
            {
                Panel panel = (Panel)element;

                foreach (UIElement child in panel.Children)
                    SetForegroundIfPresent(child, brush);
            }
            else if (element is ContentControl)
            {
                ContentControl contentControl = (ContentControl)element;

                if (contentControl.Content is DependencyObject)
                    SetForegroundIfPresent((DependencyObject)contentControl.Content, brush);
            }
            else if (element is Decorator)
            {
                Decorator decorator = (Decorator)element;

                SetForegroundIfPresent(decorator.Child, brush);
            }
        }

        public SimpleVisualiserWrapper(FrameworkElement e, IVertex _baseVertex, GraphVisualiser _ParentVisualiser)
        {
            baseVertex = _baseVertex;

            ParentVisualiser = _ParentVisualiser;            

            this.Child = e;

            this.BorderBrush = (Brush)FindResource("0LightGrayBrush");

            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.BorderThickness = new Thickness(1);

            this.Padding = new Thickness(1);

            if (baseVertex != null)
                listenerEdge = ExecutionFlowHelper.AddTriggerAndListener(baseVertex, 
                    new List<string> { },
                    new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                     GraphChangeFilterEnum.OutputEdgeAdded,
                     GraphChangeFilterEnum.OutputEdgeRemoved,
                     GraphChangeFilterEnum.OutputEdgeDisposed},
                    "BasicTrigger", 
                    VertexChange);                           
        }

        protected INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (baseVertex.DisposedState == DisposeStateEnum.Live)
            {
                IVertex valueChange = exe.Stack.Get(false, @"event:\Type:ValueChange");

                if(valueChange == null)
                    ParentVisualiser.PaintGraph();
            }

            return exe.Stack;
        }

        public void Dispose()
        {
            if (this.Child is IDisposable)
                ((IDisposable)(this.Child)).Dispose();

            if (baseVertex != null)
                GraphChangeTrigger.RemoveListener(listenerEdge);                
        }        
    }

    public class GraphVisualiser: Canvas, IListVisualiser, IHasSelectableEdges, ITypedEdge, IKeyboardHighlight
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProphibited { get; set; }

        SimpleVisualiserWrapper Highlighted;
        private IVertex keyboardHighlightedVertex;
        private bool isBeforeFirstKeyboardPosition;
        private bool isAfterLastKeyboardPosition;

        public bool IsPaiting=false;


        static string[] _MetaTriggeringUpdateVertex = new string[] { "VisualiserCircleSize", "NumberOfCircles", "ShowOutEdges", "ShowInEdges", "FastMode", "MetaLabels", "AnimateEdges", "ShowIcons" };
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

        public int CurrentHighlightPosition
        {
            get
            {
                List<IVertex> vertices = GetKeyboardHighlightVertices();

                if (keyboardHighlightedVertex == null)
                    return -1;

                return vertices.IndexOf(keyboardHighlightedVertex);
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
                List<IVertex> vertices = GetKeyboardHighlightVertices();
                return vertices.Count > 0 && CurrentHighlightPosition == vertices.Count - 1 && !isBeforeFirstKeyboardPosition && !isAfterLastKeyboardPosition;
            }
            set { if (value) SetKeyboardHighlightToLast(); }
        }

        public bool CanGoBeforeFirstPosition { get { return true; } }

        public bool CanGoAfterLastPosition { get { return true; } }

        public bool HasKeyboardHighlightItems
        {
            get { return GetKeyboardHighlightVertices().Count > 0; }
        }

        public IEdge KeyboardHighlightedEdge
        {
            get
            {
                if (keyboardHighlightedVertex == null)
                    return null;

                IEdge firstInEdge = keyboardHighlightedVertex.InEdgesRaw.FirstOrDefault();

                if (firstInEdge != null)
                    return firstInEdge;

                return new EasyEdge(null, null, keyboardHighlightedVertex);
            }
        }

        public event EventHandler KeyboardHighlightActivated;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        // TypedEdge START

        public GraphVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END


        public GraphVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            DisplayedVerticesUIElements = new Dictionary<IVertex, SimpleVisualiserWrapper>();

            this.Background = (Brush)FindResource("0BackgroundBrush");

            new ListVisualiserHelper(parentVisualiser,
              isVolatile,
              MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Graph"),
              this, 
              "GraphVisualiser", 
              this, 
              false, 
              new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" }, 
              "AtomVisualiserFull",
              baseEdgeVertex,
              UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

            this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
            this.PreviewMouseMove += dndPreviewMouseMove;
            this.Drop += dndDrop;
            this.AllowDrop = true;

            this.MouseEnter += dndMouseEnter;

            SetVertexDefaultValues();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();

            PaintGraph();

            if (IsFirstPainted)
                this.Loaded -= OnLoad;
        }

        protected SimpleVisualiserWrapper Add(double x, double y, FrameworkElement _e, IVertex baseVertex)
        {
            SimpleVisualiserWrapper e = new SimpleVisualiserWrapper(_e, baseVertex, this);
            Children.Add(e);

            e.UpdateLayout();

            Panel.SetZIndex(e, 1);
            Canvas.SetLeft(e, x-e.ActualWidth/2);
            Canvas.SetTop(e, y-e.ActualHeight/2);
            
            if (!DisplayedVerticesUIElements.ContainsKey(baseVertex))
                DisplayedVerticesUIElements.Add(baseVertex, e);
            else
                DisplayedVerticesUIElements[baseVertex] = e;
            
            return e;
        }        

        protected void AddLine(SimpleVisualiserWrapper FromWrapper, SimpleVisualiserWrapper ToWrapper, IVertex meta){
            LineTagStore lineTag = new LineTagStore();
            lineTag.ToWrapper = ToWrapper;
            lineTag.FromWrapper = FromWrapper;

            ArrowLine l = new ArrowLine();            

            l.Tag = lineTag;

            FromWrapper.Lines.Add(l);
            ToWrapper.Lines.Add(l);            

            l.X1 = Canvas.GetLeft(FromWrapper)+FromWrapper.ActualWidth/2;
            l.Y1 = Canvas.GetTop(FromWrapper)+FromWrapper.ActualHeight/2;

            double tX = Canvas.GetLeft(ToWrapper)+ToWrapper.ActualWidth/2;
            double tY = Canvas.GetTop(ToWrapper)+ToWrapper.ActualHeight/2;

            double testX = l.X1-tX;
            double testY = l.Y1-tY;

            if (testX == 0) testX = 0.001;            
            if (testY == 0) testY = 0.001;

            if (testY <= 0 && Math.Abs(testX * ToWrapper.ActualHeight) <= Math.Abs(testY * ToWrapper.ActualWidth))            
            {
                l.X2 = tX - (ToWrapper.ActualHeight / 2 * testX / testY);
                l.Y2 = tY - ToWrapper.ActualHeight / 2;                
            }

            if (testY > 0 && Math.Abs(testX * ToWrapper.ActualHeight) <= Math.Abs(testY * ToWrapper.ActualWidth))            
            {
                l.X2 = tX + (ToWrapper.ActualHeight / 2 * testX / testY);
                l.Y2 = tY + ToWrapper.ActualHeight / 2;
            }

            if (testX >= 0 && Math.Abs(testX * ToWrapper.ActualHeight) >= Math.Abs(testY * ToWrapper.ActualWidth))
            {
                l.X2 = tX + ToWrapper.ActualWidth / 2 ;
                l.Y2 = tY + (ToWrapper.ActualWidth / 2 * testY / testX);             
            }

            if (testX <= 0 && Math.Abs(testX * ToWrapper.ActualHeight) >= Math.Abs(testY * ToWrapper.ActualWidth))
            {
                l.X2 = tX - ToWrapper.ActualWidth / 2;
                l.Y2 = tY - (ToWrapper.ActualWidth / 2 * testY / testX);                
            }            
                                   
            l.Stroke = (Brush)FindResource("0LightGrayBrush");
                        
            l.EndEnding = m0.UIWpf.Controls.Fast.LineEndEnum.FilledTriangle;
            l.Fill = (Brush)FindResource("0LightGrayBrush");            
            ApplyEdgeStyle(l, false);

            Panel.SetZIndex(l, 0);            

            Children.Add(l);

            ArrowLine animatedOverlay = new ArrowLine();
            LineTagStore animatedLineTag = new LineTagStore();
            animatedLineTag.ToWrapper = ToWrapper;
            animatedLineTag.FromWrapper = FromWrapper;
            animatedLineTag.IsAnimationOverlay = true;
            animatedOverlay.Tag = animatedLineTag;
            animatedOverlay.X1 = l.X1;
            animatedOverlay.Y1 = l.Y1;
            animatedOverlay.X2 = l.X2;
            animatedOverlay.Y2 = l.Y2;
            animatedOverlay.EndEnding = m0.UIWpf.Controls.Fast.LineEndEnum.Straight;
            FromWrapper.Lines.Add(animatedOverlay);
            ToWrapper.Lines.Add(animatedOverlay);
            ApplyEdgeStyle(animatedOverlay, false);
            Panel.SetZIndex(animatedOverlay, 0);
            Children.Add(animatedOverlay);

            if (MetaLabels&&meta.Value!=null&&!GeneralUtil.CompareStrings(meta.Value,"$Empty"))
            {                
                TextBlock b = new TextBlock();
                b.Text = meta.Value.ToString();

                Canvas.SetLeft(b, l.X1 + ((l.X2 - l.X1) / 2));
                Canvas.SetTop(b, l.Y1 + ((l.Y2 - l.Y1) / 2));

                b.Foreground = (Brush)FindResource("0LightGrayBrush");

                Panel.SetZIndex(b, 2);
                Children.Add(b);

                lineTag.MetaLabel = b;
            }
        }

        protected FrameworkElement GetVisualiser(IVertex v)
        {
            FrameworkElement visualiser;

            if (!FastMode)
            {
                IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, v);

                StringViewVisualiser s = new StringViewVisualiser(baseEdgeVertex, null, false);

                //GraphUtil.ReplaceEdge(s.Vertex.Get(false, "BaseEdge:"), "To", v);

                s.ContextMenu = null; // no contextmenu, as there is gloal one for whole GraphVisualiser

                visualiser = s;
            }
            else
            {
                TextBlock b = new TextBlock();

                if (v.Value != null)
                    b.Text = v.Value.ToString();
                else
                    b.Text = "Ø";

                visualiser = b;
            }

            if (!ShowIcons)
                return visualiser;

            ImageSource iconSource = IconServer.GetIconByVertex(v);

            if (iconSource == null)
                return visualiser;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            Image iconImage = new Image();
            iconImage.Source = iconSource;
            iconImage.Width = WpfUtil.IconSize;
            iconImage.Height = WpfUtil.IconSize;
            iconImage.Margin = new Thickness(0, 0, 3, 0);
            iconImage.VerticalAlignment = VerticalAlignment.Center;
            iconImage.Stretch = Stretch.Uniform;
            iconImage.SnapsToDevicePixels = true;
            iconImage.UseLayoutRounding = true;

            RenderOptions.SetBitmapScalingMode(iconImage, BitmapScalingMode.Fant);

            visualiser.VerticalAlignment = VerticalAlignment.Center;

            panel.Children.Add(iconImage);
            panel.Children.Add(visualiser);

            return panel;
        }

        bool FastMode;
        bool MetaLabels;
        bool ShowOutEdges;
        bool ShowInEdges;
        bool AnimateEdges;
        bool ShowIcons;

        bool IsFirstPainted = false;

        public void PaintGraph()
        {
            if (Vertex.DisposedState != DisposeStateEnum.Live)
                return;

            if (ActualHeight != 0)
            {
                //MinusZero.Instance.Log(1, "PaintGraph", "");

                // turn off Vertex.Change listener

                //PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                //                                

                IsPaiting = true;

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "FastMode:"), "True"))
                    FastMode = true;
                else
                    FastMode = false;

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "MetaLabels:"), "True"))
                    MetaLabels = true;
                else
                    MetaLabels = false;

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "ShowOutEdges:"), "True"))
                    ShowOutEdges = true;
                else
                    ShowOutEdges = false;

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "ShowInEdges:"), "True"))
                    ShowInEdges = true;
                else
                    ShowInEdges = false;

                bool animateEdgesIsNull = false;
                AnimateEdges = GraphUtil.GetBooleanValue(Vertex.Get(false, "AnimateEdges:"), ref animateEdgesIsNull);

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "ShowIcons:"), "True"))
                    ShowIcons = true;
                else
                    ShowIcons = false;

                this.Children.Clear();

                foreach (UIElement e in DisplayedVerticesUIElements.Values)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();
                    
                DisplayedVerticesUIElements.Clear();

                //GraphUtil.RemoveAllEdges(Vertex.Get(false, "DisplayedEdges:"));
                
                Width = ((int)GraphUtil.GetIntegerValue(Vertex.Get(false, "NumberOfCircles:")))*(GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "VisualiserCircleSize:")))*2;
                Height = Width;                
                             
                AddCircle(0,null);

                SelectWrappersForSelectedVertices();

                IsFirstPainted = true;

                IsPaiting = false;

                // turn on Vertex.Change listener

                //PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                //
            }
        }

        internal void ApplyEdgeStyle(Shape edge, bool highlighted)
        {
            LineTagStore tag = edge.Tag as LineTagStore;
            bool isAnimationOverlay = tag != null && tag.IsAnimationOverlay;
            Brush brush = highlighted
                ? (Brush)FindResource("0LightHighlightBrush")
                : (Brush)FindResource("0LightGrayBrush");

            edge.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
            edge.StrokeDashOffset = 0;

            if (isAnimationOverlay)
            {
                if (!AnimateEdges)
                {
                    edge.Visibility = Visibility.Collapsed;
                    edge.StrokeDashArray = null;
                    return;
                }

                edge.Visibility = Visibility.Visible;
                edge.Stroke = highlighted
                    ? (Brush)FindResource("0HighlightBrush")
                    : (Brush)FindResource("0VeryLightGrayBrush");
                edge.Fill = Brushes.Transparent;
                edge.StrokeDashArray = new DoubleCollection { 8, 5 };
                edge.StrokeDashCap = PenLineCap.Flat;

                DoubleAnimation dashAnimation = new DoubleAnimation(0, -13, TimeSpan.FromSeconds(highlighted ? 0.9 : 1.8))
                {
                    RepeatBehavior = RepeatBehavior.Forever
                };
                edge.BeginAnimation(Shape.StrokeDashOffsetProperty, dashAnimation);
                return;
            }

            edge.Visibility = Visibility.Visible;
            edge.Stroke = brush;
            edge.Fill = brush;
            edge.StrokeDashArray = null;
        }

        Dictionary<IVertex, SimpleVisualiserWrapper> DisplayedVerticesUIElements;

        bool CanAddEdge(IEdge e)
        {
            if (GeneralUtil.CompareStrings(e.Meta, "$GraphChangeTrigger"))
                return false;

            return true;
        }

        protected void AddCircle(int level, IList<IVertex> InnerCircleVertices)
        {
            //MinusZero.Instance.Log(1,"AddCircle", level.ToString());

            IList<IVertex> CircleVertices=new List<IVertex>();

            if (level == 0)
            {
                IVertex b=Vertex.Get(false, @"BaseEdge:\To:");

                double x = this.Width / 2;
                double y = this.Height / 2;

                Add(x, y, (FrameworkElement)GetVisualiser(b),b).UpdateLayout();                

                CircleVertices.Add(b);

                AddCircle(1, CircleVertices);

                return;
            }

            int OutAndInEdgesCount = 0;

            SimpleVisualiserWrapper dummyPointIn = new SimpleVisualiserWrapper(null, null, this);
            SimpleVisualiserWrapper dummyPointOut = new SimpleVisualiserWrapper(null, null, this);

            foreach (IVertex v in InnerCircleVertices)
            {
                if (ShowOutEdges)
                    foreach (IEdge e in v)
                    if (!DisplayedVerticesUIElements.ContainsKey(e.To))
                    {
                        DisplayedVerticesUIElements.Add(e.To, dummyPointOut);
                        OutAndInEdgesCount++;
                    }

                if(ShowInEdges)
                foreach (IEdge e in v.InEdges)
                    if (!DisplayedVerticesUIElements.ContainsKey(e.From))
                    {
                        DisplayedVerticesUIElements.Add(e.From, dummyPointIn);
                        OutAndInEdgesCount++;
                    }
            }

            double cnt = 0;

            int CircleSize=0;
            
            int? _circleSize =GraphUtil.GetIntegerValue(Vertex.Get(false, "VisualiserCircleSize:"));
            if (_circleSize != null)
                CircleSize = (int)_circleSize;
            //IVertex DisplayedEdges = Vertex.Get(false, "DisplayedEdges:");

            if (OutAndInEdgesCount > 0)
                foreach (IVertex v in InnerCircleVertices)
                {
                    SimpleVisualiserWrapper vPoint = DisplayedVerticesUIElements[v];

                    if (ShowOutEdges)
                    foreach (IEdge e in v)
                        if (!DisplayedVerticesUIElements.ContainsKey(e.To) || DisplayedVerticesUIElements[e.To] == dummyPointOut)
                        {
                            if (CanAddEdge(e))
                            {
                                double x = (this.Width / 2) + Math.Cos(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;
                                double y = (this.Height / 2) + Math.Sin(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;

                                SimpleVisualiserWrapper toWrapper = Add(x, y, (FrameworkElement)GetVisualiser(e.To), e.To);

                                CircleVertices.Add(e.To);

                                AddLine(vPoint, toWrapper, e.Meta);
                            }

                            cnt++;
                        }
                        else
                        {
                            SimpleVisualiserWrapper eToPoint = DisplayedVerticesUIElements[e.To];

                            AddLine(vPoint, eToPoint,e.Meta);                            
                        }

                    if (ShowInEdges)
                    foreach (IEdge e in v.InEdges.ToList())
                        if (!DisplayedVerticesUIElements.ContainsKey(e.From) || DisplayedVerticesUIElements[e.From] == dummyPointIn)
                        {
                            if (CanAddEdge(e))
                            {
                                double x = (this.Width / 2) + Math.Cos(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;
                                double y = (this.Height / 2) + Math.Sin(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;

                                SimpleVisualiserWrapper fromWrapper = Add(x, y, (FrameworkElement)GetVisualiser(e.From), e.From);

                                CircleVertices.Add(e.From);

                                AddLine(fromWrapper, vPoint, e.Meta);
                            }                            

                            cnt++;
                        }
                        else
                        {
                            SimpleVisualiserWrapper eFromPoint = DisplayedVerticesUIElements[e.From];

                            AddLine(eFromPoint, vPoint, e.Meta);
                        }
                }

            if (level < GraphUtil.GetIntegerValue(Vertex.Get(false, "NumberOfCircles:")))
                AddCircle(level + 1, CircleVertices);
            else // lines from last circle
            {
                foreach (IVertex v in CircleVertices)
                {
                    SimpleVisualiserWrapper vPoint = DisplayedVerticesUIElements[v];
                    
                    if(ShowOutEdges)
                    foreach(IEdge e in v)
                        if(DisplayedVerticesUIElements.ContainsKey(e.To)) // if vertex is allready displayed, connect it
                        {
                            SimpleVisualiserWrapper eToPoint = DisplayedVerticesUIElements[e.To];

                            AddLine(vPoint, eToPoint,e.Meta);                            
                        }

                    if (ShowInEdges)
                        foreach (IEdge e in v.InEdges)
                            if (DisplayedVerticesUIElements.ContainsKey(e.From)) // if vertex is allready displayed, connect it
                            {
                                SimpleVisualiserWrapper eFromPoint = DisplayedVerticesUIElements[e.From];

                                AddLine(eFromPoint, vPoint, e.Meta);
                            }
                }
            }
        }        

        protected void SetVertexDefaultValues()
        {
            Vertex.Get(false, "Scale:").Value = 100;
            Vertex.Get(false, "VisualiserCircleSize:").Value = 200;
            Vertex.Get(false, "NumberOfCircles:").Value = 2;
            Vertex.Get(false, "FastMode:").Value = "True";
            Vertex.Get(false, "MetaLabels:").Value = "True";
            Vertex.Get(false, "ShowOutEdges:").Value = "True";
            Vertex.Get(false, "ShowIcons:").Value = "True";
        }        

        public void BaseEdgeToUpdated(){
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null)
            {
                PaintGraph();
            }
        }

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
            {
                if (ActualHeight != 0)
                {
                    this.LayoutTransform = new ScaleTransform(scale, scale, ActualWidth/2, ActualHeight/2);
                }
            }
            else
                this.LayoutTransform = null;
        }

        protected KeyValuePair<IVertex, SimpleVisualiserWrapper> GetVertexWrapperByEventSource(object eventSource)
        {
            DependencyObject current = eventSource as DependencyObject;

            while (current != null)
            {
                if (current is SimpleVisualiserWrapper)
                {
                    KeyValuePair<IVertex, SimpleVisualiserWrapper> wrapperMatch =
                        DisplayedVerticesUIElements.FirstOrDefault(x => x.Value == current);

                    if (wrapperMatch.Value != null)
                        return wrapperMatch;
                }

                KeyValuePair<IVertex, SimpleVisualiserWrapper> childMatch =
                    DisplayedVerticesUIElements.FirstOrDefault(x => x.Value.Child == current);

                if (childMatch.Value != null)
                    return childMatch;

                DependencyObject parent = null;

                if (current is Visual || current is System.Windows.Media.Media3D.Visual3D)
                    parent = VisualTreeHelper.GetParent(current);

                if (parent == null)
                    parent = LogicalTreeHelper.GetParent(current);

                current = parent;
            }

            return default(KeyValuePair<IVertex, SimpleVisualiserWrapper>);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp =
                GetVertexWrapperByEventSource(e.OriginalSource ?? e.Source);

            if (kvp.Value != null&&((SimpleVisualiserWrapper)kvp.Value).IsHighlighted==false)
            {
                SimpleVisualiserWrapper wrapper=(SimpleVisualiserWrapper)kvp.Value;

                if (Highlighted != null)
                    Highlighted.UnhighlightThisAndDescendants();

                wrapper.HighlightThisAndDescendants();
                
                Highlighted = wrapper;
            }

            base.OnMouseMove(e);
        }
        
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // switch to another BaseVertex
            {
                KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp =
                    GetVertexWrapperByEventSource(e.OriginalSource ?? e.Source);

                if (kvp.Key != null)
                {
                    SetKeyboardHighlightVertex(kvp.Key);

                    if (KeyboardHighlightActivated != null)
                    {
                        RaiseKeyboardHighlightActivated();
                        e.Handled = true;
                        return;
                    }

                    RestoreSelectedVertices();

                    GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", kvp.Key);

                    IVertex updatedBaseTo = Vertex.Get(false, @"BaseEdge:\To:");

                    if (updatedBaseTo == kvp.Key)
                        BaseEdgeToUpdated();
                }
            }

            if (e.ClickCount == 1) // change Selection
            {
                if (SelectionProphibited)
                    return;

                   KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp =
                       GetVertexWrapperByEventSource(e.OriginalSource ?? e.Source);

                   if (kvp.Key != null)
                   {
                       bool IsCtrl = false;

                       if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                           IsCtrl = true;

                       pendingMouseDownSelectionVertex = kvp.Key;
                       pendingMouseDownSelectionIsCtrl = IsCtrl;
                       suppressNextMouseUpSelection = false;

                }
            }
            
            e.Handled = true;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (!suppressNextMouseUpSelection && pendingMouseDownSelectionVertex != null)
                ApplyPendingMouseSelection();

            pendingMouseDownSelectionVertex = null;
            suppressNextMouseUpSelection = false;

            base.OnMouseUp(e);
        }

        private void ApplyPendingMouseSelection()
        {
            if (pendingMouseDownSelectionVertex == null
                || DisplayedVerticesUIElements == null
                || !DisplayedVerticesUIElements.ContainsKey(pendingMouseDownSelectionVertex))
                return;

            SimpleVisualiserWrapper wrapper = DisplayedVerticesUIElements[pendingMouseDownSelectionVertex];
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            Interaction.BeginInteractionWithGraph();

            if (pendingMouseDownSelectionIsCtrl)
            {
                if (wrapper.IsSelected)
                {
                    wrapper.Unselect();
                    EdgeHelper.DeleteVertexByEdgeTo(selectedEdges, pendingMouseDownSelectionVertex);
                }
                else
                {
                    wrapper.Select();
                    EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, pendingMouseDownSelectionVertex);
                }
            }
            else
            {
                UnselectAllSelected();
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                wrapper.Select();
                EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, pendingMouseDownSelectionVertex);
            }

            Interaction.EndInteractionWithGraph();

        }

        public void ClearKeyboardHighlight()
        {
            if (keyboardHighlightedVertex == null)
                return;

            if (DisplayedVerticesUIElements != null && DisplayedVerticesUIElements.ContainsKey(keyboardHighlightedVertex))
                DisplayedVerticesUIElements[keyboardHighlightedVertex].UnhighlightThis();

            keyboardHighlightedVertex = null;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
            List<IVertex> vertices = GetKeyboardHighlightVertices();

            if (vertices.Count == 0)
            {
                if (positionDelta < 0)
                    GoBeforeFirstKeyboardHighlightPosition();
                else
                    GoAfterLastKeyboardHighlightPosition();

                return;
            }

            int currentIndex = CurrentHighlightPosition;

            if (currentIndex < 0)
                currentIndex = positionDelta < 0 ? vertices.Count : -1;

            int newIndex = currentIndex + positionDelta;

            if (newIndex < 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else if (newIndex >= vertices.Count)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightVertex(vertices[newIndex]);
        }

        public void MoveKeyboardHighlight(KeyboardHighlightMoveDirection direction)
        {
            if (direction == KeyboardHighlightMoveDirection.Up || direction == KeyboardHighlightMoveDirection.Down)
                MoveKeyboardHighlight(direction == KeyboardHighlightMoveDirection.Up ? -1 : 1);
            else
                MoveKeyboardHighlightDirectional(direction);
        }

        public void ToggleKeyboardHighlightedEdgeSelection()
        {
            if (SelectionProphibited || keyboardHighlightedVertex == null)
                return;

            IVertex sv = Vertex.Get(false, "SelectedEdges:");
            IEdge selectedEdge = EdgeHelper.FindEdgeVertexByToVertex(sv, keyboardHighlightedVertex);

            Interaction.BeginInteractionWithGraph();

            if (selectedEdge != null)
                sv.DeleteEdge(selectedEdge);
            else
                EdgeHelper.AddEdgeVertexByToVertex(sv, keyboardHighlightedVertex);

            Interaction.EndInteractionWithGraph();
            SelectedVerticesUpdated();
        }

        private void SetKeyboardHighlightToFirst()
        {
            List<IVertex> vertices = GetKeyboardHighlightVertices();

            if (vertices.Count == 0)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightVertex(vertices[0]);
        }

        private void SetKeyboardHighlightToLast()
        {
            List<IVertex> vertices = GetKeyboardHighlightVertices();

            if (vertices.Count == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightVertex(vertices[vertices.Count - 1]);
        }

        private void SetKeyboardHighlightVertex(IVertex vertex)
        {
            ClearKeyboardHighlight();

            if (vertex == null || DisplayedVerticesUIElements == null || !DisplayedVerticesUIElements.ContainsKey(vertex))
                return;

            keyboardHighlightedVertex = vertex;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
            DisplayedVerticesUIElements[vertex].HighlightThis();
            DisplayedVerticesUIElements[vertex].BringIntoView();
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

        private List<IVertex> GetKeyboardHighlightVertices()
        {
            if (DisplayedVerticesUIElements == null)
                return new List<IVertex>();

            return DisplayedVerticesUIElements
                .Where(x => x.Key != null && x.Value != null && x.Value.Child != null)
                .OrderBy(x => Canvas.GetTop(x.Value))
                .ThenBy(x => Canvas.GetLeft(x.Value))
                .Select(x => x.Key)
                .ToList();
        }

        private void MoveKeyboardHighlightDirectional(KeyboardHighlightMoveDirection direction)
        {
            if (keyboardHighlightedVertex == null)
            {
                SetKeyboardHighlightToFirst();
                return;
            }

            if (DisplayedVerticesUIElements == null || !DisplayedVerticesUIElements.ContainsKey(keyboardHighlightedVertex))
                return;

            SimpleVisualiserWrapper currentWrapper = DisplayedVerticesUIElements[keyboardHighlightedVertex];
            Point current = GetWrapperCenter(currentWrapper);
            IVertex bestVertex = null;
            double bestScore = double.MaxValue;

            foreach (KeyValuePair<IVertex, SimpleVisualiserWrapper> item in DisplayedVerticesUIElements)
            {
                if (item.Key == null || item.Key == keyboardHighlightedVertex || item.Value == null || item.Value.Child == null)
                    continue;

                Point candidate = GetWrapperCenter(item.Value);
                double dx = candidate.X - current.X;
                double dy = candidate.Y - current.Y;

                if (!IsCandidateInDirection(direction, dx, dy))
                    continue;

                double score = (dx * dx) + (dy * dy);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestVertex = item.Key;
                }
            }

            if (bestVertex != null)
                SetKeyboardHighlightVertex(bestVertex);
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

        private void RaiseKeyboardHighlightActivated()
        {
            if (KeyboardHighlightedEdge == null)
                return;

            if (KeyboardHighlightActivated != null)
                KeyboardHighlightActivated(this, EventArgs.Empty);

            if (KeyboardHighlightEnterPressed != null)
                KeyboardHighlightEnterPressed(this, EventArgs.Empty);
        }

        protected void UnselectAllSelected()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge v in sv)
                if (v.To.Get(false, "To:") != null && DisplayedVerticesUIElements.ContainsKey(v.To.Get(false, "To:")))
                    DisplayedVerticesUIElements[v.To.Get(false, "To:")].Unselect();            
        }        

        private IVertex pendingMouseDownSelectionVertex;
        private bool pendingMouseDownSelectionIsCtrl;
        private bool suppressNextMouseUpSelection;

        private int GetSelectedEdgesCountForDndLog()
        {
            IVertex selectedEdges = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            return selectedEdges == null ? 0 : selectedEdges.Count();
        }

        private static string GetVertexLogValue(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            return vertex.Value == null ? "null-value" : vertex.Value.ToString();
        }

        IVertex tempSelectedVertices;

        protected void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();
            tempSelectedVertices.AddExternalReference();

            GraphUtil.CopyShallow(Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"), tempSelectedVertices);
        }

        protected void RestoreSelectedVertices()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            if (tempSelectedVertices != null)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

                GraphUtil.CopyShallow(tempSelectedVertices, sv);

                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(tempSelectedVertices); // 11.10.2018 ADDED. should cause no problems
                tempSelectedVertices.RemoveExternalReference();
                tempSelectedVertices = null;
            }
        }

        protected void UnselectAll()
        {
            foreach(KeyValuePair<IVertex,SimpleVisualiserWrapper> key in DisplayedVerticesUIElements)
                key.Value.Unselect();
        }

        public void UnselectAllSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 
            
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        public void SelectedVerticesUpdated()
        {
            if (IsFirstPainted)
            {
                UnselectAll();

                SelectWrappersForSelectedVertices();
            }

            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
        }

        protected void SelectWrappersForSelectedVertices()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge e in sv)
            {
                if (e.To.Get(false, "To:")!=null&&DisplayedVerticesUIElements.ContainsKey(e.To.Get(false, "To:")))
                    DisplayedVerticesUIElements[e.To.Get(false, "To:")].Select();
            }
        }


        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }


        // IHasLocalizableEdges

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByPoint(Point p)
        {
            vertexByLocationToReturn = null;

            foreach (KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp in DisplayedVerticesUIElements)
            {
                if (VisualTreeHelper.HitTest(kvp.Value, TranslatePoint(p, kvp.Value)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    EdgeHelper.AddEdgeVertexEdgesOnlyTo(v, kvp.Value.baseVertex);
                    vertexByLocationToReturn = v;
                }
            }

            // DO WANT THIS FEATURE
            if (vertexByLocationToReturn == null&&GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                vertexByLocationToReturn = Vertex.Get(false, @"BaseEdge:");

            return vertexByLocationToReturn;
        }
        
        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        ///// DRAG AND DROP

        Point dndStartPoint;
        bool hasButtonBeenDown;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);
            hasButtonBeenDown = true;

            CopySelectedVerticesToTemp();

            MinusZero.Instance.IsGUIDragging = false;
        }

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown &&
                !WpfUtil.IsMouseOverScrollbar(sender, dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) & (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                suppressNextMouseUpSelection = true;
                pendingMouseDownSelectionVertex = null;

                RestoreSelectedVertices();

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (Vertex.Get(false, @"SelectedEdges:\{$Is:Edge}") != null)
                    foreach (IEdge ee in Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"))
                        dndVertex.AddEdge(null, ee.To);
                else
                {
                    IVertex v = GetEdgeByPoint(dndStartPoint);
                    if (v != null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    dndVertex.AddExternalReference();

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = GetEdgeByPoint(e.GetPosition(this));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get(false, "BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get(false, "To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }

        // REPOSITION

        public static INoInEdgeInOutVertexVertex Reposition(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex AlgorithmVertex = GraphUtil.GetQueryOutFirst(stack, "Algorithm", null);

            RepositionAlgorithmEnum Reposition = RepositionAlgorithmEnumHelper.GetEnum(AlgorithmVertex);

            IVertex visualiserVertex = GraphUtil.GetQueryOutFirst(stack, "this", null);

            GraphVisualiser visualiser = (GraphVisualiser)VisualisersList.GetVisualiser(visualiserVertex);

            if (visualiser == null)
            {
                UserInteractionUtil.ShowException("GraphVisualiser", "GraphVisualiser instance not found for baseVertex", ExceptionLevelEnum.Error);
                
                return stack;
            }

            visualiser.Dispatcher.Invoke(() => visualiser.RepositionGraph(Reposition));

            return stack;
        }

        public void RepositionGraph(RepositionAlgorithmEnum algorithm)
        {
            if (DisplayedVerticesUIElements == null || DisplayedVerticesUIElements.Count == 0)
                return;

            List<SimpleVisualiserWrapper> wrappers = GetDistinctWrappers();
            if (wrappers.Count == 0) return;

            foreach (SimpleVisualiserWrapper w in wrappers) w.UpdateLayout();

            MinusZero.Instance.Log(1, "GraphVisualiser.RepositionGraph", algorithm.ToString() + " on " + wrappers.Count + " vertices");

            switch (algorithm)
            {
                case RepositionAlgorithmEnum.Radial:   ApplyRadialLayout(wrappers);      break;
                case RepositionAlgorithmEnum.Force:    ApplyForceLayout(wrappers);       break;
                case RepositionAlgorithmEnum.Sugiyama: ApplySugiyamaLayout(wrappers);    break;
                case RepositionAlgorithmEnum.Kamada:   ApplyKamadaKawaiLayout(wrappers); break;
                case RepositionAlgorithmEnum.Tree:     ApplyTreeLayout(wrappers);        break;
                default:                               ApplyRadialLayout(wrappers);      break;
            }

            // Post-process (6): rectangle overlap removal - applied for every algorithm
            ApplyOverlapRemoval(wrappers);

            UpdateAllLines();
        }

        // HELPERS ============================================================

        private List<SimpleVisualiserWrapper> GetDistinctWrappers()
        {
            HashSet<SimpleVisualiserWrapper> seen = new HashSet<SimpleVisualiserWrapper>();
            List<SimpleVisualiserWrapper> result = new List<SimpleVisualiserWrapper>();
            foreach (SimpleVisualiserWrapper w in DisplayedVerticesUIElements.Values)
                if (w != null && w.baseVertex != null && seen.Add(w))
                    result.Add(w);
            return result;
        }

        private void SetWrapperCenter(SimpleVisualiserWrapper w, double cx, double cy)
        {
            Canvas.SetLeft(w, cx - w.ActualWidth / 2);
            Canvas.SetTop(w, cy - w.ActualHeight / 2);
        }

        private Point GetWrapperCenter(SimpleVisualiserWrapper w)
        {
            return new Point(Canvas.GetLeft(w) + w.ActualWidth / 2,
                             Canvas.GetTop(w) + w.ActualHeight / 2);
        }

        private Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> BuildUndirectedAdjacency(List<SimpleVisualiserWrapper> wrappers)
        {
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> adj =
                new Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>>();
            foreach (SimpleVisualiserWrapper w in wrappers) adj[w] = new List<SimpleVisualiserWrapper>();

            foreach (SimpleVisualiserWrapper w in wrappers)
                foreach (Shape line in w.Lines)
                {
                    LineTagStore lts = line.Tag as LineTagStore;
                    if (lts == null) continue;

                    SimpleVisualiserWrapper other = lts.FromWrapper == w ? lts.ToWrapper : lts.FromWrapper;

                    if (other == null || other == w) continue;
                    if (!adj.ContainsKey(other)) continue;
                    if (!adj[w].Contains(other)) adj[w].Add(other);
                }
            return adj;
        }

        private void BuildDirectedAdjacency(List<SimpleVisualiserWrapper> wrappers,
            out Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> outAdj,
            out Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> inAdj)
        {
            outAdj = new Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>>();
            inAdj = new Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>>();
            foreach (SimpleVisualiserWrapper w in wrappers)
            {
                outAdj[w] = new List<SimpleVisualiserWrapper>();
                inAdj[w] = new List<SimpleVisualiserWrapper>();
            }

            foreach (SimpleVisualiserWrapper w in wrappers)
                foreach (Shape line in w.Lines)
                {
                    LineTagStore lts = line.Tag as LineTagStore;
                    if (lts == null) continue;
                    if (lts.FromWrapper != w) continue; // each edge processed once, from its FromWrapper
                    if (lts.ToWrapper == null || !outAdj.ContainsKey(lts.ToWrapper)) continue;
                    if (lts.FromWrapper == lts.ToWrapper) continue;
                    if (!outAdj[w].Contains(lts.ToWrapper))
                    {
                        outAdj[w].Add(lts.ToWrapper);
                        inAdj[lts.ToWrapper].Add(w);
                    }
                }
        }

        private SimpleVisualiserWrapper PickRootWrapper(List<SimpleVisualiserWrapper> wrappers)
        {
            IVertex baseTo = Vertex.Get(false, @"BaseEdge:\To:");
            if (baseTo != null && DisplayedVerticesUIElements.ContainsKey(baseTo))
            {
                SimpleVisualiserWrapper w = DisplayedVerticesUIElements[baseTo];
                if (wrappers.Contains(w)) return w;
            }
            return wrappers[0];
        }

        private double GetCanvasWidth()  { return this.Width  > 0 ? this.Width  : Math.Max(this.ActualWidth,  800); }
        private double GetCanvasHeight() { return this.Height > 0 ? this.Height : Math.Max(this.ActualHeight, 800); }

        private void ExpandCanvasForRelaxedLayout(double factor)
        {
            int numberOfCircles = GraphUtil.GetIntegerValue(Vertex.Get(false, "NumberOfCircles:")) ?? 2;
            int circleSize = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "VisualiserCircleSize:"));
            if (circleSize <= 0) circleSize = 200;

            double baseSize = Math.Max(800, numberOfCircles * circleSize * 2);
            double relaxedSize = baseSize * factor;

            if (Width < relaxedSize) Width = relaxedSize;
            if (Height < relaxedSize) Height = relaxedSize;
        }

        // 1) RADIAL (improved BFS) ==========================================

        private void ApplyRadialLayout(List<SimpleVisualiserWrapper> wrappers)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> adj = BuildUndirectedAdjacency(wrappers);
            SimpleVisualiserWrapper root = PickRootWrapper(wrappers);

            Dictionary<SimpleVisualiserWrapper, int> level = new Dictionary<SimpleVisualiserWrapper, int>();
            Queue<SimpleVisualiserWrapper> q = new Queue<SimpleVisualiserWrapper>();
            q.Enqueue(root);
            level[root] = 0;
            while (q.Count > 0)
            {
                SimpleVisualiserWrapper c = q.Dequeue();
                foreach (SimpleVisualiserWrapper n in adj[c])
                    if (!level.ContainsKey(n)) { level[n] = level[c] + 1; q.Enqueue(n); }
            }

            int maxLevel = level.Count > 0 ? level.Values.Max() : 0;
            foreach (SimpleVisualiserWrapper w in wrappers)
                if (!level.ContainsKey(w)) level[w] = maxLevel + 1;

            int circleSize = (int)GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "VisualiserCircleSize:"));
            if (circleSize <= 0) circleSize = 200;

            double cx = GetCanvasWidth() / 2;
            double cy = GetCanvasHeight() / 2;

            // Group by level and pre-compute per-level max half-height so we can keep
            // adjacent rings from crashing into each other.
            List<IGrouping<int, KeyValuePair<SimpleVisualiserWrapper, int>>> byLevel =
                level.GroupBy(kv => kv.Value).OrderBy(g => g.Key).ToList();

            Dictionary<int, double> maxHalfHeightAtLevel = new Dictionary<int, double>();
            foreach (IGrouping<int, KeyValuePair<SimpleVisualiserWrapper, int>> g in byLevel)
            {
                double mh = 0;
                foreach (KeyValuePair<SimpleVisualiserWrapper, int> kv in g)
                    if (kv.Key.ActualHeight / 2 > mh) mh = kv.Key.ActualHeight / 2;
                maxHalfHeightAtLevel[g.Key] = mh;
            }

            double previousRadius = 0;
            double previousHalfHeight = 0;

            const double angularPadding = 15;
            const double ringPadding = 20;

            foreach (IGrouping<int, KeyValuePair<SimpleVisualiserWrapper, int>> g in byLevel)
            {
                int lvl = g.Key;
                List<SimpleVisualiserWrapper> atLevel = g.Select(kv => kv.Key).ToList();

                if (lvl == 0)
                {
                    foreach (SimpleVisualiserWrapper w in atLevel) SetWrapperCenter(w, cx, cy);
                    previousRadius = 0;
                    previousHalfHeight = maxHalfHeightAtLevel[lvl];
                    continue;
                }

                // Slice weight uses the width, because the tangential direction on the
                // ring is dominated by width; height is handled by radial spacing below.
                double totalWeight = atLevel.Sum(w => w.ActualWidth + angularPadding);
                if (totalWeight <= 0) totalWeight = atLevel.Count;

                // Minimum radius so all rectangles fit around the ring without angular overlap.
                double radiusFromNodes = totalWeight / (2 * Math.PI);
                // Minimum radius so this ring does not collide with previous ring radially.
                double radiusFromPrevious = previousRadius + previousHalfHeight + maxHalfHeightAtLevel[lvl] + ringPadding;
                double radius = Math.Max(Math.Max(circleSize * lvl, radiusFromNodes), radiusFromPrevious);

                double angleAccFraction = 0;
                foreach (SimpleVisualiserWrapper w in atLevel)
                {
                    double weight = w.ActualWidth + angularPadding;
                    double share = weight / totalWeight;
                    double mid = angleAccFraction + share / 2;
                    double a = mid * 2 * Math.PI;
                    double x = cx + Math.Cos(a) * radius;
                    double y = cy + Math.Sin(a) * radius;
                    SetWrapperCenter(w, x, y);
                    angleAccFraction += share;
                }

                previousRadius = radius;
                previousHalfHeight = maxHalfHeightAtLevel[lvl];
            }

            sw.Stop();
            MinusZero.Instance.Log(1, "GraphVisualiser.ApplyRadialLayout",
                "wrappers=" + wrappers.Count + " rings=" + byLevel.Count + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        // 2) FORCE-DIRECTED (Fruchterman-Reingold with rectangle overlap) =====

        private void ApplyForceLayout(List<SimpleVisualiserWrapper> wrappers)
        {
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> adj = BuildUndirectedAdjacency(wrappers);

            ExpandCanvasForRelaxedLayout(2.0);

            double width = GetCanvasWidth();
            double height = GetCanvasHeight();
            int n = wrappers.Count;
            double area = width * height;
            double k = Math.Sqrt(area / Math.Max(1, n));

            Random rand = new Random(42);
            Dictionary<SimpleVisualiserWrapper, Point> pos = new Dictionary<SimpleVisualiserWrapper, Point>();
            foreach (SimpleVisualiserWrapper w in wrappers)
            {
                double px = Canvas.GetLeft(w); if (double.IsNaN(px)) px = rand.NextDouble() * width;
                double py = Canvas.GetTop(w);  if (double.IsNaN(py)) py = rand.NextDouble() * height;
                pos[w] = new Point(px + w.ActualWidth / 2, py + w.ActualHeight / 2);
            }

            // Collect edges (each pair once, undirected)
            List<KeyValuePair<SimpleVisualiserWrapper, SimpleVisualiserWrapper>> edges =
                new List<KeyValuePair<SimpleVisualiserWrapper, SimpleVisualiserWrapper>>();
            HashSet<string> seen = new HashSet<string>();
            for (int i = 0; i < wrappers.Count; i++)
            {
                SimpleVisualiserWrapper a = wrappers[i];
                foreach (SimpleVisualiserWrapper b in adj[a])
                {
                    int ai = i, bi = wrappers.IndexOf(b);
                    if (bi < 0) continue;
                    string key = ai < bi ? ai + ":" + bi : bi + ":" + ai;
                    if (seen.Add(key)) edges.Add(new KeyValuePair<SimpleVisualiserWrapper, SimpleVisualiserWrapper>(a, b));
                }
            }

            int iterations = 200;
            double temperature = Math.Max(width, height) / 10.0;
            double cooling = Math.Pow(0.02, 1.0 / iterations); // reach ~2% of initial temp

            Dictionary<SimpleVisualiserWrapper, Vector> disp = new Dictionary<SimpleVisualiserWrapper, Vector>();

            for (int iter = 0; iter < iterations; iter++)
            {
                foreach (SimpleVisualiserWrapper w in wrappers) disp[w] = new Vector(0, 0);

                // Repulsion
                for (int i = 0; i < wrappers.Count; i++)
                    for (int j = i + 1; j < wrappers.Count; j++)
                    {
                        SimpleVisualiserWrapper v = wrappers[i];
                        SimpleVisualiserWrapper u = wrappers[j];
                        double dx = pos[v].X - pos[u].X;
                        double dy = pos[v].Y - pos[u].Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist < 0.01) { dx = (rand.NextDouble() - 0.5) * 0.1; dy = (rand.NextDouble() - 0.5) * 0.1; dist = 0.01; }

                        // Additional push proportional to rectangle overlap - accounts for node size.
                        double requiredDx = (v.ActualWidth + u.ActualWidth) / 2 + 10;
                        double requiredDy = (v.ActualHeight + u.ActualHeight) / 2 + 10;
                        double overlapX = requiredDx - Math.Abs(dx);
                        double overlapY = requiredDy - Math.Abs(dy);
                        double rectPush = 0;
                        if (overlapX > 0 && overlapY > 0)
                            rectPush = Math.Min(overlapX, overlapY) * 5;

                        double force = (k * k) / dist + rectPush;
                        double ux = dx / dist;
                        double uy = dy / dist;
                        disp[v] = new Vector(disp[v].X + ux * force, disp[v].Y + uy * force);
                        disp[u] = new Vector(disp[u].X - ux * force, disp[u].Y - uy * force);
                    }

                // Attraction along edges
                foreach (KeyValuePair<SimpleVisualiserWrapper, SimpleVisualiserWrapper> e in edges)
                {
                    double dx = pos[e.Key].X - pos[e.Value].X;
                    double dy = pos[e.Key].Y - pos[e.Value].Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < 0.01) dist = 0.01;
                    double force = (dist * dist) / k;
                    double ux = dx / dist;
                    double uy = dy / dist;
                    disp[e.Key]   = new Vector(disp[e.Key].X   - ux * force, disp[e.Key].Y   - uy * force);
                    disp[e.Value] = new Vector(disp[e.Value].X + ux * force, disp[e.Value].Y + uy * force);
                }

                // Apply bounded by temperature
                foreach (SimpleVisualiserWrapper w in wrappers)
                {
                    Vector d = disp[w];
                    double dlen = Math.Sqrt(d.X * d.X + d.Y * d.Y);
                    if (dlen > 0)
                    {
                        double move = Math.Min(dlen, temperature);
                        pos[w] = new Point(pos[w].X + (d.X / dlen) * move, pos[w].Y + (d.Y / dlen) * move);
                    }
                }

                temperature *= cooling;
            }

            NormalizePositions(wrappers, pos);
            foreach (KeyValuePair<SimpleVisualiserWrapper, Point> kv in pos)
                SetWrapperCenter(kv.Key, kv.Value.X, kv.Value.Y);
        }

        // 3) SUGIYAMA (layered) ==============================================

        private void ApplySugiyamaLayout(List<SimpleVisualiserWrapper> wrappers)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> outAdj;
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> inAdj;
            BuildDirectedAdjacency(wrappers, out outAdj, out inAdj);

            // Layer assignment via Kahn's topological sort with cycle breaking.
            // Without this, a cycle A -> B -> A made the previous "longest path" loop
            // increment both layers every iteration until guard ran out, sending nodes
            // thousands of pixels below the canvas.
            Dictionary<SimpleVisualiserWrapper, int> remainingInDeg = new Dictionary<SimpleVisualiserWrapper, int>();
            foreach (SimpleVisualiserWrapper w in wrappers) remainingInDeg[w] = inAdj[w].Count;

            Dictionary<SimpleVisualiserWrapper, int> layer = new Dictionary<SimpleVisualiserWrapper, int>();
            Queue<SimpleVisualiserWrapper> readyQueue = new Queue<SimpleVisualiserWrapper>();
            HashSet<SimpleVisualiserWrapper> processed = new HashSet<SimpleVisualiserWrapper>();

            foreach (SimpleVisualiserWrapper w in wrappers)
                if (remainingInDeg[w] == 0) { readyQueue.Enqueue(w); layer[w] = 0; }

            int maxAllowedLayer = Math.Max(1, wrappers.Count - 1);
            int cyclesBroken = 0;

            while (processed.Count < wrappers.Count)
            {
                if (readyQueue.Count == 0)
                {
                    // Cycle: pick an unprocessed node with the smallest remaining
                    // in-degree and force-start it; one of its in-edges becomes a
                    // "back edge" that gets ignored for layering purposes.
                    SimpleVisualiserWrapper pick = null;
                    int minDeg = int.MaxValue;
                    foreach (SimpleVisualiserWrapper w in wrappers)
                    {
                        if (processed.Contains(w)) continue;
                        if (remainingInDeg[w] < minDeg) { minDeg = remainingInDeg[w]; pick = w; }
                    }
                    if (pick == null) break;

                    int maxP = -1;
                    foreach (SimpleVisualiserWrapper p in inAdj[pick])
                        if (layer.TryGetValue(p, out int lp) && lp > maxP) maxP = lp;
                    int lw = Math.Min(maxP + 1, maxAllowedLayer);
                    layer[pick] = lw;
                    readyQueue.Enqueue(pick);
                    cyclesBroken++;
                }

                SimpleVisualiserWrapper v = readyQueue.Dequeue();
                if (!processed.Add(v)) continue;

                foreach (SimpleVisualiserWrapper u in outAdj[v])
                {
                    if (processed.Contains(u)) continue;
                    int currentLayer = layer.ContainsKey(u) ? layer[u] : 0;
                    int candidate = Math.Min(layer[v] + 1, maxAllowedLayer);
                    if (candidate > currentLayer) layer[u] = candidate;
                    else if (!layer.ContainsKey(u)) layer[u] = currentLayer;

                    remainingInDeg[u]--;
                    if (remainingInDeg[u] <= 0) readyQueue.Enqueue(u);
                }
            }

            foreach (SimpleVisualiserWrapper w in wrappers)
                if (!layer.ContainsKey(w)) layer[w] = 0;

            Dictionary<int, List<SimpleVisualiserWrapper>> layers = layer
                .GroupBy(kv => kv.Value)
                .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

            int maxLayer = layers.Keys.Max();

            // Barycenter crossing reduction (a few sweeps).
            for (int sweep = 0; sweep < 16; sweep++)
            {
                for (int L = 1; L <= maxLayer; L++)
                {
                    if (!layers.ContainsKey(L)) continue;
                    List<SimpleVisualiserWrapper> prev = layers.ContainsKey(L - 1) ? layers[L - 1] : new List<SimpleVisualiserWrapper>();
                    layers[L].Sort((a, b) => Barycenter(a, inAdj, prev).CompareTo(Barycenter(b, inAdj, prev)));
                }
                for (int L = maxLayer - 1; L >= 0; L--)
                {
                    if (!layers.ContainsKey(L)) continue;
                    List<SimpleVisualiserWrapper> next = layers.ContainsKey(L + 1) ? layers[L + 1] : new List<SimpleVisualiserWrapper>();
                    layers[L].Sort((a, b) => Barycenter(a, outAdj, next).CompareTo(Barycenter(b, outAdj, next)));
                }
            }

            // Coordinate assignment - squeeze vertical spacing so the whole stack
            // fits within the canvas height even for deep layerings.
            double xPadding = 40;
            double canvasH = GetCanvasHeight();

            double maxLayerHeight = 0;
            foreach (KeyValuePair<int, List<SimpleVisualiserWrapper>> kv in layers)
                foreach (SimpleVisualiserWrapper w in kv.Value)
                    if (w.ActualHeight > maxLayerHeight) maxLayerHeight = w.ActualHeight;

            double topMargin = Math.Max(40, maxLayerHeight / 2 + 20);
            double bottomMargin = Math.Max(40, maxLayerHeight / 2 + 20);
            double available = Math.Max(100, canvasH - topMargin - bottomMargin);
            double preferredYPadding = 140;
            double yPadding = maxLayer > 0 ? Math.Min(preferredYPadding, available / maxLayer) : preferredYPadding;
            if (yPadding < maxLayerHeight + 20) yPadding = maxLayerHeight + 20; // do not collide with ring above

            double cx = GetCanvasWidth() / 2;
            double startY = topMargin;

            foreach (KeyValuePair<int, List<SimpleVisualiserWrapper>> kv in layers.OrderBy(k => k.Key))
            {
                int L = kv.Key;
                List<SimpleVisualiserWrapper> nodes = kv.Value;
                double totalW = nodes.Sum(w => w.ActualWidth + xPadding);
                double curX = cx - totalW / 2;
                foreach (SimpleVisualiserWrapper w in nodes)
                {
                    double nodeX = curX + (w.ActualWidth + xPadding) / 2;
                    double nodeY = startY + L * yPadding;
                    SetWrapperCenter(w, nodeX, nodeY);
                    curX += w.ActualWidth + xPadding;
                }
            }

            sw.Stop();
            MinusZero.Instance.Log(1, "GraphVisualiser.ApplySugiyamaLayout",
                "wrappers=" + wrappers.Count + " maxLayer=" + maxLayer + " cyclesBroken=" + cyclesBroken +
                " yPadding=" + yPadding.ToString("F1") + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        private double Barycenter(SimpleVisualiserWrapper w,
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> adj,
            List<SimpleVisualiserWrapper> referenceLayer)
        {
            if (!adj.ContainsKey(w) || adj[w].Count == 0 || referenceLayer.Count == 0)
                return referenceLayer.IndexOf(w);

            double sum = 0;
            int count = 0;
            foreach (SimpleVisualiserWrapper n in adj[w])
            {
                int idx = referenceLayer.IndexOf(n);
                if (idx >= 0) { sum += idx; count++; }
            }
            if (count == 0) return 0;
            return sum / count;
        }

        // 4) KAMADA-KAWAI (stress-based) =====================================

        private void ApplyKamadaKawaiLayout(List<SimpleVisualiserWrapper> wrappers)
        {
            ExpandCanvasForRelaxedLayout(2.0);

            int n = wrappers.Count;
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> adj = BuildUndirectedAdjacency(wrappers);

            // All-pairs shortest paths via BFS.
            Dictionary<SimpleVisualiserWrapper, Dictionary<SimpleVisualiserWrapper, int>> dist =
                new Dictionary<SimpleVisualiserWrapper, Dictionary<SimpleVisualiserWrapper, int>>();
            foreach (SimpleVisualiserWrapper s in wrappers)
            {
                Dictionary<SimpleVisualiserWrapper, int> d = new Dictionary<SimpleVisualiserWrapper, int>();
                Queue<SimpleVisualiserWrapper> q = new Queue<SimpleVisualiserWrapper>();
                q.Enqueue(s); d[s] = 0;
                while (q.Count > 0)
                {
                    SimpleVisualiserWrapper c = q.Dequeue();
                    foreach (SimpleVisualiserWrapper nn in adj[c])
                        if (!d.ContainsKey(nn)) { d[nn] = d[c] + 1; q.Enqueue(nn); }
                }
                dist[s] = d;
            }

            int diameter = 1;
            foreach (KeyValuePair<SimpleVisualiserWrapper, Dictionary<SimpleVisualiserWrapper, int>> kv in dist)
                foreach (int v in kv.Value.Values) if (v > diameter) diameter = v;

            double canvasSize = Math.Min(GetCanvasWidth(), GetCanvasHeight());
            double L = (canvasSize * 0.8) / diameter;
            double K = 1.0;

            // Initial positions on a circle.
            Dictionary<SimpleVisualiserWrapper, Point> pos = new Dictionary<SimpleVisualiserWrapper, Point>();
            double cx = GetCanvasWidth() / 2;
            double cy = GetCanvasHeight() / 2;
            double R = canvasSize / 3;
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * i / Math.Max(1, n);
                pos[wrappers[i]] = new Point(cx + R * Math.Cos(a), cy + R * Math.Sin(a));
            }

            int iterations = 150;
            for (int iter = 0; iter < iterations; iter++)
            {
                double maxDelta = 0;
                foreach (SimpleVisualiserWrapper m in wrappers)
                {
                    double dxSum = 0, dySum = 0;
                    foreach (SimpleVisualiserWrapper i in wrappers)
                    {
                        if (i == m) continue;
                        if (!dist[m].ContainsKey(i)) continue; // disconnected component
                        int dmi = dist[m][i];
                        if (dmi == 0) continue;
                        double lmi = L * dmi;
                        double kmi = K / (dmi * dmi);
                        double dx = pos[m].X - pos[i].X;
                        double dy = pos[m].Y - pos[i].Y;
                        double dd = Math.Sqrt(dx * dx + dy * dy);
                        if (dd < 0.01) dd = 0.01;
                        dxSum += kmi * (dx - lmi * dx / dd);
                        dySum += kmi * (dy - lmi * dy / dd);
                    }
                    double delta = Math.Sqrt(dxSum * dxSum + dySum * dySum);
                    if (delta > 0.01)
                    {
                        double step = Math.Min(delta * 0.1, 20);
                        pos[m] = new Point(pos[m].X - (dxSum / delta) * step,
                                           pos[m].Y - (dySum / delta) * step);
                        if (delta > maxDelta) maxDelta = delta;
                    }
                }
                if (maxDelta < 0.5) break;
            }

            NormalizePositions(wrappers, pos);
            foreach (KeyValuePair<SimpleVisualiserWrapper, Point> kv in pos)
                SetWrapperCenter(kv.Key, kv.Value.X, kv.Value.Y);
        }

        // 5) TREE (Reingold-Tilford style - simple subtree-width layout) =====

        private void ApplyTreeLayout(List<SimpleVisualiserWrapper> wrappers)
        {
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> adj = BuildUndirectedAdjacency(wrappers);
            SimpleVisualiserWrapper root = PickRootWrapper(wrappers);

            // BFS spanning tree.
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> children =
                new Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>>();
            foreach (SimpleVisualiserWrapper w in wrappers) children[w] = new List<SimpleVisualiserWrapper>();

            HashSet<SimpleVisualiserWrapper> visited = new HashSet<SimpleVisualiserWrapper> { root };
            Queue<SimpleVisualiserWrapper> q = new Queue<SimpleVisualiserWrapper>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                SimpleVisualiserWrapper c = q.Dequeue();
                foreach (SimpleVisualiserWrapper nb in adj[c])
                    if (visited.Add(nb)) { children[c].Add(nb); q.Enqueue(nb); }
            }
            // Disconnected nodes: attach as extra roots under the synthetic root.
            foreach (SimpleVisualiserWrapper w in wrappers)
                if (visited.Add(w)) children[root].Add(w);

            double xGap = 30;
            double yGap = 140;

            Dictionary<SimpleVisualiserWrapper, double> subtreeWidth = new Dictionary<SimpleVisualiserWrapper, double>();
            ComputeSubtreeWidth(root, children, subtreeWidth, xGap);

            double startX = GetCanvasWidth() / 2 - subtreeWidth[root] / 2;
            PlaceTreeNode(root, children, subtreeWidth, startX, 60, yGap);
        }

        private double ComputeSubtreeWidth(SimpleVisualiserWrapper w,
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> children,
            Dictionary<SimpleVisualiserWrapper, double> cache,
            double xGap)
        {
            if (cache.ContainsKey(w)) return cache[w];
            double own = w.ActualWidth + xGap;
            if (children[w].Count == 0) { cache[w] = own; return own; }
            double sum = 0;
            foreach (SimpleVisualiserWrapper c in children[w]) sum += ComputeSubtreeWidth(c, children, cache, xGap);
            double result = Math.Max(own, sum);
            cache[w] = result;
            return result;
        }

        private void PlaceTreeNode(SimpleVisualiserWrapper w,
            Dictionary<SimpleVisualiserWrapper, List<SimpleVisualiserWrapper>> children,
            Dictionary<SimpleVisualiserWrapper, double> subtreeWidth,
            double xLeft, double y, double yGap)
        {
            double nodeCx = xLeft + subtreeWidth[w] / 2;
            SetWrapperCenter(w, nodeCx, y);

            if (children[w].Count == 0) return;

            double totalChildren = 0;
            foreach (SimpleVisualiserWrapper c in children[w]) totalChildren += subtreeWidth[c];

            double childX = xLeft + (subtreeWidth[w] - totalChildren) / 2;
            foreach (SimpleVisualiserWrapper c in children[w])
            {
                PlaceTreeNode(c, children, subtreeWidth, childX, y + yGap, yGap);
                childX += subtreeWidth[c];
            }
        }

        // 6) POST-PROCESS: OVERLAP REMOVAL ==================================

        private void ApplyOverlapRemoval(List<SimpleVisualiserWrapper> wrappers)
        {
            if (wrappers.Count < 2) return;

            const double margin = 6;
            const int maxIter = 40;
            // Damping factor < 1 to prevent oscillation when many nodes are packed
            // on the same ring; each pair is only partially separated per iteration,
            // letting the global configuration settle.
            const double damping = 0.5;

            // Convergence threshold: stop as soon as the average per-node movement
            // drops below ~0.25 px, so we do not burn iterations on oscillation.
            double convergenceThreshold = 0.25 * wrappers.Count;

            int iterationsUsed = 0;
            double totalMovement = 0;

            for (int iter = 0; iter < maxIter; iter++)
            {
                iterationsUsed = iter + 1;
                totalMovement = 0;

                for (int i = 0; i < wrappers.Count; i++)
                    for (int j = i + 1; j < wrappers.Count; j++)
                    {
                        SimpleVisualiserWrapper a = wrappers[i];
                        SimpleVisualiserWrapper b = wrappers[j];

                        Point ac = GetWrapperCenter(a);
                        Point bc = GetWrapperCenter(b);
                        double dx = bc.X - ac.X;
                        double dy = bc.Y - ac.Y;
                        double overlapX = (a.ActualWidth + b.ActualWidth) / 2 + margin - Math.Abs(dx);
                        double overlapY = (a.ActualHeight + b.ActualHeight) / 2 + margin - Math.Abs(dy);

                        if (overlapX > 0 && overlapY > 0)
                        {
                            double push;
                            if (overlapX < overlapY)
                            {
                                push = overlapX / 2 * damping;
                                if (dx >= 0) { ac.X -= push; bc.X += push; }
                                else         { ac.X += push; bc.X -= push; }
                            }
                            else
                            {
                                push = overlapY / 2 * damping;
                                if (dy >= 0) { ac.Y -= push; bc.Y += push; }
                                else         { ac.Y += push; bc.Y -= push; }
                            }
                            SetWrapperCenter(a, ac.X, ac.Y);
                            SetWrapperCenter(b, bc.X, bc.Y);
                            totalMovement += push * 2;
                        }
                    }

                if (totalMovement < convergenceThreshold) break;
            }

            MinusZero.Instance.Log(1, "GraphVisualiser.ApplyOverlapRemoval",
                "iterations=" + iterationsUsed + " lastTotalMovement=" + totalMovement.ToString("F2") +
                " wrappers=" + wrappers.Count);
        }

        // LINE REDRAW ========================================================

        private void UpdateAllLines()
        {
            HashSet<Shape> seen = new HashSet<Shape>();
            foreach (SimpleVisualiserWrapper w in GetDistinctWrappers())
                foreach (Shape s in w.Lines)
                {
                    if (!seen.Add(s)) continue;
                    LineTagStore lts = s.Tag as LineTagStore;
                    if (lts == null) continue;
                    UpdateLineEndpoints(s as ArrowLine, lts);
                }
        }

        private void UpdateLineEndpoints(ArrowLine l, LineTagStore lts)
        {
            if (l == null || lts == null) return;

            SimpleVisualiserWrapper FromWrapper = lts.FromWrapper;
            SimpleVisualiserWrapper ToWrapper = lts.ToWrapper;
            if (FromWrapper == null || ToWrapper == null) return;

            // Same geometry as AddLine: line starts at From center and ends at the
            // intersection with the To-rectangle's border.
            l.X1 = Canvas.GetLeft(FromWrapper) + FromWrapper.ActualWidth / 2;
            l.Y1 = Canvas.GetTop(FromWrapper)  + FromWrapper.ActualHeight / 2;

            double tX = Canvas.GetLeft(ToWrapper) + ToWrapper.ActualWidth / 2;
            double tY = Canvas.GetTop(ToWrapper)  + ToWrapper.ActualHeight / 2;

            double testX = l.X1 - tX;
            double testY = l.Y1 - tY;

            if (testX == 0) testX = 0.001;
            if (testY == 0) testY = 0.001;

            if (testY <= 0 && Math.Abs(testX * ToWrapper.ActualHeight) <= Math.Abs(testY * ToWrapper.ActualWidth))
            {
                l.X2 = tX - (ToWrapper.ActualHeight / 2 * testX / testY);
                l.Y2 = tY - ToWrapper.ActualHeight / 2;
            }

            if (testY > 0 && Math.Abs(testX * ToWrapper.ActualHeight) <= Math.Abs(testY * ToWrapper.ActualWidth))
            {
                l.X2 = tX + (ToWrapper.ActualHeight / 2 * testX / testY);
                l.Y2 = tY + ToWrapper.ActualHeight / 2;
            }

            if (testX >= 0 && Math.Abs(testX * ToWrapper.ActualHeight) >= Math.Abs(testY * ToWrapper.ActualWidth))
            {
                l.X2 = tX + ToWrapper.ActualWidth / 2;
                l.Y2 = tY + (ToWrapper.ActualWidth / 2 * testY / testX);
            }

            if (testX <= 0 && Math.Abs(testX * ToWrapper.ActualHeight) >= Math.Abs(testY * ToWrapper.ActualWidth))
            {
                l.X2 = tX - ToWrapper.ActualWidth / 2;
                l.Y2 = tY - (ToWrapper.ActualWidth / 2 * testY / testX);
            }

            if (lts.MetaLabel != null)
            {
                Canvas.SetLeft(lts.MetaLabel, l.X1 + ((l.X2 - l.X1) / 2));
                Canvas.SetTop(lts.MetaLabel,  l.Y1 + ((l.Y2 - l.Y1) / 2));
            }
        }

        private void NormalizePositions(List<SimpleVisualiserWrapper> wrappers, Dictionary<SimpleVisualiserWrapper, Point> positions)
        {
            if (positions.Count == 0) return;

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            double maxHalfW = 0, maxHalfH = 0;
            foreach (SimpleVisualiserWrapper w in wrappers)
            {
                Point p = positions[w];
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
                if (w.ActualWidth  / 2 > maxHalfW) maxHalfW = w.ActualWidth  / 2;
                if (w.ActualHeight / 2 > maxHalfH) maxHalfH = w.ActualHeight / 2;
            }

            double margin = 60;
            double canvasW = GetCanvasWidth();
            double canvasH = GetCanvasHeight();

            double spanX = Math.Max(1, maxX - minX);
            double spanY = Math.Max(1, maxY - minY);
            double availableX = canvasW - 2 * (margin + maxHalfW);
            double availableY = canvasH - 2 * (margin + maxHalfH);

            double scaleX = availableX / spanX;
            double scaleY = availableY / spanY;
            double scale = Math.Min(scaleX, scaleY);
            if (double.IsInfinity(scale) || double.IsNaN(scale) || scale <= 0) scale = 1;
            if (scale > 1) scale = 1; // only shrink, do not stretch small graphs

            List<SimpleVisualiserWrapper> keys = new List<SimpleVisualiserWrapper>(positions.Keys);
            foreach (SimpleVisualiserWrapper w in keys)
            {
                Point p = positions[w];
                double nx = margin + maxHalfW + (p.X - minX) * scale;
                double ny = margin + maxHalfH + (p.Y - minY) * scale;
                positions[w] = new Point(nx, ny);
            }
        }
    }
}