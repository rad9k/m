using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace m0.UIWpf.Controls
{
    interface IMouseWheelHandler
    {
        void MouseWheelAction(MouseWheelEventArgs e);
    }
}
