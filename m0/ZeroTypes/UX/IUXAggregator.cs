using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public interface IUXAggregator: IUXItem
    {
        bool IsExpanded { get; set; }
        
        UX.Size ExpandedSize { get; }

        UX.Size ExpandedSizeCreate();

        UX.Size CollapsedSize { get; }

        UX.Size CollapsedSizeCreate();
    }

}
