using m0.Foundation;
using m0.Graph;
using m0.Network.Server;
using m0.ZeroCode.Helpers;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace m0.Lib.Net
{
    public class Net
    {
        static IDictionary<IVertex, HttpServer> ServerInstances = new Dictionary<IVertex, HttpServer>();

        
        public static INoInEdgeInOutVertexVertex UrlDecode(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "url", null);                       

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
                newStack.AddVertex(null, HttpUtility.UrlDecode(GraphUtil.GetStringValue(e.To)));

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex HttpServer_Start(IExecution exe)
        {
            IVertex thisVertex = GraphUtil.GetQueryOutFirst(exe.Stack, "this", null);

            if (thisVertex == null)
                return exe.Stack;

            HttpServer server = GetServer(thisVertex);

            //            
            
            if (server.DoHttps)
                server.StartAsync("https://localhost:" + server.Port);
            else
                server.StartAsync("http://localhost:" + server.Port);

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

                server = CreateServer(thisVertex);

                ServerInstances.Add(thisVertex, server);
            }

            return server;
        }

        private static HttpServer CreateServer(IVertex thisVertex)
        {
            return new HttpServer(thisVertex);
        }

        public static INoInEdgeInOutVertexVertex HttpServer_Stop(IExecution exe)
        {
            IVertex thisVertex = GraphUtil.GetQueryOutFirst(exe.Stack, "this", null);

            if (thisVertex == null)
                return exe.Stack;

            HttpServer server = GetServer(thisVertex);

            //

            server.StopAsync();

            return exe.Stack;
        }
    }
}
