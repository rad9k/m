using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class Initialize
    {
        public static void Run()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(MinusZero.Instance.Root, null, "System");
            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");
            IVertex ZeroTypes = GraphUtil.GetQueryOutFirst(Meta, null, "ZeroTypes");
            IVertex ExecutionFlow = GraphUtil.GetQueryOutFirst(ZeroTypes, null, "ExecutionFlow");
            IVertex CreateViewMetaEdgeAdded = GraphUtil.GetQueryOutFirst(ExecutionFlow, null, "CreateViewMetaEdgeAdded");

            ExecutionFlowHelper.DecorateWithDotNetStaticMethod(CreateViewMetaEdgeAdded,
                "m0.Graph.ExecutionFlow.View, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "CreateView_MetaEdgeAdded");
        }
    }
}
