using m0.Foundation;
using m0_COMPOSER.UIWpf.Visualisers.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public class AxisSegment
    {
        public IVertex baseVertex;
        public LineStyle lineStyle;
        public double SegmentStart;
        public double SegmentEnd;
    }

    public interface IZoomScrollViewerAxisDecorator
    {
        Size size { get; set; }
        List<AxisSegment> segments { get; set; }

        void SetBaseVertex(IVertex baseVertex);

        void SetZoomFactor(double zoomFactor);
    }
}
