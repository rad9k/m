using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;

namespace m0.Graph
{
    [Serializable]
    public class NonActingEdge : IEdge
    {
        protected IVertex _from;
        
        public virtual IVertex From
        {
            get { return _from; }
        }

        protected IVertex _meta; 

        public virtual IVertex Meta
        {
            get { return _meta; }
        }

        protected IVertex _to;

        public virtual IVertex To
        {
            get { return _to; }            
        }

        public NonActingEdge(IVertex From, IVertex Meta, IVertex To)
        {
            _from = From;

            _meta = Meta;

            _to = To;
        }

        public override string ToString()
        {
            if (_to != null)
                return _to.ToString();

           return "Edge";
        }
    }
}
