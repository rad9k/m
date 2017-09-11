using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using m0.Foundation;
using m0.UIWpf.Visualisers;
using m0.Graph;
using m0.ZeroTypes;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers.Controls
{
    public class DeleteButton:Button
    {
         public IEdge BaseEdge
        {
            get { return (IEdge)GetValue(BaseEdgeProperty); }
            set { SetValue(BaseEdgeProperty, value); }
        }

        public static readonly DependencyProperty BaseEdgeProperty =
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(DeleteButton), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs _e)
        {
            DeleteButton _this = (DeleteButton)d;
            IEdge e = (IEdge)_e.NewValue;
        }

        public DeleteButton()
        {
            Content = "X";

            this.Style = (Style)Application.Current.FindResource("TransparentStyle"); 

            BorderThickness = new Thickness(0);
            this.Margin = new Thickness(0);
            this.Padding = new Thickness(0);

            Foreground = (Brush)FindResource("0DeleteBrush"); 
        }

        protected override void OnClick(){
            VertexOperations.DeleteOneEdge(BaseEdge.From, BaseEdge.Meta, BaseEdge.To);
        }
        
        
    }
}
