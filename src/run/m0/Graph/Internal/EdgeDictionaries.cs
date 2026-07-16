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

        public OutList Out;
        public MetaInList MetaIn;
        public InList In;

        public IImplementedVertex Vertex;

        public EdgeDictionaries(IImplementedVertex _v)
        {
            Vertex = _v;

            Out = new OutList(this);
            MetaIn = new MetaInList(this);
            In = new InList(this);
        }
    }
}
