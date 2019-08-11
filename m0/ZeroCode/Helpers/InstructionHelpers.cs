using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    public class InstructionHelpers
    {
        public static INoInEdgeInOutVertexVertex CreateStack()
        {
            return new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
        }       

        public static void AddToStack(IEnumerable<IEdge> source, INoInEdgeInOutVertexVertex destination)
        {            
            foreach (IEdge e in source)
                destination.AddEdgeForNoInEdgeInOutVertexVertex(e);
        }

        public static INoInEdgeInOutVertexVertex CreateStackAndCopy(IEnumerable<IEdge> source)
        {
            INoInEdgeInOutVertexVertex newStack = CreateStack();

            AddToStack(source, newStack);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Create_INoInEdgeInOutVertexVertex_FromEdgesList(IEnumerable<IEdge> source)
        {
            if (source is INoInEdgeInOutVertexVertex)
                return (INoInEdgeInOutVertexVertex)source;

            return CreateStackAndCopy(source);
        }

        public static bool CheckIs(IVertex v, string i)
        {
            IVertex iv = GraphUtil.GetQueryOutFirst(v, "$Is", (object)i);

            if (iv != null)
                return true;

            return false;
        }

        public static bool CheckIsNewVertex(IVertex v)
        {
            IVertex iv = GraphUtil.GetQueryOutFirst(v, "$Is", null);

            if (iv == null)
                return true;

            return false;
        }

        public static IVertex GetIs(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "$Is", null);
        }

        public static IVertex GetLeft(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "LeftExpression", null);
        }

        public static IVertex GetRight(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "RightExpression", null);
        }

        public static IVertex GetExpression(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "Expression", null);
        }

        public static IVertex GetNextExpression(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "NextExpression", null);
        }

        public static INoInEdgeInOutVertexVertex NextExpressionHandle(ZeroCodeExecution exe, IVertex inQs, IVertex instructionVertex)
        {
            IVertex nextExpression = InstructionHelpers.GetNextExpression(instructionVertex);

            if (nextExpression != null)
                return exe.ExecuteInstruction(inQs, nextExpression);

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inQs);            
        }

        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions(ZeroCodeExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex)
        {
            INoInEdgeInOutVertexVertex stack = inStack;

            foreach (IEdge e in baseVertex)
                //stack = exe.ExecuteInstruction(stack, e.To); // XXX another interesting processing approach
                if(!ZeroCodeUtil.IsDolarMeta(e)) // XXX in some cases it might not work - instruction with meta begginning with $ will not be executed. nor its children
                    exe.ExecuteInstruction(stack, e.To);

            return stack;
        }

        public static IDictionary<EdgeKey_FromMeta, IList<IEdge>> CreateEdgeKey_FromMetaDictionary(INoInEdgeInOutVertexVertex queryResult)
        {
            IDictionary<EdgeKey_FromMeta, IList<IEdge>> dict = new Dictionary<EdgeKey_FromMeta, IList<IEdge>>();

            foreach(IEdge e in queryResult)
            {
                EdgeKey_FromMeta ekfm = new EdgeKey_FromMeta(e);

                if (dict.ContainsKey(ekfm))
                    dict[ekfm].Add(e);
                else
                {
                    IList<IEdge> list = new List<IEdge>();
                    list.Add(e);
                    dict.Add(ekfm, list);
                }
            }

            return dict;
        }

        public static ISet<EdgeKey_MetaTo> CreateEdgeKey_MetaToSet(INoInEdgeInOutVertexVertex queryResult)
        {
            HashSet<EdgeKey_MetaTo> dict = new HashSet<EdgeKey_MetaTo>();

            foreach (IEdge e in queryResult)
            {
                EdgeKey_MetaTo ekmt = new EdgeKey_MetaTo(e);

                if (!dict.Contains(ekmt))
                    dict.Add(ekmt);
            }

            return dict;
        }

        public static IList<IEdge> CreateEdgeKey_MetaToEdgesList(INoInEdgeInOutVertexVertex queryResult)
        {
            HashSet<EdgeKey_MetaTo> _dict = new HashSet<EdgeKey_MetaTo>();
            IList<IEdge> dict = new List<IEdge>();

            foreach (IEdge e in queryResult)
            {
                EdgeKey_MetaTo ekmt = new EdgeKey_MetaTo(e);

                if (!_dict.Contains(ekmt))
                {
                    dict.Add(e);
                    _dict.Add(ekmt);
                }
            }

            return dict;
        }

        public static ISet<IEdge> CreateEdgeKey_ToSet(INoInEdgeInOutVertexVertex queryResult)
        {
            HashSet<IEdge> dict = new HashSet<IEdge>();

            foreach (IEdge e in queryResult)
                if (!dict.Contains(e))
                    dict.Add(e);

            return dict;
        }

        public enum GetNumberListResult { Integer, Double, Decimal}

        public static IList<object> GetNumberList(IList<IEdge> edges, out GetNumberListResult resultType)
        {
            resultType = GetNumberListResult.Decimal;

            bool allInteger = true;
            bool allDouble = true;

            IList<object> list = new List<object>();

            object number;

            foreach(IEdge e in edges)
            {
                GraphUtil.GetNumberValue(e.To, out number);

                if (!(number is int))
                    allInteger = false;

                if (!(number is double) && !(number is int))
                    allDouble = false;

                if (number != null)
                    list.Add(number);
            }

            if (allInteger)
                resultType = GetNumberListResult.Integer;
            else
                if (allDouble)
                resultType = GetNumberListResult.Double;

            return list;
        }

        public static GetNumberListResult GetCommonNubmerResultDenominator(GetNumberListResult left, GetNumberListResult right)
        {
            GetNumberListResult result = GetNumberListResult.Integer;

            if (left == GetNumberListResult.Decimal || right == GetNumberListResult.Decimal)
                result = GetNumberListResult.Decimal;

            if (left == GetNumberListResult.Double || right == GetNumberListResult.Double)
                result = GetNumberListResult.Double;

            return result;
        }

        public static GetNumberListResult GetCommonNubmerTypeDenominator(object left, object right)
        {
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            GetNumberListResult result = GetNumberListResult.Integer;

            if (leftType == typeof(decimal) || rightType == typeof(decimal))
                result = GetNumberListResult.Decimal;

            if (leftType == typeof(double) || rightType == typeof(double))
                result = GetNumberListResult.Double;

            return result;
        }
    }
}
