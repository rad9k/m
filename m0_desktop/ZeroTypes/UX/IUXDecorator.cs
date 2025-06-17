using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public interface IUXDecorator: IUXItem // there seems to be a need to have IUXDecorator type for UXItem.GetUXItem
    {
    }
}
