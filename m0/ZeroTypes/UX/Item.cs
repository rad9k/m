using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class Item: TypedVertex
    {
        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");


        public Item(IVertex vertex) : base(vertex) { }

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

        public IList<Item> Item
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<Item> ret = new List<Item>();

                foreach (IEdge e in list)
                    ret.Add(new Item(e.To));

                return ret;
            }
            set
            {
            }
        }

        IVertex AddItem()
        {

        }
        
    }
}
