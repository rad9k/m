using m0.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers;
using m0.User.Process.UX;
using m0.ZeroTypes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace m0.UIWpf.VertexCommander
{
    /// <summary>
    /// Interaction logic for VertexCommanderControl.xaml
    /// </summary>
    public partial class VertexCommanderControl : UserControl
    {
        private const string InEdgesVisualiserQuery = @"System\Meta\Visualiser\Class:{$Inherits:ShowesInEdges,BaseEdgeTarget:Any}";
        private const string OutEdgesVisualiserQuery = @"System\Meta\Visualiser\Class:{$Inherits:ShowesOutEdges,BaseEdgeTarget:Any}";
        private const string DefaultInEdgesVisualiserClassQuery = @"System\Meta\Visualiser\InEdgesList";
        private const string DefaultOutEdgesVisualiserClassQuery = @"System\Meta\Visualiser\List";

        private IVertex leftInEdgesVisuliserInstance;
        private IVertex rightInEdgesVisuliserInstance;
        private IVertex leftOutEdgesVisuliserInstance;
        private IVertex rightOutEdgesVisuliserInstance;

        public IVertex LeftBaseEdge { get; private set; }

        public IVertex RightBaseEdge { get; private set; }

        public CodeControl LeftQueryStringCodeControl { get; private set; }

        public CodeControl RightQueryStringCodeControl { get; private set; }

        public IVertex LeftInEdgesVisuliserClass { get; private set; }

        public IVertex RightInEdgesVisuliserClass { get; private set; }

        public IVertex LeftOutEdgesVisuliserClass { get; private set; }

        public IVertex RightOutEdgesVisuliserClass { get; private set; }

        public IVertex LeftInEdgesVisuliserInstance { get { return leftInEdgesVisuliserInstance; } }

        public IVertex RightInEdgesVisuliserInstance { get { return rightInEdgesVisuliserInstance; } }

        public IVertex LeftOutEdgesVisuliserInstance { get { return leftOutEdgesVisuliserInstance; } }

        public IVertex RightOutEdgesVisuliserInstance { get { return rightOutEdgesVisuliserInstance; } }

        public VertexCommanderControl()
        {
            InitializeComponent();
            SetStaticControls();
        }

        public VertexCommanderControl(IVertex leftBaseEdge, IVertex rightBaseEdge)
        {
            LeftBaseEdge = leftBaseEdge;
            RightBaseEdge = rightBaseEdge;

            InitializeComponent();
            SetStaticControls();
        }

        private void SetStaticControls()
        {
            LeftQueryStringCodeControl = CreateQueryStringCodeControl();
            RightQueryStringCodeControl = CreateQueryStringCodeControl();

            LeftQueryStringCodeControlHost.Content = LeftQueryStringCodeControl;
            RightQueryStringCodeControlHost.Content = RightQueryStringCodeControl;

            SetDefaultVisualiserClasses();
            SetVisualiserSelectors();
            AddVisualiserSelectorEventHandlers();
            CreateAllVisualiserInstances();
        }

        private static CodeControl CreateQueryStringCodeControl()
        {
            IVertex codeControlVertex = MinusZero.Instance.CreateTempVertex();
            CodeControl codeControl = new CodeControl(codeControlVertex);

            return codeControl;
        }

        private void SetVisualiserSelectors()
        {
            IList<IVertex> inEdgesVisualisers = GetQueryToVertexes(InEdgesVisualiserQuery);
            IList<IVertex> outEdgesVisualisers = GetQueryToVertexes(OutEdgesVisualiserQuery);

            SetVisualiserSelectorItems(LeftInEdgesVisualiserSelector, inEdgesVisualisers, LeftInEdgesVisuliserClass);
            SetVisualiserSelectorItems(RightInEdgesVisualiserSelector, inEdgesVisualisers, RightInEdgesVisuliserClass);
            SetVisualiserSelectorItems(LeftOutEdgesVisualiserSelector, outEdgesVisualisers, LeftOutEdgesVisuliserClass);
            SetVisualiserSelectorItems(RightOutEdgesVisualiserSelector, outEdgesVisualisers, RightOutEdgesVisuliserClass);
        }

        private void SetDefaultVisualiserClasses()
        {
            IVertex defaultInEdgesVisualiserClass = MinusZero.Instance.Root.Get(false, DefaultInEdgesVisualiserClassQuery);
            IVertex defaultOutEdgesVisualiserClass = MinusZero.Instance.Root.Get(false, DefaultOutEdgesVisualiserClassQuery);

            LeftInEdgesVisuliserClass = defaultInEdgesVisualiserClass;
            RightInEdgesVisuliserClass = defaultInEdgesVisualiserClass;
            LeftOutEdgesVisuliserClass = defaultOutEdgesVisualiserClass;
            RightOutEdgesVisuliserClass = defaultOutEdgesVisualiserClass;
        }

        private void AddVisualiserSelectorEventHandlers()
        {
            LeftInEdgesVisualiserSelector.SelectionChanged += LeftInEdgesVisualiserSelector_SelectionChanged;
            RightInEdgesVisualiserSelector.SelectionChanged += RightInEdgesVisualiserSelector_SelectionChanged;
            LeftOutEdgesVisualiserSelector.SelectionChanged += LeftOutEdgesVisualiserSelector_SelectionChanged;
            RightOutEdgesVisualiserSelector.SelectionChanged += RightOutEdgesVisualiserSelector_SelectionChanged;
        }

        private static IList<IVertex> GetQueryToVertexes(string query)
        {
            IList<IVertex> vertexes = new List<IVertex>();

            foreach (IEdge edge in MinusZero.Instance.Root.GetAll(false, query))
                vertexes.Add(edge.To);

            return vertexes;
        }

        private static void SetVisualiserSelectorItems(ComboBox selector, IList<IVertex> visualisers, IVertex defaultVisualiserClass)
        {
            selector.ItemsSource = visualisers;

            IVertex selectedVisualiser = visualisers.FirstOrDefault(visualiser => visualiser == defaultVisualiserClass);

            if (selectedVisualiser != null)
                selector.SelectedItem = selectedVisualiser;
            else if (visualisers.Count > 0)
                selector.SelectedIndex = 0;
        }

        private void CreateAllVisualiserInstances()
        {
            RecreateLeftInEdgesVisualiser();
            RecreateRightInEdgesVisualiser();
            RecreateLeftOutEdgesVisualiser();
            RecreateRightOutEdgesVisualiser();
        }

        private void LeftInEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftInEdgesVisuliserClass = (IVertex)LeftInEdgesVisualiserSelector.SelectedItem;
            RecreateLeftInEdgesVisualiser();
        }

        private void RightInEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightInEdgesVisuliserClass = (IVertex)RightInEdgesVisualiserSelector.SelectedItem;
            RecreateRightInEdgesVisualiser();
        }

        private void LeftOutEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftOutEdgesVisuliserClass = (IVertex)LeftOutEdgesVisualiserSelector.SelectedItem;
            RecreateLeftOutEdgesVisualiser();
        }

        private void RightOutEdgesVisualiserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightOutEdgesVisuliserClass = (IVertex)RightOutEdgesVisualiserSelector.SelectedItem;
            RecreateRightOutEdgesVisualiser();
        }

        private void RecreateLeftInEdgesVisualiser()
        {
            RecreateVisualiser(
                LeftInEdgesVisuliserClass,
                LeftBaseEdge,
                LeftInEdgesVisualiserHost,
                LeftInEdgesWrapVisualiserHost,
                ref leftInEdgesVisuliserInstance);
        }

        private void RecreateRightInEdgesVisualiser()
        {
            RecreateVisualiser(
                RightInEdgesVisuliserClass,
                RightBaseEdge,
                RightInEdgesVisualiserHost,
                RightInEdgesWrapVisualiserHost,
                ref rightInEdgesVisuliserInstance);
        }

        private void RecreateLeftOutEdgesVisualiser()
        {
            RecreateVisualiser(
                LeftOutEdgesVisuliserClass,
                LeftBaseEdge,
                LeftOutEdgesVisualiserHost,
                LeftOutEdgesWrapVisualiserHost,
                ref leftOutEdgesVisuliserInstance);
        }

        private void RecreateRightOutEdgesVisualiser()
        {
            RecreateVisualiser(
                RightOutEdgesVisuliserClass,
                RightBaseEdge,
                RightOutEdgesVisualiserHost,
                RightOutEdgesWrapVisualiserHost,
                ref rightOutEdgesVisuliserInstance);
        }

        private static void RecreateVisualiser(
            IVertex visualiserClass,
            IVertex baseVertex,
            ContentControl visualiserHost,
            ContentControl wrapVisualiserHost,
            ref IVertex visualiserInstance)
        {
            DisposeVisualiser(visualiserHost, wrapVisualiserHost, ref visualiserInstance);

            if (visualiserClass == null)
                return;

            Interaction.BeginInteractionWithGraph();

            try
            {
                IPlatformClass visualiser = PlatformClass.CreatePlatformObject(visualiserClass, baseVertex);
                visualiserInstance = visualiser.Vertex;
                visualiserHost.Content = visualiser;
                SetWrapVisualiser(wrapVisualiserHost, visualiserInstance);
            }
            finally
            {
                Interaction.EndInteractionWithGraph();
            }
        }

        private static void SetWrapVisualiser(ContentControl wrapVisualiserHost, IVertex visualiserInstance)
        {
            DisposeContent(wrapVisualiserHost);

            if (visualiserInstance == null)
                return;

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, visualiserInstance);
            wrapVisualiserHost.Content = new WrapVisualiser(baseEdgeVertex, 0.6, visualiserInstance, true);
        }

        private static void DisposeVisualiser(
            ContentControl visualiserHost,
            ContentControl wrapVisualiserHost,
            ref IVertex visualiserInstance)
        {
            DisposeContent(wrapVisualiserHost);
            DisposeContent(visualiserHost);

            if (visualiserInstance != null)
            {
                visualiserInstance.Dispose();
                visualiserInstance = null;
            }
        }

        private static void DisposeContent(ContentControl host)
        {
            if (host.Content is System.IDisposable disposableContent)
                disposableContent.Dispose();

            host.Content = null;
        }
    }
}
