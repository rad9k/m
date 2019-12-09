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
        static public Dictionary<string, IVertex> FileVertexDictionary = new Dictionary<string, IVertex>();
        static public Dictionary<string, IVertex> DirectoryVertexDictionary = new Dictionary<string, IVertex>();

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
            {
                if (FileVertexDictionary.ContainsKey(fileName))
                    return FileVertexDictionary[fileName];

                return new FileVertex(fileName, this);
            }

            if (System.IO.Directory.Exists(fileName) || (fileName.Length == 3 && fileName[1] == ':' && fileName[2] == '\\'))
            {
                if (DirectoryVertexDictionary.ContainsKey(fileName))
                    return DirectoryVertexDictionary[fileName];

                return new DirectoryVertex(fileName, this);
            }

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

        public static IVertex FileSystem;
        public static IVertex Store;
        public static IVertex Directory;
        public static IVertex Directory_Filename;
        public static IVertex Directory_Extension;
        public static IVertex Directory_FullFilename;
        public static IVertex Directory_FileAttribute;
        public static IVertex Directory_CreationDateTime;
        public static IVertex Directory_UpdateDateTime;
        public static IVertex Directory_ReadDateTime;
        public static IVertex Directory_File;
        public static IVertex Directory_Directory;
        public static IVertex File;
        public static IVertex File_Content;
        public static IVertex File_Filename;
        public static IVertex File_Extension;
        public static IVertex File_FullFilename;
        public static IVertex File_Size;
        public static IVertex File_FileAttribute;
        public static IVertex File_CreationDateTime;
        public static IVertex File_UpdateDateTime;
        public static IVertex File_ReadDateTime;

        public static void FillSystemMeta()
        {
            MinusZero z = MinusZero.Instance;

            IVertex mfsf = z.Root.Get(false, @"System\Meta\Store").AddVertex(null,"FileSystem");

            FileSystem = mfsf;

            IVertex sm = z.Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(mfsf, sm, "{Class:Drive{Attribute:PathSeparator},Class:Directory{Aggregation:File{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Directory{$MinCardinality:0,$MaxCardinality:-1},Attribute:Filename,Attribute:Extension,Attribute:FullFilename,Attribute:FileAttribute,Attribute:CreationDateTime,Attribute:UpdateDateTime,Attribute:ReadDateTime},Class:File{Attribute:Content,Attribute:Filename,Attribute:Extension,Attribute:FullFilename,Attribute:Size,Attribute:FileAttribute,Attribute:CreationDateTime,Attribute:UpdateDateTime,Attribute:ReadDateTime},$Store}");

            Store = FileSystem.Get(false, "$Store");

            mfsf.Get(false, "Drive").AddEdge(sm.Get(false, @"Base\Vertex\$Inherits"), mfsf.Get(false, "Directory"));
            mfsf.Get(false, @"Drive\PathSeparator").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory = mfsf.Get(false, @"Directory");

            Directory_Filename = mfsf.Get(false, @"Directory\Filename");
            Directory_Filename.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_Extension = mfsf.Get(false, @"Directory\Extension");
            Directory_Extension.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_FullFilename = mfsf.Get(false, @"Directory\FullFilename");
            Directory_FullFilename.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_FileAttribute = mfsf.Get(false, @"Directory\FileAttribute");
            Directory_FileAttribute.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_CreationDateTime = mfsf.Get(false, @"Directory\CreationDateTime");
            Directory_CreationDateTime.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_UpdateDateTime = mfsf.Get(false, @"Directory\UpdateDateTime");
            Directory_UpdateDateTime.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_ReadDateTime = mfsf.Get(false, @"Directory\ReadDateTime");
            Directory_ReadDateTime.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            Directory_File = mfsf.Get(false, @"Directory\File");
            Directory_File.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Store\FileSystem\File"));

            Directory_Directory = mfsf.Get(false, @"Directory\Directory");
            Directory_Directory.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Store\FileSystem\Directory"));

            File = mfsf.Get(false, @"File");

            File_Content = mfsf.Get(false, @"File\Content");
            File_Content.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Vertex"));

            File_Filename = mfsf.Get(false, @"File\Filename");
            File_Filename.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            File_Extension = mfsf.Get(false, @"File\Extension");
            File_Extension.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            File_FullFilename = mfsf.Get(false, @"File\FullFilename");
            File_FullFilename.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            File_Size = mfsf.Get(false, @"File\Size");
            File_Size.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));

            File_FileAttribute = mfsf.Get(false, @"File\FileAttribute");
            File_FileAttribute.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            File_CreationDateTime = mfsf.Get(false, @"File\CreationDateTime");
            File_CreationDateTime.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            File_UpdateDateTime = mfsf.Get(false, @"File\UpdateDateTime");
            File_UpdateDateTime.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            File_ReadDateTime = mfsf.Get(false, @"File\ReadDateTime");
            File_ReadDateTime.AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
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
