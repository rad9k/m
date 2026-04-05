using m0.Foundation;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.UIWpf.Visualisers
{
    public class IntegerVisualiser: NumberVisualiser<int?>, ITypedEdge
    {
        protected override string visualiserName { get { return "IntegerVisualiser"; } set { } }

        protected override IVertex visualiserMetaVertex { get { return MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Integer"); } set { } }

        // TypedEdge START

        public IntegerVisualiser(IEdge _edge) : base(null, null, false)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public IntegerVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile) : base(baseEdgeVertex, parentVisualiser, isVolatile)
        {

        }
    }
}
