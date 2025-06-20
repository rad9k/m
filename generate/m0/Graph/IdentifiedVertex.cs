using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

namespace m0.Graph
{
    public class IdentifiedVertex:EasyVertex
    {
        protected override void VertexInit()
        {
            VertexInit_First();
        }

        public IdentifiedVertex(IStore store) : base(store) {
            VertexInit();

            _Identifier = this.GetHashCode().ToString();            

            Store.StoreVertexIdentifier(this);
        }

        public IdentifiedVertex(string identifier,IStore store):base(store)
        {
             VertexInit();

            _Identifier = identifier;

            Store.StoreVertexIdentifier(this);
        }
    }
}
