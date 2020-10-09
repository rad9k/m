using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    public class ControlChangeDownDecorator : StackPanel, IZoomScrollViewDownDecorator
    {
        public IVertex Selection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler SelectionChanged;

        ComboBox List;

        void AddCCs()
        {
            IVertex r = m0.MinusZero.Instance.root;

            foreach(IEdge e in r.Get(false, @"System\Lib\Music\Data\DefaultControlChangeDescriptionSet:"))     
                List.Items.Add(e.To.Get(false, "Name:").Value);
            
        }

        double Scale = 0.6;

        public ControlChangeDownDecorator()
        {
            this.Margin = new System.Windows.Thickness(4, 0, 4, 4);
            this.LayoutTransform = new ScaleTransform(Scale, Scale);

            List = new ComboBox();

            AddCCs();

            Label label = new Label();
            label.Content = " ^ select CC ^";
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            this.Children.Add(List);
            this.Children.Add(label);

            
            
        }
    }
}
