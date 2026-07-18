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

namespace m0.Store.FileSystem
{

    public class FileVertex : AbstractFileSystemVertex
    {             
        static string[] TextFileExensions = new string[] { ".txt", ".log", ".csv", ".xml", ".json", ".htm", ".html", ".js", ".md" };

        FileInfo FI;

        public JsonSerializationStore JsonStore;
        public TextStore TextStore;
        public BinaryStore BinaryStore;

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
                string oldIdentifier = FI.FullName;
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
                    () => FI.MoveTo(oldIdentifier));

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
            _Identifier = identifier;
        }

        public override void Refresh()
        {
            OutEdgesDictionariesNeedsRebuild = true;
            GraphUtil.RemoveAllEdges(FileSystemVertex);

            AddVertexToFileSystemVertex(MinusZero.Instance.Is, FileSystemStore.File);
            AddVertexToFileSystemVertex(FileSystemStore.File_Filename, FI.Name);            

            string extension = FI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);
            
            if (FI.Extension.Length > 1)
                AddVertexToFileSystemVertex(FileSystemStore.File_Extension, FI.Extension.Substring(1));

            AddVertexToFileSystemVertex(FileSystemStore.File_FullFilename, FI.FullName);

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
                AddEdge(FileSystemStore.File_Content, new FileContentVertex(FI.FullName, this.Store));

            string extension_lower = FI.Extension.ToLower();

            DeleteStoreEdges();

            if (extension_lower == ".m0j")
            {
                JsonStore = (JsonSerializationStore)Store.StoreUniverse.GetStore("m0.Store.Json.JsonSerializationStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Identifier.ToString());

                AddEdge(FileSystemStore.Store, JsonStore.Root);
            }

            if (extension_lower == ".m0t")
            {
                TextStore = (TextStore)Store.StoreUniverse.GetStore("m0.Store.Text.TextStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Identifier.ToString());

                AddEdge(FileSystemStore.Store, TextStore.Root);
            }

            if (extension_lower == ".m0x")
            {
                BinaryStore = (BinaryStore)Store.StoreUniverse.GetStore("m0.Store.Binary.BinaryStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Identifier.ToString());

                AddEdge(FileSystemStore.Store, BinaryStore.Root);
            }

            if (TextFileExensions.Contains(extension_lower))
            {
                //AddEdge(FileSystemStore.File_Content, new FileContentVertex(FI.FullName, this.Store));
                AddEdge(FileSystemStore.File_Content, new FileContentVertex(FI.FullName, MinusZero.Instance.TempStore));
            }
        }

        void DeleteStoreEdges()
        {
            foreach (IEdge e in OutEdgesRaw.ToList())
                if (e.Meta == FileSystemStore.Store)
                    DeleteEdge(e);
        }

        public FileVertex(IStore store, string identifier)
            : base(store, identifier)
        {
            FI = new FileInfo(Identifier.ToString());
        }
    }
}

