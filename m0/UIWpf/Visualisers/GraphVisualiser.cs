using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
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

            if(baseVertex!=null)
                PlatformClass.RegisterVertexChangeListeners(baseVertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });
        }

        public void Dispose()
        {
            if (this.Child is IDisposable)
                ((IDisposable)(this.Child)).Dispose();

            if (baseVertex != null)
                PlatformClass.RemoveVertexChangeListeners(baseVertex, new VertexChange(VertexChange));
        }
        
        public void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if(e.Type!=VertexChangeType.ValueChanged&&ParentVisualiser.IsPaiting==false) //ValueChanged is handled by FastMode
                ParentVisualiser.PaintGraph();
        }
    }

    public class GraphVisualiser: Canvas, IPlatformClass, IDisposable, IHasLocalizableEdges, IHasSelectableEdges
    {
        SimpleVisualiserWrapper Highlighted;

        public bool IsPaiting=false;

        protected SimpleVisualiserWrapper Add(double x, double y, FrameworkElement _e, IVertex baseVertex)
        {
            SimpleVisualiserWrapper e = new SimpleVisualiserWrapper(_e, baseVertex, this);
            Children.Add(e);

            e.UpdateLayout();

            Panel.SetZIndex(e, 1);
            Canvas.SetLeft(e, x-e.ActualWidth/2);
            Canvas.SetTop(e, y-e.ActualHeight/2);
            
            if (!DisplayedVertexesUIElements.ContainsKey(baseVertex))
                DisplayedVertexesUIElements.Add(baseVertex, e);
            else
                DisplayedVertexesUIElements[baseVertex] = e;
            
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
                StringViewVisualiser s = new StringViewVisualiser();

                GraphUtil.ReplaceEdge(s.Vertex.Get("BaseEdge:"), "To", v);

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
        bool ShowInEdges;

        bool IsFirstPainted = false;

        public void PaintGraph()
        {            
            if (ActualHeight != 0)
            {
                //MinusZero.Instance.Log(1, "PaintGraph", "");

                // turn off Vertex.Change listener

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                //                                

                IsPaiting = true;

                if (GeneralUtil.CompareStrings(Vertex.Get("FastMode:"), "True"))
                    FastMode = true;
                else
                    FastMode = false;

                if (GeneralUtil.CompareStrings(Vertex.Get("MetaLabels:"), "True"))
                    MetaLabels = true;
                else
                    MetaLabels = false;

                if (GeneralUtil.CompareStrings(Vertex.Get("ShowInEdges:"), "True"))
                    ShowInEdges = true;
                else
                    ShowInEdges = false;

                this.Children.Clear();

                foreach (UIElement e in DisplayedVertexesUIElements.Values)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();
                    
                DisplayedVertexesUIElements.Clear();

                //GraphUtil.RemoveAllEdges(Vertex.Get("DisplayedEdges:"));
                
                Width = GraphUtil.GetIntegerValue(Vertex.Get("NumberOfCircles:"))*GraphUtil.GetIntegerValue(Vertex.Get("VisualiserCircleSize:"))*2;
                Height = Width;                
                             
                AddCircle(0,null);

                SelectWrappersForSelectedVertexes();

                IsFirstPainted = true;

                IsPaiting = false;

                // turn on Vertex.Change listener

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                //
            }
        }

        Dictionary<IVertex, SimpleVisualiserWrapper> DisplayedVertexesUIElements;

        protected void AddCircle(int level, IList<IVertex> InnerCircleVertexes)
        {
            //MinusZero.Instance.Log(1,"AddCircle", level.ToString());

            IList<IVertex> CircleVertexes=new List<IVertex>();

            if (level == 0)
            {
                IVertex b=Vertex.Get(@"BaseEdge:\To:");

                double x = this.Width / 2;
                double y = this.Height / 2;

                Add(x, y, (FrameworkElement)GetVisualiser(b),b).UpdateLayout();                

                CircleVertexes.Add(b);

                AddCircle(1, CircleVertexes);

                return;
            }

            int OutAndInEdgesCount = 0;

            SimpleVisualiserWrapper dummyPointIn = new SimpleVisualiserWrapper(null,null,this);
            SimpleVisualiserWrapper dummyPointOut = new SimpleVisualiserWrapper(null, null, this);

            foreach (IVertex v in InnerCircleVertexes)
            {
                foreach (IEdge e in v)
                    if (!DisplayedVertexesUIElements.ContainsKey(e.To))
                    {
                        DisplayedVertexesUIElements.Add(e.To, dummyPointOut);
                        OutAndInEdgesCount++;
                    }

                if(ShowInEdges)
                foreach (IEdge e in v.InEdges)
                    if (!DisplayedVertexesUIElements.ContainsKey(e.From))
                    {
                        DisplayedVertexesUIElements.Add(e.From, dummyPointIn);
                        OutAndInEdgesCount++;
                    }
            }

            double cnt = 0;

            int CircleSize=GraphUtil.GetIntegerValue(Vertex.Get("VisualiserCircleSize:"));

            //IVertex DisplayedEdges = Vertex.Get("DisplayedEdges:");

            if (OutAndInEdgesCount > 0)
                foreach (IVertex v in InnerCircleVertexes)
                {
                    SimpleVisualiserWrapper vPoint = DisplayedVertexesUIElements[v];

                    foreach (IEdge e in v)
                        if (!DisplayedVertexesUIElements.ContainsKey(e.To) || DisplayedVertexesUIElements[e.To] == dummyPointOut)
                        {
                            double x = (this.Width / 2) + Math.Cos(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;
                            double y = (this.Height / 2) + Math.Sin(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;

                            SimpleVisualiserWrapper toWrapper =  Add(x, y, (FrameworkElement)GetVisualiser(e.To), e.To);                            

                            CircleVertexes.Add(e.To);                            
                        
                            AddLine(vPoint, toWrapper, e.Meta);

                            cnt++;
                        }
                        else
                        {
                            SimpleVisualiserWrapper eToPoint = DisplayedVertexesUIElements[e.To];

                            AddLine(vPoint, eToPoint,e.Meta);                            
                        }

                    if(ShowInEdges)
                    foreach (IEdge e in v.InEdges)
                        if (!DisplayedVertexesUIElements.ContainsKey(e.From) || DisplayedVertexesUIElements[e.From] == dummyPointIn)
                        {
                            double x = (this.Width / 2) + Math.Cos(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;
                            double y = (this.Height / 2) + Math.Sin(cnt / OutAndInEdgesCount * Math.PI * 2) * CircleSize * level;                            

                            SimpleVisualiserWrapper fromWrapper = Add(x, y, (FrameworkElement)GetVisualiser(e.From), e.From);

                            CircleVertexes.Add(e.From);
                            
                            cnt++;
                        }                        
                }

            if (level < GraphUtil.GetIntegerValue(Vertex.Get("NumberOfCircles:")))
                AddCircle(level + 1, CircleVertexes);
            else // lines from last circle
            {
                foreach (IVertex v in CircleVertexes)
                {
                    SimpleVisualiserWrapper vPoint = DisplayedVertexesUIElements[v];

                    foreach(IEdge e in v)
                        if(DisplayedVertexesUIElements.ContainsKey(e.To)) // if vertex is allready displayed, connect it
                        {
                            SimpleVisualiserWrapper eToPoint = DisplayedVertexesUIElements[e.To];

                            AddLine(vPoint, eToPoint,e.Meta);                            
                        }

                }
            }
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {            
            PaintGraph();
            
            if(IsFirstPainted)
                this.Loaded -= OnLoad;
        }

        protected void SetVertexDefaultValues()
        {
            Vertex.Get("ZoomVisualiserContent:").Value = 100;
            Vertex.Get("VisualiserCircleSize:").Value = 200;
            Vertex.Get("NumberOfCircles:").Value = 2;
            Vertex.Get("FastMode:").Value = "True";
            Vertex.Get("MetaLabels:").Value = "True";
        }
   
        public GraphVisualiser()
        {
            MinusZero mz = MinusZero.Instance;            

            DisplayedVertexesUIElements = new Dictionary<IVertex, SimpleVisualiserWrapper>();

            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.AllowDrop = true;

            if (mz != null && mz.IsInitialized)
            {
                //Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "GraphVisualiser" + this.GetHashCode()); 

                Vertex = mz.CreateTempVertex();
                Vertex.Value = "GraphVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Graph"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

                SetVertexDefaultValues();
                
                this.ContextMenu = new m0.UIWpf.Controls.m0ContextMenu(this);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;                
                this.Drop += dndDrop;
                this.MouseEnter += dndMouseEnter;
            }            

            this.Loaded+=new RoutedEventHandler(OnLoad);
        }

        private void UpdateBaseEdge(){
            IVertex bv = Vertex.Get(@"BaseEdge:\To:");

            if (bv != null)
            {
                PaintGraph();
            }
        }

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get("ZoomVisualiserContent:"))) / 100;

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
            KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp = DisplayedVertexesUIElements.Where(x => ((SimpleVisualiserWrapper)x.Value).Child == e.Source).FirstOrDefault();

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
                RestoreSelectedVertexes();
                
                KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp = DisplayedVertexesUIElements.Where(x => ((SimpleVisualiserWrapper)x.Value).Child == e.Source).FirstOrDefault();

                if (kvp.Key != null)                
                    GraphUtil.ReplaceEdge(Vertex.Get("BaseEdge:"), "To", kvp.Key);                                    
            }

            if (e.ClickCount == 1) // change Selection
            {
                   KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp = DisplayedVertexesUIElements.Where(x => ((SimpleVisualiserWrapper)x.Value).Child == e.Source).FirstOrDefault();

                   if (kvp.Key != null)
                   {
                       CopySelectedVertexesToTemp();                           

                       bool IsCtrl = false;

                       if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                           IsCtrl = true;

                       IVertex sv = Vertex.Get("SelectedEdges:");
                       

                       if (IsCtrl)
                       {
                           if (kvp.Value.IsSelected)
                           {
                               // THIS GOES TO 
                               kvp.Value.Unselect();                               

                               Edge.DeleteVertexByEdgeTo(sv, kvp.Key); 
                           }
                           else
                           {
                               kvp.Value.Select();

                               Edge.AddEdgeByToVertex(sv, kvp.Key);
                           }
                       }
                       else
                       {
                           UnselectAllSelected();                           

                           GraphUtil.RemoveAllEdges(sv);

                           kvp.Value.Select();

                           Edge.AddEdgeByToVertex(sv, kvp.Key);                           
                       }
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
            IVertex sv = Vertex.Get("SelectedEdges:");

            foreach (IEdge v in sv)
                if (v.To.Get("To:") != null && DisplayedVertexesUIElements.ContainsKey(v.To.Get("To:")))
                    DisplayedVertexesUIElements[v.To.Get("To:")].Unselect();            
        }        

        IVertex tempSelectedVertexes;

        protected void CopySelectedVertexesToTemp()
        {
            tempSelectedVertexes = MinusZero.Instance.CreateTempVertex();

            GraphUtil.CopyEdges(Vertex.Get("SelectedEdges:"), tempSelectedVertexes);
        }

        protected void RestoreSelectedVertexes()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            if (tempSelectedVertexes != null)
            {
                GraphUtil.RemoveAllEdges(sv);

                GraphUtil.CopyEdges(tempSelectedVertexes, sv);
            }

        }

        protected void UnselectAll()
        {
            foreach(KeyValuePair<IVertex,SimpleVisualiserWrapper> key in DisplayedVertexesUIElements)
                key.Value.Unselect();
        }

        public void UnselectAllSelectedEdges()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            GraphUtil.RemoveAllEdges(sv);
        }
        
        protected void SelectedVertexesUpdated()
        {
            if (IsFirstPainted)
            {
                UnselectAll();

                SelectWrappersForSelectedVertexes();
            }
        }

        protected void SelectWrappersForSelectedVertexes()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            foreach (IEdge e in sv)
            {
                if (e.To.Get("To:")!=null&&DisplayedVertexesUIElements.ContainsKey(e.To.Get("To:")))
                    DisplayedVertexesUIElements[e.To.Get("To:")].Select();
            }
        }
        
        public void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.ValueChanged)
               || ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && ((GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))))
                { UpdateBaseEdge(); return; }

            if (sender == Vertex.Get(@"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                { UpdateBaseEdge(); return; }

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
                { SelectedVertexesUpdated(); return; }

            if ((sender == Vertex.Get("SelectedEdges:")) && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
                { SelectedVertexesUpdated(); return; }

            if (sender is IVertex && GraphUtil.FindEdgeByToVertex(Vertex.GetAll(@"SelectedEdges:\"), (IVertex)sender) != null)
                { SelectedVertexesUpdated(); return; }

            if (sender == Vertex.Get("ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
                { ChangeZoomVisualiserContent(); return; }

            if (sender == Vertex.Get("VisualiserCircleSize:") && e.Type == VertexChangeType.ValueChanged)
                { PaintGraph(); return; }

            if (sender == Vertex.Get("NumberOfCircles:") && e.Type == VertexChangeType.ValueChanged)
                { PaintGraph(); return; }

            if (sender == Vertex.Get("FastMode:") && e.Type == VertexChangeType.ValueChanged)
                { PaintGraph(); return; }

            if (sender == Vertex.Get("MetaLabels:") && e.Type == VertexChangeType.ValueChanged)
                { PaintGraph(); return; }

            if (sender == Vertex.Get("ShowInEdges:") && e.Type == VertexChangeType.ValueChanged)
                { PaintGraph(); return; }     
        }        

        private IVertex _Vertex;

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

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                MinusZero mz = MinusZero.Instance;

                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                foreach (UIElement e in DisplayedVertexesUIElements.Values)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }


        // IHasLocalizableEdges

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByLocation(Point p)
        {
            vertexByLocationToReturn = null;

            foreach (KeyValuePair<IVertex, SimpleVisualiserWrapper> kvp in DisplayedVertexesUIElements)
            {
                if (VisualTreeHelper.HitTest(kvp.Value, TranslatePoint(p, kvp.Value)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    Edge.AddEdgeEdgesOnlyTo(v, kvp.Value.baseVertex);
                    vertexByLocationToReturn = v;
                }
            }

            // DO WANT THIS FEATURE
            if (vertexByLocationToReturn == null&&GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                vertexByLocationToReturn = Vertex.Get(@"BaseEdge:");

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

            CopySelectedVertexesToTemp();

            MinusZero.Instance.IsGUIDragging = false;
        }        

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown &&
                !UIWpf.IsMouseOverScrollbar(sender, dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) & (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                RestoreSelectedVertexes();

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (Vertex.Get(@"SelectedEdges:\") != null)
                    foreach (IEdge ee in Vertex.GetAll(@"SelectedEdges:\"))
                        dndVertex.AddEdge(null, ee.To);
                else
                {
                    IVertex v = GetEdgeByLocation(dndStartPoint);
                    if (v != null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = GetEdgeByLocation(e.GetPosition(this));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value,"OnlyEnd"))
                v = Vertex.Get("BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get("To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }


    }
}
