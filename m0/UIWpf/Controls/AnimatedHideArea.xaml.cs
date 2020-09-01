using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace m0.UIWpf.Controls
{
    /// <summary>
    /// Interaction logic for AnimatedHideArea.xaml
    /// </summary>
    public partial class AnimatedHideArea : UserControl
    {
        Expander Expander;

        public event EventHandler Expanded;
        public event EventHandler Collapsed;

        public bool IsExpanded {
            get {
                if (Expander == null)
                    return false;

                return this.Expander.IsExpanded;
            }
            set
            {
                if (Expander == null)
                    throw new Exception("Expander control not loaded");

                this.Expander.IsExpanded = value;
            }
        }

        public AnimatedHideArea()
        {
            InitializeComponent();

            this.Loaded += AnimatedHideArea_Loaded;

        }

        private void AnimatedHideArea_Loaded(object sender, RoutedEventArgs e)
        {
            Expander = (Expander)this.Template.FindName("Expander", this);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expanded.Invoke(sender, e);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Collapsed.Invoke(sender, e);
        }
    }

    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double)
                    result *= (double)values[i];
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }        
    }
}
