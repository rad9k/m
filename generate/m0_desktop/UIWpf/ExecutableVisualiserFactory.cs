using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers.Method;
using m0.Util;
using m0.ZeroTypes;
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
                if(GraphUtil.GetQueryOutCount(e.To, "InputParameter", null) == 0
                 && GraphUtil.GetQueryOutCount(e.To, "Output", null) == 0)
                    list.Add(e);

            return list;
        }

        public static bool IsExecutableVertex(IVertex v)
        {
            if (GraphUtil.GetQueryOutCount(v, "$Is", "Method") > 0  
                 && GraphUtil.GetQueryOutCount(v, "InputParameter", null) == 0
                 && GraphUtil.GetQueryOutCount(v, "Output", null) == 0)
                return true;
            

            return false;
        }

        static IVertex root = MinusZero.Instance.Root;

        static IVertex executableVertexMeta = root.Get(false, @"System\Meta\ZeroTypes\HasExecutableVertex\ExecutableVertex");

        public static FrameworkElement CreateExecutableVisualiser(IEdge baseEdge, IVertex executableVertex)
        {
            VoidVoidMethodVisualiser vvv = new VoidVoidMethodVisualiser();

            EdgeHelper.ReplaceEdgeVertexEdges(vvv.Vertex.Get(false, "BaseEdge:"), baseEdge);

            GraphUtil.CreateOrReplaceEdge(vvv.Vertex, executableVertexMeta, executableVertex);

            return vvv;
        }


    }
}
