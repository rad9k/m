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
    public class RectangleItem_LabeledItem : UXItem
    {
        // UNDER

        static IVertex RoundEdgeSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\RoundEdgeSize");
        static IVertex HideHeader_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\HideHeader");

        static IVertex ConstantLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ConstantLabel");
        static IVertex LabelQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\LabelQuery");
        static IVertex UseCodeLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\UseCodeLabel");
        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ShowMeta");
        static IVertex HideLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\HideLabel");

        public RectangleItem_LabeledItem(IEdge edge) : base(edge) { }

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
                    return false;

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

        //

        public string ConstantLabel
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ConstantLabel", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ConstantLabel", null);

                if (val == null)
                    val = Vertex.AddVertex(ConstantLabel_meta, value);
                else
                    val.Value = value;
            }
        }

        public string LabelQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "LabelQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "LabelQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(LabelQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool UseCodeLabel
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "UseCodeLabel", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "UseCodeLabel", null);

                if (val == null)
                    val = Vertex.AddVertex(UseCodeLabel_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool ShowMeta
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    val = Vertex.AddVertex(ShowMeta_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool HideLabel
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideLabel", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideLabel", null);

                if (val == null)
                    val = Vertex.AddVertex(HideLabel_meta, value);
                else
                    val.Value = value;
            }
        }


    }
}
