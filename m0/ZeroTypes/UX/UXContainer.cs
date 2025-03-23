using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class UXContainer : UXItem, IUXContainer
    {
        public Canvas Canvas { get; set; }
        
        public static IVertex CreateDefaultContainer(IVertex baseVertex)
        {
            IEdge Visualiser_Edge = VertexOperations.AddInstanceAndReturnEdge(baseVertex.Get(false, "To:"), MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer"));

            IVertex Visualiser_Vertex = Visualiser_Edge.To;

            UXContainer c = new UXContainer(Visualiser_Edge);

            EdgeHelper.AddEdgeVertexEdgesByEdgeVertex(Visualiser_Vertex.Get(false, "BaseEdge:"), baseVertex);

            c.SizeCreate();
            c.Size.Width = 5000;
            c.Size.Height = 5000;

            UXTemplate diagram_default_template = new UXTemplate(MinusZero.Instance.Root.GetAll(false, @"System\Data\UX\Templates\ZeroUML").FirstOrDefault());
            c.NewItemUXTemplate = diagram_default_template;

            return Visualiser_Vertex;
        }

        //

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");        
        static IVertex SubItemsNotVisible_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\SubItemsNotVisible");
        static IVertex NewItemUXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\NewItemUXTemplate");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");

        public UXContainer(IEdge edge) : base(edge) { }

        bool IUXContainer.IsExpanded
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    val = Vertex.AddVertex(IsExpanded_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.Size ExpandedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size ExpandedSizeCreate()
        {
            IEdge expectedSizeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

            if (expectedSizeEdge != null)
                Vertex.DeleteEdge(expectedSizeEdge);

            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, ExpandedSize_meta));
        }

        public UX.Size CollapsedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "CollapsedSize", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size CollapsedSizeCreate()
        {
            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, CollapsedSize_meta));
        }

        public bool SubItemsNotVisible
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "SubItemsNotVisible", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "SubItemsNotVisible", null);

                if (val == null)
                    val = Vertex.AddVertex(SubItemsNotVisible_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.UXTemplate NewItemUXTemplate
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "NewItemUXTemplate", null);

                if (val == null)
                    return null;

                ITypedEdge _i = TypedEdge.Get(val);

                if (_i != null && _i is UXTemplate)
                    return (UXTemplate)_i;

                return null;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, NewItemUXTemplate_meta, value.Vertex);
            }
        }
    }

}
