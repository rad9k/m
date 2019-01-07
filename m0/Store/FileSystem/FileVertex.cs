using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;
using m0.Store.Json;

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
                    string newFileName = (string)value;

                    //newFileName = FI.

                    //System.IO.File.Exists()

                    if ((string)value != FI.Name)
                    {
                      //  System.IO.File.Move(FI.FullName,)
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

        void AddNewVertexByMeta(IVertex metaVertex, string value)
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

                IVertex fsmf = MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem\File");

                AddNewVertexByMeta(fsmf.Get("Filename"), FI.Name); 

                string extension = FI.Extension;

                if (extension.Length > 1)
                    extension = extension.Substring(1);

                AddNewVertexByMeta(fsmf.Get("Extension"), extension);

                AddNewVertexByMeta(fsmf.Get("FullFilename"), FI.FullName);
                AddNewVertexByMeta(fsmf.Get("Size"), FI.Length.ToString());
                AddNewVertexByMeta(fsmf.Get("FileAttribute"), FI.Attributes.ToString());
                AddNewVertexByMeta(fsmf.Get("CreationDateTime"), FI.CreationTime.ToString());
                AddNewVertexByMeta(fsmf.Get("UpdateDateTime"), FI.LastWriteTime.ToString());
                AddNewVertexByMeta(fsmf.Get("ReadDateTime"), FI.LastAccessTime.ToString());

                if (((FileSystemStore)this.Store).IncludeFileContent)
                    AddEdge(fsmf.Get("Content"), new FileContentVertex(FI.FullName, this.Store));

                if (FI.Extension == ".m0" || FI.Extension == ".M0") {
                    JsonStore = new JsonSerializationStore((string)this.Identifier, MinusZero.Instance, new AccessLevelEnum[] { });
                    AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem\$Store"), JsonStore.Root);
                }


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

