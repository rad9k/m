using m0.UIWpf.Visualisers.Helper;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.UIWpf.UX.Generic
{
    public interface IUX: IVisualiser // to delete
    {
        IUXItem UXItem { get; set; }

        //IUXVisualiser IUXAggregator { get; set; }
        IUXContainer IUXAggregator { get; set; }

        Canvas Canvas { get; } 
    }
}
