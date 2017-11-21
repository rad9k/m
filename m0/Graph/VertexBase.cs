using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Util;

namespace m0.Graph
{
    [Serializable]
    public class VertexBase:IVertex, IHasUsageCounter
    {
        public IEdge this[string meta]
        {
            get {                
                IVertex r = this.GetAll(meta + ":");

                if (r.Count() > 0)
                    return r.First();

                IVertex metavertexs = this.GetAll(@"$Is:\" + meta);

                if (metavertexs.Count() > 0)
                {
                    IEdge metavertex = metavertexs.First();

                    IEdge newedge = new EasyEdge(this, metavertex.To, null);

                    return newedge;
                }
                else
                {
                    if (meta == "$EdgeTarget")
                    {
                        IEdge metavertex = m0.MinusZero.Instance.Root.GetAll(@"System\Meta\Base\Vertex\$EdgeTarget").FirstOrDefault();

                        IEdge newedge = new EasyEdge(this, metavertex.To, null);

                        return newedge;
                    }
                }

                // inherits from Vertex. hiddenly
                // this is for Table Visualiser
                IEdge vertexMetaVertex = m0.MinusZero.Instance.Root.GetAll(@"System\Meta\Base\Vertex\" + meta).FirstOrDefault();

                if(vertexMetaVertex!=null)
                   return new EasyEdge(this, vertexMetaVertex.To, null);

                return null;
            }
        }

        public event VertexChange Change;

        public virtual Delegate[] GetChangeDelegateInvocationList()
        {
            if (Change != null)
                return Change.GetInvocationList();
            else
                return null;
        }

        public bool CanFireChangeEvent = true;

        public virtual void FireChange(VertexChangeEventArgs e){
            if(Change!=null && CanFireChangeEvent)
                Change(this,e);
        }

        public virtual int UsageCounter { get; set; }

        public virtual string Identifier 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual object Value
        {
            get
            {                
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual IEnumerable<IEdge> InEdges
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IEdge> InEdgesRaw
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IEnumerable<IEdge> OutEdges
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IEdge> OutEdgesRaw
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IVertex AddVertex(IVertex metaVertex, object val)
        {
            IVertex nv = (IVertex)Activator.CreateInstance(this.GetType(), new object[] { this.Store });

            nv.Value = val;

            /*if(val==null)
                MinusZero.Instance.Log(3,"AddVertex", "NULL");
            else
                MinusZero.Instance.Log(3,"AddVertex",val.ToString());*/

            AddEdge(metaVertex, nv);

            return nv;
        }

        public virtual void AddInEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteInEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex Execute(IVertex inputVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex Get(string query)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex GetAll(string query)
        {
            throw new NotImplementedException();
        }

        public IStore _store;

        public virtual IStore Store
        {
            get { return _store; }
        }

        IList<AccessLevelEnum> _AccessLevel;
        
        public virtual IList<AccessLevelEnum> AccessLevel
        {
            get { return _AccessLevel; }
        }

        public VertexBase(IStore _Store)
        {
            UsageCounter = 0;

            _store = _Store;
            _AccessLevel = GeneralUtil.CreateAndCopyList<AccessLevelEnum>(_store.AccessLevel);            
        }

        public override string ToString()
        {
            if (Value == null)
                return null;
            else
                return Value.ToString();
        }
                
        IEnumerator<IEdge> IEnumerable<IEdge>.GetEnumerator()
        {
            return OutEdges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)OutEdges).GetEnumerator();
        }
    }
}
