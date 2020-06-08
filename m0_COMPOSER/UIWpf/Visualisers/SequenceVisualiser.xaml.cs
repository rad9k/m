using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using m0_COMPOSER.UIWpf.Visualisers.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class SequenceVisualiser : UserControl, IPlatformClass, IOwnScrolling, IZoomScrollViewerHost
    {                
        protected void SetVertexDefaultValues()
        {
            //Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;
        }

        Canvas Main;

        IVertex baseVertex;
        IVertex pitchSetVertex;
        IVertex timeSpanVertex;

        PitchSetAxisDecorator PitchSetAD;
        TimeSpanAxisDecorator TimeSpanAD;

        int Length;

        double HorizontalZoomFactor;
        double VerticalZoomFactor;

        void VisualiserUpdate()
        {
            SetVertexes();

            if (baseVertex == null)
                return;

            SetupParameters();

            SetAxisDecorators();

            CreateMain();

            SetupScrollViewer();

            DrawMain();
        }

        void SetVertexes()
        {            
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return;

            if (baseVertex.Get(false, "$Is:Sequence") == null)
            {
                baseVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;            

            pitchSetVertex = baseVertex.Get(false, "PitchSet:");

            if (pitchSetVertex == null)
                pitchSetVertex = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");

            timeSpanVertex = baseVertex.Get(false, "TimeSpan:");

            if (timeSpanVertex  == null)
                timeSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultTimeSpan:");

        }

        void SetupParameters()
        {
            if (baseVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "Length:"));
            else
                Length = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "ExtendTimeLength:"));
        }

        void SetAxisDecorators()
        {
            PitchSetAD = new PitchSetAxisDecorator();

            PitchSetAD.SetBaseVertex(pitchSetVertex);


            TimeSpanAD = new TimeSpanAxisDecorator();

            TimeSpanAD.SetBaseVertex(timeSpanVertex);

            TimeSpanAD.SetLength(Length);


            ZoomScrollView.SetVerticalAxisDecorator(PitchSetAD);

            ZoomScrollView.SetHorizontalAxisDecorator(TimeSpanAD);            
        }

        public void CreateMain()
        {
            Main = new Canvas();

            Main.Width = TimeSpanAD.Size.Width;
            Main.Height = PitchSetAD.Size.Height;
        }

        public void SetupScrollViewer()
        {
            ZoomScrollView.SetHost(this);
            ZoomScrollView.SetContent(Main);
        }

        public void DrawMain()
        {

        }

        public void SetZoomFactors(double horizontalZoomFactor, double verticalZoomFactor)
        {
            HorizontalZoomFactor = horizontalZoomFactor;
            VerticalZoomFactor = verticalZoomFactor;
        }

        public SequenceVisualiser()
        {
            InitializeComponent();

            MinusZero mz = MinusZero.Instance;

            this.Foreground = (Brush)FindResource("0ForegroundBrush");
            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.BorderThickness = new Thickness(0);
            this.Padding = new Thickness(0);
            this.AllowDrop = true;

            // THIS REDUCES PERFORMANCE ON LARGE TREES SO commented out
            //VirtualizingStackPanel.SetIsVirtualizing(this, true); 
            //VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);

            if (mz != null && mz.IsInitialized)
            {
                //Vertex = mz.Root.Get(false, @"System\Session\Visualisers").AddVertex(null, "TreeVisualiser" + this.GetHashCode());

                Vertex = mz.CreateTempVertex();
                Vertex.Value = "SequenceVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\Sequence"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                SetVertexDefaultValues();         

                /*this.ContextMenu = new m0ContextMenu(this);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;
                this.Drop += dndDrop;

                this.MouseEnter += dndMouseEnter;*/
            }
        }


        protected void UpdateBaseEdge()
        {
            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (bas != null)
                VisualiserUpdate();
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();

            if ((sender == Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))
                UpdateBaseEdge();

            if (sender == Vertex.Get(false, @"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                MinusZero mz = MinusZero.Instance;

                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(false, @"System\Session\Visualisers"), Vertex);

                /*foreach (UIElement e in Children)
                {
                    if (e is StackPanel)
                        foreach (UIElement ee in ((StackPanel)e).Children)
                            if (ee is IDisposable)
                                ((IDisposable)ee).Dispose();
                }*/
            }
        }
    }
}
