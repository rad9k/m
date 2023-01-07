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
    public class LineDecorator: LineDecoratorBase
    {
        static IVertex StartAnchor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\StartAnchor");
        static IVertex EndAnchor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\EndAnchor");
        static IVertex IsDasched_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\IsDasched");

        public LineDecorator(IEdge edge) : base(edge) { }

        public LayoutTypeEnum StartAnchor
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "StartAnchor", null);

                return LayoutTypeEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, StartAnchor_meta, LayoutTypeEnumHelper.GetVertex(value));
            }
        }

        public LayoutTypeEnum EndAnchor
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "EndAnchor", null);

                return LayoutTypeEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, EndAnchor_meta, LayoutTypeEnumHelper.GetVertex(value));
            }
        }

        public bool IsDasched
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsDasched", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsDasched", null);

                if (val == null)
                    val = Vertex.AddVertex(IsDasched_meta, value);
                else
                    val.Value = value;
            }
        }

    }
}
