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
            print("m0 SYSTEM GENERATE" +
                "version 0.5 " +
                "SYSTEM / USER / EXAMPLES m0 files generator");

            print("");

            print("initializing legacy system");

            m0.LegacySystem.LegacySystem a = new m0.LegacySystem.LegacySystem();

            print("legacy system initialized succesfully");
        }
    }
}
