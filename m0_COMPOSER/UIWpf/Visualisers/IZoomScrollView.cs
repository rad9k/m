using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IZoomScrollView
    {
        void SetHorizontalAxisDecorator(IZoomScrollViewerAxisDecorator decorator);
        void SetVerticalAxisDecorator(IZoomScrollViewerAxisDecorator decorator);
        void SetContent(Control control);
    }
}
