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
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class ExceptionInfoWindow : Window
    {
        TextBox type;
        TextBox where;
        TextBox what;

        TextBox AddSection(string label)
        {
            //Label l = new Label();
            TextBox l = new TextBox();
            l.Text = label;
            l.FontWeight = FontWeights.Bold;
            l.Margin = new Thickness(0, 0, 0, 5);
            l.Foreground = (Brush)FindResource("0ForegroundBrush");
            l.BorderBrush = (Brush)FindResource("0VeryLightHighlightBrush");
            l.Background = (Brush)FindResource("0VeryLightHighlightBrush");


            content.Children.Add(l);

            TextBox t = new TextBox();

            t.Margin = new Thickness(0, 0, 0, 5);

            content.Children.Add(t);

            t.TextWrapping = TextWrapping.WrapWithOverflow;
            t.IsReadOnly = true;
            t.Foreground = (Brush)FindResource("0ForegroundBrush");            
            t.BorderBrush = (Brush)FindResource("0VeryLightHighlightBrush");
            t.Background = (Brush)FindResource("0VeryLightHighlightBrush");

            return t;
        }

        public ExceptionInfoWindow()
        {
            InitializeComponent();

            type = AddSection("Type:");
            where = AddSection("Where:");
            what = AddSection("What:");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string Type
        {
            set {type.Text = value; }
        }

        public string Where
        {
            set { where.Text = value; }
        }

        public string What
        {
            set { what.Text = value; }
        }
    }
}
