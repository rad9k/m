using m0.Foundation;
using m0.Graph;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0.ZeroTypes.UX
{
    public class LabeledItem : UXItem
    {
        // UNDER

        static IVertex LabelQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\LabelQuery");
        static IVertex UseCodeLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\UseCodeLabel");
        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\ShowMeta");
        static IVertex HideLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\HideLabel");

        public LabeledItem(IEdge edge) : base(edge) { }

        public int RoundEdgeSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    val = Vertex.AddVertex(RoundEdgeSize_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool HideHeader
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideHeader", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideHeader", null);

                if (val == null)
                    val = Vertex.AddVertex(HideHeader_meta, value);
                else
                    val.Value = value;
            }
        }
    
    }
}
