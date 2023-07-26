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

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class CodeItem : UXItem
    {
        CodeControl codeControl;

        //

        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "ShowMeta", "HideHeader", "BorderSize"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //

        IPlatformClass ContentVisualiser;

        public CodeItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public CodeItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        public override void VertexSetedUp()
        {
            if (codeControl != null)
                TheGrid.Children.Remove((UIElement)ContentVisualiser);

            codeControl = new CodeControl(Vertex, true, true);

            Grid.SetRow(codeControl, 2);

            TheGrid.Children.Add(codeControl);

            base.VertexSetedUp();
        }
        
        public override void ItemVisualUpdate()
        {
            if (codeControl != null)
                codeControl.UpdateVertex();

            //

            base.ItemVisualUpdate();

            if (ShowMeta)
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
            
            double BorderSize_nonZero = BorderSize; ;

            if (BorderSize_nonZero == 0)
                BorderSize_nonZero = 1;

            this.Frame.BorderThickness = new Thickness(BorderSize_nonZero);

            if (HideHeader)
            {
                this.TheGrid.RowDefinitions[0].Height = new GridLength(0);
                this.TheGrid.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                this.TheGrid.RowDefinitions[0].Height = new GridLength(17);
                this.TheGrid.RowDefinitions[1].Height = new GridLength(BorderSize_nonZero);
            }

            //

            SetBaselineColors();
        }

        void SetBaselineColors()
        {
            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Frame.Background = backgroundBrush;

            this.Title.Foreground = foregroundBrush;
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


            this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");

            this.Title.Cursor = Cursors.ScrollAll;

            //

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", (Brush)FindResource("0BackgroundBrush"));
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", (Brush)FindResource("0SelectionBrush"));
            }
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBaselineColors();

            this.Title.Cursor = Cursors.Arrow;
        }

        public override void Highlight()
        {
            base.Highlight();

            this.InternalFrame.Background = (Brush)FindResource("0HighlightBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 
           
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");
            
            this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");

            if (ContentVisualiser != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Foreground", (Brush)FindResource("0HighlightForegroundBrush"));
                GeneralUtil.SetPropertyIfPresent(ContentVisualiser, "Background", (Brush)FindResource("0HighlightBrush"));
            }
        }

        public override void Unhighlight()
        {
            base.Unhighlight();
        }
        
        // UNDER        

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeItem\ShowMeta");
        static IVertex HideHeader_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeItem\HideHeader");
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

        public bool HideHeader
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideHeader", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideHeader", null);

                if (val == null)
                    val = Vertex.AddVertex(HideHeader_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}