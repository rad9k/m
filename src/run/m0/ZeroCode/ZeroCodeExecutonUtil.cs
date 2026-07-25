using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeExecutonUtil
    {
        private const int
            NextAtomHashSetPathThreshold = 32;

        static IVertex r = MinusZero.Instance.Root;

        static IVertex this_meta = r.Get(false, @"System\Meta\ZeroUML\this");
        static IVertex NextAtom_meta = r.Get(false, @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");

        private readonly struct ExecutionEdgeSnapshot
            : IDisposable
        {
            private readonly IEdge[] edges;

            internal ExecutionEdgeSnapshot(
                IEdge[] edges,
                int count)
            {
                this.edges = edges;
                Count = count;
            }

            internal int Count { get; }

            internal IEdge this[int index] =>
                edges[index];

            public void Dispose()
            {
                Array.Clear(
                    edges,
                    0,
                    Count);
                ArrayPool<IEdge>.Shared.Return(
                    edges);
            }
        }

        private static ExecutionEdgeSnapshot
            CaptureExecutionEdges(
                IList<IEdge> edges)
        {
            int count = edges.Count;
            IEdge[] snapshot =
                ArrayPool<IEdge>.Shared.Rent(
                    count);
            for (int index = 0;
                index < count;
                index++)
            {
                snapshot[index] =
                    edges[index];
            }

            return new ExecutionEdgeSnapshot(
                snapshot,
                count);
        }

        private static bool HasExecutableNextAtomEdge(
            IVertex vertex)
        {
            IList<IEdge> edges =
                vertex.OutEdgesRaw;
            for (int index = 0;
                index < edges.Count;
                index++)
            {
                IEdge edge = edges[index];
                if (edge.Meta == NextAtom_meta &&
                    !ZeroCodeUtil.ShouldNotExecute(edge))
                    return true;
            }

            return false;
        }


        public static void CreateExecutionAndVertexMethodExecute(IVertex endPoint, IVertex theObject)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.AddStackFrame(theObject);
            
            exe.AddStackFrame();

            exe.Stack.AddEdge(this_meta, theObject);
            
            endPoint.Execute(exe);            
        }

        public static void MethodCall(IExecution exe, IVertex endPoint, IVertex theObject, IVertex paramtersStack)
        {
            exe.AddStackFrame(theObject); // ENTER NEW STACK
            exe.AddStackFrame(paramtersStack);
            exe.Stack.AddEdge(this_meta, theObject);

            endPoint.Execute(exe);

            exe.RemoveStackFrame();
            exe.RemoveStackFrame(); // LEAVE NEW STACK
        }

        public static INoInEdgeInOutVertexVertex FuncionCall(IVertex endPoint, IVertex paramtersStack)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.AddStackFrame(paramtersStack);

            return endPoint.Execute(exe);
        }

        public static INoInEdgeInOutVertexVertex FuncionCall(IExecution exe, IVertex endPoint, IVertex paramtersStack)
        { 
            exe.AddStackFrame(paramtersStack); // ENTER NEW STACK

            INoInEdgeInOutVertexVertex ret = endPoint.Execute(exe);

            exe.RemoveStackFrame(); // LEAVE NEW STACK

            return ret;
        }


        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions(IExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            try
            {            
                bool local_isStackFrameReturn;

                INoInEdgeInOutVertexVertex possibleToReturnStack;

                IList<IEdge> directEdges =
                    baseVertex.OutEdgesRaw;
                if (directEdges.Count == 1)
                {
                    IEdge edge = directEdges[0];
                    if (edge.Meta != NextAtom_meta &&
                        !ZeroCodeUtil.ShouldNotExecute(edge))
                    {
                        possibleToReturnStack =
                            exe.ExecuteInstruction(
                                inStack,
                                edge.To,
                                out local_isStackFrameReturn);

                        if (local_isStackFrameReturn)
                        {
                            isStackFrameReturn = true;

                            return possibleToReturnStack;
                        }
                    }
                }
                else if (directEdges.Count > 1)
                {
                    IEdge singleExecutableEdge = null;
                    bool hasMultipleExecutableEdges = false;
                    for (int index = 0;
                        index < directEdges.Count;
                        index++)
                    {
                        IEdge edge = directEdges[index];
                        if (edge.Meta == NextAtom_meta ||
                            ZeroCodeUtil.ShouldNotExecute(edge))
                            continue;

                        if (singleExecutableEdge == null)
                        {
                            singleExecutableEdge = edge;
                            continue;
                        }

                        hasMultipleExecutableEdges = true;
                        break;
                    }

                    if (!hasMultipleExecutableEdges)
                    {
                        if (singleExecutableEdge != null)
                        {
                            possibleToReturnStack =
                                exe.ExecuteInstruction(
                                    inStack,
                                    singleExecutableEdge.To,
                                    out local_isStackFrameReturn);

                            if (local_isStackFrameReturn)
                            {
                                isStackFrameReturn = true;
                                return possibleToReturnStack;
                            }
                        }
                    }
                    else
                    {
                        // A snapshot is required because code can
                        // modify its own vertex while an instruction
                        // executes.
                        using ExecutionEdgeSnapshot snapshot =
                            CaptureExecutionEdges(
                                directEdges);
                        for (int index = 0;
                            index < snapshot.Count;
                            index++)
                        {
                            IEdge edge =
                                snapshot[index];
                            if (edge.Meta == NextAtom_meta ||
                                ZeroCodeUtil.ShouldNotExecute(edge))
                                continue;

                            possibleToReturnStack =
                                exe.ExecuteInstruction(
                                    inStack,
                                    edge.To,
                                    out local_isStackFrameReturn);

                            if (local_isStackFrameReturn)
                            {
                                isStackFrameReturn = true;
                                return possibleToReturnStack;
                            }
                        }
                    }
                }

                directEdges = baseVertex.OutEdgesRaw;
                if (directEdges.Count == 1)
                {
                    IEdge edge = directEdges[0];
                    if (edge.Meta != NextAtom_meta &&
                        HasExecutableNextAtomEdge(
                            edge.To))
                    {
                        possibleToReturnStack =
                            SequentiallyExecuteInstructions_NextEdges(
                                exe,
                                inStack,
                                edge.To,
                                out local_isStackFrameReturn);

                        if (local_isStackFrameReturn)
                        {
                            isStackFrameReturn = true;

                            return possibleToReturnStack;
                        }
                    }
                }
                else if (directEdges.Count > 1)
                {
                    // Take a new snapshot so mutations made in the
                    // direct-execution pass remain visible here.
                    using ExecutionEdgeSnapshot snapshot =
                        CaptureExecutionEdges(
                            directEdges);
                    for (int index = 0;
                        index < snapshot.Count;
                        index++)
                    {
                        IEdge edge =
                            snapshot[index];
                        if (edge.Meta == NextAtom_meta ||
                            !HasExecutableNextAtomEdge(
                                edge.To))
                            continue;

                        possibleToReturnStack =
                            SequentiallyExecuteInstructions_NextEdges(
                                exe,
                                inStack,
                                edge.To,
                                out local_isStackFrameReturn);

                        if (local_isStackFrameReturn)
                        {
                            isStackFrameReturn = true;

                            return possibleToReturnStack;
                        }
                    }
                }

                return inStack;
            } catch (Exception ex)
            {                             
                UserInteractionUtil.ShowException("Execution of code starting from vertex: " + GraphUtil.GetVertexIdString(baseVertex),                     
                    ex.GetType().ToString() + " : " + ex.Message + "\n" + ex.StackTrace, ZeroTypes.ExceptionLevelEnum.Error);

                return inStack;
            }
        }

        private struct NextAtomTraversalFrame
        {
            public IVertex Vertex;
            public IList<IEdge> Edges;
            public int NextIndex;
        }

        private sealed class NextAtomTraversalState
        {
            public readonly List<NextAtomTraversalFrame>
                Pending =
                    new List<NextAtomTraversalFrame>();
            public readonly HashSet<IVertex>
                ActivePath =
                    new HashSet<IVertex>();
            public bool UsesActivePathHashSet;
        }

        [ThreadStatic]
        private static Stack<NextAtomTraversalState>
            nextAtomTraversalStatePool;

        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions_NextEdges(IExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;
            NextAtomTraversalState state =
                RentNextAtomTraversalState();
            state.Pending.Add(
                CreateNextAtomTraversalFrame(
                    baseVertex));

            try
            {
                while (state.Pending.Count > 0)
                {
                    int frameIndex =
                        state.Pending.Count - 1;
                    NextAtomTraversalFrame frame =
                        state.Pending[frameIndex];

                    if (frame.NextIndex >=
                        frame.Edges.Count)
                    {
                        state.Pending.RemoveAt(
                            frameIndex);

                        if (state
                            .UsesActivePathHashSet)
                            state.ActivePath.Remove(
                                frame.Vertex);

                        continue;
                    }

                    IEdge edge =
                        frame.Edges[frame.NextIndex];
                    frame.NextIndex++;
                    state.Pending[frameIndex] =
                        frame;

                    if (edge.Meta !=
                            NextAtom_meta ||
                        ZeroCodeUtil.ShouldNotExecute(
                            edge))
                        continue;

                    INoInEdgeInOutVertexVertex
                        possibleToReturnStack =
                            exe.ExecuteInstruction(
                                inStack,
                                edge.To,
                                out bool
                                    local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                    {
                        isStackFrameReturn = true;
                        return possibleToReturnStack;
                    }

                    if (!TryEnterNextAtomVertex(
                        state,
                        edge.To))
                        continue;

                    state.Pending.Add(
                        CreateNextAtomTraversalFrame(
                            edge.To));
                }

                return inStack;
            }
            finally
            {
                ReturnNextAtomTraversalState(
                    state);
            }
        }

        private static NextAtomTraversalFrame
            CreateNextAtomTraversalFrame(
                IVertex vertex)
        {
            return new NextAtomTraversalFrame
            {
                Vertex = vertex,
                Edges = vertex.OutEdges,
                NextIndex = 0
            };
        }

        private static bool
            TryEnterNextAtomVertex(
                NextAtomTraversalState state,
                IVertex vertex)
        {
            if (state.UsesActivePathHashSet)
                return state.ActivePath.Add(vertex);

            for (int index = 0;
                index < state.Pending.Count;
                index++)
                if (ReferenceEquals(
                    state.Pending[index].Vertex,
                    vertex))
                    return false;

            if (state.Pending.Count <
                NextAtomHashSetPathThreshold)
                return true;

            for (int index = 0;
                index < state.Pending.Count;
                index++)
                state.ActivePath.Add(
                    state.Pending[index].Vertex);

            state.UsesActivePathHashSet = true;
            return state.ActivePath.Add(vertex);
        }

        private static NextAtomTraversalState
            RentNextAtomTraversalState()
        {
            Stack<NextAtomTraversalState> pool =
                nextAtomTraversalStatePool;

            if (pool != null &&
                pool.Count > 0)
                return pool.Pop();

            return new NextAtomTraversalState();
        }

        private static void
            ReturnNextAtomTraversalState(
                NextAtomTraversalState state)
        {
            state.Pending.Clear();
            state.ActivePath.Clear();
            state.UsesActivePathHashSet = false;

            if (state.Pending.Capacity > 4096)
                return;

            Stack<NextAtomTraversalState> pool =
                nextAtomTraversalStatePool;

            if (pool == null)
            {
                pool =
                    new Stack<
                        NextAtomTraversalState>();
                nextAtomTraversalStatePool =
                    pool;
            }

            pool.Push(state);
        }
    }
}
