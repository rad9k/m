using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public interface IItem: ITypedEdge
    {
        IItem ParentItem { get; set; }

        IVertex BaseEdgeTo { get; }

        Edge BaseEdge { get; }

        void BaseEdgeSet(IEdge value);

        Edge BaseEdgeCreate();

        IList<IItem> Items { get; }

        IItem AddItem(IVertex typeVertex);

        void MoveExistingItemAsSubItem(IItem item);

        void RemoveItem(IItem item);
    }
}
