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

        public delegate void AlgebraicVertexVisitor_EdgeEdge(IEdge leftEdge, IEdge rightEdge);

        public delegate void AlgebraicVertexVisitor_EdgeListOfEdges(IEdge leftEdge, IList<IEdge> rightEdges);

        public class AlgebraicVertexVisitor
        {
            ZeroCodeExecution exe;

            public AlgebraicVertexVisitor(ZeroCodeExecution _exe)
            {
                exe = _exe;
            }            

            public void RedirectEdgeToVertexIterator(IEdge leftEdge, IEdge rightEdge)
            {                
                leftEdge.From.AddEdge(leftEdge.Meta, rightEdge.To);
            }

            public void AddEdgesIterator(IEdge leftEdge, IList<IEdge> rightEdges)
            {
                foreach (IEdge e in rightEdges)
                    leftEdge.To.AddEdge(e.Meta, e.To);
            }

            public void DeleteEdgesIterator(IEdge leftEdge, IList<IEdge> rightEdges)
            {
                foreach (IEdge e in rightEdges)                    
                    leftEdge.To.DeleteEdge(e);
            }
        }

        public static void ZeroAlgebraLeftRightProcessor(ZeroCodeExecution exe, AlgebraicVertexVisitor_EdgeEdge visitor_EdgeEdge, AlgebraicVertexVisitor_EdgeListOfEdges visitor_EdgeListOfEdges, IVertex leftExpression, IVertex rightExpression, bool deleteLeftEdges)
        {
            bool CopyVertexValue = false;

            if (visitor_EdgeEdge == null && visitor_EdgeListOfEdges == null) // CopyVertexValue visitor is implemented in the ZeroAlgebraLeftRightProcessor body
                CopyVertexValue = true; // as there could be a need to create new vertexes if righExecuteResult.Count > l, and if we are creating new vertexes, they should have right value from the start, so need to do it here

            if (leftExpression != null && rightExpression != null)
            {
                INoInEdgeInOutVertexVertex leftExecuteResult = exe.executeInstruction(exe.stack, leftExpression);
                INoInEdgeInOutVertexVertex rightExecuteResult;

                if (leftExecuteResult != null)
                {
                    if (InstructionHelpers.CheckIsNewVertex(rightExpression))
                    {
                        rightExecuteResult = InstructionHelpers.CreateStack();
                        rightExecuteResult.AddEdgeForNoInEdgeInOutVertexVertex(GraphUtil.CreateArtificialEdge(null, rightExpression));
                    }
                    else
                    {
                        rightExecuteResult = exe.executeInstruction(exe.stack, rightExpression);
                    }

                    if (deleteLeftEdges)                    
                        foreach (IEdge e in leftExecuteResult)
                            e.From.DeleteEdge(e);                    

                    if(leftExecuteResult.Count() == 1)
                    {
                        IEdge leftExecuteResultFirst = leftExecuteResult.FirstOrDefault();

                        if (CopyVertexValue) {
                            //leftExecuteResultFirst.To.Value = 
                            if(rightExecuteResult.Count() > 1) { // need to create more left Edges
                                IVertex toAddVertex = leftExecuteResultFirst.From;

                              //  for (int x = 1; x <= rightExecuteResult.Count(); x++)
                                //    toAddVertex.a
                            }
                        }


                        /*foreach (var item in list1.Zip(list2, (a, b) => new { a, b }))
                        {
                            // use item.a and item.b
                        }*/

                     //   visitor_EdgeEdge?.Invoke()
                    } else
                    if (leftExecuteResult.Count() > 0)
                    {

                        IDictionary<EdgeKey_FromMeta, IList<IEdge>> dict = InstructionHelpers.CreateEdgeKey_FromMetaDictionary(leftExecuteResult);


                        if (rightExecuteResult.Count() == 1)
                        {
                            IVertex singleRightResult = rightExecuteResult.FirstOrDefault().To;

                            foreach (IEdge e in leftExecuteResult)
                                e.From.AddEdge(e.Meta, singleRightResult);

                        }
                    }
                }
            }
        }

        public static INoInEdgeInOutVertexVertex CopyVertexValue(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            AlgebraicVertexVisitor visitor = new AlgebraicVertexVisitor(exe);

            ZeroAlgebraLeftRightProcessor(exe, null, null, leftExpression, rightExpression, false);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex RedirectEdgeToVertex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            AlgebraicVertexVisitor visitor = new AlgebraicVertexVisitor(exe);

            ZeroAlgebraLeftRightProcessor(exe, visitor.RedirectEdgeToVertexIterator, null, leftExpression, rightExpression, true);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex AddEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            AlgebraicVertexVisitor visitor = new AlgebraicVertexVisitor(exe);

            ZeroAlgebraLeftRightProcessor(exe, null, visitor.AddEdgesIterator, leftExpression, rightExpression, false);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex DeleteEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IVertex leftExpression = InstructionHelpers.GetLeft(instructionVertex);
            IVertex rightExpression = InstructionHelpers.GetRight(instructionVertex);

            AlgebraicVertexVisitor visitor = new AlgebraicVertexVisitor(exe);

            ZeroAlgebraLeftRightProcessor(exe, null, visitor.DeleteEdgesIterator, leftExpression, rightExpression, false);

            return stack;
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
