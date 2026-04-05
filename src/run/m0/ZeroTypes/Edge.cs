using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public class Edge:TypedEdge, IEdge
    {
        public static IVertex Edge_type = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge");

        public static IVertex From_meta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\From");
        public static IVertex Meta_meta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\Meta");
        public static IVertex To_meta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\To");

        public Edge(IEdge edge) : base(edge) { }

        public IVertex From
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "From", null);

                return val;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, From_meta, value);
            }
        }

        public IVertex Meta
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Meta", null);

                return val;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, Meta_meta, value);                
            }
        }

        public IVertex To
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "To", null);

                return val;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, To_meta, value);
            }
        }

        public bool EdgeRemovalExecuting { get; set; }
    }
}
