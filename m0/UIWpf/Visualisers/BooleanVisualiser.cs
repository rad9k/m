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

namespace m0.UIWpf.Visualisers
{
    public class BooleanVisualiser : CheckBox, IVisualiser
    {
        public GenericVisualiserHelper VisualiserHelper { get; set; }

        bool IsNull { get; set; }

        public BooleanVisualiser()
        {
            new GenericVisualiserHelper(this, "BooleanVisualiser", this);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        protected override void OnToggle()
        {
            base.OnToggle();

            if (IsNull)
            {
                IVertex r = MinusZero.Instance.Root;

                IVertex from = Vertex.Get(false, @"BaseEdge:\From:");
                IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");
                IVertex toMeta = r.Get(false, @"System\Meta\ZeroTypes\Edge\To");

                if (from != null && meta != null)
                {
                    //GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", GraphUtil.SetVertexValue(from, meta, "True")); // NOT
                    //GraphUtil.SetVertexValue(Vertex.Get(false, "BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, "True")); // NOT!!!!

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
        }

        public void UpdateBaseEdge(){
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if(Vertex.Get(false, @"BaseEdge:\Meta:IsDrum") != null)
            {
                int x = 9;
            }

            if (bv != null && bv.Value != null)
            {
                if (GeneralUtil.CompareStrings(bv.Value, "True"))
                    this.IsChecked = true;
                else
                    this.IsChecked = false;

                IsNull = false;
            }
            else
                IsNull = true;
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
