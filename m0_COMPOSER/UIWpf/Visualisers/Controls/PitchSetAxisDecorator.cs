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

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class PitchSetAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        IVertex baseVertex;        

        double zoomFactor;

        private void Draw()
        {
            Children.Clear();

            foreach(AxisSegment s in Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.SegmentStart, 100, s.SegmentStart);

                s.lineStyle.SetStyle(l);

                Children.Add(l);

                //

                TextBlock t = new TextBlock();
                t.Text = s.baseVertex.Get(false, "Name:").Value.ToString();
                t.Foreground = new SolidColorBrush(Colors.Black);

                WpfUtil.SetPosition(t, 0, s.SegmentStart);

                Children.Add(t);
            }
        }

        private void Update()
        {
            Segments = new List<AxisSegment>();

            int cnt = 0;

            double segmentSize = zoomFactor / 3;

            double maxHeight = 0;

            foreach(IEdge e in baseVertex.GetAll(false,"VisualisedPitch:"))                
            {
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.SegmentStart = cnt * segmentSize;
                segment.SegmentEnd = (cnt + 1) * segmentSize;

                if (segment.SegmentEnd > maxHeight)
                    maxHeight = segment.SegmentEnd;

                segment.baseVertex = e.To;

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

            Update();
        }

        public void SetLength(double length)
        {

        }
    }
}
