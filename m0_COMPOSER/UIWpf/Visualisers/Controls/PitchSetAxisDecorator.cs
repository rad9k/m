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

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class PitchSetAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        double FontSize = 12;

        double segmentSize;

        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        IVertex baseVertex;        

        double zoomFactor;

        private void Draw()
        {
            Children.Clear();

            double maxWidth = 0;

            List<TextBlock> tbl = new List<TextBlock>();

            foreach (AxisSegment s in Segments)
            {                
                TextBlock t = new TextBlock();
                t.Text = s.baseVertex.Get(false, "Name:").Value.ToString();

                t.Background = new SolidColorBrush(s.Color);

                t.Foreground = new SolidColorBrush(WpfUtil.GetNegativeColor(s.Color));

                t.FontSize = FontSize;

                t.Height = segmentSize;

                WpfUtil.SetPosition(t, 0, s.StartPosition);

                tbl.Add(t);

                Children.Add(t);

                //

                Size si = WpfUtil.MeasureTextBlock(t);

                if (si.Width > maxWidth)
                    maxWidth = si.Width;
            }

            Size new_si = new Size();
            new_si.Width = maxWidth + 10;
            new_si.Height = Size.Height;

            Size = new_si;

            Width = Size.Width;
            Height = Size.Height;

            foreach (TextBlock tb in tbl)
                tb.Width = Size.Width;

            foreach (AxisSegment s in Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Size.Width, s.StartPosition);

                s.lineStyle.SetStyle(l);

                Children.Add(l);                
            }

            //

            Line lr = new Line();

            WpfUtil.SetLinePosition(lr, Size.Width, 0, Size.Width, Size.Height);

            lr.StrokeThickness = 5;

            lr.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            Children.Add(lr);
        }

        private void Update()
        {
            Segments = new List<AxisSegment>();

            int cnt = 0;

            //segmentSize = FontSize * 1.5;

            double maxHeight = 0;

            int prevOctave = -9;

            foreach(IEdge e in baseVertex.GetAll(false,"VisualisedPitch:"))                
            {                
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.StartPosition = cnt * segmentSize;
                segment.EndPosition = (cnt + 1) * segmentSize;

                if (segment.EndPosition > maxHeight)
                    maxHeight = segment.EndPosition;

                segment.baseVertex = e.To;

                IVertex colorVertex = segment.baseVertex.Get(false, "Color:");

                if (colorVertex != null)
                    segment.Color = WpfUtil.GetColorFromColorVertex(colorVertex);


                int? thisOctave = GraphUtil.GetIntegerValue(segment.baseVertex.Get(false, "Octave:"));

                if(thisOctave != null && thisOctave != prevOctave)
                    {
                        prevOctave = (int)thisOctave;

                        segment.lineStyle.StrokeThickness = 3;
                    }


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

            segmentSize = 3 +  (15 * (zoomFactor / 40) );

            Update();
        }

        public void SetLength(double length)
        {

        }
    }
}
