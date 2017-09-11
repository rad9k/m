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
    public class NewButton:Button
    {       
        public NewButton()
        {               
            Content = "+ Add New";

            this.Foreground = (Brush)FindResource("0LightGrayBrush");

            this.Style = (Style)Application.Current.FindResource("TransparentStyle");

            BorderThickness = new Thickness(0);
            this.Margin = new Thickness(0);
            this.Padding = new Thickness(0);
        }

        protected DependencyObject getParentListVisualiser(DependencyObject e)
        {
            if(e==null)
                return null;

            if(e is ListVisualiser)
                return e;

            return getParentListVisualiser(VisualTreeHelper.GetParent(e));
        }

        protected override void OnClick(){
            ListVisualiser v=(ListVisualiser)getParentListVisualiser(this);

            if (v != null)
            {
                IVertex baseVertex=v.Vertex.Get(@"BaseEdge:\To:");
                IVertex toShowEdgesMeta = v.Vertex.Get(@"ToShowEdgesMeta:\Meta:");

                //VertexOperations.AddInstance(baseVertex, toShowEdgesMeta);
                VertexOperations.AddInstanceByEdgeVertex(baseVertex, toShowEdgesMeta);

            }
        }
        
        
    }
}
