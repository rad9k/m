using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.User.Process.UX;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for QueryDialog.xaml
    /// </summary>
    public partial class QueryDialog : UserControl
    {
        IVertex baseVertex;
        CodeControl codeControl;
        IVertex queryEditorVertex;

        public override string ToString()
        {
            return "Query";
        }

        public QueryDialog(IVertex baseVertex)
        {
            InitializeComponent();

            MinusZero z = MinusZero.Instance;

            this.baseVertex = baseVertex;

            queryEditorVertex = z.CreateTempVertex();
            codeControl = new CodeControl(queryEditorVertex, true);
            codeControl.GenerateAfterParse = false;
            ContentHost.Child = codeControl;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////     

            GraphUtil.ReplaceEdge(this.Queries.Vertex.Get(false, "BaseEdge:"), "To", z.Root.Get(false, @"Home:\CurrentUser:\QueriesRoot:"));

            GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get(false, "BaseEdge:"), "To", z.Empty);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
            
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////     

            GraphUtil.ReplaceEdge(this.Queries.Vertex.Get(false, "BaseEdge:"), "To", z.Root.Get(false, @"Home:\CurrentUser:\QueriesRoot:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            this.Queries.SelectedEdgesChange += Queries_SelectedEdgesChange;

            this.Loaded += new RoutedEventHandler(OnLoad);

        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            codeControl.editor.Focus();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            MinusZero z = MinusZero.Instance;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////            

            IVertex res = baseVertex.GetAll(false, codeControl.editor.Text);

            if (res == null)
                GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get(false, "BaseEdge:"), "To", z.Empty);
            else
            {
                GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get(false, "BaseEdge:"), "To", res);

                this.Resoult.UnselectAllSelectedEdges();
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            BottomTabs.SelectedItem = ResoultTab;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            this.Resoult.SelectAllInBaseEdge();

            BottomTabs.SelectedItem = ResoultTab;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            MinusZero z = MinusZero.Instance;

            if (codeControl.editor.Text != "")
            {

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                z.Root.Get(false, @"Home:\CurrentUser:\QueriesRoot:").AddVertex(null, codeControl.editor.Text);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }
        }

        private void Queries_SelectedEdgesChange()
        {
            IVertex selectedQuery = Queries.Vertex.Get(false, @"SelectedEdges:\\To:");

            if (selectedQuery != null)
            {
                codeControl.editor.Text = selectedQuery.Value?.ToString() ?? "";
            }
        }
    }
}
