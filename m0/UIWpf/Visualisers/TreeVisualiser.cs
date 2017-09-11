using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using System.Collections.ObjectModel;
using System.Windows;
using m0.Util;
using m0.UML;
using m0.ZeroTypes;
using System.Windows.Input;
using System.Windows.Media;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using System.Collections;
using m0.UIWpf.Commands;

namespace m0.UIWpf.Visualisers
{
    public class TreeVisualiserViewItem : TreeViewItem, IDisposable
    {        
        public static bool HideMetaNameIfEmpty = true;

        public bool IsFilled;        

        private void Select(bool IsCtrl)
        {
            IsSelected = true;

            TreeParent.UpdateSelectedVertexes(IsCtrl, this);
        }

        private void Unselect(bool IsCtrl)
        {
            IsSelected = false;

            TreeParent.UpdateSelectedVertexes(IsCtrl, this);
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

        public TreeVisualiser TreeParent {get; set;}

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                BaseCommands.Open(Edge.CreateTempEdge((IEdge)Tag), null);            
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs a)
        {
            a.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs a)
        {
            bool IsCtrl = false;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                IsCtrl = true;

            bool WasSelected = IsSelected;            

            if (!IsCtrl)            
                TreeParent.ClearAllSelectedItems();

            if (WasSelected)
                Unselect(IsCtrl);
            else
                Select(IsCtrl);

            a.Handled = true;

            //base.OnMouseLeftButtonDown(a);
        }        

        protected override void OnExpanded(RoutedEventArgs ea)
        {         
            if(IsFilled==false)            
            {
                TreeVisualiser.ClearAllItems_Reccurent(this);                

                foreach (IEdge ee in ((IEdge)Tag).To)
                    Items.Add(TreeParent.GetTreeViewItem(ee, true));                
            }

            IsFilled = true;
            
            //base.OnExpanded(ea);
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
                ll.FontWeight = UIWpf.ValueWeight;

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
                l1.FontWeight = UIWpf.MetaWeight;

                Label l2 = new Label();
                
                l2.Content = e.To.Value;

                l2.Padding = new Thickness(0);

                l2.Foreground = (Brush)FindResource("0ForegroundBrush");
                l2.FontWeight = UIWpf.ValueWeight;

                s.Children.Add(l1);
                s.Children.Add(l2);
            }

            Header = s;
        }        

        public void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (sender != ((IEdge)this.Tag).To)
                throw new Exception("TreeVisualiserViewItem attached to some other Vertex Change");

            if (e.Type == VertexChangeType.ValueChanged)
                UpdateHeader();

            if (e.Type == VertexChangeType.EdgeAdded)
                EdgeAdded(e);

            if (e.Type == VertexChangeType.EdgeRemoved)
                EdgeRemoved(e);
        }

        private void EdgeRemoved(VertexChangeEventArgs e)
        {
            if (IsFilled){
                IList l=GeneralUtil.CreateAndCopyList(Items);
                foreach (TreeVisualiserViewItem i in l)
                    if (((IEdge)i.Tag) == e.Edge)
                        Items.Remove(i);
                }
        }

        private void EdgeAdded(VertexChangeEventArgs e)
        {
            if (!IsFilled)
            {            
                TreeVisualiser.ClearAllItems_Reccurent(this);
            }

            Items.Add(TreeParent.GetTreeViewItem(e.Edge, true));
        }


        private bool IsDisposed;

        public void Dispose()
        {
            if (!IsDisposed)
            {
                ((IEdge)Tag).To.Change -= VertexChange;
                IsDisposed = true;
            }
        }
    }

    public class TreeVisualiser: TreeView, IPlatformClass, IDisposable, IHasLocalizableEdges, IHasSelectableEdges
    {              
        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVertexesUpdate = false;

        public void SelectedVertexesUpdated()
        {
            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVertexesUpdate = true;

            IVertex sv = Vertex.Get("SelectedEdges:");

            foreach (TreeViewItem i in Items)
                SelectedVertexesUpdated_Reccurent(i,sv);

            TurnOffSelectedVertexesUpdate = false;
        }

        private void SelectedVertexesUpdated_Reccurent(TreeViewItem i,IVertex sv)
        {
            if (i is TreeVisualiserViewItem)
            {
                TreeVisualiserViewItem ii = (TreeVisualiserViewItem)i;

                
                if (Edge.FindEdgeByEdgeOnlyToVertex(sv, (IEdge)ii.Tag)!=null)                
                    ii.IsSelected = true;
                else
                    ii.IsSelected = false;
            }

            foreach (TreeViewItem ii in i.Items)
                SelectedVertexesUpdated_Reccurent(ii, sv);
        }

        public void UpdateSelectedVertexes(bool IsCtrl, TreeVisualiserViewItem item)
        {
            if (TurnOffSelectedVertexesUpdate)
                return;

            TurnOffSelectedItemsUpdate = true;

            IVertex sv = Vertex.Get("SelectedEdges:");

            IEdge e=(IEdge)item.Tag;


            if (!IsCtrl)
                GraphUtil.RemoveAllEdges(sv);

            if (item.IsSelected)
                Edge.AddEdge(sv, e);
            else
                Edge.DeleteVertexByEdgeOnlyToVertex(sv, e);                

            // LEGACY
            //
            // currently there is no support for same vertex in two places in tree begin selected / unselected
            // this is due to performance
            //
            /*IVertex sv = Vertex.Get("SelectedVertexes:");

            GraphUtil.RemoveAllEdges(sv);

            foreach (TreeViewItem i in Items)
                UpdateSelectedVertexes_Reccurent(i, sv);
             */

            TurnOffSelectedItemsUpdate = false;
        }

        public void UnselectAllSelectedEdges()
        {
            TurnOffSelectedEdgesFireChange();

            GraphUtil.RemoveAllEdges(Vertex.Get("SelectedEdges:"));

            TurnOnSelectedEdgesFireChange();         

            ClearAllSelectedItems();
        }

        private void TurnOnSelectedEdgesFireChange()
        {
            if (Vertex.Get("SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get("SelectedEdges:")).CanFireChangeEvent = true;
        }

        private void TurnOffSelectedEdgesFireChange()
        {
            if (Vertex.Get("SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get("SelectedEdges:")).CanFireChangeEvent = false;
        }

        public void ClearAllSelectedItems()
        {
            bool bef = TurnOffSelectedVertexesUpdate;

            TurnOffSelectedVertexesUpdate = true;

            foreach (TreeViewItem i in Items)
                ClearAllSelectedItems_Reccurent(i);

            TurnOffSelectedVertexesUpdate = bef;
        }

        private void ClearAllSelectedItems_Reccurent(TreeViewItem i)
        {
            if(i is TreeVisualiserViewItem)
                ((TreeVisualiserViewItem)i).IsSelected = false;

            foreach (TreeViewItem ii in i.Items)
                ClearAllSelectedItems_Reccurent(ii);
        }

        public TreeViewItem GetTreeViewItem(IEdge e, bool generateDeeperLevel){
            TreeVisualiserViewItem i = new TreeVisualiserViewItem();

            i.TreeParent = this;

            i.Tag = e;

            i.UpdateHeader();

            

            TurnOffSelectedVertexesUpdate = true;

            IVertex sv = Vertex.Get("SelectedEdges:");

            if (Edge.FindEdgeByEdge(sv, e)!=null)
                i.IsSelected = true;

            TurnOffSelectedVertexesUpdate = false;

            if(generateDeeperLevel)
                if (e.To.Count() > 0)
                {
                    TreeViewItem tvi = new TreeViewItem();
                    i.Items.Add(tvi);
                }                

            e.To.Change += i.VertexChange;

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


        private void UpdateBaseEdge()
        {
            ClearAllItems();

            IVertex bas = Vertex.Get(@"BaseEdge:\To:");            

            if (bas != null)            
                foreach (IEdge e in bas)
                    Items.Add(GetTreeViewItem(e, true));            
        }

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get("ZoomVisualiserContent:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        protected void SetVertexDefaultValues()
        {         
            Vertex.Get("ZoomVisualiserContent:").Value = 100;
        }

        public void SelectAllInBaseEdge()
        {
            TurnOffSelectedItemsUpdate=true;

            IVertex selectedEdges = Vertex.Get("SelectedEdges:");

            if (selectedEdges is VertexBase)
                ((VertexBase)selectedEdges).CanFireChangeEvent = false;                        

            foreach (IEdge ee in Vertex.Get(@"BaseEdge:\To:"))
                Edge.AddEdge(selectedEdges, ee);

            if (selectedEdges is VertexBase)
                ((VertexBase)selectedEdges).CanFireChangeEvent = true;            

            TurnOffSelectedItemsUpdate = false;

            foreach (TreeViewItem i in Items)            
                if (i is TreeVisualiserViewItem)
                {
                    TreeVisualiserViewItem ii = (TreeVisualiserViewItem)i;
                    ii.IsSelected = true;
                }
            
        }

        public TreeVisualiser()
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
                //Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "TreeVisualiser" + this.GetHashCode());

                Vertex = mz.CreateTempVertex();
                Vertex.Value = "TreeVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Tree"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

                SetVertexDefaultValues();          

                this.ContextMenu = new m0ContextMenu(this);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;
                this.Drop+=dndDrop;

                this.MouseEnter += dndMouseEnter;
            }
        }
        
        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                ||((sender==Vertex.Get("BaseEdge:"))&&(e.Type==VertexChangeType.EdgeAdded)&&((GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))))
            {
                UpdateBaseEdge();                
            }

            if (sender == Vertex.Get(@"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded))
                EdgeAdded(e);

            if (sender == Vertex.Get(@"BaseEdge:\To:") &&  (e.Type == VertexChangeType.EdgeRemoved))
                EdgeRemoved(e);

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
                SelectedVertexesUpdated();

            if ((sender == Vertex.Get("SelectedEdges:")) && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
                SelectedVertexesUpdated();

            if(sender is IVertex && GraphUtil.FindEdgeByToVertex(Vertex.GetAll(@"SelectedEdges:\"),(IVertex)sender)!=null)
                SelectedVertexesUpdated();

            if (sender == Vertex.Get("ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
                ChangeZoomVisualiserContent();                       
        }

        private void EdgeRemoved(VertexChangeEventArgs e)
        {            
                IList l = GeneralUtil.CreateAndCopyList(Items);
                foreach (TreeVisualiserViewItem i in l)
                    if (((IEdge)i.Tag) == e.Edge)
                        Items.Remove(i);            
        }

        private void EdgeAdded(VertexChangeEventArgs e)
        {         
                Items.Add(GetTreeViewItem(e.Edge, true));
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

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }


        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByLocation(Point p)
        {
            vertexByLocationToReturn = null;

            GetVertexByLocation_Reccurent(this.Items, p);

            // DO NOT WANT THIS FEATURE            
            if (vertexByLocationToReturn == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                vertexByLocationToReturn = Vertex.Get(@"BaseEdge:");

            return vertexByLocationToReturn;
        }

        protected IVertex GetVertexByLocation_Reccurent(ItemCollection items,Point p){
            foreach (TreeViewItem i in items)
            {
                if (VisualTreeHelper.HitTest(i, TranslatePoint(p, i)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    Edge.AddEdgeEdges(v, (IEdge)i.Tag);
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

        ///// DRAG AND DROP

        Point dndStartPoint;
        bool hasButtonBeenDown;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);
            hasButtonBeenDown = true;

            MinusZero.Instance.IsGUIDragging = false;
        }

        bool isDraggin = false;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown&&
                !UIWpf.IsMouseOverScrollbar(sender,dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                isDraggin = true;

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (Vertex.Get(@"SelectedEdges:\") != null)
                    foreach (IEdge ee in Vertex.GetAll(@"SelectedEdges:\"))
                        dndVertex.AddEdge(null, ee.To);
                else
                {
                    IVertex v=GetEdgeByLocation(dndStartPoint);
                    if(v!=null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);
                }

                isDraggin = false;
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = GetEdgeByLocation(e.GetPosition(this));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))            
                v = Vertex.Get("BaseEdge:");

            if(v!=null)
                Dnd.DoDrop(null, v.Get("To:"), e);

            e.Handled = true;
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
