using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class ValueDefinition : TypedEdge
    {
        static IVertex Name_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\ValueDefinition\Name");
        static IVertex MinValue_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\ValueDefinition\MinValue");
        static IVertex MaxValue_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\ValueDefinition\MaxValue");
        static IVertex Unit_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\ValueDefinition\Unit");

        public ValueDefinition(IEdge edge) : base(edge) { }

        public string Name
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Name", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Name", null);

                if (val == null)
                    val = Vertex.AddVertex(Name_meta, value);
                else
                    val.Value = value;
            }
        }

        public double MinValue
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "MinValue", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "MinValue", null);

                if (val == null)
                    val = Vertex.AddVertex(MinValue_meta, value);
                else
                    val.Value = value;
            }
        }

        public double MaxValue
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "MaxValue", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "MaxValue", null);

                if (val == null)
                    val = Vertex.AddVertex(MaxValue_meta, value);
                else
                    val.Value = value;
            }
        }

        public string Unit
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Unit", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Unit", null);

                if (val == null)
                    val = Vertex.AddVertex(Unit_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
