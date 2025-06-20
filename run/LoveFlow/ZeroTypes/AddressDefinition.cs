using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class AddressDefinition : ValueDefinition
    {
        static IVertex InstancesCount_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\AddressDefinition\InstancesCount");
        static IVertex InitialValue_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\AddressDefinition\InitialValue");
        static IVertex Flow_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\AddressDefinition\Flow");
        static IVertex Flow_type = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Flow");

        public AddressDefinition(IEdge edge) : base(edge) { }

        public double InitialValue
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "InitialValue", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "InitialValue", null);

                if (val == null)
                    val = Vertex.AddVertex(InitialValue_meta, value);
                else
                    val.Value = value;
            }
        }

        public IList<Flow> Flows
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Flow", null);

                IList<Flow> ret = new List<Flow>();

                foreach (IEdge e in list)
                    ret.Add((Flow)TypedEdge.Get(e, typeof(Flow)));

                return ret;
            }
        }

        public Flow AddFlow()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Flow_type, Flow_meta);

            return new Flow(newEdge);
        }
    }
}
