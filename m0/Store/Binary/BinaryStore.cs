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
using m0.Store.Json;
using static System.Net.WebRequestMethods;

namespace m0.Store.Binary
{
    public class BinaryStore : StoreBase
    {
        void NullStoreAndDictionariesDataInVertices()
        {
            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                VertexBase vb = (VertexBase)v;
                vb._Store = null;

                if (v is EasyVertex)
                {
                    EasyVertex ve = (EasyVertex)v;

                    ve.ClearDictionaries();
                }
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
            if (System.IO.File.Exists(Identifier))
            {
                FileStream readStream = new FileStream(Identifier, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                try
                {
                    VertexIdentifiersDictionary = (Dictionary<object, IVertex>)formatter.Deserialize(readStream);
                    string RootIdentifier = (string)formatter.Deserialize(readStream);

                    RestoreStoreDataInVertices();                    

                    root = GetVertexByIdentifier(RootIdentifier);

                    Attach();
                }
                catch (Exception e)
                {
                    root = new EasyVertex(this);
                    root.IsRoot = true;
                }

                readStream.Close();
            }
            else
            {
                root = new EasyVertex(this);
                root.IsRoot = true;
            }
        }

        public override void CommitTransaction()
        {
            CommitTransaction(Identifier);
        }

        public void CommitTransaction(string fileName)
        {
            if (DetachState != DetachStateEnum.Detached)
                throw new Exception("Store not Detached");

            FileStream writeStream = new FileStream(Identifier, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            NullStoreAndDictionariesDataInVertices();

            formatter.Serialize(writeStream, VertexIdentifiersDictionary);
            formatter.Serialize(writeStream, Root.Identifier);

            writeStream.Close();

            base.CommitTransaction();

            RestoreStoreDataInVertices();
        }

        public BinaryStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            : base(identifier, storeUniverse, accessLeveList)
        {
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


                CommitTransaction(backupFileName);
            }
        }
    }
}
