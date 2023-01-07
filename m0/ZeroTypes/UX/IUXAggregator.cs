using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public interface IUXAggregator: IUXItem
    {
        Canvas TheCanvas { get; }

        //

        Dictionary<IVertex, List<IUXItem>> GetItemsDictionary();

        void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges);

        void SetFocus();

        void UnselectAllSelectedEdges();

        void CheckAndUpdateDiagramLinesForItem(IUXItem item);

        double LineSelectionDelta { get; }

        //

        bool IsExpanded { get; set; }
        
        UX.Size ExpandedSize { get; }

        UX.Size ExpandedSizeCreate();

        UX.Size CollapsedSize { get; }

        UX.Size CollapsedSizeCreate();
    }

}
