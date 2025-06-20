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

        IList<ITypedEdge> Items { get; }

        ITypedEdge AddItem(IVertex typeVertex);

        IList<ITypedEdge> VolatileItems { get; }

        ITypedEdge AddVolatileItem(IVertex typeVertex);

        void MoveExistingItemAsThisItemsSubItem(IItem item);

        void RemoveItem(IItem item);
    }
}
