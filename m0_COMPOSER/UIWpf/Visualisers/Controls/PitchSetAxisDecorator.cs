using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class PitchSetAxisDecorator : Canvas, IZoomScrollViewerAxisDecorator
    {
        public Size size { get; set; }
        public List<AxisSegment> segments { get; set; }

        IVertex baseVertex;

        public Size SetBaseVertex(IVertex _baseVertex)
        {
            throw new NotImplementedException();
        }

        public void SetZoomFactor(double zoomFactor)
        {
            throw new NotImplementedException();
        }

        void IZoomScrollViewerAxisDecorator.SetBaseVertex(IVertex baseVertex)
        {
            throw new NotImplementedException();
        }
    }
}
