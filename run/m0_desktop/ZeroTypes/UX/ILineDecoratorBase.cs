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

namespace m0.ZeroTypes.UX
{
    public interface ILineDecoratorBase : IUXItem, IUXDecorator
    {
        double FromX { get; set; }
        double FromY { get; set; }
        double ToX { get; set; }
        double ToY { get; set; }

        bool isSelfRelation { get; set; }

        //

        IUXItem FromDiagramItem { get; set; }        

        void SetPosition(double FromX, double FromY, double ToX, double ToY, bool isSelfRelation, double selfRelationX, double selfRelationY);

        double GetMouseDistance(Point p);

        void UpdateMetaPosition();

        void AddToCanvas();

        // UNDER

        double LineWidth { get; set; }

        UX.IUXItem ToItem { get; set; }
    }
}
