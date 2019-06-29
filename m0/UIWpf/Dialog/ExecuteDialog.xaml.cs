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
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class ExecuteDialog : UserControl
    {
        IVertex baseVertex;

        public override string ToString()
        {
            return baseVertex + " execute";
        }

        public ExecuteDialog(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            InitializeComponent();

            
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
