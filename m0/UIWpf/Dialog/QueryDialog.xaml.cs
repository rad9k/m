using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0.Util;

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for ZeroCodeEditorDialog.xaml
    /// </summary>
    public partial class QueryDialog : UserControl
    {
        IVertex baseVertex;

        public override string ToString()
        {
            return "Query";
        }

        public QueryDialog(IVertex baseVertex)
        {
            InitializeComponent();

            MinusZero z = MinusZero.Instance;

            this.baseVertex = baseVertex;

            GraphUtil.ReplaceEdge(this.Queries.Vertex.Get("BaseEdge:"), "To", z.Root.Get(@"User\CurrentUser:\Queries:"));

            PlatformClass.RegisterVertexChangeListeners(Queries.Vertex, QueriesVertexChange, new string[] { "BaseEdge", "SelectedEdges" });

            this.Loaded += new RoutedEventHandler(OnLoad);

        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            Content.Focus();
        }
       
        
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            MinusZero z = MinusZero.Instance;

            GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get("BaseEdge:"),"To",z.Empty);            
                       
            IVertex res = baseVertex.GetAll(Content.Text);

            if (res != null)
            {
                this.Resoult.UnselectAllSelectedEdges();

                GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get("BaseEdge:"), "To", res);                
            }
            
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            this.Resoult.SelectAllInBaseEdge();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            MinusZero z = MinusZero.Instance;

            if (Content.Text != "")
            {
                z.Root.Get(@"User\CurrentUser:\Queries").AddVertex(null, Content.Text);
            }
        }

        private void QueriesVertexChange(object sender, VertexChangeEventArgs e){
            if(sender==Queries.Vertex.Get(@"SelectedEdges:\")&&e.Type==VertexChangeType.EdgeAdded&&GeneralUtil.CompareStrings(e.Edge.Meta.Value,"To"))
                Content.Text=Queries.Vertex.Get(@"SelectedEdges:\\To:").Value.ToString();
        }
    }
}
