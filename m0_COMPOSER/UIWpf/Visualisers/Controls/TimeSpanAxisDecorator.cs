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
    class TimeSpanAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        IVertex baseVertex;

        double zoomFactor;

        int timeSpanStructureDeepLevel;

        private void CreateTimeSpanStructure()
        {
            int deepLevel = 0;
  
        }

        private void Update()
        {
            CreateTimeSpanStructure();

            Segments = new List<AxisSegment>();

            //int cnt = 0;

            double segmentSize = zoomFactor / 10;

            double maxWidth = 0;

            //foreach (IEdge e in baseVertex)
            for(int cnt=0; cnt < 100;cnt++)
            {
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.StartPosition = cnt * segmentSize;
                segment.EndPosition = -1;

                if (maxWidth < segment.StartPosition)
                    maxWidth = segment.StartPosition;

                //segment.baseVertex = e.To;

                Segments.Add(segment);
            }

            Size s = new Size();
            s.Width = maxWidth;

            Size = s;
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
