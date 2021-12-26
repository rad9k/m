using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;
using m0.Store.Json;
using m0.ZeroTypes;
using m0.Util;
using m0.Graph.ExecutionFlow;

namespace m0.Store.FileSystem
{
    public class FileVertex : EasyVertex
    {     
        EasyVertex FileSystemVertex;

        bool FileSystemVertexFilled = false;

        FileInfo FI;

        public JsonSerializationStore JsonStore;

        public override object Value
        {
            get
            {               
                return FI.Name;
            }
            set
            {
                object oldValue;

                if (value is string)
                {
                    if (value== null || (string)value == "")
                        return;

                    oldValue = _Value;

                    string newFileName = FileSystemUtil.getFileNamePart((string)value);

                    if (newFileName == "")
                        return;

                    newFileName = FI.DirectoryName + "\\" + newFileName.Trim();

                    if (newFileName[newFileName.Length - 1] == '.')
                        newFileName = newFileName.Substring(0, newFileName.Length - 1);

                    if (newFileName != FI.FullName){
                        while (System.IO.File.Exists(newFileName) || System.IO.Directory.Exists(newFileName))
                            newFileName = FileSystemUtil.addNew(newFileName);
                        
                        FI.MoveTo(newFileName);

                        _Identifier = newFileName;                        

                        string extension = FileSystemUtil.getExtension(newFileName);
                        if (extension == "m0" || extension == "M0") // need this now
                        {
                            GraphUtil.RemoveAllEdges(this);

                            UpdateFileSystemVertex();
                        }

                        if (CanEmitGraphChangeEvents)
                            ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                                this,
                                AtomGraphChangeTypeEnum.ValueChange,
                                oldValue,
                                _Value,
                                null));
                    }
                    else
                    {
                        oldValue = _Value;

                        if (value == null)
                            return;

                        _Value = value;

                        ValueChanged();                        

                        if (CanEmitGraphChangeEvents)
                            ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                                this,
                                AtomGraphChangeTypeEnum.ValueChange,
                                oldValue,
                                _Value,
                                null));
                    }
                }
            }
        }                

        protected override IVertex CreateVertexInstance()
        {
            return MinusZero.Instance.CreateTempVertex();
        }

        void UpdateFileSystemVertex()
        {
            GraphUtil.RemoveAllEdges(FileSystemVertex);

            AddVertexToFileSystemVertex(FileSystemStore.File_Filename, FI.Name);

            string extension = FI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);

            AddVertexToFileSystemVertex(FileSystemStore.File_Extension, extension);

            AddVertexToFileSystemVertex(FileSystemStore.File_FullFilename, FI.FullName);
            AddVertexToFileSystemVertex(FileSystemStore.File_Size, FI.Length.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_FileAttribute, FI.Attributes.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_CreationDateTime, FI.CreationTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_UpdateDateTime, FI.LastWriteTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.File_ReadDateTime, FI.LastAccessTime.ToString());                        

            if (((FileSystemStore)this.Store).IncludeFileContent)
                AddEdge(FileSystemStore.File_Content, new FileContentVertex(FI.FullName, this.Store));

            if (FI.Extension == ".m0" || FI.Extension == ".M0")
            {
                JsonStore = new JsonSerializationStore(Identifier.ToString(), MinusZero.Instance, new AccessLevelEnum[] { });
                AddEdge(FileSystemStore.Store, JsonStore.Root);
            }
        }

        void AddVertexToFileSystemVertex(IVertex metaVertex, string value)
        {
            FileSystemVertex.AddVertex(metaVertex, value);
        }

        public override IList<IEdge> OutEdges
        {            
            get
            {
                if (!FileSystemVertexFilled)
                {
                    UpdateFileSystemVertex();
                    FileSystemVertexFilled = true;
                }

                if (OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    OutEdgesDictionariesRebuild_Edges();
                    return _OutEdges;
                }
                else
                    return _OutEdges;
            }
        }

        protected override void OutEdgesDictionariesRebuild_Edges()
        {
            if (HasInheritance && AllowInheritance)
            {
                List<IEdge> FullEdges = OutEdgesRaw.ToList();

                HashSet<IVertex> parents = GraphUtil.GetInheritParents_RawEnumerate(this);

                foreach (IVertex v in parents)
                    FullEdges.AddRange(v.OutEdgesRaw);

                _OutEdges = FullEdges;
            }
            else
                _OutEdges = OutEdgesRaw;

            List<IEdge> FileSystemExtendedOutEdges = new List<IEdge>();

            FileSystemExtendedOutEdges.AddRange(_OutEdges);
            FileSystemExtendedOutEdges.AddRange(FileSystemVertex.OutEdges);

            _OutEdges = FileSystemExtendedOutEdges;

            OutEdgesDictionariesNeedsRebuild_Edges = false;
        }

        public FileVertex(string identifier, IStore store)
            : base(store)
        {
            _Identifier = identifier;            

            FI = new FileInfo(Identifier.ToString());

            FileSystemStore.FileVertexDictionary.Add(identifier, this);

            FileSystemVertex = new EasyVertex(store);
        }
    }
}

