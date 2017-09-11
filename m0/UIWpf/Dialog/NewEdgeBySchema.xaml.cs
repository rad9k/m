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

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for GetGraphCreationCodeDialog.xaml
    /// </summary>
    public partial class NewEdgeBySchema : UserControl
    {
        public override string ToString()
        {
            return "New Vertex by Schema";
        }


        IVertex Vertex, MetaVertex;

        public NewEdgeBySchema(IVertex _Vertex, IVertex _MetaVertex)
        {
            InitializeComponent();

            Vertex = _Vertex;
            MetaVertex = _MetaVertex;

            NewButton.Content = "New " + MetaVertex.Value + " Edge";
        }

       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {            
            Vertex.AddEdge(MetaVertex, this.To.Vertex.Get(@"BaseEdge:\To:\To:"));

            MinusZero.Instance.DefaultShow.CloseWindowByContent(this);
        }
    }
}
