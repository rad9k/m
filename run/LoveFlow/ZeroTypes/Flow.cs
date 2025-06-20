using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class Flow: TypedEdge
    {
        static IVertex Destination_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Flow\Destination");
        static IVertex Alghoritm_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Flow\Alghoritm");

        public Flow(IEdge edge) : base(edge) { }

        public AddressDefinition Destination
        {
            get
            {
                IEdge e = GraphUtil.GetQueryOutFirstEdge(Vertex, "Destination", null);

                if (e == null)
                    return null;

                return (AddressDefinition)TypedEdge.Get(e, typeof(AddressDefinition));
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(this.Vertex, Destination_meta, value.Vertex);
            }
        }

        public IVertex Alghoritm
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Alghoritm", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Alghoritm", null);

                if (val == null)
                    val = Vertex.AddVertex(Alghoritm_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
