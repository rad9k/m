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
        // O P E R A T O R S
        //
        ////////////////////////////////////////////////////////////////

        public static IVertex CopyValue(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex AddEdges(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex RemoveEdges(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        ////////////////////////////////////////////////////////////////
        //
        // V E R T E X   C R E A T I O N
        //
        ////////////////////////////////////////////////////////////////


        public static IVertex DoubleColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }        
    }
}
