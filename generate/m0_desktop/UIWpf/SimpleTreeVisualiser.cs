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

namespace m0.UIWpf
{
    class SimpleTreeVisualiserViewItem : TreeViewItem
    {
        /*public static int cnt=0;

        public  SimpleTreeVisualiserViewItem():base(){
            cnt++;
            
        }*/

        private bool _IsSelected;
        
        public bool IsSelected { 
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
                    l.Background = SystemColors.HighlightBrush; //new SolidColorBrush(Colors.Black);
                    l.Foreground = new SolidColorBrush(Colors.White);

                    if(l2!=null){
                        l2.Background = SystemColors.HighlightBrush; //new SolidColorBrush(Colors.Black);
                        l2.Foreground = new SolidColorBrush(Colors.White);
                    }
                }else{                    
                    if(l2==null){
                        l.Background = new SolidColorBrush(Colors.White);
                        l.Foreground = new SolidColorBrush(Colors.Black);
                    }else{
                       l.Background = new SolidColorBrush(Colors.White);
                       l.Foreground = new SolidColorBrush(Colors.Gray);
                       l2.Background = new SolidColorBrush(Colors.White);
                       l2.Foreground = new SolidColorBrush(Colors.Black);
                   }

               }

               TreeParent.UpdateSelectedVertexes();
             }                        
        }

        public SimpleTreeVisualiser TreeParent {get; set;}

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs a)
        {
            bool IsCtrl = false;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                IsCtrl = true;

            bool WasSelected = IsSelected;            

            if (!IsCtrl)            
                TreeParent.ClearAllSelectedItems();

            if (WasSelected)
                IsSelected = false;
            else
                IsSelected = true;

            a.Handled = true;
        }

        protected override void OnExpanded(RoutedEventArgs ea)
        {
            foreach (object o in Items)
            {
                if (o is SimpleTreeVisualiserViewItem)
                {
                    SimpleTreeVisualiserViewItem v = (SimpleTreeVisualiserViewItem)o;

                    if(v.Items.Count==0)
                        foreach (IEdge e in (IEnumerable<IEdge>)v.Tag)
                            v.Items.Add(TreeParent.GetTreeViewItem(e, false));
                }
            }
        }
    }

    public class SimpleTreeVisualiser: TreeView, IPlatformClass
    {
        public static bool HideMetaNameIfEmpty = true;


        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVertexesUpdate = false;

        public void SelectedVertexesUpdated()
        {
            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVertexesUpdate = true;

            IVertex sv = Vertex.Get("SelectedVertexes:");

            foreach (SimpleTreeVisualiserViewItem i in Items)
                SelectedVertexesUpdated_Reccurent(i,sv);

            TurnOffSelectedVertexesUpdate = false;
        }

        private void SelectedVertexesUpdated_Reccurent(SimpleTreeVisualiserViewItem i,IVertex sv)
        {
            if (GraphUtil.FindEdgeByToVertex(sv, (IVertex)i.Tag)!=null)
                i.IsSelected = true;
            else
                i.IsSelected = false;

            foreach (SimpleTreeVisualiserViewItem ii in i.Items)
                SelectedVertexesUpdated_Reccurent(ii, sv);
        }

        public void UpdateSelectedVertexes()
        {
            if (TurnOffSelectedVertexesUpdate)
                return;

            TurnOffSelectedItemsUpdate = true;

            IVertex sv = Vertex.Get("SelectedVertexes:");

            GraphUtil.RemoveAllEdges(sv);

            foreach (SimpleTreeVisualiserViewItem i in Items)
                UpdateSelectedVertexes_Reccurent(i, sv);

            TurnOffSelectedItemsUpdate = false;
        }

        private void UpdateSelectedVertexes_Reccurent(SimpleTreeVisualiserViewItem i, IVertex sv)
        {
            if (i.IsSelected)
                sv.AddEdge(null, (IVertex)i.Tag);

            foreach (SimpleTreeVisualiserViewItem ii in i.Items)
                UpdateSelectedVertexes_Reccurent(ii, sv);
        }

        public void ClearAllSelectedItems()
        {
            bool bef = TurnOffSelectedVertexesUpdate;

            TurnOffSelectedVertexesUpdate = true;

            foreach (SimpleTreeVisualiserViewItem i in Items)
                ClearAllSelectedItems_Reccurent(i);

            TurnOffSelectedVertexesUpdate = bef;
        }

        private void ClearAllSelectedItems_Reccurent(SimpleTreeVisualiserViewItem i)
        {
            i.IsSelected = false;

            foreach (SimpleTreeVisualiserViewItem ii in i.Items)
                ClearAllSelectedItems_Reccurent(ii);
        }

        public TreeViewItem GetTreeViewItem(IEdge e, bool generateDeeperLevel){
            SimpleTreeVisualiserViewItem i = new SimpleTreeVisualiserViewItem();

            i.TreeParent = this;

            i.Tag = e.To;

            StackPanel s = new StackPanel();

            if (GeneralUtil.CompareStrings(e.Meta.Value,"$Empty") && HideMetaNameIfEmpty)
            {                
                Label ll = new Label();
             
                ll.Content = e.To.Value;// +":" + SimpleTreeVisualiserViewItem.cnt;
                ll.Padding = new Thickness(0);

                ll.FontWeight = FontWeight.FromOpenTypeWeight(999);

                s.Children.Add(ll);
            }
            else
            {                
                s.Orientation = Orientation.Horizontal;

                Label l1 = new Label();
                l1.Content = e.Meta.Value + " : ";
                l1.Padding = new Thickness(0);

                l1.Foreground = new SolidColorBrush(Colors.Gray);

                Label l2 = new Label();
                l2.Content = e.To.Value;// +":" + SimpleTreeVisualiserViewItem.cnt;
                l2.Padding = new Thickness(0);

                l2.FontWeight = FontWeight.FromOpenTypeWeight(999);

                s.Children.Add(l1);
                s.Children.Add(l2);                                
            }

            i.Header = s;

            if(generateDeeperLevel)
                foreach (IEdge ee in e.To)
                    i.Items.Add(GetTreeViewItem(ee,false));

            return i;
        }

        private void UpdateBaseVertex()
        {
            Items.Clear();

            foreach (IEdge e in Vertex.Get("BaseVertex:"))
                Items.Add(GetTreeViewItem(e,true));
        }
                

        public SimpleTreeVisualiser()
        {
            MinusZero mz = MinusZero.Instance;

            this.BorderThickness = new Thickness(0);

            VirtualizingStackPanel.SetIsVirtualizing(this, true);
            VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "SimpleTreeVisualiser" + this.GetHashCode());

                ClassVertex.AddClassAttributes(Vertex, mz.Root.Get(@"System\Meta\Visualiser\SimpleListVisualiser"));

                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange));
            }
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseVertex")))
            {
                UpdateBaseVertex();                
            }

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedVertexes")))
                SelectedVertexesUpdated();

            if ((sender == Vertex.Get("SelectedVertexes:")) && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
                SelectedVertexesUpdated();
        }


        public IVertex Vertex {get; set;}
        
    }
}
