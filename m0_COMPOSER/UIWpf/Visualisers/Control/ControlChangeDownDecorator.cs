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

namespace m0_COMPOSER.UIWpf.Visualisers.Control
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

    public class ControlChangeDownDecorator : StackPanel, IZoomScrollViewAxisDecorator
    {
        public double ValueSpaceMin { get; set; }

        public double ValueSpaceMax { get; set; }

        public double ScreenToValueSpace(double screenPosition) { return 0; }

        public double ValueSpaceToScreen(double valueSpacePosition) { return 0; }



        public bool isHorizontal { get; set; }


        public double PositionMark { get; set; }

        IVertex baseVertex;

        bool showCCList;
        public bool ShowCCList
        {
            get
            {
                return showCCList;
            }

            set {
                showCCList = value;

                ControlInitialize();
            }
        }

        int number;

        public object Selection { get => number; set => number = (int)value; }

        public Size Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        List<AxisSegment> segments;

        public List<AxisSegment> Segments {
            get
            {
                if (this.ActualHeight != heigtUsedForUpdateScale)
                    UpdateScale();

                return segments;
            }

            set { }
        }

        public double BaseUnitSize => throw new NotImplementedException();

        public double SegmentLength => throw new NotImplementedException();

        public event EventHandler SelectionChanged;

        ComboBox List;

        Dictionary<int, IVertex> CCDictionary;

        void PrepareCCDictionary()
        {
            IVertex r = m0.MinusZero.Instance.root;

            CCDictionary = new Dictionary<int, IVertex>();

            IVertex ControlChangeDescriptionSetToUse;

            if (baseVertex != null)
                ControlChangeDescriptionSetToUse = baseVertex;
            else
                ControlChangeDescriptionSetToUse = r.Get(false, @"System\Lib\Music\Data\DefaultControlChangeDescriptionSet:");

            foreach (IEdge e in ControlChangeDescriptionSetToUse.GetAll(false, @"ControlChangeDescription:"))
                CCDictionary.Add((int)GraphUtil.GetIntegerValue(e.To.Get(false, "Number:")), e.To);
        }

        void AddCCs()
        {
            PrepareCCDictionary();

            List.Items.Clear();

            for (int x = -1; x <= 127; x++) {
                CCDescription d = null;

                if (CCDictionary.ContainsKey(x))
                    d = new CCDescription(CCDictionary[x]);
                else
                    d = new CCDescription(x);

                List.Items.Add(d);
            }                        
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            if (_baseVertex != null)
            {
                baseVertex = _baseVertex;

                UpdateCCList();
            }
        }

        public void SetZoomFactor(double zoomFactor)
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

        void DrawLine(bool isBig, double y)
        {
            if (height < 30 && !isBig)
                return;

            double bigLineWidth = 10;

            double smallLineWidth = 5;

            double newy = height - ( (y / 127) * height);

            double size;

            if (isBig)
                size = bigLineWidth;
            else
                size = smallLineWidth;

            Brush brush;            

            if (isBig)            
                brush = (Brush)WpfUtil.FindResource("0ForegroundBrush");                           
            else            
                brush = (Brush)WpfUtil.FindResource("0VeryLightForegroundBrush");
                
            WpfUtil.DrawLine(Scale, scaleWidth - size, newy, scaleWidth, newy, brush);

            AxisSegment a = new AxisSegment();
            
            a.StartPosition = newy;
            a.LineStyle = new LineStyle();
            a.LineStyle.Stroke = brush;

            segments.Add(a);

            if (height > 150 || isBig)
            {
                if(y == 0)
                    WpfUtil.Print(Scale, y.ToString(), scaleWidth - 20, newy - 5, null, 10.0, brush);
                else
                if(y < 100)
                    WpfUtil.Print(Scale, y.ToString(), scaleWidth - 25, newy - 5, null, 10.0, brush);
                else
                    WpfUtil.Print(Scale, y.ToString(), scaleWidth - 30, newy - 5, null, 10.0, brush);
            }
        }

        double scaleWidth;
        double height;
        double heigtUsedForUpdateScale;

        void UpdateScale()
        {
            segments = new List<AxisSegment>();

            heigtUsedForUpdateScale = this.ActualHeight;

            //

            Brush fb = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            scaleWidth = 30;

            if (this.ActualWidth < scaleWidth)
                return;

            listPanel.Width = this.ActualWidth - scaleWidth;
            

            height = this.ActualHeight;

            Scale.Width = scaleWidth;
            Scale.Height = height;

            Scale.Children.Clear();

            WpfUtil.DrawLine(Scale, scaleWidth, 0, scaleWidth, height, fb);

            DrawLine(true, 127);

            DrawLine(true, 100);

            DrawLine(true, 50);

            DrawLine(true, 0);

            DrawLine(false, 120);
            DrawLine(false, 110);

            DrawLine(false, 90);
            DrawLine(false, 80);
            DrawLine(false, 70);
            DrawLine(false, 60);
            
            DrawLine(false, 40);
            DrawLine(false, 30);
            DrawLine(false, 20);
            DrawLine(false, 10);
        }

        StackPanel listPanel;

        public ControlChangeDownDecorator()
        {
            ControlInitialize();
        }

        public void UpdateCCList()
        {
            AddCCs();

            SelectDefault();
        }

        public void ControlInitialize()
        {
            this.Orientation = Orientation.Horizontal;            
            
            //

            List = new ComboBox();            

            List.Margin = new System.Windows.Thickness(0, 0, 4, 0);

            List.LayoutTransform = new ScaleTransform(0.6, 0.6);

            UpdateCCList();

            listPanel = new StackPanel();
            
            if(ShowCCList)
                listPanel.Children.Add(List);

            this.Children.Add(listPanel);

            List.SelectionChanged += List_SelectionChanged;

            //

            Scale = new Canvas();

            this.Children.Add(Scale);

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

            if(SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        public void PositionMarkUpdate() {}

        public bool PositionMarkPrimEnabled { get; set; }
    }
}
