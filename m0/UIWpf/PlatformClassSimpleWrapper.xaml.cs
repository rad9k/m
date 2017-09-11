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
using System.Windows.Navigation;
using System.Windows.Shapes;
using m0.ZeroTypes;
using m0.Foundation;
using System.Globalization;
using m0.UIWpf.Visualisers;
using m0.Graph;
using Xceed.Wpf.AvalonDock.Layout;

namespace m0.UIWpf
{
    /// <summary>
    /// Interaction logic for PlatformClassSimpleWrapper.xaml
    /// </summary>
    public partial class PlatformClassSimpleWrapper : UserControl
    {
        public bool IsIntialising;

        public PlatformClassSimpleWrapper()
        {
            InitializeComponent();
        }
        
        private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public void HideEventHandler(object sender, EventArgs e)        
        {
            if (sender is LayoutAnchorable && ((LayoutAnchorable)sender).IsHidden == true)
                if (!IsIntialising)
                    CloseContent();
        }

        public void ClosedEventHandler(object sender, EventArgs e)
        //public void ClosedEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!IsIntialising)
                CloseContent();
        }

        private void CloseContent()
        {
            if (Content is IDisposable)
                ((IDisposable)Content).Dispose();

            if (this.expander.Content is IDisposable)
                ((IDisposable)this.expander.Content).Dispose();
        }

        object Content;

        public void SetContent(IPlatformClass pc){
            Content = pc;

            FrameworkElement fe = (FrameworkElement)pc;

            if(fe is IOwnScrolling)
            {
                this.ParentCont.Children.Add(fe);
            }else
                  this.Cont.Content = fe;

            DockPanel.SetDock(fe, Dock.Bottom);

            WrapVisualiser w = new WrapVisualiser();

            w.Scale = 0.6;            

            GraphUtil.ReplaceEdge(w.Vertex.Get("BaseEdge:"), "To", pc.Vertex);
           
            this.expander.Content = w;
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
