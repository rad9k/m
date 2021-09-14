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
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using System.Windows;
using m0.UIWpf.Commands;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class BooleanVisualiser : CheckBox, IVisualiser
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public List<IDisposable> SubVisualisers { get; set; }

        bool IsNull { get; set; }

        public BooleanVisualiser(IVertex baseEdgeVertex)
        {
            new AtomVisualiserHelper(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Boolean"),
                this, 
                "BooleanVisualiser", 
                this,
                baseEdgeVertex);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ZoomVisualiserContentChange() { }

        protected bool CanProceedUIUpdateEvent = true;

        protected override void OnToggle()
        {
            if (!CanProceedUIUpdateEvent)
                return;

            base.OnToggle();

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (IsNull)
            {
                IVertex r = MinusZero.Instance.Root;

                IVertex from = Vertex.Get(false, @"BaseEdge:\From:");
                IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");
                IVertex toMeta = r.Get(false, @"System\Meta\ZeroTypes\Edge\To");

                if (from != null && meta != null)
                {
                    GraphUtil.CreateOrReplaceEdge(Vertex.Get(false, "BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, "True"));

                    IsNull = false;
                }
            }

            if (Vertex.Get(false, @"BaseEdge:\To:") != null)
            {
                if (this.IsChecked == true)
                    Vertex.Get(false, @"BaseEdge:\To:").Value = "True";
                else
                    Vertex.Get(false, @"BaseEdge:\To:").Value = "False";
            }

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////
        }

        public void UpdateBaseEdge(){
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null)
            {
                CanProceedUIUpdateEvent = false;

                if (GeneralUtil.CompareStrings(bv.Value, "True"))
                    this.IsChecked = true;
                else
                    this.IsChecked = false;

                CanProceedUIUpdateEvent = true;

                IsNull = false;
            }
            else
                IsNull = true;
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
            return Vertex.Get(false, "BaseEdge:");
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
