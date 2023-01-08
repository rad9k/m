using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public enum ClickTargetEnum
    {
        MouseUpOrLeave, Selection, Item, AnchorLeftTop, AnchorMiddleTop, AnchorRightTop_CreateDiagramLine, AnchorLeftMiddle, AnchorRightMiddle, AnchorLeftBottom, AnchorMiddleBottom, AnchorRightBottom
    }

    public interface IUXAggregator: IUXItem
    {
        Canvas TheCanvas { get; }

        bool IsSelecting { get; }

        bool IsDrawingLine { get; }

        double LineSelectionDelta { get; }

        double ClickPositionX_ItemCordinates { get; set; }
        double ClickPositionY_ItemCordinates { get; set; }

        double ClickPositionX_AnchorCordinates { get; set; }
        double ClickPositionY_AnchorCordinates { get; set; }

        IUXItem ClickedItem { get; set; }

        ClickTargetEnum ClickTarget { get; set; }

        FrameworkElement ClickedAnchor { get; set; }

        //

        Dictionary<IVertex, List<IUXItem>> GetItemsDictionary();

        void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges);

        void SetFocus();

        void UnselectAllSelectedEdges();

        void CheckAndUpdateDiagramLinesForItem(IUXItem item);        

        //

        bool IsExpanded { get; set; }
        
        UX.Size ExpandedSize { get; }

        UX.Size ExpandedSizeCreate();

        UX.Size CollapsedSize { get; }

        UX.Size CollapsedSizeCreate();
    }

}
