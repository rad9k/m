using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public class UXTemplate: TypedEdge
    {        
        static IVertex UXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\UXTemplate");
        static IVertex DirectVertexTestQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\DirectVertexTestQuery");
        static IVertex MetaVertexTestQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\MetaVertexTestQuery");
        static IVertex ItemClass_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ItemClass");
        static IVertex ItemVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ItemVertex");
        static IVertex InstanceCreation_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\InstanceCreation");
        static IVertex UXDecoratorTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\UXDecoratorTemplate");
        static IVertex DoNotShowInherited_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\DoNotShowInherited");
        static IVertex ForceShowEditForm_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ForceShowEditForm");
        static IVertex ContainerEdgeMetaVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ContainerEdgeMetaVertex");
        static IVertex BaseEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\BaseEdgeQuery");
        static IVertex EmptyValueInstance_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\EmptyValueInstance");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");
        static IVertex UXTemplate_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate");


        public UXTemplate(IEdge edge) : base(edge) { }

        public IList<UXTemplate> UXTemplate_
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "UXTemplate", null);

                IList<UXTemplate> ret = new List<UXTemplate>();

                foreach (IEdge e in list)
                {
                    ret.Add((UXTemplate)TypedEdge.Get(e));
                }

                return ret;
            }
        }

        public UXTemplate AddUXTemplate()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, UXTemplate_type, UXTemplate_meta);

            return (UXTemplate)TypedEdge.Get(newEdge, typeof(ZeroTypes.UX.UXTemplate));
        }

        public void RemoveUXTemplate(UXTemplate template)
        {
            Vertex.DeleteEdge(template.Edge);
        }

        public string DirectVertexTestQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DirectVertexTestQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DirectVertexTestQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(DirectVertexTestQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public string MetaVertexTestQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "MetaVertexTestQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "MetaVertexTestQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(MetaVertexTestQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex ItemClass
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ItemClass", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {                
                GraphUtil.CreateOrReplaceEdge(Vertex, ItemClass_meta, value);
            }
        }

        public IVertex ItemVertex
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ItemVertex", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, ItemVertex_meta, value);
            }
        }

        public InstanceCreationEnum InstanceCreation
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "InstanceCreation", null);

                return InstanceCreationEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, InstanceCreation_meta, InstanceCreationEnumHelper.GetVertex(value));
            }
        }

        public IList<UXDecoratorTemplate> UXDecoratorTemplates 
        // we need it as UXDecoratorTemplate inherits from UXTemplate and UXTemplate has $PlatformClass defined
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "UXDecoratorTemplate", null);

                IList<UXDecoratorTemplate> ret = new List<UXDecoratorTemplate>();

                foreach (IEdge e in list)
                    ret.Add((UXDecoratorTemplate)TypedEdge.Get(e, typeof(UXDecoratorTemplate)));

                return ret;
            }
        }

        public IList<IUXItem> Decorators
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Decorator", null);

                IList<IUXItem> ret = new List<IUXItem>();

                foreach (IEdge e in list)
                {
                    ITypedEdge _i = TypedEdge.Get(e);

                    if (_i != null && _i is IUXItem)
                        ret.Add((IUXItem)_i);
                }

                return ret;
            }
        }

        public bool DoNotShowInherited
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DoNotShowInherited", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DoNotShowInherited", null);

                if (val == null)
                    val = Vertex.AddVertex(DoNotShowInherited_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool ForceShowEditForm
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ForceShowEditForm", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ForceShowEditForm", null);

                if (val == null)
                    val = Vertex.AddVertex(ForceShowEditForm_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex ContainerEdgeMetaVertex
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeMetaVertex", null);
                
                return val;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, ContainerEdgeMetaVertex_meta, value);
            }
        }

        public string BaseEdgeQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BaseEdgeQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BaseEdgeQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(BaseEdgeQuery_meta, value);
                else
                    val.Value = value;
            }
        }
        
        public bool EmptyValueInstance
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "EmptyValueInstance", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "EmptyValueInstance", null);

                if (val == null)
                    val = Vertex.AddVertex(EmptyValueInstance_meta, value);
                else
                    val.Value = value;
            }
        }
    }

}
