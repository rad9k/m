using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph
{
    public interface IImplementedVertex : IVertex
    {
        void InheritChildsDictionariesNeedsRebuild(bool inDictiories);

        bool HasInheritance { get; set; }

        //void FireChange(VertexChangeEventArgs e);
    }
}
