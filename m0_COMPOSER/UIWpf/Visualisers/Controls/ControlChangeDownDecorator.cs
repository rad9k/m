using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using System.Windows.Controls;
using System.Windows.Media;
using m0.Graph;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class CCDescription
    {
        IVertex baseVertex;
        int number;

        public CCDescription(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;
        }

        public CCDescription(int _number)
        {
            number = _number;
        }

        public override String ToString()
        {
            return baseVertex.Get(false, "Description:").Value.ToString();
        }

        public int GetNumber()
        {
            if (baseVertex == null)
                return number;

            return (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "Number:"));
        }
    }
    public class ControlChangeDownDecorator : StackPanel, IZoomScrollViewDownDecorator
    {
        public IVertex Selection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler SelectionChanged;

        ComboBox List;

        Dictionary<int, IVertex> CCDictionary;

        void PrepareCCDictionary()
        {
            IVertex r = m0.MinusZero.Instance.root;

            CCDictionary = new Dictionary<int, IVertex>();

            foreach (IEdge e in r.GetAll(false, @"System\Lib\Music\Data\DefaultControlChangeDescriptionSet:\ControlChangeDescription:"))
                CCDictionary.Add((int)GraphUtil.GetIntegerValue(e.To.Get(false, "Number:")), e.To);
        }

        void AddCCs()
        {
            PrepareCCDictionary();

            for (int x = 0; x < 127; x++) {
                CCDescription d = null;

                if (CCDictionary.ContainsKey(x))
                    d = new CCDescription
            }
            {

                CCDescription d = new CCDescription(e.To);
                List.Items.Add(d);
            }
            
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
