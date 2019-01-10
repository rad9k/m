using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Store.FileSystem
{    
    public class DriveVertex : DirectoryVertex
    {
        public DriveVertex(string identifier, IStore store) : base (identifier, store)
        {
            IVertex pathSymbolVertex = m0.MinusZero.Instance.CreateTempVertex();

            pathSymbolVertex.Value = "\\";

            AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem\Drive\PathSeparator"), pathSymbolVertex);
            
        }
    }
}
