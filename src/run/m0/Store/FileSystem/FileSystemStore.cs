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
        static public Dictionary<string, IVertex> FileVertexDictionary;
        static public Dictionary<string, IVertex> DirectoryVertexDictionary;

        static FileSystemStore()
        {
            IEqualityComparer<string> fileSystemPathComparer =
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;

            FileVertexDictionary =
                new Dictionary<string, IVertex>(fileSystemPathComparer);
            DirectoryVertexDictionary =
                new Dictionary<string, IVertex>(fileSystemPathComparer);
        }

        private static readonly object
            vertexRegistrySynchronizationRoot =
                new object();

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

        public bool AlwaysPresent { get { return false; } }

        public void StoreVertexIdentifier(IVertex vertex)
        {
            if (vertex is FileVertex)
            {
                StoreFileSystemVertexIdentifier(
                    FileVertexDictionary,
                    DirectoryVertexDictionary,
                    vertex,
                    "file");
            }
            else if (vertex is DirectoryVertex)
            {
                StoreFileSystemVertexIdentifier(
                    DirectoryVertexDictionary,
                    FileVertexDictionary,
                    vertex,
                    "directory");
            }
        }

        public void RemoveVertexIdentifier(IVertex vertex)
        {
            if (vertex is FileVertex)
            {
                RemoveFileSystemVertexIdentifier(
                    FileVertexDictionary,
                    vertex);
            }
            else if (vertex is DirectoryVertex)
            {
                RemoveFileSystemVertexIdentifier(
                    DirectoryVertexDictionary,
                    vertex);
            }
        }

        public IVertex GetVertexByIdentifier(object VertexIdentifier)
        {
            if(!(VertexIdentifier is string))
            {
                UserInteractionUtil.ShowException("trying to create FileSystemStore vertex from identifier " + VertexIdentifier 
                    + " in the " + Identifier + " store", "identifier is not string", 
                    ZeroTypes.ExceptionLevelEnum.Error);
                return null;
            }

            string rawFileName = (string)VertexIdentifier;
            string fileName =
                FileSystemUtil.NormalizeFileSystemIdentifier(rawFileName);

            bool fileExists = System.IO.File.Exists(fileName);
            bool directoryExists = System.IO.Directory.Exists(fileName);
            bool looksLikeDriveRoot =
                FileSystemUtil.IsWindowsDriveRoot(fileName);

            lock (vertexRegistrySynchronizationRoot)
            {
                if (fileExists)
                {
                    if (FileVertexDictionary.TryGetValue(
                        fileName,
                        out IVertex fileVertex))
                        return fileVertex;

                    return new FileVertex(this, fileName);
                }

                if (directoryExists || looksLikeDriveRoot)
                {
                    if (DirectoryVertexDictionary.TryGetValue(
                        fileName,
                        out IVertex directoryVertex))
                        return directoryVertex;

                    return new DirectoryVertex(this, fileName);
                }
            }

            UserInteractionUtil.ShowException("trying to create FileSystemStore vertex from identifier " + fileName 
                + "in the " + Identifier + " store", "file or directory not found"
                , ZeroTypes.ExceptionLevelEnum.Error);
            return null;
        }

        internal void RenameFileVertex(
            FileVertex vertex,
            string oldIdentifier,
            string newIdentifier,
            Action moveToNewIdentifier,
            Action moveBackToOldIdentifier)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (moveToNewIdentifier == null)
                throw new ArgumentNullException(
                    nameof(moveToNewIdentifier));
            if (moveBackToOldIdentifier == null)
                throw new ArgumentNullException(
                    nameof(moveBackToOldIdentifier));
            if (!ReferenceEquals(vertex.Store, this))
                throw new InvalidOperationException(
                    "Cannot rename a vertex owned by another store.");

            oldIdentifier =
                FileSystemUtil.NormalizeFileSystemIdentifier(
                    oldIdentifier);
            newIdentifier =
                FileSystemUtil.NormalizeFileSystemIdentifier(
                    newIdentifier);

            lock (vertexRegistrySynchronizationRoot)
            {
                EnsureRegisteredVertex(
                    FileVertexDictionary,
                    oldIdentifier,
                    vertex);
                EnsureIdentifierAvailable(
                    newIdentifier,
                    vertex);

                bool externalMutationCompleted = false;
                try
                {
                    moveToNewIdentifier();
                    externalMutationCompleted = true;

                    FileVertexDictionary.Remove(oldIdentifier);
                    vertex.SetIdentifierAfterRename(
                        newIdentifier);
                    FileVertexDictionary.Add(
                        newIdentifier,
                        vertex);
                }
                catch (Exception renameException)
                {
                    if (!externalMutationCompleted)
                        throw;

                    try
                    {
                        moveBackToOldIdentifier();
                    }
                    catch (Exception rollbackException)
                    {
                        FileVertexDictionary.Remove(
                            oldIdentifier);
                        vertex.SetIdentifierAfterRename(
                            newIdentifier);
                        FileVertexDictionary[newIdentifier] =
                            vertex;

                        throw new AggregateException(
                            "File rename failed after moving the file, " +
                            "and restoring the original path also failed. " +
                            "The vertex remains registered at the new path.",
                            renameException,
                            rollbackException);
                    }

                    if (FileVertexDictionary.TryGetValue(
                        newIdentifier,
                        out IVertex newIdentifierVertex) &&
                        ReferenceEquals(
                            newIdentifierVertex,
                            vertex))
                    {
                        FileVertexDictionary.Remove(
                            newIdentifier);
                    }

                    vertex.SetIdentifierAfterRename(
                        oldIdentifier);
                    FileVertexDictionary[oldIdentifier] =
                        vertex;
                    throw;
                }
            }
        }

        private static void StoreFileSystemVertexIdentifier(
            Dictionary<string, IVertex> targetDictionary,
            Dictionary<string, IVertex> otherDictionary,
            IVertex vertex,
            string vertexKind)
        {
            if (!(vertex.Identifier is string identifier))
                throw new InvalidOperationException(
                    $"A file-system {vertexKind} vertex must have " +
                    "a string identifier.");

            lock (vertexRegistrySynchronizationRoot)
            {
                if (targetDictionary.TryGetValue(
                    identifier,
                    out IVertex existingVertex))
                {
                    if (!ReferenceEquals(existingVertex, vertex))
                        throw CreateIdentifierCollisionException(
                            identifier,
                            existingVertex,
                            vertex);

                    return;
                }

                if (otherDictionary.TryGetValue(
                    identifier,
                    out existingVertex))
                {
                    throw CreateIdentifierCollisionException(
                        identifier,
                        existingVertex,
                        vertex);
                }

                targetDictionary.Add(identifier, vertex);
            }
        }

        private static void RemoveFileSystemVertexIdentifier(
            Dictionary<string, IVertex> dictionary,
            IVertex vertex)
        {
            if (!(vertex.Identifier is string identifier))
                return;

            lock (vertexRegistrySynchronizationRoot)
            {
                if (!dictionary.TryGetValue(
                    identifier,
                    out IVertex existingVertex))
                {
                    return;
                }

                if (!ReferenceEquals(existingVertex, vertex))
                    throw CreateIdentifierCollisionException(
                        identifier,
                        existingVertex,
                        vertex);

                dictionary.Remove(identifier);
            }
        }

        private static void EnsureRegisteredVertex(
            Dictionary<string, IVertex> dictionary,
            string identifier,
            IVertex expectedVertex)
        {
            if (!dictionary.TryGetValue(
                identifier,
                out IVertex registeredVertex) ||
                !ReferenceEquals(
                    registeredVertex,
                    expectedVertex))
            {
                throw new InvalidOperationException(
                    $"File-system vertex '{identifier}' is not " +
                    "registered as the expected instance.");
            }
        }

        private static void EnsureIdentifierAvailable(
            string identifier,
            IVertex vertex)
        {
            if (FileVertexDictionary.TryGetValue(
                identifier,
                out IVertex existingVertex) &&
                !ReferenceEquals(existingVertex, vertex))
            {
                throw CreateIdentifierCollisionException(
                    identifier,
                    existingVertex,
                    vertex);
            }

            if (DirectoryVertexDictionary.TryGetValue(
                identifier,
                out existingVertex) &&
                !ReferenceEquals(existingVertex, vertex))
            {
                throw CreateIdentifierCollisionException(
                    identifier,
                    existingVertex,
                    vertex);
            }
        }

        private static InvalidOperationException
            CreateIdentifierCollisionException(
                string identifier,
                IVertex existingVertex,
                IVertex newVertex)
        {
            return new InvalidOperationException(
                $"File-system vertex identifier collision for " +
                $"'{identifier}'. Existing vertex type: " +
                $"'{existingVertex.GetType().FullName}', new vertex " +
                $"type: '{newVertex.GetType().FullName}'.");
        }

        public void Refresh()
        {
            //throw new NotImplementedException();
        }

        public void StartTransaction()
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
        public static IVertex Directory_Basename;
        public static IVertex Directory_FileAttribute;
        public static IVertex Directory_CreationDateTime;
        public static IVertex Directory_UpdateDateTime;
        public static IVertex Directory_ReadDateTime;
        public static IVertex Directory_File;
        public static IVertex Directory_Directory;
        public static IVertex File;
        public static IVertex File_Content;
        public static IVertex File_Filename;
        public static IVertex File_Basename;
        public static IVertex File_Extension;
        public static IVertex File_FullFilename;
        public static IVertex File_Size;
        public static IVertex File_FileAttribute;
        public static IVertex File_CreationDateTime;
        public static IVertex File_UpdateDateTime;
        public static IVertex File_ReadDateTime;

        static bool staticVariablesInitialisationMade = false;

        private void IntializeStaticVariables()
        {
            MinusZero z = MinusZero.Instance;

            FileSystem = z.Root.Get(false, @"System\Meta\Store\FileSystem");            

            IVertex sm = z.Root.Get(false, @"System\Meta");                       

            Directory = FileSystem.Get(false, "Directory");
            File = FileSystem.Get(false, "File");

            Store = FileSystem.Get(false, "$Store");
                        
            Directory_Filename = Directory.Get(false, "Filename");            
            Directory_Extension = Directory.Get(false, "Extension");           
            Directory_FullFilename = Directory.Get(false, "FullFilename");
            Directory_Basename = Directory.Get(false, "Basename");
            Directory_FileAttribute = Directory.Get(false, "FileAttribute");
            Directory_CreationDateTime = Directory.Get(false, "CreationDateTime");
            Directory_UpdateDateTime = Directory.Get(false, "UpdateDateTime");
            Directory_ReadDateTime = Directory.Get(false, "ReadDateTime");
            Directory_File = Directory.Get(false, "File");
            Directory_Directory = Directory.Get(false, "Directory");

            File_Content = File.Get(false, "Content");
            File_Filename = File.Get(false, "Filename");
            File_Basename = File.Get(false, "Basename");
            File_Extension = File.Get(false, "Extension");
            File_FullFilename = File.Get(false, "FullFilename");
            File_Size = File.Get(false, "Size");
            File_FileAttribute = File.Get(false, "FileAttribute");
            File_CreationDateTime = File.Get(false, "CreationDateTime");
            File_UpdateDateTime = File.Get(false, "UpdateDateTime");
            File_ReadDateTime = File.Get(false, "ReadDateTime");

            staticVariablesInitialisationMade = true;
        }

        public static void FillSystemMeta() // THIS IS NOT CALLED
            // called from m0_SYSTEM_GENERATE
        {
            /*MinusZero z = MinusZero.Instance;

            IVertex mfsf = z.Root.Get(false, @"System\Meta\Store").AddVertex(null,"FileSystem");

            FileSystem = mfsf;

            IVertex sm = z.Root.Get(false, @"System\Meta");

            IVertex Drive = FileSystem.AddVertex(sm.Get(false, @"ZeroUML\Class"), "Drive");
            IVertex PathSeparator = Drive.AddVertex(sm.Get(false, @"ZeroUML\Class\Attribute"), "PathSeparator");

            Directory = FileSystem.AddVertex(sm.Get(false, @"ZeroUML\Class"), "Directory");
            File = FileSystem.AddVertex(sm.Get(false, @"ZeroUML\Class"), "File");

            Store = FileSystem.AddVertex(null, "$Store");

            IVertex vEdgeTarget = sm.Get(false, @"Base\Vertex\$EdgeTarget");

            mfsf.Get(false, "Drive").AddEdge(sm.Get(false, @"Base\Vertex\$Inherits"), mfsf.Get(false, "Directory"));
            mfsf.Get(false, @"Drive\PathSeparator").AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            IVertex attribute = sm.Get(false, @"ZeroUML\Class\Attribute");
            IVertex aggregation = sm.Get(false, @"ZeroUML\Class\Aggregation");

            Directory_Filename = Directory.AddVertex(attribute, "Filename");
            Directory_Filename.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_Extension = Directory.AddVertex(attribute, "Extension");
            Directory_Extension.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_FullFilename = Directory.AddVertex(attribute, "FullFilename");
            Directory_FullFilename.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_FileAttribute = Directory.AddVertex(attribute, "FileAttribute");
            Directory_FileAttribute.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_CreationDateTime = Directory.AddVertex(attribute, "CreationDateTime");
            Directory_CreationDateTime.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_UpdateDateTime = Directory.AddVertex(attribute, "UpdateDateTime");
            Directory_UpdateDateTime.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_ReadDateTime = Directory.AddVertex(attribute, "ReadDateTime");
            Directory_ReadDateTime.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            Directory_File = Directory.AddVertex(aggregation, "File");
            Directory_File.AddEdge(vEdgeTarget, sm.Get(false, @"Store\FileSystem\File"));
            Directory_File.AddVertex(sm.Get(false, @"Base\Vertex\$MinCardinality"), "0");
            Directory_File.AddVertex(sm.Get(false, @"Base\Vertex\$MaxCardinality"), "-1");

            Directory_Directory = Directory.AddVertex(aggregation, "Directory");
            Directory_Directory.AddEdge(vEdgeTarget, sm.Get(false, @"Store\FileSystem\Directory"));
            Directory_Directory.AddVertex(sm.Get(false, @"Base\Vertex\$MinCardinality"), "0");
            Directory_Directory.AddVertex(sm.Get(false, @"Base\Vertex\$MaxCardinality"), "-1");

            File = mfsf.Get(false, @"File");

            File_Content = File.AddVertex(attribute, "Content");
            File_Content.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\Vertex"));

            File_Filename = File.AddVertex(attribute, "Filename");
            File_Filename.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            File_Extension = File.AddVertex(attribute, "Extension");
            File_Extension.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            File_FullFilename = File.AddVertex(attribute, "FullFilename");
            File_FullFilename.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            File_Size = File.AddVertex(attribute, "Size");
            File_Size.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\Integer"));

            File_FileAttribute = File.AddVertex(attribute, "FileAttribute");
            File_FileAttribute.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            File_CreationDateTime = File.AddVertex(attribute, "CreationDateTime");
            File_CreationDateTime.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            File_UpdateDateTime = File.AddVertex(attribute, "UpdateDateTime");
            File_UpdateDateTime.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));

            File_ReadDateTime = File.AddVertex(attribute, "ReadDateTime");
            File_ReadDateTime.AddEdge(vEdgeTarget, sm.Get(false, @"ZeroTypes\String"));*/
        }

        public object GetRootIdentifier()
        {
            throw new NotImplementedException();
        }

        public FileSystemStore(string identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
        {
            if (!staticVariablesInitialisationMade)
                IntializeStaticVariables();

            _Identifier =
                FileSystemUtil.NormalizeFileSystemIdentifier(identifier);

            _StoreUniverse = storeUniverse;

            _AcessLevel = GeneralUtil.CreateAndCopyList<AccessLevelEnum>(accessLeveList);
            
            storeUniverse.Stores.Add(this);

            _Root = new DirectoryVertex(this, _Identifier);

            _Root.IsRoot = true;

            String rvv = (String)_Root.Value;

           // if (rvv[rvv.Length - 1] == '\\') // problems with queries where vertex value has last character =="\\"
             //   _Root.Value = rvv.Substring(0, rvv.Length - 1);            
        }

        public void UpdateDetachStateData() { }

        public void Backup() { }
    }
}
