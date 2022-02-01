using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System.Collections.Generic;

namespace m0.Graph.ExecutionFlow
{
    public delegate void EdgeHandler(IEdge edge);

    public class EdgeAddRemoveDisposeHandlers
    {
        public IVertex fromVertex;
        public EdgeHandler AddHandler;
        public EdgeHandler RemoveHandler;
        public EdgeHandler DisposeHandler;

        public EdgeAddRemoveDisposeHandlers(IVertex _fromVertex, 
            EdgeHandler _AddHandler, 
            EdgeHandler _RemoveHandler,
            EdgeHandler _DisposeHandler)
        {
            fromVertex = _fromVertex;
            AddHandler = _AddHandler;
            RemoveHandler = _RemoveHandler;
            DisposeHandler = _DisposeHandler;
        }
    }

    public class ExecutionFlowHelper
    {
        static IVertex _is_meta;

        static IVertex dotNetEndPoint_meta;
        static IVertex typeName_meta;
        static IVertex methodName_meta;

        static IVertex dotNetDelegate_meta;
        static IVertex dotNetDelegatePointer_meta;

        static IVertex _delegate_meta;
        static IVertex object_meta;
        static IVertex method_meta;

        static IVertex listener_meta;


        public static void Initialize()
        {
            IVertex r = m0.MinusZero.Instance.root;

            dotNetEndPoint_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod");
            typeName_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod\DotNetTypeName");
            methodName_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod\DotNetMethodName");
            _is_meta = r.Get(false, @"System\Meta\Base\Vertex\$Is");

            dotNetDelegate_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetDelegate");
            dotNetDelegatePointer_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetDelegate\DotNetDelegatePointer");

            _delegate_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\Delegate");
            object_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\Delegate\Object");
            method_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\Delegate\Method");

            listener_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\EventTrigger\Listener");
        }

        public static void StartTransaction()
        {
            IExecution exe = new ZeroCodeExecution();

            Lib.Sys.StartTransaction(exe);
        }

        public static void RollbackTransaction()
        {
            IExecution exe = new ZeroCodeExecution();

            Lib.Sys.RollbackTransaction(exe);
        }

        public static void CommitTransaction()
        {
            IExecution exe = new ZeroCodeExecution();

            Lib.Sys.CommitTransaction(exe);
        }

        public static void GraphChangeWatchOff()
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            currentTransaction.GraphChangeWatch = false;
        }

        public static void GraphChangeWatchOn()
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            currentTransaction.GraphChangeWatch = true;
        }

        public static void AddTransactionAtom(ITransactionAtom atom)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            if (currentTransaction != null)
                currentTransaction.AddAtom(atom);
        }

        public static void AddSecondStageCommitAction(ISecondStageCommitAction commitAction)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            if (currentTransaction != null)
                currentTransaction.AddSecondStageCommitAction(commitAction);
        }

        public static IEdge AddListener_DotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName)
        {
            return AddListener_DotNetStaticMethod(baseVertex, _typeName, _methodName, "");
        }

        public static IEdge AddListener_DotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName, string listenerName)
        {
            IEdge listenerEdge = baseVertex.AddVertexAndReturnEdge(listener_meta, listenerName);

            if (listenerEdge != null)
                DecorateWithDotNetStaticMethod(listenerEdge.To, _typeName, _methodName);

            return listenerEdge;
        }

        public static IEdge AddListener_DotNetDelegate(IVertex baseVertex, DotNetDelegate _delegate)
        {
            return AddListener_DotNetDelegate(baseVertex, _delegate, "");
        }

        public static IEdge AddListener_DotNetDelegate(IVertex baseVertex, DotNetDelegate _delegate, string listenerName)
        {
            IEdge listenerEdge = baseVertex.AddVertexAndReturnEdge(listener_meta, listenerName);

            if (listenerEdge != null)
                DecorateWithDotNetDelegate(listenerEdge.To, _delegate);

            return listenerEdge;
        }

        public static IEdge AddListener_Delegate(IVertex baseVertex, IVertex _object, IVertex _method)
        {
            return AddListener_Delegate(baseVertex, _object, _method, "");
        }

        public static IEdge AddListener_Delegate(IVertex baseVertex, IVertex _object, IVertex _method, string listenerName)
        {
            IEdge listenerEdge = baseVertex.AddVertexAndReturnEdge(listener_meta, listenerName);

            if (listenerEdge != null)
                DecorateWithDelegate(listenerEdge.To, _object, _method);

            return listenerEdge;
        }

        public static void RemoveGraphChangeListener(IEdge graphChangeListenerEdge)
        {
            GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);
        }

        public static void DecorateWithDotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName)
        {
            baseVertex.AddEdge(_is_meta, dotNetEndPoint_meta);

            baseVertex.AddVertex(typeName_meta, _typeName);

            baseVertex.AddVertex(methodName_meta, _methodName);
        }

        public delegate INoInEdgeInOutVertexVertex DotNetDelegate(IExecution exe);

        public static void DecorateWithDotNetDelegate(IVertex baseVertex, DotNetDelegate _delegate)
        {
            baseVertex.AddEdge(_is_meta, dotNetDelegate_meta);

            baseVertex.AddVertex(dotNetDelegatePointer_meta, _delegate);
        }

        public static void DecorateWithDelegate(IVertex baseVertex, IVertex _object, IVertex _method)
        {
            baseVertex.AddEdge(_is_meta, _delegate_meta);

            baseVertex.AddEdge(object_meta, _object);

            baseVertex.AddEdge(method_meta, _method);
        }

        public static INoInEdgeInOutVertexVertex ExecuteDotNetDelegate(IVertex baseVertex, IExecution exe)
        {
            IVertex dotNetDelegatePointer = GraphUtil.GetQueryOutFirst(baseVertex, "DotNetDelegatePointer", null);

            if(dotNetDelegatePointer != null && dotNetDelegatePointer.Value is DotNetDelegate)
            {
                DotNetDelegate del = (DotNetDelegate)dotNetDelegatePointer.Value;

                del.Invoke(exe);
            }

            return null;
        }

        public static INoInEdgeInOutVertexVertex ExecuteDelegate(IVertex baseVertex, IExecution exe)
        {
            IVertex _object = GraphUtil.GetQueryOutFirst(baseVertex, "Object", null);
            IVertex method = GraphUtil.GetQueryOutFirst(baseVertex, "Method", null);

            if(_object != null && method != null)
            {
                exe.AddStackFrame(_object); // this is WRONG XXX as we have method call parameters allready on stack
                // this means that _object edges will potentially overwrite call paramaters
                // no way to do it otherwise althought

                method.Execute(exe);

                exe.RemoveStackFrame();
            }

            return null;
        }

        public static INoInEdgeInOutVertexVertex Execute(IVertex baseVertex, IExecution exe)
        {
            bool dummy;

            if (InstructionHelpers.CheckIfIsInherits(baseVertex, "Executable"))
            {
                if(InstructionHelpers.CheckIfIs(baseVertex, "DotNetStaticMethod"))
                    return CallableEndPointDictionary_INIEIOV_ZCE.CallEndPoint(exe, baseVertex);

                if (InstructionHelpers.CheckIfIs(baseVertex, "DotNetDelegate"))
                    return ExecuteDotNetDelegate(baseVertex, exe);

                if (InstructionHelpers.CheckIfIs(baseVertex, "Delegate"))
                    return ExecuteDelegate(baseVertex, exe);

                return null;
            }
            else
                return InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.Stack, baseVertex, out dummy, false);
        }

        public static bool IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(IVertex stack, IVertex from, string meta)
        {
            foreach(IEdge e in stack.GetAll(false, @"event:\ChangedVertex:"))
                if (GraphUtil.ExistQueryIn(e.To, meta, null))
                    return true;

            IVertex sameMetaEdges = stack.GetAll(false, @"event:\Edge:{Meta:" + meta+"}");

            foreach (IEdge e in sameMetaEdges)
                if (GraphUtil.GetQueryOutFirst(e.To, "From", null) == from)
                    return true;

          //  if (stack.Get(false, @"event:\Edge:\Meta:" + meta) != null)
            //    return true;

            return false;
        }

        public static bool IsVertexChange(IVertex stack, string meta)
        {
            foreach (IEdge e in stack.GetAll(false, @"event:\ChangedVertex:"))
                if (GraphUtil.ExistQueryIn(e.To, meta, null))
                    return true;

            return false;
        }

        public static bool IsVertexChange(IVertex stack, IVertex vertex)
        {
            foreach (IEdge e in stack.GetAll(false, @"event:\ChangedVertex:"))
                if (e.To == vertex)
                    return true;

            return false;
        }

        public static bool IsVertexChageOrEdgeAddedRemovedDisposedFromTo(IVertex stack, IVertex toFrom)
        {
            foreach (IEdge e in stack.GetAll(false, @"event:\ChangedVertex:"))
                if (e.To == toFrom)
                    return true;

            foreach (IEdge e in stack.GetAll(false, @"event:\Edge:\From:"))
                if (e.To == toFrom)
                    return true;

            foreach (IEdge e in stack.GetAll(false, @"event:\Edge:\To:"))
                if (e.To == toFrom)
                    return true;

            return false;
        }

        public static bool IsEdgeAddedRemovedDiscardedFrom(IVertex stack, IVertex toFrom)
        {
            foreach (IEdge e in stack.GetAll(false, @"event:\Edge:\From:"))
                if (e.To == toFrom)
                    return true;

            return false;
        }

        public static bool IsEdgeAddedTo(IVertex stack, IVertex toFrom)
        {
            foreach (IEdge eventEdge in stack.GetAll(false, @"event:"))
            {
                IVertex eventFrom = eventEdge.To.Get(false, @"Edge:\From:");

                if(eventFrom == toFrom)
                {
                    IVertex eventType = eventEdge.To.Get(false, @"Type:");

                    if (eventType != null && GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeAdded"))
                        return true;
                }
            }

            return false;
        }

        public static List<IVertex> GetEdgesAddedTo(IVertex stack, IVertex toFrom)
        {
            List<IVertex> edgesList = new List<IVertex>();

            foreach (IEdge eventEdge in stack.GetAll(false, @"event:"))
            {
                IVertex edgeVertex = eventEdge.To.Get(false, @"Edge:");
                IVertex eventFrom = edgeVertex.Get(false, @"From:");

                if (eventFrom == toFrom)
                {
                    IVertex eventType = eventEdge.To.Get(false, @"Type:");

                    if (eventType != null && GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeAdded"))
                        edgesList.Add(edgeVertex);
                }
            }

            return edgesList;
        }

        public static bool IsEdgeRemovedFrom(IVertex stack, IVertex toFrom)
        {
            foreach (IEdge eventEdge in stack.GetAll(false, @"event:"))
            {
                IVertex eventFrom = eventEdge.To.Get(false, @"Edge:\From:");

                if (eventFrom == toFrom)
                {
                    IVertex eventType = eventEdge.To.Get(false, @"Type:");

                    if (eventType != null && GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeRemoved"))
                        return true;
                }
            }

            return false;
        }

        public static List<IVertex> GetEdgesRemovedFrom(IVertex stack, IVertex toFrom)
        {
            List<IVertex> edgesList = new List<IVertex>();

            foreach (IEdge eventEdge in stack.GetAll(false, @"event:"))
            {
                IVertex edgeVertex = eventEdge.To.Get(false, @"Edge:");
                IVertex eventFrom = edgeVertex.Get(false, @"From:");

                if (eventFrom == toFrom)
                {
                    IVertex eventType = eventEdge.To.Get(false, @"Type:");

                    if (eventType != null && GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeRemoved"))
                        edgesList.Add(edgeVertex);
                }
            }

            return edgesList;
        }

        public static bool IsEdgeDisposedFrom(IVertex stack, IVertex toFrom)
        {
            foreach (IEdge eventEdge in stack.GetAll(false, @"event:"))
            {
                IVertex eventFrom = eventEdge.To.Get(false, @"Edge:\From:");

                if (eventFrom == toFrom)
                {
                    IVertex eventType = eventEdge.To.Get(false, @"Type:");

                    if (eventType != null && GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeDisposed"))
                        return true;
                }
            }

            return false;
        }

        public static List<IVertex> GetEdgesDisposedFrom(IVertex stack, IVertex toFrom)
        {
            List<IVertex> edgesList = new List<IVertex>();

            foreach (IEdge eventEdge in stack.GetAll(false, @"event:"))
            {
                IVertex edgeVertex = eventEdge.To.Get(false, @"Edge:");
                IVertex eventFrom = edgeVertex.Get(false, @"From:");

                if (eventFrom == toFrom)
                {
                    IVertex eventType = eventEdge.To.Get(false, @"Type:");

                    if (eventType != null && GraphUtil.GetValueAndCompareStrings(eventType, "OutputEdgeDisposed"))
                        edgesList.Add(edgeVertex);
                }
            }

            return edgesList;
        }

        public static void RunAddRemoveDisposeHandlers(IVertex stack, List<EdgeAddRemoveDisposeHandlers> handlers)
        {
            foreach (IEdge _event in stack.GetAll(false, @"event:"))
            {
                IVertex eventEdge = GraphUtil.GetQueryOutFirst(_event.To, "Edge", null);

                if (eventEdge == null)
                    continue;

                IVertex eventEdgeFrom = GraphUtil.GetQueryOutFirst(eventEdge, "From", null);                

                foreach(EdgeAddRemoveDisposeHandlers h in handlers)
                    if (eventEdgeFrom == h.fromVertex)
                    {
                        IVertex eventType = _event.To.Get(false, @"Type:");

                        if(eventType != null)
                            switch (eventType.Value)
                            {
                                case "OutputEdgeAdded":
                                    h.AddHandler(Edge.CreateIEdgeFromEdgeVertex(eventEdge));
                                    break;

                                case "OutputEdgeRemoved":
                                    h.RemoveHandler(Edge.CreateIEdgeFromEdgeVertex(eventEdge));
                                    break;

                                case "OutputEdgeDisposed":
                                    h.DisposeHandler(Edge.CreateIEdgeFromEdgeVertex(eventEdge));
                                    break;
                            }                     
                    }
            }
        }

        public static bool AllEventsSubVisualiser(IVertex stack)
        {
            foreach (IEdge _event in stack.GetAll(false, @"event:"))
            {
                IVertex eventEdge = GraphUtil.GetQueryOutFirst(_event.To, "Edge", null);

                if (eventEdge == null)
                    return false;

                IVertex eventEdgeFrom = GraphUtil.GetQueryOutFirst(eventEdge, "From", null);

            }

            return true;
        }
    }
}
