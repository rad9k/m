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

namespace m0
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();

       
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //UIWpf.test.Window1 w = new UIWpf.test.Window1();

            //w.Show();


            m0Main m = new m0Main();

            m.Show();

            Close();
        }
    }
}
