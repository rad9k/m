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
        [ThreadStatic]
        private static Stack<HashSet<IVertex>>
            visitedSetPool;

        public static INoInEdgeInOutVertexVertex VertexToJson_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (from == null || to == null)
                return exe.Stack;

            string json = VertexToJson_Process(from);

            to.Value = json;

            //m0.MinusZero.Instance.UserInteraction.InteractionOutput(GraphUtil.GetStringValue(to));

            return exe.Stack;
        }

        public static string VertexToJson_Process(IVertex baseVertex)
        {
            var buffer = new ArrayBufferWriter<byte>();
            var options = new JsonWriterOptions
            {
                Indented = true,
                SkipValidation = false
            };

            Utf8JsonWriter writer = new Utf8JsonWriter(buffer, options);

            HashSet<IVertex> visited =
                RentVisitedSet();

            try
            {
                ProcessVertex(baseVertex, writer, visited);
                writer.Flush();
                return Encoding.UTF8.GetString(
                    buffer.WrittenSpan);
            }
            finally
            {
                ReturnVisitedSet(visited);
            }
        }

        private static HashSet<IVertex>
            RentVisitedSet()
        {
            Stack<HashSet<IVertex>> pool =
                visitedSetPool;
            if (pool != null && pool.Count > 0)
                return pool.Pop();

            return new HashSet<IVertex>();
        }

        private static void ReturnVisitedSet(
            HashSet<IVertex> visited)
        {
            int visitedCount = visited.Count;
            visited.Clear();

            if (visitedCount > 65_536)
                return;

            visitedSetPool ??=
                new Stack<HashSet<IVertex>>();
            if (visitedSetPool.Count < 4)
                visitedSetPool.Push(visited);
        }

        static void ProcessVertex(IVertex baseVertex, Utf8JsonWriter writer, HashSet<IVertex> visited) {
            ProcessVertexInternal(baseVertex, writer, visited, inArrayContext: false, null);
        }

        static void ProcessVertexInArrayContext(
            IVertex baseVertex,
            Utf8JsonWriter writer,
            HashSet<IVertex> visited,
            IDictionary<object, object>
                precomputedOutEdgesDictionary) {
            ProcessVertexInternal(
                baseVertex,
                writer,
                visited,
                inArrayContext: true,
                precomputedOutEdgesDictionary);
        }

        static bool IsEmptyValueComplexVertex(IVertex v)
        {
            // In this codebase, complex object instances are often represented
            // by vertices with Value == "" (empty string). Those must be serialized
            // as JSON objects, not as atomic string values.
            return v != null && v.Value is string s && s == "";
        }

        static void ProcessVertexInternal(
            IVertex baseVertex,
            Utf8JsonWriter writer,
            HashSet<IVertex> visited,
            bool inArrayContext,
            IDictionary<object, object>
                precomputedOutEdgesDictionary) {
            if (!VertexOperations.CanCopy_ByVertex(baseVertex))
                return;

            if (!visited.Add(baseVertex))
            {
                WriteAtomVertex(baseVertex, writer);
                return;
            }

            IDictionary<object, object>
                baseVertex_OutEdgesDictionary =
                    precomputedOutEdgesDictionary ??
                    baseVertex.GetOutOdgesByMeta();

            
            bool IsHomogenicAndMultipleAndOnlyEmptyMeta = true;

            foreach (KeyValuePair<object, object> kvp in baseVertex_OutEdgesDictionary)
            {
                string meta = kvp.Key.ToString();
                
                if (VertexOperations.CanCopy_ByMetaString(meta) 
                    && meta != "$Empty" 
                    && !VertexOperations.DoOutEdgesDictionaryValueContainViewVertex(kvp.Value))
                    IsHomogenicAndMultipleAndOnlyEmptyMeta = false;

            }
                        
            if (IsHomogenicAndMultipleAndOnlyEmptyMeta && inArrayContext)
            {
                // In array context, unwrap homogenic vertices directly - write their children to current array
                foreach (KeyValuePair<object, object> kvp in baseVertex_OutEdgesDictionary)
                {
                    string meta = kvp.Key.ToString();
                    if (meta != "$Empty") continue;

                    if (kvp.Value is List_VertexBase list)
                    {
                        foreach (IEdge edge in list)
                        {
                            if (VertexOperations.CanCopy_ByEdge(edge) && !VertexOperations.IsViewVertex(edge.Meta) && !VertexOperations.IsSpecialVertex(edge.Meta))
                            {
                                if ((VertexOperations.IsAtomicEdge(edge) || VertexOperations.IsLink(edge)) && !IsEmptyValueComplexVertex(edge.To))
                                    WriteAtomVertex(edge.To, writer);
                                else if (VertexOperations.IsAtomicVertex(edge.To) && !IsEmptyValueComplexVertex(edge.To))
                                    WriteAtomVertex(edge.To, writer);
                                else if (!TryWriteUnwrappedEmpty(
                                    edge.To,
                                    writer,
                                    out IDictionary<object, object>
                                        childOutEdgesDictionary,
                                    inArrayContext: true))
                                    ProcessVertexInArrayContext(
                                        edge.To,
                                        writer,
                                        visited,
                                        childOutEdgesDictionary);
                            }
                        }
                    }
                    else
                    {
                        IEdge edge = (IEdge)kvp.Value;
                        if (VertexOperations.CanCopy_ByEdge(edge) && !VertexOperations.IsViewVertex(edge.Meta) && !VertexOperations.IsSpecialVertex(edge.Meta))
                        {
                            if ((VertexOperations.IsAtomicEdge(edge) || VertexOperations.IsLink(edge)) && !IsEmptyValueComplexVertex(edge.To))
                                WriteAtomVertex(edge.To, writer);
                            else if (VertexOperations.IsAtomicVertex(edge.To) && !IsEmptyValueComplexVertex(edge.To))
                                WriteAtomVertex(edge.To, writer);
                            else if (!TryWriteUnwrappedEmpty(
                                edge.To,
                                writer,
                                out IDictionary<object, object>
                                    childOutEdgesDictionary,
                                inArrayContext: true))
                                ProcessVertexInArrayContext(
                                    edge.To,
                                    writer,
                                    visited,
                                    childOutEdgesDictionary);
                        }
                    }
                }
            }
            else if (IsHomogenicAndMultipleAndOnlyEmptyMeta)
                ProcessVertex_HomogenicAndMultipleAndOnlyEmptyMetaChildren(
                    baseVertex_OutEdgesDictionary,
                    writer,
                    visited);
            else
                ProcessVertex_HeterogenicChildren(
                    baseVertex_OutEdgesDictionary,
                    writer,
                    visited);

        }

        static void ProcessVertex_HomogenicAndMultipleAndOnlyEmptyMetaChildren(
            IDictionary<object, object> outEdgesDictionary,
            Utf8JsonWriter writer,
            HashSet<IVertex> visited)
        {            
            foreach (KeyValuePair<object, object> kvp in outEdgesDictionary)
            {
                string meta = kvp.Key.ToString();
                if (meta != "$Empty") continue;

                if (kvp.Value is List_VertexBase)
                    ProcessVertex_Array(writer, visited, kvp);
                else
                    ProcessVertex_SingleArray(writer, visited, (IEdge)kvp.Value);
            }
        }

        static void ProcessVertex_HeterogenicChildren(
            IDictionary<object, object> outEdgesDictionary,
            Utf8JsonWriter writer,
            HashSet<IVertex> visited)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<object, object> kvp in outEdgesDictionary)
            {
                bool isArray = kvp.Value is List_VertexBase;

                if (kvp.Value is List_VertexBase)
                {
                    // Check if any edges in the list pass the filter (not special vertex)
                    bool hasValidEdges = false;
                    foreach (IEdge e in (List_VertexBase)kvp.Value)
                    {
                        if (VertexOperations.CanCopy_ByEdge(e) && !VertexOperations.IsViewVertex(e.Meta) && !VertexOperations.IsSpecialVertex(e.Meta))
                        {
                            hasValidEdges = true;
                            break;
                        }
                    }
                    if (!hasValidEdges)
                        continue;

                    string meta = kvp.Key.ToString();

                    if (meta == "$Empty" || meta == "")
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

                    // Skip edges with special meta
                    if (VertexOperations.IsSpecialVertex(e.Meta))
                        continue;

                    ProcessVertex_NoArray(writer, visited, e);
                }
            }

            writer.WriteEndObject();
        }

        private static void ProcessVertex_Array(Utf8JsonWriter writer, HashSet<IVertex> visited, KeyValuePair<object, object> kvp)
        {
            writer.WriteStartArray();

            foreach (IEdge e in (List_VertexBase)kvp.Value)
                if (VertexOperations.CanCopy_ByEdge(e) && !VertexOperations.IsViewVertex(e.Meta) && !VertexOperations.IsSpecialVertex(e.Meta))
                {
                    if ((VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e)) && !IsEmptyValueComplexVertex(e.To))
                        WriteAtomVertex(e.To, writer);
                    else if (VertexOperations.IsAtomicVertex(e.To) && !IsEmptyValueComplexVertex(e.To))
                        // If To is atomic (and not an empty-value complex object), write it directly
                        WriteAtomVertex(e.To, writer);
                    else if (!TryWriteUnwrappedEmpty(
                        e.To,
                        writer,
                        out IDictionary<object, object>
                            childOutEdgesDictionary,
                        inArrayContext: true))
                        ProcessVertexInArrayContext(
                            e.To,
                            writer,
                            visited,
                            childOutEdgesDictionary);
                }

            writer.WriteEndArray();
        }

        private static bool TryWriteUnwrappedEmpty(
            IVertex v,
            Utf8JsonWriter writer,
            out IDictionary<object, object>
                outEdgesDictionary,
            bool inArrayContext = false)
        {
            outEdgesDictionary =
                v.GetOutOdgesByMeta();
            IDictionary<object, object> dict =
                outEdgesDictionary;

            // Never unwrap or atomize complex object instances represented as Value == "".
            // Those should be serialized as JSON objects.
            if (IsEmptyValueComplexVertex(v))
                return false;
            
            // Check if this vertex would create an array structure (homogenic with only $Empty)
            bool wouldCreateArray = true;
            foreach (var kvp in dict)
            {
                string meta = kvp.Key.ToString();
                if (!VertexOperations.CanCopy_ByMetaString(meta))
                    continue;
                if (VertexOperations.DoOutEdgesDictionaryValueContainViewVertex(kvp.Value))
                    continue;
                    
                if (meta != "$Empty")
                {
                    wouldCreateArray = false;
                    break;
                }
            }
            
            // If this vertex would create an array and we're in array context, try to unwrap it
            // to avoid nested arrays
            if (wouldCreateArray && dict.Count > 0 && inArrayContext)
            {
                // In array context, unwrap homogenic vertices directly into the current array
                foreach (var kvp in dict)
                {
                    string meta = kvp.Key.ToString();
                    if (!VertexOperations.CanCopy_ByMetaString(meta))
                        continue;
                    if (VertexOperations.DoOutEdgesDictionaryValueContainViewVertex(kvp.Value))
                        continue;
                    if (meta != "$Empty")
                        continue;
                    
                    if (kvp.Value is List_VertexBase list)
                    {
                        // Write all items from the inner array directly to the current array
                        foreach (IEdge edge in list)
                        {
                            if ((VertexOperations.IsAtomicEdge(edge) || VertexOperations.IsLink(edge)) && !IsEmptyValueComplexVertex(edge.To))
                                WriteAtomVertex(edge.To, writer);
                            else if (VertexOperations.IsAtomicVertex(edge.To) && !IsEmptyValueComplexVertex(edge.To))
                                WriteAtomVertex(edge.To, writer);
                            else
                                return false; // Can't unwrap, need to process normally
                        }
                        return true;
                    }
                    else
                    {
                        IEdge edge = (IEdge)kvp.Value;
                        if ((VertexOperations.IsAtomicEdge(edge) || VertexOperations.IsLink(edge)) && !IsEmptyValueComplexVertex(edge.To))
                        {
                            WriteAtomVertex(edge.To, writer);
                            return true;
                        }
                        else if (VertexOperations.IsAtomicVertex(edge.To) && !IsEmptyValueComplexVertex(edge.To))
                        {
                            WriteAtomVertex(edge.To, writer);
                            return true;
                        }
                    }
                }
            }
            
            // If this vertex would create an array and we're NOT in array context, don't unwrap - preserve the array structure
            if (wouldCreateArray && dict.Count > 0)
                return false;
            
            // Find $Empty edges, ignoring view vertices
            IEdge emptyEdge = null;
            foreach (var kvp in dict)
            {
                string meta = kvp.Key.ToString();
                if (!VertexOperations.CanCopy_ByMetaString(meta))
                    continue;
                if (VertexOperations.DoOutEdgesDictionaryValueContainViewVertex(kvp.Value))
                    continue;
                    
                // Non-$Empty meta found - can't unwrap
                if (meta != "$Empty")
                    return false;
                
                // Multiple $Empty or already found one
                if (emptyEdge != null)
                    return false;
                    
                if (kvp.Value is List_VertexBase list)
                {
                    if (list.Count != 1) return false;
                    emptyEdge = list.First();
                }
                else
                {
                    emptyEdge = (IEdge)kvp.Value;
                }
            }
            
            if (emptyEdge == null) return false;
            if (!VertexOperations.IsAtomicEdge(emptyEdge) && !VertexOperations.IsLink(emptyEdge)) return false;
            
            WriteAtomVertex(emptyEdge.To, writer);
            return true;
        }

        private static void ProcessVertex_SingleArray(Utf8JsonWriter writer, HashSet<IVertex> visited, IEdge e)
        {
            writer.WriteStartArray();
            
            if (VertexOperations.CanCopy_ByEdge(e) && !VertexOperations.IsViewVertex(e.Meta) && !VertexOperations.IsSpecialVertex(e.Meta))
            {
                if ((VertexOperations.IsAtomicEdge(e) || VertexOperations.IsLink(e)) && !IsEmptyValueComplexVertex(e.To))
                    WriteAtomVertex(e.To, writer);
                else if (VertexOperations.IsAtomicVertex(e.To) && !IsEmptyValueComplexVertex(e.To))
                    // If To is atomic (and not an empty-value complex object), write it directly
                    WriteAtomVertex(e.To, writer);
                else if (!TryWriteUnwrappedEmpty(
                    e.To,
                    writer,
                    out IDictionary<object, object>
                        childOutEdgesDictionary,
                    inArrayContext: true))
                    ProcessVertexInArrayContext(
                        e.To,
                        writer,
                        visited,
                        childOutEdgesDictionary);
            }

            writer.WriteEndArray();
        }

        private static void ProcessVertex_NoArray(Utf8JsonWriter writer, HashSet<IVertex> visited, IEdge e)
        {
            if (VertexOperations.CanCopy_ByEdge(e) && !VertexOperations.IsViewVertex(e.Meta) && !VertexOperations.IsSpecialVertex(e.Meta)) 
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
                        string toValue = GraphUtil.GetStringValue(e.To);
                        
                        // If meta is empty string or $Empty, use empty string as key
                        if (metaValue == "" || metaValue == "$Empty")
                            writer.WritePropertyName("");
                        // If meta equals the vertex value, use empty string as key
                        else if (metaValue == toValue)
                            writer.WritePropertyName("");
                        else
                            writer.WritePropertyName(metaValue);

                        ProcessVertex(e.To, writer, visited);
                    }
                }
            }
        }

        static void WriteAtomEdge(IEdge e, Utf8JsonWriter writer)
        {               
            string metaValue = GraphUtil.GetStringValue(e.Meta);
            
            // If meta is empty string or $Empty, use empty string as key
            if (metaValue == "" || metaValue == "$Empty")
                writer.WritePropertyName("");
            else
                writer.WritePropertyName(metaValue);

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
