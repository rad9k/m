using m0.Foundation;
using m0.Graph;
using m0.User.Process.UX;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Store.UserCommands
{
    internal class StoreUserCommands
    {
        static IVertex r = MinusZero.Instance.Root;

        static IVertex file_meta = r.Get(false, @"System\Meta\Store\FileSystem\Directory\File");

        public static INoInEdgeInOutVertexVertex OnNewM0JStore(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseVertexEdge = GraphUtil.GetQueryOutFirst(stack, "baseVertex", null);
            IVertex baseVertex = GraphUtil.GetQueryOutFirst(baseVertexEdge, "To", null);

            if (baseVertex == null)
                return exe.Stack;

            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0j"))
                    storeName += ".m0j";

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////                

                baseVertex.AddVertex(file_meta, storeName);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }

            return null;
        }

        public static INoInEdgeInOutVertexVertex OnNewM0XStore(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseVertexEdge = GraphUtil.GetQueryOutFirst(stack, "baseVertex", null);
            IVertex baseVertex = GraphUtil.GetQueryOutFirst(baseVertexEdge, "To", null);

            if (baseVertex == null)
                return exe.Stack;

            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0x"))
                    storeName += ".m0x";

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////                

                baseVertex.AddVertex(file_meta, storeName);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex OnNewM0TStore(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseVertexEdge = GraphUtil.GetQueryOutFirst(stack, "baseVertex", null);
            IVertex baseVertex = GraphUtil.GetQueryOutFirst(baseVertexEdge, "To", null);

            if (baseVertex == null)
                return exe.Stack;

            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0t"))
                    storeName += ".m0t";

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////                

                baseVertex.AddVertex(file_meta, storeName);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }

            return exe.Stack;
        }
    }
}
