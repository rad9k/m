using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.REST
{
    public class RemoteServer
    {
        static IVertex root = MinusZero.Instance.Root;
        static IVertex is_meta = root.Get(false, @"System\Meta\Base\Vertex\$Is");

        public static INoInEdgeInOutVertexVertex CallRemoteRestServer(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex target = VertexOperations.GetTargetFromStackTop(stack);

            IVertex RemoteEndpointPathVertex = GraphUtil.GetQueryOutFirst(target, "RemoteEndpointPath", null);
            IVertex RemoteEndpointParametersVertex = GraphUtil.GetQueryOutFirst(target, "RemoteEndpointParameters", null);
            
            IVertex PackageVertex = GraphUtil.GetQueryInFirst(target, "Function", null);

            IVertex RemoteServerUrlVertex = GraphUtil.GetQueryOutFirst(PackageVertex, "RemoteServerUrl", null);

            

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();            

            return newStack;
        }
    }
}
