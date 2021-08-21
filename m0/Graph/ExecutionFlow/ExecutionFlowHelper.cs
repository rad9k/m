using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System.Collections.Generic;

namespace m0.Graph.ExecutionFlow
{
    public class ExecutionFlowHelper
    {
        static IVertex graphChangeTrigger_meta;
        static IVertex scopeQuery_meta;

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

            graphChangeTrigger_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger");
            scopeQuery_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger\ScopeQuery");

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

        public static IEdge AddEventTriggerAndListener(IVertex baseVertex, IList<string> scopeQueries, string triggerVertexName, DotNetDelegate _delegate, string listenerName)
        {
            IEdge graphChangeTriggerEdge = ExecutionFlowHelper.AddGraphChangeTrigger(baseVertex, scopeQueries, triggerVertexName);
            
            return ExecutionFlowHelper.AddListener_DotNetDelegate(graphChangeTriggerEdge.To, _delegate, listenerName);
        }

        public static IEdge AddGraphChangeTrigger(IVertex baseVertex, IList<string> scopeQueries)
        {
            return AddGraphChangeTrigger(baseVertex, scopeQueries, null);
        }

        public static IEdge AddGraphChangeTrigger(IVertex baseVertex, IList<string> scopeQueries, string triggerVertexName)
        {
            IEdge triggerEdge = null;

            if (triggerVertexName != null)
            {
                IVertex existingTriggers = baseVertex.GetAll(false, "$GraphChangeTrigger:" + triggerVertexName);

                if (existingTriggers.OutEdges.Count == 1)
                    triggerEdge = existingTriggers.OutEdges[0];
            }

            if (triggerEdge == null)
            {
                triggerEdge = VertexOperations.AddInstanceAndReturnEdge(baseVertex, graphChangeTrigger_meta);
                triggerEdge.To.Value = triggerVertexName;
            }

            if (scopeQueries != null)
                foreach (string s in scopeQueries)
                    triggerEdge.To.AddVertex(scopeQuery_meta, s);

            return triggerEdge;
        }

        public static void RemoveGraphChangeListener(IEdge listenerEdge)
        {
            IVertex triggerVertex = listenerEdge.From;

            triggerVertex.DeleteEdge(listenerEdge);

            if(GraphUtil.GetQueryOutCount(triggerVertex, "Listener", null) == 0)
            {
                IEdge triggerSourceEdge = GraphUtil.GetQueryInFirstEdge(triggerVertex, "$GraphChangeTrigger", null);

                if(triggerSourceEdge != null)
                    triggerSourceEdge.From.DeleteEdge(triggerSourceEdge);
            }
        }

        public static IEdge AddListener_DotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName)
        {
            return AddListener_DotNetStaticMethod(baseVertex, _typeName, _methodName, "");
        }

        public static IEdge AddListener_DotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName, string listenerName)
        {
            IEdge listenerEdge = baseVertex.AddVertexAndReturnEdge(listener_meta, listenerName);

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

            DecorateWithDelegate(listenerEdge.To, _object, _method);

            return listenerEdge;
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
    }
}
