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
           // MinusZero.Instance.root.Get(false, @"V\kupa").AddVertex(null, "XX");
            return null;
        }

        public INoInEdgeInOutVertexVertex yyy(IExecution exe) {                        

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

            ExtraRun5();

            //ExtraRun();

           // ExtraRun2();

            //ExtraRun3();

            //ExtraRun4();

            Close();
        }

        void ExtraRun5()
        {
            IVertex x = m0.MinusZero.Instance.root.Get(false, "examples").AddVertex(null, "X");



            GraphUtil.LoadParseAndMove(@"..\..\..\m0_SYSTEM_GENERATE\bin\Debug\_RES\Generator\test2.txt", x, "'SimpleTransformer'");

            //GraphUtil.LoadParseAndMove(@"..\..\..\m0_SYSTEM_GENERATE\bin\Debug\_RES\Generator\SimpleTransformer.txt", x, "'SimpleTransformer'");

            //ZeroCodeView.LinearizeDebug(x.Get(false, @"Class:\Method : PutIntoStore"), "x_mod.txt");
           // ZeroCodeView.LinearizeDebug(x.Get(false, @"Class:\Method : PutIntoStore"), "x_mod2.txt");
           // ZeroCodeView.LinearizeDebug(x.Get(false, @"Class:\Method : PutIntoStore"), "x_mod3.txt");


        }

        void ExtraRun4()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex examples = r.Get(false, "examples");

            examples.AddVertex(r.Get(false, @"System\FormalTextLanguage\ZeroCode\ZeroCodeView"), "XX");
        }


        void ExtraRun3()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex examples = r.Get(false, "examples");


            test t = new test();

            IEdge listener = ExecutionFlowHelper.AddTriggerAndListener_NonTransacted(examples, t.xxx);


            r.AddVertex(examples, "XXX");
            //r.AddEdge(null, examples);

            if (examples.HasOnlyNonTransactedRootVertexEventsEdge)
            {
                int x = 0;
            }
        }


        void ExtraRun2()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex v = r/*.Get(false, "examples")*/.AddVertex(null, "X");

            List<string> scopelist = new List<string>();

            scopelist.Add("met");
            scopelist.Add("znak");


            IVertex trigger = GraphChangeTrigger.AddTrigger(v, scopelist, new List<GraphChangeFilterEnum>
            {
                // GraphChangeFilterEnum.FilterOutRootVertexEvents,
                GraphChangeFilterEnum.InputEdgeAdded
                //GraphChangeFilterEnum.ValueChange
                //         GraphChangeFilterEnum.MetaEdgeAdded
            }).To;



            test t = new test();

            ExecutionFlowHelper.AddListener_DotNetDelegate(trigger, t.xxx);

            ExecutionFlowHelper.StartTransaction();

            v.AddVertex(null, "znak");

            //IVertex meta = v.AddVertex(null, "met");

            //meta.AddVertex(null, "new");

            //r.Get(false, "examples").AddVertex(meta, "nowy werteks").AddVertex(meta, "new");

            ExecutionFlowHelper.CommitTransaction();
        }

        void ExtraRun()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex v = r.Get(false, "examples").AddVertex(null, "V");

            List<string> scopelist = new List<string>();

            scopelist.Add("child");
            

            IVertex trigger = GraphChangeTrigger.AddTrigger(v, scopelist, new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                     GraphChangeFilterEnum.FilterOutRootVertexEvents,
                     GraphChangeFilterEnum.OutputEdgeAdded,
                     GraphChangeFilterEnum.OutputEdgeRemoved,
                     GraphChangeFilterEnum.OutputEdgeDisposed}).To;
            


            test t = new test();

            ExecutionFlowHelper.AddListener_DotNetDelegate(trigger, t.xxx);

            ExecutionFlowHelper.StartTransaction();

            v.AddVertex(null, "test");

            ExecutionFlowHelper.CommitTransaction();

            ExecutionFlowHelper.StartTransaction();

            IVertex c = v.AddVertex(null, "child");

            c.AddVertex(null, "new");

           // GraphChangeTrigger.AddEventTriggerAndListener(k, new List<string> { }, null, "t", t.yyy, "l");

            ExecutionFlowHelper.CommitTransaction();

            return;

            

            v.Value = "kupa";

            

            v.AddVertex(v, "dupa1");

       /*     IVertex d2 = v.AddVertex(null, "dupa2");

            IVertex d22 = d2.AddVertex(null, "dupa22");

            d22.AddVertex(null, "test");

            

            IEdge e = v.OutEdges[2];*/

            //v.DeleteEdge(e);

    

            

            //ExecutionFlowHelper.RollbackTransaction();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m0_RUN.AltWindow w = new m0_RUN.AltWindow();
            w.Show();
        }
    }
}
