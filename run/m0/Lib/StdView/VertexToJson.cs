using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using m0.Graph;
using System.Text.Json.Serialization;
using m0.ZeroTypes;

namespace m0.Lib.StdView
{
    public class VertexToJson
    {
        public static INoInEdgeInOutVertexVertex VertexToJson_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (from == null || to == null)
                return exe.Stack;

            string json = VertexToJson_Process(from);

            //m0.MinusZero.Instance.UserInteraction.InteractionOutput(GraphUtil.GetStringValue(output));

            return exe.Stack;
        }

        public static string VertexToJson_Process(IVertex baseVertex){
            if (baseVertex == null)
                throw new ArgumentNullException(nameof(baseVertex));

            var recursionPath = new HashSet<IVertex>();

            VertexJsonNode BuildNode(IVertex v, IEdge incomingEdge){
                if (v == null)
                    return null;

                if (incomingEdge != null && !VertexOperations.CanCopyEdge(incomingEdge))
                    return null;

                if (recursionPath.Contains(v))
                    return new VertexJsonNode { Value = ConvertValue(v.Value) };

                var node = new VertexJsonNode{
                    Value = ConvertValue(v.Value),
                    Relationships = new Dictionary<string, object>()
                };

                recursionPath.Add(v);

                var outEdges = v.OutEdges;
                if (outEdges != null)
                {
                    var grouped = outEdges
                        .Where(e => e != null && VertexOperations.CanCopyEdge(e))
                        .GroupBy(e => e.Meta != null && e.Meta.Value != null ? e.Meta.Value.ToString() : "null");

                    foreach (var group in grouped)
                    {
                        var children = group
                            .Select(e => BuildNode(e.To, e))
                            .Where(child => child != null)
                            .ToList();

                        if (children.Count == 1)
                            node.Relationships[group.Key] = children[0];
                        else if(children.Count > 1)
                            node.Relationships[group.Key] = children;
                    }
                }
                
                recursionPath.Remove(v);
                return node;
            }

            var root = BuildNode(baseVertex, null);

            var jsonOptions = new JsonSerializerOptions{ WriteIndented = true };
            return JsonSerializer.Serialize(root, jsonOptions);
        }

        private static object ConvertValue(object value){
            if(value == null)
                return null;

            var type = value.GetType();
            switch(Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                case TypeCode.DateTime:
                    return value;
                default:
                    return value.ToString();
            }
        }

        private class VertexJsonNode
        {
            public object Value { get; set; }

            [JsonExtensionData]
            public Dictionary<string, object> Relationships { get; set; }
        }
    }
}
