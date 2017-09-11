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
using System.Windows.Media.Imaging;
using m0.UIWpf.Commands;

namespace m0.UIWpf.Visualisers.Controls
{
    public class InfoButton:Button
    {
         public IEdge BaseEdge
        {
            get { return (IEdge)GetValue(BaseEdgeProperty); }
            set { SetValue(BaseEdgeProperty, value); }
        }

        public static readonly DependencyProperty BaseEdgeProperty =
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(InfoButton), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs _e)
        {
            InfoButton _this = (InfoButton)d;
            IEdge e = (IEdge)_e.NewValue;
        }

        public InfoButton()
        {
            Image i = new Image();
            BitmapImage b = new BitmapImage(new Uri("mag.gif", UriKind.Relative));
            int q = b.PixelHeight; // will not load without this
            i.Source = b;            
            
            Content = i;

            this.Style = (Style)Application.Current.FindResource("TransparentStyle");

            BorderThickness = new Thickness(0);
            this.Margin = new Thickness(0);
            this.Padding = new Thickness(0);

        
        }

  

        protected override void OnClick(){
            FormVisualiser v=(FormVisualiser)UIWpf.getParentFormVisualiser(this);

            if (v != null)
                Edge.ReplaceEdgeEdges(v.Vertex.Get("BaseEdge:"), BaseEdge);
            else
            {
                IVertex v2 = MinusZero.Instance.CreateTempVertex();
                Edge.AddEdgeEdges(v2, BaseEdge);

                //BaseCommands.Open(v2,null); // want Form Visuliser always
                BaseCommands.OpenFormVisualiser(v2);
            }
        }
        
        
    }
}
