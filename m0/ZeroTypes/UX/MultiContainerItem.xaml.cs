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
using System.Windows.Media.Media3D;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerItem : UXContainer_RectangleItem_LabeledItem, IUXMultiContainerItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "ShowMeta",  "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

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
            Canvas = null;

            base.VertexSetedUp();

            //

            CreateSubItemVertexes();

            CreateSubConainerControls();
        }

        void CreateSubItemVertexes()
        {
            IVertex baseEdgeTo = BaseEdge.To;

            if (Items.Count == 0)
                foreach (UXTemplate template in UXTemplate.UXTemplate_)
                {
                    IUXItem item = (IUXItem)AddItem(MultiContainerSubItem_type);

                    item.NestingLevel = this.NestingLevel + 1;

                    item.Vertex.Value = template.Name;

                    item.UXTemplate = template;                    

                    IEdge template_SizeEdge = GraphUtil.GetQueryOutFirstEdge(template.ItemVertex, "Size", null);

                    Size template_Size = null;

                    if (template_SizeEdge != null)
                    {
                        template_Size = new Size(template_SizeEdge);

                        item.SizeCreate();

                        Size item_Size = item.Size;

                        item_Size.Width = template_Size.Width;
                        item_Size.Height = template_Size.Height;
                    }

                    item.BaseEdgeCreate();

                    IVertex empty = m0.MinusZero.Instance.Empty;

                    IEdge baseEdge = baseEdgeTo.GetAll(false, template.BaseEdgeQuery).FirstOrDefault();

                    Edge item_BaseEdge = item.BaseEdge;

                    item_BaseEdge.From = baseEdge.From;
                    item_BaseEdge.Meta = baseEdge.Meta;
                    item_BaseEdge.To = baseEdge.To;
                }
        }

        void CreateSubConainerControls()
        {            
            int cnt = 0;            

            foreach (ITypedEdge _i in Items)
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

        void InsertSubContainer(IUXItem item, int cnt, bool addSplitter)
        {
            if (!(item is UIElement))
                return;

            item.OwningVisualiser = this.OwningVisualiser;

            UIElement subItem_UIElement = (UIElement)item;

            UXTemplate iUXTemplate = item.UXTemplate;


            Size size = item.Size;
            
            //

            GridSplitter splitter = null;            

            if (addSplitter) {
                splitter = new GridSplitter();

                splitter.Background = (Brush)FindResource("0VeryLightHighlightBrush");

                SubGrid.Children.Add(splitter);

                splitter.DragCompleted += Splitter_DragCompleted;
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

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            UpdateSubItemAchors();
        }
        
        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();

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

            ItemVisualUpdate_Items();
        }

        public void ItemVisualUpdate_Items()
        {
            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.ViewAttributesUpdated();
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

            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;            

                i.Select();
            }

            AddSubItemAchors();
        }

        public override void Unselect()
        {   
            SetBaselineColors();

            base.Unselect();

            //

            foreach (ITypedEdge _i in Items)
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

            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Highlight();
            }
        }
        protected override void UpdateAnchors(double left, double top, double width, double height)
        {
            base.UpdateAnchors(left, top, width, height);

            UpdateSubItemAchors();
        }

        void UpdateSubItemAchors() { UpdateOrAddSubItemAchors(true); }

        void AddSubItemAchors() { UpdateOrAddSubItemAchors(false); }

        void UpdateOrAddSubItemAchors(bool doUpdate)
        {
            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null || !(i is IUXContainer) || !(i is FrameworkElement))
                    continue;

                IUXContainer subItem = (IUXContainer)i;

                FrameworkElement subItem_FrameworkElement = (FrameworkElement)i;

                Point subItemLeftTop = subItem_FrameworkElement.TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

                double _left = subItemLeftTop.X;
                double _top = subItemLeftTop.Y;
                double _right = _left + subItem_FrameworkElement.ActualWidth;

                if (doUpdate)
                    UpdateAnchor(ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine, _right, _top - AnchorSize, subItem);
                else
                    AddAnchor(ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine, _right, _top - AnchorSize, subItem);
            }
        }

        protected void UpdateAnchor(ClickTargetEnum anchorType, double left, double top, IUXItem item)
        {
            foreach (FrameworkElement r in Anchors)
                if (GetAnchorsClickTarget(r) == anchorType && GetAnchorsSubItem(r) == item)
                {
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);

                    r.Width = AnchorSize;
                    r.Height = AnchorSize;
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
        
        // UNDER        

        static IVertex Orientation_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\Orientation");
        static IVertex SubFontSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\SubFontSize");
        static IVertex SubForegroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\SubForegroundColor");
        static IVertex SubBackgroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\SubBackgroundColor");


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