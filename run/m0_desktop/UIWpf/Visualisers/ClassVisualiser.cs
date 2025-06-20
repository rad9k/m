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
using m0.Lib;

namespace m0.UIWpf.Visualisers
{
    class ClassVisualiser : Border, IVisualiser, ITypedEdge
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        List<IVertex> manuallyAddedVertexChangeListeners = new List<IVertex>();

        // TypedEdge START
        public ClassVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        StackPanel stackPanel;

        public ClassVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            this.Padding = new Thickness(2);

            stackPanel = new StackPanel();

            stackPanel.Orientation = Orientation.Vertical;

            this.Child = stackPanel;

            new AtomVisualiserHelper(
               parentVisualiser,
               isVolatile,
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

        public void ScaleChange() { }

        public void BaseEdgeToUpdated()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            stackPanel.Children.Clear();

            if (bv != null && bv.Value != null /*&& ((String)bv.Value)!="$Empty"*/) {                                                
                foreach (IEdge e in GraphUtil.GetQueryOut(bv, "Attribute", null)) {                     
                    StackPanel s = new StackPanel();
                    s.Orientation = Orientation.Horizontal;
                    stackPanel.Children.Add(s);

                    string str = e.To.Value.ToString();
                    TextBlock tb = new TextBlock();
                    tb.FontWeight = FontWeights.Bold;

                    IVertex eToEdgeTarget = GraphUtil.GetQueryOutFirst(e.To, "$EdgeTarget", null);

                    if (eToEdgeTarget != null)
                    {
                        str += " : ";
                        tb.Text = str;

                        s.Children.Add(tb);

                        tb = new TextBlock();
                        tb.FontStyle = FontStyles.Italic;
                        tb.Text = GraphUtil.GetStringValue(eToEdgeTarget);

                        s.Children.Add(tb);
                    }
                    else
                    {
                        tb.Text = str;

                        s.Children.Add(tb);
                    }

                    string cardinalites = ClassVertex.GetCardinalitiesString(e.To);

                    if (cardinalites != "") 
                    {
                        tb = new TextBlock();

                        tb.Text = " " + cardinalites;

                        s.Children.Add(tb);
                    }

                    string valueRanges = ClassVertex.GetValueRangeString(e.To);

                    if (valueRanges != "")
                    {
                        tb = new TextBlock();
                        tb.FontStyle = FontStyles.Italic;

                        tb.Text = valueRanges;

                        s.Children.Add(tb);
                    }

                }                
            }
            else
            {
                TextBlock tb = new TextBlock();

                tb.Text = "Ø";

                stackPanel.Children.Add(tb);
            }                
        }
        
        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }

        public IVertex GetEdgeByPoint(System.Windows.Point point)
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
