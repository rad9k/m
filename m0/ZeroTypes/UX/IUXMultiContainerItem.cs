using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public interface IUXMultiContainerItem: IUXContainer
    {
        double SubFontSize { get; set; }
        UX.Color SubBackgroundColor { get; }
        UX.Color SubForegroundColor { get; }
    }
}
