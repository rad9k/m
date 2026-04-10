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

                return FileSystemUtil.GetFileNamePart((string)Identifier);
            }
            set
            {
                object oldValue;

                if (value is string)
                {
                    oldValue = _Value;

                    string newFileName = FileSystemUtil.GetFileNamePart((string)value);

                    if (newFileName == "")
                        return;
                    
                    string DI_DirectoryName = DI.FullName.Substring(0, DI.FullName.LastIndexOf(Path.DirectorySeparatorChar));

                    newFileName = DI_DirectoryName + Path.DirectorySeparatorChar + newFileName.Trim();

                    if (newFileName[newFileName.Length - 1] == '.')
                        newFileName = newFileName.Substring(0, newFileName.Length - 1);

                    if (newFileName != DI.FullName)
                    {
                        while (System.IO.Directory.Exists(newFileName) || System.IO.Directory.Exists(newFileName))
                            newFileName = FileSystemUtil.AddNew(newFileName);

                        _Identifier = newFileName;

                        //System.IO.Directory.Move(DI.FullName, newFileName);
                        throw new Exception("trying to rename directory, not implemented yet");

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

        public override void Refresh()
        {
            FileSystemVertex.DeleteAllEdges();

            AddVertexToFileSystemVertex(MinusZero.Instance.Is, FileSystemStore.Directory);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_Filename, DI.Name);

            string extension = DI.Extension;

            if (extension.Length > 1)
                extension = extension.Substring(1);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_Extension, extension);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_FullFilename, DI.FullName);

            if (DI.Name.Contains("."))
                AddVertexToFileSystemVertex(FileSystemStore.Directory_Basename, DI.Name.Substring(0, DI.Name.LastIndexOf(".")));
            else
                AddVertexToFileSystemVertex(FileSystemStore.Directory_Basename, DI.Name);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_FileAttribute, DI.Attributes.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_CreationDateTime, DI.CreationTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_UpdateDateTime, DI.LastWriteTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_ReadDateTime, DI.LastAccessTime.ToString());            

            IVertex FileMetaVertex = FileSystemStore.Directory_File;

            IVertex DirectoryMetaVertex = FileSystemStore.Directory;

            try
            {
                foreach (DirectoryInfo directoryInfo in DI.EnumerateDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
                {
                    IVertex DirectoryVertex = new DirectoryVertex(this.Store, directoryInfo.FullName);

                    //base.AddEdge(DirectoryMetaVertex, DirectoryVertex);
                    AddVertexToFileSystemVertex(DirectoryMetaVertex, DirectoryVertex);
                }

                foreach (FileInfo fileInfo in DI.EnumerateFiles().OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
                {
                    IVertex FileVertex = new FileVertex(this.Store, fileInfo.FullName);                        

                    //base.AddEdge(FileMetaVertex, FileVertex);
                    AddVertexToFileSystemVertex(FileMetaVertex, FileVertex);
                }
            }
            catch (Exception) { } // no access
        }

        public override IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val)
        {
            if(!GraphUtil.GetValueAndCompareStrings(metaVertex, "Directory") 
                && !GraphUtil.GetValueAndCompareStrings(metaVertex, "File"))
                return base.AddVertexAndReturnEdge(metaVertex, val);

            if (val == null) val = "name";

            string name = val.ToString();

            while (this.Get(false, "File:'" + name+"'") != null || this.Get(false, "Directory:'" + name+"'") != null)
                name = FileSystemUtil.AddNew(name);

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "Directory"))
            {
                DI.CreateSubdirectory(name);

                IVertex DirectoryVertex = new DirectoryVertex(this.Store, this.Identifier + "\\" + name);

                return base.AddEdge(metaVertex, DirectoryVertex);
            }

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "File"))
            {
                FileInfo fi = new FileInfo(this.Identifier.ToString() + Path.DirectorySeparatorChar + name);

                fi.Create().Dispose();

                IVertex FileVertex = new FileVertex(this.Store, this.Identifier.ToString() + Path.DirectorySeparatorChar + name);

                return base.AddEdge(metaVertex, FileVertex);
            }

            return null;
        }        


        public override void DeleteEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value,"File"))
            {
                UserInteractionUtil.ShowException(Identifier + " file", "tried to delete file", ZeroTypes.ExceptionLevelEnum.Warning);
                return;// not sure if there will be not unwanted file deletion

                FileInfo fi= new FileInfo(Identifier.ToString() + Path.DirectorySeparatorChar + edge.To.Value);

                fi.Delete();
            }

            if (GeneralUtil.CompareStrings(edge.Meta.Value, "Directory"))
            {
                UserInteractionUtil.ShowException(Identifier + " directory", "tried to delete directory", ZeroTypes.ExceptionLevelEnum.Warning);
                return;// not sure if there will be not unwanted file deletion

                DirectoryInfo di = new DirectoryInfo(Identifier.ToString() + Path.DirectorySeparatorChar + edge.To.Value);

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
