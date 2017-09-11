using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers;
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
    enum EditDialogEnum {NoEdit, Edit, EditWithName};

    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : UserControl
    {
        IVertex baseVertex;
        Point _mousePosition;

        public override string ToString()
        {
            return baseVertex.Value + " edit / new";
        }

          void OnLoad(object sender, RoutedEventArgs e)
           {
            FormVisuliser.Focus();

            //UIWpf.SetWindowPosition(this, _mousePosition);
        }

        FormVisualiser FormVisuliser;

        public EditDialog(IVertex _baseVertex, Point? position)
        {
            baseVertex = _baseVertex;

            InitializeComponent();

            FormVisuliser = new FormVisualiser();

            Wrap.SetContent(FormVisuliser);

            GraphUtil.ReplaceEdge(FormVisuliser.Vertex.Get("BaseEdge:"), "To", baseVertex);

            this.Loaded += new RoutedEventHandler(OnLoad);

            /*this.Title = baseVertex.Value + " edit / new";

            if (position!=null)
            {
                _mousePosition =(Point) position;
                this.Loaded += new RoutedEventHandler(OnLoad);
            }
            else
                Owner = m0Main.Instance;

            ShowDialog();*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MinusZero.Instance.DefaultShow.CloseWindowByContent(this);

            //Close();
        }
    }
}
