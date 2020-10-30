using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IZoomScrollViewerHost
    {
        void VisualiserDraw();

        void ChildControlsLoaded();

        double Height_Down { get; set; }
    }
}
