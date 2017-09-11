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
    public partial class SelectDialogButton : Window
    {
        public IVertex SelectedOption = null;

        Point _mousePosition;

        void OnLoad(object sender, RoutedEventArgs e)
        {
            UIWpf.SetWindowPosition(this, _mousePosition);
        }

        public SelectDialogButton(IVertex info, IVertex options, Point? position)
        {            
            if (options.Count() > 1)
            {
                InitializeComponent();

                if (position != null)
                {
                    _mousePosition = (Point) position;
                    this.Loaded += new RoutedEventHandler(OnLoad);
                }
                else
                    Owner = m0Main.Instance;

                Info.Content = info.Value;

                AddButtons(options);

                this.Loaded += SelectDialogButton_Loaded;

               // List.ItemsSource = options;

                ShowDialog();
            }
            else
            {
                IEdge e = options.FirstOrDefault();

                if (e != null)
                    SelectedOption = e.To;
            }
        }

        private void SelectDialogButton_Loaded(object sender, RoutedEventArgs e)
        {
            Button b = (Button)this.List.Children[0];

            b.Focusable = true;
            Keyboard.Focus(b);
        }

        private void AddButtons(IVertex options)
        {
            foreach(IEdge e in options)
            {
                Button b = new Button();
                
                b.Margin = new Thickness(5);

                b.Padding = new Thickness(6,2,6,2);

                b.Content = e.To.Value;

                b.Tag = e.To;

                b.Click += Button_Click;

                List.Children.Add(b);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
            SelectedOption = (IVertex)((Button)e.Source).Tag;

            Close();
               
        }
    }
}
