using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers.Method;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0.UIWpf
{
    public class ExecutableVisualiserFactory
    {
        public static bool IsOfExecutableMeta(IVertex metaForForm)
        {
            if (metaForForm == null || metaForForm.Count() == 0)
                return false;

            if (GraphUtil.GetQueryOutCount(metaForForm, "$Is", "Class") > 0)
                return true;

            return false;
        }

        public static IList<IEdge> GetExecutableEdges(IVertex metaForForm)
        {
            IList<IEdge> list = new List<IEdge>();

            foreach (IEdge e in GraphUtil.GetQueryOut(metaForForm, "Method", null))
                list.Add(e);

            return list;
        }

        public static bool IsExecutableVertex(IVertex v)
        {
            if (GraphUtil.GetQueryOutCount(v, "$Is", "Method") > 0  
                 && GraphUtil)
                return true;
            

            return false;
        }

        public static FrameworkElement CreateExecutableVisualiser(IVertex baseEdge, IVertex executableVertex)
        {
            VoidVoidMethodVisualiser vvv = new VoidVoidMethodVisualiser();

            //GraphUtil.CreateOrReplaceEdge(vvv.Vertex,)

            return vvv;
        }


    }
}
