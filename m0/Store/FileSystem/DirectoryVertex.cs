using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;
using m0.Util;

namespace m0.Store.FileSystem
{
    public class DirectoryVertex:EasyVertex
    {        
        DirectoryInfo DI;

        public override object Value { get; set; }
        
        /*{
            get
            {
                return DI.Name;
            }
            set
            {
                throw new NotImplementedException();
            }
        } */       

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

                IVertex fsm = MinusZero.Instance.Root.Get(@"System\Meta\Store\FileSystem");                

                AddMeta(fsm.Get("Filename"), DI.Name);
                AddMeta(fsm.Get("Extension"), DI.Extension);                
                AddMeta(fsm.Get("FullFilename"), DI.FullName);
                AddMeta(fsm.Get("FileAttribute"), DI.Attributes.ToString());
                AddMeta(fsm.Get("CreationDateTime"), DI.CreationTime.ToString());
                AddMeta(fsm.Get("UpdateDateTime"), DI.LastWriteTime.ToString());
                AddMeta(fsm.Get("ReadDateTime"), DI.LastAccessTime.ToString());
                

                IVertex FileMetaVertex=fsm.Get("File");

                IVertex DirectoryMetaVertex=fsm.Get("Directory");

                try{
                    foreach (FileSystemInfo fsi in DI.EnumerateFileSystemInfos())                
                    {
                    
                        if (fsi is DirectoryInfo)
                        {
                            IVertex DirectoryVertex = new DirectoryVertex(fsi.FullName, this.Store);

                            AddEdge(DirectoryMetaVertex, DirectoryVertex);
                        }

                        if (fsi is FileInfo)
                        {
                            IVertex FileVertex = new FileVertex(fsi.FullName, this.Store);

                            AddEdge(FileMetaVertex, FileVertex);
                        }
                   
                    }
                }catch (Exception e) { } // no access


                CanFireChangeEvent = true;                
                OutEdgesFilled = true;

                return OutEdgesRaw;
            }
        }

        public override IVertex AddVertex(IVertex metaVertex, object val)
        {
            if (val == null) return null;

            string name = val.ToString();

            if (this.Get(name) != null)
                return null;

            if (GeneralUtil.CompareStrings(metaVertex.Value,"Directory"))
            {                
                DI.CreateSubdirectory(name);

                IVertex DirectoryVertex = new DirectoryVertex(this.Identifier+"\\"+name, this.Store);

                AddEdge(metaVertex, DirectoryVertex);

                return DirectoryVertex;
            }

            if (GeneralUtil.CompareStrings(metaVertex.Value,"File"))
            {
                FileInfo fi = new FileInfo(this.Identifier + "\\" + name);

                fi.Create();                

                IVertex FileVertex = new FileVertex(this.Identifier + "\\" + name, this.Store);

                AddEdge(metaVertex, FileVertex);

                return FileVertex;
            }
            
            return null;
        }

        public override void DeleteEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value,"File"))
            {
                FileInfo fi= new FileInfo(Identifier + "\\" + edge.To.Value);

                fi.Delete();
            }

            if (GeneralUtil.CompareStrings(edge.Meta.Value, "Directory"))
            {
                DirectoryInfo di = new DirectoryInfo(Identifier + "\\" + edge.To.Value);

                di.Delete();
            }
            
            base.DeleteEdge(edge);
        }

        public DirectoryVertex(string identifier,IStore store)
            : base(store)
        {
            _Identifier = identifier;

            UsageCounter++; // identified vertex are used for volatile stores            

            DI = new DirectoryInfo(Identifier);

            Value = DI.Name;
        }
    }
}
