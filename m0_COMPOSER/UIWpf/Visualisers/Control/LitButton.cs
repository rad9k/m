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
        Brush LitColor;
        Brush DimColor;

        public bool Value;

        public void On()
        {
            Value = true;

            Background = LitColor;
        }

        public void Off()
        {
            Value = false;

            Background = DimColor;
        }

        TextBlock textBlock;

        public LitButton(Brush dimColor, Brush litColor, string text)
        {
            LitColor = litColor;
            DimColor = dimColor;
            
            textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = 8;
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
