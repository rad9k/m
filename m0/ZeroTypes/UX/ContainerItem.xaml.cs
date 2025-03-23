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
    public partial class ContainerItem : UXContainer_RectangleItem_LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "HideHeader", "ConstantLabel", "LabelQuery", "ShowMeta", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel", "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        public ContainerItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public ContainerItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        protected override void UpdateLabelControl(FrameworkElement LabelControl)
        {
            LabelContainer.Child = LabelControl;
        }

        public override void VertexSetedUp()
        {
            base.VertexSetedUp();

            if (Canvas == null) {
                Canvas = new Canvas();
                TheGrid.Children.Add(Canvas);
            }

            Canvas.ClipToBounds = true;

            Grid.SetRow(Canvas, 2);
        }

        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();

            double roundEdgeSize = RoundEdgeSize;

            this.LabelContainer.Margin = new Thickness(RoundEdgeSize + 2, RoundEdgeSize, RoundEdgeSize + 2, 1);

            if (roundEdgeSize != 0)
            {
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);
               
                Canvas.Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

                TheGrid.RowDefinitions[0].Height = new GridLength(16 + RoundEdgeSize);
            }

            double borderSize = BorderSize;

            if (borderSize != 0)
            {
                this.Frame.BorderThickness = new Thickness(borderSize);
                this.InternalFrame.BorderThickness = new Thickness(borderSize / 2);

                this.TheGrid.RowDefinitions[1].Height = new GridLength(borderSize);
            }
            else
            {
                this.Frame.BorderThickness = new Thickness(1);
                this.InternalFrame.BorderThickness = new Thickness(1);
            }

            //

            SetBaselineColors();

            //
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

        public override void Select()
        {
            base.Select();

            this.InternalFrame.BorderBrush = (Brush)FindResource("0SelectionBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0SelectionBrush");

            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");
        }        

        public override void Highlight()
        {
            base.Highlight();

            this.InternalFrame.BorderBrush = (Brush)FindResource("0HighlightBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 

            this.Frame.Background = (Brush)FindResource("0HighlightBrush");         
        }
        
        // ContainerItem     
        
       
    }
}