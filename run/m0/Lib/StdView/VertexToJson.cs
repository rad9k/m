using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
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

            IDictionary<object, object> baseVertex_OutEdgesDictionary = baseVertex.GetOutOdgesByMeta();

            
            bool IsHomogenicAndMultipleAndOnlyEmptyMeta = true;

            foreach (KeyValuePair<object, object> kvp in baseVertex_OutEdgesDictionary)
            {
                string meta = kvp.Key.ToString();
                
                if (VertexOperations.CanCopyCountViewMetaString(meta) 
                    && meta != "$Empty" 
                    && !VertexOperations.DoOutEdgesDictionaryValueContainViewVertex(kvp.Value))
                    IsHomogenicAndMultipleAndOnlyEmptyMeta = false;

            }
                        
            if (IsHomogenicAndMultipleAndOnlyEmptyMeta)
                ProcessVertex_HomogenicAndMultipleAndOnlyEmptyMetaChildren(baseVertex, writer, visited);
            else
                ProcessVertex_HeterogenicChildren(baseVertex, writer, visited);

        }

        static void ProcessVertex_HomogenicAndMultipleAndOnlyEmptyMetaChildren(IVertex baseVertex, Utf8JsonWriter writer, IList<IVertex> visited)
        {            
            foreach (KeyValuePair<object, object> kvp in baseVertex.GetOutOdgesByMeta())
            {
                string meta = kvp.Key.ToString();
                if (meta != "$Empty") continue;

                if (kvp.Value is List_VertexBase)
                    ProcessVertex_Array(writer, visited, kvp);
                else
                    ProcessVertex_SingleArray(writer, visited, (IEdge)kvp.Value);
            }
        }

        static void ProcessVertex_HeterogenicChildren(IVertex baseVertex, Utf8JsonWriter writer, IList<IVertex> visited)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<object, object> kvp in baseVertex.GetOutOdgesByMeta())
            {
                bool isArray = kvp.Value is List_VertexBase;

                if (kvp.Value is List_VertexBase)
                {
                    string meta = kvp.Key.ToString();

                    if (meta == "$Empty")
                    {                        
                        writer.WritePropertyName("");

                        ProcessVertex_Array(writer, visited, kvp);
                    }
                    else
                    {
                        writer.WritePropertyName(meta);

                        ProcessVertex_Array(writer, visited, kvp);
                    }
                }
                else
                {
                    IEdge e = (IEdge)kvp.Value;

                    ProcessVertex_NoArray(writer, visited, e);
                }
            }

            writer.WriteEndObject();
        }

        private static void ProcessVertex_Array(Utf8JsonWriter writer, IList<IVertex> visited, KeyValuePair<object, object> kvp)
        {
            writer.WriteStartArray();

            foreach (IEdge e in (List_VertexBase)kvp.Value)
                if (VertexOperations.CanCopyCountViewEdge(e) && !VertexOperations.IsViewVertex(e.Meta))
                {
                    if (VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e))
                        WriteAtomVertex(e.To, writer);
                    else
                        ProcessVertex(e.To, writer, visited);
                }

            writer.WriteEndArray();
        }

        private static void ProcessVertex_SingleArray(Utf8JsonWriter writer, IList<IVertex> visited, IEdge e)
        {
            writer.WriteStartArray();
            
            if (VertexOperations.CanCopyCountViewEdge(e) && !VertexOperations.IsViewVertex(e.Meta))
            {
                if (VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e))
                    WriteAtomVertex(e.To, writer);
                else
                    ProcessVertex(e.To, writer, visited);
            }

            writer.WriteEndArray();
        }

        private static void ProcessVertex_NoArray(Utf8JsonWriter writer, IList<IVertex> visited, IEdge e)
        {
            if (VertexOperations.CanCopyCountViewEdge(e) && !VertexOperations.IsViewVertex(e.Meta)) 
            {
                string metaValue = GraphUtil.GetStringValue(e.Meta);

                if (GraphUtil.ExistQueryOut(e.Meta, "$IsJsonArray", null))
                {
                    writer.WritePropertyName(metaValue);
                    ProcessVertex_SingleArray(writer, visited, e);
                }
                else
                {
                    if (VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e)) // NoArray
                        WriteAtomEdge(e, writer);
                    else
                    {                        
                        if (metaValue != "$Empty")
                            writer.WritePropertyName(metaValue);
                        else
                            writer.WritePropertyName(GraphUtil.GetStringValue(e.To));

                        ProcessVertex(e.To, writer, visited);
                    }
                }
            }
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
