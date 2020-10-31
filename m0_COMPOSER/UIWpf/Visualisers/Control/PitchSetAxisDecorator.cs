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
    class PitchSetAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        double FontSize = 12;

        double segmentSize;

        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        public double BaseUnitSize => throw new NotImplementedException();

        public double BarLength => throw new NotImplementedException();

        IVertex baseVertex;        

        double zoomFactor;

        private void Draw()
        {
            Children.Clear();

            double maxWidth = 60;

            List<TextBlock> tbl = new List<TextBlock>();

            foreach (AxisSegment s in Segments)
            {                
                TextBlock t = new TextBlock();
                
                t.Text = s.BaseVertex.Get(false, "Name:").Value.ToString();                

                if (s.Color != null)
                {
                    t.Background = new SolidColorBrush(s.Color);

                    if (segmentSize > 13)
                        t.Foreground = new SolidColorBrush(WpfUtil.GetNegativeColor(s.Color));
                    else
                        t.Foreground = new SolidColorBrush(s.Color);
                }

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

            Size newSize = new Size();
            newSize.Width = maxWidth;

            if (newSize.Width < 50)
                newSize.Width += 10;

            newSize.Height = Size.Height;

            Size = newSize;

            Width = Size.Width;
            Height = Size.Height;

            foreach (TextBlock tb in tbl)
                tb.Width = Size.Width;

            foreach (AxisSegment s in Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Size.Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

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

                //

                IVertex noteBackgroundColorVertex = segment.BaseVertex.Get(false, "NoteBackgroundColor:");

                if (noteBackgroundColorVertex != null)
                {
                    segment.UseBackgroundColor = true;
                    segment.BackgroundColor = WpfUtil.GetColorFromColorVertex(noteBackgroundColorVertex);
                }


                int? thisOctave = GraphUtil.GetIntegerValue(segment.BaseVertex.Get(false, "Octave:"));

                if(thisOctave != null && thisOctave != prevOctave)
                    {
                        prevOctave = (int)thisOctave;

                        segment.LineStyle.StrokeThickness = 3;
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
