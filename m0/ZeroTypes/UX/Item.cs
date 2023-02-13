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
    public class Item: TypedEdge, IItem
    {
        public IItem ParentItem { get; set; }

        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex UXItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem");
        static IVertex UXAggregator_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator");
        static IVertex Edge_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\Edge");

        public Item(IEdge edge) : base(edge) { }

        public IVertex BaseEdgeTo
        {
            get {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

                if (val == null)
                    return GraphUtil.GetQueryOutFirst(val.To, "To", null);

                return null;
            }
        }

        public Edge BaseEdge
        {
            get {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

                if (val == null)
                    return null;

                return (Edge)TypedEdge.Get(val, typeof(Edge));
            }
            set
            {
                Edge baseEdge = BaseEdge;

                if (value.From != null)
                    baseEdge.From = value.From;

                if (value.Meta != null)
                    baseEdge.Meta = value.Meta;

                if (value.To != null)
                    baseEdge.To = value.To;
            }
        }

        public void BaseEdgeSet(IEdge value)
        {
            Edge baseEdge = BaseEdge;

            if (value.From != null)
                baseEdge.From = value.From;

            if (value.Meta != null)
                baseEdge.Meta = value.Meta;

            if (value.To != null)
                baseEdge.To = value.To;
        }

        public Edge BaseEdgeCreate()
        {
            IEdge baseEdgeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

            if (baseEdgeEdge != null)
                Vertex.DeleteEdge(baseEdgeEdge);

            baseEdgeEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Edge_type, BaseEdge_meta);

            baseEdgeEdge.To.AddVertex(ZeroTypes.Edge.From_meta, ""); // from has 0..1 multiplicity

            return new Edge(baseEdgeEdge);
        }

        public IList<IItem> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<IItem> ret = new List<IItem>();

                foreach (IEdge e in list)
                {
                    IItem i = (IItem)TypedEdge.Get(e);
                    i.ParentItem = this;
                    ret.Add(i);
                }

                return ret;
            }
        }

        public IItem AddItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            return (IItem)TypedEdge.Get(newEdge);
        }

        public void AddExistingItem(IItem item)
        {
            Vertex.AddEdge(Item_meta, item.Vertex);
            item.ParentItem = this;
        }

        public void RemoveItem(IItem item)
        {
            Vertex.DeleteEdge(item.Edge);
        }

    }
}
