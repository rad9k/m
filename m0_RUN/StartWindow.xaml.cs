using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Commands;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

          //  LovFlov.LovFlov.Execute();

            m0Main.mainTree.UpdateVertex();

            //ExtraRun8();

            //ExtraRun6();

            //ExtraRun5();

            //ExtraRun();

           // ExtraRun2();

            //ExtraRun3();

            //ExtraRun4();

            Close();
        }

        static IVertex r = null; 

        static IVertex UXTest = null;
        static IVertex UXContainerType = null;
        static IVertex UXItemType = null;
        static IVertex RectangleItem = null;
        static IVertex ContainerItem = null;

        static IEdge template = null;

        void ExtraRun8()
        {
            r = m0.MinusZero.Instance.root;

            UXTest = r.Get(false, @"System\Meta\Visualiser\UXTest");

            UXContainerType = r.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer");

            UXItemType = r.Get(false, @"System\Meta\ZeroTypes\UX\UXItem");

            RectangleItem = r.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem");

            ContainerItem = r.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem");

            template = r.GetAll(false, @"System\Data\UX\Templates\ZeroUML\Vertex").First();

            IVertex e = r.Get(false, "examples");

            IEdge e_edge = r.GetAll(false, "examples").FirstOrDefault();

            IVertex x_vertex = e.AddVertex(null, "X");

            //x_vertex.AddEdge(x_vertex, x_vertex);

            IVertex z_vertex =  x_vertex.AddVertex(x_vertex, "Z");

            IVertex agg_vertex = r.Get(false, @"System\Meta\ZeroTypes\UX");

            IVertex x1_vertex = x_vertex.AddVertex(agg_vertex, "X1");
            //IVertex x1_vertex = x_vertex.AddVertex(null, "X1");

            IVertex x2_vertex = e.AddVertex(agg_vertex, "X2");           

            //IVertex z1_vertex = x_vertex.AddVertex(x_vertex, "Z1");
            IVertex z2_vertex = null; // x_vertex.AddVertex(x_vertex, "Z2");

            //z1_vertex.AddEdge(null, x1_vertex);

            IEdge a_e = VertexOperations.AddInstanceAndReturnEdge(e,
                UXContainerType,
                UXContainerType);

            UXContainer a = new UXContainer(a_e);

            a.BaseEdgeSet(e_edge);

            //a.PositionCreate();

            //a.Position.X = 0;
            //a.Position.Y = 0;

            UXTemplate diagram_template = new UXTemplate(r.GetAll(false, @"System\Data\UX\Templates\ZeroUML").FirstOrDefault());
            a.UXTemplate = diagram_template;

            a.Vertex.Value = "VIS";

            a.SizeCreate();

            a.Size.Width = 5000;
            a.Size.Height = 5000;
            
            IUXItem xi = UXAdd(0,0, a, x_vertex);

            //UXAdd2(xi, x1_vertex);

            IUXItem zi = UXAdd(200, 200, a, z_vertex);

           // UXAdd2(zi, z1_vertex, z2_vertex);
        }

        IUXItem UXAdd(double x, double y, UXItem a, IVertex v)
        {
            IUXItem i1 = (IUXItem)a.AddItem(ContainerItem);

            i1.UXTemplate = new UXTemplate(template);
            i1.Layout = LayoutTypeEnum.Manual;
            i1.PositionCreate();
            i1.Position.X = x+10;
            i1.Position.Y = y+10;
            i1.SizeCreate();
            i1.Size.Width = 100;
            i1.Size.Height = 100;

            //i1.BackgroundColorCreate();
            //i1.BackgroundColor.Red = 100;

            i1.ForegroundColorCreate();
            i1.ForegroundColor.Blue = 255;
            i1.ForegroundColor.Green = 255;

            i1.BorderSize = 5;
            i1.BorderColorCreate();
            i1.BorderColor.Green = 100;

            i1.BaseEdgeCreate();

            i1.BaseEdge.To = v;            

            return i1;
        }

        IUXItem UXAdd2(IUXItem a, IVertex v)
        {
            IUXItem i1 = (IUXItem)a.AddItem(RectangleItem);

            i1.UXTemplate = new UXTemplate(template);
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

            /*IUXItem i2 = (IUXItem)a.AddItem(RectangleItem);

            i2.UXTemplate = new UXTemplate(template);
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

            i2.BaseEdge.To = v2;*/

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

            UXContainer i = new UXContainer(i_e);

            i.BackgroundColorCreate();
            i.BackgroundColor.Blue = 1;

            i.BorderColorCreate();
            i.BorderColor.Green = 2;

            i.BorderSize = 3;

            i.DesignMode = true;

            i.ForegroundColorCreate();
            i.ForegroundColor.Red = 4;

            i.Layout = LayoutTypeEnum.Manual;

            i.Gap = 4;

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


            /*for (int x = 0; x < 10; x++)
                i.AddItem_UXItem().Vertex.Value = x;

            for (int x = 0; x < 10; x++)
                i.AddItem_UXItem(r.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator")).Vertex.Value = x;*/
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

            data.AddEdge(xxx, null);

            for (double xx=-30; xx < 30 ; xx += 0.1)
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
        }
    }
}
