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
    public partial class NewEdge : UserControl
    {
        EdgeVisualiser Meta;
        EdgeVisualiser To;


        public override string ToString()
        {
            return "New Edge";
        }

        IVertex Vertex;

        public NewEdge(IVertex _Vertex)
        {
            InitializeComponent();

            Vertex = _Vertex;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex metaBaseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(
                                           null,
                                           null,
                                           EdgeHelper.CreateTempEdgeVertex(null,
                                                null,
                                                MinusZero.Instance.Empty));

            Meta = new EdgeVisualiser(metaBaseEdgeVertex, null, false);

            Meta_Border.Child = Meta;

            //IVertex MetaEdge =MinusZero.Instance.CreateTempVertex();   

            //Edge.AddEdgeVertexEdgesOnlyTo(MetaEdge, MinusZero.Instance.Empty);

            //GraphUtil.ReplaceEdge(this.Meta.Vertex.Get(false, "BaseEdge:"),"To", MetaEdge);

            IVertex toBaseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(
                                           null,
                                           null,
                                           EdgeHelper.CreateTempEdgeVertex(null,
                                                null,
                                                MinusZero.Instance.Empty));

            To = new EdgeVisualiser(toBaseEdgeVertex, null, false);

            To_Border.Child = To;

            //IVertex ToEdge = MinusZero.Instance.CreateTempVertex();

            //Edge.AddEdgeVertexEdgesOnlyTo(ToEdge, MinusZero.Instance.Empty);

            //GraphUtil.ReplaceEdge(this.To.Vertex.Get(false, "BaseEdge:"), "To", ToEdge);

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////

        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //if (!GeneralUtil.CompareStrings(this.To.Vertex.Get(false, @"BaseEdge:\To:\To:").Value, "$Empty")) // can make edge to $Empty
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                if (GeneralUtil.CompareStrings(this.Meta.Vertex.Get(false, @"BaseEdge:\To:\To:").Value, "$Empty"))
                    Vertex.AddEdge(null, this.To.Vertex.Get(false, @"BaseEdge:\To:\To:"));
                else
                    Vertex.AddEdge(this.Meta.Vertex.Get(false, @"BaseEdge:\To:\To:"), this.To.Vertex.Get(false, @"BaseEdge:\To:\To:"));

                //////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                //////////////////////////////////////

                To.Dispose(); // TO BE ADDED TO ALL DIALOGS !!!!!!!

                MinusZero.Instance.UserInteraction.CloseWindowByContent(this);
            }
        }
    }
}
