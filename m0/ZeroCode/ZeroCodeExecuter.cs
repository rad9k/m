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
        const string slash = @"\ ";

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

                IVertex target=null;
                
                if(right!=null)
                    target = GetTargetExpression(right);

                if (target != null)
                {
                    if (CheckIs(target, slash))
                    {
                        IVertex newExpression = GetTargetExpression(target);

                        qs = stepIntoAllEdges(qs);

                        return GetAll(qs, newExpression);
                    }
                }
            }

            return qs;
        }

        private IVertex CreateQueryStack()
        {
            return new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
        }

        private IVertex stepIntoAllEdges(IVertex oldQs)
        {
            IVertex newQs = CreateQueryStack();

            foreach (IEdge e in oldQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdge(ee.Meta, ee.To);

            return newQs;
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
            return GraphUtil.GetOutFirst(v, "LeftExpression", null);
        }

        private IVertex GetRight(IVertex v)
        {
            return GraphUtil.GetOutFirst(v, "RightExpression", null);
        }

        private IVertex GetTargetExpression(IVertex v)
        {
            return GraphUtil.GetOutFirst(v, "TargetExpression", null);
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

        public IVertex InnerOperator (IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }


        public ZeroCodeExecuter()
        {

        }

    }
}
