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

        internal sealed class RedirectAssignmentCacheEntry
        {
            internal RedirectAssignmentCacheEntry(
                IEdge targetEdge,
                object scalarPlan)
            {
                TargetEdge = targetEdge;
                ScalarPlan = scalarPlan;
            }

            internal IEdge TargetEdge { get; set; }

            internal object ScalarPlan { get; set; }

            internal object PropagationPlan { get; set; }

            internal NoInEdgeInOutVertexVertex
                ValidatedCurrentFrame;

            internal NoInEdgeInOutVertexVertex
                ValidatedParentFrame;

            internal long ValidatedCurrentFrameVersion;

            internal long ValidatedParentFrameVersion;

            internal int ValidatedFrameCount;

            internal bool ValidatedTargetIsExclusive;

            internal void ClearTargetValidation()
            {
                ValidatedCurrentFrame = null;
                ValidatedParentFrame = null;
                ValidatedCurrentFrameVersion = 0;
                ValidatedParentFrameVersion = 0;
                ValidatedFrameCount = 0;
                ValidatedTargetIsExclusive = false;
            }
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
            out object scalarPlan,
            out RedirectAssignmentCacheEntry cacheEntry,
            out bool targetIsExclusive)
        {
            targetEdge = null;
            scalarPlan = null;
            cacheEntry = null;
            targetIsExclusive = false;
            if (redirectAssignmentTargetCache == null ||
                !redirectAssignmentTargetCache.TryGetValue(
                    instructionVertex,
                    out RedirectAssignmentCacheEntry entry))
                return false;

            bool targetIsValid;
            if (entry.TargetEdge?.To is EasyVertex &&
                entry.TargetEdge.From is
                    NoInEdgeInOutVertexVertex &&
                Stack is
                    NoInEdgeInOutVertexVertex currentFrame)
            {
                if (TryGetCachedActiveStackTargetValidation(
                        entry.TargetEdge,
                        entry,
                        currentFrame,
                        out targetIsExclusive))
                {
                    targetIsValid = true;
                }
                else
                {
                    targetIsValid =
                        TryValidateActiveStackTarget(
                            entry.TargetEdge,
                            entry,
                            out _,
                            out targetIsExclusive);
                }
            }
            else
            {
                targetIsValid =
                    IsCachedAssignmentTargetValid(
                        entry.TargetEdge);
            }

            if (!targetIsValid)
                return false;

            targetEdge = entry.TargetEdge;
            scalarPlan = entry.ScalarPlan;
            cacheEntry = entry;
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

        internal bool TryUpdateExclusiveScalarAssignmentTarget(
            IEdge targetEdge,
            object value,
            bool targetIsValidatedExclusive = false)
        {
            return TryGetExclusiveScalarAssignmentTarget(
                    targetEdge,
                    targetIsValidatedExclusive,
                    out EasyVertex targetVertex) &&
                targetVertex
                    .TryUpdateExclusiveEphemeralValue(
                        value);
        }

        internal bool TryUpdateExclusiveScalarAssignmentTarget(
            IEdge targetEdge,
            EasyVertex.ScalarNumericValue value,
            bool targetIsValidatedExclusive = false)
        {
            return TryGetExclusiveScalarAssignmentTarget(
                    targetEdge,
                    targetIsValidatedExclusive,
                    out EasyVertex targetVertex) &&
                targetVertex
                    .TryUpdateExclusiveEphemeralValue(
                        value);
        }

        private bool TryGetExclusiveScalarAssignmentTarget(
            IEdge targetEdge,
            bool targetIsValidatedExclusive,
            out EasyVertex targetVertex)
        {
            if (targetIsValidatedExclusive)
            {
                targetVertex =
                    targetEdge?.To as EasyVertex;
                return targetVertex != null;
            }

            return TryValidateActiveStackTarget(
                    targetEdge,
                    null,
                    out targetVertex,
                    out bool targetIsExclusive) &&
                targetIsExclusive;
        }

        private bool TryValidateActiveStackTarget(
            IEdge targetEdge,
            RedirectAssignmentCacheEntry cacheEntry,
            out EasyVertex targetVertex,
            out bool targetIsExclusive)
        {
            targetIsExclusive = false;
            if (!(targetEdge?.From is
                    NoInEdgeInOutVertexVertex sourceFrame) ||
                !(targetEdge.To is EasyVertex localTargetVertex) ||
                !(Stack is
                    NoInEdgeInOutVertexVertex currentFrame))
            {
                targetVertex = null;
                return false;
            }

            targetVertex = localTargetVertex;
            if (TryGetCachedActiveStackTargetValidation(
                    targetEdge,
                    cacheEntry,
                    currentFrame,
                    out targetIsExclusive))
            {
                return true;
            }

            bool foundTargetEdge = false;
            int targetReferenceCount = 0;
            int traversedFrameCount = 0;
            HashSet<IVertex> visitedFrames = null;
            NoInEdgeInOutVertexVertex
                validatedCurrentFrame = null;
            NoInEdgeInOutVertexVertex
                validatedParentFrame = null;
            long validatedCurrentFrameVersion = 0;
            long validatedParentFrameVersion = 0;
            int validatedFrameCount = 0;
            bool canCacheValidation =
                cacheEntry != null;

            while (currentFrame != null)
            {
                if (canCacheValidation)
                {
                    long frameVersion =
                        currentFrame
                            .EnableTrackedLocalMutationVersion();
                    if (validatedFrameCount == 0)
                    {
                        validatedCurrentFrame =
                            currentFrame;
                        validatedCurrentFrameVersion =
                            frameVersion;
                    }
                    else if (validatedFrameCount == 1)
                    {
                        validatedParentFrame =
                            currentFrame;
                        validatedParentFrameVersion =
                            frameVersion;
                    }
                    else
                    {
                        canCacheValidation = false;
                    }

                    validatedFrameCount++;
                }

                IList<IEdge> edges =
                    currentFrame.OutEdgesRaw;
                for (int index = 0;
                    index < edges.Count;
                    index++)
                {
                    IEdge edge = edges[index];
                    if (ReferenceEquals(
                            edge,
                            targetEdge))
                    {
                        foundTargetEdge = true;
                    }

                    if (ReferenceEquals(
                            edge.To,
                            targetVertex))
                    {
                        targetReferenceCount++;
                    }
                }

                if (ReferenceEquals(
                        currentFrame,
                        sourceFrame))
                {
                    sourceFrame = null;
                }

                IVertex parentFrame =
                    currentFrame.GetParentStackFrame();
                currentFrame =
                    parentFrame as
                        NoInEdgeInOutVertexVertex;
                if (currentFrame == null)
                    break;

                traversedFrameCount++;
                if (traversedFrameCount < 1024)
                    continue;

                visitedFrames ??=
                    new HashSet<IVertex>(
                        ReferenceEqualityComparer.Instance);
                if (!visitedFrames.Add(
                        currentFrame))
                {
                    targetIsExclusive = false;
                    return false;
                }
            }

            bool targetIsValid =
                sourceFrame == null &&
                foundTargetEdge;
            targetIsExclusive =
                targetIsValid &&
                targetReferenceCount == 1;
            if (targetIsValid &&
                canCacheValidation &&
                validatedFrameCount <= 2)
            {
                CacheActiveStackTargetValidation(
                    cacheEntry,
                    validatedCurrentFrame,
                    validatedCurrentFrameVersion,
                    validatedParentFrame,
                    validatedParentFrameVersion,
                    validatedFrameCount,
                    targetIsExclusive);
            }

            return targetIsValid;
        }

        private bool TryGetCachedActiveStackTargetValidation(
            IEdge targetEdge,
            RedirectAssignmentCacheEntry cacheEntry,
            NoInEdgeInOutVertexVertex currentFrame,
            out bool targetIsExclusive)
        {
            targetIsExclusive = false;
            if (cacheEntry == null ||
                cacheEntry.ValidatedFrameCount <= 0 ||
                !ReferenceEquals(
                    cacheEntry.TargetEdge,
                    targetEdge) ||
                !ReferenceEquals(
                    cacheEntry.ValidatedCurrentFrame,
                    currentFrame) ||
                cacheEntry
                    .ValidatedCurrentFrame
                    .TrackedLocalMutationVersion !=
                    cacheEntry
                        .ValidatedCurrentFrameVersion)
            {
                return false;
            }

            if (cacheEntry.ValidatedFrameCount == 2 &&
                (cacheEntry.ValidatedParentFrame == null ||
                    cacheEntry
                        .ValidatedParentFrame
                        .TrackedLocalMutationVersion !=
                        cacheEntry
                            .ValidatedParentFrameVersion))
            {
                return false;
            }

            targetIsExclusive =
                cacheEntry.ValidatedTargetIsExclusive;
            return true;
        }

        private static void CacheActiveStackTargetValidation(
            RedirectAssignmentCacheEntry cacheEntry,
            NoInEdgeInOutVertexVertex currentFrame,
            long currentFrameVersion,
            NoInEdgeInOutVertexVertex parentFrame,
            long parentFrameVersion,
            int frameCount,
            bool targetIsExclusive)
        {
            cacheEntry.ValidatedCurrentFrame =
                currentFrame;
            cacheEntry.ValidatedCurrentFrameVersion =
                currentFrameVersion;
            cacheEntry.ValidatedParentFrame =
                parentFrame;
            cacheEntry.ValidatedParentFrameVersion =
                parentFrameVersion;
            cacheEntry.ValidatedFrameCount =
                frameCount;
            cacheEntry.ValidatedTargetIsExclusive =
                targetIsExclusive;
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
            object scalarPlan,
            object propagationPlan,
            RedirectAssignmentCacheEntry cacheEntry)
        {
            if (!(targetEdge?.From is
                INoInEdgeInOutVertexVertex))
                return;

            if (cacheEntry != null)
            {
                if (!ReferenceEquals(
                    cacheEntry.TargetEdge,
                    targetEdge))
                {
                    cacheEntry.ClearTargetValidation();
                }

                cacheEntry.TargetEdge = targetEdge;
                cacheEntry.ScalarPlan = scalarPlan;
                cacheEntry.PropagationPlan =
                    propagationPlan;
                return;
            }

            redirectAssignmentTargetCache ??=
                new Dictionary<
                    IVertex,
                    RedirectAssignmentCacheEntry>(
                    ReferenceEqualityComparer.Instance);
            redirectAssignmentTargetCache[
                instructionVertex] =
                    new RedirectAssignmentCacheEntry(
                        targetEdge,
                        scalarPlan)
                    {
                        PropagationPlan =
                            propagationPlan
                    };
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
