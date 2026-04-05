using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;

namespace m0.Graph
{
    [Serializable]
    public class EdgeBase:IEdge
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

        public EdgeBase(IVertex From, IVertex Meta, IVertex To)
        {
            _from = From;

            if (Meta != null)            
                _meta = Meta;                        
            else
                _meta = MinusZero.Instance.Empty;

            if (Meta is IHasUsageCounter)
            {
                IHasUsageCounter huc = (IHasUsageCounter)Meta;
                huc.UsageCounter++;
            }

            _to = To;

            if(_to!=null) // edge.To==null used for visualizing not existing edges (possible to be filled by user)
                _to.AddInEdge(this);
        }

        public override string ToString()
        {
            if (_to != null)
                return _to.ToString();

           return "Edge";
        }
    }
}
