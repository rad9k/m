using m0.Foundation;
using m0_COMPOSER.UIWpf.Visualisers.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace m0_COMPOSER.UIWpf.Visualisers
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

        public object Tag;
    }

    public interface IZoomScrollViewAxisDecorator
    {
        double ValueSpaceMin { get; set; }

        double ValueSpaceMax { get; set; }

        bool isHorizontal { get; set; }
        Size Size { get; set; }
        List<AxisSegment> Segments { get; }

        double BaseUnitSize { get; }

        double SegmentLength { get; }

        void SetBaseVertex(IVertex baseVertex);

        void SetZoomFactor(double zoomFactor);



        double ScreenToValueSpace(double screenPosition);

        double ValueSpaceToScreen(double valueSpacePosition);

        event EventHandler SelectionChanged;

        object Selection { get; set; }

        void PositionMarkUpdate();

        bool PositionMarkPrimEnabled { get; set; }
    }
}
