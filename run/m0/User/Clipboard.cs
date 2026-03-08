using m0.Foundation;
using m0.Graph;
using m0.User.Process.UX;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.User
{
    public class Clipboard
    {
        static IVertex ClipboardCut_meta = m0.MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\User\Session\ClipboardCut");
        static IVertex ClipboardCopy_meta = m0.MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\User\Session\ClipboardCopy");

        public static void ClearClipboard()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"Home:\CurrentUser:\CurrentSession:");

            IEnumerable<IEdge> allClipboard = currenSession.GetAll(false, @"ClipboardCut:");

            foreach (IEdge e in allClipboard)
                currenSession.DeleteEdge(e);

            allClipboard = currenSession.GetAll(false, @"ClipboardCopy:");

            foreach (IEdge e in allClipboard)
                currenSession.DeleteEdge(e);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void PutToClipboard(IEnumerable<IEdge> edges, bool isCut)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"Home:\CurrentUser:\CurrentSession:");

            IVertex clipboard = null;

            if(isCut)
                clipboard = ClipboardCut_meta;
            else
                clipboard = ClipboardCopy_meta;

            foreach (IEdge e in edges)
                currenSession.AddEdge(clipboard, e.To);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void PutToClipboard(IVertex vertex, bool isCut)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"Home:\CurrentUser:\CurrentSession:");

            IVertex clipboard = null;

            if (isCut)
                clipboard = ClipboardCut_meta;
            else
                clipboard = ClipboardCopy_meta;

            currenSession.AddEdge(clipboard, vertex);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static IEnumerable<IEdge> GetFromClipboard()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            List<IEdge> ret = new List<IEdge>();

            IVertex currentSession = m0.MinusZero.Instance.root.Get(false, @"Home:\CurrentUser:\CurrentSession:");

            IVertex cut = currentSession.GetAll(false, @"ClipboardCut:");

            if(cut!=null)
                ret.AddRange(cut);

            IVertex copy = currentSession.GetAll(false, @"ClipboardCopy:");

            if(copy!=null)
                ret.AddRange(copy);

            List<IEdge> retEdges = new List<IEdge>();

            foreach (IEdge e in ret)
                retEdges.Add(EdgeHelper.FindEdgeVertexByToVertex(currentSession, e.To.Get(false, "To:")));            

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
            
            return retEdges;
        }

        public static IEnumerable<IEdge> GetFromClipboard(string meta)
        {
            return m0.MinusZero.Instance.root.Get(false, @"Home:\CurrentUser:\CurrentSession:\Clipboard:\$Is:"+meta);
        }
    }
}
