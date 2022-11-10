using m0.Foundation;
using m0.Graph;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public interface IUXItem: IItem
    {
        IUXAggregator Diagram { get; set; }

        List<LineDecoratorBase> DiagramLines { get; }

        void VertexSetedUp();

        void Dispose();

        Dictionary<IVertex, List<LineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary();

        void RemoveFromCanvas();

        void DoCreateDiagramLine(UXItem toItem);

        void AddDiagramLineVertex(IEdge edge, IVertex diagramLineDefinition, UXItem toItem);

        void AddDiagramLineObject(IUXItem toItem, LineDecorator lineDecorator);

        void RemoveDiagramLine(LineDecoratorBase line);

        void Select();

        void Unselect();

        void Highlight();

        void Unhighlight();

        void MoveItem(double x, double y);

        void MoveAndResizeItem(double left, double top, double width, double height);

        void AddToSelectedEdges();
        
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
    }
}
