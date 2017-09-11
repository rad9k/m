using m0.Foundation;
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
    /// Interaction logic for SelectDialog.xaml
    /// </summary>
    public partial class SelectDialog : Window
    {
        public IVertex SelectedOption = null;

        Point _mousePosition;

        void OnLoad(object sender, RoutedEventArgs e)
        {
            UIWpf.SetWindowPosition(this, _mousePosition);
        }

        public SelectDialog(IVertex info, IVertex options, Point? position)
        {            
            if (options.Count() > 1)
            {
                InitializeComponent();

                if (position != null)
                {
                    _mousePosition = (Point)position;
                    this.Loaded += new RoutedEventHandler(OnLoad);
                }
                else
                    Owner = m0Main.Instance;

                Info.Content = info.Value;

                List.ItemsSource = options;

                ShowDialog();
            }
            else
            {
                IEdge e = options.FirstOrDefault();

                if (e != null)
                    SelectedOption = e.To;
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
             if (List.SelectedItem != null)
                {
                    SelectedOption = ((IEdge)List.SelectedItem).To;

                    Close();
                }
        }
    }
}
