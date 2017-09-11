using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using System.Windows;

namespace m0.UIWpf.Foundation
{
    public interface IHasLocalizableEdges
    {
        IVertex GetEdgeByLocation(Point point);

        IVertex GetEdgeByVisualElement(FrameworkElement visualElement);

        FrameworkElement GetVisualElementByEdge(IVertex edge);
    }
}
