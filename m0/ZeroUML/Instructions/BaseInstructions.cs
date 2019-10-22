using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.ZeroCode.Helpers;
using m0.ZeroCode;
using m0.Util;
using m0.Graph;
using static m0.ZeroCode.Helpers.InstructionHelpers;

namespace m0.ZeroUML.Instructions
{
    public class BaseInstructions
    {
        ////////////////////////////////////////////////////////////////
        //
        // query 
        //
        ////////////////////////////////////////////////////////////////

#region Query

        public static INoInEdgeInOutVertexVertex QueryOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            if (instructionVertex.Value == null)
                return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            string value = instructionVertex.Value.ToString();

            if (value == "" || value == "\r")
                return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            IEdge e;
            IList<IEdge> eList;

            if (exe.metaMode)
                inputQs.QueryOutEdges(value, null, out e, out eList);
            else
                inputQs.QueryOutEdges(null, value, out e, out eList);

            if (e != null)
                newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

            if (eList != null)
                InstructionHelpers.AddToStack(newQs, eList);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex _inputQs = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex newQs = _inputQs;
            INoInEdgeInOutVertexVertex oldQs = _inputQs;

            foreach (IEdge expression in expressions)
            {
                newQs = InstructionHelpers.CreateStack();

                foreach (IEdge e in oldQs)
                {
                    IVertex outQs = exe.ExecuteInstructionByMontevideoPrinciples(e.To, expression.To);

                    if (outQs.OutEdges.Count() > 0)
                        newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);
                }

                oldQs = newQs;
            }

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex QuestionMarkOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();
            GraphIterator iter = new GraphIterator(newQs);

            GraphUtil.DeepIterator(inputQs, iter.AddToINoInEdgeInOutVertexVertex, false, false, true);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex SlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex ColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            bool isLeftExpressionQuery = false;
            bool isRightExpressionQuery = false;

            if (leftExpression != null)
                isLeftExpressionQuery = InstructionHelpers.CheckIs(leftExpression, "Query");

            if (rightExpression != null)
                isRightExpressionQuery = InstructionHelpers.CheckIs(rightExpression, "Query");

            string leftValue = null;
            string rightValue = null;

            if (isLeftExpressionQuery)
                leftValue = GraphUtil.GetStringValue(leftExpression);

            if (isRightExpressionQuery)
                rightValue = GraphUtil.GetStringValue(rightExpression);

            string metaQueryString = null, toQueryString = null;

            if (leftValue != null && leftValue != "")
                metaQueryString = leftValue;

            if (rightValue != null && rightValue != "")
                toQueryString = rightValue;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            if (isLeftExpressionQuery || isRightExpressionQuery)
            {
                IEdge e;
                IList<IEdge> eList;

                inputQs.QueryOutEdges(metaQueryString, toQueryString, out e, out eList);

                if (e != null)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

                if (eList != null)
                    InstructionHelpers.AddToStack(newQs, eList);
            }
            else
                InstructionHelpers.AddToStack(newQs, inputQs);

            if (leftExpression != null)
            {
                if (isRightExpressionQuery)
                {
                    IVertex nextExpression = InstructionHelpers.GetNextExpression(instructionVertex);

                    if (nextExpression != null)
                        newQs = ColonSubExpressionProcess_Meta(exe, newQs, nextExpression);
                }
                else
                    newQs = ColonSubExpressionProcess_Meta(exe, newQs, leftExpression);
            }

            if (rightExpression != null)
            {
                if (isRightExpressionQuery)
                    newQs = InstructionHelpers.NextExpressionHandle(exe, newQs, rightExpression);
                else
                    newQs = exe.ExecuteInstructionByMontevideoPrinciples(newQs, rightExpression);
            }

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        private static INoInEdgeInOutVertexVertex ColonSubExpressionProcess_Meta(ZeroCodeExecution exe, INoInEdgeInOutVertexVertex inQs, IVertex expression)
        {
            Dictionary<IVertex, bool> metaDict = new Dictionary<IVertex, bool>();

            INoInEdgeInOutVertexVertex localQs = InstructionHelpers.CreateStack();

            foreach (IEdge e in inQs)
                if (!metaDict.ContainsKey(e.Meta))
                {
                    localQs.AddEdgeForNoInEdgeInOutVertexVertex(GraphUtil.CreateArtificialEdge(null, e.Meta));
                    metaDict.Add(e.Meta, false);
                }

            INoInEdgeInOutVertexVertex afterCallQs = exe.ExecuteInstructionByMontevideoPrinciples(localQs, expression);

            foreach (IEdge e in afterCallQs)
                metaDict[e.To] = true;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            foreach (IEdge e in inQs)
                if (metaDict[e.Meta] == true)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

            return newQs;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // edge operators
        //
        ////////////////////////////////////////////////////////////////

#region EdgeOperators

        // =
        public static INoInEdgeInOutVertexVertex RedirectLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict = InstructionHelpers.CreateEdgeKey_FromMetaDictionary(leftExecuteResult);

            foreach (KeyValuePair<EdgeKey_FromMeta, IList<IEdge>> localLeft in leftFromMeta_dict)
            {
                IEdge toAdd = localLeft.Value[0];

                toAdd.From.DeleteEdgesList(localLeft.Value);

                foreach (IEdge e in rightExecuteResult)
                    toAdd.From.AddEdge(toAdd.Meta, e.To);
            }

            return exe.stack;
        }

        // +=
        public static INoInEdgeInOutVertexVertex AddLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict = InstructionHelpers.CreateEdgeKey_FromMetaDictionary(leftExecuteResult);

            foreach (KeyValuePair<EdgeKey_FromMeta, IList<IEdge>> localLeft in leftFromMeta_dict)
            {
                IEdge toAdd = localLeft.Value[0];

                foreach (IEdge e in rightExecuteResult)
                    toAdd.From.AddEdge(toAdd.Meta, e.To);
            }

            return exe.stack;
        }

        // +<
        public static INoInEdgeInOutVertexVertex AddRightEdgesIntoLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            foreach (IEdge leftEdge in leftExecuteResult)
                foreach (IEdge rightEdge in rightExecuteResult)
                    leftEdge.To.AddEdge(rightEdge.Meta, rightEdge.To);

            return exe.stack;
        }

        // ~=
        public static INoInEdgeInOutVertexVertex DeleteRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            ISet<IEdge> rightResultToSet = InstructionHelpers.CreateEdgeKey_ToSet(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
            {
                HashSet<IEdge> usedEdges = new HashSet<IEdge>();

                foreach (IEdge rightEdge in rightResultToSet)
                    if (leftEdge.To == rightEdge.To && !usedEdges.Contains(rightEdge))
                    {
                        leftEdge.From.DeleteEdge(leftEdge);
                        usedEdges.Add(rightEdge);
                    }
            }

            return exe.stack;
        }

        // -<
        public static INoInEdgeInOutVertexVertex DeleteRightEdgesFromLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            IList<IEdge> rightResultMetaToEdgesList = InstructionHelpers.CreateEdgeKey_MetaToEdgesList(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
                leftEdge.To.DeleteEdgesList(rightResultMetaToEdgesList);

            return exe.stack;
        }

        // ~<
        public static INoInEdgeInOutVertexVertex DeleteRightVerticesFromLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            ISet<IEdge> rightResultToSet = InstructionHelpers.CreateEdgeKey_ToSet(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
                foreach (IEdge intoLeftEdge in leftEdge.To.ToList<IEdge>())
                {
                    HashSet<IEdge> usedEdges = new HashSet<IEdge>();

                    foreach (IEdge rightEdge in rightResultToSet)
                        if (intoLeftEdge.To == rightEdge.To && !usedEdges.Contains(rightEdge))
                        {
                            intoLeftEdge.From.DeleteEdge(intoLeftEdge);
                            usedEdges.Add(rightEdge);
                        }
                }

            return exe.stack;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // edge set operators
        //
        ////////////////////////////////////////////////////////////////

#region EdgeSetOperators

        public static INoInEdgeInOutVertexVertex EdgeSetAdd(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in leftExecuteResult)
                localStack.AddEdge(e.Meta, e.To);

            foreach (IEdge e in rightExecuteResult)
                localStack.AddEdge(e.Meta, e.To);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex EdgeSetSubstract(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = leftExecuteResult;

            leftExecuteResult.DeleteEdgesList(rightExecuteResult);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex SetIndex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);            

            if (expression == null)
                return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            INoInEdgeInOutVertexVertex executeResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, expression);

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            foreach(IEdge e in executeResult)
            {
                int? index=GraphUtil.GetIntegerValue(e.To);

                if (index != null && index >=1 && index <= inputStack.OutEdges.Count())
                    localStack.AddEdgeForNoInEdgeInOutVertexVertex(inputStack.OutEdges[(int)index - 1]);
            }
            
            return InstructionHelpers.NextExpressionHandle(exe, localStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex SetCount(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;
            
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            localStack.AddVertex(null, inputStack.OutEdges.Count());

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex EmptySet(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;
           
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();            

            return localStack;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // number algebra operators
        //
        ////////////////////////////////////////////////////////////////

        #region Operators

        public static INoInEdgeInOutVertexVertex Add(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.NumericTypeEnum leftResultType;
            InstructionHelpers.NumericTypeEnum rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.NumericTypeEnum.Integer:
                    return _Add_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.NumericTypeEnum.Double:
                    return _Add_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.NumericTypeEnum.Decimal:
                    return _Add_Logic_decimal(leftNumbers, rightNumbers);
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        static INoInEdgeInOutVertexVertex _Add_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                int left = Convert.ToInt32(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    int right = Convert.ToInt32(_right);
                    localStack.AddVertex(null, left + right);
                }
            }
            else
            {
                int right = Convert.ToInt32(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    int left = Convert.ToInt32(_left);
                    localStack.AddVertex(null, left + right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Add_Logic_double(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                double left = Convert.ToDouble(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    double right = Convert.ToDouble(_right);
                    localStack.AddVertex(null, left + right);
                }
            }
            else
            {
                double right = Convert.ToDouble(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    double left = Convert.ToDouble(_left);
                    localStack.AddVertex(null, left + right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Add_Logic_decimal(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                decimal left = Convert.ToDecimal(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    decimal right = Convert.ToDecimal(_right);
                    localStack.AddVertex(null, left + right);
                }
            }
            else
            {
                decimal right = Convert.ToDecimal(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    decimal left = Convert.ToDecimal(_left);
                    localStack.AddVertex(null, left + right);
                }
            }

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex Substract(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.NumericTypeEnum leftResultType;
            InstructionHelpers.NumericTypeEnum rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.NumericTypeEnum.Integer:
                    return _Substract_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.NumericTypeEnum.Double:
                    return _Substract_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.NumericTypeEnum.Decimal:
                    return _Substract_Logic_decimal(leftNumbers, rightNumbers);
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }
        static INoInEdgeInOutVertexVertex _Substract_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                int left = Convert.ToInt32(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    int right = Convert.ToInt32(_right);
                    localStack.AddVertex(null, left - right);
                }
            }
            else
            {
                int right = Convert.ToInt32(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    int left = Convert.ToInt32(_left);
                    localStack.AddVertex(null, left - right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Substract_Logic_double(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                double left = Convert.ToDouble(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    double right = Convert.ToDouble(_right);
                    localStack.AddVertex(null, left - right);
                }
            }
            else
            {
                double right = Convert.ToDouble(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    double left = Convert.ToDouble(_left);
                    localStack.AddVertex(null, left - right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Substract_Logic_decimal(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                decimal left = Convert.ToDecimal(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    decimal right = Convert.ToDecimal(_right);
                    localStack.AddVertex(null, left - right);
                }
            }
            else
            {
                decimal right = Convert.ToDecimal(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    decimal left = Convert.ToDecimal(_left);
                    localStack.AddVertex(null, left - right);
                }
            }

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex Multiply(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.NumericTypeEnum leftResultType;
            InstructionHelpers.NumericTypeEnum rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.NumericTypeEnum.Integer:
                    return _Multiply_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.NumericTypeEnum.Double:
                    return _Multiply_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.NumericTypeEnum.Decimal:
                    return _Multiply_Logic_decimal(leftNumbers, rightNumbers);
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        static INoInEdgeInOutVertexVertex _Multiply_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                int left = Convert.ToInt32(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    int right = Convert.ToInt32(_right);
                    localStack.AddVertex(null, left * right);
                }
            }
            else
            {
                int right = Convert.ToInt32(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    int left = Convert.ToInt32(_left);
                    localStack.AddVertex(null, left * right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Multiply_Logic_double(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                double left = Convert.ToDouble(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    double right = Convert.ToDouble(_right);
                    localStack.AddVertex(null, left * right);
                }
            }
            else
            {
                double right = Convert.ToDouble(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    double left = Convert.ToDouble(_left);
                    localStack.AddVertex(null, left * right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Multiply_Logic_decimal(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                decimal left = Convert.ToDecimal(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    decimal right = Convert.ToDecimal(_right);
                    localStack.AddVertex(null, left * right);
                }
            }
            else
            {
                decimal right = Convert.ToDecimal(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    decimal left = Convert.ToDecimal(_left);
                    localStack.AddVertex(null, left * right);
                }
            }

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex Divide(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.NumericTypeEnum leftResultType;
            InstructionHelpers.NumericTypeEnum rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.NumericTypeEnum.Integer:
                    return _Divide_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.NumericTypeEnum.Double:
                    return _Divide_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.NumericTypeEnum.Decimal:
                    return _Divide_Logic_decimal(leftNumbers, rightNumbers);
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        static INoInEdgeInOutVertexVertex _Divide_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                int left = Convert.ToInt32(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    int right = Convert.ToInt32(_right);
                    localStack.AddVertex(null, left / right);
                }
            }
            else
            {
                int right = Convert.ToInt32(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    int left = Convert.ToInt32(_left);
                    localStack.AddVertex(null, left / right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Divide_Logic_double(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                double left = Convert.ToDouble(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    double right = Convert.ToDouble(_right);
                    localStack.AddVertex(null, left / right);
                }
            }
            else
            {
                double right = Convert.ToDouble(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    double left = Convert.ToDouble(_left);
                    localStack.AddVertex(null, left / right);
                }
            }

            return localStack;
        }
        static INoInEdgeInOutVertexVertex _Divide_Logic_decimal(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (leftNumbers.Count == 1 || rightNumbers.Count > 1)
            {
                decimal left = Convert.ToDecimal(leftNumbers[0]);

                foreach (object _right in rightNumbers)
                {
                    decimal right = Convert.ToDecimal(_right);
                    localStack.AddVertex(null, left / right);
                }
            }
            else
            {
                decimal right = Convert.ToDecimal(rightNumbers[0]);

                foreach (object _left in leftNumbers)
                {
                    decimal left = Convert.ToDecimal(_left);
                    localStack.AddVertex(null, left / right);
                }
            }

            return localStack;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // logic operators
        //
        ////////////////////////////////////////////////////////////////

#region LogicOperators

        private static INoInEdgeInOutVertexVertex LogicDoubleOperator(LogicDoubleOpertorEnum opetationType, ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, String leftAndRightResultsEmptyOperatorResult)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            int toBeProcessedCount;

            if (leftExecuteResult.Count > rightExecuteResult.Count)
                toBeProcessedCount = rightExecuteResult.Count;
            else
                toBeProcessedCount = leftExecuteResult.Count;

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            if (toBeProcessedCount == 0) // left and right empty
            {
                localStack.AddVertex(null, leftAndRightResultsEmptyOperatorResult);
            }
            else
            {
                for (int x = 0; x < toBeProcessedCount; x++)
                {
                    bool logicalResult = false;

                    IVertex leftVertex = leftExecuteResult[x].To;
                    IVertex rightVertex = rightExecuteResult[x].To;

                    logicalResult = LogicDoubleOperator_VertexLevel(leftVertex, rightVertex, opetationType);

                    if (logicalResult)
                        localStack.AddVertex(null, "True");
                    else
                        localStack.AddVertex(null, "False");

                }
            }

            return localStack;
        }

        enum LogicDoubleOpertorEnum {Equal, ExactEqual, NotEqual, And, Or, MoreThan, LessThan, MoreOrEqualThan, LessOrEqualThan }

        private static bool LogicDoubleOperator_VertexLevel(IVertex leftVertex, IVertex rightVertex, LogicDoubleOpertorEnum operationType)
        {
            bool logicalResult = false;

            object leftNumber;
            object rightNumber;

            GraphUtil.GetNumberValue(leftVertex, out leftNumber);
            GraphUtil.GetNumberValue(rightVertex, out rightNumber);

            if (leftNumber != null && rightNumber != null)
            {
                switch (InstructionHelpers.GetCommonNumericTypeDenominator(leftNumber, rightNumber))
                {
                    case InstructionHelpers.NumericTypeEnum.Integer:
                        int leftInt = Convert.ToInt32(leftNumber);
                        int rightInt = Convert.ToInt32(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<int>(leftInt, rightInt, operationType, 0);
                        break;

                    case InstructionHelpers.NumericTypeEnum.Double:
                        double leftDouble = Convert.ToDouble(leftNumber);
                        double rightDouble = Convert.ToDouble(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<double>(leftDouble, rightDouble, operationType, 0);
                        break;

                    case InstructionHelpers.NumericTypeEnum.Decimal:
                        decimal leftDecimal = Convert.ToDecimal(leftNumber);
                        decimal rightDecimal = Convert.ToDecimal(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<decimal>(leftDecimal, rightDecimal, operationType, 0);
                        break;
                }
            }
            else
                logicalResult = LogicDoubleOperator_ExecuteString(leftVertex, rightVertex, operationType);

            return logicalResult;
        }

        private static bool LogicDoubleOperator_ExecuteNumeric<T>(T leftValue, T rightValue, LogicDoubleOpertorEnum operationType, T zeroValue)
        {
            bool output = false;

            switch (operationType)
            {
                case LogicDoubleOpertorEnum.Equal:
                    if (EqualityComparer<T>.Default.Equals(leftValue, rightValue))
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.ExactEqual:
                    if (EqualityComparer<T>.Default.Equals(leftValue, rightValue))
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.NotEqual:
                    if (!EqualityComparer<T>.Default.Equals(leftValue, rightValue))
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.And:
                    if (Comparer<T>.Default.Compare(zeroValue, leftValue) < 0 &&
                        Comparer<T>.Default.Compare(zeroValue, rightValue) < 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.Or:
                    if (Comparer<T>.Default.Compare(zeroValue, leftValue) < 0 ||
                        Comparer<T>.Default.Compare(zeroValue, rightValue) < 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.MoreThan:
                    if (Comparer<T>.Default.Compare(leftValue, rightValue) > 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.MoreOrEqualThan:
                    if (Comparer<T>.Default.Compare(leftValue, rightValue) >= 0 )
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.LessThan:
                    if (Comparer<T>.Default.Compare(leftValue, rightValue) < 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.LessOrEqualThan:
                    if (Comparer<T>.Default.Compare(leftValue, rightValue) <= 0)
                        output = true;
                    break;
            }

            return output;
        }

        private static bool LogicDoubleOperator_ExecuteString(IVertex leftVertex, IVertex rightVertex, LogicDoubleOpertorEnum operationType)
        {
            bool output = false;

            if (leftVertex == null || leftVertex.Value == null || rightVertex == null || rightVertex.Value == null)
                return output;

            string leftValue = leftVertex.Value.ToString();
            string rightValue = rightVertex.Value.ToString();

            switch (operationType)
            {
                case LogicDoubleOpertorEnum.Equal:
                    if (
                        EqualityComparer<string>.Default.Equals(leftValue, rightValue) ||

                        (InstructionHelpers.GetBolleanValue(leftVertex) == BooleanEnum.True && InstructionHelpers.GetBolleanValue(rightVertex) == BooleanEnum.True) || // true true

                        (InstructionHelpers.GetBolleanValue(leftVertex) == BooleanEnum.False && InstructionHelpers.GetBolleanValue(rightVertex) == BooleanEnum.False) // false false
                        )                        
                            output = true;
                    break;

                case LogicDoubleOpertorEnum.ExactEqual:
                    if (
                        EqualityComparer<string>.Default.Equals(leftValue, rightValue)
                        )
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.NotEqual:
                    if(
                        !EqualityComparer<string>.Default.Equals(leftValue, rightValue) ||

                        (InstructionHelpers.GetBolleanValue(leftVertex) == BooleanEnum.True && InstructionHelpers.GetBolleanValue(rightVertex) == BooleanEnum.False) || // true false

                        (InstructionHelpers.GetBolleanValue(leftVertex) == BooleanEnum.False && InstructionHelpers.GetBolleanValue(rightVertex) == BooleanEnum.True) // false true
                        )

                        output = true;
                    break;

                case LogicDoubleOpertorEnum.And:
                    if (InstructionHelpers.GetBolleanValue(leftVertex) == BooleanEnum.True && InstructionHelpers.GetBolleanValue(rightVertex) == BooleanEnum.True)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.Or:
                    if (InstructionHelpers.GetBolleanValue(leftVertex) == BooleanEnum.True || InstructionHelpers.GetBolleanValue(rightVertex) == BooleanEnum.True)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.MoreThan:
                    if (Comparer<string>.Default.Compare(leftValue, rightValue) > 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.MoreOrEqualThan:
                    if (Comparer<string>.Default.Compare(leftValue, rightValue) >= 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.LessThan:
                    if (Comparer<string>.Default.Compare(leftValue, rightValue) < 0)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.LessOrEqualThan:
                    if (Comparer<string>.Default.Compare(leftValue, rightValue) <= 0)
                        output = true;
                    break;
            }

            return output;
        }

        private static INoInEdgeInOutVertexVertex LogicSingleOperator(LogicSingleOpertorEnum opetationType, ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);

            if (expression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _executeResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            IList<IEdge> executeResult = _executeResult.OutEdges;

            int toBeProcessedCount = executeResult.Count;

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            for (int x = 0; x < toBeProcessedCount; x++)
            {
                bool logicalResult = false;

                IVertex vertex = executeResult[x].To;

                logicalResult = LogicSingleOperator_VertexLevel(vertex, opetationType);

                if (logicalResult)
                    localStack.AddVertex(null, "True");
                else
                    localStack.AddVertex(null, "False");

            }

            return localStack;
        }

        enum LogicSingleOpertorEnum { Negation }

        private static bool LogicSingleOperator_VertexLevel(IVertex vertex, LogicSingleOpertorEnum operationType)
        {
            bool logicalResult = false;

            object number;

            GraphUtil.GetNumberValue(vertex, out number);

            if (number!=null)
            {
                switch (InstructionHelpers.GetNumericType(number))
                {
                    case InstructionHelpers.NumericTypeEnum.Integer:
                        int valInt = Convert.ToInt32(number);
                        logicalResult = LogicSingleOperator_ExecuteNumeric<int>(valInt, operationType, 0);
                        break;

                    case InstructionHelpers.NumericTypeEnum.Double:
                        double valDouble = Convert.ToDouble(number);
                        logicalResult = LogicSingleOperator_ExecuteNumeric<double>(valDouble, operationType, 0);
                        break;

                    case InstructionHelpers.NumericTypeEnum.Decimal:
                        decimal valDecimal = Convert.ToDecimal(number);
                        logicalResult = LogicSingleOperator_ExecuteNumeric<decimal>(valDecimal, operationType, 0);
                        break;
                }
            }
            else
                logicalResult = LogicSingleOperator_ExecuteString(vertex, operationType);

            return logicalResult;
        }

        private static bool LogicSingleOperator_ExecuteNumeric<T>(T value, LogicSingleOpertorEnum operationType, T zeroValue)
        {
            bool output = true;

            switch (operationType)
            {
                case LogicSingleOpertorEnum.Negation:
                    if (Comparer<T>.Default.Compare((T)value, zeroValue) > 0)                        
                        output = false;
                    break;
            }

            return output;
        }

        private static bool LogicSingleOperator_ExecuteString(IVertex vertex, LogicSingleOpertorEnum operationType)
        {
            bool output = false;

            if (vertex == null)
                return output;

            string value = vertex.Value.ToString();

            switch (operationType)
            {
                case LogicSingleOpertorEnum.Negation:
                    if (EqualityComparer<string>.Default.Equals(value,"False") ||
                        EqualityComparer<string>.Default.Equals(value, "false"))
                        output = true;
                    break;
            }

            return output;
        }

        public static INoInEdgeInOutVertexVertex Equal(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.Equal, exe, inputStack, instructionVertex, "True");
        }

        public static INoInEdgeInOutVertexVertex ExactEqual(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.ExactEqual, exe, inputStack, instructionVertex, "True");
        }

        public static INoInEdgeInOutVertexVertex NotEqual(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.NotEqual, exe, inputStack, instructionVertex, "False");
        }

        public static INoInEdgeInOutVertexVertex Negation(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicSingleOperator(LogicSingleOpertorEnum.Negation, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex And(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.And, exe, inputStack, instructionVertex, "False");
        }

        public static INoInEdgeInOutVertexVertex Or(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.Or, exe, inputStack, instructionVertex, "False");
        }

        public static INoInEdgeInOutVertexVertex MoreThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.MoreThan, exe, inputStack, instructionVertex, "False");
        }

        public static INoInEdgeInOutVertexVertex LessThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.LessThan, exe, inputStack, instructionVertex, "False");
        }

        public static INoInEdgeInOutVertexVertex MoreOrEqualThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.MoreOrEqualThan, exe, inputStack, instructionVertex, "True");
        }

        public static INoInEdgeInOutVertexVertex LessOrEqualThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.LessOrEqualThan, exe, inputStack, instructionVertex, "True");
        }

        #endregion


        ////////////////////////////////////////////////////////////////
        //
        // general operators
        //
        ////////////////////////////////////////////////////////////////

#region GeneralOperators

        public static INoInEdgeInOutVertexVertex Bracket(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GraphUtil.GetQueryOutFirst(instructionVertex, "Expression", null);
                //instructionVertex.Get(false, "Expression:");

            INoInEdgeInOutVertexVertex localStack = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            return InstructionHelpers.NextExpressionHandle(exe, localStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex Call(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex target = GraphUtil.GetQueryOutFirst(instructionVertex, "Target", null);
                //instructionVertex.Get(false, "Target:");

            if(!InstructionHelpers.CheckIs(target, "Function"))
            {
                INoInEdgeInOutVertexVertex targetExpressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, target); 
                if (targetExpressionExecution.Count() > 0)
                    target = targetExpressionExecution.OutEdges[0].To;
            }

            if (target == null)
                return exe.stack;

            exe.AddStackFrame(); // ENTER NEW STACK

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);
            //instructionVertex.GetAll(false, "Expression:");
            IList<IEdge> inputParameters = GraphUtil.GetQueryOut(target, "InputParameter", null);
                //target.GetAll(false, "InputParameter:");

            int minParameters = Math.Min(expressions.Count(), inputParameters.Count());

            for (int x=0; x < minParameters; x++)
            {
                IVertex expression = expressions[x].To;
                IVertex inputParameter = inputParameters[x].To;

                INoInEdgeInOutVertexVertex expressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, expression);

                foreach (IEdge e in expressionExecution)
                    exe.stack.AddEdge(inputParameter, e.To);
            }

            bool local_isStackFrameReturn;

            INoInEdgeInOutVertexVertex possibleToReturnStack = InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.stack, target, out local_isStackFrameReturn, false);

            exe.RemoveStackFrame(); // LEAVE NEW STACK

            if (local_isStackFrameReturn)
                return possibleToReturnStack;
            else
                return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Return(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = true;

            IVertex expression = GraphUtil.GetQueryOutFirst(instructionVertex, "Expression", null);
                //instructionVertex.Get(false, "Expression:");

            if (expression != null)
            {
                return exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex ForEach(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex variable = GraphUtil.GetQueryOutFirst(instructionVertex, "Variable", null);
            //instructionVertex.Get(false, "Variable:");
            IVertex set = GraphUtil.GetQueryOutFirst(instructionVertex, "Set", null); 
                //instructionVertex.Get(false, "Set:");

            if (variable!=null && set != null)
            {
                INoInEdgeInOutVertexVertex setExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, set);

                bool local_isStackFrameReturn = false;
                INoInEdgeInOutVertexVertex possibleToReturnStack = null;

                foreach (IEdge setEdge in setExecution)
                {
                    exe.AddStackFrame(); // ENTER NEW STACK

                    IEdge variableEdge = GraphUtil.CreateArtificialEdge(variable, setEdge.To);

                    exe.stack.AddEdgeForNoInEdgeInOutVertexVertex(variableEdge);

                    possibleToReturnStack = InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.stack, instructionVertex, out local_isStackFrameReturn, false);

                    if (local_isStackFrameReturn)
                        break;

                    exe.RemoveStackFrame();  // LEAVE NEW STACK
                }

                if (local_isStackFrameReturn)
                    return possibleToReturnStack;
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex While(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex test = GraphUtil.GetQueryOutFirst(instructionVertex, "Test", null); 
                //instructionVertex.Get(false, "Test:");

            if (test != null)
            {

                bool local_isStackFrameReturn = false;
                INoInEdgeInOutVertexVertex possibleToReturnStack = null;

                INoInEdgeInOutVertexVertex testResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, test);

                while (InstructionHelpers.IsTrue_Stack(testResult))
                {
                    exe.AddStackFrame(); // ENTER NEW STACK

                    possibleToReturnStack = InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.stack, instructionVertex, out local_isStackFrameReturn, false);

                    if (local_isStackFrameReturn)
                        break;


                    testResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, test);

                    exe.RemoveStackFrame(); // LEAVE NEW STACK
                }

                if (local_isStackFrameReturn)
                    return possibleToReturnStack;
            }

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex Link(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            IVertex target = GraphUtil.GetQueryOutFirst(instructionVertex, "Target", null);

            if (target != null)
                return exe.ExecuteInstructionByMontevideoPrinciples(inputStack, target);

            return InstructionHelpers.CreateStack();
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // stack operators
        //
        ////////////////////////////////////////////////////////////////

#region StackOperators

        public static INoInEdgeInOutVertexVertex CreateStackEdge(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex stack = exe.stack;

            int? minCardinality = GraphUtil.GetIntegerValue(instructionVertex.Get(false, "$MinCardinality:"));

            if(minCardinality != null)
                for(int x=0;x<minCardinality;x++)
                    stack.AddVertex(instructionVertex, "");
            else
                stack.AddVertex(instructionVertex, "");

            return stack;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // vertex creation operators
        //
        ////////////////////////////////////////////////////////////////

        #region VertexCreationOperators

        public static INoInEdgeInOutVertexVertex MetaToTo(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);

            if (expression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach(IEdge e in expressionResult)
            {
                newStack.AddEdge(null, e.Meta);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex CopySet(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);

            if (expression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in expressionResult)
                InstructionHelpers.CopyVertex(e, newStack);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex DoubleColonOperator(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            bool isExeStackSameAsExeNewVertexCreationSpace = false;

            if (exe.stack == exe.newVertexCreationSpace)
                isExeStackSameAsExeNewVertexCreationSpace = true;

            IVertex creationTarget = exe.newVertexCreationSpace;
            IVertex stackForNextExpression;

            if (isExeStackSameAsExeNewVertexCreationSpace)
                stackForNextExpression = InstructionHelpers.CreateStack();
            else
                stackForNextExpression = creationTarget;            

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult=null;
            if(leftExpression!= null)
                leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);

            INoInEdgeInOutVertexVertex rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            if(rightExecuteResult.OutEdges.Count > 0) // what about more than one edge in results
            {
                IVertex meta = null;

                if(leftExecuteResult!=null && leftExecuteResult.OutEdges.Count > 0)
                    meta = leftExecuteResult.OutEdges[0].To;

                foreach(IEdge e in rightExecuteResult)
                { 
                    creationTarget.AddEdge(meta, e.To);

                    if (isExeStackSameAsExeNewVertexCreationSpace)
                        stackForNextExpression.AddEdge(meta, e.To);
                }                
            }

            return InstructionHelpers.NextExpressionHandle(exe, stackForNextExpression, instructionVertex);            
        }

        public static INoInEdgeInOutVertexVertex InnerCreation(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            bool local_isStackFrameReturn = false;
            INoInEdgeInOutVertexVertex possibleToReturnStack = null;

            foreach (IEdge e in inputStack)
            {
                IVertex newVertexCreationSpace_copy = exe.newVertexCreationSpace;

                exe.newVertexCreationSpace = e.To;

                possibleToReturnStack = InstructionHelpers.SequentiallyExecuteInstructions(exe, 
                    exe.stack, instructionVertex, out local_isStackFrameReturn, false);

                exe.newVertexCreationSpace = newVertexCreationSpace_copy;

                if (local_isStackFrameReturn)
                    break;                
            }

            if (local_isStackFrameReturn)
                return possibleToReturnStack;

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // meta
        //
        ////////////////////////////////////////////////////////////////

        #region Meta

        public static INoInEdgeInOutVertexVertex Execute(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);

            if (expression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in expressionResult)
            {
                INoInEdgeInOutVertexVertex nestedExpressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, e.To);

                foreach (IEdge ee in nestedExpressionResult)
                    newStack.AddEdgeForNoInEdgeInOutVertexVertex(ee);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Parse(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);

            if (expression == null)
                return exe.stack;

            IVertex language;

            IVertex instuctionFormalTextLanguage = GraphUtil.GetQueryOutFirst(instructionVertex, "FormalTextLanguage", null);

            if (instuctionFormalTextLanguage == null)
                language = MinusZero.Instance.DefaultFormalTextLanguage;
            else
                language = InstructionHelpers.GetFirstExecutionEdge(exe, instuctionFormalTextLanguage).To;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in expressionResult)
            {
                IVertex newVertex = newStack.AddVertex(null, "");

                MinusZero.Instance.NewDefaultParser.Parse(language, newVertex, e.To.Value.ToString());
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Generate(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);

            if (expression == null)
                return exe.stack;

            IVertex language;

            IVertex instuctionFormalTextLanguage = GraphUtil.GetQueryOutFirst(instructionVertex, "FormalTextLanguage", null);

            if (instuctionFormalTextLanguage == null)
                language = MinusZero.Instance.DefaultFormalTextLanguage;
            else
                language = InstructionHelpers.GetFirstExecutionEdge(exe, instuctionFormalTextLanguage).To;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in expressionResult)
            {
                string parsed = MinusZero.Instance.DefaultCodeGenerator.Generate(language, e);

                newStack.AddVertex(null, parsed);
            }

            return newStack;
        }

        #endregion
    }
}

