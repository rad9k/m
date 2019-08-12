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

        public static INoInEdgeInOutVertexVertex QueryOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
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
                InstructionHelpers.AddToStack(eList, newQs);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex _inputQs = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex newQs = _inputQs;
            INoInEdgeInOutVertexVertex oldQs = _inputQs;

            foreach (IEdge expression in expressions)
            {
                newQs = InstructionHelpers.CreateStack();

                foreach (IEdge e in oldQs)
                {
                    IVertex outQs = exe.ExecuteInstruction(e.To, expression.To);

                    if (outQs.OutEdges.Count() > 0)
                        newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);
                }

                oldQs = newQs;
            }

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex QuestionMarkOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();
            GraphIterator iter = new GraphIterator(newQs);

            GraphUtil.DeepIterator(inputQs, iter.AddToINoInEdgeInOutVertexVertex, false, false, true);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex SlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex ColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
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
                    InstructionHelpers.AddToStack(eList, newQs);
            }
            else
                InstructionHelpers.AddToStack(inputQs, newQs);

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
                    newQs = exe.ExecuteInstruction(newQs, rightExpression);
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

            INoInEdgeInOutVertexVertex afterCallQs = exe.ExecuteInstruction(localQs, expression);

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

        // :=
        public static INoInEdgeInOutVertexVertex CopyVertexValue(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict = InstructionHelpers.CreateEdgeKey_FromMetaDictionary(leftExecuteResult);

            int rightCountMax = rightExecuteResult.Count() - 1;

            foreach (KeyValuePair<EdgeKey_FromMeta, IList<IEdge>> localLeft in leftFromMeta_dict)
            {
                int localLeftCountMax = localLeft.Value.Count() - 1;

                if (localLeftCountMax > rightCountMax)
                {
                    for (int x = rightCountMax + 1; x <= localLeftCountMax; x++)
                    {
                        IEdge toDelete = localLeft.Value[x];
                        toDelete.From.DeleteEdge(toDelete);
                    }

                    localLeftCountMax = rightCountMax;
                }

                for (int x = 0; x <= localLeftCountMax; x++)
                    localLeft.Value[x].To.Value = rightExecuteResult[x].To.Value;

                if (localLeftCountMax < rightCountMax)
                {
                    IEdge toAdd = localLeft.Value[0];

                    for (int x = localLeftCountMax + 1; x <= rightCountMax; x++)
                        toAdd.From.AddVertex(toAdd.Meta, rightExecuteResult[x].To.Value);
                }
            }

            return exe.stack;
        }

        // =
        public static INoInEdgeInOutVertexVertex RedirectLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

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
        public static INoInEdgeInOutVertexVertex AddLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

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
        public static INoInEdgeInOutVertexVertex AddRightEdgesIntoLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            foreach (IEdge leftEdge in leftExecuteResult)
                foreach (IEdge rightEdge in rightExecuteResult)
                    leftEdge.To.AddEdge(rightEdge.Meta, rightEdge.To);

            return exe.stack;
        }

        // ~=
        public static INoInEdgeInOutVertexVertex DeleteRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

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
        public static INoInEdgeInOutVertexVertex DeleteRightEdgesFromLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            IList<IEdge> rightResultMetaToEdgesList = InstructionHelpers.CreateEdgeKey_MetaToEdgesList(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
                leftEdge.To.DeleteEdgesList(rightResultMetaToEdgesList);

            return exe.stack;
        }

        // ~<
        public static INoInEdgeInOutVertexVertex DeleteRightVerticesFromLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

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

        public static INoInEdgeInOutVertexVertex EdgeSetAdd(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in leftExecuteResult)
                localStack.AddEdge(e.Meta, e.To);

            foreach (IEdge e in rightExecuteResult)
                localStack.AddEdge(e.Meta, e.To);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex EdgeSetSubstract(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = leftExecuteResult;

            leftExecuteResult.DeleteEdgesList(rightExecuteResult);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex SetIndex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {            
            IVertex expression = InstructionHelpers.GetExpression(instructionVertex);            

            if (expression == null)
                return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            INoInEdgeInOutVertexVertex executeResult = exe.ExecuteInstruction(exe.stack, expression);

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            foreach(IEdge e in executeResult)
            {
                int? index=GraphUtil.GetIntegerValue(e.To);

                if (index != null && index >=1 && index <= inputStack.OutEdges.Count())
                    localStack.AddEdgeForNoInEdgeInOutVertexVertex(inputStack.OutEdges[(int)index - 1]);
            }
            
            return localStack;
        }

            #endregion

        ////////////////////////////////////////////////////////////////
        //
        // number algebra operators
        //
        ////////////////////////////////////////////////////////////////

#region Operators

        public static INoInEdgeInOutVertexVertex Add(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.GetNumberListResult leftResultType;
            InstructionHelpers.GetNumberListResult rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.GetNumberListResult.Integer:
                    return _Add_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.GetNumberListResult.Double:
                    return _Add_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.GetNumberListResult.Decimal:
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

        public static INoInEdgeInOutVertexVertex Substract(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.GetNumberListResult leftResultType;
            InstructionHelpers.GetNumberListResult rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.GetNumberListResult.Integer:
                    return _Substract_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.GetNumberListResult.Double:
                    return _Substract_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.GetNumberListResult.Decimal:
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

        public static INoInEdgeInOutVertexVertex Multiply(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.GetNumberListResult leftResultType;
            InstructionHelpers.GetNumberListResult rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.GetNumberListResult.Integer:
                    return _Multiply_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.GetNumberListResult.Double:
                    return _Multiply_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.GetNumberListResult.Decimal:
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

        public static INoInEdgeInOutVertexVertex Divide(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            InstructionHelpers.GetNumberListResult leftResultType;
            InstructionHelpers.GetNumberListResult rightResultType;

            IList<object> leftNumbers = InstructionHelpers.GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = InstructionHelpers.GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return InstructionHelpers.CreateStackAndCopy(leftExecuteResult);

            switch (InstructionHelpers.GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case InstructionHelpers.GetNumberListResult.Integer:
                    return _Divide_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case InstructionHelpers.GetNumberListResult.Double:
                    return _Divide_Logic_double(leftNumbers, rightNumbers);

                case InstructionHelpers.GetNumberListResult.Decimal:
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

        private static INoInEdgeInOutVertexVertex LogicDoubleOperator(LogicDoubleOpertorEnum opetationType, ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstruction(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstruction(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            int toBeProcessedCount;

            if (leftExecuteResult.Count > rightExecuteResult.Count)
                toBeProcessedCount = rightExecuteResult.Count;
            else
                toBeProcessedCount = leftExecuteResult.Count;

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            for (int x=0; x < toBeProcessedCount; x++)
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

            return localStack;
        }

        enum LogicDoubleOpertorEnum {Equal, NotEqual, Negation, And, Or, MoreThan, LessThan, MoreOrEqualThan, LessOrEqualThan }

        private static bool LogicDoubleOperator_VertexLevel(IVertex leftVertex, IVertex rightVertex, LogicDoubleOpertorEnum operationType)
        {
            bool logicalResult = false;

            object leftNumber;
            object rightNumber;

            GraphUtil.GetNumberValue(leftVertex, out leftNumber);
            GraphUtil.GetNumberValue(rightVertex, out rightNumber);

            if (leftNumber != null && rightNumber != null)
            {
                switch (InstructionHelpers.GetCommonNubmerTypeDenominator(leftNumber, rightNumber))
                {
                    case InstructionHelpers.GetNumberListResult.Integer:
                        int leftInt = Convert.ToInt32(leftNumber);
                        int rightInt = Convert.ToInt32(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<int>(leftInt, rightInt, operationType, 0);
                        break;

                    case InstructionHelpers.GetNumberListResult.Double:
                        double leftDouble = Convert.ToDouble(leftNumber);
                        double rightDouble = Convert.ToDouble(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<double>(leftDouble, rightDouble, operationType, 0);
                        break;

                    case InstructionHelpers.GetNumberListResult.Decimal:
                        decimal leftDecimal = Convert.ToDecimal(leftNumber);
                        decimal rightDecimal = Convert.ToDecimal(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<decimal>(leftDecimal, rightDecimal, operationType, 0);
                        break;
                }
            }
            else
                logicalResult = GraphUtil.GetValueAndCompareStrings(leftVertex, rightVertex);

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

            }

            return output;
        }

        public static INoInEdgeInOutVertexVertex Equal(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.Equal, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex NotEqual(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.NotEqual, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex Negation(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return null;
        }

        public static INoInEdgeInOutVertexVertex And(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.And, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex Or(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.Or, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex MoreThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.MoreThan, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex LessThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.LessThan, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex MoreOrEqualThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.MoreOrEqualThan, exe, inputStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex LessOrEqualThan(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return LogicDoubleOperator(LogicDoubleOpertorEnum.LessOrEqualThan, exe, inputStack, instructionVertex);
        }

        #endregion


        ////////////////////////////////////////////////////////////////
        //
        // general operators
        //
        ////////////////////////////////////////////////////////////////

#region GeneralOperators

        public static INoInEdgeInOutVertexVertex Bracket(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex expression = instructionVertex.Get(false, "Expression:");

            return exe.ExecuteInstruction(inputStack, expression);
        }

        public static INoInEdgeInOutVertexVertex Call(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            IVertex target = instructionVertex.Get(false, "Target:");

            if(!InstructionHelpers.CheckIs(target, "Function"))
            {
                INoInEdgeInOutVertexVertex targetExpressionExecution = exe.ExecuteInstruction(exe.stack, target); 
                if (targetExpressionExecution.Count() > 0)
                    target = targetExpressionExecution.OutEdges[0].To;
            }

            if (target == null)
                return exe.stack;

            exe.AddStackFrame(newStack); // ENTER NEW STACK

            IVertex expressions = instructionVertex.GetAll(false, "Expression:");
            IVertex inputParameters = target.GetAll(false, "InputParameter:");

            for (int x=0; x < expressions.Count(); x++)
            {
                IVertex expression = expressions.OutEdges[x].To;
                IVertex inputParameter = inputParameters.OutEdges[x].To;

                INoInEdgeInOutVertexVertex expressionExecution = exe.ExecuteInstruction(exe.stack, expression);

                foreach (IEdge e in expressionExecution)
                    exe.stack.AddEdge(inputParameter, e.To);
            }


            InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.stack, target);

            exe.RemoveStackFrame(); // LEAVE NEW STACK

            return exe.stack;
        }

#endregion

        ////////////////////////////////////////////////////////////////
        //
        // stack operators
        //
        ////////////////////////////////////////////////////////////////

#region StackOperators

        public static INoInEdgeInOutVertexVertex CreateStackEdge(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
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
        // V E R T E X   C R E A T I O N
        //
        ////////////////////////////////////////////////////////////////

        public static INoInEdgeInOutVertexVertex DoubleColonOperator(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            return null;
        }        
    }
}
