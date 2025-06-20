using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class Size: TypedEdge
    {
        static IVertex Width_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size\Width");
        static IVertex Height_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size\Height");

        public Size(IEdge edge) : base(edge) { }

        public double Width
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Width", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Width", null);

                if (val == null)
                    val = Vertex.AddVertex(Width_meta, value);
                else
                    val.Value = value;
            }
        }

        public double Height
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Height", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Height", null);

                if (val == null)
                    val = Vertex.AddVertex(Width_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
