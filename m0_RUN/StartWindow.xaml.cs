using m0.Foundation;
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

            IVertex v=VertexOperations.AddInstance(r, r.Get(false, @"System\Lib\Music\Sequence"));

            v.AddVertex(r.Get(false, @"System\Lib\Music\Sequence\IsDrum"), "True");

            IVertex v2 = VertexOperations.AddInstance(r, r.Get(false, @"System\Lib\Music\Song"));

            //v2.AddVertex(r.Get(false, @"System\Lib\Music\Sequence\IsDrum"), "True");

            VertexOperations.AddInstance(r, r.Get(false, @"System\Lib\Music\Generator\TriggerSet"));

            VertexOperations.AddInstance(r, r.Get(false, @"System\Lib\Music\Generator\MelodyFlow"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m0_RUN.AltWindow w = new m0_RUN.AltWindow();
            w.Show();
        }
    }
}
