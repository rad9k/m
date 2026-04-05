using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.Store.FileSystem;

namespace m0.Store.Json
{
    [JsonSerializable(typeof(JsonSerializationData))]
    [JsonSerializable(typeof(JsonVertex))]
    [JsonSerializable(typeof(JsonEdge))]
    [JsonSerializable(typeof(Dictionary<int, StoreId>))]
    [JsonSerializable(typeof(List<JsonVertex>))]
    [JsonSerializable(typeof(List<JsonEdge>))]
    [JsonSerializable(typeof(StoreId))]
    // Dodaj wszystkie możliwe typy dla właściwości object
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(decimal))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(long?))]
    [JsonSerializable(typeof(int?))]
    [JsonSerializable(typeof(double?))]
    [JsonSerializable(typeof(decimal?))]
    [JsonSerializable(typeof(bool?))]
    [JsonSourceGenerationOptions(
        GenerationMode = JsonSourceGenerationMode.Default,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true)]
    public partial class JsonSerializationContext : JsonSerializerContext
    {
    }

    public class JsonSerializationData
    {
        public Dictionary<int, StoreId> StoreIdDictionary { get; set; } = new();
        public List<JsonVertex> Vertices { get; set; } = new();
    }

    public class IdentifierObjectJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long longValue))
                        return longValue;

                    throw new JsonException("Identifier number must fit into Int64.");
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException("Only string and Int64 identifiers are supported.");
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value)
            {
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;
                case long longValue:
                    writer.WriteNumberValue(longValue);
                    break;
                default:
                    throw new JsonException("Only string and integer identifiers are supported.");
            }
        }
    }

    public class PrimitiveValueJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return reader.GetBoolean();
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int intValue))
                        return intValue;

                    if (reader.TryGetInt64(out long longValue))
                        return longValue;

                    if (reader.TryGetDecimal(out decimal decimalValue))
                        return decimalValue;

                    return reader.GetDouble();
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException("Only primitive json values are supported.");
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value)
            {
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case bool boolValue:
                    writer.WriteBooleanValue(boolValue);
                    break;
                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;
                case long longValue:
                    writer.WriteNumberValue(longValue);
                    break;
                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;
                case decimal decimalValue:
                    writer.WriteNumberValue(decimalValue);
                    break;
                default:
                    throw new JsonException("Only primitive clr values are supported.");
            }
        }
    }

    public class JsonVertex
    {
        [JsonPropertyName("Id")]
        [JsonConverter(typeof(IdentifierObjectJsonConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Id { get; set; }

        [JsonPropertyName("Value")]
        [JsonConverter(typeof(PrimitiveValueJsonConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Value { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<JsonEdge> Edges { get; set; } = new();
    }

    public class JsonEdge
    {
        public int MetaStoreId { get; set; }

        [JsonPropertyName("MetaId")]
        [JsonConverter(typeof(IdentifierObjectJsonConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object MetaId { get; set; }

        public int ToStoreId { get; set; }

        [JsonPropertyName("ToId")]
        [JsonConverter(typeof(IdentifierObjectJsonConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object ToId { get; set; }
    }

    public class JsonSerializationStore : StoreBase
    {
        bool canWrite = true;

        private StoreId RootStore;

        // Use source-generated context for maximum performance
        private static readonly JsonSerializationContext JsonContext = new();

        string GetIdentifierToUse(){
            if (Identifier.Contains(System.IO.Path.DirectorySeparatorChar.ToString()))
                return Identifier;
            else
                return MinusZero.Instance.ApplicationPath + System.IO.Path.DirectorySeparatorChar + Identifier;
        }

        void Load()
        {
            string IdentifierToUse = GetIdentifierToUse();

            if (File.Exists(IdentifierToUse))
            {
                try
                {
                    ReadOnlySpan<byte> jsonBytes = File.ReadAllBytes(IdentifierToUse);

                    if (jsonBytes.Length == 0)
                    { // create new sub graph
                        EasyVertex __root = new EasyVertex(this);
                        root = __root;
                        root.IsRoot = true;
                    }
                    else
                    { // load graph from store
                        JsonSerializationData data = JsonSerializer.Deserialize(jsonBytes, JsonContext.JsonSerializationData);

                        ReconstructVerticesFromSerialisationData(data);

                        root = GetVertexByIdentifier(GetRootIdentifier());
                        root.IsRoot = true;

                        Attach();
                    }
                }
                catch (Exception ex)
                {
                    canWrite = false;
                    UserInteractionUtil.ShowException("Json Deserialization from " + Identifier,
                        "Failed to load file: " + ex.Message, ZeroTypes.ExceptionLevelEnum.Error);

                    // Fallback to creating new
                    EasyVertex __root = new EasyVertex(this);
                    root = __root;
                    root.IsRoot = true;
                }
            }
            else
            { // create new
                EasyVertex __root = new EasyVertex(this);
                root = __root;
                root.IsRoot = true;
            }
        }

        internal void VertexIdentifierCompensate(long vertexIdentifierCompensate)
        {
            IDictionary<object, IVertex> oldVertexIdentifiersDictionary = VertexIdentifiersDictionary;

            VertexIdentifiersDictionary = new Dictionary<object, IVertex>();

            foreach (KeyValuePair<object, IVertex> kvp in oldVertexIdentifiersDictionary)
            {
                if (kvp.Value.Identifier is long && kvp.Value is EasyVertex)
                {
                    EasyVertex ev = (EasyVertex)kvp.Value;

                    ev._Identifier = (long)ev.Identifier + vertexIdentifierCompensate;

                    VertexIdentifiersDictionary.Add(ev.Identifier, kvp.Value);
                }
                else // can not compensate
                    VertexIdentifiersDictionary.Add(kvp.Key, kvp.Value);
            }

            VertexIdentifierCount += vertexIdentifierCompensate;
        }

        private void ReconstructVerticesFromSerialisationData(JsonSerializationData data)
        {
            long maxVertexIdentifierCount = 0;

            foreach (JsonVertex jv in data.Vertices)
            {
                object toBeIdentifier;

                if (jv.Id is string)
                    toBeIdentifier = jv.Id;
                else
                {
                    toBeIdentifier = jv.Id;

                    if (toBeIdentifier is long longId && longId > maxVertexIdentifierCount)
                        maxVertexIdentifierCount = longId;
                }

                EasyVertex v = new EasyVertex(this, toBeIdentifier);

                v.Value = jv.Value;

                v._Store = this;

                if (jv.Edges != null)
                foreach (JsonEdge je in jv.Edges)
                {
                    object MetaId = je.MetaId;
                    object ToId = je.ToId;

                    StoreId MetaStoreId = null;

                    if (je.MetaStoreId == 0)
                        MetaStoreId = new StoreId(this.TypeName, this.Identifier);
                    else
                    {
                        if (!data.StoreIdDictionary.ContainsKey(je.MetaStoreId))
                        {
                            UserInteractionUtil.ShowException("Json Deserialisation from " + Identifier, "MetaStoreId " + je.MetaStoreId 
                                + " not found in StoreIdDictionary", ZeroTypes.ExceptionLevelEnum.Error);
                            return;
                        }
                        else
                            MetaStoreId = data.StoreIdDictionary[je.MetaStoreId];
                    }

                    StoreId ToStoreId;

                    if (je.ToStoreId == 0)
                        ToStoreId = new StoreId(this.TypeName, this.Identifier);
                    else
                    if (je.ToStoreId == -1)
                    {
                        ToStoreId = RootStore;
                        ToId = MinusZero.Instance.root.Identifier;
                    }
                    else
                    {
                        if (!data.StoreIdDictionary.ContainsKey(je.ToStoreId))
                        {
                            UserInteractionUtil.ShowException("Json Deserialisation from " + Identifier, "ToStoreId " + je.ToStoreId 
                                + " not found in StoreIdDictionary", ZeroTypes.ExceptionLevelEnum.Error);
                            return;
                        }
                        else
                            ToStoreId = data.StoreIdDictionary[je.ToStoreId];
                    }

                    EasyEdge e = new EasyEdge(MetaStoreId.TypeName, MetaStoreId.Identifier, MetaId,
                        ToStoreId.TypeName, ToStoreId.Identifier, ToId);

                    e._DetachState = DetachStateEnum.Detached;

                    e._from = v;

                    v.OutEdgesRaw.Add(e);
                }
            }

            VertexIdentifierCount = maxVertexIdentifierCount + 1;
        }

        public override void Refresh()
        {
            RefreshPre();

            Load();

            RefreshPost();
        }

        Dictionary<string, StoreId> storeOverride = null;

        public void SetStoreOverride(Dictionary<string, StoreId> _storeOverride)
        {
            storeOverride = _storeOverride;
        }

        public override void CommitTransaction()
        {
            string fileName = GetIdentifierToUse();

            if (MinusZero.Instance.CommandLineParameters == null || !MinusZero.Instance.CommandLineParameters.NoBackup)
                File.Copy(fileName, fileName + ".backup", true);

            CommitTransaction(GetIdentifierToUse(), true);            
        }

        public void CommitTransaction(string fileName, bool checkIfIsDetached)
        {
            if (!canWrite)
            {
                UserInteractionUtil.ShowException("Json Serialisation to " + fileName, "As json serialisation file " + fileName 
                    + " has not been properly loaded, commit (saving) is disabled for the file. This will protect existing file content."
                    , ZeroTypes.ExceptionLevelEnum.Warning);
                return;
            }

            if (ReadOnly)
                return;

            if (checkIfIsDetached && DetachState != DetachStateEnum.Detached)
                throw new Exception("Store not Detached");

            JsonSerializationData data = GetJsonSerializationData();

            if (storeOverride != null)
                foreach (StoreId storeId in data.StoreIdDictionary.Values)
                    foreach (KeyValuePair<string, StoreId> kvp in storeOverride)
                        if (kvp.Key == storeId.Identifier)
                        {
                            storeId.Identifier = kvp.Value.Identifier;
                            storeId.TypeName = kvp.Value.TypeName;
                        }

            // Use source generation for maximum performance
            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonContext.JsonSerializationData);
            File.WriteAllBytes(fileName, jsonBytes);

            base.CommitTransaction();
        }

        private JsonSerializationData GetJsonSerializationData()
        {
            JsonSerializationData data = new JsonSerializationData();

            data.Vertices = new List<JsonVertex>();

            data.StoreIdDictionary = new Dictionary<int, StoreId>();

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                JsonVertex jv = new JsonVertex();

                jv.Id = v.Identifier;

                jv.Value = v.Value;

                data.Vertices.Add(jv);

                List<JsonEdge> edges = new List<JsonEdge>();

                foreach (IEdge e in v.OutEdgesRaw)
                    if (e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;

                        JsonEdge je = new JsonEdge();

                        je.MetaStoreId = GetStoreId(data, de.MetaStoreTypeName, de.MetaStoreIdentifier, de.MetaIdentifier);

                        je.MetaId = de.MetaIdentifier;

                        je.ToStoreId = GetStoreId(data, de.ToStoreTypeName, de.ToStoreIdentifier, de.ToIdentifier);

                        je.ToId = de.ToIdentifier;

                        edges.Add(je);
                    }

                jv.Edges = edges.Count > 0 ? edges : null;
            }

            return data;
        }

        private int GetStoreId(JsonSerializationData data, string StoreTypeName, string StoreIdentifier, object vertexIdentifier)
        {
            if (StoreTypeName == this.TypeName && StoreIdentifier == this.Identifier)
                return 0;

            if (StoreIdentifier == "$-0$ROOT$STORE$" && vertexIdentifier is long && (long)vertexIdentifier == 0)
                return -1;

            foreach (KeyValuePair<int, StoreId> sid in data.StoreIdDictionary)
                if (sid.Value.TypeName == StoreTypeName && sid.Value.Identifier == StoreIdentifier)
                    return sid.Key;

            int key = data.StoreIdDictionary.Count + 1;

            data.StoreIdDictionary.Add(key, new StoreId(StoreTypeName, StoreIdentifier));

            return key;
        }

        public bool RefreshOnRollback { get; set; }

        public override void RollbackTransaction()
        {
            if (RefreshOnRollback)
                Refresh();

            base.RollbackTransaction();
        }

        public override void UpdateDetachStateData()
        {
            foreach (IVertex v in VertexIdentifiersDictionary.Values.ToList())
            {
                foreach (IEdge e in v.OutEdgesRaw)
                    if (e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;

                        de.UpdateDetachStateData();
                    }
            }
        }

        public override void Detach()
        {
            if (DetachState != DetachStateEnum.Attached)
                throw new Exception("Store not Attached");

            _DetachState = DetachStateEnum.Detaching;

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                foreach (IEdge e in v.OutEdgesRaw.ToList())
                    if (e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;

                        de.Detach();
                    }
            }

            _DetachState = DetachStateEnum.Detached;
        }

        public JsonSerializationStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            : base(identifier, storeUniverse, accessLeveList)
        {
            RootStore = new StoreId(MinusZero.Instance.root.Store.TypeName, MinusZero.Instance.root.Store.Identifier);

            RefreshOnRollback = false;

            Load();

            Attach();
        }

        public override void Backup()
        {
            if (MinusZero.Instance.CommandLineParameters != null && MinusZero.Instance.CommandLineParameters.NoBackup)
                return;

            if (DetachState == DetachStateEnum.Attached)
            {
                UpdateDetachStateData();

                string fileName = FileSystemUtil.GetFileName(Identifier);

                string extension = FileSystemUtil.GetExtension(Identifier);

                string pathPart = FileSystemUtil.GetPathPart(Identifier);

                string backupFileName = pathPart + fileName + "." + extension + ".backup";

                CommitTransaction(backupFileName, false);
            }
        }
    }
}