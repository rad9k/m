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
    public class StringViewVisualiser : TextBlock, IVisualiser
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public StringViewVisualiser()
        {            
            new AtomVisualiserHelper(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\StringView"), this, "StringViewVisualiser", this);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ZoomVisualiserContentChange() { }

        public void UpdateBaseEdge()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null /*&& ((String)bv.Value)!="$Empty"*/)
            {
                this.Text = bv.Value.ToString();
                return;
            }
            else
            {
                this.Text = "Ø";
                return;
            }      
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
