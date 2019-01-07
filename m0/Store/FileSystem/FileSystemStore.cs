using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;

namespace m0.Store.FileSystem
{
    public class FileSystemStore : IStore
    {
        public bool IncludeFileContent { get; set; }

        protected IStoreUniverse _StoreUniverse;

        public virtual IStoreUniverse StoreUniverse
        {
            get { return _StoreUniverse; }
        }

        public virtual long VertexIdentifierCount {get; set;}

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

        public IVertex GetVertexByIdentifier(object VertexIdentidier)
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

            IVertex mfsf = z.Root.Get(@"System\Meta\Store").AddVertex(null,"FileSystem");

            IVertex sm = z.Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(mfsf, sm, "{Class:Drive{Attribute:PathSeparator},Class:Directory{Aggregation:File{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Directory{$MinCardinality:0,$MaxCardinality:-1},Attribute:Filename,Attribute:Extension,Attribute:FullFilename,Attribute:FileAttribute,Attribute:CreationDateTime,Attribute:UpdateDateTime,Attribute:ReadDateTime},Class:File{Attribute:Filename,Attribute:Extension,Attribute:FullFilename,Attribute:Size,Attribute:FileAttribute,Attribute:CreationDateTime,Attribute:UpdateDateTime,Attribute:ReadDateTime}}");

            mfsf.Get("Drive").AddEdge(sm.Get(@"Base\Vertex\$Inherits"), mfsf.Get("Directory"));
            mfsf.Get(@"Drive\PathSeparator").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));

            mfsf.Get(@"Directory\Filename").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\Extension").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\FullFilename").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\FileAttribute").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\CreationDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\UpdateDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\ReadDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\UpdateDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\ReadDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"Directory\File").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Store\FileSystem\File"));
            mfsf.Get(@"Directory\Directory").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Store\FileSystem\Directory"));

            mfsf.Get(@"File\Filename").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"File\Extension").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"File\FullFilename").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"File\Size").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            mfsf.Get(@"File\FileAttribute").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"File\CreationDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"File\UpdateDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            mfsf.Get(@"File\ReadDateTime").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));


            mfsf.AddVertex(null, "$Store");
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
