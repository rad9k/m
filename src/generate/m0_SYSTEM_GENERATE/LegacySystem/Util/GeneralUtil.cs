using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace m0.LegacySystem.Util
{
    public class GeneralUtil
    {
        public static IVertex ParseAndExcute(IVertex baseVertex, IVertex inputVertex, string expressionAsString)
        {
            MinusZero z = MinusZero.Instance;

            IVertex expressionAsVertex = MinusZero.Instance.CreateTempVertex();

            //z.DefaultParser.Parse(expressionAsVertex, expressionAsString);
            IEdge edge;

            LegacySystem.ZeroCodeEngine_OLD.Parse(new EdgeBase(null, null, expressionAsVertex), expressionAsString, m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out edge);

            return LegacySystem.ZeroCodeEngine_OLD.OldStyleExecute(baseVertex, inputVertex, expressionAsVertex);
        }
    }
}
