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
        public static IVertex stepIntoAllEdges(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionsVertex)
        {
            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return newQs;
        }

        public static IVertex QueryOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            if(instructionVertex.Value==null)
                return inputQs;

            string value = instructionVertex.Value.ToString();

            if (value == "" || value == "\r")
                return inputQs;

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

        public static IVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            IVertex newQs = inputQs;
            IVertex oldQs = inputQs;

            foreach (IEdge expression in expressions)
            {
                INoInEdgeInOutVertexVertex _newQs = InstructionHelpers.CreateQueryStack();

                foreach(IEdge e in oldQs)
                {
                    IVertex outQs = exe.executeInstruction(e.To, expression.To);

                    if (outQs.OutEdges.Count() > 0)
                        _newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);
                }

                newQs = _newQs;
                oldQs = _newQs;
            }

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static IVertex StarOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex SlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static IVertex ColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
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

            return InstructionHelpers.NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static IVertex DoubleColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }        
    }
}
