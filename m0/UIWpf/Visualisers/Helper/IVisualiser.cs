using m0.Foundation;
using m0.UIWpf.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0.UIWpf.Visualisers.Helper
{
    public interface IVisualiser : IHasLocalizableEdges, IPlatformClass, IDisposable
    {
        AtomVisualiserHelper VisualiserHelper { get; set; }      

        void OnLoad(object sender, RoutedEventArgs e);

        void BaseEdgeToUpdated();

        void ScaleChange();
    }
}
