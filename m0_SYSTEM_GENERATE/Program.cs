using m0;
using m0.Foundation;
using m0.Graph;
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

        static void Main(string[] args)
        {
            print("m0 SYSTEM GENERATE");
            print("version 0.5 ");
            print("SYSTEM / USER / EXAMPLES m0 files generator");

            print("");

            print("* initializing legacy system");

            m0.LegacySystem.LegacySystem a = new m0.LegacySystem.LegacySystem();

            print("* legacy system initialized succesfully");


            //

            print("* filling System and User");

            LegacySystem_MinusZero.Instance.Initialize();

            print("* filling examples");

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

            DebugDB.EmitDB();
            return;

            //

            List<IVertex> systemSubGraphWithLinks = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(system);
            
            //

            Lib.CreateLib.Create();
            Music.CreateMusic.Create();            

            //            

            Dictionary<string, StoreId> storeOverride = new Dictionary<string, StoreId>();

            storeOverride.Add("system.m0", new StoreId("m0.Store.MemoryStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "$-0$ROOT$STORE$"));

            //

            print("* saving User to \"user.m0\"");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("user.m0", User, systemSubGraphWithLinks, storeOverride);

            print("* User saved to \"user.m0\"");

            //

            print("* saving examples to \"examples.m0\"");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("examples.m0", examples, systemSubGraphWithLinks, storeOverride);

            print("* examples saved to \"examples.m0\"");

            //


            Lib.CreateLib.Save(systemSubGraphWithLinks, storeOverride);

            Music.CreateMusic.Save(systemSubGraphWithLinks, storeOverride);
                        

            //

            print("* creating \"_bootstrap.m0\"");

            CreateBootstrap.Create("_bootstrap.m0", true);
                       
            //

            print("");

            print("execution succesfull finish");            
        }
    }
}
