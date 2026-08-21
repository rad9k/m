using m0.FormalTextLanguage;
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
using System.Runtime.CompilerServices;
using static m0.ZeroCode.Helpers.InstructionHelpers;
using static System.Net.WebRequestMethods;

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

            if (value.Length <= 2 ||
                value[0] != '(' ||
                value[value.Length - 1] != ')')
            {
                AddQueryOperatorMatches(
                    exe,
                    inputQs,
                    newQs,
                    value,
                    false);
            }
            else
            {
                bool queryByVariable;
                IList<string> processedValueList =
                    processQueryValue(
                        exe,
                        value,
                        out queryByVariable);

                foreach (string processedValue in processedValueList)
                    AddQueryOperatorMatches(
                        exe,
                        inputQs,
                        newQs,
                        processedValue,
                        queryByVariable);
            }

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        private static void AddQueryOperatorMatches(
            ZeroCodeExecution exe,
            IVertex inputQs,
            INoInEdgeInOutVertexVertex output,
            string processedValue,
            bool queryByVariable)
        {
            IEdge edge;
            IList<IEdge> edges;

            if (queryByVariable)
            {
                if (processedValue.Length > 0 &&
                    processedValue[0] == ':')
                    inputQs.QueryOutEdges(
                        null,
                        processedValue.Substring(1),
                        out edge,
                        out edges);
                else
                    inputQs.QueryOutEdges(
                        processedValue,
                        null,
                        out edge,
                        out edges);
            }
            else if (exe.MetaMode)
                inputQs.QueryOutEdges(
                    processedValue,
                    null,
                    out edge,
                    out edges);
            else
                inputQs.QueryOutEdges(
                    null,
                    processedValue,
                    out edge,
                    out edges);

            if (exe.CollapseQueryResultsByFromMeta)
            {
                long collapsedEdgeCount = 0;

                if (edge != null &&
                    !AddDistinctFromMetaQueryMatch(
                        output,
                        edge))
                    collapsedEdgeCount++;

                if (edges != null)
                    foreach (IEdge matchingEdge in edges)
                        if (!AddDistinctFromMetaQueryMatch(
                            output,
                            matchingEdge))
                            collapsedEdgeCount++;

                ZeroCodePerformanceCounters
                    .RecordCollapsedAssignmentQueryEdges(
                        collapsedEdgeCount);
            }
            else
            {
                if (edge != null)
                    output
                        .AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                            edge);

                if (edges != null)
                    AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                        output,
                        edges);
            }
        }

        private static bool AddDistinctFromMetaQueryMatch(
            INoInEdgeInOutVertexVertex output,
            IEdge candidate)
        {
            if (candidate == null)
                return false;

            IList<IEdge> existingEdges =
                output.OutEdgesRaw;
            for (int index = 0;
                index < existingEdges.Count;
                index++)
            {
                IEdge existing = existingEdges[index];
                if (ReferenceEquals(
                        existing.From,
                        candidate.From) &&
                    ReferenceEquals(
                        existing.Meta,
                        candidate.Meta))
                    return false;
            }

            output
                .AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                    candidate);
            return true;
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

            IList<IEdge> expressions =
                GraphUtil.GetQueryOut(
                    instructionVertex,
                    "Expression",
                    null);

            INoInEdgeInOutVertexVertex newQs = _inputQs;
            INoInEdgeInOutVertexVertex oldQs = _inputQs;
            long nestedExecutionCount = 0;

            foreach (IEdge expression in expressions)
            {
                if (IsIsHierarchyFilterExpression(expression.To))
                {
                    nestedExecutionCount++;
                    newQs = ProcessIsHierarchyFilter(exe, oldQs, expression.To);
                }
                else
                {
                    newQs = CreateStack();

                    foreach (IEdge e in oldQs)
                    {
                        nestedExecutionCount++;
                        IVertex outQs = exe.ExecuteInstructionByMontevideoPrinciples(e.To, expression.To);

                        if (outQs.OutEdges.Count > 0)
                            newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(e);
                    }
                }

                oldQs = newQs;
            }

            ZeroCodePerformanceCounters.RecordInnerOperator(
                nestedExecutionCount);
            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        private static bool IsIsHierarchyFilterExpression(IVertex expression)
        {
            if (expression == null || !CheckIfIs(expression, "Colon"))
                return false;

            IVertex leftExpression = GetLeft(expression);
            IVertex rightExpression = GetRight(expression);

            if (leftExpression == null || rightExpression == null
                || !CheckIfIs(leftExpression, "Query")
                || !GraphUtil.GetValueAndCompareStrings(leftExpression, "$Is"))
                return false;

            return GetNextExpression(expression) == null
                && GetNextExpression(leftExpression) == null
                && GetNextExpression(rightExpression) == null;
        }

        private static INoInEdgeInOutVertexVertex ProcessIsHierarchyFilter(
            ZeroCodeExecution exe,
            INoInEdgeInOutVertexVertex inputEdges,
            IVertex expression)
        {
            Dictionary<IEdge, ISet<IVertex>> typesByInputEdge = new Dictionary<IEdge, ISet<IVertex>>();
            HashSet<IVertex> allTypes = new HashSet<IVertex>();

            foreach (IEdge inputEdge in inputEdges)
            {
                if (typesByInputEdge.ContainsKey(inputEdge))
                    continue;

                ISet<IVertex> inputEdgeTypes = VertexOperations.GetIsAndInheritedTypes(inputEdge.To);
                typesByInputEdge.Add(inputEdge, inputEdgeTypes);

                foreach (IVertex typeVertex in inputEdgeTypes)
                    allTypes.Add(typeVertex);
            }

            INoInEdgeInOutVertexVertex typeEdges = CreateStack();

            foreach (IVertex typeVertex in allTypes)
                typeEdges.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                    GraphUtil.CreateArtificialEdge(isMeta, typeVertex));

            IVertex matchingTypeEdges = exe.ExecuteInstructionByMontevideoPrinciples(typeEdges, expression);
            HashSet<IVertex> matchingTypes = new HashSet<IVertex>();

            foreach (IEdge matchingTypeEdge in matchingTypeEdges)
                if (allTypes.Contains(matchingTypeEdge.To))
                    matchingTypes.Add(matchingTypeEdge.To);

            INoInEdgeInOutVertexVertex result = CreateStack();

            foreach (IEdge inputEdge in inputEdges)
                if (typesByInputEdge[inputEdge].Any(typeVertex => matchingTypes.Contains(typeVertex)))
                    result.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(inputEdge);

            return result;
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
                newQs.AddRangeOriginalEdges(e.To);

            return NextExpressionHandle(exe, newQs, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex InEdgesSlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex newQs = CreateStack();

            bool isFirstOperatorInExpression = true;

            foreach (IEdge e in instructionVertex.InEdgesRaw)
            {
                IList<IEdge> metaIsValuesList =
                    GraphUtil.GetQueryOut(
                        e.From,
                        "$Is",
                        null);

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
                    foreach (IEdge ee in e.InEdgesRaw)
                        newQs.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(GraphUtil.CreateArtificialEdge(ee.Meta, ee.From));
            }
            else
            {
                foreach (IEdge e in inputQs)
                    foreach (IEdge ee in e.To.InEdgesRaw)
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
                ZeroCodePerformanceCounters
                    .RecordColonOperatorQueryCombinations(
                        (long)processedToQueryStrings.Count *
                        processedMetaQueryStrings.Count);

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
                }
                else
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

        private enum SimpleScalarNumberStatus
        {
            Number,
            NoNumber,
            Unsupported
        }

        private enum SimpleScalarNodeKind
        {
            Unsupported,
            Literal,
            Query,
            Add,
            Multiply,
            Bracket
        }

        private sealed class SimpleScalarNode
        {
            internal EasyVertex Source;
            internal long SourceVersion;
            internal SimpleScalarNodeKind Kind;
            internal string QueryValue;
            internal object LiteralNumber;
            internal NumericTypeEnum LiteralType;
            internal int LiteralIntegerValue;
            internal bool LiteralHasNumber;
            internal bool ContainsOperator;
            internal SimpleScalarNode Left;
            internal SimpleScalarNode Right;
            internal SimpleScalarNode Inner;

            internal bool IsValid =>
                Source.TrackedLocalMutationVersion ==
                SourceVersion;
        }

        private sealed class SimpleScalarLessOrEqualPlan
        {
            internal EasyVertex Source;
            internal long SourceVersion;
            internal SimpleScalarNode Left;
            internal SimpleScalarNode Right;

            internal bool IsValid =>
                Source.TrackedLocalMutationVersion ==
                SourceVersion;
        }

        private sealed class RedirectPropagationPlan
        {
            internal EasyVertex Source;
            internal long SourceVersion;
            internal bool Result;
            internal long InheritanceEpoch;
            internal EasyVertex FirstTypeDependency;
            internal long FirstTypeDependencyVersion;
            internal EasyVertex[] AdditionalTypeDependencies;
            internal long[] AdditionalTypeDependencyVersions;

            internal bool IsValid(IVertex source)
            {
                if (!ReferenceEquals(Source, source) ||
                    InheritanceEpoch !=
                        EasyVertex
                            .InheritanceDependencyEpoch ||
                    !Source.IsQueryMetaOutIndexCurrent ||
                    Source.TrackedLocalMutationVersion !=
                        SourceVersion)
                {
                    return false;
                }

                if (FirstTypeDependency != null &&
                    (!FirstTypeDependency
                            .IsQueryMetaAndValueOutIndexCurrent ||
                        FirstTypeDependency
                            .TrackedLocalMutationVersion !=
                        FirstTypeDependencyVersion))
                {
                    return false;
                }

                if (AdditionalTypeDependencies == null)
                    return true;

                for (int index = 0;
                    index < AdditionalTypeDependencies.Length;
                    index++)
                {
                    if (!AdditionalTypeDependencies[index]
                            .IsQueryMetaAndValueOutIndexCurrent ||
                        AdditionalTypeDependencies[index]
                            .TrackedLocalMutationVersion !=
                        AdditionalTypeDependencyVersions[index])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static RedirectPropagationPlan
            CreateRedirectPropagationPlan(
            IVertex leftExpression,
            bool result)
        {
            if (!(leftExpression is
                EasyVertex easyLeftExpression))
            {
                return null;
            }

            EasyVertex firstTypeDependency = null;
            List<EasyVertex>
                additionalTypeDependencies = null;
            foreach (IEdge isEdge in
                GraphUtil.GetQueryOutResult(
                    leftExpression,
                    "$Is",
                    null))
            {
                if (!(isEdge.To is
                    EasyVertex typeDependency))
                {
                    return null;
                }

                if (ReferenceEquals(
                        firstTypeDependency,
                        typeDependency) ||
                    additionalTypeDependencies
                        ?.Contains(typeDependency) == true)
                {
                    continue;
                }

                if (firstTypeDependency == null)
                    firstTypeDependency =
                        typeDependency;
                else
                    (additionalTypeDependencies ??=
                        new List<EasyVertex>())
                        .Add(typeDependency);
            }

            EasyVertex[] additionalTypeDependencyArray =
                additionalTypeDependencies?.ToArray();
            long[] additionalTypeDependencyVersions =
                additionalTypeDependencyArray == null
                    ? null
                    : new long[
                        additionalTypeDependencyArray
                            .Length];
            if (additionalTypeDependencyArray != null)
            {
                for (int index = 0;
                    index <
                        additionalTypeDependencyArray.Length;
                    index++)
                {
                    additionalTypeDependencyVersions[index] =
                        additionalTypeDependencyArray[index]
                            .EnableTrackedLocalMutationVersion();
                }
            }

            return new RedirectPropagationPlan
            {
                Source = easyLeftExpression,
                SourceVersion =
                    easyLeftExpression
                        .EnableTrackedLocalMutationVersion(),
                Result = result,
                InheritanceEpoch =
                    EasyVertex
                        .InheritanceDependencyEpoch,
                FirstTypeDependency =
                    firstTypeDependency,
                FirstTypeDependencyVersion =
                    firstTypeDependency
                        ?.EnableTrackedLocalMutationVersion() ??
                    0,
                AdditionalTypeDependencies =
                    additionalTypeDependencyArray,
                AdditionalTypeDependencyVersions =
                    additionalTypeDependencyVersions
            };
        }

        private static bool GetLeftPropagateToStackExpression(
            IVertex leftExpression,
            ref RedirectPropagationPlan plan)
        {
            if (plan?.IsValid(leftExpression) == true)
                return plan.Result;

            bool result =
                CheckIfIsInherits_WRONG(
                    leftExpression,
                    "PropagateToStackExpression");
            plan =
                CreateRedirectPropagationPlan(
                    leftExpression,
                    result);
            return result;
        }

        private static readonly
            ConditionalWeakTable<IVertex, SimpleScalarNode>
                simpleScalarNodeCache =
                    new ConditionalWeakTable<
                        IVertex,
                        SimpleScalarNode>();

        private static SimpleScalarNode GetSimpleScalarNode(
            IVertex expression)
        {
            if (expression == null)
                return null;

            if (simpleScalarNodeCache.TryGetValue(
                    expression,
                    out SimpleScalarNode cachedNode) &&
                cachedNode.IsValid)
                return cachedNode;

            lock (simpleScalarNodeCache)
            {
                return GetSimpleScalarNode(
                    expression,
                    new HashSet<IVertex>(
                        ReferenceEqualityComparer.Instance),
                    0);
            }
        }

        private static SimpleScalarNode GetSimpleScalarNode(
            IVertex expression,
            HashSet<IVertex> visiting,
            int depth)
        {
            if (depth > 64 ||
                !(expression is EasyVertex easyExpression))
                return null;

            if (simpleScalarNodeCache.TryGetValue(
                    expression,
                    out SimpleScalarNode cachedNode))
            {
                if (cachedNode.IsValid)
                    return cachedNode;

                simpleScalarNodeCache.Remove(expression);
            }

            if (!visiting.Add(expression))
                return null;

            long sourceVersion =
                easyExpression
                    .EnableTrackedLocalMutationVersion();
            SimpleScalarNode node =
                CompileSimpleScalarNode(
                    expression,
                    easyExpression,
                    sourceVersion,
                    visiting,
                    depth);
            visiting.Remove(expression);

            if (node == null ||
                easyExpression
                    .TrackedLocalMutationVersion !=
                sourceVersion)
                return null;

            simpleScalarNodeCache.Add(
                expression,
                node);
            return node;
        }

        private static SimpleScalarNode CompileSimpleScalarNode(
            IVertex expression,
            EasyVertex easyExpression,
            long sourceVersion,
            HashSet<IVertex> visiting,
            int depth)
        {
            SimpleScalarNode node =
                new SimpleScalarNode
                {
                    Source = easyExpression,
                    SourceVersion = sourceVersion,
                    Kind =
                        SimpleScalarNodeKind.Unsupported
                };
            IVertex instructionType = GetIs(expression);
            if (instructionType == null)
            {
                GraphUtil.GetNumberValue(
                    expression,
                    out node.LiteralNumber);
                node.LiteralHasNumber =
                    node.LiteralNumber != null;
                node.LiteralType =
                    GetSimpleScalarNumericType(
                        node.LiteralNumber);
                if (node.LiteralHasNumber &&
                    node.LiteralType ==
                        NumericTypeEnum.Integer)
                {
                    node.LiteralIntegerValue =
                        (int)node.LiteralNumber;
                }
                node.Kind = SimpleScalarNodeKind.Literal;
                return node;
            }

            string operation =
                instructionType.Value?.ToString();
            if (string.Equals(
                    operation,
                    "Query",
                    StringComparison.Ordinal))
            {
                string queryValue =
                    expression.Value?.ToString();
                if (GetNextExpression(expression) == null &&
                    !string.IsNullOrEmpty(queryValue) &&
                    (queryValue.Length <= 2 ||
                        queryValue[0] != '(' ||
                        queryValue[
                            queryValue.Length - 1] != ')'))
                {
                    node.Kind =
                        SimpleScalarNodeKind.Query;
                    node.QueryValue = queryValue;
                }

                return node;
            }

            if (string.Equals(
                    operation,
                    "+",
                    StringComparison.Ordinal) ||
                string.Equals(
                    operation,
                    "Mul",
                    StringComparison.Ordinal))
            {
                node.Left = GetSimpleScalarNode(
                    GetLeft(expression),
                    visiting,
                    depth + 1);
                node.Right = GetSimpleScalarNode(
                    GetRight(expression),
                    visiting,
                    depth + 1);
                if (node.Left == null ||
                    node.Right == null)
                    return node;

                node.Kind = string.Equals(
                    operation,
                    "+",
                    StringComparison.Ordinal)
                    ? SimpleScalarNodeKind.Add
                    : SimpleScalarNodeKind.Multiply;
                node.ContainsOperator = true;
                return node;
            }

            if (string.Equals(
                    operation,
                    "()",
                    StringComparison.Ordinal) &&
                GetNextExpression(expression) == null)
            {
                node.Inner = GetSimpleScalarNode(
                    GetExpression(expression),
                    visiting,
                    depth + 1);
                if (node.Inner != null)
                {
                    node.Kind =
                        SimpleScalarNodeKind.Bracket;
                    node.ContainsOperator =
                        node.Inner.ContainsOperator;
                }
            }

            return node;
        }

        private static NumericTypeEnum
            GetSimpleScalarNumericType(
            object number)
        {
            if (number is int)
                return NumericTypeEnum.Integer;
            if (number is double)
                return NumericTypeEnum.Double;
            return NumericTypeEnum.Decimal;
        }

        private static SimpleScalarLessOrEqualPlan
            CompileSimpleScalarLessOrEqualPlan(
            IVertex expression)
        {
            if (!(expression is EasyVertex easyExpression))
                return null;

            long sourceVersion =
                easyExpression
                    .EnableTrackedLocalMutationVersion();
            if (!string.Equals(
                    GetIs(expression)?.Value?.ToString(),
                    "LessOrEqualThan",
                    StringComparison.Ordinal))
                return null;

            SimpleScalarNode left =
                GetSimpleScalarNode(GetLeft(expression));
            SimpleScalarNode right =
                GetSimpleScalarNode(GetRight(expression));
            if (left == null ||
                right == null ||
                easyExpression
                    .TrackedLocalMutationVersion !=
                sourceVersion)
                return null;

            return new SimpleScalarLessOrEqualPlan
            {
                Source = easyExpression,
                SourceVersion = sourceVersion,
                Left = left,
                Right = right
            };
        }

        private static bool
            TryEvaluateSimpleScalarNumericOperatorExpression(
            ZeroCodeExecution exe,
            IVertex inputStack,
            IVertex expression,
            out object result)
        {
            result = null;
            SimpleScalarNode node =
                GetSimpleScalarNode(expression);
            bool evaluated =
                TryEvaluateSimpleScalarNumericOperatorNode(
                    exe,
                    inputStack,
                    node,
                    out result,
                    out bool planIsValid);
            if (!planIsValid)
                simpleScalarNodeCache.Remove(expression);

            return evaluated && planIsValid;
        }

        private static bool
            TryEvaluateSimpleScalarNumericOperatorNode(
            ZeroCodeExecution exe,
            IVertex inputStack,
            SimpleScalarNode node,
            out object result,
            out bool planIsValid)
        {
            result = null;
            planIsValid = node?.IsValid == true;
            if (!planIsValid ||
                !node.ContainsOperator)
                return false;

            bool evaluated =
                TryEvaluateSimpleScalarNode(
                    exe,
                    inputStack,
                    node,
                    out result,
                    out _,
                    out bool hasNumber,
                    out bool wasComputed,
                    out planIsValid);

            return evaluated &&
                planIsValid &&
                hasNumber &&
                wasComputed;
        }

        private static bool
            TryEvaluateSimpleScalarNumericExpression(
            ZeroCodeExecution exe,
            IVertex inputStack,
            IVertex expression,
            int depth,
            out object result,
            out NumericTypeEnum resultType,
            out bool hasNumber,
            out bool wasComputed)
        {
            result = null;
            resultType = NumericTypeEnum.Decimal;
            hasNumber = false;
            wasComputed = false;
            if (depth > 64 || expression == null)
                return false;

            SimpleScalarNode node =
                GetSimpleScalarNode(expression);
            if (node == null)
                return false;

            bool evaluated =
                TryEvaluateSimpleScalarNode(
                    exe,
                    inputStack,
                    node,
                    out result,
                    out resultType,
                    out hasNumber,
                    out wasComputed,
                    out bool planIsValid);
            if (!planIsValid)
                simpleScalarNodeCache.Remove(expression);

            return evaluated && planIsValid;
        }

        private static bool TryEvaluateSimpleScalarNode(
            ZeroCodeExecution exe,
            IVertex inputStack,
            SimpleScalarNode node,
            out object result,
            out NumericTypeEnum resultType,
            out bool hasNumber,
            out bool wasComputed,
            out bool planIsValid)
        {
            result = null;
            resultType = NumericTypeEnum.Decimal;
            hasNumber = false;
            wasComputed = false;
            planIsValid = node?.IsValid == true;
            if (!planIsValid)
                return false;

            if (node.Kind ==
                SimpleScalarNodeKind.Unsupported)
                return false;

            if (node.Kind ==
                SimpleScalarNodeKind.Literal)
            {
                result = node.LiteralNumber;
                resultType = node.LiteralType;
                hasNumber = node.LiteralHasNumber;
                return true;
            }

            if (node.Kind ==
                SimpleScalarNodeKind.Query)
            {
                SimpleScalarNumberStatus status =
                    GetSimpleScalarQueryNumberStatus(
                        exe,
                        inputStack,
                        node.QueryValue,
                        out result,
                        out resultType);
                hasNumber =
                    status ==
                    SimpleScalarNumberStatus.Number;
                return status !=
                    SimpleScalarNumberStatus.Unsupported;
            }

            if (node.Kind ==
                SimpleScalarNodeKind.Bracket)
                return TryEvaluateSimpleScalarNode(
                    exe,
                    inputStack,
                    node.Inner,
                    out result,
                    out resultType,
                    out hasNumber,
                    out wasComputed,
                    out planIsValid);

            if (!TryEvaluateSimpleScalarNode(
                    exe,
                    inputStack,
                    node.Left,
                    out object leftNumber,
                    out NumericTypeEnum leftType,
                    out bool leftHasNumber,
                    out bool leftWasComputed,
                    out planIsValid) ||
                !planIsValid)
                return false;

            if (!TryEvaluateSimpleScalarNode(
                    exe,
                    inputStack,
                    node.Right,
                    out object rightNumber,
                    out NumericTypeEnum rightType,
                    out bool rightHasNumber,
                    out bool rightWasComputed,
                    out planIsValid) ||
                !planIsValid)
                return false;

            if (!leftHasNumber)
            {
                result = rightNumber;
                resultType = rightType;
                hasNumber = rightHasNumber;
                wasComputed = rightWasComputed;
                return true;
            }

            if (!rightHasNumber)
            {
                result = leftNumber;
                resultType = leftType;
                hasNumber = true;
                wasComputed = leftWasComputed;
                return true;
            }

            hasNumber = true;
            wasComputed = true;
            resultType = GetCommonNubmerResultDenominator(
                leftType,
                rightType);
            bool isAdd =
                node.Kind == SimpleScalarNodeKind.Add;
            switch (resultType)
            {
                case NumericTypeEnum.Integer:
                    result = isAdd
                        ? Convert.ToInt32(leftNumber) +
                            Convert.ToInt32(rightNumber)
                        : Convert.ToInt32(leftNumber) *
                            Convert.ToInt32(rightNumber);
                    return true;
                case NumericTypeEnum.Double:
                    result = isAdd
                        ? Convert.ToDouble(leftNumber) +
                            Convert.ToDouble(rightNumber)
                        : Convert.ToDouble(leftNumber) *
                            Convert.ToDouble(rightNumber);
                    return true;
                case NumericTypeEnum.Decimal:
                    result = isAdd
                        ? Convert.ToDecimal(leftNumber) +
                            Convert.ToDecimal(rightNumber)
                        : Convert.ToDecimal(leftNumber) *
                            Convert.ToDecimal(rightNumber);
                    return true;
                default:
                    return false;
            }
        }

        private static bool
            TryEvaluateSimpleScalarNumericOperatorNodeUnboxed(
            ZeroCodeExecution exe,
            IVertex inputStack,
            SimpleScalarNode node,
            IEdge directQueryEdge,
            out EasyVertex.ScalarNumericValue result,
            out bool planIsValid)
        {
            result = default;
            planIsValid = node?.IsValid == true;
            if (!planIsValid ||
                !node.ContainsOperator)
                return false;

            if (TryEvaluateSimpleIntegerQueryLiteralOperator(
                    exe,
                    inputStack,
                    node,
                    directQueryEdge,
                    out result,
                    out planIsValid))
            {
                return true;
            }

            if (!planIsValid)
                return false;

            bool evaluated =
                TryEvaluateSimpleScalarNodeUnboxed(
                    exe,
                    inputStack,
                    node,
                    out result,
                    out _,
                    out bool hasNumber,
                    out bool wasComputed,
                    out planIsValid);

            return evaluated &&
                planIsValid &&
                hasNumber &&
                wasComputed;
        }

        private static bool
            TryEvaluateSimpleIntegerQueryLiteralOperator(
            ZeroCodeExecution exe,
            IVertex inputStack,
            SimpleScalarNode node,
            IEdge directQueryEdge,
            out EasyVertex.ScalarNumericValue result,
            out bool planIsValid)
        {
            result = default;
            planIsValid = true;
            if (node.Kind != SimpleScalarNodeKind.Add &&
                node.Kind != SimpleScalarNodeKind.Multiply)
            {
                return false;
            }

            SimpleScalarNode queryNode;
            SimpleScalarNode literalNode;
            if (node.Left?.Kind ==
                    SimpleScalarNodeKind.Query &&
                node.Right?.Kind ==
                    SimpleScalarNodeKind.Literal)
            {
                queryNode = node.Left;
                literalNode = node.Right;
            }
            else if (node.Right?.Kind ==
                    SimpleScalarNodeKind.Query &&
                node.Left?.Kind ==
                    SimpleScalarNodeKind.Literal)
            {
                queryNode = node.Right;
                literalNode = node.Left;
            }
            else
            {
                return false;
            }

            planIsValid =
                queryNode.IsValid &&
                literalNode.IsValid;
            if (!planIsValid ||
                !literalNode.LiteralHasNumber ||
                literalNode.LiteralType !=
                    NumericTypeEnum.Integer)
            {
                return false;
            }

            EasyVertex.ScalarNumericValue queryNumber;
            SimpleScalarNumberStatus status;
            if (directQueryEdge?.To is
                    EasyVertex directQueryTarget &&
                string.Equals(
                    directQueryEdge.Meta?.Value as string,
                    queryNode.QueryValue,
                    StringComparison.Ordinal) &&
                directQueryTarget.TryGetScalarNumericValue(
                    out queryNumber))
            {
                status =
                    SimpleScalarNumberStatus.Number;
            }
            else
            {
                status =
                    GetSimpleScalarQueryNumberStatusUnboxed(
                        exe,
                        inputStack,
                        queryNode.QueryValue,
                        out queryNumber,
                        out _);
            }
            if (status !=
                    SimpleScalarNumberStatus.Number ||
                queryNumber.Kind !=
                    EasyVertex.ScalarNumericKind.Integer)
            {
                return false;
            }

            int literalValue =
                literalNode.LiteralIntegerValue;
            result =
                EasyVertex.ScalarNumericValue
                    .FromInteger(
                        node.Kind ==
                            SimpleScalarNodeKind.Add
                            ? queryNumber.IntegerValue +
                                literalValue
                            : queryNumber.IntegerValue *
                                literalValue);
            return true;
        }

        private static bool
            TryEvaluateSimpleScalarNodeUnboxed(
            ZeroCodeExecution exe,
            IVertex inputStack,
            SimpleScalarNode node,
            out EasyVertex.ScalarNumericValue result,
            out NumericTypeEnum resultType,
            out bool hasNumber,
            out bool wasComputed,
            out bool planIsValid)
        {
            result = default;
            resultType = NumericTypeEnum.Decimal;
            hasNumber = false;
            wasComputed = false;
            planIsValid = node?.IsValid == true;
            if (!planIsValid)
                return false;

            if (node.Kind ==
                SimpleScalarNodeKind.Unsupported)
                return false;

            if (node.Kind ==
                SimpleScalarNodeKind.Literal)
            {
                resultType = node.LiteralType;
                hasNumber = node.LiteralHasNumber;
                return !hasNumber ||
                    EasyVertex.ScalarNumericValue
                        .TryCreate(
                            node.LiteralNumber,
                            out result);
            }

            if (node.Kind ==
                SimpleScalarNodeKind.Query)
            {
                SimpleScalarNumberStatus status =
                    GetSimpleScalarQueryNumberStatusUnboxed(
                        exe,
                        inputStack,
                        node.QueryValue,
                        out result,
                        out resultType);
                hasNumber =
                    status ==
                    SimpleScalarNumberStatus.Number;
                return status !=
                    SimpleScalarNumberStatus.Unsupported;
            }

            if (node.Kind ==
                SimpleScalarNodeKind.Bracket)
            {
                return TryEvaluateSimpleScalarNodeUnboxed(
                    exe,
                    inputStack,
                    node.Inner,
                    out result,
                    out resultType,
                    out hasNumber,
                    out wasComputed,
                    out planIsValid);
            }

            if (!TryEvaluateSimpleScalarNodeUnboxed(
                    exe,
                    inputStack,
                    node.Left,
                    out EasyVertex.ScalarNumericValue
                        leftNumber,
                    out NumericTypeEnum leftType,
                    out bool leftHasNumber,
                    out bool leftWasComputed,
                    out planIsValid) ||
                !planIsValid)
            {
                return false;
            }

            if (!TryEvaluateSimpleScalarNodeUnboxed(
                    exe,
                    inputStack,
                    node.Right,
                    out EasyVertex.ScalarNumericValue
                        rightNumber,
                    out NumericTypeEnum rightType,
                    out bool rightHasNumber,
                    out bool rightWasComputed,
                    out planIsValid) ||
                !planIsValid)
            {
                return false;
            }

            if (!leftHasNumber)
            {
                result = rightNumber;
                resultType = rightType;
                hasNumber = rightHasNumber;
                wasComputed = rightWasComputed;
                return true;
            }

            if (!rightHasNumber)
            {
                result = leftNumber;
                resultType = leftType;
                hasNumber = true;
                wasComputed = leftWasComputed;
                return true;
            }

            hasNumber = true;
            wasComputed = true;
            resultType =
                GetCommonNubmerResultDenominator(
                    leftType,
                    rightType);
            bool isAdd =
                node.Kind == SimpleScalarNodeKind.Add;
            switch (resultType)
            {
                case NumericTypeEnum.Integer:
                    result =
                        EasyVertex.ScalarNumericValue
                            .FromInteger(
                                isAdd
                                    ? leftNumber.IntegerValue +
                                        rightNumber.IntegerValue
                                    : leftNumber.IntegerValue *
                                        rightNumber.IntegerValue);
                    return true;
                case NumericTypeEnum.Double:
                    double leftDouble =
                        GetSimpleScalarDouble(
                            leftNumber);
                    double rightDouble =
                        GetSimpleScalarDouble(
                            rightNumber);
                    result =
                        EasyVertex.ScalarNumericValue
                            .FromDouble(
                                isAdd
                                    ? leftDouble +
                                        rightDouble
                                    : leftDouble *
                                        rightDouble);
                    return true;
                case NumericTypeEnum.Decimal:
                    decimal leftDecimal =
                        GetSimpleScalarDecimal(
                            leftNumber);
                    decimal rightDecimal =
                        GetSimpleScalarDecimal(
                            rightNumber);
                    result =
                        EasyVertex.ScalarNumericValue
                            .FromDecimal(
                                isAdd
                                    ? leftDecimal +
                                        rightDecimal
                                    : leftDecimal *
                                        rightDecimal);
                    return true;
                default:
                    return false;
            }
        }

        private static double GetSimpleScalarDouble(
            EasyVertex.ScalarNumericValue value)
        {
            switch (value.Kind)
            {
                case EasyVertex.ScalarNumericKind.Integer:
                    return value.IntegerValue;
                case EasyVertex.ScalarNumericKind.Double:
                    return value.DoubleValue;
                case EasyVertex.ScalarNumericKind.Decimal:
                    return (double)value.DecimalValue;
                default:
                    return default;
            }
        }

        private static decimal GetSimpleScalarDecimal(
            EasyVertex.ScalarNumericValue value)
        {
            switch (value.Kind)
            {
                case EasyVertex.ScalarNumericKind.Integer:
                    return value.IntegerValue;
                case EasyVertex.ScalarNumericKind.Double:
                    return (decimal)value.DoubleValue;
                case EasyVertex.ScalarNumericKind.Decimal:
                    return value.DecimalValue;
                default:
                    return default;
            }
        }

        private static SimpleScalarNumberStatus
            GetSimpleScalarQueryNumberStatusUnboxed(
            ZeroCodeExecution exe,
            IVertex inputStack,
            string queryValue,
            out EasyVertex.ScalarNumericValue number,
            out NumericTypeEnum numericType)
        {
            number = default;
            numericType = NumericTypeEnum.Decimal;
            IEdge matchingEdge;
            IList<IEdge> matchingEdges;
            if (exe.MetaMode)
            {
                inputStack.QueryOutEdges(
                    queryValue,
                    null,
                    out matchingEdge,
                    out matchingEdges);
            }
            else
            {
                inputStack.QueryOutEdges(
                    null,
                    queryValue,
                    out matchingEdge,
                    out matchingEdges);
            }

            int matchCount =
                (matchingEdge == null ? 0 : 1) +
                (matchingEdges?.Count ?? 0);
            if (matchCount == 0)
                return SimpleScalarNumberStatus.NoNumber;
            if (matchCount != 1)
                return SimpleScalarNumberStatus.Unsupported;

            IVertex target =
                matchingEdge?.To ??
                matchingEdges[0].To;
            if (target is EasyVertex easyTarget &&
                easyTarget.TryGetScalarNumericValue(
                    out number))
            {
                numericType =
                    GetSimpleScalarNumericType(
                        number.Kind);
                return SimpleScalarNumberStatus.Number;
            }

            GraphUtil.GetNumberValue(
                target,
                out object boxedNumber);
            if (boxedNumber == null)
                return SimpleScalarNumberStatus.NoNumber;
            if (!EasyVertex.ScalarNumericValue.TryCreate(
                    boxedNumber,
                    out number))
            {
                return SimpleScalarNumberStatus.Unsupported;
            }

            numericType =
                GetSimpleScalarNumericType(
                    number.Kind);
            return SimpleScalarNumberStatus.Number;
        }

        private static NumericTypeEnum
            GetSimpleScalarNumericType(
            EasyVertex.ScalarNumericKind kind)
        {
            switch (kind)
            {
                case EasyVertex.ScalarNumericKind.Integer:
                    return NumericTypeEnum.Integer;
                case EasyVertex.ScalarNumericKind.Double:
                    return NumericTypeEnum.Double;
                default:
                    return NumericTypeEnum.Decimal;
            }
        }

        private static SimpleScalarNumberStatus
            GetSimpleScalarQueryNumberStatus(
            ZeroCodeExecution exe,
            IVertex inputStack,
            string queryValue,
            out object number,
            out NumericTypeEnum numericType)
        {
            number = null;
            numericType = NumericTypeEnum.Decimal;
            IEdge matchingEdge;
            IList<IEdge> matchingEdges;
            if (exe.MetaMode)
                inputStack.QueryOutEdges(
                    queryValue,
                    null,
                    out matchingEdge,
                    out matchingEdges);
            else
                inputStack.QueryOutEdges(
                    null,
                    queryValue,
                    out matchingEdge,
                    out matchingEdges);

            int matchCount =
                (matchingEdge == null ? 0 : 1) +
                (matchingEdges?.Count ?? 0);
            if (matchCount == 0)
                return SimpleScalarNumberStatus.NoNumber;
            if (matchCount != 1)
                return SimpleScalarNumberStatus.Unsupported;

            GraphUtil.GetNumberValue(
                matchingEdge?.To ??
                    matchingEdges[0].To,
                out number);
            if (number == null)
                return SimpleScalarNumberStatus.NoNumber;

            numericType =
                GetSimpleScalarNumericType(number);

            return SimpleScalarNumberStatus.Number;
        }

        // =
        public static INoInEdgeInOutVertexVertex RedirectLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            RedirectPropagationPlan propagationPlan = null;
            bool leftPropagateToStackExpression =
                GetLeftPropagateToStackExpression(
                    leftExpression,
                    ref propagationPlan);
            SimpleScalarNode scalarNode = null;

            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;

            // left

            INoInEdgeInOutVertexVertex leftStack = null;
            INoInEdgeInOutVertexVertex rightStack = null;
            INoInEdgeInOutVertexVertex leftExecuteResult = null;
            INoInEdgeInOutVertexVertex _rightExecuteResult = null;
            bool leftIsSingleQueryTarget = false;

            try
            {
                leftStack = CreateStack();
                exe.NewVertexCreationSpace = leftStack;
                leftExecuteResult =
                    exe.ExecuteInstructionByMontevideoPrinciples(
                        exe.Stack,
                        leftExpression);
                leftIsSingleQueryTarget =
                    leftExecuteResult.OutEdges.Count == 1 &&
                    string.Equals(
                        GetIs(leftExpression)
                            ?.Value?.ToString(),
                        "Query",
                        StringComparison.Ordinal) &&
                    GetNextExpression(leftExpression) == null;

                // right

                bool hasSingleLeftTarget =
                    leftExecuteResult.OutEdges.Count == 1;
                IEdge singleLeftTargetEdge =
                    hasSingleLeftTarget
                        ? leftExecuteResult.OutEdges[0]
                        : null;
                EasyVertex.ScalarNumericValue
                    scalarNumericResult = default;
                if (scalarNode?.IsValid != true)
                    scalarNode =
                        GetSimpleScalarNode(
                            rightExpression);
                bool scalarPlanIsValid = true;
                bool hasDirectScalarNumericResult =
                    hasSingleLeftTarget &&
                    TryEvaluateSimpleScalarNumericOperatorNodeUnboxed(
                        exe,
                        exe.Stack,
                        scalarNode,
                        exe.MetaMode &&
                        leftIsSingleQueryTarget &&
                        string.Equals(
                            singleLeftTargetEdge
                                ?.Meta?.Value as string,
                            leftExpression.Value as string,
                            StringComparison.Ordinal)
                            ? singleLeftTargetEdge
                            : null,
                        out scalarNumericResult,
                        out scalarPlanIsValid);
                if (!scalarPlanIsValid)
                {
                    scalarNode = null;
                    simpleScalarNodeCache.Remove(
                        rightExpression);
                }
                if (hasDirectScalarNumericResult)
                    exe.NewVertexCreationSpace =
                        newVertexCreationSpace_copy;
                else
                {
                    rightStack = CreateStack();
                    exe.NewVertexCreationSpace =
                        rightStack;
                }

                if (!hasDirectScalarNumericResult)
                    _rightExecuteResult =
                        exe.ExecuteInstructionByMontevideoPrinciples(
                            exe.Stack,
                            rightExpression);

                // NEW

                IList<IEdge> rightExecuteResult =
                    _rightExecuteResult?.OutEdges;

                if (hasSingleLeftTarget)
                {
                    IEdge toAdd =
                        singleLeftTargetEdge;

                    IVertex redirectTarget =
                        leftPropagateToStackExpression
                            ? exe.Stack
                            : toAdd.From;
                    IEdge redirectedEdge = null;
                    bool updatedExclusiveScalarTarget =
                        hasDirectScalarNumericResult &&
                        !leftPropagateToStackExpression &&
                        exe.TryUpdateExclusiveScalarAssignmentTarget(
                            toAdd,
                            scalarNumericResult);
                    if (updatedExclusiveScalarTarget)
                    {
                        redirectedEdge = toAdd;
                    }
                    else
                    {
                        toAdd.From.DeleteEdge(toAdd);

                        if (hasDirectScalarNumericResult)
                            redirectedEdge =
                                redirectTarget
                                    .AddVertexAndReturnEdge(
                                        toAdd.Meta,
                                        scalarNumericResult
                                            .ToObject());
                        else
                            foreach (IEdge e in
                                rightExecuteResult)
                                redirectedEdge =
                                    redirectTarget.AddEdge(
                                        toAdd.Meta,
                                        e.To);
                    }

                }
                else
                {
                    IDictionary<EdgeKey_FromMeta, IList<IEdge>> leftFromMeta_dict =
                        CreateEdgeKey_FromMetaDictionary(
                            leftExecuteResult);

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
                }

                return exe.Stack;
            }
            finally
            {
                exe.NewVertexCreationSpace =
                    newVertexCreationSpace_copy;

                if (leftExecuteResult != null &&
                    !ReferenceEquals(
                        leftExecuteResult,
                        leftStack) &&
                    !ReferenceEquals(
                        leftExecuteResult,
                        rightStack))
                    ReleaseTemporaryStack(
                        leftExecuteResult,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);

                if (_rightExecuteResult != null &&
                    !ReferenceEquals(
                        _rightExecuteResult,
                        leftStack) &&
                    !ReferenceEquals(
                        _rightExecuteResult,
                        rightStack))
                    ReleaseTemporaryStack(
                        _rightExecuteResult,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);

                if (leftStack != null &&
                    leftStack.OutEdgesRaw.Count == 0)
                    ReleaseTemporaryStack(
                        leftStack,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);

                if (rightStack != null &&
                    rightStack.OutEdgesRaw.Count == 0)
                    ReleaseTemporaryStack(
                        rightStack,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);
            }
        }

        // +=
        public static INoInEdgeInOutVertexVertex AddLeftEdgesToRightVertices(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            bool collapseQueryResults =
                string.Equals(
                    GetIs(leftExpression)?.Value?.ToString(),
                    "Query",
                    StringComparison.Ordinal) &&
                GetNextExpression(leftExpression) == null;
            INoInEdgeInOutVertexVertex leftExecuteResult =
                null;
            bool previousCollapseQueryResults =
                exe.CollapseQueryResultsByFromMeta;

            try
            {
                exe.CollapseQueryResultsByFromMeta =
                    collapseQueryResults;
                leftExecuteResult =
                    exe.ExecuteInstructionByMontevideoPrinciples(
                        exe.Stack,
                        leftExpression);
            }
            finally
            {
                exe.CollapseQueryResultsByFromMeta =
                    previousCollapseQueryResults;
            }

            //INoInEdgeInOutVertexVertex _rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.stack, rightExpression);

            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            INoInEdgeInOutVertexVertex rightStack = null;
            INoInEdgeInOutVertexVertex _rightExecuteResult = null;

            try
            {
                bool hasSingleTarget =
                    leftExecuteResult.OutEdges.Count == 1;
                if (hasSingleTarget &&
                    TryEvaluateSimpleScalarNumericOperatorExpression(
                        exe,
                        exe.Stack,
                        rightExpression,
                        out object scalarNumericResult))
                {
                    IEdge toAdd =
                        leftExecuteResult.OutEdges[0];
                    toAdd.From.AddVertex(
                        toAdd.Meta,
                        scalarNumericResult);
                    return exe.Stack;
                }

                rightStack = CreateStack();
                exe.NewVertexCreationSpace = rightStack;
                _rightExecuteResult =
                    exe.ExecuteInstructionByMontevideoPrinciples(
                        exe.Stack,
                        rightExpression);

                // NEW

                IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

                if (hasSingleTarget)
                {
                    IEdge toAdd =
                        leftExecuteResult.OutEdges[0];

                    foreach (IEdge e in rightExecuteResult)
                        toAdd.From.AddEdge(toAdd.Meta, e.To);
                }
                else
                {
                    IDictionary<EdgeKey_FromMeta, IEdge> leftFromMeta_dict =
                        CreateEdgeKey_FromMetaFirstDictionary(
                            leftExecuteResult);

                    foreach (KeyValuePair<EdgeKey_FromMeta, IEdge> localLeft in leftFromMeta_dict)
                    {
                        IEdge toAdd = localLeft.Value;

                        foreach (IEdge e in rightExecuteResult)
                            toAdd.From.AddEdge(toAdd.Meta, e.To);
                    }
                }

                return exe.Stack;
            }
            finally
            {
                exe.NewVertexCreationSpace =
                    newVertexCreationSpace_copy;

                if (leftExecuteResult != null)
                    ReleaseTemporaryStack(
                        leftExecuteResult,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);

                if (_rightExecuteResult != null &&
                    !ReferenceEquals(
                    _rightExecuteResult,
                    rightStack))
                    ReleaseTemporaryStack(
                        _rightExecuteResult,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);

                if (rightStack != null &&
                    rightStack.OutEdgesRaw.Count == 0)
                    ReleaseTemporaryStack(
                        rightStack,
                        inputStack,
                        exe.Stack,
                        newVertexCreationSpace_copy);
            }
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

        // <<copy<<
        public static INoInEdgeInOutVertexVertex CopySubgraph(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (leftExecuteResult.OutEdges.Count > 0)
            {
                foreach (IEdge e in leftExecuteResult)
                    VertexOperations.CopyEdgesSet(rightExecuteResult.OutEdges, e.To);
            }

            return exe.Stack;
        }

        // <<move<<
        public static INoInEdgeInOutVertexVertex MoveSubraph(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (leftExecuteResult.OutEdges.Count > 0)
            {
                foreach (IEdge e in leftExecuteResult)
                    VertexOperations.MoveEdgesSet(rightExecuteResult.OutEdges, e.To);
            }

            return exe.Stack;
        }

        // <<copy&replace<<
        public static INoInEdgeInOutVertexVertex CopyAndReplaceSubgraph(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (leftExecuteResult.OutEdges.Count > 0)
            {
                foreach (IEdge e in leftExecuteResult)
                    VertexOperations.CopyAndReplaceEdgesSet(rightExecuteResult.OutEdges, e.To);
            }

            return exe.Stack;
        }

        // <<move&replace<<
        public static INoInEdgeInOutVertexVertex MoveAndReplaceSubgraph(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex leftExpression = GetLeft(instructionVertex);
            IVertex rightExpression = GetRight(instructionVertex);

            if (leftExpression == null || rightExpression == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex leftExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, leftExpression);


            IVertex newVertexCreationSpace_copy = exe.NewVertexCreationSpace;
            exe.NewVertexCreationSpace = CreateStack();

            INoInEdgeInOutVertexVertex rightExecuteResult = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, rightExpression);

            exe.NewVertexCreationSpace = newVertexCreationSpace_copy;

            if (leftExecuteResult.OutEdges.Count > 0)
            {
                foreach (IEdge e in leftExecuteResult)
                    VertexOperations.MoveAndReplaceEdgesSet(rightExecuteResult.OutEdges, e.To);
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
            IList<IEdge> inputEdges = inputStack.OutEdges;
            IList<IEdge> executeEdges = executeResult.OutEdges;

            foreach (IEdge e in executeEdges)
            {
                int? index = GraphUtil.GetIntegerValue(e.To);

                if (index != null && index >= 1 && index <= inputEdges.Count)
                {
                    IEdge selectedEdge = inputEdges[(int)index - 1];

                    localStack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(selectedEdge);
                }
            }

            return NextExpressionHandle(exe, localStack, instructionVertex);
        }

        public static INoInEdgeInOutVertexVertex SetCount(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex localStack = CreateStack();

            localStack.AddVertex(null, inputStack.OutEdges.Count);

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

            try
            {
                IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
                IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

                if (TryGetSingleNumber(
                        leftExecuteResult,
                        out object leftNumber,
                        out NumericTypeEnum singleLeftResultType) &&
                    TryGetSingleNumber(
                        rightExecuteResult,
                        out object rightNumber,
                        out NumericTypeEnum singleRightResultType))
                {
                    INoInEdgeInOutVertexVertex singleResult =
                        CreateStack();

                    switch (GetCommonNubmerResultDenominator(
                        singleLeftResultType,
                        singleRightResultType))
                    {
                        case NumericTypeEnum.Integer:
                            singleResult.AddVertex(
                                null,
                                Convert.ToInt32(leftNumber) +
                                Convert.ToInt32(rightNumber));
                            break;

                        case NumericTypeEnum.Double:
                            singleResult.AddVertex(
                                null,
                                Convert.ToDouble(leftNumber) +
                                Convert.ToDouble(rightNumber));
                            break;

                        case NumericTypeEnum.Decimal:
                            singleResult.AddVertex(
                                null,
                                Convert.ToDecimal(leftNumber) +
                                Convert.ToDecimal(rightNumber));
                            break;
                    }

                    return singleResult;
                }

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
            finally
            {
                ReleaseTemporaryStack(
                    _leftExecuteResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
                ReleaseTemporaryStack(
                    _rightExecuteResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }
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

            try
            {
                IList<IEdge> leftExecuteResult = _leftExecuteResult.OutEdges;
                IList<IEdge> rightExecuteResult = _rightExecuteResult.OutEdges;

                if (TryGetSingleNumber(
                        leftExecuteResult,
                        out object leftNumber,
                        out NumericTypeEnum singleLeftResultType) &&
                    TryGetSingleNumber(
                        rightExecuteResult,
                        out object rightNumber,
                        out NumericTypeEnum singleRightResultType))
                {
                    INoInEdgeInOutVertexVertex singleResult =
                        CreateStack();

                    switch (GetCommonNubmerResultDenominator(
                        singleLeftResultType,
                        singleRightResultType))
                    {
                        case NumericTypeEnum.Integer:
                            singleResult.AddVertex(
                                null,
                                Convert.ToInt32(leftNumber) *
                                Convert.ToInt32(rightNumber));
                            break;

                        case NumericTypeEnum.Double:
                            singleResult.AddVertex(
                                null,
                                Convert.ToDouble(leftNumber) *
                                Convert.ToDouble(rightNumber));
                            break;

                        case NumericTypeEnum.Decimal:
                            singleResult.AddVertex(
                                null,
                                Convert.ToDecimal(leftNumber) *
                                Convert.ToDecimal(rightNumber));
                            break;
                    }

                    return singleResult;
                }

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
            finally
            {
                ReleaseTemporaryStack(
                    _leftExecuteResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
                ReleaseTemporaryStack(
                    _rightExecuteResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }
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

            try
            {
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
            finally
            {
                ReleaseTemporaryStack(
                    _leftExecuteResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
                ReleaseTemporaryStack(
                    _rightExecuteResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }
        }

        private static bool
            TryEvaluateSimpleIntegerQueryLiteralLessOrEqual(
            ZeroCodeExecution exe,
            IVertex inputStack,
            SimpleScalarNode leftNode,
            SimpleScalarNode rightNode,
            out bool result)
        {
            result = false;
            SimpleScalarNode queryNode;
            SimpleScalarNode literalNode;
            bool queryIsLeft;
            if (leftNode?.Kind ==
                    SimpleScalarNodeKind.Query &&
                rightNode?.Kind ==
                    SimpleScalarNodeKind.Literal)
            {
                queryNode = leftNode;
                literalNode = rightNode;
                queryIsLeft = true;
            }
            else if (rightNode?.Kind ==
                    SimpleScalarNodeKind.Query &&
                leftNode?.Kind ==
                    SimpleScalarNodeKind.Literal)
            {
                queryNode = rightNode;
                literalNode = leftNode;
                queryIsLeft = false;
            }
            else
            {
                return false;
            }

            if (!queryNode.IsValid ||
                !literalNode.IsValid ||
                !literalNode.LiteralHasNumber ||
                literalNode.LiteralType !=
                    NumericTypeEnum.Integer)
            {
                return false;
            }

            SimpleScalarNumberStatus status =
                GetSimpleScalarQueryNumberStatusUnboxed(
                    exe,
                    inputStack,
                    queryNode.QueryValue,
                    out EasyVertex.ScalarNumericValue
                        queryNumber,
                    out _);
            if (status !=
                    SimpleScalarNumberStatus.Number ||
                queryNumber.Kind !=
                    EasyVertex.ScalarNumericKind.Integer)
            {
                return false;
            }

            int literalValue =
                literalNode.LiteralIntegerValue;
            result = queryIsLeft
                ? queryNumber.IntegerValue <=
                    literalValue
                : literalValue <=
                    queryNumber.IntegerValue;
            return true;
        }

        private static bool TryEvaluateWhileCondition(
            ZeroCodeExecution exe,
            IVertex inputStack,
            IVertex instructionVertex,
            SimpleScalarLessOrEqualPlan plan,
            out bool result)
        {
            result = false;
            IVertex leftExpression = null;
            IVertex rightExpression = null;
            SimpleScalarNode leftNode;
            SimpleScalarNode rightNode;
            if (plan != null)
            {
                if (!plan.IsValid)
                    return false;

                leftNode = plan.Left;
                rightNode = plan.Right;
            }
            else
            {
                if (!string.Equals(
                        GetIs(instructionVertex)
                            ?.Value?.ToString(),
                        "LessOrEqualThan",
                        StringComparison.Ordinal))
                    return false;

                leftExpression = GetLeft(instructionVertex);
                rightExpression = GetRight(instructionVertex);
                if (leftExpression == null ||
                    rightExpression == null)
                    return false;

                leftNode =
                    GetSimpleScalarNode(leftExpression);
                rightNode =
                    GetSimpleScalarNode(rightExpression);
            }

            if (TryEvaluateSimpleIntegerQueryLiteralLessOrEqual(
                    exe,
                    inputStack,
                    leftNode,
                    rightNode,
                    out result))
            {
                return true;
            }

            if (leftNode != null &&
                rightNode != null &&
                TryEvaluateSimpleScalarNodeUnboxed(
                    exe,
                    inputStack,
                    leftNode,
                    out EasyVertex.ScalarNumericValue
                        leftNumber,
                    out NumericTypeEnum leftType,
                    out bool leftHasNumber,
                    out _,
                    out bool leftPlanIsValid) &&
                leftPlanIsValid &&
                leftHasNumber &&
                TryEvaluateSimpleScalarNodeUnboxed(
                    exe,
                    inputStack,
                    rightNode,
                    out EasyVertex.ScalarNumericValue
                        rightNumber,
                    out NumericTypeEnum rightType,
                    out bool rightHasNumber,
                    out _,
                    out bool rightPlanIsValid) &&
                rightPlanIsValid &&
                rightHasNumber)
            {
                switch (GetCommonNubmerResultDenominator(
                    leftType,
                    rightType))
                {
                    case NumericTypeEnum.Integer:
                        result =
                            leftNumber.IntegerValue <=
                            rightNumber.IntegerValue;
                        return true;
                    case NumericTypeEnum.Double:
                        result =
                            GetSimpleScalarDouble(
                                leftNumber) <=
                            GetSimpleScalarDouble(
                                rightNumber);
                        return true;
                    case NumericTypeEnum.Decimal:
                        result =
                            GetSimpleScalarDecimal(
                                leftNumber) <=
                            GetSimpleScalarDecimal(
                                rightNumber);
                        return true;
                }
            }

            if (plan != null)
                return false;

            INoInEdgeInOutVertexVertex leftResult =
                exe.ExecuteInstructionByMontevideoPrinciples(
                    inputStack,
                    leftExpression);
            INoInEdgeInOutVertexVertex rightResult =
                exe.ExecuteInstructionByMontevideoPrinciples(
                    inputStack,
                    rightExpression);

            try
            {
                IList<IEdge> leftEdges = leftResult.OutEdges;
                IList<IEdge> rightEdges = rightResult.OutEdges;
                int count = Math.Min(
                    leftEdges.Count,
                    rightEdges.Count);

                result = true;
                for (int index = 0; index < count; index++)
                    if (!LogicDoubleOperator_VertexLevel(
                        leftEdges[index].To,
                        rightEdges[index].To,
                        LogicDoubleOpertorEnum.LessOrEqualThan))
                        result = false;

                return true;
            }
            finally
            {
                ReleaseTemporaryStack(
                    leftResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
                ReleaseTemporaryStack(
                    rightResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }
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
                if (leftNumber is int leftInteger &&
                    rightNumber is int rightInteger)
                    return LogicDoubleOperator_ExecuteInteger(
                        leftInteger,
                        rightInteger,
                        operationType);

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

        private static bool LogicDoubleOperator_ExecuteInteger(
            int leftValue,
            int rightValue,
            LogicDoubleOpertorEnum operationType)
        {
            switch (operationType)
            {
                case LogicDoubleOpertorEnum.Equal:
                case LogicDoubleOpertorEnum.ExactEqual:
                    return leftValue == rightValue;
                case LogicDoubleOpertorEnum.NotEqual:
                    return leftValue != rightValue;
                case LogicDoubleOpertorEnum.And:
                    return leftValue > 0 && rightValue > 0;
                case LogicDoubleOpertorEnum.Or:
                    return leftValue > 0 || rightValue > 0;
                case LogicDoubleOpertorEnum.MoreThan:
                    return leftValue > rightValue;
                case LogicDoubleOpertorEnum.LessThan:
                    return leftValue < rightValue;
                case LogicDoubleOpertorEnum.MoreOrEqualThan:
                    return leftValue >= rightValue;
                case LogicDoubleOpertorEnum.LessOrEqualThan:
                    return leftValue <= rightValue;
                default:
                    return false;
            }
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
                if (targetExpressionExecution.OutEdges.Count > 0)
                    target = targetExpressionExecution.OutEdges[0].To;

                ReleaseTemporaryStack(
                    targetExpressionExecution,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }

            if (target == null)
                return exe.Stack;

            exe.AddStackFrame(); // ENTER NEW STACK

            exe.Stack.AddEdge(functionTarget_meta, target); // to be able to know the function target vertex in the function body

            EdgeQueryResult expressions =
                GraphUtil.GetQueryOutResult(
                    instructionVertex,
                    "Expression",
                    null);
            EdgeQueryResult inputParameters =
                GraphUtil.GetQueryOutResult(
                    target,
                    "InputParameter",
                    null);

            int minParameters =
                Math.Min(
                    expressions.Count,
                    inputParameters.Count);

            for (int x = 0; x < minParameters; x++)
            {
                IVertex expression = expressions[x].To;
                IVertex inputParameter = inputParameters[x].To;

                INoInEdgeInOutVertexVertex expressionExecution = exe.ExecuteInstructionByMontevideoPrinciples(exe.Stack, expression);

                foreach (IEdge e in expressionExecution)
                    exe.Stack.AddEdge(inputParameter, e.To);

                ReleaseTemporaryStack(
                    expressionExecution,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }

            //bool local_isStackFrameReturn;

            //INoInEdgeInOutVertexVertex possibleToReturnStack = SequentiallyExecuteInstructions(exe, exe.stack, target, out local_isStackFrameReturn, false);

            INoInEdgeInOutVertexVertex toReturnStack = target.Execute(exe);

            INoInEdgeInOutVertexVertex completedFrame =
                exe.Stack;
            exe.RemoveStackFrame(); // LEAVE NEW STACK
            if (!ReferenceEquals(
                toReturnStack,
                completedFrame))
                ReleaseTemporaryStack(
                    completedFrame,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);

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
                    ZeroCodePerformanceCounters
                        .RecordForVertexIteration();
                    exe.AddStackFrame(); // ENTER NEW STACK

                    IEdge variableEdge = GraphUtil.CreateArtificialEdge(variable, setEdge.To);

                    exe.Stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(variableEdge);

                    possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                        break;

                    INoInEdgeInOutVertexVertex completedFrame =
                        exe.Stack;
                    exe.RemoveStackFrame(); // LEAVE NEW STACK
                    ReleaseTemporaryStack(
                        completedFrame,
                        inputStack,
                        exe.Stack,
                        exe.NewVertexCreationSpace);
                }

                ReleaseTemporaryStack(
                    setExecution,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);

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
                    ZeroCodePerformanceCounters
                        .RecordForEdgeIteration();
                    exe.AddStackFrame(); // ENTER NEW STACK

                    IEdge variableEdge = GraphUtil.CreateArtificialEdge(variable, EdgeHelper.CreateTempEdgeVertex(setEdge));

                    exe.Stack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(variableEdge);

                    possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                        break;

                    INoInEdgeInOutVertexVertex completedFrame =
                        exe.Stack;
                    exe.RemoveStackFrame();  // LEAVE NEW STACK
                    ReleaseTemporaryStack(
                        completedFrame,
                        inputStack,
                        exe.Stack,
                        exe.NewVertexCreationSpace);
                }

                ReleaseTemporaryStack(
                    setExecution,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);

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
                SimpleScalarLessOrEqualPlan
                    directConditionPlan =
                        CompileSimpleScalarLessOrEqualPlan(
                            test);

                bool directConditionEvaluation =
                    TryEvaluateWhileCondition(
                        exe,
                        exe.Stack,
                        test,
                        directConditionPlan,
                        out bool directConditionResult);
                INoInEdgeInOutVertexVertex testResult =
                    directConditionEvaluation
                        ? null
                        : exe.ExecuteInstructionByMontevideoPrinciples(
                            exe.Stack,
                            test);

                while (directConditionEvaluation
                    ? directConditionResult
                    : IsTrue_Stack(testResult))
                {
                    ZeroCodePerformanceCounters
                        .RecordWhileIteration();
                    exe.AddStackFrame(); // ENTER NEW STACK

                    possibleToReturnStack = ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, instructionVertex, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                        break;


                    if (directConditionEvaluation)
                    {
                        directConditionEvaluation =
                            TryEvaluateWhileCondition(
                                exe,
                                exe.Stack,
                                test,
                                directConditionPlan,
                                out directConditionResult);
                        if (!directConditionEvaluation)
                            testResult =
                                exe.ExecuteInstructionByMontevideoPrinciples(
                                    exe.Stack,
                                    test);
                    }
                    else
                    {
                        INoInEdgeInOutVertexVertex previousTestResult =
                            testResult;
                        testResult =
                            exe.ExecuteInstructionByMontevideoPrinciples(
                                exe.Stack,
                                test);
                        ReleaseTemporaryStack(
                            previousTestResult,
                            inputStack,
                            exe.Stack,
                            exe.NewVertexCreationSpace);
                    }

                    INoInEdgeInOutVertexVertex completedFrame =
                        exe.Stack;
                    exe.RemoveStackFrame(); // LEAVE NEW STACK
                    ReleaseTemporaryStack(
                        completedFrame,
                        inputStack,
                        exe.Stack,
                        exe.NewVertexCreationSpace);
                }

                if (local_isStackFrameReturn)
                {
                    ReleaseTemporaryStack(
                        testResult,
                        inputStack,
                        exe.Stack,
                        exe.NewVertexCreationSpace);
                    return possibleToReturnStack;
                }

                ReleaseTemporaryStack(
                    testResult,
                    inputStack,
                    exe.Stack,
                    exe.NewVertexCreationSpace);
            }

            return Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputStack);
        }

        public static INoInEdgeInOutVertexVertex Link(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

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

            IList<IEdge> cases =
                GraphUtil.GetQueryOut(
                    instructionVertex,
                    "Case",
                    null);

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

            //foreach (IEdge e in expressionResult)
            VertexOperations.CopyEdgesSet(expressionResult, newStack);

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
                if (!VertexOperations.CanPassAsExecutionResult_ByEdge(e))
                    continue;

                INoInEdgeInOutVertexVertex nestedExpressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, e.To);

                newStack.AddRangeOriginalEdges(
                    nestedExpressionResult);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Parse(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            IVertex languageProcessing = null;

            IVertex instuctionFormalTextLanguage = GraphUtil.GetQueryOutFirst(instructionVertex, "FormalTextLanguage", null);

            if (instuctionFormalTextLanguage != null)                
                languageProcessing = GetFirstExecutionEdge(exe, instuctionFormalTextLanguage).To;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
            {
                //IEdge newEdge = newStack.AddVertexAndReturnEdge(null, "");

                IEdge newEdge_temp = MinusZero.Instance.CreateTempEdge();

                IEdge baseEdge_new;

                IVertex errorList;

                if (languageProcessing != null && GraphUtil.ExistQueryOut(languageProcessing, "$Is", "FormalTextLanguageProcessing"))
                    errorList = ZeroCodeProcessingHelper.Parse(languageProcessing, newEdge_temp, e.To.Value.ToString(), out baseEdge_new);                
                else
                    errorList = ZeroCodeProcessingHelper.Parse(newEdge_temp, e.To.Value.ToString(), out baseEdge_new);

                if (errorList != null && errorList.OutEdges.Count == 0)
                {
                    if (baseEdge_new != null)
                        newStack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(baseEdge_new);
                    else                    
                        newStack.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(newEdge_temp);                    
                }
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Generate(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex expression = GetExpression(instructionVertex);

            if (expression == null)
                return exe.Stack;

            IVertex formalTextLanguageProcessing = null;

            IVertex instuctionFormalTextLanguage = GraphUtil.GetQueryOutFirst(instructionVertex, "FormalTextLanguage", null);

            if (instuctionFormalTextLanguage != null)
                formalTextLanguageProcessing = GetFirstExecutionEdge(exe, instuctionFormalTextLanguage).To;

            INoInEdgeInOutVertexVertex expressionResult = exe.ExecuteInstructionByMontevideoPrinciples(inputStack, expression);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge e in expressionResult)
            {
                string parsed;

                if (formalTextLanguageProcessing == null)
                    parsed = ZeroCodeProcessingHelper.Generate(e);
                else
                    parsed = ZeroCodeProcessingHelper.Generate(formalTextLanguageProcessing, e);                 

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

            IList<IEdge> parameterExpressions =
                GraphUtil.GetQueryOut(
                    instructionVertex,
                    "Expression",
                    null);

            INoInEdgeInOutVertexVertex newStack = CreateStack();

            foreach (IEdge objectEdge in inputStack)
            {
                INoInEdgeInOutVertexVertex returnedStack = MethodCallForOneObject(objectEdge.To, exe, targetExpression, parameterExpressions);

                newStack.AddRangeOriginalEdges(
                    returnedStack);
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
            IList<IEdge> inputParameters =
                GraphUtil.GetQueryOut(
                    methodBody,
                    "InputParameter",
                    null);

            int minParameters =
                Math.Min(
                    parameterExpressions.Count,
                    inputParameters.Count);

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

            IList<IEdge> parameterExpressions =
                GraphUtil.GetQueryOut(
                    instructionVertex,
                    "Expression",
                    null);

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

