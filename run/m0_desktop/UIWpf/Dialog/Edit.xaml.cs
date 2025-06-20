using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers;
using m0.User.Process.UX;
using m0.ZeroTypes;
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
using System.Windows.Shapes;

namespace m0.UIWpf.Dialog
{    
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class Edit : UserControl
    {
        IVertex baseVertex;
        Point _mousePosition;

        public override string ToString()
        {
            return baseVertex.Value + " edit / new";
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            FormVisualiser.Focus();

            //UIWpf.SetWindowPosition(this, _mousePosition);
        }

        FormVisualiser FormVisualiser;

        public Edit(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            InitializeComponent();

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, baseVertex);

            FormVisualiser = new FormVisualiser(baseEdgeVertex, null, false);

            Wrap.SetContent(FormVisualiser);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FormVisualiser.Dispose();

            MinusZero.Instance.UserInteraction.CloseWindowByContent(this);

            //Close();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            FormVisualiser.Dispose();
        }
    }
}
