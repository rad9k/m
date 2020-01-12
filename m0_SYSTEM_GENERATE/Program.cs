using m0;
using m0.Foundation;
using m0.Store.Json;
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

            print("* filling System");
            print("* saving System to \"system.m0\"");

            JsonSerializationStore s = new JsonSerializationStore(@"system.m0", MinusZero.Instance, new AccessLevelEnum[] { });

            print("* System saved to \"system.m0\"");

            print("* filling User");
            print("* saving User to \"user.m0\"");
            print("* User saved to \"user.m0\"");

            print("* filling examples");
            print("* saving examples to \"examples.m0\"");
            print("* examples saved to \"examples.m0\"");

            print("");

            print("execution succesfull finish");
            
            //System.Console.ReadKey();
        }
    }
}
