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

using m0.UIWpf;
using m0.Graph;

using Xceed.Wpf.AvalonDock.Layout;
using m0.Foundation;

using m0.UIWpf.Visualisers;
using m0.ZeroTypes;
using m0.Util;
using Xceed.Wpf.AvalonDock.Controls;
using m0.UIWpf.Dialog;

namespace m0
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class m0Main : Window, IShow
    {
        public static m0Main Instance;        

        public m0Main()
        {
            Instance = this;

            InitializeComponent();

            MinusZero.Instance.Initialize();


            CreateTestData();


            TreeVisualiser stv = new TreeVisualiser();

            GraphUtil.ReplaceEdge(stv.Vertex.Get("BaseEdge:"), "To", MinusZero.Instance.Root);                                    
            
            this.root.Content=stv;
        }

        

        private string randomChars()
        {
            Random r = new Random();
            int x=r.Next(5);

            string xxx = "";

            for(int xx=0;xx<x;xx++)
                xxx+=xx;

            return xxx;
        }

        private void CreateTestData()
        {
            IVertex r=MinusZero.Instance.Root;

            GeneralUtil.ParseAndExcute(r, r.Get(@"System\Meta"), @"{TEST3{Class:Customer{},Class:Person{$Description:opis,Attribute:Name,Attribute:Surname,Attribute:DateOfBirth},Class:Company{Attribute:Name,Attribute:RegistrationNumber,},Class:Adress{Attribute:Line 1,Attribute:Line 2,Attribute:Line 3,Attribute:City,Attribute:County,Attribute:Postal code,Attribute:Country},Class:Basket{Attribute:Creation date,Attribute:Status},Class:Item{Attribute:Name,Attribute:Description,Attribute:Price}}}");

            r.Get(@"TEST3\Customer").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Person").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Company").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Adress").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Basket").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Item").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));

            GeneralUtil.ParseAndExcute(r, r.Get(@"System\Meta"), "{TEST2,TEST{Class:Person{Association:Spouse{$MaxCardinality:1,$MaxTargetCardinality:1},Aggregation:Child{$MaxCardinality:3},Attribute:Name,Attribute:Surname,Attribute:Age{MinValue:0,MaxValue:40},Attribute:NoseLength{MinValue:0,MaxValue:40},Attribute:Money{MinValue:0,MaxValue:1000},Attribute:IsGood,Attribute:IsPretty,Attribute:IsPretty2,Attribute:IsPretty3},Enum:Pretty{EnumValue:Yes,EnumValue:No,EnumValue:Maybe}}}");

            r.Get(@"TEST\Pretty").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"System\Meta\ZeroTypes\EnumBase"));
            r.Get(@"TEST\Person").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));

            IVertex smzt=r.Get(@"System\Meta\ZeroTypes");

            IVertex EdgeTarget = r.Get(@"System\Meta*$EdgeTarget");

            IVertex Person = r.Get(@"TEST\Person");



            IVertex smu = r.Get(@"System\Meta\UML");
            IVertex smb = r.Get(@"System\Meta\Base");


          IVertex function_function = Person.AddVertex(smu.Get(@"Function"), "Sleep");

            function_function.AddEdge(smu.Get(@"Function\Output"), smzt.Get("Integer"));

            IVertex ffi = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "time");

            ffi.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Integer"));

            //
            ffi.AddVertex(smb.Get(@"Vertex\$Description"), "this is opis time");
            function_function.AddVertex(smb.Get(@"Vertex\$Description"), "this is opis function");
            //

            IVertex ffi2 = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "fastMode");

            ffi2.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Boolean"));

            IVertex ffi3 = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "interrupt");

            ffi3.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Integer"));


            function_function.AddVertex(smu.Get(@"[]"), null);

            function_function.AddEdge(smb.Get(@"$NewLine"), smb.Get(@"$Empty"));

            //

            /*IVertex function2_function = Person.AddVertex(smu.Get(@"Function"), "Sleep");

            function2_function.AddEdge(smu.Get(@"Function\Output"), smzt.Get("Integer"));

            IVertex f2fi = function2_function.AddVertex(smu.Get(@"Function\InputParameter"), "time");

            f2fi.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Integer"));


            function2_function.AddVertex(smu.Get(@"[]"), null);

            function2_function.AddVertex(smb.Get(@"$NewLine"), "3");

            */

            Person.Get("Name").AddEdge(EdgeTarget, smzt.Get("String"));

            Person.Get("Spouse").AddEdge(r.Get(@"System\Meta*$EdgeTarget"), Person);
            Person.Get("Child").AddEdge(r.Get(@"System\Meta*$EdgeTarget"), Person);

            Person.Get("Surname").AddEdge(EdgeTarget, smzt.Get("String"));
            Person.Get("Age").AddEdge(EdgeTarget, smzt.Get("Integer"));
            Person.Get("NoseLength").AddEdge(EdgeTarget, smzt.Get("Float"));
            Person.Get("Money").AddEdge(EdgeTarget, smzt.Get("Decimal"));
            Person.Get("IsGood").AddEdge(EdgeTarget, smzt.Get("Boolean"));
            Person.Get("IsPretty").AddEdge(EdgeTarget, r.Get(@"TEST\Pretty"));
            Person.Get("IsPretty2").AddEdge(EdgeTarget, r.Get(@"TEST\Pretty"));
            Person.Get("IsPretty3").AddEdge(EdgeTarget, r.Get(@"TEST\Pretty"));

            //Person.AddEdge(smu.Get(@"Class\Attribute"), Person.Get("Surname"));
            // what is it for?

            GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"TEST"), "{Person:Person1{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");

            GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"TEST"), "{Person:Person3{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person4{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");
            
            r.Get(@"TEST\Person1").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\Person2").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\Person3").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\Person4").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));

            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person1"), "IsPretty", r.Get(@"TEST\Pretty\No"));
            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person2"), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person3"), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person4"), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
                

            for (int x = 0; x < 1; x++)
            {
                GeneralUtil.ParseAndExcute(r.Get("TEST2"), r.Get(@"TEST"), "{Person:Person1"+x+"{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2"+x+"{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");
                
              
                GeneralUtil.ParseAndExcute(r.Get("TEST2"), r.Get(@"TEST"), "{Person:Person3"+x+"{Name:Magda,Surname:Tereszczuk,Age:18,NoseLength:\"2,1\",Money:999,IsGood:True,IsPretty:},Person:Person4"+x+"{Name:Jan,Surname:Kuciak,Age:10,NoseLength:0.6,Money:99999,IsGood:True,IsPretty:}}");

                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person1"+x), "IsPretty", r.Get(@"TEST\Pretty\No"));
                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person2"+x), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person3"+x), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person4"+x), "IsPretty", r.Get(@"TEST\Pretty\Yes"));

                r.Get(@"TEST2\Person1"+x+@"\Radek").AddEdge(r.Get(@"System\Meta*$Is"), smzt.Get("String"));


                r.Get(@"TEST2\Person1"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
                r.Get(@"TEST2\Person2"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
                r.Get(@"TEST2\Person3"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
                r.Get(@"TEST2\Person4"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            }

            for (int x = 0; x < 1; x++)
                for (int y = 0; y < 1; y++)
                {
                r.Get(@"TEST2\Person1" + x).AddEdge(r.Get(@"TEST\Person\Child"), r.Get(@"TEST2\Person1" + y));
                r.Get(@"TEST2\Person2" + x).AddEdge(r.Get(@"TEST\Person\Child"), r.Get(@"TEST2\Person2" + y));
                r.Get(@"TEST2\Person3" + x).AddEdge(r.Get(@"TEST\Person\Child"), r.Get(@"TEST2\Person3" + y));
                r.Get(@"TEST2\Person4" + x).AddEdge(r.Get(@"TEST\Person\Child"), r.Get(@"TEST2\Person4" + y));
            }

            for (int i = 1; i <= 1; i++)
            {
                IVertex x=r.Get("TEST2").AddVertex(null, i);

                for (int ii = 1; ii <= 1; ii++)
                {
                    IVertex xx = x.AddVertex(null, i + " " + ii);

                    for (int iii = 1; iii <= 1; iii++)
                    {
                        IVertex xxx=xx.AddVertex(null, i + " " + ii + " " + iii);

                        for (int iiii = 1; iiii <= 3; iiii++)
                            xxx.AddVertex(null, i + " " + ii + " " + iii+" "+iiii);
                    }
                }
            }

            //r.Get(@"TEST2\1").AddEdge(null, r.Get(@"TEST2\2\2 2\2 2 1"));
            
            GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"System\Meta"), "{Diagram:TestDiagram{ZoomVisualiserContent:100,SelectedEdges:,CreationPool:}}");

            r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta\Visualiser\Diagram\SizeX"), 600.0);

            r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta\Visualiser\Diagram\SizeY"), 600.0);

            r.Get(@"TEST\TestDiagram").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta*Diagram"));
            
            IVertex i1=r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"),null);
            
            GeneralUtil.ParseAndExcute(i1,r.Get(@"System\Meta"),"{PositionX:0,PositionY:0,SizeX:100,SizeY:100}");

            IVertex i2 = r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i2, r.Get(@"System\Meta"), "{PositionX:200,PositionY:200,SizeX:100,SizeY:100}");

            i1.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i1.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i1, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person1"));

            i2.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i2, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person2"));

            



            i1 = r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i1, r.Get(@"System\Meta"), "{PositionX:350,PositionY:0}");

            i2 = r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i2, r.Get(@"System\Meta"), "{PositionX:0,PositionY:350}");

            i1.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            i1.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            Edge.AddEdgeByToVertex(i1, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person3"));

            i2.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i2, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person4"));

            /////////////////////

           /* GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"System\Meta"), "{Class:X1,Class:X2,Class:X3,Class:X4,Class:PersonA,Class:PersonB,Class:PersonB2{Attribute:New}}");

            r.Get(@"TEST\PersonB2\New").AddEdge(r.Get(@"System\Meta*$EdgeTarget"), r.Get(@"System\Meta*String"));

            VertexOperations.AddInstance(r.Get("TEST"), r.Get(@"TEST\PersonB2"), r.Get(@"TEST\Person")).Value="XXX";

            r.Get(@"TEST\X2").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\X3").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\X2"));
            r.Get(@"TEST\X4").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\X3"));

            r.Get(@"TEST\PersonA").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\PersonB").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\PersonB2").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\PersonB"));

            r.Get(@"TEST\XXX").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\PersonA"));
            r.Get(@"TEST\XXX").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\X4"));*/

            //////////////////////

            IVertex tt = r.Get("TEST").AddVertex(r.Get("System*Class"), "TestClass");

            for(int x=0;x<1;x++)
                for (int y = 0; y < 1; y++)
                {
                    IVertex tta = tt.AddVertex(r.Get("System*Attribute"), "a" + x + " " + y + ";" + randomChars());
                    tta.AddVertex(r.Get("System*$Group"), x.ToString());
                    tta.AddVertex(r.Get("System*$Section"), y.ToString());

                    tta.AddEdge(r.Get("System*$EdgeTarget"), r.Get("System*String"));

                    IVertex ttb = tt.AddVertex(r.Get("System*Attribute"), "b" + x + " " + y + ";" + randomChars());
                    ttb.AddVertex(r.Get("System*$Group"), x.ToString());
                    //ttb.AddVertex(r.Get("System*$Section"), y.ToString());
                    ttb.AddEdge(r.Get("System*$EdgeTarget"), r.Get("System*String"));

                    IVertex ttc = tt.AddVertex(r.Get("System*Attribute"), "c" + x + " " + y + ";" + randomChars());
                    ttc.AddVertex(r.Get("System*$Group"), x.ToString());
                    ttc.AddVertex(r.Get("System*$Section"), y.ToString());
                    ttc.AddVertex(r.Get("System*$MaxCardinality"), 6);
                    ttc.AddEdge(r.Get("System*$EdgeTarget"), r.Get("System*String"));
                }

            VertexOperations.AddInstance(r.Get("TEST"), tt);

            //////////////////////


            IVertex start = r.Get(@"TEST3");

            for (int i = 0; i < 1; i++) {
                IVertex sm = start.AddVertex(r.Get(@"System\Meta\UML\StateMachine"), "sm "+i);

                for (int ii = 0; ii < 1; ii++)
                    sm.AddVertex(r.Get(@"System\Meta\UML\StateMachine\State"), "state "+ii+" of machine"+i);

                IVertex allstates = sm.GetAll("");

                foreach (IEdge e in allstates)
                    foreach (IEdge ee in allstates)
                        e.To.AddEdge(r.Get(@"System\Meta\UML\StateMachine\State\Transition"), ee.To);
            }

            //////////////////////

            IVertex associations = r.GetAll(@"TEST\Person\Association:");
            IVertex ismeta = r.Get(@"System\Meta*$Is");
            IVertex asmeta = r.Get(@"System\Meta\UML\Class\Association");

            //foreach (IEdge v in associations)
             //   v.To.AddEdge(ismeta, asmeta);
            
            IVertex attributes = r.GetAll(@"TEST\Person\Attribute:");
            //IVertex ismeta=r.Get(@"System\Meta*$Is");
            IVertex ameta=r.Get(@"System\Meta\UML\Class\Attribute");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            attributes = r.GetAll(@"TEST3\\Attribute:");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            IVertex test = r.Get("TEST");

            test.AddVertex(test.AddVertex(null, "Counter"),(int)0);


            IVertex vvv = VertexOperations.AddInstance(test, r.Get(@"System\Meta\Base\$Import"));

            vvv.AddEdge(r.Get(@"System\Meta\Base\Vertex\$$IsLink"), MinusZero.Instance.Empty);

            vvv.Value="tst";

            test.AddEdge(r.Get(@"TEST\tst"), r.Get(@"System\Meta\Visualiser"));

            /////

            IVertex aattributes = r.GetAll(@"TEST\\Attribute:");

            IVertex isAggregation = r.Get(@"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = r.Get(@"System\Meta\Base\$Empty");

            foreach (IEdge v in aattributes)
                v.To.AddEdge(isAggregation, empty);

            IVertex aggregations = r.GetAll(@"TEST\\Aggregation:");

            foreach (IEdge v in aggregations)
                v.To.AddEdge(isAggregation, empty);

            ///
            
            IVertex vx=r.AddVertex(null, "X");

            IVertex my = vx.AddVertex(null, "j e s ");

            IVertex vxx = vx.AddVertex(null, "VXX");

            IVertex c = VertexOperations.AddInstance(vxx, smu.Get("[]"));
            
            c.Value="";

            c.AddVertex(smu.Get(@"MultiOperator\Expression"), "raz");

            IVertex dwa=c.AddVertex(smu.Get(@"MultiOperator\Expression"), "dwa");

            //dwa.AddVertex(smb.Get(@"Vertex\$Description"), "3 3 3");

            c.AddEdge(smu.Get(@"MultiOperator\Expression"), my);

            IVertex cztery=c.AddVertex(smu.Get(@"MultiOperator\Expression"), "cztery");

            //cztery.AddVertex(smb.Get(@"Vertex\$Description"), "cztery 1");

            //cztery.AddVertex(smb.Get(@"Vertex\$Description"), "cztery 2");


            // IVertex zzz = vx.AddVertex(null, "raz");

            //   IVertex yyy = vx.AddVertex(null, "dwa");

            //  IVertex wh = VertexOperations.AddInstance(vx, smu.Get(@"While"));


            // IVertex plus = VertexOperations.AddInstance(wh, smu.Get("-"), smu.Get(@"While\Test"));

            IVertex plus = VertexOperations.AddInstance(c, smu.Get("+"), smu.Get(@"MultiOperator\Expression"));

            //plus.AddVertex(smb.Get(@"Vertex\$Description"), "plus opis MAIN");

            //plus.AddVertex(null, "KUPA");

            //  plus.AddVertex(smu.Get(@"DoubleOperator\LeftExpression"), "10");

            // plus.AddVertex(smu.Get(@"DoubleOperator\RightExpression"), "30");

            plus.Value = "";

           // IVertex h = vx.AddVertex(null, "h");

         //  vx.AddEdge(zzz,plus);

          //  vx.AddVertex(zzz, "");

           // vx.AddEdge(yyy, plus);

              IVertex leftplus = VertexOperations.AddInstance(plus, smu.Get("-"), smu.Get(@"DoubleOperator\LeftExpression"));

            leftplus.Value = "";

              leftplus.AddVertex(smu.Get(@"DoubleOperator\LeftExpression"), "1");

              leftplus.AddVertex(smu.Get(@"DoubleOperator\RightExpression"), "2");

             IVertex rightplus = VertexOperations.AddInstance(plus, smu.Get("+"), smu.Get(@"DoubleOperator\RightExpression"));

            //rightplus.AddVertex(smb.Get(@"Vertex\$Description"), "plus opis tak");

            rightplus.Value = "";

            IVertex w3=rightplus.AddVertex(smu.Get(@"DoubleOperator\LeftExpression"), "3");
           // w3.AddVertex(smb.Get(@"Vertex\$Description"), "3 3 3");


            IVertex v4=rightplus.AddVertex(smu.Get(@"DoubleOperator\RightExpression"), "4");

              v4.AddVertex(smb.Get(@"Vertex\$Description"), "opis");


            
            

            //////////////////////////


            IVertex xXx = r.AddVertex(null, "XX");


            addf(xXx);


            //////////////////////////

            IVertex yv = r.AddVertex(null, "Y");


        }

        void addf(IVertex where)
        {        
            IVertex r=MinusZero.Instance.Root;

            IVertex smzt=r.Get(@"System\Meta\ZeroTypes");
            IVertex smu = r.Get(@"System\Meta\UML");
            IVertex smb = r.Get(@"System\Meta\Base");

            IVertex function_function = where.AddVertex(smu.Get(@"Function"), "Sleep");

            function_function.AddEdge(smb.Get(@"Vertex\$Is"), smu.Get("Function"));

            function_function.AddEdge(smu.Get(@"Function\Output"), smzt.Get("Integer"));

            IVertex ffi = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "time");

            ffi.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Integer"));

            //
            ffi.AddVertex(smb.Get(@"Vertex\$Description"), "this is opis");
            
            //function_function.AddVertex(smb.Get(@"Vertex\$Description"), "this is opis");
            //

            IVertex ffi2 = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "fastMode");

            ffi2.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Boolean"));

            IVertex ffi3 = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "interrupt");

            ffi3.AddEdge(smb.Get(@"Vertex\$VertexTarget"), smzt.Get("Integer"));


            ffi3.AddVertex(smb.Get(@"Vertex\$Description"), "inter opis");


            function_function.AddVertex(smu.Get(@"[]"), null);

            function_function.AddEdge(smb.Get(@"$NewLine"), smb.Get(@"$Empty"));
        }

        public void ShowContent(object obj)
        {
            _ShowContent(obj);
        }        

        protected LayoutAnchorable _ShowContent(object obj)
        {            
            LayoutAnchorable a = new LayoutAnchorable();                        

            if (obj is IPlatformClass)
            {
                IPlatformClass pc=(IPlatformClass)obj;

                if (pc.Vertex.Get(@"BaseEdge:\To:")!=null&&pc.Vertex.Get(@"BaseEdge:\To:").Value != null&&(!GeneralUtil.CompareStrings(pc.Vertex.Get(@"BaseEdge:\To:").Value,"")))
                    a.Title = pc.Vertex.Get(@"BaseEdge:\To:").Value.ToString();
                else
                    a.Title = (string)pc.Vertex.Value;

                PlatformClassSimpleWrapper pcsw = new PlatformClassSimpleWrapper();

                pcsw.SetContent(pc);

                a.Content = pcsw;

                a.IsVisibleChanged += pcsw.HideEventHandler;

                //a.Closing +=pcsw.ClosedEventHandler;
                a.Closed += pcsw.ClosedEventHandler;

                // not work - to focus
                //System.Windows.Input.Keyboard.Focus((IInputElement)pc);

                this.Pane.Children.Add(a);
                
                pcsw.IsIntialising = true;

                a.Hide(); // this works
                a.Show(); // for getting focus

                pcsw.IsIntialising = false;
            }else{
                a.Title = obj.ToString();
                a.Content = obj;

                this.Pane.Children.Add(a);

                a.Hide(); // this works
                a.Show(); // for getting focus
            }
            
            //a.AddToLayout(this.dockingManager, AnchorableShowStrategy.Most); 
            // maybe for later use

            return a;
        }

        public int DialogWindowDefaultWidth=300;
        public int DialogWindowDefaultHeight = 325;

        public void ShowContentFloating(object obj)
        {
            ShowContentFloating_withSize(obj, DialogWindowDefaultWidth, DialogWindowDefaultHeight);
        }

        public void ShowContentFloating_withSize(object obj, double DialogWindowWidth, double DialogWindowHeight){            
            LayoutAnchorable a=_ShowContent(obj);
            

            a.FloatingTop = this.Top + (this.Height / 2) - (Math.Min(this.Height,DialogWindowHeight) / 2);
            a.FloatingLeft = this.Left + (this.Width / 2) - (Math.Min(this.Width,DialogWindowWidth) / 2);

            a.FloatingWidth = DialogWindowWidth;
            a.FloatingHeight = DialogWindowHeight;            

            a.Float();
            
        }

        public void CloseWindowByContent(object obj)
        {
            LayoutContent layoutContent = dockingManager.Layout.ActiveContent;

            layoutContent.Close();
        }

        public void ShowInfo(string info)
        {
            m0.UIWpf.Dialog.Info i = new UIWpf.Dialog.Info();

            i.Owner = this;

            i.Text = info;

            i.ShowDialog();
        }

        public IVertex SelectDialog(IVertex info, IVertex options, Point? position)
        {
            SelectDialog d = new SelectDialog(info, options,position);

            return d.SelectedOption;
        }

        public IVertex SelectDialogButton(IVertex info, IVertex options, Point? position)
        {
            SelectDialogButton d = new SelectDialogButton(info, options, position);

            return d.SelectedOption;
        }

        public void EditDialog(IVertex baseVertex, Point? position)
        {
            ShowContentFloating_withSize( new EditDialog(baseVertex, position),500,550);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m0.MinusZero.Instance.Dispose();
        }
    }
}
