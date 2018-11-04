using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jil;

namespace m0.Store.Json
{
    class test
    {
        public string name { get; set; }

        public object o { get; set; }
    }


    class JsonSerializationStore
    {
        public static void test()
        {
            List<test> l = new List<test>();

            test t = new test();
            t.name = "raz";
            t.o = "Magda";

            l.Add(t);

            t = new test();
            t.name = "raz";
            t.o = 100;

            l.Add(t);

            t = new test();
            t.name = "raz";
            t.o = 100.0;

            l.Add(t);

            t = new test();
            t.name = "raz";
            t.o = 100.1;

            l.Add(t);

            var output = new StreamWriter("xxx");

            JSON.SerializeDynamic(l, output);

            output.Close();
            
        }
    }
}
