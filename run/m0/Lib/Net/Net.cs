using m0.Foundation;
using m0.Graph;
using m0.Network.Server;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.Net
{
    public class Net
    {
        static IDictionary<IVertex, HttpServer> ServerInstances = new Dictionary<IVertex, HttpServer>();

        public static INoInEdgeInOutVertexVertex HttpServer_Start(IExecution exe)
        {
            IVertex thisVertex = GraphUtil.GetQueryOutFirst(exe.Stack, "this", null);

            if (thisVertex == null)
                return exe.Stack;

            HttpServer server = null;

            if (ServerInstances.ContainsKey(thisVertex))
                server = ServerInstances[thisVertex];
            else
                server = CreateNamedPipeServerStreamContext()

                return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex HttpServer_Stop(IExecution exe)
        {
            return exe.Stack;
        }
    }
}
