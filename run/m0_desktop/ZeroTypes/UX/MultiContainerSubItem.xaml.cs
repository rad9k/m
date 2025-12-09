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
using m0.Util;
using System.Windows.Forms.VisualStyles;
using m0.Graph.ExecutionFlow;
using m0.User.Process.UX;
using m0.UIWpf.Controls;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerSubItem : UXItem, IUXMultiContainerSubItem
    {
        Canvas canvas;
        
        public Canvas Canvas { 
            get { return canvas; }
            set { }
        }

        private void CreateNotExistingContentQueryEdge()
        {
            if (!SubItemsNotVisible)
                return;

            IVertex notExistingContentQueryEdge = NotExistingContentQueryEdge;

            if (notExistingContentQueryEdge != null)
            {
                string contentQuery = ContentQuery;

                if (contentQuery != null) {
                    IVertex baseEdgeTo = BaseEdge.To;

                    if (baseEdgeTo.Get(false, contentQuery) == null)
                        baseEdgeTo.AddVertex(notExistingContentQueryEdge, null);
                }                        
            }
        }

        public MultiContainerSubItem() : base(new ZeroTypes.Edge(null), true)
        {
            InitializeComponent();
        }

        public MultiContainerSubItem(IEdge edge) : base(edge, true)
        {
            InitializeComponent();            
        }

        bool SubItemsNotVisible_prev;
        bool wasUpdated = false;

        private void ClearContent()
        {
            if (Content.Child != null)
            {
                if (Content.Child is Canvas)
                {
                    foreach (object o in ((Canvas)Content.Child).Children)
                    {
                        if (o is IUXItem)
                        {
                            IUXItem o_IUXItem = (IUXItem)o;

                            o_IUXItem.Dispose();
                        }
                    }                    
                }

                Content.Child = null;
            }
        }

        private void ContentUpdate()
        {
            bool subItemnsNotVisible = SubItemsNotVisible;

            CreateNotExistingContentQueryEdge();

            if (!wasUpdated || subItemnsNotVisible != SubItemsNotVisible_prev)
            {
                SubItemsNotVisible_prev = subItemnsNotVisible;                

                ClearContent();

                if (SubItemsNotVisible)
                {
                    CodeToggle.IsChecked = true;

                    CodeControl codeControl = new CodeControl(Vertex);

                    codeControl.Margin = new Thickness(2, 0, 2, 0);

                    Content.Child = codeControl;

                    codeControl.BaseEdgeToUpdated();
                }
                else
                {
                    canvas = new Canvas();

                    Content.Child = canvas;

                    Canvas.ClipToBounds = true;

                    CodeToggle.IsChecked = false;
                }

                if (wasUpdated)
                    this.OwningVisualiser.Paint();

                wasUpdated = true;                
            }
        }

        public override void VertexSetedUp()
        {
            ContentUpdate();            
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

        private void CodeToggle_Click(object sender, RoutedEventArgs e)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

                if (CodeToggle.IsChecked == true)
                    SubItemsNotVisible = true;
                else
                    SubItemsNotVisible = false;
            
            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
            
            ContentUpdate();
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

            return 8;
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



        public override void ViewAttributesUpdated() // this is not called when it should be. there is NO proper event routing in multicointinersubitems
        {            
            Label.Text = GraphUtil.GetStringValue(UXTemplate.Vertex);

            Label.FontSize = GetParentSubFontSize();

            SetColors(GetParentBackgroundBrush(), GetParentSubBackgroundBrush(), GetParentSubForegroundBrush());

            ContentUpdate();
        }

        private void SetColors(Brush backgroundBrush_canvas, Brush foregroundBrush_label, Brush backgroundBrush_label)
        {
            TopPane.Background = foregroundBrush_label;
            Label.Foreground = backgroundBrush_label;
            CodeLabel.Foreground = backgroundBrush_label;

            Content.Background = backgroundBrush_canvas;
        }

        public override void Select() {
            SetColors((Brush)FindResource("0SelectionBrush"), (Brush)FindResource("0SelectionBrush"), (Brush)FindResource("0BackgroundBrush"));
        }

        public override void Unselect() {
            SetColors(GetParentBackgroundBrush(), GetParentSubBackgroundBrush(), GetParentSubForegroundBrush());
        }

        public override void Highlight() {
            SetColors((Brush)FindResource("0HighlightForegroundBrush"), (Brush)FindResource("0HighlightBrush"), (Brush)FindResource("0HighlightForegroundBrush"));
        }

        public override void Unhighlight() {
            SetColors(GetParentBackgroundBrush(), GetParentSubBackgroundBrush(), GetParentSubForegroundBrush());
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            return exe.Stack;
            // return base.VertexChange(exe);
        }

        // MultiContainerSubItem

        static IVertex NotExistingContentQueryEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerSubItem\NotExistingContentQueryEdge");

        public IVertex NotExistingContentQueryEdge
        {
            get
            {
                return GraphUtil.GetQueryOutFirst(Vertex, "NotExistingContentQueryEdge", null);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, NotExistingContentQueryEdge_meta, value);
            }
        }

        // UXContainer

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeQuery");
        static IVertex SubItemsNotVisible_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\SubItemsNotVisible");
        static IVertex NewItemUXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\NewItemUXTemplate");
        static IVertex ContentQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeView\ContentQuery");
        static IVertex FormalTextLanguageProcessing_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\FormalTextLanguageProcessing");

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

        public string ContentQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContentQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContentQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ContentQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex FormalTextLanguageProcessing
        {
            get
            {
                return GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguageProcessing", null);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, FormalTextLanguageProcessing_meta, value);
            }
        }

    }
}