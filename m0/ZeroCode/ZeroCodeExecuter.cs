using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    class ZeroCodeExecuter
    {
        IVertex baseVertex;
        IVertex inputVertex;

        const string colon = "|";

        static IList<IEdge> dummy = new List<IEdge>();

        public IVertex Execute(IVertex baseVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public IVertex Get(IVertex baseVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public IVertex GetAll(IVertex baseVertex, IVertex expression)
        {
            IVertex qs = CreateQueryStack();            

            if (CheckIs(expression, colon))
            {
                IVertex left = GetLeft(expression);
                IVertex right = GetRight(expression);

                object meta=null;
                object value = null;

                if (left != null)
                    meta = left.Value;

                if (right != null)
                    value = right.Value;

                AddResults(baseVertex, true, meta, value, qs);

            }

            if (qs.Count() == 0)
                return null;

            return qs;
        }

        private IVertex CreateQueryStack()
        {
            return new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
        }

        private bool CheckIs(IVertex v, string i)
        {
            IVertex iv=GraphUtil.GetOutFirst(v, "$Is", (object) i);

            if (iv != null)
                return true;

            return false;
        }

        private IVertex GetLeft(IVertex v)
        {
            IVertex i = GraphUtil.GetOutFirst(v, "LeftExpression", null);

            return i;
        }

        private IVertex GetRight(IVertex v)
        {
            IVertex i = GraphUtil.GetOutFirst(v, "RightExpression", null);

            return i;
        }

        public void AddResults(IVertex baseVertex, bool outEdges, object meta, object value, IVertex toAdd)
        {
            IEdge result;
            IList<IEdge> results;

            if(outEdges)
                baseVertex.QueryOutEdges(meta, value, out result, out results);
            else
                baseVertex.QueryInEdges(meta, value, out result, out results);

            if (result != null)            
                toAdd.AddEdge(result.Meta, result.To);

            if (results != null)
                foreach (IEdge e in results)
                    toAdd.AddEdge(e.Meta, e.To);
        }

        public ZeroCodeExecuter()
        {

        }

    }
}
