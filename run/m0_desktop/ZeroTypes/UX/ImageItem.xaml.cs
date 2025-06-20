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
using System.Xml.Linq;
using m0.User.Process.UX;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class ImageItem : RectangleItem_LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "HideHeader", "BorderSize", "ConstantLabel", "LabelQuery", "ShowMeta", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        public ImageItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public ImageItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        bool shouldTryToLoad = true;

        protected override void UpdateLabelControl(FrameworkElement LabelControl)
        {
            if (!HideHeader)
                LabelContainer.Child = LabelControl;
        }

        public override void ViewAttributesUpdated()
        {
            if(shouldTryToLoad)
                try
                {
                    BitmapImage b = new BitmapImage(new Uri("images\\" + Filename, UriKind.Relative));
                    int q = b.PixelHeight; // will not load without this
                    Image.Source = b;
                } catch {
                    UserInteractionUtil.ShowError("ImageItem", "images\\" + Filename + " not found");

                    shouldTryToLoad = false;
                }

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
        
        // UNDER        

        static IVertex Filename_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\Filename");

        public string Filename
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Filename", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Filename", null);

                if (val == null)
                    val = Vertex.AddVertex(Filename_meta, value);
                else
                    val.Value = value;
            }
        }

    }
}