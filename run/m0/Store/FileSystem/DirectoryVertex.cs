using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;
using m0.Util;
using m0.Graph.ExecutionFlow;

namespace m0.Store.FileSystem
{
    public class DirectoryVertex: AbstractFileSystemVertex
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
                object oldValue;

                if (value is string)
                {
                    oldValue = _Value;

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

        protected override void UpdateFileSystemVertex()
        {           
            AddVertexToFileSystemVertex(FileSystemStore.Directory_Filename, DI.Name);

            string extension = DI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_Extension, extension);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_FullFilename, DI.FullName);
            AddVertexToFileSystemVertex(FileSystemStore.Directory_FileAttribute, DI.Attributes.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_CreationDateTime, DI.CreationTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_UpdateDateTime, DI.LastWriteTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_ReadDateTime, DI.LastAccessTime.ToString());            

            IVertex FileMetaVertex = FileSystemStore.Directory_File;

            IVertex DirectoryMetaVertex = FileSystemStore.Directory;

            try
            {
                foreach (FileSystemInfo fsi in DI.EnumerateFileSystemInfos())
                {

                    if (fsi is DirectoryInfo)
                    {
                        IVertex DirectoryVertex = new DirectoryVertex(this.Store, fsi.FullName);

                        //base.AddEdge(DirectoryMetaVertex, DirectoryVertex);
                        AddVertexToFileSystemVertex(DirectoryMetaVertex, DirectoryVertex);
                    }

                    if (fsi is FileInfo)
                    {
                        IVertex FileVertex = new FileVertex(this.Store, fsi.FullName);                        

                        //base.AddEdge(FileMetaVertex, FileVertex);
                        AddVertexToFileSystemVertex(FileMetaVertex, FileVertex);
                    }

                }
            }
            catch (Exception e) { } // no access
        }

        public override IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val)
        {
            if(!GraphUtil.GetValueAndCompareStrings(metaVertex, "Directory") 
                && !GraphUtil.GetValueAndCompareStrings(metaVertex, "File"))
                return base.AddVertexAndReturnEdge(metaVertex, val);

            if (val == null) val = "name";

            string name = val.ToString();

            while (this.Get(false, "File:'" + name+"'") != null || this.Get(false, "Directory:'" + name+"'") != null)
                name = FileSystemUtil.addNew(name);

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "Directory"))
            {
                DI.CreateSubdirectory(name);

                IVertex DirectoryVertex = new DirectoryVertex(this.Store, this.Identifier + "\\" + name);

                return base.AddEdge(metaVertex, DirectoryVertex);
            }

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "File"))
            {
                FileInfo fi = new FileInfo(this.Identifier + "\\" + name);

                fi.Create().Dispose();

                IVertex FileVertex = new FileVertex(this.Store, this.Identifier + "\\" + name);

                return base.AddEdge(metaVertex, FileVertex);
            }

            return null;
        }        


        public override void DeleteEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value,"File"))
            {
                UserInteractionUtil.ShowError(Identifier + " file", "tried to delete file");
                return;// not sure if there will be not unwanted file deletion

                FileInfo fi= new FileInfo(Identifier + "\\" + edge.To.Value);

                fi.Delete();
            }

            if (GeneralUtil.CompareStrings(edge.Meta.Value, "Directory"))
            {
                UserInteractionUtil.ShowError(Identifier + " directory", "tried to delete directory");
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

        public DirectoryVertex(IStore store, string identifier)
            : base(store, identifier)
        {
            _Identifier = identifier;

            DI = new DirectoryInfo(Identifier.ToString());

            FileSystemStore.DirectoryVertexDictionary.Add(identifier, this);            
        }
    }
}
