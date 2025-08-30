using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store.Json;
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
            print("-zero SYSTEM GENERATE");
            print("version 1.0");
            print("SYSTEM / USER / EXAMPLES m0j files generator");

            print("");

            // print("* initializing legacy system");

            //m0.LegacySystem.LegacySystem a = new m0.LegacySystem.LegacySystem();

            // print("* legacy system initialized succesfully");


            //

            print("* filling System, User and Quick");

            LegacySystem_MinusZero.Instance.Initialize();

            print("* filling examples");

            //

            MinusZeroInstanceFix();

            ExecutionFlowHelper.StartTransaction();

            //

            CreateExamples.CreateTestData();

            IVertex root = LegacySystem_MinusZero.Instance.Root;
            IVertex SystemVertex = root.Get(false, "System");
            IVertex User = root.Get(false, "User");
            IVertex Quick = root.Get(false, "Quick");

            IVertex examples = root.Get(false, "examples");

            print("* saving System to \"system.m0j\"");

            IVertex system = GeneralUtil.CreateM0JAndMoveEdgesIntoIt(@"system.m0j", SystemVertex, 1);

            LegacySystem_MinusZero.Instance.AddFastAccessVertexes(); // after save need to update

            //



            print("* System saved to \"system.m0j\"");

            //VertexDebugDB.EmitDB();
            //return;

            //

            IEnumerable<IVertex> systemSubGraphWithLinks = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(system);

            //            

            Dictionary<string, StoreId> storeOverride = new Dictionary<string, StoreId>();

            storeOverride.Add("system.m0j", new StoreId("m0.Store.MemoryStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "$-0$ROOT$STORE$"));


            //

            ExecutionFlowHelper.StartTransaction();

            //

            print("* saving User to \"user.m0j\"");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("user.m0j", User, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* User saved to \"user.m0j\"");

            //

            print("* saving Quick to \"quick.m0j\"");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("quick.m0j", Quick, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* Quick saved to \"quick.m0j\"");

            //

            print("* saving examples to \"examples.m0j\"");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("examples.m0j", examples, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* examples saved to \"examples.m0j\"");

            //

            // system.m0 instead of $-0$ROOT$STORE$ BEG

            LegacySystem_MinusZero.Instance.DefaultFormalTextLanguageVertexSetup();
            LegacySystem_MinusZero.Instance.Init_AfterZeroCodeDefintionCreated();

            // system.m0 instead of $-0$ROOT$STORE$ END

            StaticMetaInitialize();

            print("* filling Lib::Std");

            Lib.CreateLib.CreateLibStd();

            print("* filling Lib::Sys");

            Lib.CreateLib.CreateLibSys();

            print("* filling Lib::StdUI");

            Lib.CreateLib.CreateLibStdUI();

            print("* filling Lib::StdView");

            Lib.CreateLib.CreateLibStdView();

            print("* filling Lib::Net");

            Lib.CreateLib.CreateLibNet();

            print("* filling Lib::Music");
            Music.CreateMusic.CreateLibMusic();


            Lib.CreateLib.Save(systemSubGraphWithLinks, storeOverride);

            print("* Lib::Std and Lib::Sys saved to \"lib_std.m0j\" and \"lib_sys.m0j\"");

            Music.CreateMusic.Save(systemSubGraphWithLinks, storeOverride);

            print("* Lib::Music saved \"lib_music.m0j\"");
            //

            print("* creating \"_bootstrap.m0j\"");

            CreateBootstrap.Create("_bootstrap.m0j", true);

            //

            ExecutionFlowHelper.CommitTransaction();

            print("");

            print("execution succesfull finish");

            if (System.IO.Directory.Exists(@"c:\Users\rad9k"))
                System.Diagnostics.Process.Start(@"c:\Users\rad9k\source\repos\m\generate\m0j_copy_rad9k.bat");

            if (System.IO.Directory.Exists(@"c:\Users\radoslaw.tereszczuk"))
                System.Diagnostics.Process.Start(@"c:\Users\radoslaw.tereszczuk\source\repos\m\generate\m0j_copy_radoslaw.bat");

            if (System.IO.Directory.Exists(@"c:\Users\teres"))
                System.Diagnostics.Process.Start(@"c:\Users\teres\source\repos\m\generate\m0j_copy_teres.bat");
        }
    }
}
