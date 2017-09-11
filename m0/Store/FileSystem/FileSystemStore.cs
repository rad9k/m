using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;

namespace m0.Store.FileSystem
{
    public class FileSystemStore:IStore
    {
        public bool IncludeFileContent { get; set; }

        protected IStoreUniverse _StoreUniverse;

        public virtual IStoreUniverse StoreUniverse
        {
            get { return _StoreUniverse; }
        }

        public virtual string TypeName
        {
            get { return GeneralUtil.GetTypeName(this); }
        }

        protected string _Identifier;

        public virtual string Identifier
        {
            get { return _Identifier; }
        }

        protected IVertex _Root;

        public virtual IVertex Root
        {
            get { return _Root; }
        }
        

        public void Detach()
        {
            //throw new NotImplementedException();
        }

        public void InDetach(IStore InDetachStore)
        {
           // throw new NotImplementedException();
        }

        public void Attach()
        {
            //throw new NotImplementedException();
        }

        public void Close()
        {
            //throw new NotImplementedException();
        }

        public DetachStateEnum DetachState
        {
            get { return DetachStateEnum.Attached; }
        }

        IList<AccessLevelEnum> _AcessLevel;

        public virtual IList<AccessLevelEnum> AccessLevel
        {
            get { return _AcessLevel; }
        }

        public void StoreVertexIdentifier(IVertex Vertex)
        {
            //throw new NotImplementedException();
        }

        public void RemoveVertexIdentifier(IVertex Vertex)
        {
            //throw new NotImplementedException();
        }

        public IVertex GetVertexByIdentifier(string VertexIdentidier)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            //throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            //throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            //throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            //throw new NotImplementedException();
        }

        public static void FillSystemMeta()
        {
            MinusZero z = MinusZero.Instance;

            IVertex mfs = z.Root.Get(@"System\Meta\Store").AddVertex(null,"FileSystem");

            mfs.AddVertex(null, "Drive");

            mfs.AddVertex(null, "File");

            mfs.AddVertex(null, "Directory");

            mfs.AddVertex(null, "Filename");

            mfs.AddVertex(null, "Extension");

            mfs.AddVertex(null, "FullFilename");

            mfs.AddVertex(null, "Size");

            mfs.AddVertex(null, "FileAttribute");

            mfs.AddVertex(null, "CreationDateTime");

            mfs.AddVertex(null, "UpdateDateTime");

            mfs.AddVertex(null, "ReadDateTime");

            mfs.AddVertex(null, "Content");
        }

        public FileSystemStore(string identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
        {
            _Identifier = identifier;

            _StoreUniverse = storeUniverse;

            _AcessLevel = GeneralUtil.CreateAndCopyList<AccessLevelEnum>(accessLeveList);
            
            storeUniverse.Stores.Add(this);

            _Root = new DirectoryVertex(identifier, this);

            String rvv = (String)_Root.Value;

            if (rvv[rvv.Length - 1] == '\\') // problems with queries where vertex value has last character =="\\"
                _Root.Value = rvv.Substring(0, rvv.Length - 1);            
        }
    }
}
