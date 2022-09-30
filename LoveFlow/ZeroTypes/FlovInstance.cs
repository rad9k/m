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
    public class FlovInstance : TypedEdge
    {
        static IVertex Parameter_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\FlovInstance\Parameter");
        static IVertex Address_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\FlovInstance\Address");
        static IVertex Parameter_type = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Parameter");
        static IVertex Address_type = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Address");

        public FlovInstance(IEdge edge) : base(edge) { }

        public IList<Parameter> Parameters
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Parameter", null);

                IList<Parameter> ret = new List<Parameter>();

                foreach (IEdge e in list)    
                        ret.Add((Parameter)TypedEdge.Get(e, typeof(Parameter)));
                 
                return ret;
            }
        }

        public Parameter AddParameter()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Parameter_type, Parameter_meta);

            return new Parameter(newEdge);
        }

        public IList<Address> Addresses
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Address", null);

                IList<Address> ret = new List<Address>();

                foreach (IEdge e in list)
                    ret.Add((Address)TypedEdge.Get(e, typeof(Address)));

                return ret;
            }
        }

        public Address AddAddress()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Address_type, Address_meta);

            return new Address(newEdge);
        }
    }
}
