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
using System.Runtime.InteropServices;
using System.Windows.Interop;

using m0.UIWpf;
using m0.Graph;

using Xceed.Wpf.AvalonDock.Layout;
using m0.Foundation;

using m0.UIWpf.Visualisers;
using m0.ZeroTypes;
using m0.Util;
using Xceed.Wpf.AvalonDock.Controls;
using m0.UIWpf.Dialog;
using m0.Store;
using m0.Store.Json;

namespace m0
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class m0Main : Window, IUserInteraction
    {
        public static m0Main Instance;        

        public m0Main()
        {
            Instance = this;

            InitializeComponent();

            MinusZero.Instance.Initialize();


//            CreateTestData();
            

            TreeVisualiser stv = new TreeVisualiser();

            GraphUtil.ReplaceEdge(stv.Vertex.Get(false, "BaseEdge:"), "To", MinusZero.Instance.Root);                                    
            
            this.root.Content=stv;


            this.Loaded += new RoutedEventHandler(m0Main_Loaded);

            this_static = this;

        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // SYSTEM MENU BEG

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        #region Win32 API Stuff

        // Define the Win32 API methods we are going to use
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        /// Define our Constants we will use
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;

        #endregion

        // The constants we'll use to identify our custom system menu items
        public const Int32 _TransactionSysMenuID = 1000;
        public const Int32 _AboutSysMenuID = 1001;

        /// <summary>
        /// This is the Win32 Interop Handle for this Window
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        private void m0Main_Loaded(object sender, RoutedEventArgs e)
        {
            /// Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);            
            
            InsertMenu(systemMenuHandle, 0, MF_BYPOSITION, _TransactionSysMenuID, "Transactions");
            InsertMenu(systemMenuHandle, 1, MF_BYPOSITION, _AboutSysMenuID, "About");
            InsertMenu(systemMenuHandle, 2, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator

            // Attach our WndProc handler to this Window
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        static m0Main this_static;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == WM_SYSCOMMAND)
            {
                // Execute the appropriate code for the System Menu item that was clicked
                switch (wParam.ToInt32())
                {
                    case _TransactionSysMenuID:

                        m0.UIWpf.Forms.Transaction transaction = new UIWpf.Forms.Transaction(this_static);

                        handled = true;
                        break;
                    case _AboutSysMenuID:

                        m0.UIWpf.Forms.About about = new UIWpf.Forms.About(this_static);

                        handled = true;

                        break;
                }
            }

            return IntPtr.Zero;
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // SYSTEM MENU END

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!



        private string randomChars()
        {
            Random r = new Random();
            int x=r.Next(5);

            string xxx = "";

            for(int xx=0;xx<x;xx++)
                xxx+=xx;

            return xxx;
        }

        private void SerTest()
        {
            JsonSerializationStore s = new JsonSerializationStore(@"c:\m0\test1200", MinusZero.Instance, new AccessLevelEnum[] { });

           // SerTestCreate(s.Root);
            //SerTestSave(s);
        }

        void SerTestCreate(IVertex r)
        {
            IVertex xxx = MinusZero.Instance.Root;

            for (int x = 0; x < 1200; x++) {
                IVertex v = r.AddVertex(xxx, "KOHAM MAGDE");
                for (int xx = 0; xx < 1200; xx++)
                    v.AddVertex(xxx, "BARDZO KOHAM MAGDE");
                }
        }

        void SerTestSave(IStore s)
        {
            s.Detach();
            s.CommitTransaction();
            s.Attach();
        }

        private void queryTest(IVertex tr)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex meta_a = tr.AddVertex(null, "meta_a");
            IVertex meta_b = tr.AddVertex(null, "meta_b");

            IVertex s = tr.AddVertex(null, "s");

            IVertex a = tr.AddVertex(null, "a");
            s.AddEdge(null, a);
            s.AddEdge(meta_a, a);

            IVertex b = tr.AddVertex(null, "b");
            s.AddEdge(null, b);
            s.AddEdge(meta_b, b);
            s.AddEdge(meta_a, b);

            a.AddEdge(meta_a, b);

            b.AddEdge(r.Get(false, @"System\Meta*$Inherits"), a);

            IVertex re = ((EasyVertex)s).NewGetAll(false, @"meta_a|a\meta_a|b");
            re = ((EasyVertex)s).NewGetAll(false, "meta_b|a");
            re = ((EasyVertex)s).NewGetAll(false, "meta_a|");
            re = ((EasyVertex)s).NewGetAll(false, "meta_b|");
        }

        private void CreateTestData()
        {
            IVertex r=MinusZero.Instance.Root;

            //JsonSerializationStore jss = new JsonSerializationStore(@"c:\m0\x",MinusZero.Instance, new AccessLevelEnum[] { });

          //  IVertex tr = jss.Root;

            //MinusZero.Instance.Root.AddEdge(null, jss.Root);

            //return;

            IVertex tr = MinusZero.Instance.Root.AddVertex(null, "kupa");

            queryTest(tr);

            /*IEdge result;
            IList<IEdge> results;

            a.QueryInEdges(null, "s", out result, out results);
            a.QueryInEdges("meta_a", null, out result, out results);
            a.QueryInEdges("meta_a", "aa", out result, out results);
            a.QueryInEdges("meta_a", "s", out result, out results);
            a.QueryInEdges(null, "b", out result, out results);

            b.QueryInEdges(null, "aa", out result, out results);
            b.QueryInEdges("meta_a", null, out result, out results);
            b.QueryInEdges("meta_a", "s", out result, out results);
            b.QueryInEdges(null, "b", out result, out results);

            b.QueryInEdges(null, "s", out result, out results);
            b.QueryInEdges("meta_b", null, out result, out results);
            b.QueryInEdges("meta_b", "s", out result, out results);
            b.QueryInEdges(null, "b", out result, out results);
            */






            GeneralUtil.ParseAndExcute(tr, r.Get(false, @"System\Meta"), @"{TEST3{Class:Customer{},Class:Person{$Description:opis,Attribute:Name,Attribute:Surname,Attribute:DateOfBirth},Class:Company{Attribute:Name,Attribute:RegistrationNumber,},Class:Adress{Attribute:Line 1,Attribute:Line 2,Attribute:Line 3,Attribute:City,Attribute:County,Attribute:Postal code,Attribute:Country},Class:Basket{Attribute:Creation date,Attribute:Status},Class:Item{Attribute:Name,Attribute:Description,Attribute:Price}}}");


            tr.Get(false, @"TEST3\Customer").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));
            tr.Get(false, @"TEST3\Person").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));
            tr.Get(false, @"TEST3\Company").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));
            tr.Get(false, @"TEST3\Adress").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));
            tr.Get(false, @"TEST3\Basket").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));
            tr.Get(false, @"TEST3\Item").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));

            GeneralUtil.ParseAndExcute(tr, r.Get(false, @"System\Meta"), "{TEST2,TEST{Class:Person{Association:Spouse{$MaxCardinality:1,$MaxTargetCardinality:1},Aggregation:Child{$MaxCardinality:3},Attribute:Name,Attribute:Surname,Attribute:Age{MinValue:0,MaxValue:40},Attribute:NoseLength{MinValue:0,MaxValue:40},Attribute:Money{MinValue:0,MaxValue:1000},Attribute:IsGood,Attribute:IsPretty,Attribute:IsPretty2,Attribute:IsPretty3},Enum:Pretty{EnumValue:Yes,EnumValue:No,EnumValue:Maybe}}}");

            tr.Get(false, @"TEST\Pretty").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"System\Meta\ZeroTypes\EnumBase"));
            tr.Get(false, @"TEST\Person").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\UML\Class"));


            ///


    

            ///

            IVertex smzt=r.Get(false, @"System\Meta\ZeroTypes");

            IVertex EdgeTarget = r.Get(false, @"System\Meta*$EdgeTarget");

            IVertex Person = tr.Get(false, @"TEST\Person");



            //////////////



            IVertex smu = r.Get(false, @"System\Meta\UML");
            IVertex smb = r.Get(false, @"System\Meta\Base");


          IVertex function_function = Person.AddVertex(smu.Get(false, @"Function"), "Sleep");

            function_function.AddEdge(smu.Get(false, @"Function\Output"), smzt.Get(false, "Integer"));

            IVertex ffi = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "time");

            ffi.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));

            //
            ffi.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis time");
            function_function.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis function");
            //

            IVertex ffi2 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "fastMode");

            ffi2.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Boolean"));

            IVertex ffi3 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "interrupt");

            ffi3.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));


            function_function.AddVertex(smu.Get(false, @"[]"), null);

            function_function.AddEdge(smb.Get(false, @"$NewLine"), smb.Get(false, @"$Empty"));

            //

            

            Person.Get(false, "Name").AddEdge(EdgeTarget, smzt.Get(false, "String"));

            Person.Get(false, "Spouse").AddEdge(r.Get(false, @"System\Meta*$EdgeTarget"), Person);
            Person.Get(false, "Child").AddEdge(r.Get(false, @"System\Meta*$EdgeTarget"), Person);

            Person.Get(false, "Surname").AddEdge(EdgeTarget, smzt.Get(false, "String"));
            Person.Get(false, "Age").AddEdge(EdgeTarget, smzt.Get(false, "Integer"));
            Person.Get(false, "NoseLength").AddEdge(EdgeTarget, smzt.Get(false, "Float"));
            Person.Get(false, "Money").AddEdge(EdgeTarget, smzt.Get(false, "Decimal"));
            Person.Get(false, "IsGood").AddEdge(EdgeTarget, smzt.Get(false, "Boolean"));
            Person.Get(false, "IsPretty").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));
            Person.Get(false, "IsPretty2").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));
            Person.Get(false, "IsPretty3").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));

            
            GeneralUtil.ParseAndExcute(tr.Get(false, "TEST"), tr.Get(false, @"TEST"), "{Person:Person1{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");

            GeneralUtil.ParseAndExcute(tr.Get(false, "TEST"), tr.Get(false, @"TEST"), "{Person:Person3{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person4{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");
            
            tr.Get(false, @"TEST\Person1").AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
            tr.Get(false, @"TEST\Person2").AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
            tr.Get(false, @"TEST\Person3").AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
            tr.Get(false, @"TEST\Person4").AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));

            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person1"), "IsPretty", tr.Get(false, @"TEST\Pretty\No"));
            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person2"), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person3"), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person4"), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
                

            for (int x = 0; x < 1; x++)
            {
                GeneralUtil.ParseAndExcute(tr.Get(false, "TEST2"), tr.Get(false, @"TEST"), "{Person:Person1"+x+"{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2"+x+"{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");
                
              
                GeneralUtil.ParseAndExcute(tr.Get(false, "TEST2"), tr.Get(false, @"TEST"), "{Person:Person3"+x+"{Name:Magda,Surname:Tereszczuk,Age:18,NoseLength:\"2,1\",Money:999,IsGood:True,IsPretty:},Person:Person4"+x+"{Name:Jan,Surname:Kuciak,Age:10,NoseLength:0.6,Money:99999,IsGood:True,IsPretty:}}");

                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person1"+x), "IsPretty", tr.Get(false, @"TEST\Pretty\No"));
                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person2"+x), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person3"+x), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person4"+x), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));

                tr.Get(false, @"TEST2\Person1"+x+@"\Radek").AddEdge(r.Get(false, @"System\Meta*$Is"), smzt.Get(false, "String"));


                tr.Get(false, @"TEST2\Person1"+x).AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
                tr.Get(false, @"TEST2\Person2"+x).AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
                tr.Get(false, @"TEST2\Person3"+x).AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
                tr.Get(false, @"TEST2\Person4"+x).AddEdge(r.Get(false, @"System\Meta*$Is"), tr.Get(false, @"TEST\Person"));
            }

            for (int x = 0; x < 1; x++)
                for (int y = 0; y < 1; y++)
                {
                tr.Get(false, @"TEST2\Person1" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person1" + y));
                tr.Get(false, @"TEST2\Person2" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person2" + y));
                tr.Get(false, @"TEST2\Person3" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person3" + y));
                tr.Get(false, @"TEST2\Person4" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person4" + y));
            }

            for (int i = 1; i <= 1; i++)
            {
                IVertex x=tr.Get(false, "TEST2").AddVertex(null, i);

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
            
            GeneralUtil.ParseAndExcute(tr.Get(false, "TEST"), tr.Get(false, @"System\Meta"), "{Diagram:TestDiagram{ZoomVisualiserContent:100,SelectedEdges:,CreationPool:}}");

            tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta\Visualiser\Diagram\SizeX"), 600.0);

            tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta\Visualiser\Diagram\SizeY"), 600.0);

            tr.Get(false, @"TEST\TestDiagram").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta*Diagram"));
            
            IVertex i1=tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta*Item"),null);
            
            GeneralUtil.ParseAndExcute(i1,r.Get(false, @"System\Meta"),"{PositionX:0,PositionY:0,SizeX:100,SizeY:100}");

            IVertex i2 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i2, r.Get(false, @"System\Meta"), "{PositionX:200,PositionY:200,SizeX:100,SizeY:100}");

            i1.AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i1.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i1, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person1"));

            i2.AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i2, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person2"));

            



            i1 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i1, r.Get(false, @"System\Meta"), "{PositionX:350,PositionY:0}");

            i2 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i2, r.Get(false, @"System\Meta"), "{PositionX:0,PositionY:350}");

            i1.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            i1.AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            Edge.AddEdgeByToVertex(i1, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person3"));

            i2.AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i2, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person4"));

            /////////////////////

           /* GeneralUtil.ParseAndExcute(r.Get(false, "TEST"), r.Get(false, @"System\Meta"), "{Class:X1,Class:X2,Class:X3,Class:X4,Class:PersonA,Class:PersonB,Class:PersonB2{Attribute:New}}");

            r.Get(false, @"TEST\PersonB2\New").AddEdge(r.Get(false, @"System\Meta*$EdgeTarget"), r.Get(false, @"System\Meta*String"));

            VertexOperations.AddInstance(r.Get(false, "TEST"), r.Get(false, @"TEST\PersonB2"), r.Get(false, @"TEST\Person")).Value="XXX";

            r.Get(false, @"TEST\X2").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\Person"));
            r.Get(false, @"TEST\X3").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\X2"));
            r.Get(false, @"TEST\X4").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\X3"));

            r.Get(false, @"TEST\PersonA").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\Person"));
            r.Get(false, @"TEST\PersonB").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\Person"));
            r.Get(false, @"TEST\PersonB2").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\PersonB"));

            r.Get(false, @"TEST\XXX").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"TEST\PersonA"));
            r.Get(false, @"TEST\XXX").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"TEST\X4"));*/

            //////////////////////

            IVertex tt = tr.Get(false, "TEST").AddVertex(r.Get(false, "System*Class"), "TestClass");

            for(int x=0;x<1;x++)
                for (int y = 0; y < 1; y++)
                {
                    IVertex tta = tt.AddVertex(r.Get(false, "System*Attribute"), "a" + x + " " + y + ";" + randomChars());
                    tta.AddVertex(r.Get(false, "System*$Group"), x.ToString());
                    tta.AddVertex(r.Get(false, "System*$Section"), y.ToString());

                    tta.AddEdge(r.Get(false, "System*$EdgeTarget"), r.Get(false, "System*String"));

                    IVertex ttb = tt.AddVertex(r.Get(false, "System*Attribute"), "b" + x + " " + y + ";" + randomChars());
                    ttb.AddVertex(r.Get(false, "System*$Group"), x.ToString());
                    //ttb.AddVertex(r.Get(false, "System*$Section"), y.ToString());
                    ttb.AddEdge(r.Get(false, "System*$EdgeTarget"), r.Get(false, "System*String"));

                    IVertex ttc = tt.AddVertex(r.Get(false, "System*Attribute"), "c" + x + " " + y + ";" + randomChars());
                    ttc.AddVertex(r.Get(false, "System*$Group"), x.ToString());
                    ttc.AddVertex(r.Get(false, "System*$Section"), y.ToString());
                    ttc.AddVertex(r.Get(false, "System*$MaxCardinality"), 6);
                    ttc.AddEdge(r.Get(false, "System*$EdgeTarget"), r.Get(false, "System*String"));
                }

            VertexOperations.AddInstance(tr.Get(false, "TEST"), tt);

            //////////////////////


            IVertex start = tr.Get(false, @"TEST3");

            for (int i = 0; i < 1; i++) {
                IVertex sm = start.AddVertex(r.Get(false, @"System\Meta\UML\StateMachine"), "sm "+i);

                for (int ii = 0; ii < 1; ii++)
                    sm.AddVertex(r.Get(false, @"System\Meta\UML\StateMachine\State"), "state "+ii+" of machine"+i);

                IVertex allstates = sm.GetAll(false, "");

                foreach (IEdge e in allstates)
                    foreach (IEdge ee in allstates)
                        e.To.AddEdge(r.Get(false, @"System\Meta\UML\StateMachine\State\Transition"), ee.To);
            }

            //////////////////////

            IVertex associations = tr.GetAll(false, @"TEST\Person\Association:");
            IVertex ismeta = r.Get(false, @"System\Meta*$Is");
            IVertex asmeta = r.Get(false, @"System\Meta\UML\Class\Association");

            //foreach (IEdge v in associations)
             //   v.To.AddEdge(ismeta, asmeta);
            
            IVertex attributes = tr.GetAll(false, @"TEST\Person\Attribute:");
            //IVertex ismeta=r.Get(false, @"System\Meta*$Is");
            IVertex ameta=r.Get(false, @"System\Meta\UML\Class\Attribute");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            attributes = tr.GetAll(false, @"TEST3\\Attribute:");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            IVertex test = tr.Get(false, "TEST");

            test.AddVertex(test.AddVertex(null, "Counter"),(int)0);


            IVertex vvv = VertexOperations.AddInstance(test, r.Get(false, @"System\Meta\Base\$Import"));

            vvv.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$IsLink"), MinusZero.Instance.Empty);

            vvv.Value="tst";

            test.AddEdge(tr.Get(false, @"TEST\tst"), r.Get(false, @"System\Meta\Visualiser"));

            /////

            IVertex aattributes = tr.GetAll(false, @"TEST\\Attribute:");

            IVertex isAggregation = r.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = r.Get(false, @"System\Meta\Base\$Empty");

            foreach (IEdge v in aattributes)
                v.To.AddEdge(isAggregation, empty);

            IVertex aggregations = tr.GetAll(false, @"TEST\\Aggregation:");

            foreach (IEdge v in aggregations)
                v.To.AddEdge(isAggregation, empty);

            ///
            
            IVertex vx=tr.AddVertex(null, "X");

            IVertex my = vx.AddVertex(null, "j e s ");

            IVertex vxx = vx.AddVertex(null, "VXX");

            IVertex c = VertexOperations.AddInstance(vxx, smu.Get(false, "[]"));
            
            c.Value="";

            c.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "raz");

            IVertex dwa=c.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "dwa");

            //dwa.AddVertex(smb.Get(false, @"Vertex\$Description"), "3 3 3");

            c.AddEdge(smu.Get(false, @"MultiOperator\Expression"), my);

            IVertex cztery=c.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "cztery");

            //cztery.AddVertex(smb.Get(false, @"Vertex\$Description"), "cztery 1");

            //cztery.AddVertex(smb.Get(false, @"Vertex\$Description"), "cztery 2");


            // IVertex zzz = vx.AddVertex(null, "raz");

            //   IVertex yyy = vx.AddVertex(null, "dwa");

            //  IVertex wh = VertexOperations.AddInstance(vx, smu.Get(false, @"While"));


            // IVertex plus = VertexOperations.AddInstance(wh, smu.Get(false, "-"), smu.Get(false, @"While\Test"));

            IVertex plus = VertexOperations.AddInstance(c, smu.Get(false, "+"), smu.Get(false, @"MultiOperator\Expression"));

            //plus.AddVertex(smb.Get(false, @"Vertex\$Description"), "plus opis MAIN");

            //plus.AddVertex(null, "KUPA");

            //  plus.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "10");

            // plus.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "30");

            plus.Value = "";

           // IVertex h = vx.AddVertex(null, "h");

         //  vx.AddEdge(zzz,plus);

          //  vx.AddVertex(zzz, "");

           // vx.AddEdge(yyy, plus);

              IVertex leftplus = VertexOperations.AddInstance(plus, smu.Get(false, "-"), smu.Get(false, @"DoubleOperator\LeftExpression"));

            leftplus.Value = "";

              leftplus.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "1");

              leftplus.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "2");

             IVertex rightplus = VertexOperations.AddInstance(plus, smu.Get(false, "+"), smu.Get(false, @"DoubleOperator\RightExpression"));

            //rightplus.AddVertex(smb.Get(false, @"Vertex\$Description"), "plus opis tak");

            rightplus.Value = "";

            IVertex w3=rightplus.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "3");
           // w3.AddVertex(smb.Get(false, @"Vertex\$Description"), "3 3 3");


            IVertex v4=rightplus.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "4");

              v4.AddVertex(smb.Get(false, @"Vertex\$Description"), "opis");


            
            

            //////////////////////////


            IVertex xXx = tr.AddVertex(null, "XX");


            addf(xXx);


            //////////////////////////

            IVertex yv = tr.AddVertex(null, "Y");

            /////////////////

           // jss.Detach();
            //jss.CommitTransaction();
           // jss.Attach();

        }

        void addf(IVertex where)
        {        
            IVertex r=MinusZero.Instance.Root;

            IVertex smzt=r.Get(false, @"System\Meta\ZeroTypes");
            IVertex smu = r.Get(false, @"System\Meta\UML");
            IVertex smb = r.Get(false, @"System\Meta\Base");

            IVertex function_function = where.AddVertex(smu.Get(false, @"Function"), "Sleep");

            function_function.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Function"));

            function_function.AddEdge(smu.Get(false, @"Function\Output"), smzt.Get(false, "Integer"));

            IVertex ffi = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "time");

            ffi.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));

            //
            ffi.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis");
            
            //function_function.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis");
            //

            IVertex ffi2 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "fastMode");

            ffi2.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Boolean"));

            IVertex ffi3 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "interrupt");

            ffi3.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));


            ffi3.AddVertex(smb.Get(false, @"Vertex\$Description"), "inter opis");


            function_function.AddVertex(smu.Get(false, @"[]"), null);

            function_function.AddEdge(smb.Get(false, @"$NewLine"), smb.Get(false, @"$Empty"));
        }

        public void ShowContent(object obj)
        {
            _ShowContent(obj);
        }        

        protected LayoutAnchorable _ShowContent(object obj)
        {            
            LayoutAnchorable a = new LayoutAnchorable();

            a.CanClose = true;

            if (obj is IPlatformClass)
            {
                IPlatformClass pc=(IPlatformClass)obj;

                if (pc.Vertex.Get(false, @"BaseEdge:\To:")!=null&&pc.Vertex.Get(false, @"BaseEdge:\To:").Value != null&&(!GeneralUtil.CompareStrings(pc.Vertex.Get(false, @"BaseEdge:\To:").Value,"")))
                    a.Title = pc.Vertex.Get(false, @"BaseEdge:\To:").Value.ToString();
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

        public void ShowException(IVertex exception)
        {
            m0.UIWpf.Dialog.Info i = new UIWpf.Dialog.Info();

            i.Owner = this;

            string toShow = "";

            if (exception.Get(false, "Type:") != null)
                toShow += "type: "+exception.Get(false, "Type:")+"\n\n";

            if (exception.Get(false, "Where:")!=null)
                toShow += "where: "+exception.Get(false, "Where:") + "\n\n";

            if (exception.Get(false, "What:") != null)
                toShow += "what: "+exception.Get(false, "What:");

            i.Text = toShow;

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
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m0.MinusZero.Instance.Dispose();
        }
    }
}
