using m0.Foundation;
using m0.UIWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IItem : ICentered
    {
        IEdge BaseEdge { get; set; }

        void Add(Canvas canvas);

        void Remove();

        bool IsSelected { get; }        

        string Label { set; }

        bool CanResizeHorizontally { get; }

        bool CanResizeVertically { get; }

        IZoomScrollViewerHost Host { get; set; }

        void OpenDefaultVisualiser();

        void OpenFormVisualiser();

        void PlayHighlight();

        void StopHighlight();

        void SelectHighlight();

        void NoHighlight();

        void Update();

        double HiddenLeft { get; set; }

        double HiddenRight { get; set; }

        double HiddenHorizontalCenter { get; set; }

        double HiddenVerticalCenter { get; set; }

        double HiddenTop { get; set; }

        double HiddenBottom { get; set; }

        void SetHiddenFromReal();

        double Left { get; set; }

        double Right { get; set; }
        
        double Top { get; set; }

        double Bottom { get; set; }        
    }
}
