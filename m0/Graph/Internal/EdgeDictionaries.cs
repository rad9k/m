using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.Internal
{
    public class EdgeDictionaries
    {
        public bool NoInEdgeInOutVertexVertexMode = false;

        public OutList Out;
        public ExtandableList<IEdge> In;
        public ExtandableList<IEdge> MetaIn;

        public IInternalCollectionsVertex v;

        public EdgeDictionaries(IInternalCollectionsVertex _v)
        {
            v = _v;


        }
    }
}
