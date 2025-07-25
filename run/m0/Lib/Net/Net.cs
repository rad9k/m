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

            HttpServer server = GetServer(thisVertex);

            IVertex portVertex = GraphUtil.GetQueryOutFirst(thisVertex, "Port", null);

            int port = GraphUtil.GetIntegerValueOr0(portVertex);

            server.StartAsync("http://localhost:" + port);

            return exe.Stack;
        }

        private static HttpServer GetServer(IVertex thisVertex)
        {
            HttpServer server = null;

            if (ServerInstances.ContainsKey(thisVertex))
                server = ServerInstances[thisVertex];
            else
            {
                IVertex mappingVertex = GraphUtil.GetQueryOutFirst(thisVertex, "Mapping", null);

                if (mappingVertex == null)
                    return null;

                server = CreateServer(mappingVertex);

                ServerInstances.Add(thisVertex, server);
            }

            return server;
        }

        private static HttpServer CreateServer(IVertex mappingVertex)
        {
            return new HttpServer(mappingVertex);
        }

        public static INoInEdgeInOutVertexVertex HttpServer_Stop(IExecution exe)
        {
            IVertex thisVertex = GraphUtil.GetQueryOutFirst(exe.Stack, "this", null);

            if (thisVertex == null)
                return exe.Stack;

            HttpServer server = GetServer(thisVertex);

            //server.StopAsync();

            return exe.Stack;
        }
    }
}
