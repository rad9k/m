using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using m0.Foundation;
using System.Windows.Controls;
using m0.Util;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class TimeSpanAxisDecorator : Canvas, IZoomScrollViewerAxisDecorator
    {
        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        IVertex baseVertex;

        double zoomFactor;

        private void Update()
        {
            Segments = new List<AxisSegment>();

            //int cnt = 0;

            double segmentSize = zoomFactor / 10;

            //foreach (IEdge e in baseVertex)
            for(int cnt=0; cnt < 100;cnt++)
            {
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.SegmentStart = cnt * segmentSize;
                segment.SegmentEnd = -1;

                //segment.baseVertex = e.To;

                Segments.Add(segment);
            }
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
