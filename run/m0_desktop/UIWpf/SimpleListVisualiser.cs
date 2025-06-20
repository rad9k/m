using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using m0.Foundation;
using System.Windows.Data;
using m0.Graph;
using m0.UML;
using m0.ZeroTypes;
using m0.Util;
using System.Windows.Media;
using System.Windows;

namespace m0.UIWpf
{
    public class SimpleListVisualiser:DataGrid, IPlatformClass
    {                
        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVertexesUpdate = false;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e){
            if (!TurnOffSelectedVertexesUpdate)
            {
                TurnOffSelectedItemsUpdate = true;

                IVertex sv = Vertex.Get("SelectedVertexes:");
    
                GraphUtil.RemoveAllEdges(sv);

                foreach (IEdge v in SelectedItems)
                    sv.AddEdge(null, v.To);

                TurnOffSelectedItemsUpdate = false;
            }

            base.OnSelectionChanged(e);
        }

        private void AddColumns(){
            DataGridTextColumn metaColumn = new DataGridTextColumn();
            metaColumn.Header = "meta";
            Binding mb = new Binding("Meta.Value");
            mb.Mode = BindingMode.OneWay;
            metaColumn.Binding = mb;
            metaColumn.CellStyle = (Style)FindResource("0ListMetaColum");
            metaColumn.Foreground = new SolidColorBrush((Color)FindResource("0Gray"));
            
            Columns.Add(metaColumn);

            DataGridTextColumn valueColumn = new DataGridTextColumn();
            valueColumn.Header = "value";
            valueColumn.Binding = new Binding("To.Value");
            Columns.Add(valueColumn); 
        }

        protected void SelectedVertexesUpdated(){
            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVertexesUpdate = true;

            this.SelectedItems.Clear();

            IVertex b=Vertex.Get("BaseVertex:");

            foreach(IEdge e in Vertex.Get("SelectedVertexes:")){
                IEdge ee = GraphUtil.FindEdgeByToVertex(b, e.To);
                if (ee != null)
                    this.SelectedItems.Add(ee);
            }

            TurnOffSelectedVertexesUpdate = false;
        }

        public SimpleListVisualiser()
        {
            this.AutoGenerateColumns = false;
            this.BorderThickness = new System.Windows.Thickness(0);
            this.HeadersVisibility = DataGridHeadersVisibility.Column;
            this.GridLinesVisibility = DataGridGridLinesVisibility.None;

            AddColumns();
            this.SelectedValuePath = "To";

            MinusZero mz=MinusZero.Instance;

            if (mz != null&&mz.IsInitialized)
            {
                Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "SimpleListVisualiser" + this.GetHashCode());

                ClassVertex.AddClassAttributes(Vertex, mz.Root.Get(@"System\Meta\Visualiser\SimpleListVisualiser"));

                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange));
            }
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if((sender==Vertex)&&(e.Type==VertexChangeType.EdgeAdded)&&(GeneralUtil.CompareStrings(e.Edge.Meta.Value,"BaseVertex"))){
                 ItemsSource = e.Edge.To.ToList(); // if there is no .ToList DataGrid can not edit
            }

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedVertexes")))
                SelectedVertexesUpdated();

            if ((sender == Vertex.Get("SelectedVertexes:")) && ((e.Type == VertexChangeType.EdgeAdded)||(e.Type == VertexChangeType.EdgeRemoved)))
                SelectedVertexesUpdated();
        }       

        private IVertex _Vertex;

        public IVertex Vertex { 
            get { return _Vertex;}
            set{
                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange));            
            }
        }
        
    }
}
