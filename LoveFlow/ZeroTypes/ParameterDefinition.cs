using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class ParameterDefinition : ValueDefinition
    {
        static IVertex IsDerived_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\ParameterDefinition\IsDerived");

        public ParameterDefinition(IEdge edge) : base(edge) { }

        public bool IsDerived
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsDerived", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsDerived", null);

                if (val == null)
                    val = Vertex.AddVertex(IsDerived_meta, value);
                else
                    val.Value = value;
            }
        }

    }
}
