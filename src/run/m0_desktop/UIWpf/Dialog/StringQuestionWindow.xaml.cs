using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers;
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

namespace m0.UIWpf.Dialog
{    
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class StringQuestionWindow : Window
    {        
        Point _mousePosition;

        public String Answer;

        public override string ToString()
        {
            return "question";
        }
        

        public StringQuestionWindow(string question, Point? position)
        {            
            InitializeComponent();

            this.Title = "question";

            Question.Content = question;

            if (position!=null)
            {
                _mousePosition =(Point) position;
                //this.Loaded += new RoutedEventHandler(OnLoad);
            }
            else
                Owner = m0Main.Instance;

            ShowDialog();
        }

        void FinishDialog()
        {
            Answer = AnswerBox.Text;

            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishDialog();
        }

        private void AnswerBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FinishDialog();
        }
    }
}
