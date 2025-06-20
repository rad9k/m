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
    public partial class SelectWindow : Window
    {
        public IVertex SelectedOption = null;

        Point _mousePosition;

        void OnLoad(object sender, RoutedEventArgs e)
        {
            WpfUtil.SetWindowPosition(this, _mousePosition);
        }

        public SelectWindow(IVertex info, IList<IEdge> options, bool firstSelected, Point? position)
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

                IList<IVertex> list = new List<IVertex>();

                foreach (IEdge e in options)
                    list.Add(e.To);

                List.ItemsSource = list;

                if (firstSelected)
                    List.SelectedIndex = 0;

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
                    SelectedOption = (IVertex)List.SelectedItem;

                    Close();
                }
        }
    }
}
