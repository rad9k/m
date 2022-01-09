using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using System.Collections.ObjectModel;
using System.Windows;
using m0.Util;
using m0.ZeroUML;
using m0.ZeroTypes;
using System.Windows.Input;
using System.Windows.Media;
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
        
        public  bool IsSelected { 
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                
                StackPanel s = (StackPanel)Header;

                int cc = s.Children.Count;
                
                Label l = (Label)s.Children[0];
                Label l2=null;

                if(cc>1)
                    l2=(Label)s.Children[1];

                if (_IsSelected)
                {
                    l.Background = (Brush)FindResource("0SelectionBrush");
                    l.Foreground = (Brush)FindResource("0BackgroundBrush");

                    if(l2!=null){
                        l2.Background = (Brush)FindResource("0SelectionBrush");
                        l2.Foreground = (Brush)FindResource("0BackgroundBrush");
                    }
                }else{                    
                    if(l2==null){
                        l.Background = (Brush)FindResource("0BackgroundBrush");
                        l.Foreground = (Brush)FindResource("0ForegroundBrush");
                    }else{
                        l.Background = (Brush)FindResource("0BackgroundBrush");
                        l.Foreground = (Brush)FindResource("0GrayBrush");
                       l2.Background = (Brush)FindResource("0BackgroundBrush");
                       l2.Foreground = (Brush)FindResource("0ForegroundBrush");
                   }
               }               
             }                        
        }

        public TreeVisualiser ParentVisualiser {get; set;}

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                BaseCommands.Open(Edge.CreateTempEdgeVertex(GetEdge()), null);            
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs a)
        {
            a.Handled = true;
        }

        IEdge GetEdge()
        {
            return (IEdge)Tag;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs a)
        {
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

        void Fill()
        {
            TreeVisualiser.ClearAllItems_Reccurent(this);

            IEnumerable<IEdge> filteredList = VisualiserUtil.FilterEdges(((IEdge)Tag).To, ParentVisualiser.Vertex);

            foreach (IEdge ee in filteredList)
                Items.Add(ParentVisualiser.CreateTreeViewItem(ee, true, this));
        }

        protected override void OnExpanded(RoutedEventArgs ea)
        {
            if (IsFilled == false)
                Fill();

            IsFilled = true;                        
        }

        public void UpdateHeader(){
            StackPanel s = new StackPanel();

            IEdge e = (IEdge)Tag;

            if ((GeneralUtil.CompareStrings(e.Meta.Value, "$Empty") && HideMetaNameIfEmpty)||e.Meta.Value==null)
            {
                Label ll = new Label();

                if (e.To.Value == null || GeneralUtil.CompareStrings(e.To.Value, ""))
                    ll.Content = "[$Empty]";
                else
                    ll.Content = e.To.Value;

                ll.Padding = new Thickness(0);

                ll.Foreground = (Brush)FindResource("0ForegroundBrush");
                ll.FontWeight = WpfUtil.ValueWeight;

                s.Children.Add(ll);
            }
            else
            {
                s.Orientation = Orientation.Horizontal;

                Label l1 = new Label();
                l1.Content = e.Meta.Value + " : ";
                l1.Padding = new Thickness(0);

                l1.Foreground = (Brush)FindResource("0GrayBrush");
                l1.FontStyle = FontStyles.Italic;
                l1.FontWeight = WpfUtil.MetaWeight;

                Label l2 = new Label();
                
                l2.Content = e.To.Value;

                l2.Padding = new Thickness(0);

                l2.Foreground = (Brush)FindResource("0ForegroundBrush");
                l2.FontWeight = WpfUtil.ValueWeight;

                s.Children.Add(l1);
                s.Children.Add(l2);
            }

            Header = s;
        }        

        public INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (ParentVisualiser.VisualiserHelper.IsDisposed)
                return exe.Stack;

            if (GetEdge().To.DisposedState != DisposeStateEnum.Live)
                return exe.Stack;

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

            return exe.Stack;
        }

        private void EdgeRemoved(IEdge edge)
        {
            if (IsFilled)
            {
                IList l = GeneralUtil.CreateAndCopyList(Items);
                foreach (TreeVisualiserViewItem i in l)
                    if (Edge.CompareIEdges(((IEdge)i.Tag), edge))
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

    public class TreeVisualiser: TreeView, IListVisualiser, IHasSelectableEdges
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        


        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

        public TreeVisualiser() : this(null, null) { }


        static string[] _MetaTriggeringBaseEdgeUpdate = new string[] {  };
        public string[] MetaTriggeringUpdateBaseEdge { get { return _MetaTriggeringBaseEdgeUpdate; } }

        static string[] _MetaTriggeringUpdateViewSettings = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateViewSettings; } }

        public void UpdateView() { }

        public TreeVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
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

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void UpdateBaseEdge()
        {
            ClearAllItems();

            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (bas != null)
            {
                IEnumerable<IEdge> filteredList = VisualiserUtil.FilterEdges(bas, Vertex);

                foreach (IEdge e in filteredList)
                    Items.Add(CreateTreeViewItem(e, true, null));
            }
        }

        public void ZoomVisualiserContentChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "ZoomVisualiserContent:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        protected INoInEdgeInOutVertexVertex CustomVertexChange(IExecution exe)
        {
            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "ZoomVisualiserContent"))
            {
                ZoomVisualiserContentChange();
                return exe.Stack;
            }

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "ZoomVisualiserContent")
                || IsEdgeAddedRemovedDiscardedFrom(exe.Stack, Vertex.Get(false, @"SelectedEdges:")))
            {
                SelectedVerticesUpdated();
            }

            IVertex baseEdgeTo = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

            IVertex edgeVertex = exe.Stack.Get(false, @"event:\Edge:");

            if (IsEdgeAddedTo(exe.Stack, baseEdgeTo))
            {
                EdgeAdded(Edge.CreateIEdgeFromEdgeVertex(edgeVertex));
                return exe.Stack;
            }

            if (IsEdgeRemovedFrom(exe.Stack, baseEdgeTo))
            {
                EdgeRemoved(Edge.CreateIEdgeFromEdgeVertex(edgeVertex));
                return exe.Stack;
            }

            if (IsEdgeDisposedFrom(exe.Stack, baseEdgeTo))
            {
                UpdateBaseEdge();
                return exe.Stack;
            }
           
            //UpdateBaseEdge();

            return exe.Stack;
        }

        public void SelectedVerticesUpdated()
        {
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

                
                if (Edge.FindEdgeVertexByToVertex(sv, ((IEdge)ii.Tag).To)!=null)                
                    ii.IsSelected = true;
                else
                    ii.IsSelected = false;
            }

            foreach (TreeViewItem ii in i.Items)
                SelectedVerticesUpdated_Reccurent(ii, sv);
        }

        public void UpdateSelectedVertices(bool IsCtrl, TreeVisualiserViewItem item)
        {
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
                Edge.AddEdgeVertex(sv, e);
            else
                Edge.DeleteVertexByEdgeOnlyToVertex(sv, e);

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

            if (e.Meta != null && GeneralUtil.CompareStrings(e.Meta.Value, "$GraphChangeTrigger"))
                i.doNotTrackGraphChanges = true;

            i.ParentVisualiser = this;

            i.Tag = e;

            i.UpdateHeader();

            

            TurnOffSelectedVerticesUpdate = true;

            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            if (Edge.FindIEdgeVertexByIEdge(sv, e)!=null)
                i.IsSelected = true;

            TurnOffSelectedVerticesUpdate = false;

            if(generateDeeperLevel)
                if (e.To.Count() > 0)
                {
                    TreeViewItem tvi = new TreeViewItem();
                    i.Items.Add(tvi);
                }

            if (!i.doNotTrackGraphChanges)
                i.vertexChangeListenerEdge = GraphChangeTrigger.AddTriggerAndListener(e.To,
                    new List<string> { },
                    new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                     GraphChangeFilterEnum.OutputEdgeAdded,
                     GraphChangeFilterEnum.OutputEdgeRemoved,
                     GraphChangeFilterEnum.OutputEdgeDisposed},
                     "TreeViewItem",
                     i.VertexChange);
                //i.vertexChangeListenerEdge = ExecutionFlowHelper.AddListener_DotNetDelegate(e.To, i.VertexChange);

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
            Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;
        }

        public void SelectAllInBaseEdge()
        {
            TurnOffSelectedItemsUpdate=true;

            IVertex selectedEdges = Vertex.Get(false, @"SelectedEdges:");

            //if (selectedEdges is VertexBase)
              //  ((VertexBase)selectedEdges).CanFireChangeEvent = false;                        

            foreach (IEdge ee in Vertex.Get(false, @"BaseEdge:\To:"))
                Edge.AddEdgeVertex(selectedEdges, ee);

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

        private void EdgeRemoved(IEdge edge)
        {            
                IList l = GeneralUtil.CreateAndCopyList(Items);
                foreach (TreeVisualiserViewItem i in l)
                if (Edge.CompareIEdges(((IEdge)i.Tag), edge))                    
                        Items.Remove(i);            
        }

        private void EdgeAdded(IEdge edge)
        {         
                Items.Add(CreateTreeViewItem(edge, true, null));
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        public void Dispose()
        {
            VisualiserHelper.Dispose();
        }

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByLocation(Point p)
        {
            vertexByLocationToReturn = null;

            GetVertexByLocation_Reccurent(this.Items, p);

            // DO NOT WANT THIS FEATURE            
            if (vertexByLocationToReturn == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                vertexByLocationToReturn = Vertex.Get(false, @"BaseEdge:");

            return vertexByLocationToReturn;
        }

        protected IVertex GetVertexByLocation_Reccurent(ItemCollection items,Point p){
            foreach (TreeViewItem i in items)
            {
                if (VisualTreeHelper.HitTest(i, TranslatePoint(p, i)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    Edge.AddEdgeVertexEdges(v, (IEdge)i.Tag);
                    vertexByLocationToReturn = v;
                }
                    
                GetVertexByLocation_Reccurent(i.Items, p);                
            }

            return null;
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
