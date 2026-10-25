using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;
using m0.Store.Json;
using m0.Store.Text;
using m0.Store.Binary;
using m0.ZeroTypes;
using m0.Util;
using m0.Graph.ExecutionFlow;
using System.Runtime.CompilerServices;

namespace m0.Store.FileSystem
{

    public class FileVertex : AbstractFileSystemVertex
    {             
        static string[] TextFileExensions = new string[] { ".txt", ".log", ".csv", ".xml", ".json", ".htm", ".html", ".js", ".md" };

        FileInfo FI;

        public JsonSerializationStore JsonStore;
        public TextStore TextStore;
        public BinaryStore BinaryStore;

        bool IsStoreBackedFile()
        {
            string extension = FI?.Extension?.ToLower();

            return extension == ".m0j" ||
                extension == ".m0t" ||
                extension == ".m0x";
        }

        string DescribeStoreDiagnosticVertex(IVertex vertex)
        {
            if (vertex == null)
                return "<null>";

            IStore store = vertex.Store;

            return
                "storeId='" + (store?.Identifier ?? "<null>") +
                "', vertexId='" + vertex.Identifier +
                "', value='" + (vertex.Value ?? "<null>") +
                "', objectId='" +
                RuntimeHelpers.GetHashCode(vertex) +
                "'";
        }

        void LogCurrentStoreEdges(string phase)
        {
            if (!IsStoreBackedFile())
                return;

            int count = 0;

            foreach (IEdge edge in OutEdgesRaw)
            {
                if (edge.Meta == null ||
                    !GeneralUtil.CompareStrings(
                        edge.Meta.Value,
                        "$Store"))
                    continue;

                count++;

                MinusZero.Instance.Log(
                    0,
                    "StoreEdgeTrace",
                    phase +
                    " file='" + Identifier +
                    "' edgeIndex='" + count +
                    "' from={" +
                    DescribeStoreDiagnosticVertex(edge.From) +
                    "} meta={" +
                    DescribeStoreDiagnosticVertex(edge.Meta) +
                    "} to={" +
                    DescribeStoreDiagnosticVertex(edge.To) +
                    "}");
            }

            MinusZero.Instance.Log(
                0,
                "StoreEdgeTrace",
                phase +
                " file='" + Identifier +
                "' storeEdgeCount='" + count + "'");
        }

        void AddStoreEdgeIfMissing(
            string storeTypeName,
            IVertex storeRoot)
        {
            foreach (IEdge edge in OutEdgesRaw)
            {
                if (edge.Meta == FileSystemStore.Store &&
                    ReferenceEquals(edge.To, storeRoot))
                {
                    MinusZero.Instance.Log(
                        0,
                        "StoreEdgeTrace",
                        "FileVertex.Refresh skipped duplicate " +
                        storeTypeName +
                        " store edge " +
                        "file='" + Identifier +
                        "' meta={" +
                        DescribeStoreDiagnosticVertex(
                            FileSystemStore.Store) +
                        "} to={" +
                        DescribeStoreDiagnosticVertex(storeRoot) +
                        "}");
                    return;
                }
            }

            MinusZero.Instance.Log(
                0,
                "StoreEdgeTrace",
                "FileVertex.Refresh adding " +
                storeTypeName +
                " store edge " +
                "file='" + Identifier +
                "' meta={" +
                DescribeStoreDiagnosticVertex(FileSystemStore.Store) +
                "} to={" +
                DescribeStoreDiagnosticVertex(storeRoot) +
                "}");

            AddEdge(FileSystemStore.Store, storeRoot);
        }

        public override object Value
        {
            get
            {               
                return FI.Name;
            }
            set
            {
                if (!(value is string valueString) ||
                    string.IsNullOrWhiteSpace(valueString))
                {
                    return;
                }

                object oldValue = Value;
                string oldIdentifier = Identifier.ToString();
                string oldPhysicalFileName = FI.FullName;
                string newFileName =
                    FileSystemUtil.GetFileNamePart(valueString);

                if (string.IsNullOrWhiteSpace(newFileName))
                    return;

                newFileName =
                    FI.DirectoryName +
                    Path.DirectorySeparatorChar +
                    newFileName.Trim();

                if (newFileName[newFileName.Length - 1] == '.')
                    newFileName = newFileName.Substring(
                        0,
                        newFileName.Length - 1);

                if (newFileName == oldIdentifier)
                    return;

                while (System.IO.File.Exists(newFileName) ||
                       System.IO.Directory.Exists(newFileName))
                {
                    newFileName =
                        FileSystemUtil.AddNew(newFileName);
                }

                string finalNewFileName = newFileName;
                ((FileSystemStore)Store).RenameFileVertex(
                    this,
                    oldIdentifier,
                    finalNewFileName,
                    () => FI.MoveTo(finalNewFileName),
                    () => FI.MoveTo(oldPhysicalFileName));

                _Value = FI.Name;
                ValueChanged();

                if (CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(
                        new GraphChangeTransactionAtom(
                            this,
                            AtomGraphChangeTypeEnum.ValueChange,
                            oldValue,
                            Value,
                            null));

                Refresh();
            }
        }                        

        internal void SetIdentifierAfterRename(
            string identifier)
        {
            _Identifier =
                FileSystemUtil.GetCanonicalFileIdentifier(identifier);
        }

        public override void Refresh()
        {
            LogCurrentStoreEdges("FileVertex.Refresh BEFORE");

            OutEdgesDictionariesNeedsRebuild = true;
            GraphUtil.RemoveAllEdges(FileSystemVertex);

            AddVertexToFileSystemVertex(MinusZero.Instance.Is, FileSystemStore.File);
            AddVertexToFileSystemVertex(FileSystemStore.File_Filename, FI.Name);            

            string extension = FI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);
            
            if (FI.Extension.Length > 1)
                AddVertexToFileSystemVertex(FileSystemStore.File_Extension, FI.Extension.Substring(1));

            AddVertexToFileSystemVertex(
                FileSystemStore.File_FullFilename,
                Identifier.ToString());

            if (FI.Name.Contains("."))
                AddVertexToFileSystemVertex(FileSystemStore.File_Basename, FI.Name.Substring(0, FI.Name.LastIndexOf(".")));
            else
                AddVertexToFileSystemVertex(FileSystemStore.File_Basename, FI.Name);
             
            AddVertexToFileSystemVertex(FileSystemStore.File_Size, FI.Length.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_FileAttribute, FI.Attributes.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_CreationDateTime, FI.CreationTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_UpdateDateTime, FI.LastWriteTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_ReadDateTime, FI.LastAccessTime.ToString());                        

            if (((FileSystemStore)this.Store).IncludeFileContent)
                AddEdge(
                    FileSystemStore.File_Content,
                    new FileContentVertex(
                        Identifier.ToString(),
                        this.Store));

            string extension_lower = FI.Extension.ToLower();

            DeleteStoreEdges();
            LogCurrentStoreEdges("FileVertex.Refresh AFTER DeleteStoreEdges");

            if (extension_lower == ".m0j")
            {
                JsonStore = (JsonSerializationStore)
                    FileSystemUtil.GetFileStore(
                        Store.StoreUniverse,
                        typeof(JsonSerializationStore)
                            .AssemblyQualifiedName,
                        Identifier.ToString());

                AddStoreEdgeIfMissing("JSON", JsonStore.Root);
            }

            if (extension_lower == ".m0t")
            {
                TextStore = (TextStore)
                    FileSystemUtil.GetFileStore(
                        Store.StoreUniverse,
                        typeof(TextStore).AssemblyQualifiedName,
                        Identifier.ToString());

                AddStoreEdgeIfMissing("text", TextStore.Root);
            }

            if (extension_lower == ".m0x")
            {
                BinaryStore = (BinaryStore)
                    FileSystemUtil.GetFileStore(
                        Store.StoreUniverse,
                        typeof(BinaryStore).AssemblyQualifiedName,
                        Identifier.ToString());

                AddStoreEdgeIfMissing("binary", BinaryStore.Root);
            }

            if (TextFileExensions.Contains(extension_lower))
            {
                //AddEdge(FileSystemStore.File_Content, new FileContentVertex(FI.FullName, this.Store));
                AddEdge(
                    FileSystemStore.File_Content,
                    new FileContentVertex(
                        Identifier.ToString(),
                        MinusZero.Instance.TempStore));
            }
        }

        void DeleteStoreEdges()
        {
            foreach (IEdge e in OutEdgesRaw.ToList())
                if (e.Meta == FileSystemStore.Store)
                    DeleteEdge(e);
        }

        public FileVertex(IStore store, string identifier)
            : base(
                store,
                FileSystemUtil.GetCanonicalFileIdentifier(identifier))
        {
            FI = new FileInfo(
                FileSystemUtil.GetPhysicalFileName(
                    Identifier.ToString()));
        }
    }
}

