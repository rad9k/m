using m0.UIWpf.Visualisers.Helper;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.UIWpf.UX
{
    public interface IUX: IVisualiser
    {
        UXItem uxItem { get; set; }

        UXAggregator uxAggregator { get; set; }

        Canvas Canvas { get; }
    }
}
