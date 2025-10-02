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

            IList<IVertex> visited = new List<IVertex>();

            ProcessVertex(baseVertex, writer, visited);

            writer.Flush();
            return Encoding.UTF8.GetString(buffer.WrittenSpan);            
        }

        static void ProcessVertex(IVertex baseVertex, Utf8JsonWriter writer, IList<IVertex> visited) {
            if (!VertexOperations.CanCopyCountViewVertex(baseVertex))
                return;

            if (visited.Contains(baseVertex))
            {
                WriteAtomVertex(baseVertex, writer);
                return;
            }

            visited.Add(baseVertex);

            writer.WriteStartObject();

            foreach (KeyValuePair<object,object> kvp in baseVertex.GetOutOdgesByMeta())
            {
                if (kvp.Value is List_VertexBase)
                {
                    string meta = kvp.Key.ToString();

                    if (meta != "$Empty")
                        writer.WritePropertyName(meta);

                    writer.WriteStartArray();

                    foreach(IEdge e in (List_VertexBase)kvp.Value)
                        if (VertexOperations.CanCopyCountViewEdge(e) && !VertexOperations.IsViewVertex(e.Meta))
                        {
                            if (VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e))
                                WriteAtomVertex(e.To, writer);
                            else
                                ProcessVertex(e.To, writer, visited);
                        }

                    writer.WriteEndArray();
                }
                else
                {
                    IEdge e = (IEdge)kvp.Value;

                    if (VertexOperations.CanCopyCountViewEdge(e) && !VertexOperations.IsViewVertex(e.Meta))
                    {
                        if (VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e))
                            WriteAtomEdge(e, writer);
                        else
                        {
                            string metaValue = GraphUtil.GetStringValue(e.Meta);

                            if (metaValue != "$Empty")
                                writer.WritePropertyName(metaValue);
                            else
                                writer.WritePropertyName(GraphUtil.GetStringValue(e.To));

                            ProcessVertex(e.To, writer, visited);
                        }
                    }
                    
                }
            }

            /*foreach (IEdge e in baseVertex)
                if (!VertexOperations.IsViewVertex(e.To))
                {
                    if (VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e))
                        WriteAtomEdge(e, writer);
                    else
                    {
                        string metaValue = GraphUtil.GetStringValue(e.Meta);

                        if (metaValue != "$Empty")
                            writer.WritePropertyName(metaValue);
                        else
                            writer.WritePropertyName(GraphUtil.GetStringValue(e.To));

                        ProcessVertex(e.To, writer, visited);
                    }
                        
                }*/
                        
            writer.WriteEndObject();            
        }

        static void WriteAtomEdge(IEdge e, Utf8JsonWriter writer)
        {
            writer.WritePropertyName(GraphUtil.GetStringValue(e.Meta));

            WriteAtomVertex(e.To, writer);
        }

        static void WriteAtomVertex(IVertex v, Utf8JsonWriter writer)
        {
            var value = v.Value;

            if (value is string)
            {
                writer.WriteStringValue((string)value);
            }
            else if (value is int)
            {
                writer.WriteNumberValue((int)value);
            }
            else if (value is long)
            {
                writer.WriteNumberValue((long)value);
            }
            else if (value is float)
            {
                writer.WriteNumberValue((float)value);
            }
            else if (value is double)
            {
                writer.WriteNumberValue((double)value);
            }
            else if (value is bool)
            {
                writer.WriteBooleanValue((bool)value);
            }
            else if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }

    }
}
