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

namespace m0.Store.Json
{
    public class JsonSerializationStore:StoreBase
    {
        bool canWrite = true;

        void Load()
        {
            if (File.Exists(Identifier))
            {
                try
                {
                    StreamReader readStream = new StreamReader(Identifier);

                    JsonSerializationData data = JSON.Deserialize<JsonSerializationData>(readStream);

                    readStream.Close();

                    ReconstructVertexesFromSerialisationData(data);

                    _root = GetVertexByIdentifier((long)0);

                    ((EasyVertex)_root).UsageCounter = 1;

                    Attach();
                }catch(Exception e)
                {
                    UserInteractionUtil.ShowError("Json Deserlialisation from " + Identifier, e.ToString() + "\n\nSAVING IS DISABLED FOR THE "+Identifier+" FILE. THIS WILL PROTECT THE FILE CONTENT");

                    canWrite = false;

                    EasyVertex __root = new EasyVertex(this);

                    __root.UsageCounter++;

                    _root = __root;
                }
            }
            else
            {
                EasyVertex __root = new EasyVertex(this);

                __root.UsageCounter++;

                _root = __root;
            }
            
        }

        private void ReconstructVertexesFromSerialisationData(JsonSerializationData data)
        {
            foreach(JsonVertex jv in data.Vertexes)
            {
                EasyVertex v = new EasyVertex(this);

                if (jv.IdString == null)
                    v._Identifier = jv.IdLong;
                else
                    v._Identifier = jv.IdString;

                if (jv.ValueString == null)
                    v.Value = jv.ValueDouble;
                else
                    v.Value = jv.ValueString;

                v._Store = this;

                VertexIdentifiersDictionary.Add(v.Identifier, v);

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

                    e._From = v;

                    v.OutEdgesRaw.Add(e);
                }
            }
        }

        public override void Refresh()
        {
            RefreshPre();

            Load();

            RefreshPost();
        }

        public override void CommitTransaction()
        {
            if (!canWrite)
            {
                UserInteractionUtil.ShowError("Json Serlialisation to " + Identifier, "\n\nSAVING IS DISABLED FOR THE " + Identifier + " FILE. THIS WILL PROTECT THE FILE CONTENT");
                return;
            }

            if (DetachState != DetachStateEnum.Detached)
                throw new Exception("Store not Detached");

            StreamWriter writeStream = new StreamWriter(Identifier);

            JsonSerializationData data = GetJsonSerializationData();

            JSON.Serialize<JsonSerializationData>(data, writeStream);
            

            writeStream.Close();

            base.CommitTransaction();
        }

        private JsonSerializationData GetJsonSerializationData()
        {
            JsonSerializationData data = new JsonSerializationData();

            data.Vertexes = new List<JsonVertex>();

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
                    else
                        jv.ValueDouble = Convert.ToDouble(v.Value); // :)
                }

                data.Vertexes.Add(jv);

                jv.Edges = new List<JsonEdge>();

                foreach(IEdge e in v.OutEdgesRaw)
                   if(e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;

                        JsonEdge je = new JsonEdge();

                        je.MetaStoreId = GetStoreId(data, de.MetaStoreTypeName, de.MetaStoreIdentifier);

                        if (de.MetaIdentifier != null)
                        {
                            if (de.MetaIdentifier is string)
                                je.MetaIdString = (string)de.MetaIdentifier;
                            else
                                je.MetaIdLong = (long)de.MetaIdentifier; // :)
                        }

                        je.ToStoreId = GetStoreId(data, de.ToStoreTypeName, de.ToStoreIdentifier);

                        if (de.ToIdentifier is string)
                            je.ToIdString = (string)de.ToIdentifier;
                        else
                            je.ToIdLong = (long)de.ToIdentifier; // :)


                        jv.Edges.Add(je);
                    }
            }

            return data;
        }

        private int GetStoreId(JsonSerializationData data, string StoreTypeName, string StoreIdentifier)
        {
            if (StoreTypeName == this.TypeName && StoreIdentifier == this.Identifier)
                return 0;

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

        public override void Detach()
        {
            if (DetachState != DetachStateEnum.Attached)
                throw new Exception("Store not Attached");

            _DetachState = DetachStateEnum.Detaching;

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                //foreach (IEdge e in v.OutEdges)
                foreach (IEdge e in v.OutEdgesRaw)
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
            RefreshOnRollback = false;

            Load();

            Attach();
        }
    }
}
