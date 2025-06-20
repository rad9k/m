using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class Flov : TypedEdge
    {
        static IVertex ParameterDefinition_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Flov\ParameterDefinition");
        static IVertex AddressDefinition_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Flov\AddressDefinition");
        static IVertex ParameterDefinition_type = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\ParameterDefinition");
        static IVertex AddressDefinition_type = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\AddressDefinition");

        public Flov(IEdge edge) : base(edge) { }

        public IList<ParameterDefinition> ParameterDefinitions
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "ParameterDefinition", null);

                IList<ParameterDefinition> ret = new List<ParameterDefinition>();

                foreach (IEdge e in list)
                    ret.Add((ParameterDefinition)TypedEdge.Get(e, typeof(ParameterDefinition)));

                return ret;
            }
        }

        public ParameterDefinition AddParameterDefinition()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, ParameterDefinition_type, ParameterDefinition_meta);

            return new ParameterDefinition(newEdge);
        }

        public IList<AddressDefinition> AddressDefinition
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "AddressDefinition", null);

                IList<AddressDefinition> ret = new List<AddressDefinition>();

                foreach (IEdge e in list)
                    ret.Add((AddressDefinition)TypedEdge.Get(e, typeof(AddressDefinition)));

                return ret;
            }
        }

        public AddressDefinition AddAddressDefinition()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, AddressDefinition_type, AddressDefinition_meta);

            return new AddressDefinition(newEdge);
        }
    }
}
