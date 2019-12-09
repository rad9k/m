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

namespace m0.Store.FileSystem
{
    public class FileVertex : EasyVertex
    {        
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
                if (value is string)
                {
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

                            updateOutEdges();
                        }

                        FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));
                    }
                }
            }
        }        

        public override void AddInEdge(IEdge edge)
        {

        }

        public override void DeleteInEdge(IEdge edge)
        {

        }

        public override IVertex AddVertex(IVertex metaVertex, object val)
        {
            UserInteractionUtil.ShowError("FileVertex.AddVertex", Identifier + " : can not create vertex here");

            return null;
        }

        bool OutEdgesFilled = false;
       
        void updateOutEdges()
        {            


            AddMeta(FileSystemStore.File_Filename, FI.Name);

            string extension = FI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);

            AddMeta(FileSystemStore.File_Extension, extension);

            AddMeta(FileSystemStore.File_FullFilename, FI.FullName);
            AddMeta(FileSystemStore.File_Size, FI.Length.ToString());
            AddMeta(FileSystemStore.File_FileAttribute, FI.Attributes.ToString());
            AddMeta(FileSystemStore.File_CreationDateTime, FI.CreationTime.ToString());
            AddMeta(FileSystemStore.File_UpdateDateTime, FI.LastWriteTime.ToString());
            AddMeta(FileSystemStore.File_ReadDateTime, FI.LastAccessTime.ToString());                        

            if (((FileSystemStore)this.Store).IncludeFileContent)
                AddEdge(FileSystemStore.File_Content, new FileContentVertex(FI.FullName, this.Store));

            if (FI.Extension == ".m0" || FI.Extension == ".M0")
            {
                JsonStore = new JsonSerializationStore((string)this.Identifier, MinusZero.Instance, new AccessLevelEnum[] { });
                AddEdge(FileSystemStore.Store, JsonStore.Root);
            }
        }

        void AddMeta(IVertex metaVertex, string value)
        {
            IVertex v = new EasyVertex(MinusZero.Instance.TempStore); // XXX

            v.Value = value;

            AddEdge(metaVertex, v);
        }

        public override IList<IEdge> OutEdges
        {
            get
            {
                if (OutEdgesFilled)
                    return OutEdgesRaw;

                CanFireChangeEvent = false;

                updateOutEdges();

                CanFireChangeEvent = true;
                OutEdgesFilled = true;

                return OutEdgesRaw;
            }
        }

        public FileVertex(string identifier, IStore store)
            : base(store)
        {
            _Identifier = identifier;

            UsageCounter++; // identified vertex are used for volatile stores

            FI = new FileInfo(Identifier.ToString());

            FileSystemStore.FileVertexDictionary.Add(identifier, this);
        }
    }
}

