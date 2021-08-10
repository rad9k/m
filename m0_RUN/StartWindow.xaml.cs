using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.ZeroCode;
using m0.ZeroTypes;
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

namespace m0
{
    class test
    {
        public INoInEdgeInOutVertexVertex xxx(IExecution exe)
        {
            return null;
        }
    }

    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();

           // m0_RUN.Main.Run();

          //  Close();            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {            
            m0_RUN.Main.Run();

          //  ExtraRun();

            Close();
        }       

        void ExtraRun()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex v = r.AddVertex(null, "V");

            List<string> scopelist = new List<string>();

            scopelist.Add(@"");

            IVertex trigger = ExecutionFlowHelper.AddGraphChangeTrigger(v, scopelist).To;

            test t = new test();

            ExecutionFlowHelper.AddListener_DotNetDelegate(trigger, t.xxx);

            ExecutionFlowHelper.StartTransaction();

            v.Value = "kupa";

            

            v.AddVertex(null, "dupa1");

            IVertex d2 = v.AddVertex(null, "dupa2");

            IVertex d22 = d2.AddVertex(null, "dupa22");

            d22.AddVertex(null, "test");

            

            IEdge e = v.OutEdges[2];

            //v.DeleteEdge(e);

    

            ExecutionFlowHelper.CommitTransaction();

            //ExecutionFlowHelper.RollbackTransaction();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m0_RUN.AltWindow w = new m0_RUN.AltWindow();
            w.Show();
        }
    }
}
