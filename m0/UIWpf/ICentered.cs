using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf
{
    public interface ICentered
    {
        double HorizontalCenter { get; set; }

        double VerticalCenter { get; set; }

        bool IsCentered { get; }
    }
}
