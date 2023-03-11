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

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerItem : UXItem, IUXContainer
    {
        public Canvas Canvas { get; set; }

        static IVertex MultiContainerSubItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerSubItem");

        public MultiContainerItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();            
        }

        void CreateSubItemVertexes()
        {
            if (Items.Count == 0)
                foreach (UXTemplate t in UXTemplate.UXTemplate_)
                {
                    IUXItem i = (IUXItem)AddItem(MultiContainerSubItem_type);
                    i.UXTemplate = t;
                }
        }

        void CreateSubConainerControls()
        {
            int cnt = 0;

            foreach(IItem _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(_i);

                if (i == null)
                    continue;

                InsertSubContainer(i, cnt++);
            }
        }

        void InsertSubContainer(IUXItem i, int cnt)
        {
            UXTemplate iUXTemplate = i.UXTemplate;

            IEdge sizeEdge = GraphUtil.GetQueryOutFirstEdge(iUXTemplate.ItemVertex, "Size", null);

            TextBlock label = null;

            if (iUXTemplate.Name != null) {
                label = new TextBlock();
                label.Text = iUXTemplate.Name;

                SubGrid.Children.Add(label);
            }

            //

            Canvas canvas = new Canvas();
            SubGrid.Children.Add(canvas);

            //

            GridSplitter splitter = new GridSplitter();
            SubGrid.Children.Add(splitter);

            if (Orientation == OrientationEnum.Horizontal)
            {
                RowDefinition rowDefinition = new RowDefinition();

                if (sizeEdge != null) {
                    Size size = new Size(sizeEdge);

                    rowDefinition.Height = new GridLength(size.Height, GridUnitType.Star);
                }

                SubGrid.RowDefinitions.Add(rowDefinition);

                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                splitter.Height = 5;

                if (label != null)
                    Grid.SetRow(label, cnt);

                Grid.SetRow(canvas, cnt);
            }
            else
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();

                if (sizeEdge != null)
                {
                    Size size = new Size(sizeEdge);

                    columnDefinition.Width = new GridLength(size.Width, GridUnitType.Star);
                }

                SubGrid.ColumnDefinitions.Add(columnDefinition);

                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                splitter.Width = 5;

                if (label != null)
                    Grid.SetColumn(label, cnt);

                Grid.SetColumn(canvas, cnt);
            }
        }

        public MultiContainerItem(IEdge edge) : base(edge) {
            InitializeComponent();        
        }

        
        public override void VertexSetedUp()
        {
            if(Canvas == null) { 
                Canvas = new Canvas();
                MainGrid.Children.Add(Canvas);
            }

            Canvas.ClipToBounds = true;

            Grid.SetRow(Canvas, 2);
            
            base.VertexSetedUp();

            //

            CreateSubItemVertexes();

            CreateSubConainerControls();
        }
        
        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            if(ShowMeta)            
            {
                IVertex baseEdgeTo = BaseEdgeTo;

                if (baseEdgeTo != null)
                    this.Title.Text = baseEdgeTo.Value.ToString();
                else
                    this.Title.Text = "Ø";
            }
            else
            {
                IEdge baseEdge = BaseEdge;
                IVertex baseEdgeTo = baseEdge.To;
                IVertex baseEdgeMeta = baseEdge.Meta;

                string meta_text, to_text;

                if (baseEdgeMeta != null)
                    meta_text = baseEdgeMeta.Value.ToString();
                else
                    meta_text = "Ø";

                if (baseEdgeTo != null)
                    to_text = baseEdgeTo.Value.ToString();
                else
                    to_text = "Ø";

                if (meta_text != "$Empty" && meta_text != "")
                    this.Title.Text = meta_text + " : " + to_text;
                else
                    this.Title.Text = to_text;
            }


            double roundEdgeSize = RoundEdgeSize;

            if (roundEdgeSize != 0)
            {                
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);
                
                this.Title.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 0);

                Canvas.Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

                MainGrid.RowDefinitions[0].Height = new GridLength(18 + RoundEdgeSize);
                
            }

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();
            

            this.Frame.Background = backgroundBrush;

            this.Title.Foreground = foregroundBrush;

            this.InternalFrame.BorderBrush = foregroundBrush;

            this.Frame.BorderBrush = foregroundBrush;

            Canvas.Background = backgroundBrush;


            if (BorderSize != 0)
            {
                this.Frame.BorderThickness = new Thickness(BorderSize);

                this.InternalFrame.BorderThickness = new Thickness(BorderSize / 2);

                this.MainGrid.RowDefinitions[1].Height = new GridLength(BorderSize);                
            }
        }         

        public override void Select()
        {
            base.Select();

            
            this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");

            this.Title.Cursor = Cursors.ScrollAll;
        }

        public override void Unselect()
        {
            base.Unselect();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();


            this.Frame.Background = backgroundBrush;

            this.Title.Foreground = foregroundBrush;
            this.Foreground = foregroundBrush;

            this.InternalFrame.BorderBrush = foregroundBrush;

            this.Frame.BorderBrush = foregroundBrush;

            this.Title.Cursor = Cursors.Arrow;
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 

            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");

            this.InternalFrame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");            
        }

        public override void Unhighlight()
        {
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            this.Foreground = foregroundBrush; 

            this.Frame.Background = backgroundBrush;
            this.Frame.BorderBrush = foregroundBrush;

            this.InternalFrame.BorderBrush = foregroundBrush;
            this.Title.Foreground = foregroundBrush;
            
            base.Unhighlight();
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "RoundEdgeSize", null))
                {
                    VisualiserUpdate();
                    return exe.Stack;
                }
            }            

            return base.VertexChange(exe);
        }
        
        // UNDER        

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\ShowMeta");
        static IVertex RoundEdgeSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\RoundEdgeSize");
        static IVertex Orientation_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\Orientation");

        public bool ShowMeta
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    val = Vertex.AddVertex(ShowMeta_meta, value);
                else
                    val.Value = value;
            }
        }

        public double RoundEdgeSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    val = Vertex.AddVertex(RoundEdgeSize_meta, value);
                else
                    val.Value = value;
            }
        }

        public OrientationEnum Orientation
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Orientation", null);

                return OrientationEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, Orientation_meta, OrientationEnumHelper.GetVertex(value));
            }
        }


        // UXContainer

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeQuery");

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

    }
}