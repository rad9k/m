using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IZoomScrollViewerAxisDecorator
    {
        Size SetBaseVertex(IVertex baseVertex);
    }
}
