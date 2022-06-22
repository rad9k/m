using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace m0.UIWpf.Controls
{
    public class AxisSegment
    {
        public IVertex BaseVertex;
        public IEdge BaseEdge;

        public LineStyle LineStyle;
        public double StartPosition;
        public double EndPosition;
        public Color Color;

        public bool UseBackgroundColor;
        public Color BackgroundColor;
    }

    public interface IZoomScrollViewAxisDecorator
    {
        Size Size { get; set; }
        List<AxisSegment> Segments { get; }

        double BaseUnitSize { get; }

        double SegmentLength { get; }

        void SetBaseVertex(IVertex baseVertex);

        void SetZoomFactor(double zoomFactor);

        void SetLength(double length);

        event EventHandler SelectionChanged;

        object Selection { get; set; }

        void PositionMarkUpdate();

        bool PositionMarkPrimEnabled { get; set; }
    }
}
