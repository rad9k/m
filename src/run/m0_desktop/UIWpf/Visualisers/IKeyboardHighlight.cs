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

        bool IsBeforeFirstPosition { get; } // if this is true, the highglighting for this visualiser should be stopped

        bool IsAfterLastPosition { get; } // if this is true, the highglighting for this visualiser should be stopped

        bool IsFirstPosition { get; set; } // setting this to true should start highlighting for this visualiser

        bool IsLastPosition { get; set; } // setting this to true should start highlighting for this visualiser

        bool CanGoBeforeFirstPosition { get; }

        bool CanGoAfterLastPosition { get; }

        event EventHandler GoneBeforeFirstPosition;

        event EventHandler GoneAfterLastPosition;
    }
}
