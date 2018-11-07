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
    public class JsonSerializationStore:StoreBase
    {
        
        
        void Load()
        {
            if (File.Exists(Identifier))
            {
                FileStream readStream = new FileStream(Identifier, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                
                VertexIdentifiersDictionary = (Dictionary<string, IVertex>)formatter.Deserialize(readStream);
                string RootIdentifier = (string)formatter.Deserialize(readStream);

                readStream.Close();

                _root = GetVertexByIdentifier(RootIdentifier);

                Attach();
            }
            else
            {
                _root = new EasyVertex(this);
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
            bool needToDetachAttach = false;

            if (DetachState != DetachStateEnum.Detached)
                needToDetachAttach = true;

            if (needToDetachAttach)
                Detach();

            FileStream writeStream = new FileStream(Identifier, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(writeStream, VertexIdentifiersDictionary);
            formatter.Serialize(writeStream, Root.Identifier);

            writeStream.Close();

            base.CommitTransaction();

            if (needToDetachAttach)
                Detach();
        }

        public bool RefreshOnRollback { get; set; }
        
        public override void RollbackTransaction()
        {
            if (RefreshOnRollback)
                Refresh();

            base.RollbackTransaction();
        }


        public JsonSerializationStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            : base(identifier, storeUniverse, accessLeveList)
        {
            RefreshOnRollback = false;

            Load();
        }
    }
}
