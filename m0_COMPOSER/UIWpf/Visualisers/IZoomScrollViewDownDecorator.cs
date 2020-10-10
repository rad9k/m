using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IZoomScrollViewDownDecorator: IZoomScrollViewAxisDecorator
    {
        event EventHandler SelectionChanged;

        IVertex Selection { get; set; }
    }
}
