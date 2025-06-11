using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0;

namespace m0_desktop
{
    public class Main
    {
        public static void Run()
        {
            m0Main m = new m0Main();

            m0_COMPOSER.Runtime.Initialisation.Execute();

            m.Show();            
        }
    }
}
