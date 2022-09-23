using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class Item: TypedEdge
    {
        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex UXItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem");


        public Item(IEdge edge) : base(edge) { }

        public IVertex BaseEdge
        {
            get {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BaseEdge", null);

                return val;
            }
            set {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BaseEdge", null);

                if (val == null)
                    val = Vertex.AddVertex(BaseEdge_meta, value);
                else
                    val.Value = value;
            }
        }

        public IList<Item> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<Item> ret = new List<Item>();

                foreach (IEdge e in list)
                {
                    Item i = (Item)TypedEdge.Get(e.To);

                    if (i == null)
                        i = new Item(e);
                    ret.Add(i);
                }

                return ret;
            }
            set
            {
            }
        }

        public UXItem AddItem_UXItem()
        {
            return AddItem_UXItem(UXItem_type);
        }

        public UXItem AddItem_UXItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            return new UXItem(newEdge);
        }

        public void RemoveItem(Item item)
        {
            Vertex.DeleteEdge(item.Edge);
        }

    }
}
