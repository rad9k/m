using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 
    From >----Meta-----> To
               +
               +
               --------> Meta


From:                 To
OutEdgesRaw           InEdgesRaw

                      Meta
	                  MetaInEdgesRaw


>From.AddEdge:

	+From.OutEdgesRaw
	+To.InEdgesRaw
	+Meta.MetaInEdgesRaw


>From.DeleteEdge

	-From.OutEdgesRaw
	-To.InEdgesRaw
	-Meta.MetaInEdgesRaw
     
*/


namespace m0.Graph.Internal
{
    [Serializable]
    public class EdgeDictionaries
    {
        public bool NoInEdgeInOutVertexVertexMode = false;

        private OutList outEdges;

        public OutList Out
        {
            get
            {
                return outEdges ??=
                    new OutList(this);
            }
        }

        internal int OutCount =>
            outEdges?.Count ?? 0;

        private MetaInList metaIn;
        private InList inEdges;

        public IList<IEdge> MetaIn
        {
            get
            {
                return metaIn ??=
                    new MetaInList(this);
            }
        }

        public IList<IEdge> In
        {
            get
            {
                return inEdges ??=
                    new InList(this);
            }
        }

        internal int MetaInCount =>
            metaIn?.Count ?? 0;

        internal int InCount =>
            inEdges?.Count ?? 0;

        public IImplementedVertex Vertex;

        public EdgeDictionaries(
            IImplementedVertex _v,
            bool noIncomingEdgeStorage = false)
        {
            Vertex = _v;
            NoInEdgeInOutVertexVertexMode =
                noIncomingEdgeStorage;
        }
    }
}
