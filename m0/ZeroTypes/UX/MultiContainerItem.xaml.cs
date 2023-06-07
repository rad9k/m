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
using m0.UIWpf;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerItem : UXContainer
    {
        public Canvas Canvas { get; set; }

        static IVertex MultiContainerSubItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerSubItem");

        public MultiContainerItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();            
        }

        public MultiContainerItem(IEdge edge) : base(edge)
        {
            InitializeComponent();
        }

        public override void VertexSetedUp()
        {
            Canvas = new Canvas();// _Canvas;

            base.VertexSetedUp();


            //

            CreateSubItemVertexes();

            CreateSubConainerControls();
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

            foreach (IItem _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                if (cnt == 0)
                {
                    InsertSubContainer(i, cnt, false);
                    cnt++;
                }
                else
                {
                    InsertSubContainer(i, cnt, true);
                    cnt += 2;
                }                
            }
        }

        void InsertSubContainer(IUXItem subItem, int cnt, bool addSplitter)
        {
            if (!(subItem is UIElement))
                return;

            subItem.OwningVisualiser = this.OwningVisualiser;

            UIElement subItem_UIElement = (UIElement)subItem;

            UXTemplate iUXTemplate = subItem.UXTemplate;

            IEdge sizeEdge = GraphUtil.GetQueryOutFirstEdge(iUXTemplate.ItemVertex, "Size", null);

            Size size = null;

            if (sizeEdge != null)
                size = new Size(sizeEdge);
            
            //

            GridSplitter splitter = null;            

            if (addSplitter) {
                splitter = new GridSplitter();

                splitter.Background = (Brush)FindResource("0VeryLightHighlightBrush");

                SubGrid.Children.Add(splitter);
            }

            if (Orientation == OrientationEnum.Horizontal)
            {                                
                if (addSplitter)
                {
                    RowDefinition splitterRow = new RowDefinition();

                    splitterRow.Height = new GridLength(3, GridUnitType.Pixel);

                    SubGrid.RowDefinitions.Add(splitterRow);

                    splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    splitter.Height = 3;
                    splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;

                    Grid.SetRow(splitter, cnt);

                    cnt++;

                    WpfUtil.DecorateWithCustomCursor(splitter, Cursors.SizeNS);
                }

                //

                RowDefinition rowDefinition = new RowDefinition();

                if (size != null)
                    rowDefinition.Height = new GridLength(size.Height, GridUnitType.Star);

                SubGrid.RowDefinitions.Add(rowDefinition);

                //

                SubGrid.Children.Add(subItem_UIElement);

                Grid.SetRow(subItem_UIElement, cnt);
            }
            else
            {
                if (addSplitter)
                {
                    ColumnDefinition splitterColumn = new ColumnDefinition();

                    splitterColumn.Width = new GridLength(3, GridUnitType.Pixel);

                    SubGrid.ColumnDefinitions.Add(splitterColumn);

                    splitter.VerticalAlignment = VerticalAlignment.Stretch;
                    splitter.Width = 3;
                    splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;

                    Grid.SetColumn(splitter, cnt);

                    cnt++;

                    WpfUtil.DecorateWithCustomCursor(splitter, Cursors.SizeWE);
                }

                //

                ColumnDefinition columnDefinition = new ColumnDefinition();

                if (size != null)
                    columnDefinition.Width = new GridLength(size.Width, GridUnitType.Star);

                SubGrid.ColumnDefinitions.Add(columnDefinition);

                //
                
                SubGrid.Children.Add(subItem_UIElement);

                Grid.SetColumn(subItem_UIElement, cnt);
            }
        }
        
        public override void ItemVisualUpdate()
        {
            base.ItemVisualUpdate();

            if(ShowMeta)
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
            else
            {
                IVertex baseEdgeTo = BaseEdgeTo;

                if (baseEdgeTo != null)
                    this.Title.Text = baseEdgeTo.Value.ToString();
                else
                    this.Title.Text = "Ø";
            }

            double roundEdgeSize = RoundEdgeSize;

            if (roundEdgeSize != 0)
            {                
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);
                
                this.Title.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 0);

                Canvas.Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

                MainGrid.RowDefinitions[0].Height = new GridLength(18 + RoundEdgeSize);              
            }                        

            double borderSize = BorderSize;

            if (borderSize != 0)
            {
                this.Frame.BorderThickness = new Thickness(borderSize);
                this.InternalFrame.BorderThickness = new Thickness(borderSize / 2);

                this.MainGrid.RowDefinitions[1].Height = new GridLength(borderSize);
            }
            else
            {
                this.Frame.BorderThickness = new Thickness(1);
                this.InternalFrame.BorderThickness = new Thickness(1 / 2);
                this.MainGrid.RowDefinitions[1].Height = new GridLength(1);
            }
           
            Brush borderBrush = GetBorderBrush();

            this.Frame.BorderBrush = borderBrush;
            this.InternalFrame.BorderBrush = borderBrush;          

            VisualiserUpdate_Items();
        }

        public void VisualiserUpdate_Items()
        {
            foreach (IItem _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.ItemVisualUpdate();
            }
        }

        public override void Select()
        {            
            base.Select();

            this.Frame.BorderBrush = (Brush)FindResource("0SelectionBrush");
            this.InternalFrame.BorderBrush = (Brush)FindResource("0SelectionBrush");

            this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");//new SolidColorBrush(Colors.Red);

            this.Title.Cursor = Cursors.ScrollAll;

            //

            foreach (IItem _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Select();
            }
        }

        public override void Unselect()
        {   
            SetBaselineColors();

            base.Unselect();

            //

            foreach (IItem _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Unselect();
            }
        }

        public override void Highlight()
        {            
            base.Highlight();

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 

            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");

            this.InternalFrame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");

            //

            foreach (IItem _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Highlight();
            }
        }

        void SetBaselineColors()
        {
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();

            this.Frame.BorderBrush = borderBrush;
            this.InternalFrame.BorderBrush = borderBrush;

            this.Foreground = foregroundBrush;

            this.Frame.Background = backgroundBrush;

            this.Title.Foreground = foregroundBrush;
        }

        public override void Unhighlight()
        {
            base.Unhighlight();
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)        
        {
            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "RoundEdgeSize", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "ShowMeta", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "BorderSize", null))
                {
                    ItemVisualUpdate();
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

    }
}