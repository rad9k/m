using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib
{
    public class LibUtil
    {
        public static int GetIntFromVertex(IVertex baseVertex, string name, ref bool isNull)
        {
            IVertex v = GraphUtil.GetQueryOutFirst(baseVertex, name, null);

            return GraphUtil.GetIntegerValue(v, ref isNull);
        }

        public static decimal GetDecimalFromVertex(IVertex baseVertex, string name, ref bool isNull)
        {
            IVertex v = GraphUtil.GetQueryOutFirst(baseVertex, name, null);

            return GraphUtil.GetDecimalValue(v, ref isNull);
        }

        public static double GetDoubleFromVertex(IVertex baseVertex, string name, ref bool isNull)
        {
            IVertex v = GraphUtil.GetQueryOutFirst(baseVertex, name, null);

            return GraphUtil.GetDoubleValue(v, ref isNull);
        }
    }
}
