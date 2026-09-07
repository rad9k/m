using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using m0.Foundation;
using m0.UIWpf;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for IconItem.xaml
    /// </summary>
    public partial class IconItem : RectangleItem_LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "HideHeader", "BorderSize", "ConstantLabel", "LabelQuery", "ShowMeta", "ShowIcons", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        public IconItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public IconItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        protected override void UpdateLabelControl(FrameworkElement LabelControl)
        {
            RemoveLabelIcon(LabelControl);

            if (!HideHeader)
                LabelContainer.Child = LabelControl;
        }

        public override void BaseEdgeToUpdated()
        {
            base.BaseEdgeToUpdated();

            UpdateIcon();
        }

        public override void ViewAttributesUpdated()
        {
            UpdateIcon();

            base.ViewAttributesUpdated();

            if (!HideHeader)
            {             
                double allHeight = this.ActualHeight;

                TheGrid.RowDefinitions[1].Height = new GridLength(17);

                if (allHeight > 0)
                    this.Height = allHeight;
            }
            else
            {
                double allHeight = this.ActualHeight;

                TheGrid.RowDefinitions[1].Height = new GridLength(0);

                if (allHeight > 0)
                    this.Height = allHeight;
            }

            this.Frame.BorderThickness = new Thickness(BorderSize);

            //

            this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);


            this.LabelContainer.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 0);

            Image.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize/2, RoundEdgeSize, RoundEdgeSize/2);

            TheGrid.RowDefinitions[1].Height = new GridLength(16 + RoundEdgeSize);

            SetBaselineColors();
        }

        protected override void SetBaselineColors()
        {
            base.SetBaselineColors();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Frame.Background = backgroundBrush;
            
            this.Foreground = foregroundBrush;

            this.Frame.BorderBrush = borderBrush;            
        }

        public override void Select()
        {
            base.Select();

            this.Frame.BorderBrush = (Brush)FindResource("0SelectionBrush");
            
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 
           
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");                       
        }

        void UpdateIcon()
        {
            IEdge labelEdge = LabeledItemLabelHelper.GetLabelEdge(BaseEdge, ContentQuery);

            Image.Source = IconServer.GetIconByEdge(labelEdge);

            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Fant);
        }

        static void RemoveLabelIcon(FrameworkElement labelControl)
        {
            StackPanel stack = labelControl as StackPanel;

            if (stack == null)
                return;

            for (int i = stack.Children.Count - 1; i >= 0; i--)
            {
                if (stack.Children[i] is Image)
                    stack.Children.RemoveAt(i);
            }
        }
    }
}
