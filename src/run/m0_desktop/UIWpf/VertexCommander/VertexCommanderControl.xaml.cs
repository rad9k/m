using m0.Foundation;
using System.Windows.Controls;

namespace m0.UIWpf.VertexCommander
{
    /// <summary>
    /// Interaction logic for VertexCommanderControl.xaml
    /// </summary>
    public partial class VertexCommanderControl : UserControl
    {
        public IVertex LeftBaseEdge { get; private set; }

        public IVertex RightBaseEdge { get; private set; }

        public VertexCommanderControl()
        {
            InitializeComponent();
        }

        public VertexCommanderControl(IVertex leftBaseEdge, IVertex rightBaseEdge)
        {
            LeftBaseEdge = leftBaseEdge;
            RightBaseEdge = rightBaseEdge;

            InitializeComponent();
        }
    }
}
