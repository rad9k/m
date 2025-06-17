using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.UIWpf.Controls
{
    interface IHasScrollViewer
    {
        ScrollViewer GetScrollViewer();
    }
}
