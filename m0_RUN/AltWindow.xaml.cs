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
using System.Windows.Shapes;

namespace m0_RUN
{
    /// <summary>
    /// Interaction logic for AltWindow.xaml
    /// </summary>
    public partial class AltWindow : Window
    {
        public AltWindow()
        {
            InitializeComponent();
        }

        string a = "";

        //int cnt = 0;

        private void AnimatedHideArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Grid.RowDefinitions[2].Height = new GridLength(e.NewSize.Height);

            //a =  a + (cnt.ToString() + " " + e.NewSize.Height + "\n");

            a += e.NewSize.Height + "\n";

            //       cnt++;

        //    e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int x = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Height += 100;
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double a = Grid.RowDefinitions[2].Height.Value;

            l.Height = a;

            Grid.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Auto);
        }

        Button l;
        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            l = (Button)sender;
        }
    }
}
