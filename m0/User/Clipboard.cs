using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.User
{
    public class Clipboard
    {
        public static void ClearClipboard()
        {
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:");

            IEnumerable<IEdge> allClipboard = currenSession.GetAll(false, @"Clipboard:");

            foreach (IEdge e in allClipboard)
                currenSession.DeleteEdge(e);
        }

        public static void PutToClipboard(IEnumerable<IEdge> edges)
        {
            IVertex currenSession = m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:");

            IVertex clipboard = m0.MinusZero.Instance.root.Get(false, @"Meta\User\Session\Clipboard");

            foreach (IEdge e in edges)
                currenSession.AddEdge(clipboard, e.To);
        }

        public static IEnumerable<IEdge> GetFromClipboard()
        {
            return m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:\Clipboard:");
        }

        public static IEnumerable<IEdge> GetFromClipboard(string meta)
        {
            return m0.MinusZero.Instance.root.Get(false, @"User\CurrentUser:\CurrentSession:\Clipboard:\$Is:"+meta);
        }
    }
}
