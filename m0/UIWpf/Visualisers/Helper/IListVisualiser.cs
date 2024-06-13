using m0.UIWpf.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Visualisers.Helper
{
    public interface IListVisualiser : IVisualiser, IHasSelectableEdges
    {
        event Notify SelectedEdgesChange;

        void SelectedVerticesUpdated();

        void ViewAttributesUpdated();

        string[] MetaTriggeringUpdateVertex { get; }

        string[] MetaTriggeringUpdateView { get; }
    }
}
