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
    public class LineDecoratorBase : UXItem, ILineDecoratorBase
    {
        public double FromX { get; set; }
        public double FromY { get; set; }
        public double ToX { get; set; }
        public double ToY { get; set; }

        public bool isSelfRelation { get; set; }

        public IUXItem FromDiagramItem { get; set; }        

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

        

        // UNDER

        static IVertex LineWidth_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecoratorBase\LineWidth");
        static IVertex ToItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecoratorBase\ToItem");

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

        public UX.IUXItem ToItem
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ToItem", null);

                if (val == null)
                    return null;

                ITypedEdge _i = TypedEdge.Get(val);

                if (_i != null && _i is IUXItem)
                    return (IUXItem)_i;

                return null;                
            }
            set
            {                
                GraphUtil.CreateOrReplaceEdge(Vertex, ToItem_meta, value.Vertex);
            }
        }
    }
}
