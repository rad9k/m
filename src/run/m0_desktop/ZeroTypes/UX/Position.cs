using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0.ZeroTypes.UX
{
    public class Position: TypedEdge
    {
        static IVertex X_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Position\X");
        static IVertex Y_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Position\Y");

        public Position(IEdge edge) : base(edge) { }

        public Point GetPoint()
        {
            return new Point(X, Y);
        }

        public double X
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "X", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "X", null);

                if (val == null)
                    val = Vertex.AddVertex(X_meta, value);
                else
                    val.Value = value;
            }
        }

        public double Y
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Y", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Y", null);

                if (val == null)
                    val = Vertex.AddVertex(Y_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
