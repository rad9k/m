using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using System.Windows.Forms;
using System.Windows;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class MultiContainerItem : UXContainer_RectangleItem_LabeledItem, IUXMultiContainerItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "HideHeader", "ConstantLabel", "LabelQuery", "ShowMeta", "UseCodeLabel", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel", "BorderSize", "SubFontSize", "SubBackgroundColor", "SubForegroundColor"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

        static IVertex MultiContainerSubItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerSubItem");

        bool SizeInTemplateWasMinus = false;

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

            //

            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.VertexSetedUp();
            }
        }

        void CreateSubItemVertexes()
        {
            IVertex baseEdgeTo = BaseEdge.To;

            if (Items.Count == 0)
                foreach (UXTemplate template in UXTemplate.UXTemplate_)
                {
                    IUXContainer item = (IUXContainer)AddItem(MultiContainerSubItem_type);

                    item.NestingLevel = this.NestingLevel + 1;

                    item.Vertex.Value = template.Name;

                    item.UXTemplate = template;                    

                    // size

                    IEdge template_SizeEdge = GraphUtil.GetQueryOutFirstEdge(template.ItemVertex, "Size", null);

                    Size template_Size = null;

                    if (template_SizeEdge != null)
                    {
                        template_Size = new Size(template_SizeEdge);

                        item.SizeCreate();

                        Size item_Size = item.Size;

                        item_Size.Width = Math.Abs(template_Size.Width); // can be -1 >> grid size in stars 
                        item_Size.Height = Math.Abs(template_Size.Height);
                        // also if template_Size.Width/.Height is < 0, we assume it is < 0 for all the templates


                        if (Orientation == OrientationEnum.Vertical)
                        {
                            if (template_Size.Height < 0)
                                SizeInTemplateWasMinus = true;
                        }
                        else
                        {
                            if (template_Size.Width < 0)
                                SizeInTemplateWasMinus = true;
                        }
                    }

                    // subitemsnotvisible

                    IEdge template_SubItemsNotVisible = GraphUtil.GetQueryOutFirstEdge(template.ItemVertex, "SubItemsNotVisible", null);

                    if (template_SubItemsNotVisible != null)                    
                        if (GraphUtil.GetBooleanValueOrFalse(template_SubItemsNotVisible.To))
                            item.SubItemsNotVisible = true;
                        else
                            item.SubItemsNotVisible = false;

                    // base edge

                    item.BaseEdgeCreate();                    

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

                splitter.Background = (System.Windows.Media.Brush)FindResource("0VeryLightHighlightBrush");

                SubGrid.Children.Add(splitter);

                splitter.DragCompleted += Splitter_DragCompleted;
            }

            if (Orientation == OrientationEnum.Vertical)
            {                                
                if (addSplitter)
                {
                    RowDefinition splitterRow = new RowDefinition();

                    splitterRow.Height = new GridLength(3, GridUnitType.Pixel);

                    SubGrid.RowDefinitions.Add(splitterRow);

                    splitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    splitter.Height = 3;
                    splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;

                    Grid.SetRow(splitter, cnt);

                    cnt++;

                    WpfUtil.DecorateWithCustomCursor(splitter, System.Windows.Input.Cursors.SizeNS);
                }

                //

                RowDefinition rowDefinition = new RowDefinition();

                if (size != null && size.Height != 0)
                {                        
                    if (SizeInTemplateWasMinus)
                        rowDefinition.Height = new GridLength(size.Height, GridUnitType.Star);
                    else
                        rowDefinition.Height = new GridLength(size.Height, GridUnitType.Pixel);
                }

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

                    WpfUtil.DecorateWithCustomCursor(splitter, System.Windows.Input.Cursors.SizeWE);
                }

                //

                ColumnDefinition columnDefinition = new ColumnDefinition();

                if (size != null & size.Width != 0)
                {                    
                    if (SizeInTemplateWasMinus)
                        columnDefinition.Width = new GridLength(size.Width, GridUnitType.Star);
                    else
                        columnDefinition.Width = new GridLength(size.Width, GridUnitType.Pixel);
                }

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

            LabelContainer.Child = LabelControl;

            double roundEdgeSize = RoundEdgeSize;

            if (roundEdgeSize != 0)
            {                
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);
                
                this.LabelContainer.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 0);

                _Canvas.Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

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

            //Brush borderBrush = GetBorderBrush();

            //this.Frame.BorderBrush = borderBrush;
            //this.InternalFrame.BorderBrush = borderBrush;          

            //

            SetBaselineColors();

            //

            ViewAttributesUpdated_Items();
        }

        public void ViewAttributesUpdated_Items()
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
            
            this.InternalFrame.BorderBrush = (System.Windows.Media.Brush)FindResource("0SelectionBrush");
            this.Frame.BorderBrush = (System.Windows.Media.Brush)FindResource("0SelectionBrush");

            this.Foreground = (System.Windows.Media.Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (System.Windows.Media.Brush)FindResource("0SelectionBrush");//new SolidColorBrush(Colors.Red);            

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

            this.InternalFrame.BorderBrush = (System.Windows.Media.Brush)FindResource("0HighlightBrush");
            this.Frame.BorderBrush = (System.Windows.Media.Brush)FindResource("0HighlightBrush");

            this.Foreground = (System.Windows.Media.Brush)FindResource("0HighlightForegroundBrush"); 

            
            this.Frame.Background = (System.Windows.Media.Brush)FindResource("0HighlightBrush");                       

            //

            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Highlight();
            }
        }

        public override void Unhighlight()
        {
            base.Unhighlight();

            //

            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Unhighlight();
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

                System.Windows.Point subItemLeftTop = subItem_FrameworkElement.TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

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

        protected override void SetBaselineColors()
        {
            base.SetBaselineColors();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();
            
            this.Foreground = foregroundBrush;

            this.Frame.Background = backgroundBrush;
            this.Frame.BorderBrush = borderBrush;

            this.InternalFrame.BorderBrush = borderBrush;            
        }
        
        // UNDER        

        static IVertex Orientation_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\Orientation");
        static IVertex SubFontSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\SubFontSize");
        static IVertex SubForegroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\SubForegroundColor");
        static IVertex SubBackgroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\SubBackgroundColor");

        static IVertex Color_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color");

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

        public double SubFontSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "SubFontSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "SubFontSize", null);

                if (val == null)
                    val = Vertex.AddVertex(SubFontSize_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.Color SubBackgroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "SubBackgroundColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color SubBackgroundColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, SubBackgroundColor_meta));
        }

        public UX.Color SubForegroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "SubForegroundColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color SubForegroundColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, SubBackgroundColor_meta));
        }
    }
}