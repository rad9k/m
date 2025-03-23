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
using m0.UIWpf.Visualisers;
using m0.UIWpf.UX;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class RectangleVisualiserItem : RectangleItem_LabeledItem
    {
        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "HideHeader", "ConstantLabel", "LabelQuery", "ShowMeta", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel", "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

        IPlatformClass ContentVisualiser;

        public RectangleVisualiserItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public RectangleVisualiserItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        protected void VisualiserHack()
        {
            if (ContentVisualiser is CodeVisualiser)
            {
                ((CodeVisualiser)ContentVisualiser).ViewAttributesUpdated();
                ((CodeVisualiser)ContentVisualiser).ScaleChange();
            }

            if (ContentVisualiser is ClassVisualiser)
            {
                ((ClassVisualiser)ContentVisualiser).BaseEdgeToUpdated();
                ((ClassVisualiser)ContentVisualiser).ScaleChange();
            }
        }

        public override void VertexSetedUp()
        {
            if (VisualiserClass != null)        
            {
                if (ContentVisualiser != null && ContentVisualiser is IDisposable)
                {
                    TheGrid.Children.Remove((UIElement)ContentVisualiser);
                    ((IDisposable)ContentVisualiser).Dispose();
                }

                ContentVisualiser = PlatformClass.CreatePlatformObject(VisualiserClass, BaseEdge, this.Vertex, true);

                if (ContentVisualiser != null)
                {
                    Grid.SetRow((UIElement)ContentVisualiser, 2);

                    TheGrid.Children.Add((UIElement)ContentVisualiser);

                    UXVisualiser.AddEdgesFromDefintion(ContentVisualiser.Vertex, VisualiserVertex);

                    VisualiserHack();
                }
            }
            else
            {
                ContentVisualiser = null;
            }
                    

            base.VertexSetedUp();
        }

        protected override void UpdateLabelControl(FrameworkElement LabelControl)
        {
            LabelContainer.Child = LabelControl;
        }

        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();

            double roundEdgeSize = RoundEdgeSize;
            
            double BorderSize_nonZero = BorderSize; ;

            if (BorderSize_nonZero == 0)
                BorderSize_nonZero = 1;

            this.Frame.BorderThickness = new Thickness(BorderSize_nonZero);
            int headerHeight = 16;

            if (ContentVisualiser != null)
            {
                if (HideHeader)
                {
                    headerHeight = 0;

                    this.TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight);
                    this.TheGrid.RowDefinitions[1].Height = new GridLength(0);
                }
                else
                {
                    this.TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight);
                    this.TheGrid.RowDefinitions[1].Height = new GridLength(BorderSize_nonZero);
                }
            }
            else
            {
                this.TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight);
                this.TheGrid.RowDefinitions[1].Height = new GridLength(0);
            }

            //

            //if (roundEdgeSize != 0)
            {
                this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);

                if (ContentVisualiser != null)
                {
                    this.LabelContainer.Margin = new Thickness(RoundEdgeSize + 2, RoundEdgeSize, RoundEdgeSize + 2, 1);

                    ((FrameworkElement)this.ContentVisualiser).Margin = new Thickness(RoundEdgeSize + 2, 0, RoundEdgeSize + 2, RoundEdgeSize);

                    TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight + RoundEdgeSize);
                }
                else
                {
                    this.LabelContainer.Margin = new Thickness(RoundEdgeSize + 2, RoundEdgeSize, RoundEdgeSize + 2, RoundEdgeSize + 1);

                    // this.LabelContainer.TextWrapping = TextWrapping.Wrap;

                    if (roundEdgeSize > 0)
                        TheGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);
                    else
                        TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight + RoundEdgeSize);

                    TheGrid.Children.Remove(InternalFrame);
                }
            }

            //

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

            this.InternalFrame.Background = borderBrush;

            this.Frame.BorderBrush = borderBrush;

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", foregroundBrush);
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", backgroundBrush);
            }
        }

        public override void Select()
        {
            base.Select();

            this.InternalFrame.Background = (Brush)FindResource("0SelectionBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0SelectionBrush");
            
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");

            //

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", (Brush)FindResource("0BackgroundBrush"));
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", (Brush)FindResource("0SelectionBrush"));
            }
        }

        public override void Highlight()
        {
            base.Highlight();

            this.InternalFrame.Background = (Brush)FindResource("0HighlightBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 
           
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");                       

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", (Brush)FindResource("0HighlightForegroundBrush"));
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", (Brush)FindResource("0HighlightBrush"));
            }
        }
        
        // UNDER        

        static IVertex VisualiserClass_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserClass");
        static IVertex VisualiserVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserVertex");

        public IVertex VisualiserClass
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserClass", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserClass", null);

                if (val == null)
                    val = Vertex.AddVertex(VisualiserClass_meta, value);
                else
                    val.Value = value;
            }
        }

        public IVertex VisualiserVertex
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserVertex", null);

                if (val == null)
                    return null;

                return val;
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "VisualiserVertex", null);

                if (val == null)
                    val = Vertex.AddVertex(VisualiserVertex_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}