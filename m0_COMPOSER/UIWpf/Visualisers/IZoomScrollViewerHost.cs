using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IZoomScrollViewerHost
    {
        void SetZoomFactors(double horizontalZoomFactor, double verticalZoomFactor);

        void ChildControlsLoaded();
    }
}
