using m0.Foundation;
using m0.Graph;
using m0.Util;
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

        public static bool CheckIfIsOrInherits(IVertex baseVertex, string value)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            return CheckIfIsOrInherits_recurrent(baseVertex, inheritsSet, value);
        }

        private static bool CheckIfIsOrInherits_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet, string value)
        {
            if (GraphUtil.GetValueAndCompareStrings(baseVertex, value))
                return true;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits", null))
                if (!inheritedSet.Contains(e.To))
                {
                    inheritedSet.Add(e.To);
                    if (CheckIfIsOrInherits_recurrent(e.To, inheritedSet, value))
                        return true;
                }

            return false;
        }

        public static bool CheckIfIsAtomType(IVertex is_v)
        {
            if (is_v == null)
                return true;

            return CheckIfIsOrInherits(is_v, "AtomType");
        }

        public static bool CheckIfHasExecutableEndPoint(IVertex is_v)
        {
            IVertex eepv = GraphUtil.GetQueryOutFirst(is_v, "$ExecutableEndPoint", null);

            if (eepv != null)
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

        public static INoInEdgeInOutVertexVertex NextExpressionHandle(ZeroCodeExecution exe, IVertex inStack, IVertex instructionVertex)
        {
            bool dummy;

            return NextExpressionHandle(exe, inStack, instructionVertex, out dummy);
        }

        public static INoInEdgeInOutVertexVertex NextExpressionHandle(ZeroCodeExecution exe, IVertex inStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;
            IVertex nextExpression = InstructionHelpers.GetNextExpression(instructionVertex);

            if (nextExpression != null)
                return exe.ExecuteInstruction(inStack, nextExpression, out isStackFrameReturn);

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inStack);
        }

        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions(ZeroCodeExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex stack = inStack;

            foreach (IEdge e in baseVertex)
                //stack = exe.ExecuteInstruction(stack, e.To); // XXX another interesting processing approach
                if (!ZeroCodeUtil.IsDolarMeta(e))
                { // XXX in some cases it might not work - instruction with meta begginning with $ will not be executed. nor its children
                    bool local_isStackFrameReturn;

                    INoInEdgeInOutVertexVertex possibleToReturnStack = exe.ExecuteInstruction(stack, e.To, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                    {
                        isStackFrameReturn = true;

                        stack = possibleToReturnStack;

                        break;
                    }
                }

            return stack;
        }

        public static IDictionary<EdgeKey_FromMeta, IList<IEdge>> CreateEdgeKey_FromMetaDictionary(INoInEdgeInOutVertexVertex queryResult)
        {
            IDictionary<EdgeKey_FromMeta, IList<IEdge>> dict = new Dictionary<EdgeKey_FromMeta, IList<IEdge>>();

            foreach (IEdge e in queryResult)
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

        public enum NumericTypeEnum { Integer, Double, Decimal }

        public static IList<object> GetNumberList(IList<IEdge> edges, out NumericTypeEnum resultType)
        {
            resultType = NumericTypeEnum.Decimal;

            bool allInteger = true;
            bool allDouble = true;

            IList<object> list = new List<object>();

            object number;

            foreach (IEdge e in edges)
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
                resultType = NumericTypeEnum.Integer;
            else
                if (allDouble)
                resultType = NumericTypeEnum.Double;

            return list;
        }

        public static NumericTypeEnum GetCommonNubmerResultDenominator(NumericTypeEnum left, NumericTypeEnum right)
        {
            NumericTypeEnum result = NumericTypeEnum.Integer;

            if (left == NumericTypeEnum.Decimal || right == NumericTypeEnum.Decimal)
                result = NumericTypeEnum.Decimal;

            if (left == NumericTypeEnum.Double || right == NumericTypeEnum.Double)
                result = NumericTypeEnum.Double;

            return result;
        }

        public static NumericTypeEnum GetCommonNumericTypeDenominator(object left, object right)
        {
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            NumericTypeEnum result = NumericTypeEnum.Integer;

            if (leftType == typeof(decimal) || rightType == typeof(decimal))
                result = NumericTypeEnum.Decimal;

            if (leftType == typeof(double) || rightType == typeof(double))
                result = NumericTypeEnum.Double;

            return result;
        }

        public static NumericTypeEnum GetNumericType(object obj)
        {
            Type type = obj.GetType();

            NumericTypeEnum result = NumericTypeEnum.Integer;

            if (type == typeof(decimal))
                result = NumericTypeEnum.Decimal;

            if (type == typeof(double))
                result = NumericTypeEnum.Double;

            return result;
        }

        public static bool isTrue_Stack(IVertex baseVertex)
        {
            if (baseVertex == null || baseVertex.Count() == 0)
                return false;

            foreach (IEdge e in baseVertex)
                if (!isTrue_Vertex(e.To))
                    return false;

            return true;
        }

        public static bool isTrue_Vertex(IVertex baseVertex) { 
            if (baseVertex.Value == null)
                return false;

            object val;

            GraphUtil.GetNumberValue(baseVertex, out val);

            if (val != null)
            {
                NumericTypeEnum numericType = GetNumericType(val);

                switch (numericType)
                {
                    case NumericTypeEnum.Decimal:
                        if (NumberCompare<decimal>((decimal)val, 1))
                            return true;
                        break;

                    case NumericTypeEnum.Double:
                        if (NumberCompare<double>((double)val, 1))
                            return true;
                        break;

                    case NumericTypeEnum.Integer:
                        if (NumberCompare<int>((int)val, 1))
                            return true;
                        break;
                }
            }

            if (GeneralUtil.CompareStrings(baseVertex.Value, "True") ||
                GeneralUtil.CompareStrings(baseVertex.Value, "true"))
                return true;

            return false;
        }

        private static bool NumberCompare<T>(T leftValue, T rightValue)
        {
            if (EqualityComparer<T>.Default.Equals((T)leftValue, rightValue))
                return true;

            return false;
        }
    }
}
