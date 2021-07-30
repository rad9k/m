using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
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

           // m0_RUN.Main.Run();

          //  Close();            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {            
            m0_RUN.Main.Run();

            ExtraRun();

            Close();
        }

        

        void ExtraRun()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex v = r.AddVertex(null, "V");

            GraphUtil.LoadAndParse("x.txt", v);

            IVertex n = VertexOperations.AddInstance(r, v.Get(false, @"Class:"));

            GraphUtil.AddDelegate(v, n, v.Get(false, @"Class:\Method:"));

            v.Execute(new ZeroCodeExecution());

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m0_RUN.AltWindow w = new m0_RUN.AltWindow();
            w.Show();
        }
    }
}
