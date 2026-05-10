using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Visualisers
{
    public interface IKeyboardHighlight
    {
        // If -1 - we can have IsBeforeFirstPosition == true or IsAfterLastPosition == true
        int CurrentHighlightPosition { get; }

        bool IsBeforeFirstPosition { get; }

        bool IsAfterLastPosition { get; }

        bool IsFirstPosition { get; set; }

        bool IsLastPosition { get; set; }

        bool CanGoBeforeFirstPosition { get; }

        bool CanGoAfterLastPosition { get; }

        event EventHandler GoneBeforeFirstPosition;

        event EventHandler GoneAfterLastPosition;
    }
}
