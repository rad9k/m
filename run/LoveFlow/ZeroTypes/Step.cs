using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class Step: TypedEdge
    {
        static IVertex Day_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Step\Day");
        static IVertex Value_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Step\Value");

        public Step(IEdge edge) : base(edge) { }

        public int Day
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Day", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Day", null);

                if (val == null)
                    val = Vertex.AddVertex(Day_meta, value);
                else
                    val.Value = value;
            }
        }

        public double Value
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Value", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Value", null);

                if (val == null)
                    val = Vertex.AddVertex(Value_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
