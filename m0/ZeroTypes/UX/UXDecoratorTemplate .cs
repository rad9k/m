using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class UXDecoratorTemplate: UXTemplate
    {
        static IVertex EdgeTestQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate\EdgeTestQuery");
        static IVertex ToDiagramItemTestQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate\ToDiagramItemTestQuery");
        static IVertex DecoratorClass_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate\DecoratorClass");
        static IVertex DecoratorVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate\DecoratorVertex");
        static IVertex CreateEdgeOnly_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate\CreateEdgeOnly");
        static IVertex ForceShowEditForm_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate\ForceShowEditForm");        

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");

        public UXDecoratorTemplate(IEdge edge) : base(edge) { }

        public string EdgeTestQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "EdgeTestQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "EdgeTestQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(EdgeTestQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public string ToDiagramItemTestQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ToDiagramItemTestQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ToDiagramItemTestQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ToDiagramItemTestQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex DecoratorClass
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DecoratorClass", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DecoratorClass", null);

                if (val == null)
                    val = Vertex.AddVertex(DecoratorClass_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex DecoratorVertex
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DecoratorVertex", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DecoratorVertex", null);

                if (val == null)
                    val = Vertex.AddVertex(DecoratorVertex_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool CreateEdgeOnly
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "CreateEdgeOnly", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "CreateEdgeOnly", null);

                if (val == null)
                    val = Vertex.AddVertex(CreateEdgeOnly_meta, value);
                else
                    val.Value = value;
            }
        }

    }

}
