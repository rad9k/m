using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.UX
{
    public interface IUX
    {
        UXItem uxItem { get; set; }
        UXAggregator uxAggregator { get; set; }
    }
}
