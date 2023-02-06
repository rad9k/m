using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class UXContainer : UXItem, IUXContainer
    {
        public Canvas Canvas { get; }

        //

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeQuery");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");

        public UXContainer(IEdge edge) : base(edge) { }

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
            IEdge expectedSizeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

            if (expectedSizeEdge != null)
                Vertex.DeleteEdge(expectedSizeEdge);

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

        public string ContainerEdgeQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    return "";

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ContainerEdgeQuery_meta, value);
                else
                    val.Value = value;
            }
        }
    }

}
