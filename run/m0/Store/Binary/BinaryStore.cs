#pragma warning disable SYSLIB0011

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;

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

        public override void RemoveVertexIdentifier(IVertex Vertex)
        {
            VertexIdentifiersDictionary.Remove(Vertex.Identifier);
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
                    object RootIdentifier = formatter.Deserialize(readStream);
                    VertexIdentifierCount = (long)formatter.Deserialize(readStream);


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

        List<IEdge> temporaryRemovedEdges;

        void removeTemporaryRemovedEdges()
        {
            temporaryRemovedEdges = new List<IEdge>();

            temporaryRemoveEdges_recurent(Root);
        }

        void temporaryRemoveEdges_recurent(IVertex v)
        {
            if (v.Store != this)
                return;

            foreach (IEdge e in v.OutEdges.ToList())
            {
                if (e.Meta.Value.ToString() == "$GraphChangeTrigger")
                    removeListener(e);

                temporaryRemoveEdges_recurent(e.To);
            }
        }

        void removeListener(IEdge baseEdge)
        {
            IEdge GraphChangeTrigger_TreeViewItem = baseEdge;

            IEdge Listener_Listener = GraphUtil.GetQueryOutFirstEdge(baseEdge.To, "Listener", "Listener");

            IEdge DotNetDelegatePointer_delegate = null;

            if (Listener_Listener != null)
                DotNetDelegatePointer_delegate = GraphUtil.GetQueryOutFirstEdge(Listener_Listener.To, "DotNetDelegatePointer", null);

            temporaryRemoveEdge(GraphChangeTrigger_TreeViewItem);
            temporaryRemoveEdge(Listener_Listener);
            temporaryRemoveEdge(DotNetDelegatePointer_delegate);
        }

        void temporaryRemoveEdge(IEdge e)
        {
            if (e == null)
                return;

            temporaryRemovedEdges.Add(e);

            e.From.DeleteEdge(e);

            VertexIdentifiersDictionary.Remove(e.To.Identifier);
        }

        void addTemporaryRemovedEdges() 
        {
            if (temporaryRemovedEdges == null)
                return;

            foreach(IEdge e in temporaryRemovedEdges)
            {
                e.From.AddEdge(e.Meta, e.To);
                VertexIdentifiersDictionary.Add(e.To.Identifier, e.To);
            }
        }

        public override void Attach()
        {
            base.Attach();

            addTemporaryRemovedEdges();
        }

        public override void Detach()
        {
            removeTemporaryRemovedEdges();

            base.Detach();
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
            formatter.Serialize(writeStream, VertexIdentifierCount);

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


                string fileName = FileSystemUtil.GetFileName(Identifier);

                string extension = FileSystemUtil.GetExtension(Identifier);

                string pathPart = FileSystemUtil.GetPathPart(Identifier);

                string backupFileName = pathPart + fileName + "." + extension + ".backup";


                CommitTransaction(backupFileName);
            }
        }
    }
}
