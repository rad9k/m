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
        public static INoInEdgeInOutVertexVertex QueryOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            if (!(inputQs is INoInEdgeInOutVertexVertex))
                return null;

            if(instructionVertex.Value==null)
                return (INoInEdgeInOutVertexVertex)inputQs;

            string value = instructionVertex.Value.ToString();

            if (value == "" || value == "\r")
                return (INoInEdgeInOutVertexVertex)inputQs;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

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
            if (!(inputQs is INoInEdgeInOutVertexVertex))
                return null;

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex newQs = (INoInEdgeInOutVertexVertex) inputQs;
            INoInEdgeInOutVertexVertex oldQs = (INoInEdgeInOutVertexVertex) inputQs;

            foreach (IEdge expression in expressions)
            {
                newQs = InstructionHelpers.CreateQueryStack();

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

        public static INoInEdgeInOutVertexVertex StarOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return null;
        }

        public static INoInEdgeInOutVertexVertex SlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex ColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            string leftValue = GraphUtil.GetStringValue(leftExpression);
            string rightValue = GraphUtil.GetStringValue(rightExpression);

            string meta=null, to=null;

            if (leftValue != null && leftValue != "")
                meta = leftValue;

            if (rightValue != null && rightValue != "")
                to = rightValue;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

            IEdge e;
            IList<IEdge> eList;
            
            inputQs.QueryOutEdges(meta, to, out e, out eList);            

            if (e != null)
                newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

            if (eList != null)
                InstructionHelpers.AddToStack(eList, newQs);

           // newQs = ColonSubExpressionProcess_Meta(newQs, leftExpression);

            IVertex _newQs;

            if (rightExpression != null)
                _newQs = InstructionHelpers.NextExpressionHandle(exe, newQs, rightExpression);
            else
                _newQs = newQs;

            return InstructionHelpers.NextExpressionHandle(exe, _newQs, instructionVertex);
        }

        private static INoInEdgeInOutVertexVertex ColonSubExpressionProcess_Meta(INoInEdgeInOutVertexVertex inQs, IVertex expression)
        {
            IVertex nextExpression = InstructionHelpers.GetNextExpression(expression);

            if (nextExpression == null)
                return inQs;

            return null;
        }

        public static IVertex DoubleColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }        
    }
}
