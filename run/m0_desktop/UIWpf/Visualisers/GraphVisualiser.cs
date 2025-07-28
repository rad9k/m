using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using System.Windows;
using m0.Graph;
using m0.Util;
using System.Windows.Media;
using System.Windows.Shapes;
using m0.UIWpf.Controls.Fast;
using System.Windows.Input;
using System.Diagnostics;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class LineTagStore
    {
        public SimpleVisualiserWrapper ToWrapper;
        public SimpleVisualiserWrapper FromWrapper;
        public TextBlock MetaLabel;
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

            GeneralUtil.SetPropertyIfPresent(this.Child, "Foreground", (Brush)FindResource("0BackgroundBrush"));
        }

        public void Unselect()
        {
            IsSelected = false;

            this.Background = (Brush)FindResource("0BackgroundBrush");

            if(IsHighlighted)
                GeneralUtil.SetPropertyIfPresent(this.Child, "Foreground", (Brush)FindResource("0HighlightBrush"));
            else
                GeneralUtil.SetPropertyIfPresent(this.Child, "Foreground", (Brush)FindResource("0ForegroundBrush"));
        }


        public bool IsHighlighted;

        public void HighlightThisAndDescendants()
        {
            IsHighlighted = true;

            HighlightThis();

            foreach(Shape e in Lines){
                e.Stroke = (Brush)FindResource("0LightHighlightBrush");
                e.Fill = (Brush)FindResource("0LightHighlightBrush");

                Panel.SetZIndex(e, 99998);

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

            GeneralUtil.SetPropertyIfPresent(this.Child, "Foreground", (Brush)FindResource("0HighlightBrush"));
        }

        public void UnhighlightThisAndDescendants()
        {            
            IsHighlighted = false;

            UnhighlightThis();

            foreach(Shape e in Lines){
                e.Stroke = (Brush)FindResource("0LightGrayBrush");
                e.Fill = (Brush)FindResource("0LightGrayBrush");

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
                GeneralUtil.SetPropertyIfPresent(this.Child, "Foreground", (Brush)FindResource("0BackgroundBrush"));
            else
                GeneralUtil.SetPropertyIfPresent(this.Child, "Foreground", (Brush)FindResource("0ForegroundBrush"));
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

    public class GraphVisualiser: Canvas, IListVisualiser, IHasSelectableEdges, ITypedEdge
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        SimpleVisualiserWrapper Highlighted;

        public bool IsPaiting=false;


        static string[] _MetaTriggeringUpdateVertex = new string[] { "VisualiserCircleSize", "NumberOfCircles", "ShowOutEdges", "ShowInEdges", "FastMode", "MetaLabels" };
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

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
              new List<string> {""/*, @"BaseEdge:\To:"*/ }, 
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
                        
            l.EndEnding = LineEndEnum.FilledTriangle;
            l.Fill = (Brush)FindResource("0LightGrayBrush");            

            Panel.SetZIndex(l, 0);            

            Children.Add(l);            

            if (MetaLabels&&meta.Value!=null&&!GeneralUtil.CompareStrings(meta.Value,"$Empty"))
            {                
                TextBlock b = new TextBlock();
                b.Text = meta.Value.ToString();

                Canvas.SetLeft(b, l.X1 + ((l.X2 - l.X1) / 2));
                Canvas.SetTop(b, l.Y1 + ((l.Y2 - l.Y1) / 2));

                b.Foreground = (Brush)FindResource("0LightGrayBrush");

                Children.Add(b);

                lineTag.MetaLabel = b;
            }
        }

        protected FrameworkElement GetVisualiser(IVertex v)
        {
            if (!FastMode)
            {
                IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, v);

                StringViewVisualiser s = new StringViewVisualiser(baseEdgeVertex, null, false);

                //GraphUtil.ReplaceEdge(s.Vertex.Get(false, "BaseEdge:"), "To", v);

                s.ContextMenu = null; // no contextmenu, as there is gloal one for whole GraphVisualiser

                return s;
            }
            else
            {
                TextBlock b = new TextBlock();

                if (v.Value != null)
                    b.Text = v.Value.ToString();
                else
                    b.Text = "Ø";

                return b;
            }       
        }

        bool FastMode;
        bool MetaLabels;
        bool ShowOutEdges;
        bool ShowInEdges;

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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp = DisplayedVerticesUIElements.Where(x => ((SimpleVisualiserWrapper)x.Value).Child == e.Source).FirstOrDefault();

            if (kvp.Value != null&&((SimpleVisualiserWrapper)kvp.Value).IsHighlighted==false)
            {
                SimpleVisualiserWrapper wrapper=(SimpleVisualiserWrapper)kvp.Value;

                if (Highlighted != null)
                    Highlighted.UnhighlightThisAndDescendants();

                wrapper.HighlightThisAndDescendants();
                
                Highlighted = wrapper;
            }

            base.OnMouseEnter(e);
        }
        
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // switch to another BaseVertex
            {
                RestoreSelectedVertices();
                
                KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp = DisplayedVerticesUIElements.Where(x => ((SimpleVisualiserWrapper)x.Value).Child == e.Source).FirstOrDefault();

                if (kvp.Key != null)                
                    GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", kvp.Key);                                    
            }

            if (e.ClickCount == 1) // change Selection
            {
                   KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp = DisplayedVerticesUIElements.Where(x => ((SimpleVisualiserWrapper)x.Value).Child == e.Source).FirstOrDefault();

                   if (kvp.Key != null)
                   {
                       ////////////////////////////////////////
                       Interaction.BeginInteractionWithGraph();
                       //////////////////////////////////////// 
                    
                       CopySelectedVerticesToTemp();                           

                       bool IsCtrl = false;

                       if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                           IsCtrl = true;

                       IVertex sv = Vertex.Get(false, "SelectedEdges:");
                       

                       if (IsCtrl)
                       {
                           if (kvp.Value.IsSelected)
                           {
                               // THIS GOES TO 
                               kvp.Value.Unselect();                               

                               EdgeHelper.DeleteVertexByEdgeTo(sv, kvp.Key); 
                           }
                           else
                           {
                               kvp.Value.Select();

                               EdgeHelper.AddEdgeVertexByToVertex(sv, kvp.Key);
                           }
                       }
                       else
                       {
                           UnselectAllSelected();                           

                           GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

                           kvp.Value.Select();

                           EdgeHelper.AddEdgeVertexByToVertex(sv, kvp.Key);                           
                       }

                       ////////////////////////////////////////
                       Interaction.EndInteractionWithGraph();
                       //////////////////////////////////////// 
                }
            }
            
            e.Handled = true;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected void UnselectAllSelected()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge v in sv)
                if (v.To.Get(false, "To:") != null && DisplayedVerticesUIElements.ContainsKey(v.To.Get(false, "To:")))
                    DisplayedVerticesUIElements[v.To.Get(false, "To:")].Unselect();            
        }        

        IVertex tempSelectedVertices;

        protected void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();

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
            if (vertexByLocationToReturn == null&&GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
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

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get(false, "BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get(false, "To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}