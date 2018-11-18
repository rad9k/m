using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;

namespace m0.Store.FileSystem
{
    public class FileVertex : EasyVertex
    {        
        FileInfo FI;

        public override object Value
        {
            get
            {
                return FI.Name;
            }
            set
            {                
                //throw new NotImplementedException();
            }
        }        

        public override void AddInEdge(IEdge edge)
        {

        }

        public override void DeleteInEdge(IEdge edge)
        {

        }

        bool OutEdgesFilled = false;

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

                IVertex fsmf = MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem\File");

                AddMeta(fsmf.Get("Filename"), FI.Name);
                AddMeta(fsmf.Get("Extension"), FI.Extension);
                AddMeta(fsmf.Get("FullFilename"), FI.FullName);
                AddMeta(fsmf.Get("Size"), FI.Length.ToString());
                AddMeta(fsmf.Get("FileAttribute"), FI.Attributes.ToString());
                AddMeta(fsmf.Get("CreationDateTime"), FI.CreationTime.ToString());
                AddMeta(fsmf.Get("UpdateDateTime"), FI.LastWriteTime.ToString());
                AddMeta(fsmf.Get("ReadDateTime"), FI.LastAccessTime.ToString());

                if (((FileSystemStore)this.Store).IncludeFileContent)
                    AddEdge(fsmf.Get("Content"), new FileContentVertex(FI.FullName, this.Store));

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

