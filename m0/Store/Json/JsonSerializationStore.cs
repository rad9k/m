using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;
using Jil;
using m0.Util;
using m0.Store.FileSystem;

namespace m0.Store.Json
{
    public class JsonSerializationStore : StoreBase
    {
        bool canWrite = true;

        private StoreId RootStore;

        void Load()
        {
            if (File.Exists(Identifier))
            {            
                using (StreamReader readStream = new StreamReader(Identifier))
                {
                    //try
                    //{
                    if (readStream.EndOfStream)
                    { // create new sub graph
                        EasyVertex __root = new EasyVertex(this);

                        root = __root;

                        root.IsRoot = true;                        
                    }
                    else
                    { // load graph from store
                        JsonSerializationData data = JSON.Deserialize<JsonSerializationData>(readStream);

                        //readStream.Close();

                        ReconstructVerticesFromSerialisationData(data);


                        root = GetVertexByIdentifier(GetRootIdentifier());

                        root.IsRoot = true;
                        

                        Attach();
                    }
                    /*}catch(Exception e)
                    { // can not deserislize graph
                        UserInteractionUtil.ShowError("Json Deserlialisation from " + Identifier, e.ToString() + "\n\nAs json serialisation file " + Identifier +" has not been properly loaded, commit (saving) is disabled for the file. This will protect existing file content.");

                        canWrite = false;

                        EasyVertex __root = new EasyVertex(this);

                        __root.UsageCounter++;

                        _root = __root;
                    }*/
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

            foreach(KeyValuePair<object, IVertex> kvp in oldVertexIdentifiersDictionary)
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

                if (jv.IdString == null)
                {
                    toBeIdentifier = jv.IdLong;

                    if ((long)toBeIdentifier > maxVertexIdentifierCount)
                        maxVertexIdentifierCount = (long)toBeIdentifier;
                }
                else
                    toBeIdentifier = jv.IdString;

                EasyVertex v = new EasyVertex(this, toBeIdentifier);

                if (jv.ValueString != null)
                    v.Value = jv.ValueString;

                if (jv.ValueInt != null)
                    v.Value = jv.ValueInt;

                if (jv.ValueDouble != null)
                    v.Value = jv.ValueDouble;

                if (jv.ValueDecimal != null)
                    v.Value = jv.ValueDecimal;

                v._Store = this;

                foreach(JsonEdge je in jv.Edges)
                {
                    object MetaId;
                    object ToId;

                    if (je.MetaIdString == null)
                        MetaId = je.MetaIdLong;
                    else
                        MetaId = je.MetaIdString;

                    if (je.ToIdString == null)
                        ToId = je.ToIdLong;
                    else
                        ToId = je.ToIdString;

                    StoreId MetaStoreId = null;

                    if (je.MetaStoreId == 0)
                        MetaStoreId = new StoreId(this.TypeName, this.Identifier);
                    else
                    {
                        if (!data.StoreIdDictionary.ContainsKey(je.MetaStoreId))
                        {                            
                            UserInteractionUtil.ShowError("Json Deserialisation from " + Identifier, "MetaStoreId " + je.MetaStoreId + " not found in StoreIdDictionary");

                            return;
                        }
                        else
                            MetaStoreId = data.StoreIdDictionary[je.MetaStoreId];
                    }

                    StoreId ToStoreId;

                    if (je.ToStoreId == 0)
                        ToStoreId = new StoreId(this.TypeName, this.Identifier);
                    else
                    if(je.ToStoreId == -1)
                    {
                        ToStoreId = RootStore;
                        ToId = MinusZero.Instance.root.Identifier;
                    }
                    else
                    {
                        if (!data.StoreIdDictionary.ContainsKey(je.ToStoreId))
                        {                            
                            UserInteractionUtil.ShowError("Json Deserialisation from " + Identifier, "ToStoreId " + je.ToStoreId + " not found in StoreIdDictionary");

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

            //if (maxVertexIdentifierCount > 0)
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
            CommitTransaction(Identifier, true);
        }

        public void CommitTransaction(string fileName, bool checkIfIsDetached)
        {            
            if (!canWrite)
            {
                UserInteractionUtil.ShowError("Json Serlialisation to " + fileName, "As json serialisation file " + fileName + " has not been properly loaded, commit (saving) is disabled for the file. This will protect existing file content.");
                return;
            }

            if (checkIfIsDetached && DetachState != DetachStateEnum.Detached)
                throw new Exception("Store not Detached");

            StreamWriter writeStream = new StreamWriter(fileName);

            JsonSerializationData data = GetJsonSerializationData();

            if(storeOverride!=null)
                foreach (StoreId storeId in data.StoreIdDictionary.Values)
                    foreach(KeyValuePair<string, StoreId> kvp in storeOverride)
                        if(kvp.Key == storeId.Identifier)
                        {
                            storeId.Identifier = kvp.Value.Identifier;
                            storeId.TypeName = kvp.Value.TypeName;
                        }

            Options o = new Options(false, true, false, DateTimeFormat.MicrosoftStyleMillisecondsSinceUnixEpoch, false);                
            
            JSON.Serialize<JsonSerializationData>(data, writeStream, o);
            

            writeStream.Close();

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

                if (v.Identifier is string)
                    jv.IdString = (string)v.Identifier;
                else
                    jv.IdLong = (long)v.Identifier; // :)

                if (v.Value != null)
                {
                    if (v.Value is string)                    
                        jv.ValueString = (string)v.Value;                                            
                    //else
                    //  jv.ValueDouble = Convert.ToDouble(v.Value); // :)

                    if (v.Value is double)                    
                        jv.ValueDouble = (double)v.Value;                                            

                    if (v.Value is int)                   
                        jv.ValueInt = (int)v.Value;                                            

                    if (v.Value is decimal)                    
                        jv.ValueDecimal = (decimal)v.Value;

                    if (v.Value is bool)
                        if ((bool)v.Value)
                            jv.ValueString = "True";
                        else
                            jv.ValueString = "False";
                }

                data.Vertices.Add(jv);

                jv.Edges = new List<JsonEdge>();

                foreach(IEdge e in v.OutEdgesRaw)
                   if(e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;

                        JsonEdge je = new JsonEdge();

                        je.MetaStoreId = GetStoreId(data, de.MetaStoreTypeName, de.MetaStoreIdentifier, de.MetaIdentifier);

                        if (de.MetaIdentifier != null)
                        {
                            if (de.MetaIdentifier is string)
                                je.MetaIdString = (string)de.MetaIdentifier;
                            else
                                je.MetaIdLong = (long)de.MetaIdentifier; // :)
                        }

                        je.ToStoreId = GetStoreId(data, de.ToStoreTypeName, de.ToStoreIdentifier, de.ToIdentifier);

                        if (de.ToIdentifier is string)
                            je.ToIdString = (string)de.ToIdentifier;
                        else
                            je.ToIdLong = (long)de.ToIdentifier; // :)                        

                        jv.Edges.Add(je);
                    }
            }

            return data;
        }

        private int GetStoreId(JsonSerializationData data, string StoreTypeName, string StoreIdentifier, object vertexIdentifier)
        {
            if (StoreTypeName == this.TypeName && StoreIdentifier == this.Identifier)
                return 0;

            if (StoreIdentifier == "$-0$ROOT$STORE$" && vertexIdentifier is long && (long)vertexIdentifier == 0)
                return -1;

            foreach (KeyValuePair<int,StoreId> sid in data.StoreIdDictionary)
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
                //foreach (IEdge e in v.OutEdges)
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
            if (DetachState == DetachStateEnum.Attached)
            {
                UpdateDetachStateData();


                string fileName = FileSystemUtil.getFileName(Identifier);

                string extension = FileSystemUtil.getExtension(Identifier);

                string pathPart = FileSystemUtil.getPathPart(Identifier);

                string backupFileName = pathPart + fileName + "." + extension + ".backup";


                CommitTransaction(backupFileName, false);
            }
        }
    }
}
