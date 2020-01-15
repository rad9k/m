using m0;
using m0.Foundation;
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

            print("* saving System to \"system.m0\"");

            IVertex root= LegacySystem_MinusZero.Instance.Root;

            GeneralUtil.CreateM0AndMoveEdgesIntoIt(@"system.m0", root.Get(false, "System"));

            print("* System saved to \"system.m0\"");

            //

            print("* saving User to \"user.m0\"");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt("user.m0", root.Get(false, "User"));

            print("* User saved to \"user.m0\"");

            //

            print("* filling examples");

            CreateExamples.CreateTestData();

            print("* saving examples to \"examples.m0\"");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt("example.m0", root.Get(false, "examples"));

            print("* examples saved to \"examples.m0\"");

            print("");

            print("execution succesfull finish");
            
            //System.Console.ReadKey();
        }
    }
}
