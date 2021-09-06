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
    public class DebugVisualiser : StackPanel, IVisualiser
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public List<IDisposable> SubVisualisers { get; set; }

        public DebugVisualiser(IVertex baseEdgeVertex)
        {            
            new AtomVisualiserHelper(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Debug"), 
                this, 
                "DebugVisualiser", 
                this, 
                false, 
                new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" }, 
                "ListVisualiser",
                baseEdgeVertex);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ZoomVisualiserContentChange() { }

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

        private void AddVertexLine(string s, IVertex v)
        {
            StackPanel sp = new StackPanel();

            TextBlock tb = new TextBlock();

            tb.FontFamily = new FontFamily("Consolas");

            tb.Text = " " + s;

            Button b = new Button();

            b.Tag = v;

            b.Width = 10;
            b.Height = 10;

            b.Content = "go";

            b.Click += GoEvent;

            sp.Orientation = Orientation.Horizontal;

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

            GraphUtil.ReplaceEdge(Vertex.Get(false, @"BaseEdge:"), Edge.MetaMeta, MinusZero.Instance.Empty);
            GraphUtil.ReplaceEdge(Vertex.Get(false, @"BaseEdge:"), Edge.ToMeta, v);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public void UpdateBaseEdge()
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
                    AddVertexLine(GetEdgeString(e.Meta, e.From), e.From);
                

                sb.Clear();

                sb.AppendLine();

                sb.AppendLine("OUTPUT EDGES RAW [" + bv.OutEdgesRaw.Count + "]:");

                //sb.AppendLine();

                AddLine(sb);

                foreach (IEdge e in bv.OutEdgesRaw)
                    AddVertexLine(GetEdgeString(e.Meta, e.To), e.To);                       

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
