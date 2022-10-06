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
    public class ImageItem : UXItem
    {
        static IVertex Filename_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\Filename");

        public ImageItem(IEdge edge) : base(edge) { }

        public string Filename
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Filename", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Filename", null);

                if (val == null)
                    val = Vertex.AddVertex(Filename_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
