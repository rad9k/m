using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.DotNetIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace m0.ZeroCode
{
    public class ZeroCodeExecution: IExecution
    {
        private int stackFrameDepth;
        private readonly
            Stack<StackFrameInheritanceLink>
                stackFrameInheritanceLinks =
                    new Stack<StackFrameInheritanceLink>();
        private Dictionary<IVertex, IEdge>
            addAssignmentTargetCache;
        private Dictionary<IVertex, RedirectAssignmentCacheEntry>
            redirectAssignmentTargetCache;

        public INoInEdgeInOutVertexVertex Stack { get; set; }

        public IVertex NewVertexCreationSpace { get; set; }

        public bool MetaMode { get; set; }

        internal bool CollapseQueryResultsByFromMeta { get; set; }

        public ZeroCodeExecution()
        {
            MetaMode = true;

            CreateEmptyStack();

            NewVertexCreationSpace = Stack;

            AddRootToStack();
        }

        public ZeroCodeExecution(IVertex expression)
        {
            MetaMode = true;

            CreateEmptyStack();

            NewVertexCreationSpace = Stack;

            AddRootToStack();

            AddDolarToStack(expression);
        }

        public ZeroCodeExecution(IVertex toBeStackVertex, IVertex expression)
        {
            MetaMode = true;

            IEnumerable<IEdge> _toBeStackVertex;

            if (toBeStackVertex == null)
                _toBeStackVertex = new List<IEdge>();
            else
                _toBeStackVertex = toBeStackVertex;

            Stack = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(_toBeStackVertex);

            NewVertexCreationSpace = Stack;

            AddRootToStack();

            AddDolarToStack(expression);
        }

        private void AddDolarToStack(IVertex expression)
        {
            Stack.AddEdge(MinusZero.Instance.Dolar, expression);
        }

        public void AddRootToStack()
        {
            Stack.AddEdge(MinusZero.Instance.StackFrameInherits, MinusZero.Instance.Root);
        }

        public void CreateEmptyStack()
        {
            Stack = InstructionHelpers.CreateStack();
        }

        public void AddStackFrame()
        {
            INoInEdgeInOutVertexVertex newStackFrame = InstructionHelpers.CreateStack();

            IEdge inheritanceEdge =
                newStackFrame.AddEdge(
                    MinusZero.Instance.StackFrameInherits,
                    Stack);
            stackFrameInheritanceLinks.Push(
                new StackFrameInheritanceLink(
                    newStackFrame,
                    inheritanceEdge));

            Stack = newStackFrame;
            stackFrameDepth++;
            ZeroCodePerformanceCounters.RecordStackFramePush(
                stackFrameDepth);
        }

        public void AddStackFrame(IVertex newStackFrame)
        {            
            INoInEdgeInOutVertexVertex newStackFrameINIEIOV = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(newStackFrame);
            
            IEdge inheritanceEdge =
                newStackFrameINIEIOV.AddEdge(
                    MinusZero.Instance.StackFrameInherits,
                    Stack);
            stackFrameInheritanceLinks.Push(
                new StackFrameInheritanceLink(
                    newStackFrameINIEIOV,
                    inheritanceEdge));

            Stack = newStackFrameINIEIOV;
            stackFrameDepth++;
            ZeroCodePerformanceCounters.RecordStackFramePush(
                stackFrameDepth);
        }

        public void RemoveStackFrame()
        {
            IEdge stackFrameInheritsEdge = null;
            if (stackFrameInheritanceLinks.Count > 0)
            {
                StackFrameInheritanceLink cachedLink =
                    stackFrameInheritanceLinks.Peek();
                if (ReferenceEquals(
                        cachedLink.Frame,
                        Stack) &&
                    cachedLink.Frame.OutEdgesRaw.Contains(
                        cachedLink.Edge))
                {
                    stackFrameInheritanceLinks.Pop();
                    stackFrameInheritsEdge =
                        cachedLink.Edge;
                }
                else
                    stackFrameInheritanceLinks.Clear();
            }

            stackFrameInheritsEdge ??=
                GraphUtil.GetQueryOutFirstEdge(
                    Stack,
                    "$StackFrameInherits",
                    null);

            if (stackFrameInheritsEdge == null)
                throw new Exception("Can not remove stack frame. No $StackFrameInherits");

            IVertex _prevStackFrame = stackFrameInheritsEdge.To;

            Stack.DeleteEdge(stackFrameInheritsEdge);

            if(_prevStackFrame != null && _prevStackFrame is INoInEdgeInOutVertexVertex)
            {
                INoInEdgeInOutVertexVertex prevStackFrame = (INoInEdgeInOutVertexVertex)_prevStackFrame;

                Stack = prevStackFrame;
                if (stackFrameDepth > 0)
                    stackFrameDepth--;
                ZeroCodePerformanceCounters.RecordStackFramePop();
            }
        }

        private readonly struct StackFrameInheritanceLink
        {
            internal StackFrameInheritanceLink(
                INoInEdgeInOutVertexVertex frame,
                IEdge edge)
            {
                Frame = frame;
                Edge = edge;
            }

            internal INoInEdgeInOutVertexVertex Frame
            {
                get;
            }

            internal IEdge Edge
            {
                get;
            }
        }

        private readonly struct RedirectAssignmentCacheEntry
        {
            internal RedirectAssignmentCacheEntry(
                IEdge targetEdge,
                object scalarPlan)
            {
                TargetEdge = targetEdge;
                ScalarPlan = scalarPlan;
            }

            internal IEdge TargetEdge { get; }

            internal object ScalarPlan { get; }
        }

        internal bool TryGetCachedAddAssignmentTarget(
            IVertex instructionVertex,
            out IEdge targetEdge)
        {
            return TryGetCachedAssignmentTarget(
                addAssignmentTargetCache,
                instructionVertex,
                out targetEdge);
        }

        internal bool TryGetCachedRedirectAssignmentTarget(
            IVertex instructionVertex,
            out IEdge targetEdge,
            out object scalarPlan)
        {
            targetEdge = null;
            scalarPlan = null;
            if (redirectAssignmentTargetCache == null ||
                !redirectAssignmentTargetCache.TryGetValue(
                    instructionVertex,
                    out RedirectAssignmentCacheEntry entry) ||
                !IsCachedAssignmentTargetValid(
                    entry.TargetEdge))
                return false;

            targetEdge = entry.TargetEdge;
            scalarPlan = entry.ScalarPlan;
            return true;
        }

        private bool TryGetCachedAssignmentTarget(
            Dictionary<IVertex, IEdge> cache,
            IVertex instructionVertex,
            out IEdge targetEdge)
        {
            targetEdge = null;
            if (cache == null ||
                !cache.TryGetValue(
                    instructionVertex,
                    out IEdge cachedEdge) ||
                !IsCachedAssignmentTargetValid(cachedEdge))
                return false;

            targetEdge = cachedEdge;
            return true;
        }

        private bool IsCachedAssignmentTargetValid(
            IEdge cachedEdge)
        {
            return cachedEdge?.From is
                    INoInEdgeInOutVertexVertex sourceFrame &&
                IsCurrentStackFrameOrAncestor(sourceFrame) &&
                sourceFrame.OutEdgesRaw.Contains(cachedEdge);
        }

        internal void CacheAddAssignmentTarget(
            IVertex instructionVertex,
            IEdge targetEdge)
        {
            CacheAssignmentTarget(
                ref addAssignmentTargetCache,
                instructionVertex,
                targetEdge);
        }

        internal void CacheRedirectAssignmentTarget(
            IVertex instructionVertex,
            IEdge targetEdge,
            object scalarPlan)
        {
            if (!(targetEdge?.From is
                INoInEdgeInOutVertexVertex))
                return;

            redirectAssignmentTargetCache ??=
                new Dictionary<
                    IVertex,
                    RedirectAssignmentCacheEntry>(
                    ReferenceEqualityComparer.Instance);
            redirectAssignmentTargetCache[
                instructionVertex] =
                    new RedirectAssignmentCacheEntry(
                        targetEdge,
                        scalarPlan);
        }

        private static void CacheAssignmentTarget(
            ref Dictionary<IVertex, IEdge> cache,
            IVertex instructionVertex,
            IEdge targetEdge)
        {
            if (!(targetEdge?.From is
                INoInEdgeInOutVertexVertex))
                return;

            cache ??=
                new Dictionary<IVertex, IEdge>(
                    ReferenceEqualityComparer.Instance);
            cache[instructionVertex] =
                targetEdge;
        }

        private bool IsCurrentStackFrameOrAncestor(
            IVertex candidate)
        {
            IVertex current = Stack;

            while (current is
                INoInEdgeInOutVertexVertex)
            {
                if (ReferenceEquals(current, candidate))
                    return true;

                IEdge parentEdge =
                    GraphUtil.GetQueryOutFirstEdge(
                        current,
                        "$StackFrameInherits",
                        null);
                current = parentEdge?.To;
            }

            return false;
        }

        public INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(IVertex inputQs, IVertex instructionVertex)
        {
            bool dummy;

            return ExecuteInstructionByMontevideoPrinciples(inputQs, instructionVertex, out dummy);
        }

        public INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;
            ZeroCodeInstructionTimingToken timingToken =
                ZeroCodePerformanceCounters.BeginInstruction(true);
            IVertex is_v = null;

            try
            {
                is_v = InstructionHelpers.GetIs(instructionVertex);
                INoInEdgeInOutVertexVertex endpointResult = null;
                bool isExecutable =
                    is_v != null &&
                    CallableEndPointDictionary_INIEIOV_ZCE_IV_IV_B
                        .TryCallEndPoint(
                            this,
                            inputQs,
                            instructionVertex,
                            is_v,
                            out endpointResult,
                            out isStackFrameReturn);
                ZeroCodePerformanceCounters.RecordInstructionResolution(
                    isExecutable);

                if (isExecutable)  // execute if you can.....
                    return endpointResult;

                // ...OR...
                INoInEdgeInOutVertexVertex stack_ = InstructionHelpers.CreateStack();

                stack_.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(GraphUtil.CreateArtificialEdge(null, instructionVertex)); // create stack and put reference

                return stack_;
            }
            finally
            {
                ZeroCodePerformanceCounters.EndInstruction(
                    timingToken,
                    is_v);
            }
        }

        public INoInEdgeInOutVertexVertex ExecuteInstruction(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;
            ZeroCodeInstructionTimingToken timingToken =
                ZeroCodePerformanceCounters.BeginInstruction(false);
            IVertex is_v = null;

            try
            {
                is_v = InstructionHelpers.GetIs(instructionVertex);
                INoInEdgeInOutVertexVertex endpointResult = null;
                bool isExecutable =
                    is_v != null &&
                    CallableEndPointDictionary_INIEIOV_ZCE_IV_IV_B
                        .TryCallEndPoint(
                            this,
                            inputQs,
                            instructionVertex,
                            is_v,
                            out endpointResult,
                            out isStackFrameReturn);
                ZeroCodePerformanceCounters.RecordInstructionResolution(
                    isExecutable);

                if (isExecutable)  // execute if you can
                    return endpointResult;

                return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);
            }
            finally
            {
                ZeroCodePerformanceCounters.EndInstruction(
                    timingToken,
                    is_v);
            }
        }
    }
}
