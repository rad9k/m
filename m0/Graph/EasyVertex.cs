using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Store;
using m0;
using m0.Util;
using m0.ZeroCode;
using System.Runtime.InteropServices;

namespace m0.Graph
{
    [Serializable]
    public class EasyVertex:VertexBase, IDisposable
    {        
        public override int UsageCounter
        {
            get
            {            
                return base.UsageCounter;
            }
            set
            {
                if(base.UsageCounter < value && UsageCounter == 0 && Store.DetachState == DetachStateEnum.Attached)
                        Store.StoreVertexIdentifier(this);

                if (base.UsageCounter > value && UsageCounter == 1 && Store.DetachState == DetachStateEnum.Attached)
                        Store.RemoveVertexIdentifier(this);

                base.UsageCounter = value;
            }
        }

        public object _Identifier;
        
        public override object Identifier { get { return _Identifier; }}


        protected object _Value;

        public override object Value {
            get{
                return _Value;
            }
            set{
                _Value = value;

                FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));
            }
        }

        protected bool HasInheritance=false;

        protected int InheritanceCount = 0;

        protected IList<IEdge> _InEdgesRaw;

        public override IList<IEdge> InEdgesRaw { get { return _InEdgesRaw; } }

        public override IEnumerable<IEdge> InEdges
        {
            get
            {
                if (HasInheritance)
                {
                    List<IEdge> FullEdges = InEdgesRaw.ToList();

                    foreach (IEdge e in InEdgesRaw)
                        if (GeneralUtil.CompareStrings(e.Meta.Value, "$Inherits"))
                            FullEdges.AddRange(e.To);

                    return FullEdges;
                }
                else
                    return InEdgesRaw;
            }
        }

        protected IList<IEdge> _OutEdgesRaw;

        public override IList<IEdge> OutEdgesRaw { get { return _OutEdgesRaw; } }

        public override IEnumerable<IEdge> OutEdges
        {
            get
            {
                if (HasInheritance)
                {
                    List<IEdge> FullEdges = OutEdgesRaw.ToList();

                    foreach (IEdge e in OutEdgesRaw)
                        if (GeneralUtil.CompareStrings(e.Meta.Value, "$Inherits"))
                            FullEdges.AddRange(e.To);

                    //return FullEdges;
                    return ZeroCodeEngine_OLD.RemoveDuplicates(FullEdges); // can be optimised. probably
                }else
                    return OutEdgesRaw;
            }
        }

        public override void AddInEdge(IEdge edge)
        {            
            InEdgesRaw.Add(edge);

            UsageCounter++;

            InEdgesDictionariesNeedsRebuild = true;

            VertexesThatInheritsDictionariesNeedsRebuild(true);

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeAdded,edge));
            // not needed as for now
        }

        public override void DeleteInEdge(IEdge _edge)
        {
            base.DeleteInEdge(_edge);

            IEdge edge = null;

            foreach (IEdge e in InEdgesRaw)
                if (e.Meta == _edge.Meta && e.From == _edge.From)
                    edge = e;

            if (edge != null)
            {
                InEdgesRaw.Remove(edge);

                UsageCounter--;

                InEdgesDictionariesNeedsRebuild = true;

                VertexesThatInheritsDictionariesNeedsRebuild(true);
            }

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeRemoved, edge));
            // not needed as for now
        }

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            if (destVertex == null)
                throw new Exception("target vertex can not be null");

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            UsageCounter++;

            OutEdgesDictionariesNeedsRebuild = true;

            VertexesThatInheritsDictionariesNeedsRebuild(false);

            if (GeneralUtil.CompareStrings(ne.Meta.Value, "$Inherits"))
            {
                InheritanceCount++;

                HasInheritance = true;
            }

            FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, ne));

            return ne;
        }

        public override void DeleteEdge(IEdge _edge)
        {
            IEdge edge = null;

            foreach (IEdge e in OutEdgesRaw)
                if(e.Meta==_edge.Meta && e.To==_edge.To)
                   edge = e;

            if (edge != null)
            {
                OutEdgesRaw.Remove(edge);

                UsageCounter--;

                OutEdgesDictionariesNeedsRebuild = true;
                VertexesThatInheritsDictionariesNeedsRebuild(false);

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritanceCount--;

                    if (InheritanceCount == 0)
                        HasInheritance = false;
                }

                FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge));

                edge.To.DeleteInEdge(edge);
            }
            //else // becouse of inheritance this may happen
                //throw new Exception(_edge.Meta + " : " + _edge.To + " edge does not exist in given Vertex");
        }        

        public EasyVertex(IStore _store):base(_store)
        {
            _InEdgesRaw = new List<IEdge>();
            _OutEdgesRaw = new List<IEdge>();


            _Identifier = Store.VertexIdentifierCount++;

            Value = "";
        }

        public override IVertex Execute(IVertex inputVertex, IVertex expression)
        { 
            //
            MinusZero m0 = MinusZero.Instance;
            //m0.Log(2,"Execute", "\"" + expression + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex ret= MinusZero.Instance.DefaultExecuter.Execute(this, inputVertex, expression);

            //
            m0.DoLog = prevDoLog;
            //

            return ret;
        }

        private static IDictionary<String, IVertex> ParseChache= new Dictionary<String,IVertex>();

        public override IVertex Get(string query)
        {
            //
            MinusZero m0=MinusZero.Instance;
            //m0.Log(2,"Get", "\"" + query + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex queryVertex = null;
            IVertex parseError = null;

            if (ParseChache.ContainsKey(query))
                queryVertex = ParseChache[query];
            else            
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null)
                    ParseChache.Add(query, queryVertex);
            }
                            
            //
            m0.DoLog = prevDoLog;
            //

            if (parseError != null)
                return null;

            return MinusZero.Instance.DefaultExecuter.Get(this, queryVertex);            
        }

        public override IVertex GetAll(string query)
        {
            //
            MinusZero m0 = MinusZero.Instance;
            //m0.Log(2, "GetAll", "\"" + query + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex queryVertex = null;
            IVertex parseError = null;

            if (ParseChache.ContainsKey(query))
                queryVertex = ParseChache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null)
                    ParseChache.Add(query, queryVertex);
            }

            //
            m0.DoLog = prevDoLog;
            //

            if (parseError != null)
                return null;

            return MinusZero.Instance.DefaultExecuter.GetAll(this, queryVertex);
        }

        public void Dispose()
        {
            GraphUtil.RemoveAllEdges(this);                        
        }

        private void VertexesThatInheritsDictionariesNeedsRebuild(bool inDictiories)
        {
            HashSet<IVertex> inheritsSet = GetVertexesThatInherits();

            foreach (IVertex v in inheritsSet)
                if (inDictiories)
                    v.InEdgesDictionariesNeedsRebuild = true;
                else
                    v.OutEdgesDictionariesNeedsRebuild = true;
        }

        private HashSet<IVertex> GetVertexesThatInherits()
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetVertexesThatInherits_reccurent(this, inheritsSet);

            return inheritsSet;
        }

        private void GetVertexesThatInherits_reccurent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            foreach(IEdge e in OutEdges)
                if(GeneralUtil.CompareStrings(e.Meta,"$Iherits") && !inheritedSet.Contains(e.To))
                {
                    inheritedSet.Add(e.To);
                    GetVertexesThatInherits_reccurent(e.To, inheritedSet);
                }
        }
    }
}
