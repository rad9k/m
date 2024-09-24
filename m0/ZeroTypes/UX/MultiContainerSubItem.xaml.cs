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
using m0.User.Process.UX;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerSubItem : UXItem, IUXMultiContainerSubItem
    {
        public Canvas Canvas { 
            get { return canvas; }
            set { }
        }        

        public MultiContainerSubItem() : base(new ZeroTypes.Edge(null), true)
        {
            InitializeComponent();
        }

        public MultiContainerSubItem(IEdge edge) : base(edge, true)
        {
            InitializeComponent();
        }

        public override void VertexSetedUp()
        {            
            Canvas.ClipToBounds = true;                                   
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size s = Size;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            s.Width = ActualWidth;

            s.Height = ActualHeight;

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 

            UpdateDiagramLines();
        }

        protected Brush GetParentBackgroundBrush()
        {
            if (ParentItem != null && ParentItem is IUXMultiContainerItem)
            {
                Color backgroundColor_parent = ((IUXMultiContainerItem)ParentItem).BackgroundColor;

                if (backgroundColor_parent != null)
                    return backgroundColor_parent.GetBrush();
            }
            
                return (Brush)FindResource("0BackgroundBrush");
        }

        protected Brush GetParentForegroundBrush()
        {
            if (ParentItem != null && ParentItem is IUXMultiContainerItem)
            {
                Color color_parent = ((IUXMultiContainerItem)ParentItem).ForegroundColor;

                if (color_parent != null)
                    return color_parent.GetBrush();
            }

            return (Brush)FindResource("0ForegroundBrush");
        }

        protected double GetParentSubFontSize()
        {
            if (ParentItem != null && ParentItem is IUXMultiContainerItem)
            {
                double value_parent = ((IUXMultiContainerItem)ParentItem).SubFontSize;

                if (value_parent != 0)
                    return value_parent;
            }

            return 12;
        }

        protected Brush GetParentSubBackgroundBrush()
        {
            if (ParentItem != null && ParentItem is IUXMultiContainerItem)
            {
                Color color_parent = ((IUXMultiContainerItem)ParentItem).SubBackgroundColor;

                if (color_parent != null)
                    return color_parent.GetBrush();
            }

            return (Brush)FindResource("0BackgroundBrush");
        }

        protected Brush GetParentSubForegroundBrush()
        {
            if (ParentItem != null && ParentItem is IUXMultiContainerItem)
            {
                Color color_parent = ((IUXMultiContainerItem)ParentItem).SubForegroundColor;

                if (color_parent != null)
                    return color_parent.GetBrush();
            }

            return (Brush)FindResource("0ForegroundBrush");
        }



        public void ViewAttributesUpdated()
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
    }
}