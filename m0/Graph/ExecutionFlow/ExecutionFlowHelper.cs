using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.ZeroUML.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static m0.Graph.GraphUtil;

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


        public static void ExecutionHelperInitialize()
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
            Lib.Sys.StartTransaction(null);
        }

        public static void RollbackTransaction()
        {
            Lib.Sys.RollbackTransaction(null);
        }

        public static void CommitTransaction()
        {
            Lib.Sys.CommitTransaction(null);
        }

        public static void AddTransactionAtom(ITransactionAtom atom)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            if(currentTransaction != null)
                currentTransaction.AddAtom(atom);
        }

        public static IVertex AddGraphChangeTrigger(IVertex baseVertex, string scopeQuery)
        {
            IVertex trigger = VertexOperations.AddInstance(baseVertex, graphChangeTrigger_meta);

            if (scopeQuery != null)
                trigger.AddVertex(scopeQuery_meta, scopeQuery);

            return trigger;
        }


        public static void AddListener_DotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName)
        {
            IVertex listener = baseVertex.AddVertex(listener_meta, "");

            DecorateWithDotNetStaticMethod(listener, _typeName, _methodName);
        }

        public static void AddListener_DotNetDelegate(IVertex baseVertex, DotNetDelegate _delegate)
        {
            IVertex listener = baseVertex.AddVertex(listener_meta, "");

            DecorateWithDotNetDelegate(listener, _delegate);
        }

        public static void AddListener_Delegate(IVertex baseVertex, IVertex _object, IVertex _method)
        {
            IVertex listener = baseVertex.AddVertex(listener_meta, "");

            DecorateWithDelegate(listener, _object, _method);
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
