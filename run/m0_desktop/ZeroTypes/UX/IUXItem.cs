using m0.Foundation;
using m0.Graph;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public interface IUXItem: IItem, IDisposable
    {
        bool ForceVertexChangeOff { get; set; }

        IEdge ContainerEdge { get; set; }

        int NestingLevel { get; set; }

        IUXVisualiser OwningVisualiser { get; set; }        

        bool IsSelected { get; set; }

        bool IsHighlighted { get; set; }       

        List<ILineDecoratorBase> DiagramToLines { get; }

        List<ILineDecoratorBase> DiagramToAsMetaLines { get; }

        //        

        void VertexSetedUp();        

        Dictionary<IUXItem, List<ILineDecoratorBase>> GetDiagramLinesToDiagramItemDictionary();

        Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary();

        void RemoveFromCanvas();
        
        void AddDiagramLineObject(IUXItem toItem, ILineDecoratorBase lineDecorator, bool AddDecoratorVertex);

        void RemoveDiagramLine(ILineDecoratorBase line);

        void BaseEdgeToUpdated();

        void ViewAttributesUpdated();

        void Select();

        void Unselect();

        void Highlight();

        void Unhighlight();

        void MoveItem(double x, double y, bool onlyAnchors);

        void MoveAndResizeItem(double left, double top, double width, double height);

        void AddToSelectedEdges();

        Point GetLineAnchorLocation(IUXItem toItem, bool useToPoint, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart);

        void UpdateDiagramLines();

        //
        double Scale { get; set; }
       
        bool DesignMode { get; set; }
        
        UX.Size Size { get; }

        UX.Size SizeCreate();

        UX.Position Position { get; }

        UX.Position PositionCreate();

        LayoutTypeEnum Layout { get; set; }

        UX.Color BackgroundColor { get; }

        UX.Color BackgroundColorCreate();
        
        UX.Color ForegroundColor { get; }

        UX.Color ForegroundColorCreate();

        UX.Color BorderColor { get; }

        UX.Color BorderColorCreate();

        double BorderSize { get; set; }
        
        double Gap { get; set; }

        UX.UXTemplate UXTemplate { get; set; }

        IList<IUXItem> Decorators { get; }
        
        IUXItem AddDecorator(IVertex typeVertex);

        void RemoveDecorator(IUXItem decorator);

        void AddAsToMetaLine(ILineDecoratorBase line);
    }
}
