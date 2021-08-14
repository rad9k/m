using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows.Media;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using System.Windows;
using m0.UIWpf.Visualisers.Helper;

namespace m0.UIWpf.Visualisers
{
    class ClassVisualiser : TextBlock, IVisualiser
    {
        public GenericVisualiserHelper VisualiserHelper { get; set; }

        List<IVertex> manuallyAddedVertexChangeListeners = new List<IVertex>();

        public ClassVisualiser()
        {
            new GenericVisualiserHelper(this, "TestVisualiser", this, false, new List<string> { @"BaseEdge:\To:", @"BaseEdge:\To:\", @"SelectedEdges:" }, "ListVisualiser");
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        private void UpdateBaseEdge()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null /*&& ((String)bv.Value)!="$Empty"*/){
                StringBuilder sb=new StringBuilder();

                bool isFirst=true;

                foreach(IEdge e in bv.GetAll(false, @"Attribute:")){
                    if(isFirst==false)
                        sb.Append("\n");
                    else
                        isFirst=false;

                    sb.Append(e.To.Value);

                    if (e.To.Get(false, "$EdgeTarget:") != null)
                        sb.Append(" : " + e.To.Get(false, @"$EdgeTarget:"));

                    string cardinalites = ClassVertex.GetStringCardinalities(e.To);

                    if(cardinalites!="")
                        sb.Append(" "+cardinalites);
                }

                this.Text = sb.ToString();
            }
            else
                this.Text = "Ø";
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();                        

            if ((sender == Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged))            
                UpdateBaseEdge();

            if(sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeAdded){
                e.Edge.To.Change += new VertexChange(VertexChange);

                manuallyAddedVertexChangeListeners.Add(e.Edge.To);

                UpdateBaseEdge();
            }

            if (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeRemoved)
            {
                e.Edge.To.Change -= new VertexChange(VertexChange);

                manuallyAddedVertexChangeListeners.Remove(e.Edge.To);

                UpdateBaseEdge();
            }

            foreach (IEdge ee in Vertex.GetAll(false, @"BaseEdge:\To:\"))
                if (sender == ee.To) // all events
                    UpdateBaseEdge();
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper._Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        public void Dispose()
        {
            VisualiserHelper.Dispose();
        }

        public IVertex GetEdgeByLocation(System.Windows.Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(System.Windows.FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public System.Windows.FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
    }
}
