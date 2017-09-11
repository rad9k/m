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

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for GetGraphCreationCodeDialog.xaml
    /// </summary>
    public partial class NewEdge : UserControl
    {
        public override string ToString()
        {
            return "New Edge";
        }

        IVertex Vertex;

        public NewEdge(IVertex _Vertex)
        {
            InitializeComponent();

            Vertex = _Vertex;

            
            IVertex MetaEdge=MinusZero.Instance.CreateTempVertex();   
                     
            Edge.AddEdgeEdgesOnlyTo(MetaEdge, MinusZero.Instance.Empty);

            GraphUtil.ReplaceEdge(this.Meta.Vertex.Get("BaseEdge:"),"To", MetaEdge);


            IVertex ToEdge = MinusZero.Instance.CreateTempVertex();

            Edge.AddEdgeEdgesOnlyTo(ToEdge, MinusZero.Instance.Empty);

            GraphUtil.ReplaceEdge(this.To.Vertex.Get("BaseEdge:"), "To", ToEdge);



        }

    

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!GeneralUtil.CompareStrings(this.To.Vertex.Get(@"BaseEdge:\To:\To:").Value, "$Empty"))
            {
                if (GeneralUtil.CompareStrings(this.Meta.Vertex.Get(@"BaseEdge:\To:\To:").Value, "$Empty"))
                    Vertex.AddEdge(null, this.To.Vertex.Get(@"BaseEdge:\To:\To:"));
                else
                    Vertex.AddEdge(this.Meta.Vertex.Get(@"BaseEdge:\To:\To:"), this.To.Vertex.Get(@"BaseEdge:\To:\To:"));

                MinusZero.Instance.DefaultShow.CloseWindowByContent(this);
            }
        }
    }
}
