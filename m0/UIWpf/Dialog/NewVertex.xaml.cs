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
    public partial class NewVertex : UserControl
    {
        public override string ToString()
        {
            return "New Vertex";
        }

        IVertex Vertex;

        public NewVertex(IVertex _Vertex)
        {
            InitializeComponent();

            Vertex = _Vertex;

            
            IVertex SchemaEdge=MinusZero.Instance.CreateTempVertex();            
            Edge.AddEdgeEdgesOnlyTo(SchemaEdge,MinusZero.Instance.Empty);
            GraphUtil.ReplaceEdge(this.Schema.Vertex.Get("BaseEdge:"),"To",SchemaEdge);

            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e){
            //Keyboard.Focus(Content);
            //FocusManager.SetFocusedElement(this,Content);
            Content.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MinusZero.Instance.DefaultShow.CloseWindowByContent(this);

            if (GeneralUtil.CompareStrings(this.Schema.Vertex.Get(@"BaseEdge:\To:\To:").Value, "$Empty"))
                Vertex.AddVertex(null, this.Content.Text);
            else
            {
                //Vertex.AddVertex(this.Schema.Vertex.Get(@"BaseEdge:\To:\To:"), this.Content.Text);

                IVertex meta = this.Schema.Vertex.Get(@"BaseEdge:\To:\To:");

                IVertex v=VertexOperations.AddInstance(Vertex, meta);

                v.Value = this.Content.Text;

                if(VertexOperations.GetChildEdges(meta).Count()>0)
                    MinusZero.Instance.DefaultShow.EditDialog(v, null);
            }             
        }
    }
}
