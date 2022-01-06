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
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        List<IVertex> manuallyAddedVertexChangeListeners = new List<IVertex>();

        public ClassVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
           this.Padding = new Thickness(2);

           new AtomVisualiserHelper(
               parentVisualiser,
               MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class"), 
               this, 
               "ClassVisualiser", 
               this, 
               false, 
               new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\"}, 
               "ListVisualiser",
               baseEdgeVertex,
               UpdateBaseEdgeCallSchemeEnum.OmmitSecond);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ZoomVisualiserContentChange() { }

        public void UpdateBaseEdge()
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

                    string cardinalites = ClassVertex.GetCardinalitiesString(e.To);

                    if(cardinalites!="")
                        sb.Append(" "+cardinalites);
                }

                this.Text = sb.ToString();
            }
            else
                this.Text = "Ø";
        }
        
        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
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
