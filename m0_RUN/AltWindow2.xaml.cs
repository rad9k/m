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
    /// Interaction logic for AltWindow2.xaml
    /// </summary>
    public partial class AltWindow2 : Window
    {
        public AltWindow2()
        {
            InitializeComponent();
        }

        string a = "";

        FrameworkElement but;

        private void Button_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int x = 0;

            //but = (FrameworkElement)sender;
        }

        private void Button_LayoutUpdated(object sender, EventArgs e)
        {
            int x = 0;
            
            a += but.Height + "\n";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int x = 0;
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int x = 0;

            but = (FrameworkElement)sender;
        }
    }
}
