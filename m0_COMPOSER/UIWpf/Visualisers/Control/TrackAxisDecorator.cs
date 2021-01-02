using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using m0;
using System.Windows.Shapes;
using m0.UIWpf;
using System.Windows.Media;
using m0.Graph;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    class TrackAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        double FontSize = 12;

        double segmentSize;

        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        public double BaseUnitSize => throw new NotImplementedException();

        public double BarLength => throw new NotImplementedException();

        IVertex baseVertex;        

        double zoomFactor;

        private void DrawAdditionalSegmentControls(AxisSegment s)
        {
            LitButton m = new LitButton(new SolidColorBrush(Colors.DarkRed), new SolidColorBrush(Colors.Red), "M");

            m.Width = 10;

            WpfUtil.SetPosition(m, Width - 14, s.StartPosition + 2);

            this.Children.Add(m);

            //

            LitButton so = new LitButton(new SolidColorBrush(Colors.DarkKhaki), new SolidColorBrush(Colors.Yellow), "S");

            so.Width = 10;

            WpfUtil.SetPosition(so, Width - 26, s.StartPosition + 2);

            this.Children.Add(so);
        }

        private void Draw()
        {
            Children.Clear();

            Width = 80;

            List<TextBlock> tbl = new List<TextBlock>();

            foreach (AxisSegment s in Segments)
            {
                bool isSelected = false;

                if (Selection != null && ((AxisSegment)Selection).BaseVertex == s.BaseVertex)
                    isSelected = true;

                Border segment = new Border();

                WpfUtil.SetPosition(segment, 0, s.StartPosition, Width, s.EndPosition);

                segment.Background = new SolidColorBrush((Color)WpfUtil.FindResource("0Background"));

                Children.Add(segment);

                TextBlock text = new TextBlock();
                
                text.Text = s.BaseVertex.Value.ToString();

                text.Foreground = new SolidColorBrush((Color)WpfUtil.FindResource("0Foreground"));
                text.Background = new SolidColorBrush((Color)WpfUtil.FindResource("0Background"));

                Color segmentColor = (Color)WpfUtil.FindResource("0Background");

                IVertex trackColorVertex = s.BaseVertex.Get(false, @"Color:");

                if (trackColorVertex != null)
                {
                    Color trackColorVertexColor = WpfUtil.GetColorFromColorVertex(trackColorVertex);

                    Brush trackColorVertexBrush = new SolidColorBrush(trackColorVertexColor);

                    segment.Background = trackColorVertexBrush;

                    text.Background = trackColorVertexBrush;

                    text.Foreground = new SolidColorBrush(WpfUtil.GetNegativeColor(trackColorVertexColor));
                }

                if (segmentSize < 13)                        
                    text.Foreground = segment.Background;

                text.FontSize = FontSize;

                text.Height = segmentSize;

                text.Padding = new Thickness(3, 0, 3, 0);

                text.Width = 50;

                WpfUtil.SetPosition(text, 0, s.StartPosition);            

                Children.Add(text);

                //                

                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

                Children.Add(l);

                //

                DrawAdditionalSegmentControls(s);
            }

            Size newSize = new Size();
            newSize.Width = Width;            

            newSize.Height = Size.Height;

            Size = newSize;
            
            Height = Size.Height;

            //

            WpfUtil.DrawLine(this, 0, 0, 0, Size.Height, 1, (Brush)WpfUtil.FindResource("0ForegroundBrush"));

            WpfUtil.DrawLine(this, Size.Width, 0, Size.Width, Size.Height, 5, (Brush)WpfUtil.FindResource("0ForegroundBrush"));            

            //

            if(Segments.Count > 0)
            {
                Line ld = new Line();

                WpfUtil.SetLinePosition(ld, 0, Size.Height, Size.Width, Height);

                ld.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                Children.Add(ld);
            }            
        }

        private void Update()
        {
            Segments = new List<AxisSegment>();

            int cnt = 0;            

            double maxHeight = 0;            

            foreach(IEdge e in baseVertex.GetAll(false, "Track:"))                
            {                
                AxisSegment segment = new AxisSegment();

                segment.LineStyle = new LineStyle();

                segment.StartPosition = cnt * segmentSize;
                segment.EndPosition = (cnt + 1) * segmentSize;

                if (segment.EndPosition > maxHeight)
                    maxHeight = segment.EndPosition;

                segment.BaseVertex = e.To;

                //

                IVertex colorVertex = segment.BaseVertex.Get(false, "Color:");

                if (colorVertex != null)
                    segment.Color = WpfUtil.GetColorFromColorVertex(colorVertex);                                

                Segments.Add(segment);

                cnt++;
            }

            Size s = new Size();
            s.Height = maxHeight;

            Size = s;

            Draw();
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;            

            Update();
        }

        public void SetZoomFactor(double _zoomFactor)
        {
            zoomFactor = _zoomFactor;

            segmentSize = 3 +  (35 * (zoomFactor / 40) );

            Update();
        }

        public void SetLength(double length)
        {

        }

        public TrackAxisDecorator()
        {
            //this.MouseDown += TrackAxisDecorator_MouseDown;
        }

        private void TrackAxisDecorator_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            foreach (AxisSegment s in Segments)
                if (s.StartPosition <= p.Y && p.Y <= s.EndPosition)
                {
                    if (Selection == s)
                        Selection = null;
                    else
                        Selection = s;

                    if(SelectionChanged!=null)
                        SelectionChanged(sender, null);

                    Draw();
                }
        }

        public event EventHandler SelectionChanged;

        public object Selection { get; set; }
    }
}
