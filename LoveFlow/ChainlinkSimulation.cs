using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov
{
    public class ChainlinkSimulation
    {
        static IVertex r = m0.MinusZero.Instance.root;

        static public void Create()
        {
            IVertex lf = r.Get(false, "LovFlov");
        }
    }
}
