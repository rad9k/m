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
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class DebugVisualiser : StackPanel, IVisualiser, ITypedEdge
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        // TypedEdge START

        public DebugVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public DebugVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {            
            new AtomVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Debug"), 
                this, 
                "DebugVisualiser", 
                this, 
                false, 
                new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" }, 
                "ListVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        string GetEdgeString(IVertex meta, IVertex to)
        {
            string ret = "";

            if (meta != null && meta.Value != null)
                ret += meta.Value.ToString();

            ret += " :: ";

            if (to != null && to.Value != null)
                ret += to.Value.ToString();

            ret += " [";

            if (meta != null)
                ret += meta.Store.Identifier + " " + meta.Identifier;
                    
            ret += " :: ";

            ret += to.Store.Identifier + " " + to.Identifier + "]";

            return ret;
        }

        private void AddLine(StringBuilder sb)
        {
            TextBlock tb = new TextBlock();
                
            tb.FontFamily = new FontFamily("Consolas");

            tb.Text = sb.ToString();

            this.Children.Add(tb);
        }

        private void AddVertexVertexLine(string s, IVertex v1, IVertex v2)
        {
            StackPanel sp = new StackPanel();

            sp.Orientation = Orientation.Horizontal;

            TextBlock tb = new TextBlock();

            tb.FontFamily = new FontFamily("Consolas");

            tb.Text = " " + s;

            //meta

            Button b_m = new Button();

            b_m.Padding = new Thickness(0);

            b_m.FontSize = 6;

            b_m.Tag = v1;

            b_m.Width = 25;
            b_m.Height = 10;

            b_m.Content = "go meta";

            b_m.Click += GoEvent;

            sp.Children.Add(b_m);

            //to

            Button b = new Button();

            b.Padding = new Thickness(0);

            b.FontSize = 6;

            b.Tag = v2;

            b.Width = 20;
            b.Height = 10;

            b.Content = "go to";

            b.Click += GoEvent;

            sp.Children.Add(b);


            sp.Children.Add(tb);

            this.Children.Add(sp);
        }        

        private void GoEvent(object sender, RoutedEventArgs e)
        {
            IVertex v = (IVertex)((Button)e.Source).Tag;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            GraphUtil.ReplaceEdge(Vertex.Get(false, @"BaseEdge:"), EdgeHelper.MetaMeta, MinusZero.Instance.Empty);
            GraphUtil.ReplaceEdge(Vertex.Get(false, @"BaseEdge:"), EdgeHelper.ToMeta, v);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public void BaseEdgeToUpdated()
        {
            IVertex mv = Vertex.Get(false, @"BaseEdge:\Meta:");
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null)
            {
                this.Children.Clear();

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(GetEdgeString(mv, bv));

                sb.AppendLine();

                sb.AppendLine("INPUT EDGES RAW [" + bv.InEdgesRaw.Count + "]:");

                //sb.AppendLine();

                AddLine(sb);

                foreach (IEdge e in bv.InEdgesRaw)
                    AddVertexVertexLine(GetEdgeString(e.Meta, e.From), e.Meta, e.From);
                

                sb.Clear();

                sb.AppendLine();

                sb.AppendLine("OUTPUT EDGES RAW [" + bv.OutEdgesRaw.Count + "]:");

                //sb.AppendLine();

                AddLine(sb);

                foreach (IEdge e in bv.OutEdgesRaw)
                    AddVertexVertexLine(GetEdgeString(e.Meta, e.To), e.Meta, e.To);                       

                return;
            }
            else
            {
                this.Children.Clear();

                return;
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
