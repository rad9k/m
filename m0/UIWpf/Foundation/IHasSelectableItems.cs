using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.UIWpf.Foundation
{
    public delegate void Notify();
    public interface IHasSelectableEdges
    {
        event Notify SelectedEdgeChange;
        void UnselectAllSelectedEdges();
    }
}
