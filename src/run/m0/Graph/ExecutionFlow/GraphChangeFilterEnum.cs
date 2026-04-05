using m0.Foundation;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public enum GraphChangeFilterEnum { 
        OnlyNonTransactedRootVertexEvents, 
        FilterOutRootVertexEvents, 
        ValueChange, 
        OutputEdgeAdded, 
        OutputEdgeRemoved, 
        InputEdgeAdded, 
        InputEdgeRemoved, 
        MetaEdgeAdded, 
        MetaEdgeRemoved, 
        OutputEdgeDisposed };

    class GraphChangeFilterEnumHelper
    {
        static IVertex OnlyNonTransactedRootVertexEvents_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OnlyNonTransactedRootVertexEvents");
        static IVertex FilterOutRootVertexEvents_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\FilterOutRootVertexEvents");
        static IVertex ValueChange_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\ValueChange");
        static IVertex OutputEdgeAdded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OutputEdgeAdded");
        static IVertex OutputEdgeRemoved_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OutputEdgeRemoved");
        static IVertex InputEdgeAdded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\InputEdgeAdded");
        static IVertex InputEdgeRemoved_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\InputEdgeRemoved");
        static IVertex MetaEdgeAdded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\MetaEdgeAdded");
        static IVertex MetaEdgeRemoved_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\MetaEdgeRemoved");
        static IVertex OutputEdgeDisposed_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OutputEdgeDisposed");

        public static GraphChangeFilterEnum GetEnum(IVertex v)
        {
            switch (v.Value.ToString())
            {
                case "OnlyNonTransactedRootVertexEvents": return GraphChangeFilterEnum.OnlyNonTransactedRootVertexEvents;
                case "FilterOutRootVertexEvents": return GraphChangeFilterEnum.FilterOutRootVertexEvents;
                case "ValueChange": return GraphChangeFilterEnum.ValueChange;
                case "OutputEdgeAdded": return GraphChangeFilterEnum.OutputEdgeAdded;
                case "OutputEdgeRemoved": return GraphChangeFilterEnum.OutputEdgeRemoved;
                case "InputEdgeAdded": return GraphChangeFilterEnum.InputEdgeAdded;
                case "InputEdgeRemoved": return GraphChangeFilterEnum.InputEdgeRemoved;
                case "MetaEdgeAdded": return GraphChangeFilterEnum.MetaEdgeAdded;
                case "MetaEdgeRemoved": return GraphChangeFilterEnum.MetaEdgeRemoved;
                case "OutputEdgeDisposed": return GraphChangeFilterEnum.OutputEdgeDisposed;                
            }

            return GraphChangeFilterEnum.ValueChange; // can not return null
        }

        public static IVertex GetVertex(GraphChangeFilterEnum e)
        {
            switch (e)
            {
                case GraphChangeFilterEnum.OnlyNonTransactedRootVertexEvents: return OnlyNonTransactedRootVertexEvents_meta;
                case GraphChangeFilterEnum.FilterOutRootVertexEvents: return FilterOutRootVertexEvents_meta;
                case GraphChangeFilterEnum.ValueChange: return ValueChange_meta;
                case GraphChangeFilterEnum.OutputEdgeAdded: return OutputEdgeAdded_meta;
                case GraphChangeFilterEnum.OutputEdgeRemoved: return OutputEdgeRemoved_meta;
                case GraphChangeFilterEnum.InputEdgeAdded: return InputEdgeAdded_meta;
                case GraphChangeFilterEnum.InputEdgeRemoved: return InputEdgeRemoved_meta;
                case GraphChangeFilterEnum.MetaEdgeAdded: return MetaEdgeAdded_meta;
                case GraphChangeFilterEnum.MetaEdgeRemoved: return MetaEdgeRemoved_meta;
                case GraphChangeFilterEnum.OutputEdgeDisposed: return OutputEdgeDisposed_meta;
            }

            return null;
        }
    }

}
