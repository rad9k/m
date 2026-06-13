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

        int sectionRow = 0;

        TextBox AddSection(string label)
        {
            TextBox labelTextBox = new TextBox();
            labelTextBox.Text = label;
            labelTextBox.FontWeight = FontWeights.Bold;
            labelTextBox.Margin = new Thickness(0, 0, 0, 5);
            labelTextBox.Foreground = (Brush)FindResource("0ForegroundBrush");
            labelTextBox.BorderBrush = (Brush)FindResource("0VeryLightHighlightBrush");
            labelTextBox.Background = (Brush)FindResource("0VeryLightHighlightBrush");
            labelTextBox.SetValue(Grid.RowProperty, sectionRow);
            content.Children.Add(labelTextBox);
            sectionRow++;

            TextBox contentTextBox = new TextBox();
            contentTextBox.Margin = new Thickness(0, 0, 0, 5);
            contentTextBox.TextWrapping = TextWrapping.WrapWithOverflow;
            contentTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            contentTextBox.IsReadOnly = true;
            contentTextBox.Foreground = (Brush)FindResource("0ForegroundBrush");
            contentTextBox.BorderBrush = (Brush)FindResource("0VeryLightHighlightBrush");
            contentTextBox.Background = (Brush)FindResource("0VeryLightHighlightBrush");
            contentTextBox.SetValue(Grid.RowProperty, sectionRow);

            if (sectionRow == 1)
                contentTextBox.MaxHeight = 60;
            else if (sectionRow == 3)
                contentTextBox.MaxHeight = 80;
            else if (sectionRow == 5)
            {
                contentTextBox.VerticalAlignment = VerticalAlignment.Stretch;
                contentTextBox.MinHeight = 40;
            }

            content.Children.Add(contentTextBox);
            sectionRow++;

            return contentTextBox;
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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetFullContent());
        }

        private string GetFullContent()
        {
            return "Type:\r\n" + type.Text + "\r\n\r\nWhere:\r\n" + where.Text + "\r\n\r\nWhat:\r\n" + what.Text;
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
