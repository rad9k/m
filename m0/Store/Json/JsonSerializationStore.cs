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

namespace m0.Store.Json
{
    public class JsonSerializationStore:StoreBase
    {
        
        
        void Load()
        {
            if (File.Exists(Identifier))
            {
                StreamReader readStream = new StreamReader(Identifier);

                JsonSerializationData data = JSON.Deserialize<JsonSerializationData>(readStream);

                readStream.Close();

                _root = GetVertexByIdentifier(0);                
            }
            else
            {
                EasyVertex __root = new EasyVertex(this);

                __root.UsageCounter++;

                _root = __root;
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

            data.StoreIdDictionary = new Dictionary<int, StoreId>();

            if (Root.Value != null)
            {
                if (Root.Value is string)
                    data.RootValueString = (string)Root.Value;
                else
                    data.RootValueDouble = Convert.ToDouble(Root.Value); // :)
            }

            data.Vertexes = new List<JsonVertex>();

            foreach(IVertex v in VertexIdentifiersDictionary.Values)
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
            foreach(KeyValuePair<int,StoreId> sid in data.StoreIdDictionary)
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
