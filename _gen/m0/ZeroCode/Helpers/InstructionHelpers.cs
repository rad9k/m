using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace m0.ZeroCode.Helpers
{
    public class InstructionHelpers
    {
        public static INoInEdgeInOutVertexVertex CreateStack()
        {
            return new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
        }

        public static void AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(INoInEdgeInOutVertexVertex destination, IEnumerable<IEdge> source)
        {
            foreach (IEdge e in source)
                destination.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);
                //destination.AddEdgeForNoInEdgeInOutVertexVertex(e);
        }

        public static void AddToStack(INoInEdgeInOutVertexVertex destination, IEnumerable<IEdge> source)
        {
            foreach (IEdge e in source)
                //destination.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);
                destination.AddEdgeForNoInEdgeInOutVertexVertex(e);
        }

        public static INoInEdgeInOutVertexVertex CreateStackAndCopy(IEnumerable<IEdge> source)
        {
            INoInEdgeInOutVertexVertex newStack = CreateStack();

            //AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(newStack, source);
            AddToStack(newStack, source);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Create_INoInEdgeInOutVertexVertex_FromEdgesList(IEnumerable<IEdge> source)
        {
            if (source is INoInEdgeInOutVertexVertex)
                return (INoInEdgeInOutVertexVertex)source;

            return CreateStackAndCopy(source);
        }

        public static bool CheckIfIs(IVertex v, string i)
        {
            IVertex iv = GraphUtil.GetQueryOutFirst(v, "$Is", (object)i);

            if (iv != null)
                return true;

            return false;
        }

        // PROPER BEG

        public static bool CheckIfIsOrInherits(IVertex baseVertex, string test)
        {
            IList<IEdge> allIs = InstructionHelpers.GetAllIs(baseVertex);

            foreach (IEdge e in allIs)
                if (GraphUtil.GetValueAndCompareStrings(e.To, test))
                    return true;

            foreach (IEdge e in allIs)
                if (CheckIfInherits(e.To, test))
                    return true;

            return false;
        }

        public static bool CheckIfInherits(IVertex baseVertex, string test)
        {
            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits", null)) {
                if (GraphUtil.GetValueAndCompareStrings(e.To, test))
                    return true;

                if (CheckIfInherits(e.To, test))
                    return true;
            }

            return false;
        }
        
        // PROPER END

        // WRONG BEG

        public static bool CheckIfInherits_WRONG(IVertex v, string i) // need to recursively check $Inheritence
        {
            IVertex iv = GraphUtil.GetQueryOutFirst(v, "$Inherits", (object)i);

            if (iv != null)
                return true;

            return false;
        }

        public static bool CheckIfIsOrInherits_WRONG(IVertex baseVertex, string value) // need to recursively check $Inheritence
                                                                                       // $Inherits target can have $Inherits also. This is not checked here, so basically IS WRONG
                                                                                       // BUT as this seems to be working IN SOME PLACES, I leave it for now XXX
        {
            IList<IEdge> allIs = InstructionHelpers.GetAllIs(baseVertex);

            foreach(IEdge e in allIs)
                if (GraphUtil.GetValueAndCompareStrings(e.To, value))
                    return true;

            /*foreach (IEdge e in allIs) // I WOULD SAY THAT THIS IS WRONG. This is another meta level!!!!!! XXX
                foreach (IEdge ee in InstructionHelpers.GetAllIs(e.To))
                    if (GraphUtil.GetValueAndCompareStrings(ee.To, value))
                        return true;*/

            return CheckIfIsInherits_WRONG(baseVertex, value);            
        }

        public static bool CheckIfIsInherits_WRONG(IVertex baseVertex, string value) // need to recursively check $Inheritence
        // $Inherits target can have $Inherits also. This is not checked here, so basically IS WRONG
        // BUT as this seems to be working IN SOME PLACES, I leave it for now XXX
        {
            IList<IEdge> allIs = InstructionHelpers.GetAllIs(baseVertex);            

            foreach (IEdge e in allIs)                
                if(GraphUtil.GetQueryOutCount(e.To, "$Inherits", value) > 0)                
                    return true;

            return false;
        }

        public static bool CheckIfIsAtomType_WRONG(IVertex is_v) // need to recursively check $Inheritence
        {
            if (is_v == null)
                return true;

            return CheckIfIsOrInherits_WRONG(is_v, "AtomType");
        }

        // WRONG END

        public static void CopyVertex(IEdge edgeToCopy, IVertex copyTo)
        {
            if (CheckIfIsAtomType_WRONG(edgeToCopy.To))
                copyTo.AddVertex(edgeToCopy.Meta, edgeToCopy.To.Value);            
            else            
                GraphUtil.DeepCopy(edgeToCopy, copyTo);
            
        }

        public static bool CheckIfHasExecutableEndPoint(IVertex is_v)
        {
            if (is_v == null)
                return false;

            IVertex eepv = GraphUtil.GetQueryOutFirst(is_v, "$ExecutableEndPoint", null);

            if (eepv != null)
                return true;

            return false;
        }

        public static IVertex GetIs(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "$Is", null);
        }

        public static IList<IEdge> GetAllIs(IVertex v)
        {
            return GraphUtil.GetQueryOut(v, "$Is", null);
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
                return exe.ExecuteInstructionByMontevideoPrinciples(inStack, nextExpression, out isStackFrameReturn);

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inStack);
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

        public static bool IsTrue_Stack(IVertex baseVertex)
        {
            if (baseVertex == null || baseVertex.Count() == 0)
                return false;

            foreach (IEdge e in baseVertex)
                if (!IsTrue_Vertex(e.To))
                    return false;

            return true;
        }

        public static bool IsTrue_Vertex(IVertex baseVertex) { 
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
                        if(Comparer<decimal>.Default.Compare((decimal)val, 0) > 0)
                        //if (NumberCompare<decimal>((decimal)val, 1)) // other def of true
                            return true;
                        break;

                    case NumericTypeEnum.Double:
                        if (Comparer<double>.Default.Compare((double)val, 0) > 0)
                        //if (NumberCompare<double>((double)val, 1))
                            return true;
                        break;

                    case NumericTypeEnum.Integer:
                        if (Comparer<int>.Default.Compare((int)val, 0) > 0)
                        //if (NumberCompare<int>((int)val, 1))
                            return true;
                        break;
                }
            }

            if (GeneralUtil.CompareStrings(baseVertex.Value, "True") ||
                GeneralUtil.CompareStrings(baseVertex.Value, "true"))
                return true;

            return false;
        }

        public enum BooleanEnum { True, False, Undefined}

        public static BooleanEnum GetBolleanValue(IVertex baseVertex)
        {
            if (baseVertex.Value == null)
                return BooleanEnum.Undefined;

            object val;

            GraphUtil.GetNumberValue(baseVertex, out val);

            if (val != null)
            {
                NumericTypeEnum numericType = GetNumericType(val);

                switch (numericType)
                {
                    case NumericTypeEnum.Decimal:
                        if (Comparer<double>.Default.Compare(0, (double)val) < 0)
                            return BooleanEnum.True;
                        else
                            return BooleanEnum.False;

                    case NumericTypeEnum.Double:
                        if (Comparer<double>.Default.Compare(0, (double)val) < 0)
                            return BooleanEnum.True;
                        else
                            return BooleanEnum.False;

                    case NumericTypeEnum.Integer:
                        if(Comparer<int>.Default.Compare(0, (int)val) < 0)                        
                            return BooleanEnum.True;
                        else
                            return BooleanEnum.False;
                }
            }

            if (GeneralUtil.CompareStrings(baseVertex.Value, "True") ||
                GeneralUtil.CompareStrings(baseVertex.Value, "true"))
                return BooleanEnum.True;

            if (GeneralUtil.CompareStrings(baseVertex.Value, "False") ||
                GeneralUtil.CompareStrings(baseVertex.Value, "false"))
                return BooleanEnum.False;

            return BooleanEnum.Undefined;
        }

        private static bool NumberCompare<T>(T leftValue, T rightValue)
        {
            if (EqualityComparer<T>.Default.Equals(leftValue, rightValue))
                return true;

            return false;
        } 
        
        public static IEdge GetFirstExecutionEdge(ZeroCodeExecution exe, IVertex instructionVertex)
        {
             return exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, instructionVertex).FirstOrDefault();
        }

        public static INoInEdgeInOutVertexVertex SequenciallyExecuteIntructionsWithNewStackAndIsStackFrameReturnSupport(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex possibleToReturnStack = null;

            exe.AddStackFrame(); // ENTER NEW STACK                                

            possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out isStackFrameReturn);

            exe.RemoveStackFrame();  // LEAVE NEW STACK                

            if (isStackFrameReturn)
                return possibleToReturnStack;

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static bool CompareVertexValues(IVertex leftVertex, IVertex rightVertex)
        {
            object leftNumber;
            object rightNumber;

            GraphUtil.GetNumberValue(leftVertex, out leftNumber);
            GraphUtil.GetNumberValue(rightVertex, out rightNumber);

            if (leftNumber != null && rightNumber != null)
            {
                switch (GetCommonNumericTypeDenominator(leftNumber, rightNumber))
                {
                    case NumericTypeEnum.Integer:
                        int leftInt = Convert.ToInt32(leftNumber);
                        int rightInt = Convert.ToInt32(rightNumber);
                        
                        if (EqualityComparer<int>.Default.Equals(leftInt, rightInt))
                            return true;
                        else
                            return false;                        

                    case NumericTypeEnum.Double:
                        double leftDouble = Convert.ToDouble(leftNumber);
                        double rightDouble = Convert.ToDouble(rightNumber);

                        if (EqualityComparer<double>.Default.Equals(leftDouble, rightDouble))
                            return true;
                        else
                            return false;                        

                    case NumericTypeEnum.Decimal:
                        decimal leftDecimal = Convert.ToDecimal(leftNumber);
                        decimal rightDecimal = Convert.ToDecimal(rightNumber);

                        if (EqualityComparer<decimal>.Default.Equals(leftDecimal, rightDecimal))
                            return true;
                        else
                            return false;                        
                }
            }

            string leftValue = leftVertex.Value.ToString();
            string rightValue = rightVertex.Value.ToString();

            if (EqualityComparer<string>.Default.Equals(leftValue, rightValue) ||
                        (GetBolleanValue(leftVertex) == BooleanEnum.True && GetBolleanValue(rightVertex) == BooleanEnum.True) || // true true
                        (GetBolleanValue(leftVertex) == BooleanEnum.False && GetBolleanValue(rightVertex) == BooleanEnum.False) // false false
                        )
                return true;
            else
                return false;
        }

        public static IVertex Get(bool metaMode, IVertex baseVertex, IVertex expression)
        {
            return MinusZero.Instance.DefaultExecuter.Get(metaMode, baseVertex, expression);
        }
    }
}
