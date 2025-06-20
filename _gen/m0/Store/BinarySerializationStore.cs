#pragma warning disable SYSLIB0011

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;

namespace m0.Store
{
    public class BinarySerializationStore:StoreBase
    {
        void NullStoreDataInVertices()
        {
            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                VertexBase vb = (VertexBase)v;
                vb._Store = null;
            }
        }

        void RestoreStoreDataInVertices()
        {
            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                VertexBase vb = (VertexBase)v;
                vb._Store = this;
            }
        }
        
        void Load()
        {
            if (File.Exists(Identifier))
            {
                FileStream readStream = new FileStream(Identifier, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                
                VertexIdentifiersDictionary = (Dictionary<object, IVertex>)formatter.Deserialize(readStream);
                string RootIdentifier = (string)formatter.Deserialize(readStream);

                RestoreStoreDataInVertices();

                readStream.Close();

                root = GetVertexByIdentifier(RootIdentifier);

                Attach();
            }
            else
            {
                root = new EasyVertex(this);
                root.IsRoot = true;
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

            FileStream writeStream = new FileStream(Identifier, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            NullStoreDataInVertices();

            formatter.Serialize(writeStream, VertexIdentifiersDictionary);
            formatter.Serialize(writeStream, Root.Identifier);

            writeStream.Close();

            base.CommitTransaction();

            RestoreStoreDataInVertices();            
        }

        public bool RefreshOnRollback { get; set; }
        
        public override void RollbackTransaction()
        {
            if (RefreshOnRollback)
                Refresh();

            base.RollbackTransaction();
        }


        public BinarySerializationStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            : base(identifier, storeUniverse, accessLeveList)
        {
            RefreshOnRollback = false;

            Load();
        }
    }
}
