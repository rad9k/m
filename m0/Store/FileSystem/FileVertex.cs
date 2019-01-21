using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;
using m0.Store.Json;
using m0.ZeroTypes;

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

                        System.IO.File.Move(FI.FullName, newFileName);

                        FI = new FileInfo(newFileName);

                        refreshOutEdges();

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

        bool OutEdgesFilled = false;
       
        void refreshOutEdges()
        {            
            IVertex fsmf = MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem\File");

            GraphUtil.SetVertexValue(this, fsmf.Get("Filename"), FI.Name);

            string extension = FI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);

            GraphUtil.SetVertexValue(this, fsmf.Get("Extension"), extension);

            GraphUtil.SetVertexValue(this, fsmf.Get("FullFilename"), FI.FullName);
            GraphUtil.SetVertexValue(this, fsmf.Get("Size"), FI.Length.ToString());
            GraphUtil.SetVertexValue(this, fsmf.Get("FileAttribute"), FI.Attributes.ToString());
            GraphUtil.SetVertexValue(this, fsmf.Get("CreationDateTime"), FI.CreationTime.ToString());
            GraphUtil.SetVertexValue(this, fsmf.Get("UpdateDateTime"), FI.LastWriteTime.ToString());
            GraphUtil.SetVertexValue(this, fsmf.Get("ReadDateTime"), FI.LastAccessTime.ToString());

            if (((FileSystemStore)this.Store).IncludeFileContent)
                AddEdge(fsmf.Get("Content"), new FileContentVertex(FI.FullName, this.Store));

            if (FI.Extension == ".m0" || FI.Extension == ".M0")
            {
                JsonStore = new JsonSerializationStore((string)this.Identifier, MinusZero.Instance, new AccessLevelEnum[] { });
                AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem\$Store"), JsonStore.Root);
            }
        }

        void AddMeta(IVertex metaVertex, string value)
        {
            IVertex v = new EasyVertex(this.Store);

            v.Value = value;

            AddEdge(metaVertex, v);
        }

        public override IEnumerable<IEdge> OutEdges
        {
            get
            {
                if (OutEdgesFilled)
                    return OutEdgesRaw;

                    CanFireChangeEvent = false;

                IVertex fsm = MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem");

                IVertex fsmf = fsm.Get(@"File");

                AddMeta(fsmf.Get("Filename"), FI.Name);

                string extension = FI.Extension;

                if (extension.Length > 1)
                    extension = extension.Substring(1);

                AddMeta(fsmf.Get("Extension"), extension);

                AddMeta(fsmf.Get("FullFilename"), FI.FullName);
                AddMeta(fsmf.Get("Size"), FI.Length.ToString());
                AddMeta(fsmf.Get("FileAttribute"), FI.Attributes.ToString());
                AddMeta(fsmf.Get("CreationDateTime"), FI.CreationTime.ToString());
                AddMeta(fsmf.Get("UpdateDateTime"), FI.LastWriteTime.ToString());
                AddMeta(fsmf.Get("ReadDateTime"), FI.LastAccessTime.ToString());

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
        }
    }
}

