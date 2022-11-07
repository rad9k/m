using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class UXAggregator: UXItem
    {
        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\CollapsedSize");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");

        public UXAggregator(IEdge edge) : base(edge) { }

        public bool IsExpanded
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    val = Vertex.AddVertex(IsExpanded_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.Size ExpandedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size ExpandedSizeCreate()
        {
            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, ExpandedSize_meta));
        }

        public UX.Size CollapsedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "CollapsedSize", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size CollapsedSizeCreate()
        {
            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, CollapsedSize_meta));
        }
    }

}
