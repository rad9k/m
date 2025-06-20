using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Util;

namespace m0.Graph
{
    [Serializable]
    public class EdgeBase : IEdge
    {
        public IVertex _from;
        
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

        public bool EdgeRemovalExecuting { get; set; }

        public EdgeBase(IVertex From, IVertex Meta, IVertex To)
        {
            _from = From;
            _to = To;

            if (Meta != null)            
                _meta = Meta;                        
            else
                _meta = MinusZero.Instance.Empty;          
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                int hash = 17;
                
                if(_from != null)
                    hash = hash * 23 + _from.GetHashCode();

                if(_meta != null)
                    hash = hash * 23 + _meta.GetHashCode();

                if(_to != null)
                    hash = hash * 23 + _to.GetHashCode();

                return hash;
            }
        }

        public EdgeBase()
        {

        }

        public override string ToString()
        {
            if (_to != null)
            {
                if (_meta != null && !GeneralUtil.CompareStrings(_meta, "$Empty"))
                    return _meta.ToString() + " : " + _to.ToString();

                return _to.ToString();
            }

           return "Edge";
        }
    }
}
