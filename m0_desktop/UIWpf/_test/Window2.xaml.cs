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
using System.Windows.Shapes;
using m0.Foundation;
using m0.Util;
using m0.Graph;

namespace m0.UIWpf._test
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();


            MinusZero m = MinusZero.Instance;

            IVertex r = m.Root.AddVertex(null, "0");

            GeneralUtil.ParseAndExcute(r, null, "{\"meta1\"{test1{test11,test12}},meta2{test2},meta3{test3}}");




            for (int zzz = 0; zzz < 5; zzz++)
                for (int x = 1; x <= 10; x++)
                {
                    IVertex rr = r.AddVertex(null, "node " + x);

                    for (int xx = 1; xx <= 5; xx++)
                    {
                        IVertex rrr = rr.AddVertex(null, "2node " + xx);

                        for (int xxx = 1; xxx <= 3; xxx++)
                            rrr.AddVertex(null, "3node " + zzz);
                        //rrr.AddVertex(GraphUtil.DeepFindOneByValue(r, "meta" + xxx), "a");
                    }

                }

            IVertex tmp = m.GetTempVertex();

            GraphUtil.CopyEdges(r, tmp);

            GraphUtil.ReplaceEdge(this.ListVisualiser1.Vertex, "BaseVertex", r);

            GraphUtil.ReplaceEdge(this.ListVisualiser1.Vertex, "SelectedVertexes", tmp);            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
