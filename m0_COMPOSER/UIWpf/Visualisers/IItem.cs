using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IItem
    {
        IEdge BaseEdge { get; set; }

        bool IsSelected { get; }

        string Label { set; }

        bool CanResizeHorizontally { get; }

        bool CanResizeVertically { get; }

        IZoomScrollViewerHost Host { get; set; }

        void Select();

        void Unselect();

        void Update();

        double HiddenLeft { get; set; }

        double HiddenRight { get; set; }

        double HiddenTop { get; set; }

        double HiddenBottom { get; set; }

        void SetHiddenFromReal();
        
    }
}
