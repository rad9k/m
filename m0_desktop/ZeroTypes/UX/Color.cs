using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace m0.ZeroTypes.UX
{
    public class Color: TypedEdge
    {
        static IVertex Red_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color\Red");
        static IVertex Green_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color\Green");
        static IVertex Blue_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color\Blue");
        static IVertex Opacity_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color\Opacity");

        public Color(IEdge edge) : base(edge) { }

        public int Red
        {
            get {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Red", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Red", null);

                if (val == null)
                    val = Vertex.AddVertex(Red_meta, value);
                else
                    val.Value = value;
            }
        }

        public int Green
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Green", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Green", null);

                if (val == null)
                    val = Vertex.AddVertex(Green_meta, value);
                else
                    val.Value = value;
            }
        }

        public int Blue
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Blue", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Blue", null);

                if (val == null)
                    val = Vertex.AddVertex(Blue_meta, value);
                else
                    val.Value = value;
            }
        }

        public int Opacity
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Opacity", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Opacity", null);

                if (val == null)
                    val = Vertex.AddVertex(Opacity_meta, value);
                else
                    val.Value = value;
            }
        }

        public System.Windows.Media.Color GetColor()
        {
            return ColorHelper_desktop.GetColorFromColorVertex(Vertex);
        }

        public Brush GetBrush()
        {
            return new SolidColorBrush(GetColor());
        }
    }
}