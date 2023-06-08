using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using m0.Graph;
using m0.Foundation;
using m0.ZeroTypes;
using m0.Util;
using System.Windows.Forms.VisualStyles;
using m0.Graph.ExecutionFlow;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerSubItem : UserControl, IUXMultiContainerSubItem
    {
        public Canvas Canvas { 
            get { return canvas; }
            set { }
        }        

        public MultiContainerSubItem()
        {
            InitializeComponent();
        }

        public MultiContainerSubItem(IEdge _edge) {
            NestingLevel = 0;

            Edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);

            InitializeComponent();
        }

        public void VertexSetedUp()
        {            
            Canvas.ClipToBounds = true;                                   
        }

        protected Brush GetParentBackgroundBrush()
        {
            if (ParentItem == null || !(ParentItem is UXItem))
                return null;

            Color backgroundColor_parent = ((UXItem)ParentItem).BackgroundColor;

            if (backgroundColor_parent != null)
                return backgroundColor_parent.GetBrush();
            else
                return (Brush)FindResource("0BackgroundBrush");
        }

        protected Brush GetParentForegroundBrush()
        {
            if (ParentItem == null || !(ParentItem is UXItem))
                return null;

            Color foregroundColor_parent = ((UXItem)ParentItem).ForegroundColor;

            if (foregroundColor_parent != null)
                return foregroundColor_parent.GetBrush();
            else
                return (Brush)FindResource("0ForegroundBrush");
        }

        public void ItemVisualUpdate()
        {            
            Label.Text = UXTemplate.Name;

            SetColors(GetParentBackgroundBrush(), GetParentForegroundBrush());      
        }

        private void SetColors(Brush backgroundBrush, Brush foregroundBrush)
        {
            Label.Background = foregroundBrush;
            Label.Foreground = backgroundBrush;

            canvas.Background = backgroundBrush;
        }

        public void Select() {
            SetColors((Brush)FindResource("0SelectionBrush"), (Brush)FindResource("0BackgroundBrush"));
        }

        public void Unselect() {
            SetColors(GetParentBackgroundBrush(), GetParentForegroundBrush());            
        }

        public void Highlight() {
            SetColors((Brush)FindResource("0HighlightForegroundBrush"), (Brush)FindResource("0HighlightBrush"));
        }

        public void Unhighlight() {
            SetColors(GetParentBackgroundBrush(), GetParentForegroundBrush());
        }

        protected INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            return exe.Stack;
            // return base.VertexChange(exe);
        }
        
        // UXContainer

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeQuery");
        static IVertex NewItemUXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\NewItemUXTemplate");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");        

        public bool IsExpanded
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

        public string ContainerEdgeQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    return "";

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ContainerEdgeQuery_meta, value);
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

        // UNDER

        // UXItem

        static IVertex Scale_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale");
        static IVertex DesignMode_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\DesignMode");
        static IVertex Size_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Size");
        static IVertex Position_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Position");
        static IVertex Layout_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Layout");
        static IVertex BackgroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BackgroundColor");
        static IVertex ForegroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\ForegroundColor");
        static IVertex BorderColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BorderColor");
        static IVertex BorderSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BorderSize");
        static IVertex Gap_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Gap");
        static IVertex UXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\UXTemplate");
        static IVertex Decorator_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Decorator");

        static IVertex Color_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color");
        //static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");
        static IVertex Position_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Position");

        public double Scale
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Scale", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Scale", null);

                if (val == null)
                    val = Vertex.AddVertex(Scale_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool DesignMode
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DesignMode", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DesignMode", null);

                if (val == null)
                    val = Vertex.AddVertex(DesignMode_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.Size Size
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "Size", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size SizeCreate()
        {
            IEdge sizeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "Size", null);

            if (sizeEdge != null)
                Vertex.DeleteEdge(sizeEdge);

            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, Size_meta));
        }

        public UX.Position Position
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "Position", null);

                if (val == null)
                    return null;

                return (UX.Position)TypedEdge.Get(val, typeof(UX.Position));
            }
        }

        public UX.Position PositionCreate()
        {
            IEdge positionEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "Position", null);

            if (positionEdge != null)
                Vertex.DeleteEdge(positionEdge);

            return new UX.Position(VertexOperations.AddInstanceAndReturnEdge(Vertex, Position_type, Position_meta));
        }

        public LayoutTypeEnum Layout
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Layout", null);

                return LayoutTypeEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, Layout_meta, LayoutTypeEnumHelper.GetVertex(value));
            }
        }

        public UX.Color BackgroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BackgroundColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color BackgroundColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, BackgroundColor_meta));
        }

        public UX.Color ForegroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ForegroundColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color ForegroundColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, ForegroundColor_meta));
        }

        public UX.Color BorderColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BorderColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color BorderColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, BorderColor_meta));
        }

        public double BorderSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BorderSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BorderSize", null);

                if (val == null)
                    val = Vertex.AddVertex(BorderSize_meta, value);
                else
                    val.Value = value;
            }
        }

        public double Gap
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Gap", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Gap", null);

                if (val == null)
                    val = Vertex.AddVertex(Gap_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.UXTemplate UXTemplate
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "UXTemplate", null);

                if (val == null)
                    return null;

                ITypedEdge _i = TypedEdge.Get(val);

                if (_i != null && _i is UXTemplate)
                    return (UXTemplate)_i;

                return null;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, UXTemplate_meta, value.Vertex);
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

        public IUXItem AddDecorator(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Decorator_meta);

            ITypedEdge _i = TypedEdge.Get(newEdge);

            if (_i != null && _i is IUXItem)
                return (IUXItem)_i;

            return null;
        }

        // Item

        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex UXItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem");
        static IVertex UXAggregator_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator");
        static IVertex Edge_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\Edge");

        public IVertex BaseEdgeTo
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

                if (val != null)
                    return GraphUtil.GetQueryOutFirst(val.To, "To", null);

                return null;
            }
        }

        public Edge BaseEdge
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

                if (val == null)
                    return null;

                return (Edge)TypedEdge.Get(val, typeof(Edge));
            }
            set
            {
                Edge baseEdge = BaseEdge;

                if (value.From != null)
                    baseEdge.From = value.From;

                if (value.Meta != null)
                    baseEdge.Meta = value.Meta;

                if (value.To != null)
                    baseEdge.To = value.To;
            }
        }

        public void BaseEdgeSet(IEdge value)
        {
            Edge baseEdge = BaseEdge;

            if (value.From != null)
                baseEdge.From = value.From;

            if (value.Meta != null)
                baseEdge.Meta = value.Meta;

            if (value.To != null)
                baseEdge.To = value.To;
        }

        public Edge BaseEdgeCreate()
        {
            IEdge baseEdgeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

            if (baseEdgeEdge != null)
                Vertex.DeleteEdge(baseEdgeEdge);

            baseEdgeEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Edge_type, BaseEdge_meta);

            baseEdgeEdge.To.AddVertex(ZeroTypes.Edge.From_meta, ""); // from has 0..1 multiplicity

            return new Edge(baseEdgeEdge);
        }

        public IList<IItem> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<IItem> ret = new List<IItem>();

                foreach (IEdge e in list)
                {
                    ITypedEdge _i = TypedEdge.Get(e);

                    if (_i != null && _i is IItem)
                    {
                        IItem item = (IItem)_i;

                        item.ParentItem = this;
                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        public IItem AddItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            /*IItem item = null;

            if (GraphUtil.GetValueAndCompareStrings(typeVertex, "UXItem"))
                item = (IItem)TypedEdge.Get(newEdge, typeof(ZeroTypes.UX.UXItem));
            else
            if (GraphUtil.GetValueAndCompareStrings(typeVertex, "UXAggregator"))
                item = (IItem)TypedEdge.Get(newEdge, typeof(ZeroTypes.UX.UXContainer));
            else
                item = TypedEdge.Get_ItemVersion(newEdge);
            */

            ITypedEdge _i = TypedEdge.Get(newEdge);

            if (_i != null && _i is IItem)
            {
                IItem item = (IItem)_i;

                item.ParentItem = this;

                if (item is IUXItem)
                    ((IUXItem)item).NestingLevel = NestingLevel + 1;

                return item;
            }

            return null;
        }

        public void MoveExistingItemAsSubItem(IItem item)
        {
            item.Edge.From.DeleteEdge(item.Edge);

            IEdge e = Vertex.AddEdge(Item_meta, item.Vertex);
            item.Edge = e;

            item.ParentItem = this;

            if (item is IUXItem)
                ((IUXItem)item).NestingLevel = NestingLevel + 1;
        }

        public void RemoveItem(IItem item)
        {
            Vertex.DeleteEdge(item.Edge);
        }

        public Dictionary<IUXItem, List<ILineDecoratorBase>> GetDiagramLinesToDiagramItemDictionary()
        {
            throw new NotImplementedException();
        }

        public Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary()
        {
            throw new NotImplementedException();
        }

        public void RemoveFromCanvas()
        {
            throw new NotImplementedException();
        }

        public void AddDiagramLineObject(IUXItem toItem, ILineDecoratorBase lineDecorator)
        {
            throw new NotImplementedException();
        }

        public void RemoveDiagramLine(ILineDecoratorBase line)
        {
            throw new NotImplementedException();
        }

        public void MoveItem(double x, double y, bool onlyAnchors)
        {
            throw new NotImplementedException();
        }

        public void MoveAndResizeItem(double left, double top, double width, double height)
        {
            throw new NotImplementedException();
        }

        public void AddToSelectedEdges()
        {
            throw new NotImplementedException();
        }

        public Point GetLineAnchorLocation(IUXItem toItem, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart)
        {
            throw new NotImplementedException();
        }

        public void UpdateDiagramLines() {}

        public void AddAsToMetaLine(ILineDecoratorBase line)
        {
            throw new NotImplementedException();
        }

        public bool IsDisposed = false;

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                TypedEdge.RemoveFromDictionary(this);
            }
        }

        // TypedEdge

        public IEdge Edge { get; set; }

        IVertex vertex;
        public IVertex Vertex
        {
            get { return vertex; }
            set
            {
                throw new Exception("please correct. not handling Vertex set in UXItem");
            }
        }

        public IEdge ContainerEdge { get; set; }
        public int NestingLevel { get; set; }
        public IUXVisualiser OwningVisualiser { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }

        public List<ILineDecoratorBase> DiagramToLines => throw new NotImplementedException();

        public List<ILineDecoratorBase> DiagramToAsMetaLines => throw new NotImplementedException();

        public IItem ParentItem { get; set; }
    }
}