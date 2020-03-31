using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_SYSTEM_GENERATE
{
    public class CreateLibStd
    {
        public static IVertex Create()
        {
            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            IVertex std = lib.AddVertex(null, "Std");

            std.AddVertex(null, "siema");

            return std;
        }
    }
}
