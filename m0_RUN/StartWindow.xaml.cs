using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Commands;
using m0.ZeroCode;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
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

            LovFlov.LovFlov.Execute();

            m0Main.mainTree.UpdateVertex();

            ExtraRun8();

           // ExtraRun6();

            //ExtraRun5();

            //ExtraRun();

           // ExtraRun2();

            //ExtraRun3();

            //ExtraRun4();

            Close();
        }

        static IVertex r = null; 
        static IVertex UXTest = null; 

        void ExtraRun8()
        {
            r = m0.MinusZero.Instance.root;

            UXTest = r.Get(false, @"System\Meta\Visualiser\UXTest");

            IVertex e = r.Get(false, "examples");

            IVertex v = e.AddVertex(null, "X");

            IEdge a_e = VertexOperations.AddInstanceAndReturnEdge(e, r.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator"));

            UXAggregator a = new UXAggregator(a_e);

            a.Vertex.Value = "VIS";

            UXAdd(a, v);
        }

        UXItem UXAdd(UXItem a, IVertex v)
        {
            UXItem i1 = a.AddItem_UXAggregator(UXTest);

            i1.Layout = LayoutTypeEnum.Manual;
            i1.PositionCreate();
            i1.Position.X = 10;
            i1.Position.Y = 10;
            i1.SizeCreate();
            i1.Size.Width = 100;
            i1.Size.Height = 100;

            i1.BackgroundColorCreate();
            i1.BackgroundColor.Red = 100;

            i1.ForegroundColorCreate();
            i1.ForegroundColor.Blue = 255;
            i1.ForegroundColor.Green = 255;

            i1.BorderSize = 5;
            i1.BorderColorCreate();
            i1.BorderColor.Green = 100;

            i1.BaseEdgeCreate();

            i1.BaseEdge.To = v;

            UXItem i2 = a.AddItem_UXAggregator(UXTest);

            i2.Layout = LayoutTypeEnum.Manual;
            i2.PositionCreate();
            i2.Position.X = 150;
            i2.Position.Y = 150;
            i2.SizeCreate();
            i2.Size.Width = 100;
            i2.Size.Height = 100;

            i2.BackgroundColorCreate();
            i2.BackgroundColor.Blue = 100;

            i2.ForegroundColorCreate();
            i2.ForegroundColor.Red = 250;

            i2.BaseEdgeCreate();

            i2.BaseEdge.To = v;
            
            UXAdd2(i1, v);

            UXAdd2(i2, v);

            return i1;
        }

        UXItem UXAdd2(UXItem a, IVertex v)
        {
            UXItem i1 = a.AddItem_UXAggregator(UXTest);

            i1.Layout = LayoutTypeEnum.Manual;
            i1.PositionCreate();
            i1.Position.X = 10;
            i1.Position.Y = 10;
            i1.SizeCreate();
            i1.Size.Width = 30;
            i1.Size.Height = 30;

            i1.BackgroundColorCreate();
            i1.BackgroundColor.Red = 100;

            i1.ForegroundColorCreate();
            i1.ForegroundColor.Blue = 255;
            i1.ForegroundColor.Green = 255;

            i1.BorderSize = 5;
            i1.BorderColorCreate();
            i1.BorderColor.Green = 100;

            i1.BaseEdgeCreate();

            i1.BaseEdge.To = v;

            UXItem i2 = a.AddItem_UXAggregator(UXTest);

            i2.Layout = LayoutTypeEnum.Manual;
            i2.PositionCreate();
            i2.Position.X = 50;
            i2.Position.Y = 50;
            i2.SizeCreate();
            i2.Size.Width = 30;
            i2.Size.Height = 30;

            i2.BackgroundColorCreate();
            i2.BackgroundColor.Blue = 100;

            i2.ForegroundColorCreate();
            i2.ForegroundColor.Red = 250;

            i2.BaseEdgeCreate();

            i2.BaseEdge.To = v;

            return i1;
        }



        void ExtraRun7()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex e = r.Get(false, "examples");

            IEdge c = VertexOperations.AddInstanceAndReturnEdge(e, r.Get(false, @"System\Meta\ZeroTypes\UX\Color"));

            ZeroTypes.UX.Color clr = new ZeroTypes.UX.Color(c);

            int red = clr.Red;
            int green = clr.Green;
            int blue = clr.Blue;
            int opacity = clr.Opacity;

            clr.Opacity = 150;

            IEdge i_e = VertexOperations.AddInstanceAndReturnEdge(e, r.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator"));

            UXAggregator i = new UXAggregator(i_e);

            i.BackgroundColorCreate();
            i.BackgroundColor.Blue = 1;

            i.BorderColorCreate();
            i.BorderColor.Green = 2;

            i.BorderSize = 3;

            i.DesignMode = true;

            i.ForegroundColorCreate();
            i.ForegroundColor.Red = 4;

            i.Layout = LayoutTypeEnum.Manual;

            i.Margin = 4;

            i.SizeCreate();
            i.Size.Height = 50;
            i.Size.Width = 500;

            i.PositionCreate();
            i.Position.X = 12;
            i.Position.Y = 13;

            i.ExpandedSizeCreate();
            i.ExpandedSize.Width = 111;
            i.ExpandedSize.Height = 222;

            i.CollapsedSizeCreate();
            i.CollapsedSize.Width = 333;
            i.CollapsedSize.Height = 444;


            for (int x = 0; x < 10; x++)
                i.AddItem_UXItem().Vertex.Value = x;

            for (int x = 0; x < 10; x++)
                i.AddItem_UXItem(r.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator")).Vertex.Value = x;
        }

        void ExtraRun6()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex xxx = r.Get(false, "examples").AddVertex(null, "XXX");

            IVertex c = xxx.AddVertex(null, "C");

            IVertex x = c.AddVertex(null, "X");
            x.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), r.Get(false, @"System\Meta\ZeroTypes\Float"));

            IVertex y = c.AddVertex(null, "Y");
            y.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), r.Get(false, @"System\Meta\ZeroTypes\Float"));

            IVertex z = c.AddVertex(null, "Z");
            z.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), r.Get(false, @"System\Meta\ZeroTypes\Float"));

            IVertex w = c.AddVertex(null, "W");
            w.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), r.Get(false, @"System\Meta\ZeroTypes\Float"));

            IVertex ed1 = xxx.AddVertex(null, "edge1");
            ed1.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), c);

            IVertex ed2 = xxx.AddVertex(null, "edge2");
            ed2.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), c);

            IVertex data = xxx.AddVertex(null, "data");

            for(double xx=-30; xx < 30 ; xx += 0.1)
            //for (double xx = 1.99; xx <= 3; xx++)
            //for (double xx = -3.01; xx <= 200.01; xx++)
            //for (double xx = 2; xx <= 3.01; xx+=1.01)
            {
                IVertex d = data.AddVertex(ed1, xx);

                d.AddVertex(x, xx);
                d.AddVertex(y, xx * 2);
                d.AddVertex(z, xx * -4);
                d.AddVertex(w, Math.Sin(xx));

                d = data.AddVertex(ed2, xx);

                d.AddVertex(x, xx * 10 );
                d.AddVertex(y, xx * 20);
                d.AddVertex(z, xx * -400);
                d.AddVertex(w, Math.Sin(((double)xx)/3));
            }

            BaseCommands.OpenVisualiser(EdgeHelper.CreateTempEdgeVertex(null, null, data), r.Get(false, @"System\Meta\Visualiser\Set2D"));
        }

        void ExtraRun5()
        {
            IVertex x = m0.MinusZero.Instance.root.Get(false, "examples").AddVertex(null, "X");



            GraphUtil.LoadParseAndMove(@"..\..\..\m0_SYSTEM_GENERATE\bin\Debug\_RES\Generator\test.txt", x, "'SimpleTransformer'");

            //GraphUtil.LoadParseAndMove(@"..\..\..\m0_SYSTEM_GENERATE\bin\Debug\_RES\Generator\SimpleTransformer.txt", x, "'SimpleTransformer'");

            //ZeroCodeView.GraphDebug(ZeroCodeView.LinearizeGraph(x), "y_pis.txt");
            //ZeroCodeView.GraphDebug(x, "z_pis.txt");

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
