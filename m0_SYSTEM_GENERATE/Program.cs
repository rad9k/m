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
        static void print(string text)
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

            IVertex root = LegacySystem_MinusZero.Instance.Root;
            IVertex System = root.Get(false, "System");
            IVertex User = root.Get(false, "User");

            print("* saving System to \"system.m0\"");
            
            GeneralUtil.CreateM0AndMoveEdgesIntoIt(@"system.m0", System);

            print("* System saved to \"system.m0\"");

            //

            print("* saving User to \"user.m0\"");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_LeaveVertexesFromList("user.m0", User, GraphUtil.GetSubGraphAsList(System) );

            print("* User saved to \"user.m0\"");

            //

            print("* filling examples");

            CreateExamples.CreateTestData();

            print("* saving examples to \"examples.m0\"");

            IVertex examples = root.Get(false, "examples");            
            GeneralUtil.CreateM0AndMoveEdgesIntoIt_LeaveVertexesFromList("examples.m0", User, GraphUtil.GetSubGraphAsList(examples));

            print("* examples saved to \"examples.m0\"");

            print("");

            print("execution succesfull finish");
            
            //System.Console.ReadKey();
        }
    }
}
