using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Bootstrap
{
    public class CommandLineExecution
    {
        static IVertex root = MinusZero.Instance.root;
        static IVertex file_meta = root.Get(false, @"System\");
        public static void RunUser(string username)
        {

        }

        public static void CreateUser(string username) 
        { 
        
        }
    }
}
