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

        public IVertex GetVertexByIdentifier(object VertexIdentifier)
        {
            if(!(VertexIdentifier is string))
            {
                UserInteractionUtil.ShowError("trying to create FileSystemStore vertex from identifier " + VertexIdentifier + " in the "+Identifier+" store", "identifier is not string");
                return null;
            }

            string fileName = (string)VertexIdentifier;

            if (System.IO.File.Exists(fileName))
                return new FileVertex(fileName, this);

            if (System.IO.Directory.Exists(fileName) || (fileName.Length==3 && fileName[1]==':' && fileName[2]=='\\'))
                return new DirectoryVertex(fileName, this);

            UserInteractionUtil.ShowError("trying to create FileSystemStore vertex from identifier " + fileName + "in the " + Identifier + " store", "file or directory not found");
            return null;
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

            IVertex mfsf = z.Root.Get(false, @"System\Meta\Store").AddVertex(null,"FileSystem");

            IVertex sm = z.Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(mfsf, sm, "{Class:Drive{Attribute:PathSeparator},Class:Directory{Aggregation:File{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Directory{$MinCardinality:0,$MaxCardinality:-1},Attribute:Filename,Attribute:Extension,Attribute:FullFilename,Attribute:FileAttribute,Attribute:CreationDateTime,Attribute:UpdateDateTime,Attribute:ReadDateTime},Class:File{Attribute:Filename,Attribute:Extension,Attribute:FullFilename,Attribute:Size,Attribute:FileAttribute,Attribute:CreationDateTime,Attribute:UpdateDateTime,Attribute:ReadDateTime}}");

            mfsf.Get(false, "Drive").AddEdge(sm.Get(false, @"Base\Vertex\$Inherits"), mfsf.Get(false, "Directory"));
            mfsf.Get(false, @"Drive\PathSeparator").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            mfsf.Get(false, @"Directory\Filename").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\Extension").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\FullFilename").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\FileAttribute").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\CreationDateTime").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\UpdateDateTime").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\ReadDateTime").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"Directory\File").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Store\FileSystem\File"));
            mfsf.Get(false, @"Directory\Directory").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Store\FileSystem\Directory"));

            mfsf.Get(false, @"File\Filename").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"File\Extension").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"File\FullFilename").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"File\Size").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            mfsf.Get(false, @"File\FileAttribute").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"File\CreationDateTime").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"File\UpdateDateTime").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            mfsf.Get(false, @"File\ReadDateTime").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));


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

           // if (rvv[rvv.Length - 1] == '\\') // problems with queries where vertex value has last character =="\\"
             //   _Root.Value = rvv.Substring(0, rvv.Length - 1);            
        }
    }
}
