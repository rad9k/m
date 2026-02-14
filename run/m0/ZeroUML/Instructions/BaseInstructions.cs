using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static m0.ZeroCode.Helpers.InstructionHelpers;

namespace m0.ZeroUML.Instructions
{
    public class BaseInstructions
    {
        static IVertex r = MinusZero.Instance.Root;

        static IVertex thisMeta = r.Get(false, @"System\Meta\ZeroUML\this");
        static IVertex isMeta = r.Get(false, @"System\Meta\Base\Vertex\$Is");

        static IVertex functionTarget_meta = r.Get(false, @"System\Meta\ZeroUML\FunctionCall\Target");

        static IVertex dolarGraphChangeTriggerMeta = r.Get(false, @"System\Meta\Base\Vertex\$GraphChangeTrigger");
        static IVertex graphChangeTriggerMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger");
        static IVertex graphChangeTrigger_ScopeQueryMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\ScopeQuery");
        static IVertex graphChangeTrigger_ChageTypeFilterMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\ChangeTypeFilter");
        static IVertex graphChangeTrigger_ListenerMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\Listener");

        static IVertex viewMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View");
        static IVertex view_FromTriggerQueryMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View\FromTriggerQuery");
        static IVertex view_FromTriggerFilterMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View\FromTriggerFilter");
        static IVertex view_FromToTransformFunctionMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View\FromToTransformFunction");
        static IVertex view_ToTriggerQueryMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View\ToTriggerQuery");
        static IVertex view_ToTriggerFilterMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View\ToTriggerFilter");
        static IVertex view_ToFromTransformFunctionMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\View\ToFromTransformFunction");

        static IVertex httpMappingMeta = r.Get(false, @"System\Lib\Net\HttpMapping");
        static IVertex httpMappingEntryMeta = r.Get(false, @"System\Lib\Net\HttpMappingEntry");
        static IVertex httpMappingEntry_ActionMeta = r.Get(false, @"System\Lib\Net\HttpMappingEntry\Action");
        static IVertex httpMappingEntry_PathMaskMeta = r.Get(false, @"System\Lib\Net\HttpMappingEntry\PathMask");
        static IVertex httpMappingEntry_HandlerMeta = r.Get(false, @"System\Lib\Net\HttpMappingEntry\Handler");


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
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            string value = instructionVertex.Value.ToString();

            if (value == "" || value == "\r")
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            INoInEdgeInOutVertexVertex newQs = CreateStack();

            IEdge e;
            IList<IEdge> eList;

            bool queryByVariable;
            IList<string> processedValueList = processQueryValue(exe, value, out queryByVariable);


            foreach (string processedValue in processedValueList)
            {
                if (queryByVariable)
                {
                    if (processedValue.Length > 0 && processedValue[0]==':')
                        inputQs.QueryOutEdges(null, processedValue.Substring(1), out e, out eList);
                    else
                        inputQs.QueryOutEdges(processedValue, null, out e, out eList);                                            
                } else { 
                    if (exe.MetaMode)
                        inputQs.QueryOutEdges(processedValue, null, out e, out eList);
                    else
                        inputQs.QueryOutEdges(null, processedValue, out e, out eList);
                }

                if (e != null)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);

                if (eList != null)
                    AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(newQs, eList);
            }

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        private static List<string> processQueryValue(ZeroCodeExecution exe, string value)
        {
            bool queryByVariable;
            return processQueryValue(exe, value, out queryByVariable);
        }

        private static List<string> processQueryValue(ZeroCodeExecution exe, string value, out bool queryByVariable)
        {
            queryByVariable = false;

            List<string> retList = new List<string>();

            if (value == null)
                return retList;

            if (value == "")
            {
                retList.Add("");
                return retList;
            }

            if (value.Length > 2 && value[0] == '(' && value[value.Length - 1] == ')')
            {
                string expression = value.Substring(1, value.Length - 2);

                IEnumerable<IEdge> stackQueryResult = exe.Stack.GetAll(exe.MetaMode, expression);

                foreach (IEdge e in stackQueryResult)
                    if (e.To.Value != null)
                        retList.Add(e.To.Value.ToString());

                queryByVariable = true;
            }
            else
                retList.Add(value);

            return retList;
        }

        public static INoInEdgeInOutVertexVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex _inputQs = Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex newQs = _inputQs;
            INoInEdgeInOutVertexVertex oldQs = _inputQs;

            foreach (IEdge expression in expressions)
            {
                newQs = CreateStack();

                foreach (IEdge e in oldQs)
                {
                    IVertex outQs = exe.ExecuteInstructionByMontevideoPrinciples(e.To, expression.To);

                    if (outQs.OutEdges.Count() > 0)
                        newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);
                }

                oldQs = newQs;
            }

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex QuestionMarkOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newQs = CreateStack();
            GraphIterator iter = new GraphIterator(newQs);

            GraphUtil.DeepIterator(inputQs, iter.AddToINoInEdgeInOutVertexVertex, false, false, true);

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex SlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newQs = CreateStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(ee);

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex InEdgesSlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newQs = CreateStack();

            bool isFirstOperatorInExpression = true;

            foreach (IEdge e in instructionVertex.InEdges)
            {
                IList<IEdge> metaIsValuesList = GraphUtil.GetQueryOut(e.From, "$Is", null);

                foreach (IEdge ee in metaIsValuesList)
                    if (GeneralUtil.CompareStrings(ee.To, new string[] { "Query", "{}", "Colon", "\\ ", "?", "InEdgesSlash" }))
                        isFirstOperatorInExpression = false;
            }

            if (isFirstOperatorInExpression)
            {
                IList<IVertex> fromVertexList = new List<IVertex>();

                foreach (IEdge e in inputQs)
                    if (!fromVertexList.Contains(e.From) && e.From != null)
                        fromVertexList.Add(e.From);

                foreach (IVertex e in fromVertexList)
                    foreach (IEdge ee in e.InEdges)
                        newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(GraphUtil.CreateArtificialEdge(ee.Meta, ee.From));
            }
            else
            {
                foreach (IEdge e in inputQs)
                    foreach (IEdge ee in e.To.InEdges)
                        newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(GraphUtil.CreateArtificialEdge(ee.Meta, ee.From));
            }

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex ColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            bool isLeftExpressionQuery = false;
            bool isRightExpressionQuery = false;

            if (leftExpression != null)
                isLeftExpressionQuery = CheckIfIs(leftExpression, "Query");

            if (rightExpression != null)
                isRightExpressionQuery = CheckIfIs(rightExpression, "Query");

            string leftValue = null;
            string rightValue = null;

            if (isLeftExpressionQuery)
                leftValue = GraphUtil.GetStringValue(leftExpression);

            if (isRightExpressionQuery)
                rightValue = GraphUtil.GetStringValue(rightExpression);

            string metaQueryString = null, toQueryString = null;
            IList<string> processedMetaQueryStrings = null, processedToQueryStrings = null;


            if (leftValue != null && leftValue != "")
            {
                metaQueryString = leftValue;
                processedMetaQueryStrings = processQueryValue(exe, metaQueryString);
            }
            else
            {
                processedMetaQueryStrings = new List<string>();
                processedMetaQueryStrings.Add(null);
            }

            if (rightValue != null && rightValue != "")
            {
                toQueryString = rightValue;
                processedToQueryStrings = processQueryValue(exe, toQueryString);
            }
            else
            {
                processedToQueryStrings = new List<string>();
                processedToQueryStrings.Add(null);
            }

            INoInEdgeInOutVertexVertex newQs = CreateStack();

            if (isLeftExpressionQuery || isRightExpressionQuery)
            {
                IEdge e;
                IList<IEdge> eList;

                foreach (string processedToString in processedToQueryStrings)
                    foreach (string processedMetaString in processedMetaQueryStrings)
                    {
                        inputQs.QueryOutEdges(processedMetaString, processedToString, out e, out eList);

                        if (e != null)
                            newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);

                        if (eList != null)
                            AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(newQs, eList);
                    }
            }
            else
                AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(newQs, inputQs);

            if (leftExpression != null)
            {
                if (isLeftExpressionQuery)
                {
                    IVertex nextExpression = GetNextExpression(leftExpression);

                    if (nextExpression != null)
                        newQs = ColonSubExpressionProcess_Meta(exe, newQs, nextExpression);
                } else
                    newQs = ColonSubExpressionProcess_Meta(exe, newQs, leftExpression);
            }

            if (rightExpression != null)
            {
                if (isRightExpressionQuery)
                    newQs = NextExpressionHandle(exe, newQs, rightExpression);
                else
                    newQs = exe.ExecuteInstructionByMontevideoPrinciples(newQs, rightExpression);
            }

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        private static INoInEdgeInOutVertexVertex ColonSubExpressionProcess_Meta(ZeroCodeExecution exe, INoInEdgeInOutVertexVertex inQs, IVertex expression)
        {
            Dictionary<IVertex, bool> metaDict = new Dictionary<IVertex, bool>();

            INoInEdgeInOutVertexVertex localQs = CreateStack();

            foreach (IEdge e in inQs)
                if (!metaDict.ContainsKey(e.Meta))
                {
                    localQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(GraphUtil.CreateArtificialEdge(null, e.Meta));
                    metaDict.Add(e.Meta, false);
                }

            INoInEdgeInOutVertexVertex afterCallQs = exe.ExecuteInstructionByMontevideoPrinciples(localQs, expression);

            foreach (IEdge e in afterCallQs)
                metaDict[e.To] = true;

            INoInEdgeInOutVertexVertex newQs = CreateStack();

            foreach (IEdge e in inQs)
                if (metaDict[e.Meta] == true)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);

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

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            bool leftPropagateToStackExpression = CheckIfIsInherits_WRONG(leftExpression, "PropagateToStackExpression");

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;

            // left

            INoInEdgeInOutVertexVertex leftStack = CreateStack();

            exe.NewVertexCreationSpace = leftStack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);            

            // right

            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            // NEW

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict = CreateEdgeKey_FromMetaDictionary(leftExecuteResult);

            foreach (KeyValuePair<EdgeKey_FromMeta, IList<IEdge>> localLeft in leftFromMeta_dict)
            {
                IEdge toAdd = localLeft.Value[0];

                toAdd.From.DeleteEdgesList(localLeft.Value);

                foreach (IEdge e in rightExecuteResult)
                    //if (leftPropagateToStackExpression && exe.stack == exe.newVertexCreationSpace) // left expression was separated from exe.stack
                    if (leftPropagateToStackExpression /*&& exe.stack == exe.newVertexCreationSpace*/) // XXX EXPERIMENTA !!!! for issue 84
                        exe.Stack.AddEdge(toAdd.Meta, e.To);
                    else
                        toAdd.From.AddEdge(toAdd.Meta, e.To);
            }

            return exe.Stack;
        }

        // +=
        public static INoInEdgeInOutVertexVertex AddLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);

            //INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;
            // NEW

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict = CreateEdgeKey_FromMetaDictionary(leftExecuteResult);

            foreach (KeyValuePair<EdgeKey_FromMeta, IList<IEdge>> localLeft in leftFromMeta_dict)
            {
                IEdge toAdd = localLeft.Value[0];

                foreach (IEdge e in rightExecuteResult)
                    toAdd.From.AddEdge(toAdd.Meta, e.To);
            }

            return exe.Stack;
        }

        // +<
        public static INoInEdgeInOutVertexVertex AddRightEdgesIntoLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);

            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            foreach (IEdge leftEdge in leftExecuteResult)
                foreach (IEdge rightEdge in rightExecuteResult)
                    leftEdge.To.AddEdge(rightEdge.Meta, rightEdge.To);

            return exe.Stack;
        }

        // ~=
        public static INoInEdgeInOutVertexVertex DeleteRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);
            //INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;
            // NEW

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            ISet<IEdge> rightResultToSet = CreateEdgeKey_ToSet(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
            {
                HashSet<IEdge> usedEdges = new HashSet<IEdge>();

                foreach (IEdge rightEdge in rightResultToSet)
                    if (leftEdge.To == rightEdge.To && !usedEdges.Contains(rightEdge))
                    {
                        usedEdges.Add(rightEdge);
                        leftEdge.From.DeleteEdge(leftEdge);
                    }
            }

            return exe.Stack;
        }

        // -<
        public static INoInEdgeInOutVertexVertex DeleteRightEdgesFromLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);
            //INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);
            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;
            // NEW

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            IList<IEdge> rightResultMetaToEdgesList = CreateEdgeKey_MetaToEdgesList(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
                leftEdge.To.DeleteEdgesList(rightResultMetaToEdgesList);

            return exe.Stack;
        }

        // ~<
        public static INoInEdgeInOutVertexVertex DeleteRightVerticesFromLeftEdges(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);
            //INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);
            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;
            // NEW

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;

            ISet<IEdge> rightResultToSet = CreateEdgeKey_ToSet(_rightExecuteResult);

            foreach (IEdge leftEdge in leftExecuteResult)
            {
                HashSet<IEdge> usedEdges = new HashSet<IEdge>();

                foreach (IEdge intoLeftEdge in leftEdge.To.ToList<IEdge>())
                    foreach (IEdge rightEdge in rightResultToSet)
                        if (intoLeftEdge.To == rightEdge.To && !usedEdges.Contains(rightEdge))
                        {
                            usedEdges.Add(rightEdge);
                            intoLeftEdge.From.DeleteEdge(intoLeftEdge);
                        }
            }

            return exe.Stack;
        }

        // <-
        public static INoInEdgeInOutVertexVertex SetLeftVertexesToFirstRightVertexValue(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);
            //INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);
            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;
            // NEW

            if (_rightExecuteResult.OutEdges.Count > 0)
            {
                IVertex FirstRightVertex = _rightExecuteResult.OutEdges[0].To;

                foreach (IEdge e in leftExecuteResult)
                    e.To.Value = FirstRightVertex.Value;
            }

            return exe.Stack;
        }

        // <+<
        public static INoInEdgeInOutVertexVertex AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (_leftExecuteResult.Count() > 0)
            {
                IVertex leftExecuteFirstVertex = _leftExecuteResult.OutEdges[0].To;
                //IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges; XXX

                ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_NoLinksNoBootstrap(_rightExecuteResult, leftExecuteFirstVertex);
            }

            return exe.Stack;
        }

        // <<<
        public static INoInEdgeInOutVertexVertex AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (_leftExecuteResult.Count() > 0)
            {
                IVertex leftExecuteFirstVertex = _leftExecuteResult.OutEdges[0].To;
                //IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges; XXX

                ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_NoBootstrap(_rightExecuteResult, leftExecuteFirstVertex);
            }

            return exe.Stack;
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

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            foreach (IEdge e in leftExecuteResult)
                localStack.AddEdge(e.Meta, e.To);

            foreach (IEdge e in rightExecuteResult)
                localStack.AddEdge(e.Meta, e.To);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex EdgeSetSubstract(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            INoInEdgeInOutVertexVertex localStack = leftExecuteResult;

            leftExecuteResult.DeleteEdgesList(rightExecuteResult);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex SetIndex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            INoInEdgeInOutVertexVertex executeResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, expression);

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            foreach (IEdge e in executeResult)
            {
                int? index = GraphUtil.GetIntegerValue(e.To);

                if (index != null && index >= 1 && index <= inputStack.OutEdges.Count())
                    localStack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(inputStack.OutEdges[(int)index - 1]);
            }

            return NextExpressionHandle(exe, localStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex SetCount(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            localStack.AddVertex(null, inputStack.OutEdges.Count());

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex EmptySet(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

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

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            NumericTypeEnum leftResultType;
            NumericTypeEnum rightResultType;

            IList<object> leftNumbers = GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return CreateStackAndCopy(leftExecuteResult);

            switch (GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case NumericTypeEnum.Integer:
                    return _Add_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case NumericTypeEnum.Double:
                    return _Add_Logic_double(leftNumbers, rightNumbers);

                case NumericTypeEnum.Decimal:
                    return _Add_Logic_decimal(leftNumbers, rightNumbers);
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        static INoInEdgeInOutVertexVertex _Add_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            NumericTypeEnum leftResultType;
            NumericTypeEnum rightResultType;

            IList<object> leftNumbers = GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return CreateStackAndCopy(leftExecuteResult);

            switch (GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case NumericTypeEnum.Integer:
                    return _Substract_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case NumericTypeEnum.Double:
                    return _Substract_Logic_double(leftNumbers, rightNumbers);

                case NumericTypeEnum.Decimal:
                    return _Substract_Logic_decimal(leftNumbers, rightNumbers);
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }
        static INoInEdgeInOutVertexVertex _Substract_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            NumericTypeEnum leftResultType;
            NumericTypeEnum rightResultType;

            IList<object> leftNumbers = GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return CreateStackAndCopy(leftExecuteResult);

            switch (GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case NumericTypeEnum.Integer:
                    return _Multiply_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case NumericTypeEnum.Double:
                    return _Multiply_Logic_double(leftNumbers, rightNumbers);

                case NumericTypeEnum.Decimal:
                    return _Multiply_Logic_decimal(leftNumbers, rightNumbers);
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        static INoInEdgeInOutVertexVertex _Multiply_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            NumericTypeEnum leftResultType;
            NumericTypeEnum rightResultType;

            IList<object> leftNumbers = GetNumberList(leftExecuteResult, out leftResultType);
            IList<object> rightNumbers = GetNumberList(rightExecuteResult, out rightResultType);

            if (leftNumbers.Count == 0)
                return CreateStackAndCopy(rightExecuteResult);

            if (rightNumbers.Count == 0)
                return CreateStackAndCopy(leftExecuteResult);

            switch (GetCommonNubmerResultDenominator(leftResultType, rightResultType))
            {
                case NumericTypeEnum.Integer:
                    return _Divide_Logic_int(leftNumbers, rightNumbers); // can not use generics when doing T + T

                case NumericTypeEnum.Double:
                    return _Divide_Logic_double(leftNumbers, rightNumbers);

                case NumericTypeEnum.Decimal:
                    return _Divide_Logic_decimal(leftNumbers, rightNumbers);
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        static INoInEdgeInOutVertexVertex _Divide_Logic_int(IList<object> leftNumbers, IList<object> rightNumbers)
        {
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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
            INoInEdgeInOutVertexVertex localStack = CreateStack();

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

        private static INoInEdgeInOutVertexVertex LogicDoubleOperator(LogicDoubleOpertorEnum operationType, ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, String leftAndRightResultsEmptyOperatorResult)
        {
            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);
            INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
            IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

            int toBeProcessedCount;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            if (operationType == LogicDoubleOpertorEnum.ExactEqual &&
                leftExecuteResult.Count != rightExecuteResult.Count)
            {
                localStack.AddVertex(null, "False");
                return localStack;
            }

            if (leftExecuteResult.Count > rightExecuteResult.Count)
                toBeProcessedCount = rightExecuteResult.Count;
            else
                toBeProcessedCount = leftExecuteResult.Count;

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

                    logicalResult = LogicDoubleOperator_VertexLevel(leftVertex, rightVertex, operationType);

                    if (logicalResult)
                        localStack.AddVertex(null, "True");
                    else
                        localStack.AddVertex(null, "False");

                }
            }

            return localStack;
        }

        enum LogicDoubleOpertorEnum { Equal, ExactEqual, VertexEqual, NotEqual, And, Or, MoreThan, LessThan, MoreOrEqualThan, LessOrEqualThan }

        private static bool LogicDoubleOperator_VertexLevel(IVertex leftVertex, IVertex rightVertex, LogicDoubleOpertorEnum operationType)
        {
            bool logicalResult = false;

            if (operationType == LogicDoubleOpertorEnum.VertexEqual)
            {
                if (leftVertex == rightVertex)
                    return true;
                else
                    return false;
            }

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
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<int>(leftInt, rightInt, operationType, 0);
                        break;

                    case NumericTypeEnum.Double:
                        double leftDouble = Convert.ToDouble(leftNumber);
                        double rightDouble = Convert.ToDouble(rightNumber);
                        logicalResult = LogicDoubleOperator_ExecuteNumeric<double>(leftDouble, rightDouble, operationType, 0);
                        break;

                    case NumericTypeEnum.Decimal:
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
                    if (Comparer<T>.Default.Compare(leftValue, rightValue) >= 0)
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

                        (GetBolleanValue(leftVertex) == BooleanEnum.True && GetBolleanValue(rightVertex) == BooleanEnum.True) || // true true

                        (GetBolleanValue(leftVertex) == BooleanEnum.False && GetBolleanValue(rightVertex) == BooleanEnum.False) // false false
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
                    if (
                        !EqualityComparer<string>.Default.Equals(leftValue, rightValue) ||

                        (GetBolleanValue(leftVertex) == BooleanEnum.True && GetBolleanValue(rightVertex) == BooleanEnum.False) || // true false

                        (GetBolleanValue(leftVertex) == BooleanEnum.False && GetBolleanValue(rightVertex) == BooleanEnum.True) // false true
                        )

                        output = true;
                    break;

                case LogicDoubleOpertorEnum.And:
                    if (GetBolleanValue(leftVertex) == BooleanEnum.True && GetBolleanValue(rightVertex) == BooleanEnum.True)
                        output = true;
                    break;

                case LogicDoubleOpertorEnum.Or:
                    if (GetBolleanValue(leftVertex) == BooleanEnum.True || GetBolleanValue(rightVertex) == BooleanEnum.True)
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
            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex _executeResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            IList<IEdge> executeResult = _executeResult.OutEdges;

            int toBeProcessedCount = executeResult.Count;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

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

            if (number != null)
            {
                switch (GetNumericType(number))
                {
                    case NumericTypeEnum.Integer:
                        int valInt = Convert.ToInt32(number);
                        logicalResult = LogicSingleOperator_ExecuteNumeric<int>(valInt, operationType, 0);
                        break;

                    case NumericTypeEnum.Double:
                        double valDouble = Convert.ToDouble(number);
                        logicalResult = LogicSingleOperator_ExecuteNumeric<double>(valDouble, operationType, 0);
                        break;

                    case NumericTypeEnum.Decimal:
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
                    if (EqualityComparer<string>.Default.Equals(value, "False") ||
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

        public static INoInEdgeInOutVertexVertex VertexEqual(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return LogicDoubleOperator(LogicDoubleOpertorEnum.VertexEqual, exe, inputStack, instructionVertex, "True");
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

            return NextExpressionHandle(exe, localStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex FunctionCall(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex target = GraphUtil.GetQueryOutFirst(instructionVertex, "Target", null);

            if (!CheckIfIs(target, "Function"))
            {
                INoInEdgeInOutVertexVertex targetExpressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, target);

                // currently only one target BUT we can have set! of targets. to support this need to implement
                // name based call params passing instead of only index based
                if (targetExpressionExecution.Count() > 0) 
                    target = targetExpressionExecution.OutEdges[0].To;
            }

            if (target == null)
                return exe.Stack;

            exe.AddStackFrame(); // ENTER NEW STACK

            exe.Stack.AddEdge(functionTarget_meta, target); // to be able to know the function target vertex in the function body

            IList<IEdge> expressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);
            IList<IEdge> inputParameters = GraphUtil.GetQueryOut(target, "InputParameter", null);

            int minParameters = Math.Min(expressions.Count(), inputParameters.Count());

            for (int x = 0; x < minParameters; x++)
            {
                IVertex expression = expressions[x].To;
                IVertex inputParameter = inputParameters[x].To;

                INoInEdgeInOutVertexVertex expressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, expression);

                foreach (IEdge e in expressionExecution)
                    exe.Stack.AddEdge(inputParameter, e.To);
            }

            //bool local_isStackFrameReturn;

            //INoInEdgeInOutVertexVertex possibleToReturnStack = SequentiallyExecuteInstructions(exe, exe.stack, target, out local_isStackFrameReturn, false);

            INoInEdgeInOutVertexVertex toReturnStack = target.Execute(exe);

            exe.RemoveStackFrame(); // LEAVE NEW STACK

            //if (local_isStackFrameReturn)
            
            //return toReturnStack; want to have []\

            return NextExpressionHandle(exe, toReturnStack, instructionVertex); // []\ worx
            
            //else
            //   return CreateStack();
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

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex ForVertex(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex variable = GraphUtil.GetQueryOutFirst(instructionVertex, "Variable", null);
            IVertex set = GraphUtil.GetQueryOutFirst(instructionVertex, "Set", null);

            if (variable != null && set != null)
            {
                INoInEdgeInOutVertexVertex setExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, set);

                bool local_isStackFrameReturn = false;
                INoInEdgeInOutVertexVertex possibleToReturnStack = null;

                foreach (IEdge setEdge in setExecution)
                {
                    exe.AddStackFrame(); // ENTER NEW STACK

                    IEdge variableEdge = GraphUtil.CreateArtificialEdge(variable, setEdge.To);

                    exe.Stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(variableEdge);

                    possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                        break;

                    exe.RemoveStackFrame();  // LEAVE NEW STACK
                }

                if (local_isStackFrameReturn)
                    return possibleToReturnStack;
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex ForEdge(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex variable = GraphUtil.GetQueryOutFirst(instructionVertex, "Variable", null);
            IVertex set = GraphUtil.GetQueryOutFirst(instructionVertex, "Set", null);

            if (variable != null && set != null)
            {
                INoInEdgeInOutVertexVertex setExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, set);

                bool local_isStackFrameReturn = false;
                INoInEdgeInOutVertexVertex possibleToReturnStack = null;

                foreach (IEdge setEdge in setExecution)
                {
                    exe.AddStackFrame(); // ENTER NEW STACK

                    IEdge variableEdge = GraphUtil.CreateArtificialEdge(variable, EdgeHelper.CreateTempEdgeVertex(setEdge));

                    exe.Stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(variableEdge);

                    possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                        break;

                    exe.RemoveStackFrame();  // LEAVE NEW STACK
                }

                if (local_isStackFrameReturn)
                    return possibleToReturnStack;
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
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

                INoInEdgeInOutVertexVertex testResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, test);

                while (IsTrue_Stack(testResult))
                {
                    exe.AddStackFrame(); // ENTER NEW STACK

                    possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                        break;


                    testResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, test);

                    exe.RemoveStackFrame(); // LEAVE NEW STACK
                }

                if (local_isStackFrameReturn)
                    return possibleToReturnStack;
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex Link(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            IVertex target = GraphUtil.GetQueryOutFirst(instructionVertex, "Target", null);

            if (target != null)
                return exe.ExecuteInstructionByMontevideoPrinciples(inputStack, target);

            return CreateStack();
        }

        public static INoInEdgeInOutVertexVertex Block(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            return SequenciallyExecuteIntructionsWithNewStackAndIsStackFrameReturnSupport(exe, inputStack, instructionVertex, out isStackFrameReturn);
        }

        public static INoInEdgeInOutVertexVertex If(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex test = GraphUtil.GetQueryOutFirst(instructionVertex, "Test", null);

            if (test == null)
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            INoInEdgeInOutVertexVertex testExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, test);

            if (IsTrue_Stack(testExecution))
                return SequenciallyExecuteIntructionsWithNewStackAndIsStackFrameReturnSupport(exe, inputStack, instructionVertex, out isStackFrameReturn);

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex Test(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GraphUtil.GetQueryOutFirst(instructionVertex, "Expression", null);

            if (expression == null)
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            INoInEdgeInOutVertexVertex expressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, expression);

            if (expressionExecution.OutEdges.Count == 0)
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            IVertex firstExpression = expressionExecution.OutEdges[0].To;

            IList<IEdge> cases = GraphUtil.GetQueryOut(instructionVertex, "Case", null);

            foreach (IEdge _case in cases)
            {
                IVertex test = GraphUtil.GetQueryOutFirst(_case.To, "Test", null);

                if (test != null)
                    if (CompareVertexValues(firstExpression, test))
                        return SequenciallyExecuteIntructionsWithNewStackAndIsStackFrameReturnSupport(exe, inputStack, _case.To, out isStackFrameReturn);
            }

            IVertex _fallback = GraphUtil.GetQueryOutFirst(instructionVertex, "Fallback", null);

            if (_fallback != null)
                return SequenciallyExecuteIntructionsWithNewStackAndIsStackFrameReturnSupport(exe, inputStack, _fallback, out isStackFrameReturn);

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
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

            INoInEdgeInOutVertexVertex stack = exe.Stack;

            int? minCardinality = GraphUtil.GetIntegerValue(instructionVertex.Get(false, "$MinCardinality:"));

            if (minCardinality != null)
                for (int x = 0; x < minCardinality; x++)
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

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
            {
                newStack.AddEdge(null, e.Meta);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex CopySet(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
                VertexOperations.CopyVertex(e, newStack);

            return newStack;
        }        

        // old, stackForNextExpression based version is in int the DoubleSemicolonOperator below
        public static INoInEdgeInOutVertexVertex DoubleColonOperator(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex creationTarget = exe.NewVertexCreationSpace;

            INoInEdgeInOutVertexVertex additionalCreationStack = null;

            if (exe.Stack == creationTarget)
                additionalCreationStack = CreateStack();

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            INoInEdgeInOutVertexVertex leftExecuteResult = null;
            if (leftExpression != null)
                leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);

            INoInEdgeInOutVertexVertex rightExecuteResult;

            if (rightExpression == null)
            {
                rightExecuteResult = CreateStack();
                rightExecuteResult.AddEdge(null, null); // will generate MinusZero.Instance.Empty
            }
            else
                rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            INoInEdgeInOutVertexVertex toReturn = null;

            if (rightExecuteResult.OutEdges.Count > 0) // what about more than one edge in results
            {
                IVertex meta = null;

                if (leftExecuteResult != null && leftExecuteResult.OutEdges.Count > 0)
                    meta = leftExecuteResult.OutEdges[0].To;

                foreach (IEdge e in rightExecuteResult)
                {
                    IVertex newVertex = creationTarget.AddVertex(meta, e.To.Value);

                    if (additionalCreationStack != null)
                        additionalCreationStack.AddEdge(meta, newVertex);

                    toReturn = NextExpressionHandle(exe, newVertex, instructionVertex);
                }
            }

            if (additionalCreationStack != null)
                return additionalCreationStack;
            else
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(creationTarget);
        }

        public static INoInEdgeInOutVertexVertex DoubleSemicolonOperator(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            //bool isExeStackSameAsExeNewVertexCreationSpace = false;

            //  if (exe.stack == exe.newVertexCreationSpace)
            //     isExeStackSameAsExeNewVertexCreationSpace = true;

            IVertex creationTarget = exe.NewVertexCreationSpace;
            //IVertex stackForNextExpression;

            // if (isExeStackSameAsExeNewVertexCreationSpace)
            //     stackForNextExpression = CreateStack();
            // else
            //     stackForNextExpression = creationTarget;            

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            INoInEdgeInOutVertexVertex leftExecuteResult = null;
            if (leftExpression != null)
                leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, leftExpression);

            INoInEdgeInOutVertexVertex rightExecuteResult;

            if (rightExpression == null)
            {
                rightExecuteResult = CreateStack();
                rightExecuteResult.AddEdge(null, null); // will generate MinusZero.Instance.Empty
            }
            else
                rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, rightExpression);

            INoInEdgeInOutVertexVertex toReturn = null;

            if (rightExecuteResult.OutEdges.Count > 0) // what about more than one edge in results
            {
                IVertex meta = null;

                if (leftExecuteResult != null && leftExecuteResult.OutEdges.Count > 0)
                    meta = leftExecuteResult.OutEdges[0].To;

                foreach (IEdge e in rightExecuteResult)
                {
                    IEdge newEdge = creationTarget.AddEdge(meta, e.To);

                    toReturn = NextExpressionHandle(exe, newEdge.To, instructionVertex);

                    //       if (isExeStackSameAsExeNewVertexCreationSpace)
                    //           stackForNextExpression.AddEdge(meta, e.To);
                }
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(creationTarget);

            //if (toReturn == null) // XXX
            //return CreateStack();
            //return toReturn;
        }

        public static INoInEdgeInOutVertexVertex InnerCreation(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            bool local_isStackFrameReturn = false;
            INoInEdgeInOutVertexVertex possibleToReturnStack = null;

            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;

            exe.NewVertexCreationSpace = inputStack;

            possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe,
                    exe.Stack, instructionVertex, out local_isStackFrameReturn);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (local_isStackFrameReturn)
                return possibleToReturnStack;

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
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

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
            {
                INoInEdgeInOutVertexVertex nestedExpressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, e.To);

                foreach (IEdge ee in nestedExpressionResult)
                    newStack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(ee);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Parse(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            IVertex language;

            IVertex instuctionFormalTextLanguage = GraphUtil.GetQueryOutFirst(instructionVertex, "FormalTextLanguage", null);

            if (instuctionFormalTextLanguage == null)
                language = MinusZero.Instance.DefaultFormalTextLanguage;
            else
                language = GetFirstExecutionEdge(exe, instuctionFormalTextLanguage).To;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
            {
                IEdge newEdge = newStack.AddVertexAndReturnEdge(null, "");

                IEdge baseEdge_new;
                MinusZero.Instance.DefaultFormalTextParser.Parse(language, newEdge, e.To.Value.ToString(), CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Generate(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            IVertex language;

            IVertex instuctionFormalTextLanguage = GraphUtil.GetQueryOutFirst(instructionVertex, "FormalTextLanguage", null);

            if (instuctionFormalTextLanguage == null)
                language = MinusZero.Instance.DefaultFormalTextLanguage;
            else
                language = GetFirstExecutionEdge(exe, instuctionFormalTextLanguage).To;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
            {
                string parsed = MinusZero.Instance.DefaultFormalTextGenerator.Generate(language, e, CodeRepresentationEnum.VertexAndManyLines);

                newStack.AddVertex(null, parsed);
            }

            return newStack;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // oo
        //
        ////////////////////////////////////////////////////////////////

        #region oo

        public static INoInEdgeInOutVertexVertex MethodCall(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex targetExpression = GraphUtil.GetQueryOutFirst(instructionVertex, "Target", null);

            if (targetExpression == null)
                return exe.Stack;

            IList<IEdge> parameterExpressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge objectEdge in inputStack)
            {
                INoInEdgeInOutVertexVertex returnedStack = MethodCallForOneObject(objectEdge.To, exe, targetExpression, parameterExpressions);

                foreach (IEdge e in returnedStack)
                    newStack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);
            }

            // return newStack; want to have []\

            return NextExpressionHandle(exe, newStack, instructionVertex); // []\ worx
        }

        private static INoInEdgeInOutVertexVertex MethodCallForOneObject(IVertex theObject, ZeroCodeExecution exe, IVertex targetExpression, IList<IEdge> parameterExpressions)
        {
            IVertex objectIs = GetIs(theObject);

            if (objectIs == null)
                return CreateStack();

            //IVertex methodBody = Get(false, objectIs, targetExpression); // interesting but slow
            IVertex methodBody = GraphUtil.GetQueryOutFirst(objectIs, "Method", targetExpression.Value.ToString());

            if (methodBody == null) // not found
                return CreateStack();

            //if (methodBody != null && !CheckIfIsOrInherits_WRONG(methodBody, "Method")) // not a method
            //    return CreateStack();

            INoInEdgeInOutVertexVertex toReturnStack = MethodCallForOneObject_Internal(theObject, exe, parameterExpressions, methodBody);

            //if (local_isStackFrameReturn)
            return toReturnStack;
            //else
            //  return CreateStack();
        }

        private static INoInEdgeInOutVertexVertex MethodCallForOneObject_Internal(IVertex theObject, ZeroCodeExecution exe, IList<IEdge> parameterExpressions, IVertex methodBody)
        {
            IList<IEdge> inputParameters = GraphUtil.GetQueryOut(methodBody, "InputParameter", null);

            int minParameters = Math.Min(parameterExpressions.Count(), inputParameters.Count());

            exe.AddStackFrame(theObject); // ENTER NEW STACK
            exe.AddStackFrame();

            for (int x = 0; x < minParameters; x++)
            {
                IVertex expression = parameterExpressions[x].To;
                IVertex inputParameter = inputParameters[x].To;

                INoInEdgeInOutVertexVertex expressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, expression);

                foreach (IEdge e in expressionExecution)
                    exe.Stack.AddEdge(inputParameter, e.To);
            }

            exe.Stack.AddEdge(thisMeta, theObject);

            //bool local_isStackFrameReturn;
            //INoInEdgeInOutVertexVertex possibleToReturnStack = SequentiallyExecuteInstructions(exe, exe.stack, methodBody, out local_isStackFrameReturn, false);

            INoInEdgeInOutVertexVertex toReturnStack = methodBody.Execute(exe);


            exe.RemoveStackFrame();
            exe.RemoveStackFrame(); // LEAVE NEW STACK
            return toReturnStack;
        }

        public static INoInEdgeInOutVertexVertex New(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex target = GraphUtil.GetQueryOutFirst(instructionVertex, "Target", null);

            if (target == null)
                return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);

            IList<IEdge> parameterExpressions = GraphUtil.GetQueryOut(instructionVertex, "Expression", null);

            INoInEdgeInOutVertexVertex targetExecution = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, target);

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            foreach (IEdge oneTarget in targetExecution)
                if (CheckIfIsOrInherits_WRONG(oneTarget.To, "Class"))
                {
                    IVertex classVertex = oneTarget.To;

                    IVertex theObject = ZeroUMLInstructionHelpers.AddInstance(localStack, oneTarget.To);

                    IVertex methodBody = GraphUtil.GetQueryOutFirst(classVertex, "Method", classVertex.Value.ToString());

                    if (methodBody != null)
                        MethodCallForOneObject_Internal(theObject, exe, parameterExpressions, methodBody);
                }

            return localStack;
        }

        #endregion

        ////////////////////////////////////////////////////////////////
        //
        // create trigger & view
        //
        ////////////////////////////////////////////////////////////////   

        public static IList<IEdge> ExecuteInstruction(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            INoInEdgeInOutVertexVertex _executeResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, instructionVertex);

            return _executeResult.OutEdges;
        }

        public static INoInEdgeInOutVertexVertex CreateTrigger(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex nameVertex = GraphUtil.GetQueryOutFirst(instructionVertex, "Name", null);

            if (nameVertex == null)
                return exe.Stack;

            string name = GraphUtil.GetStringValue(nameVertex);

            IVertex innerVertex = GraphUtil.GetQueryOutFirst(instructionVertex, "CreateTriggerInner", null);

            if (innerVertex == null)
                return exe.Stack;

            IList<string> ScopeQueries = new List<string>();
            IList<IVertex> ChangeTypeFilters = new List<IVertex>();
            IList<IVertex> Listeners = new List<IVertex>();

            foreach (IEdge e in innerVertex)
            {
                if (GraphUtil.GetStringValue(e.Meta) != "Expression")
                    continue;

                IVertex expressionIs = GraphUtil.GetQueryOutFirst(e.To, "$Is", null);

                if (expressionIs == null)
                    continue;

                switch (GraphUtil.GetStringValue(expressionIs))
                {
                    case "ScopeQuery":
                        IVertex queryInstruction = GraphUtil.GetQueryOutFirst(e.To, "Query", null);

                        if (queryInstruction == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, queryInstruction).OutEdges)
                            ScopeQueries.Add(GraphUtil.GetStringValue(executeEdge.To));

                        break;

                    case "ChangeTypeFilter":
                        IVertex valueInstruction = GraphUtil.GetQueryOutFirst(e.To, "Value", null);

                        if (valueInstruction == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, valueInstruction).OutEdges)
                            ChangeTypeFilters.Add(executeEdge.To);

                        break;

                    case "Listener":
                        IVertex targetInstrucion = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        if (targetInstrucion == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, targetInstrucion).OutEdges)
                            Listeners.Add(executeEdge.To);

                        break;
                }
            }

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            IVertex trigger = localStack.AddVertex(dolarGraphChangeTriggerMeta, name);

            trigger.AddEdge(isMeta, graphChangeTriggerMeta);

            foreach (string query in ScopeQueries)
                trigger.AddVertex(graphChangeTrigger_ScopeQueryMeta, query);

            foreach (IVertex filter in ChangeTypeFilters)
                trigger.AddEdge(graphChangeTrigger_ChageTypeFilterMeta, filter);

            foreach (IVertex listener in Listeners)
                trigger.AddEdge(graphChangeTrigger_ListenerMeta, listener);


            return localStack;
        }

        public static INoInEdgeInOutVertexVertex CreateView(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            IEdge createViewTriggerEdge = GraphChangeTrigger.AddTrigger(localStack,
                new List<string>(),
                new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.OnlyNonTransactedRootVertexEvents,
                     GraphChangeFilterEnum.MetaEdgeAdded},
                "CreateView");

            ExecutionFlowHelper.AddListener_DotNetDelegate(createViewTriggerEdge.To, m0.Graph.ExecutionFlow.View.CreateView_MetaEdgeAdded, "CreateViewMetaEdgeAdded");

            //

            IList<string> FromTriggerQueries = new List<string>();
            IList<IVertex> FromTriggerFilters = new List<IVertex>();
            IList<IVertex> FromToTransformFunctions = new List<IVertex>(); 
            IList<string> ToTriggerQueries = new List<string>();
            IList<IVertex> ToTriggerFilters = new List<IVertex>();
            IList<IVertex> ToFromTransformFunctions = new List<IVertex>();

            IVertex innerVertex = GraphUtil.GetQueryOutFirst(instructionVertex, "CreateViewInner", null);
    

            if (innerVertex == null)
                return localStack;

            foreach (IEdge e in innerVertex)
            {
                if (GraphUtil.GetStringValue(e.Meta) != "Expression")
                    continue;

                IVertex expressionIs = GraphUtil.GetQueryOutFirst(e.To, "$Is", null);

                if (expressionIs == null)
                    continue;

                switch (GraphUtil.GetStringValue(expressionIs))
                {
                    case "FromTriggerQuery":
                        IVertex queryInstruction = GraphUtil.GetQueryOutFirst(e.To, "Query", null);

                        if (queryInstruction == null)
                            continue;                        
                        
                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, queryInstruction).OutEdges)
                            FromTriggerQueries.Add(GraphUtil.GetStringValue(executeEdge.To));

                        break;

                    case "FromTriggerFilter":
                        IVertex valueInstruction = GraphUtil.GetQueryOutFirst(e.To, "Value", null);

                        if (valueInstruction == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, valueInstruction).OutEdges)
                            FromTriggerFilters.Add(executeEdge.To);
                        
                        break;

                    case "FromToTransformFunction":
                        IVertex targetInstruction = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        if (targetInstruction == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, targetInstruction).OutEdges)
                            FromToTransformFunctions.Add(executeEdge.To);

                        break;

                    case "ToTriggerQuery":
                        IVertex queryInstruction2 = GraphUtil.GetQueryOutFirst(e.To, "Query", null);

                        if (queryInstruction2 == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, queryInstruction2).OutEdges)
                            ToTriggerQueries.Add(GraphUtil.GetStringValue(executeEdge.To));

                        break;

                    case "ToTriggerFilter":
                        IVertex valueInstruction2 = GraphUtil.GetQueryOutFirst(e.To, "Value", null);

                        if (valueInstruction2 == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, valueInstruction2).OutEdges)
                            ToTriggerFilters.Add(executeEdge.To);

                        break;

                    case "ToFromTransformFunction":
                        IVertex targetInstruction2 = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        if (targetInstruction2 == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, targetInstruction2).OutEdges)
                            ToFromTransformFunctions.Add(executeEdge.To);

                        break;
                }
            }


            IVertex view = localStack.AddVertex(viewMeta, "");

            view.AddEdge(isMeta, viewMeta);
            
            foreach (string query in FromTriggerQueries)
                view.AddVertex(view_FromTriggerQueryMeta, query);

            foreach (IVertex filter in FromTriggerFilters)
                view.AddEdge(view_FromTriggerFilterMeta, filter);

            foreach (IVertex function in FromToTransformFunctions)
                view.AddEdge(view_FromToTransformFunctionMeta, function);

            foreach (string query in ToTriggerQueries)
                view.AddVertex(view_ToTriggerQueryMeta, query);

            foreach (IVertex filter in ToTriggerFilters)
                view.AddEdge(view_ToTriggerFilterMeta, filter);

            foreach (IVertex function in ToFromTransformFunctions)
                view.AddEdge(view_ToFromTransformFunctionMeta, function);

            return localStack;
        }

        public static INoInEdgeInOutVertexVertex CreateHttpMapping(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex nameVertex = GraphUtil.GetQueryOutFirst(instructionVertex, "Name", null);

            if (nameVertex == null)
                return exe.Stack;

            string name = GraphUtil.GetStringValue(nameVertex);

            IVertex innerVertex = GraphUtil.GetQueryOutFirst(instructionVertex, "CreateHttpMappingInner", null);

            if (innerVertex == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            IVertex mapping = localStack.AddVertex(httpMappingMeta, name);

            mapping.AddEdge(isMeta, httpMappingMeta);

            foreach (IEdge e in innerVertex)
            {
                if (GraphUtil.GetStringValue(e.Meta) != "Expression")
                    continue;

                IVertex expressionIs = GraphUtil.GetQueryOutFirst(e.To, "$Is", null);

                if (expressionIs == null)
                    continue;

                switch (GraphUtil.GetStringValue(expressionIs))
                {
                    case "HttpMappingEntry":
                        IVertex Action = null;
                        string PathMask = null;
                        IVertex Handler = null;

                        IVertex action = GraphUtil.GetQueryOutFirst(e.To, "Action", null);

                        if (action == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, action).OutEdges)
                        {
                            Action = executeEdge.To;
                            break;
                        }

                        //

                        IVertex pathMask = GraphUtil.GetQueryOutFirst(e.To, "PathMask", null);

                        if (pathMask == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, pathMask).OutEdges)
                        {
                            PathMask = GraphUtil.GetStringValue(executeEdge.To);
                            break;
                        }

                        //

                        IVertex handler = GraphUtil.GetQueryOutFirst(e.To, "Handler", null);

                        if (handler == null)
                            continue;

                        foreach (IEdge executeEdge in exe.ExecuteInstructionByMontevideoPrinciples(inputStack, handler).OutEdges)
                        {
                            Handler = executeEdge.To;
                            break;
                        }

                        IVertex entry = mapping.AddVertex(httpMappingEntryMeta, null);

                        entry.AddEdge(isMeta, httpMappingEntryMeta);

                        entry.AddEdge(httpMappingEntry_ActionMeta, Action);

                        entry.AddVertex(httpMappingEntry_PathMaskMeta, PathMask);

                        entry.AddEdge(httpMappingEntry_HandlerMeta, Handler);

                        break;                    
                }
            }

            

            return localStack;
        }
    }
}

