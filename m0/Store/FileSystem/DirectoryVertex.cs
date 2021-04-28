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

        public override object Value         
        {
            get
            {
                if (((string)Identifier).Length == 3 && ((string)Identifier)[1] == ':' && ((string)Identifier)[2] == '\\')
                    return ((string)Identifier)[0].ToString();

                return FileSystemUtil.getFileNamePart((string)Identifier);
            }
            set
            {
                if (value is string)
                {
                    string newFileName = FileSystemUtil.getFileNamePart((string)value);

                    if (newFileName == "")
                        return;
                    
                    string DI_DirectoryName = DI.FullName.Substring(0, DI.FullName.LastIndexOf('\\'));

                    newFileName = DI_DirectoryName + "\\" + newFileName.Trim();

                    if (newFileName[newFileName.Length - 1] == '.')
                        newFileName = newFileName.Substring(0, newFileName.Length - 1);

                    if (newFileName != DI.FullName)
                    {
                        while (System.IO.Directory.Exists(newFileName) || System.IO.Directory.Exists(newFileName))
                            newFileName = FileSystemUtil.addNew(newFileName);

                        _Identifier = newFileName;

                        System.IO.Directory.Move(DI.FullName, newFileName);

                        DI = new DirectoryInfo(newFileName);

                        FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));
                    }
                }
            }
        }        
        

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            return AddVertexAndReturnEdge(metaVertex, destVertex.Value);
        }

        public override IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val)
        {
            if (val == null) val = "name";

            string name = val.ToString();

            while (this.Get(false, "File:'" + name+"'") != null || this.Get(false, "Directory:'" + name+"'") != null)
                name = FileSystemUtil.addNew(name);

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "Directory"))
            {
                DI.CreateSubdirectory(name);

                IVertex DirectoryVertex = new DirectoryVertex(this.Identifier + "\\" + name, this.Store);

                return base.AddEdge(metaVertex, DirectoryVertex);
            }

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "File"))
            {
                FileInfo fi = new FileInfo(this.Identifier + "\\" + name);

                fi.Create().Dispose();

                IVertex FileVertex = new FileVertex(this.Identifier + "\\" + name, this.Store);

                return base.AddEdge(metaVertex, FileVertex);
            }

            UserInteractionUtil.ShowError("FileVertex.AddVertex", Identifier + " : can not create vertex here");

            return null;
        }

        bool OutEdgesFilled = false;

        void AddMeta(IVertex metaVertex, string value)
        {
            //IVertex v = new EasyVertex(this.Store);

            IVertex v = new EasyVertex(MinusZero.Instance.TempStore);

            v.Value = value;

            base.AddEdge(metaVertex, v);
        }

        public override IList<IEdge> OutEdges
        {
            get
            {
                if (OutEdgesFilled)
                    return OutEdgesRaw;

                CanFireChangeEvent = false;                

                AddMeta(FileSystemStore.Directory_Filename, DI.Name);

                string extension = DI.Extension;

                if (extension.Length > 1)
                    extension = extension.Substring(1);

                AddMeta(FileSystemStore.Directory_Extension, extension);
                
                AddMeta(FileSystemStore.Directory_FullFilename, DI.FullName);
                AddMeta(FileSystemStore.Directory_FileAttribute, DI.Attributes.ToString());
                AddMeta(FileSystemStore.Directory_CreationDateTime, DI.CreationTime.ToString());
                AddMeta(FileSystemStore.Directory_UpdateDateTime, DI.LastWriteTime.ToString());
                AddMeta(FileSystemStore.Directory_ReadDateTime, DI.LastAccessTime.ToString());
                

                IVertex FileMetaVertex= FileSystemStore.Directory_File;

                IVertex DirectoryMetaVertex= FileSystemStore.Directory;

                try{
                    foreach (FileSystemInfo fsi in DI.EnumerateFileSystemInfos())                
                    {
                    
                        if (fsi is DirectoryInfo)
                        {
                            IVertex DirectoryVertex = new DirectoryVertex(fsi.FullName, this.Store);

                            base.AddEdge(DirectoryMetaVertex, DirectoryVertex);
                        }

                        if (fsi is FileInfo)
                        {
                            IVertex FileVertex = new FileVertex(fsi.FullName, this.Store);

                            base.AddEdge(FileMetaVertex, FileVertex);
                        }
                   
                    }
                }
                catch (Exception e) { } // no access


                CanFireChangeEvent = true;                
                OutEdgesFilled = true;

                return OutEdgesRaw;
            }
        }

        public override IVertex AddVertex(IVertex metaVertex, object val)
        {
            return AddVertexAndReturnEdge(metaVertex, val).To;
        }

        public override void DeleteEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value,"File"))
            {
                UserInteractionUtil.ShowError(Identifier + " file", "tried to delete");
                return;// not sure if there will be not unwanted file deletion

                FileInfo fi= new FileInfo(Identifier + "\\" + edge.To.Value);

                fi.Delete();
            }

            if (GeneralUtil.CompareStrings(edge.Meta.Value, "Directory"))
            {
                UserInteractionUtil.ShowError(Identifier + " directory", "tried to delete");
                return;// not sure if there will be not unwanted file deletion

                DirectoryInfo di = new DirectoryInfo(Identifier + "\\" + edge.To.Value);

                di.Delete();
            }
            
            base.DeleteEdge(edge);
        }

        string getFileName(string name)
        {
            if (name.Length == 3 && name[1]==':' && name[2]=='\\')
                return name[0].ToString();

            return name;
        }

        public DirectoryVertex(string identifier,IStore store)
            : base(store)
        {
            _Identifier = identifier;

            DI = new DirectoryInfo(Identifier.ToString());

            FileSystemStore.DirectoryVertexDictionary.Add(identifier, this);
        }
    }
}
