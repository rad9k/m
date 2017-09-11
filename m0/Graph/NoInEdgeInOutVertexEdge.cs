using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;
using m0;

namespace m0.Graph
{
    public class NoInEdgeInOutVertexEdge:IEdge
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

        public NoInEdgeInOutVertexEdge(IVertex From, IVertex Meta, IVertex To)
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

            // THIS IS NOT DONE HERE BY PURPOSE
            //
            // _to.AddInEdge(this);
            //
        }
    }
}
