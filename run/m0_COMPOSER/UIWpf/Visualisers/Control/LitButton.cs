using m0.UIWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    public class LitButton: Border
    {
        Brush LitBrush;
        Brush DimBrush;

        Brush LitTextBrush = new SolidColorBrush(Colors.Black);
        Brush DimTextBrush = new SolidColorBrush(Colors.Black);

        public bool Value;

        public void On()
        {
            Value = true;

            Background = LitBrush;

            textBlock.Foreground = LitTextBrush;
        }

        public void Off()
        {
            Value = false;

            Background = DimBrush;

            textBlock.Foreground = DimTextBrush;
        }

        TextBlock textBlock;

        public LitButton(Brush dimBrush, Brush litBrush, Brush dimTextBrush, Brush litTextBrush, string text) {
            DimTextBrush = dimTextBrush;
            LitTextBrush = litTextBrush;

            _LitButton(dimBrush, litBrush, text);
        }

        public LitButton(Brush dimColor, Brush litColor, string text)
        {
            _LitButton(dimColor, litColor, text);
        }
        public void _LitButton(Brush dimColor, Brush litColor, string text)
        {
            LitBrush = litColor;
            DimBrush = dimColor;
            
            textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = 11;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            this.Child = textBlock;

            BorderBrush = new SolidColorBrush((Color)WpfUtil.FindResource("0Foreground"));
            BorderThickness = new Thickness(1);

            Off();

            this.MouseDown += LitButton_MouseDown;
        }

        private void LitButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Value)
                Off();
            else
                On();
        }

        protected void Button_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
