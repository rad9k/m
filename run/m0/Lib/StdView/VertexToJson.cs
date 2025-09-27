using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

            to.Value = json;

            m0.MinusZero.Instance.UserInteraction.InteractionOutput(GraphUtil.GetStringValue(to));

            return exe.Stack;
        }

        static string VertexToJson_Process(IVertex baseVertex)
        {
            var buffer = new ArrayBufferWriter<byte>();
            var options = new JsonWriterOptions
            {
                Indented = true,
                SkipValidation = false
            };

            Utf8JsonWriter writer = new Utf8JsonWriter(buffer, options);

            ProcessVertex(baseVertex, writer);

            writer.Flush();
            return Encoding.UTF8.GetString(buffer.WrittenSpan);            
        }

        static void ProcessVertex(IVertex baseVertex, Utf8JsonWriter writer) { 
            writer.WriteStartObject();

            foreach (IEdge e in baseVertex)
            {                
                if (VertexOperations.IsAtomicEdge(e))

            }
            
            writer.WriteEndObject();            
        }

    }
}
