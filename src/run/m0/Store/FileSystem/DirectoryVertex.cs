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
                string identifierString =
                    Identifier as string ?? "";

                if (FileSystemUtil.IsWindowsDriveRoot(identifierString))
                    return identifierString[0].ToString();

                if (DI != null)
                    return DI.Name;

                return FileSystemUtil.GetFileNamePart(identifierString);
            }
            set
            {
                if (!(value is string valueString))
                    return;

                string newDirectoryName =
                    FileSystemUtil.GetFileNamePart(valueString);

                if (string.IsNullOrWhiteSpace(newDirectoryName))
                    return;

                string parentDirectoryName =
                    DI.FullName.Substring(
                        0,
                        DI.FullName.LastIndexOf(
                            Path.DirectorySeparatorChar));
                string newIdentifier =
                    parentDirectoryName +
                    Path.DirectorySeparatorChar +
                    newDirectoryName.Trim();

                if (newIdentifier[newIdentifier.Length - 1] == '.')
                    newIdentifier = newIdentifier.Substring(
                        0,
                        newIdentifier.Length - 1);

                if (newIdentifier != DI.FullName)
                    throw new NotSupportedException(
                        "Directory rename is not implemented.");
            }
        }

        public override void Refresh()
        {
            OutEdgesDictionariesNeedsRebuild = true;
            FileSystemVertex.DeleteAllEdges();

            AddVertexToFileSystemVertex(MinusZero.Instance.Is, FileSystemStore.Directory);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_Filename, DI.Name);

            GetDirectoryBasenameAndExtension(
                DI.Name,
                out string basename,
                out string extension);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_Extension, extension);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_FullFilename, DI.FullName);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_Basename, basename);

            AddVertexToFileSystemVertex(FileSystemStore.Directory_FileAttribute, DI.Attributes.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_CreationDateTime, DI.CreationTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_UpdateDateTime, DI.LastWriteTime.ToString());
            AddVertexToFileSystemVertex(FileSystemStore.Directory_ReadDateTime, DI.LastAccessTime.ToString());            

            IVertex FileMetaVertex = FileSystemStore.Directory_File;

            IVertex DirectoryMetaVertex = FileSystemStore.Directory;

            try
            {
                List<DirectoryInfo> childDirectories =
                    DI.EnumerateDirectories()
                        .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                List<FileInfo> childFiles =
                    DI.EnumerateFiles()
                        .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                foreach (DirectoryInfo directoryInfo in childDirectories)
                    AddChildFileSystemVertex(
                        DirectoryMetaVertex,
                        directoryInfo.FullName);

                foreach (FileInfo fileInfo in childFiles)
                    AddChildFileSystemVertex(
                        FileMetaVertex,
                        fileInfo.FullName);
            }
            catch (Exception)
            {
            }
        }

        void AddChildFileSystemVertex(
            IVertex metaVertex,
            string childPath)
        {
            try
            {
                IVertex childVertex =
                    this.Store.GetVertexByIdentifier(childPath);

                if (childVertex == null)
                    return;

                base.AddEdge(metaVertex, childVertex);
            }
            catch (Exception)
            {
            }
        }

        static void GetDirectoryBasenameAndExtension(
            string directoryName,
            out string basename,
            out string extension)
        {
            basename = directoryName;
            extension = "";

            if (string.IsNullOrEmpty(directoryName))
                return;

            int lastDotIndex = directoryName.LastIndexOf('.');
            if (lastDotIndex <= 0 ||
                lastDotIndex == directoryName.Length - 1)
                return;

            if (directoryName.IndexOf('.') != lastDotIndex)
                return;

            basename = directoryName.Substring(0, lastDotIndex);
            extension = directoryName.Substring(lastDotIndex + 1);
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

                string childDirectoryPath =
                    Path.Combine(Identifier.ToString(), name);
                IVertex childDirectoryVertex =
                    this.Store.GetVertexByIdentifier(childDirectoryPath);

                if (childDirectoryVertex == null)
                    return null;

                return base.AddEdge(metaVertex, childDirectoryVertex);
            }

            if (GraphUtil.GetValueAndCompareStrings(metaVertex, "File"))
            {
                string childFilePath =
                    Path.Combine(Identifier.ToString(), name);
                FileInfo fi = new FileInfo(childFilePath);

                fi.Create().Dispose();

                IVertex childFileVertex =
                    this.Store.GetVertexByIdentifier(childFilePath);

                if (childFileVertex == null)
                    return null;

                return base.AddEdge(metaVertex, childFileVertex);
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
            : base(
                store,
                FileSystemUtil.NormalizeFileSystemIdentifier(identifier))
        {
            DI = new DirectoryInfo(Identifier.ToString());
        }
    }
}
