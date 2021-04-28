using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph
{
    public interface IInternalCollectionsVertex : IVertex
    {
        void InheritChildsDictionariesNeedsRebuild(bool inDictiories);

        int InheritanceCount;
    }
}
