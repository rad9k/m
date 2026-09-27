using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;

// this one is a bit of trash

namespace m0_SYSTEM_GENERATE
{
    public class CreateDocumentationDiagrams
    {
        
        public static void Create()
        {            
            IVertex r = MinusZero.Instance.Root;
            
            IVertex dd = MinusZero.Instance.Root.AddVertex(null, "DocumentationDiagrams");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\DocumentationDiagrams\DocumentationDiagrams.txt", dd);

        }
    }
}
