using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public class Item: TypedEdge, IItem
    {
        public IItem ParentItem { get; set; }

        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex VolatileItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\VolatileItem");
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

        public IList<ITypedEdge> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<ITypedEdge> ret = new List<ITypedEdge>();

                foreach (IEdge e in list)
                {
                    ITypedEdge item = TypedEdge.Get(e);

                    if(item != null)
                    {
                        if (item is IItem)
                            ((IItem)item).ParentItem = this;
                        
                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        public ITypedEdge AddItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            ITypedEdge item = TypedEdge.Get(newEdge);
            
            if (item != null)
            {
                if (item is IItem)
                    ((IItem)item).ParentItem = this;

                return item;
            }

            return null;
        }

        public IList<ITypedEdge> VolatileItems
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "VolatileItem", null);

                IList<ITypedEdge> ret = new List<ITypedEdge>();

                foreach (IEdge e in list)
                {
                    ITypedEdge item = TypedEdge.Get(e);

                    if (item != null)
                    {
                        if (item is IItem)
                            ((IItem)item).ParentItem = this;

                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        public ITypedEdge AddVolatileItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, VolatileItem_meta);

            ITypedEdge item = TypedEdge.Get(newEdge);

            if (item != null)
            {
                if (item is IItem)
                    ((IItem)item).ParentItem = this;

                return item;
            }

            return null;
        }



        public void MoveExistingItemAsThisItemsSubItem(IItem item)
        {
            item.Edge.From.DeleteEdge(item.Edge);

            IEdge e = Vertex.AddEdge(Item_meta, item.Vertex);
            item.Edge = e;

            item.ParentItem = this;
        }

        public void RemoveItem(IItem item)
        {
            Vertex.DeleteEdge(item.Edge);
        }

        //

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                foreach (ITypedEdge e in Items)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();

                foreach (ITypedEdge e in VolatileItems)
                {
                    Vertex.DeleteEdge(e.Edge);

                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();
                }

                TypedEdge.RemoveFromDictionary(this);
            }
        }
    }
}
