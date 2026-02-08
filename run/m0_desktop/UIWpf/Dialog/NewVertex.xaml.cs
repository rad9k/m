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
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using m0.User.Process.UX;
using m0.UIWpf.Visualisers;

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for GetGraphCreationCodeDialog.xaml
    /// </summary>
    public partial class NewVertex : UserControl
    {
        EdgeVisualiser Schema;

        public override string ToString()
        {
            return "New Vertex";
        }

        IVertex Vertex;

        public NewVertex(IVertex _Vertex)
        {
            InitializeComponent();

            Vertex = _Vertex;

            IVertex schemaBaseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(
                               null,
                               null,
                               EdgeHelper.CreateTempEdgeVertex(null,
                                    null,
                                    MinusZero.Instance.Empty));

            Schema = new EdgeVisualiser(schemaBaseEdgeVertex, null, false);

            Schema_Border.Child = Schema;

            //IVertex SchemaEdge=MinusZero.Instance.CreateTempVertex();            
            //Edge.AddEdgeVertexEdgesOnlyTo(SchemaEdge,MinusZero.Instance.Empty);
            //GraphUtil.ReplaceEdge(this.Schema.Vertex.Get(false, "BaseEdge:"),"To",SchemaEdge);

            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e){
            //Keyboard.Focus(Content);
            //FocusManager.SetFocusedElement(this,Content);
            Content.Focus();
        }

        void FinishDialog()
        {
            MinusZero.Instance.UserInteraction.CloseWindowByContent(this);

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (GeneralUtil.CompareStrings(this.Schema.Vertex.Get(false, @"BaseEdge:\To:\To:").Value, "$Empty"))
                Vertex.AddVertex(null, this.Content.Text);
            else
            {
                //Vertex.AddVertex(this.Schema.Vertex.Get(false, @"BaseEdge:\To:\To:"), this.Content.Text);

                IVertex meta = this.Schema.Vertex.Get(false, @"BaseEdge:\To:\To:");

                IVertex v = VertexOperations.AddInstance(Vertex, meta);

                v.Value = this.Content.Text;

                if (VertexOperations.GetChildEdges(meta).Any())
                    MinusZero.Instance.UserInteraction.EditEdge(v);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FinishDialog();
        }

        private void Content_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FinishDialog();
        }
    }
}
