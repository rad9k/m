using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;
using m0.Util;
using ProtoBuf;

namespace m0.Store.Binary
{
    [ProtoContract]
    public class ProtoSerializationData
    {
        [ProtoMember(1)]
        public List<ProtoStoreIdEntry> StoreIdEntries { get; set; } = new();

        [ProtoMember(2)]
        public List<ProtoVertex> Vertices { get; set; } = new();
    }

    [ProtoContract]
    public class ProtoStoreIdEntry
    {
        [ProtoMember(1)]
        public int Key { get; set; }

        [ProtoMember(2)]
        public string TypeName { get; set; }

        [ProtoMember(3)]
        public string Identifier { get; set; }
    }

    [ProtoContract]
    public class ProtoVertex
    {
        // 0 = long (default), 1 = string
        [ProtoMember(1)]
        public int IdType { get; set; }

        [ProtoMember(2)]
        public string IdString { get; set; }

        [ProtoMember(3)]
        public long IdLong { get; set; }

        // 0 = null (default), 1 = string, 2 = int, 3 = double, 4 = decimal
        [ProtoMember(4)]
        public int ValueType { get; set; }

        [ProtoMember(5)]
        public string ValueString { get; set; }

        [ProtoMember(6)]
        public int ValueInt { get; set; }

        [ProtoMember(7)]
        public double ValueDouble { get; set; }

        [ProtoMember(8)]
        public string ValueDecimalString { get; set; }

        [ProtoMember(9)]
        public List<ProtoEdge> Edges { get; set; } = new();
    }

    [ProtoContract]
    public class ProtoEdge
    {
        [ProtoMember(1)]
        public int MetaStoreId { get; set; }

        // 0 = null (default), 1 = long, 2 = string
        [ProtoMember(2)]
        public int MetaIdType { get; set; }

        [ProtoMember(3)]
        public string MetaIdString { get; set; }

        [ProtoMember(4)]
        public long MetaIdLong { get; set; }

        [ProtoMember(5)]
        public int ToStoreId { get; set; }

        // 0 = long (default), 1 = string
        [ProtoMember(6)]
        public int ToIdType { get; set; }

        [ProtoMember(7)]
        public string ToIdString { get; set; }

        [ProtoMember(8)]
        public long ToIdLong { get; set; }
    }

    public class BinaryStore : StoreBase
    {
        bool canWrite = true;

        private StoreId RootStore;

        string GetIdentifierToUse()
        {
            if (Identifier.Contains(System.IO.Path.DirectorySeparatorChar.ToString()))
                return Identifier;
            else
                return MinusZero.Instance.ApplicationPath + System.IO.Path.DirectorySeparatorChar + Identifier;
        }

        void Load()
        {
            string identifierToUse = GetIdentifierToUse();

            if (File.Exists(identifierToUse))
            {
                try
                {
                    using FileStream readStream = new FileStream(identifierToUse, FileMode.Open);

                    if (readStream.Length == 0)
                    {
                        EasyVertex __root = new EasyVertex(this);
                        root = __root;
                        root.IsRoot = true;
                    }
                    else
                    {
                        ProtoSerializationData data = Serializer.Deserialize<ProtoSerializationData>(readStream);

                        ReconstructVerticesFromSerializationData(data);

                        root = GetVertexByIdentifier(GetRootIdentifier());
                        root.IsRoot = true;

                        Attach();
                    }
                }
                catch (Exception ex)
                {
                    canWrite = false;
                    UserInteractionUtil.ShowException("Binary (Protobuf) Deserialization from " + Identifier,
                        "Failed to load file: " + ex.Message, ZeroTypes.ExceptionLevelEnum.Error);

                    EasyVertex __root = new EasyVertex(this);
                    root = __root;
                    root.IsRoot = true;
                }
            }
            else
            {
                EasyVertex __root = new EasyVertex(this);
                root = __root;
                root.IsRoot = true;
            }
        }

        private void ReconstructVerticesFromSerializationData(ProtoSerializationData data)
        {
            Dictionary<int, StoreId> storeIdDict = new Dictionary<int, StoreId>();

            foreach (ProtoStoreIdEntry entry in data.StoreIdEntries)
                storeIdDict[entry.Key] = new StoreId(entry.TypeName, entry.Identifier);

            long maxVertexIdentifierCount = 0;

            foreach (ProtoVertex pv in data.Vertices)
            {
                object toBeIdentifier;

                if (pv.IdType == 1)
                    toBeIdentifier = pv.IdString;
                else
                {
                    toBeIdentifier = pv.IdLong;

                    if (pv.IdLong > maxVertexIdentifierCount)
                        maxVertexIdentifierCount = pv.IdLong;
                }

                EasyVertex v = new EasyVertex(this, toBeIdentifier);

                switch (pv.ValueType)
                {
                    case 1: v.Value = pv.ValueString; break;
                    case 2: v.Value = pv.ValueInt; break;
                    case 3: v.Value = pv.ValueDouble; break;
                    case 4: v.Value = decimal.Parse(pv.ValueDecimalString); break;
                }

                v._Store = this;

                foreach (ProtoEdge pe in pv.Edges)
                {
                    object MetaId = pe.MetaIdType switch
                    {
                        0 => null,
                        1 => (object)pe.MetaIdLong,
                        _ => pe.MetaIdString
                    };

                    object ToId = pe.ToIdType == 1 ? (object)pe.ToIdString : pe.ToIdLong;

                    StoreId MetaStoreId;

                    if (pe.MetaStoreId == 0)
                        MetaStoreId = new StoreId(this.TypeName, this.Identifier);
                    else
                    {
                        if (!storeIdDict.ContainsKey(pe.MetaStoreId))
                        {
                            UserInteractionUtil.ShowException("Binary Deserialization from " + Identifier,
                                "MetaStoreId " + pe.MetaStoreId + " not found in StoreIdDictionary",
                                ZeroTypes.ExceptionLevelEnum.Error);
                            return;
                        }

                        MetaStoreId = storeIdDict[pe.MetaStoreId];
                    }

                    StoreId ToStoreId;

                    if (pe.ToStoreId == 0)
                        ToStoreId = new StoreId(this.TypeName, this.Identifier);
                    else if (pe.ToStoreId == -1)
                    {
                        ToStoreId = RootStore;
                        ToId = MinusZero.Instance.root.Identifier;
                    }
                    else
                    {
                        if (!storeIdDict.ContainsKey(pe.ToStoreId))
                        {
                            UserInteractionUtil.ShowException("Binary Deserialization from " + Identifier,
                                "ToStoreId " + pe.ToStoreId + " not found in StoreIdDictionary",
                                ZeroTypes.ExceptionLevelEnum.Error);
                            return;
                        }

                        ToStoreId = storeIdDict[pe.ToStoreId];
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

        public override void CommitTransaction()
        {
            string fileName = GetIdentifierToUse();

            if (MinusZero.Instance.CommandLineParameters == null || !MinusZero.Instance.CommandLineParameters.NoBackup)
                File.Copy(fileName, fileName + ".backup", true);

            CommitTransaction(fileName, true);            
        }

        public void CommitTransaction(string fileName, bool checkIfIsDetached)
        {
            if (!canWrite)
            {
                UserInteractionUtil.ShowException("Binary Serialization to " + fileName,
                    "As binary serialization file " + fileName
                    + " has not been properly loaded, commit (saving) is disabled for the file. This will protect existing file content.",
                    ZeroTypes.ExceptionLevelEnum.Warning);
                return;
            }

            if (ReadOnly)
                return;

            if (checkIfIsDetached && DetachState != DetachStateEnum.Detached)
                throw new Exception("Store not Detached");

            ProtoSerializationData data = GetProtoSerializationData();

            using FileStream writeStream = new FileStream(fileName, FileMode.Create);
            Serializer.Serialize(writeStream, data);

            base.CommitTransaction();
        }

        private ProtoSerializationData GetProtoSerializationData()
        {
            ProtoSerializationData data = new ProtoSerializationData();
            data.Vertices = new List<ProtoVertex>();
            data.StoreIdEntries = new List<ProtoStoreIdEntry>();

            Dictionary<int, StoreId> storeIdDict = new Dictionary<int, StoreId>();

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                ProtoVertex pv = new ProtoVertex();

                if (v.Identifier is string idStr)
                {
                    pv.IdType = 1;
                    pv.IdString = idStr;
                }
                else
                {
                    pv.IdType = 0;
                    pv.IdLong = (long)v.Identifier;
                }

                if (v.Value != null)
                {
                    if (v.Value is string s)
                    { pv.ValueType = 1; pv.ValueString = s; }
                    else if (v.Value is double d)
                    { pv.ValueType = 3; pv.ValueDouble = d; }
                    else if (v.Value is int i)
                    { pv.ValueType = 2; pv.ValueInt = i; }
                    else if (v.Value is decimal dec)
                    { pv.ValueType = 4; pv.ValueDecimalString = dec.ToString(); }
                    else if (v.Value is bool b)
                    { pv.ValueType = 1; pv.ValueString = b ? "True" : "False"; }
                }

                data.Vertices.Add(pv);

                pv.Edges = new List<ProtoEdge>();

                foreach (IEdge e in v.OutEdgesRaw)
                    if (e is IDetachableEdge de)
                    {
                        ProtoEdge pe = new ProtoEdge();

                        pe.MetaStoreId = GetStoreId(storeIdDict, de.MetaStoreTypeName, de.MetaStoreIdentifier, de.MetaIdentifier);

                        if (de.MetaIdentifier == null)
                            pe.MetaIdType = 0;
                        else if (de.MetaIdentifier is string ms)
                        { pe.MetaIdType = 2; pe.MetaIdString = ms; }
                        else
                        { pe.MetaIdType = 1; pe.MetaIdLong = (long)de.MetaIdentifier; }

                        pe.ToStoreId = GetStoreId(storeIdDict, de.ToStoreTypeName, de.ToStoreIdentifier, de.ToIdentifier);

                        if (de.ToIdentifier is string ts)
                        { pe.ToIdType = 1; pe.ToIdString = ts; }
                        else
                        { pe.ToIdType = 0; pe.ToIdLong = (long)de.ToIdentifier; }

                        pv.Edges.Add(pe);
                    }
            }

            foreach (KeyValuePair<int, StoreId> kvp in storeIdDict)
                data.StoreIdEntries.Add(new ProtoStoreIdEntry
                {
                    Key = kvp.Key,
                    TypeName = kvp.Value.TypeName,
                    Identifier = kvp.Value.Identifier
                });

            return data;
        }

        private int GetStoreId(Dictionary<int, StoreId> storeIdDict, string StoreTypeName, string StoreIdentifier, object vertexIdentifier)
        {
            if (StoreTypeName == this.TypeName && StoreIdentifier == this.Identifier)
                return 0;

            if (StoreIdentifier == "$-0$ROOT$STORE$" && vertexIdentifier is long && (long)vertexIdentifier == 0)
                return -1;

            foreach (KeyValuePair<int, StoreId> sid in storeIdDict)
                if (sid.Value.TypeName == StoreTypeName && sid.Value.Identifier == StoreIdentifier)
                    return sid.Key;

            int key = storeIdDict.Count + 1;
            storeIdDict.Add(key, new StoreId(StoreTypeName, StoreIdentifier));
            return key;
        }

        public override void UpdateDetachStateData()
        {
            foreach (IVertex v in VertexIdentifiersDictionary.Values.ToList())
                foreach (IEdge e in v.OutEdgesRaw)
                    if (e is IDetachableEdge de)
                        de.UpdateDetachStateData();
        }

        public override void Detach()
        {
            if (DetachState != DetachStateEnum.Attached)
                throw new Exception("Store not Attached");

            _DetachState = DetachStateEnum.Detaching;

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
                foreach (IEdge e in v.OutEdgesRaw.ToList())
                    if (e is IDetachableEdge de)
                        de.Detach();

            _DetachState = DetachStateEnum.Detached;
        }

        public BinaryStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            : base(identifier, storeUniverse, accessLeveList)
        {
            RootStore = new StoreId(MinusZero.Instance.root.Store.TypeName, MinusZero.Instance.root.Store.Identifier);

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
