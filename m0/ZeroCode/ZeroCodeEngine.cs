using m0.TextLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

namespace m0.ZeroCode
{
    public class ZeroCodeEngine : IZeroCodeGraph2String, IParser
    {
        public IVertex Parse(IVertex rootVertex, string text)
        {             
            String2ZeroCodeGraphProcessing p = new String2ZeroCodeGraphProcessing();

            return p.Process(rootVertex, text); 
        }

        public string ZeroCodeGraph2String(IEdge graphBaseEdge)
        {
            ZeroCodeGraph2StringProcessing p = new ZeroCodeGraph2StringProcessing();

            return p.Process(graphBaseEdge);

        }
    }
}
