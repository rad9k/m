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
    public partial class ImageItem : UXItem
    {        
        public ImageItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public ImageItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        bool shouldTryToLoad = true;
        
        public override void ItemVisualUpdate()
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
                if (ShowName)
                {
                    IVertex baseEdgeTo = BaseEdgeTo;

                    if (baseEdgeTo != null)
                        this.Title.Text = baseEdgeTo.Value.ToString();
                    else
                        this.Title.Text = "Ø";
                }
                else
                {
                    Title.Height = 0;
                }
            }
                       
            this.Frame.BorderThickness = new Thickness(BorderSize);            

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

            this.Frame.BorderBrush = borderBrush;            
        }

        public override void Select()
        {
            base.Select();

            this.Frame.BorderBrush = (Brush)FindResource("0SelectionBrush");

            this.Title.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");

            this.Title.Cursor = Cursors.ScrollAll;    
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

            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 
           
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");
            
            this.Title.Foreground = (Brush)FindResource("0HighlightForegroundBrush");            
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

            //return exe.Stack;

            return base.VertexChange(exe);
        }
        
        // UNDER        

        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\ShowMeta");
        static IVertex ShowName_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\ShowName");
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

        public bool ShowName
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowName", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    val = Vertex.AddVertex(ShowName_meta, value);
                else
                    val.Value = value;
            }
        }
    
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
    }
}