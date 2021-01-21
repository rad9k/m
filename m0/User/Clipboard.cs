using m0.Foundation;
using m0.Graph;
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

            IEnumerable<IEdge> allClipboard = currenSession.GetAll(false, @"Clipboard:");

            foreach (IEdge e in allClipboard)
                currenSession.DeleteEdge(e);
        }

        public static void PutToClipboard(IEnumerable<IEdge> edges, bool isCut)
        {
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:");

            IVertex clipboard = null;

            if(isCut)
                clipboard = m0.MinusZero.Instance.root.Get(false, @"Systemm\Meta\User\Session\ClipboardCut");
            else
                clipboard = m0.MinusZero.Instance.root.Get(false, @"Systemm\Meta\User\Session\ClipboardCopy");

            foreach (IEdge e in edges)
                currenSession.AddEdge(clipboard, e.To);
        }

        public static IEnumerable<IEdge> GetFromClipboard()
        {
            List<IEdge> ret = new List<IEdge>();

            ret.AddRange(m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:\ClipboardCut:"));
            ret.AddRange(m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:\ClipboardCopy:"));

            return ret;
        }

        public static IEnumerable<IEdge> GetFromClipboard(string meta)
        {
            return m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:\Clipboard:\$Is:"+meta);
        }
    }
}
