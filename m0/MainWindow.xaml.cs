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

namespace m0
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MinusZero.Instance.Initialize();

            m0.UIWpf.test.Window1 w = new UIWpf.test.Window1();

            w.Show();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            MinusZero.Instance.Initialize();

            m0.UIWpf.test.Window2 w = new UIWpf.test.Window2();

            w.Show();
        }
    }
}
