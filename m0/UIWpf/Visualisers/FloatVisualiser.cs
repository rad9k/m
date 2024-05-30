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
using m0.UIWpf.Commands;
using m0.UIWpf.Foundation;

namespace m0.UIWpf.Visualisers
{
    public class FloatVisualiser : NumberVisualiser<double?>, ITypedEdge
    {
        protected override string visualiserName { get { return "FloatVisualiser"; } set { } }

        protected override IVertex visualiserMetaVertex { get { return MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Float"); } set { } }

        // TypedEdge START

        public FloatVisualiser(IEdge _edge) : base(null, null, false)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END
        public FloatVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile) : base(baseEdgeVertex, parentVisualiser, isVolatile)
        {

        }
    }
}


