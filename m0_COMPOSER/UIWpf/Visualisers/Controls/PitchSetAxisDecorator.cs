using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using m0;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class PitchSetAxisDecorator : Canvas, IZoomScrollViewerAxisDecorator
    {
        public Size size { get; set; }
        public List<AxisSegment> segments { get; set; }

        IVertex baseVertex;

        IVertex pitchSet;

        double zoomFactor;

        private void Update()
        {
            segments = new List<AxisSegment>();

            int cnt = 0;

            double segmentSize = zoomFactor / 10;

            foreach(IEdge e in pitchSet)
            {
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.SegmentStart = cnt * segmentSize;
                segment.SegmentEnd = (cnt + 1) * segmentSize;

                segment.baseVertex = e.To;

                segments.Add(segment);

                cnt++;
            }
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            IVertex r = MinusZero.Instance.Root;

            baseVertex = _baseVertex;

            pitchSet = baseVertex.Get(false, "PitchSet:");

            if (pitchSet == null)
                pitchSet = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");
            

            Update();
        }

        public void SetZoomFactor(double _zoomFactor)
        {
            zoomFactor = _zoomFactor;

            Update();
        }
    }
}
