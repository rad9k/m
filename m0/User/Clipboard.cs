using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.User
{
    public class SessionClipboard
    {
        public static void ClearClipboard()
        {
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:");

            IEnumerable<IEdge> allClipboard = currenSession.GetAll(false, @"ClipboardCut:");

            foreach (IEdge e in allClipboard)
                currenSession.DeleteEdge(e);

            allClipboard = currenSession.GetAll(false, @"ClipboardCopy:");

            foreach (IEdge e in allClipboard)
                currenSession.DeleteEdge(e);
        }

        public static void PutToClipboard(IEnumerable<IEdge> edges, bool isCut)
        {
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:");

            IVertex clipboard = null;

            if(isCut)
                clipboard = m0.MinusZero.Instance.root.Get(false, @"System\Meta\User\Session\ClipboardCut");
            else
                clipboard = m0.MinusZero.Instance.root.Get(false, @"System\Meta\User\Session\ClipboardCopy");

            foreach (IEdge e in edges)
                currenSession.AddEdge(clipboard, e.To);
        }

        public static IEnumerable<IEdge> GetFromClipboard()
        {
            List<IEdge> ret = new List<IEdge>();

            IVertex currentSession = m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:");

            IVertex cut = currentSession.GetAll(false, @"ClipboardCut:");

            if(cut!=null)
                ret.AddRange(cut);

            IVertex copy = currentSession.GetAll(false, @"ClipboardCopy:");

            if(copy!=null)
                ret.AddRange(copy);

            List<IEdge> retEdges = new List<IEdge>();

            foreach (IEdge e in ret)
                retEdges.Add(EdgeHelper.FindEdgeVertexByToVertex(currentSession, e.To.Get(false, "To:")));

            return retEdges;
        }

        public static IEnumerable<IEdge> GetFromClipboard(string meta)
        {
            return m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:\Clipboard:\$Is:"+meta);
        }
    }
}
