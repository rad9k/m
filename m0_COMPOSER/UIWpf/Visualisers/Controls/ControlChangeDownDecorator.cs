using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using System.Windows.Controls;
using System.Windows.Media;
using m0.Graph;
using System.Windows;
using System.Windows.Shapes;
using m0.UIWpf;

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
            StringBuilder s = new StringBuilder();

            int number = GetNumber();

            if (number == -1)
                s.Append("--");
            else
                s.Append(GetNumber().ToString());

            if (baseVertex != null)
            {
                s.Append(" ");
                s.Append(baseVertex.Get(false, "Description:"));

                IVertex type = baseVertex.Get(false, "Type:");
                if(type != null)
                {
                    s.Append(" [");
                    s.Append(type);
                    s.Append("]");
                }
            }

            return s.ToString();
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
        int number;

        public object Selection { get => number; set => throw new NotImplementedException(); }

        public Size Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<AxisSegment> Segments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double BaseUnitSize => throw new NotImplementedException();

        public double BarLength => throw new NotImplementedException();

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

            for (int x = -1; x <= 127; x++) {
                CCDescription d = null;

                if (CCDictionary.ContainsKey(x))
                    d = new CCDescription(CCDictionary[x]);
                else
                    d = new CCDescription(x);

                List.Items.Add(d);
            }                        
        }

        public void SetBaseVertex(IVertex baseVertex)
        {
            throw new NotImplementedException();
        }

        public void SetZoomFactor(double zoomFactor)
        {
            throw new NotImplementedException();
        }

        public void SetLength(double length)
        {
            throw new NotImplementedException();
        }

        void SelectDefault()
        {
            number = -1;

            foreach(object o in List.Items)
            {
                CCDescription d = (CCDescription)o;
                if(d.GetNumber() == number) {
                    List.SelectedItem = o;
                    return;
                }
            }
        }        

        Canvas Scale;

        void DrawLine(double X1, double Y1, double X2, double Y2)
        {
            Line l = new Line();

            l.StrokeThickness = 1;

            WpfUtil.SetLinePosition(l, X1, Y1, X2, Y2);

            Scale.Children.Add(l);
        }

        void UpdateScale()
        {
            double width = 50;

            if (this.ActualWidth < width)
                return;

            listPanel.Width = this.ActualWidth - width;


            double bigLineWidth = 30;

            double smallLineWidth = 20;

            double height = this.ActualHeight;

            Scale.Width = width;
            Scale.Height = height;

            Scale.Children.Clear();

            DrawLine(0, width, height, width);

            DrawLine(width - bigLineWidth, 0, width, 0);

            DrawLine(width - bigLineWidth, height, width, height);

            Scale.Background = new SolidColorBrush(Colors.AliceBlue);
        }

        StackPanel listPanel;

        public ControlChangeDownDecorator()
        {
            this.Orientation = Orientation.Horizontal;            
            
            //

            List = new ComboBox();            

            List.Margin = new System.Windows.Thickness(0, 0, 4, 0);

            List.LayoutTransform = new ScaleTransform(0.6, 0.6);

            AddCCs();

            SelectDefault();

            listPanel = new StackPanel();
            
            listPanel.Children.Add(List);

            this.Children.Add(listPanel);

            List.SelectionChanged += List_SelectionChanged;

            //

            Scale = new Canvas();

            this.Children.Add(Scale);

            this.SizeChanged += ControlChangeDownDecorator_SizeChanged;

            UpdateScale();
            
        }

        private void ControlChangeDownDecorator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScale();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (List.SelectedItem == null)
            {
                Selection = 0;

                return;
            }

            CCDescription d = (CCDescription)List.SelectedItem;

            number = d.GetNumber();

            SelectionChanged(sender, e);
        }
    }
}
