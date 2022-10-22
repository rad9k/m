using m0.Foundation;
using m0.Graph;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class LineDecoratorBase : UXItem
    {
        public UXItem FromDiagramItem;

        public UXItem ToDiagramItem;

        public virtual void SetPosition(double FromX, double FromY, double ToX, double ToY, bool isSelfRelation, double selfRelationX, double selfRelationY)
        {
        }

        public virtual double GetMouseDistance(Point p)
        {
            return 0;
        }

        public virtual void UpdateMetaPosition()
        {

        }

        public virtual void AddToCanvas()
        {

        }

        public override void RemoveFromCanvas()
        {

        }

        public override void Highlight()
        {

        }

        public override void Unhighlight()
        {

        }

        public override void Dispose()
        {

        }

        // UNDER

        static IVertex LineWidth_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\LineDecoratorBase\LineWidth");
        static IVertex ToItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\LineDecoratorBase\ToItem");

        public LineDecoratorBase(IEdge edge) : base(edge) { }

        public double LineWidth
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "LineWidth", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "LineWidth", null);

                if (val == null)
                    val = Vertex.AddVertex(LineWidth_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
