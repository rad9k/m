using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public interface IUXContainer: IUXItem
    {
        Canvas Canvas { get; set; }

        bool IsExpanded { get; set; }

        UX.Size ExpandedSize { get; }

        UX.Size ExpandedSizeCreate();

        UX.Size CollapsedSize { get; }

        UX.Size CollapsedSizeCreate();

        bool SubItemsNotVisible { get; set; }

        UX.UXTemplate NewItemUXTemplate { get; set; }
    }
}
