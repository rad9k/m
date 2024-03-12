using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store.Json;
using m0.UIWpf.Visualisers.Helper;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace m0_SYSTEM_GENERATE
{
    class Program
    {
        public static void print(string text)
        {
            System.Console.Out.WriteLine(text);
        }

        static void StaticMetaInitialize()
        {
            //GraphChangeTrigger.Initialize();
            ExecutionFlowHelper.Initialize();
            //GraphChangeTransactionAtom.Initialize();
           // Transaction.Initialize();
           //AtomVisualiserHelper.Initialize();
        }

        static void MinusZeroInstanceFix() // fpr ZeroCodeView
        {
            MinusZero.Instance.StackFrameInherits = LegacySystem_MinusZero.Instance.StackFrameInherits;

            Transaction.GenericEventHandler_event_meta = LegacySystem_MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GenericEventHandler\event");

            GraphChangeTransactionAtom.GraphChangeEvent_Type_meta = LegacySystem_MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Type");
            GraphChangeTransactionAtom.GraphChangeEnum_MetaEdgeRemoved_meta = LegacySystem_MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\MetaEdgeRemoved");
            GraphChangeTransactionAtom.GraphChangeEvent_Edge_meta = LegacySystem_MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Edge");
        }

        static void Main(string[] args)
        {
            print("m0 SYSTEM GENERATE");
            print("version 1.0");
            print("SYSTEM / USER / EXAMPLES m0 files generator");

            print("");

            // print("* initializing legacy system");

            //m0.LegacySystem.LegacySystem a = new m0.LegacySystem.LegacySystem();

            // print("* legacy system initialized succesfully");


            //

            print("* filling System and User");

            LegacySystem_MinusZero.Instance.Initialize();

            print("* filling examples");

            //

            MinusZeroInstanceFix();

            ExecutionFlowHelper.StartTransaction();

            //

            CreateExamples.CreateTestData();            

            IVertex root = LegacySystem_MinusZero.Instance.Root;
            IVertex System = root.Get(false, "System");
            IVertex User = root.Get(false, "User");

            IVertex examples = root.Get(false, "examples");

            print("* saving System to \"system.m0\"");
            
            IVertex system = GeneralUtil.CreateM0AndMoveEdgesIntoIt(@"system.m0", System, 1);

            LegacySystem_MinusZero.Instance.AddFastAccessVertexes(); // after save need to update

            //



            print("* System saved to \"system.m0\"");

            //VertexDebugDB.EmitDB();
            //return;

            //

            IEnumerable<IVertex> systemSubGraphWithLinks = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(system);  

            //            

            Dictionary<string, StoreId> storeOverride = new Dictionary<string, StoreId>();

            storeOverride.Add("system.m0", new StoreId("m0.Store.MemoryStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "$-0$ROOT$STORE$"));

            //

            ExecutionFlowHelper.StartTransaction();

            //

            print("* saving User to \"user.m0\"");            

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("user.m0", User, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);            

            print("* User saved to \"user.m0\"");

            //

            print("* saving examples to \"examples.m0\"");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("examples.m0", examples, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* examples saved to \"examples.m0\"");

            //

            StaticMetaInitialize();

            print("* filling Lib::Std");

            Lib.CreateLib.CreateLibStd();

            print("* filling Lib::Sys");

            Lib.CreateLib.CreateLibSys();

            LegacySystem_MinusZero.Instance.DefaultFormalTextLanguageVertexSetup(); // system.m0 instead of $-0$ROOT$STORE$

            print("* filling Lib::Music");
            Music.CreateMusic.CreateLibMusic();


            Lib.CreateLib.Save(systemSubGraphWithLinks, storeOverride);

            print("* Lib::Std and Lib::Sys saved to \"lib_std.m0\" and \"lib_sys.m0\"");

            Music.CreateMusic.Save(systemSubGraphWithLinks, storeOverride);

            print("* Lib::Music saved \"lib_music.m0\"");
            //

            print("* creating \"_bootstrap.m0\"");

            CreateBootstrap.Create("_bootstrap.m0", true);

            //

            ExecutionFlowHelper.CommitTransaction();

            print("");

            print("execution succesfull finish");            
        }
    }
}
