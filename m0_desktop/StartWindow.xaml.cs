using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Commands;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace m0.desktop
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();

            //m0_desktop.RUN();

            //Close();            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            m0_desktop.RUN();

            Close();
        }
    }
}
