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
        // Q U E R Y kindgdom
        //
        ////////////////////////////////////////////////////////////////
        
        public static INoInEdgeInOutVertexVertex QueryOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            if (instructionVertex.Value == null)
                return InstructionHelpers.MakeINoInEdgeInOutVertexVertex(inputQs);
            
            string value = instructionVertex.Value.ToString();

            if (value == "" || value == "\r")
                return InstructionHelpers.MakeINoInEdgeInOutVertexVertex(inputQs);

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            IEdge e;
            IList<IEdge> eList;

            if (exe.metaMode)                            
                inputQs.QueryOutEdges(value, null, out e, out eList);            
            else
                inputQs.QueryOutEdges(null, value, out e, out eList);

            if (e != null)
                newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

            if(eList != null)
                InstructionHelpers.AddToStack(eList, newQs);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex _inputQs = InstructionHelpers.MakeINoInEdgeInOutVertexVertex(inputQs);

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex newQs = _inputQs;
            INoInEdgeInOutVertexVertex oldQs = _inputQs;

            foreach (IEdge expression in expressions)
            {
                newQs = InstructionHelpers.CreateStack();

                foreach(IEdge e in oldQs)
                {
                    IVertex outQs = exe.executeInstruction(e.To, expression.To);

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

            GraphUtil.DeepIterator(inputQs,iter.AddToINoInEdgeInOutVertexVertex , false, false, true);

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
            
            if(leftExpression!=null)
                isLeftExpressionQuery = InstructionHelpers.CheckIs(leftExpression, "Query");

            if(rightExpression!=null)
                isRightExpressionQuery = InstructionHelpers.CheckIs(rightExpression, "Query");

            string leftValue = null;
            string rightValue = null;

            if(isLeftExpressionQuery)
                leftValue = GraphUtil.GetStringValue(leftExpression);

            if(isRightExpressionQuery)
                rightValue = GraphUtil.GetStringValue(rightExpression);

            string metaQueryString=null, toQueryString=null;

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
                    newQs = exe.executeInstruction(newQs, rightExpression);
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
            
            INoInEdgeInOutVertexVertex afterCallQs = exe.executeInstruction(localQs, expression);

            foreach (IEdge e in afterCallQs)
                metaDict[e.To] = true;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateStack();

            foreach (IEdge e in inQs)
                if (metaDict[e.Meta] == true)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

            return newQs;
        }

        ////////////////////////////////////////////////////////////////
        //
        // E D G E  O P E R A T O R S
        //
        ////////////////////////////////////////////////////////////////
 
        // :=
        public static INoInEdgeInOutVertexVertex CopyVertexValue(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict = InstructionHelpers.CreateEdgeKey_FromMetaDictionary(leftExecuteResult);

            int rightCountMax = rightExecuteResult.Count() - 1;

            foreach(KeyValuePair<EdgeKey_FromMeta, IList<IEdge>> localLeft in leftFromMeta_dict)
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

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

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

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

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

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

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

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

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

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

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

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            ISet<IEdge> rightResultToSet = InstructionHelpers.CreateEdgeKey_ToSet(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
                foreach(IEdge intoLeftEdge in leftEdge.To.ToList<IEdge>())
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

        ////////////////////////////////////////////////////////////////
        //
        // edge set operators
        //
        ////////////////////////////////////////////////////////////////

        public static INoInEdgeInOutVertexVertex EdgeSetAdd(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

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

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = leftExecuteResult;

            leftExecuteResult.DeleteEdgesList(rightExecuteResult);            

            return localStack ;
        }

        ////////////////////////////////////////////////////////////////
        //
        // O P E R A T O R S
        //
        ////////////////////////////////////////////////////////////////

        public static INoInEdgeInOutVertexVertex Add(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = InstructionHelpers.CreateStack();

            int? left = GraphUtil.GetIntegerValue(leftExecuteResult[0].To);
            int? right = GraphUtil.GetIntegerValue(rightExecuteResult[0].To);

            localStack.AddVertex(null, left + right);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex Substract(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            return null;
        }

        public static INoInEdgeInOutVertexVertex Multiply(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            return null;
        }

        public static INoInEdgeInOutVertexVertex Divide(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            return null;
        }

        ////////////////////////////////////////////////////////////////
        //
        // StackFrameCreator
        //
        ////////////////////////////////////////////////////////////////

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
