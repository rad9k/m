using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;

namespace m0.UIWpf.Visualisers
{
    public class DecimalVisualiser : NumberVisualiser<decimal?>, ITypedEdge
    {
        protected override string visualiserName { get { return "DecimalVisualiser"; } set { } }

        protected override IVertex visualiserMetaVertex { get { return MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Decimal"); } set { } }

        // TypedEdge START

        public DecimalVisualiser(IEdge _edge) : base(null, null, false)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public DecimalVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile) : base(baseEdgeVertex, parentVisualiser, isVolatile)
        {

        }
    }
}

