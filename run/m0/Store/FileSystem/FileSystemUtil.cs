using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace m0.Store.FileSystem
{
    public class FileSystemUtil
    {
        public static IVertex GetDirectoryFromFileSystem(string path)
        {
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            
            if (Directory.Exists(path))
            {
                string fullPath = Path.GetFullPath(path);
                string[] directories = fullPath.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                IVertex vertex = MinusZero.Instance.root;

                foreach (string directory in directories)
                {
                    if (vertex == null)
                        return null;

                    string query = directory;

                    if (query.EndsWith(":"))
                        query = query[0].ToString();

                    IVertex nextVertex = GraphUtil.GetQueryOutFirst(vertex, null, query);

                    if (nextVertex == null && isWindows)
                        nextVertex = GetQueryOutFirstCaseInsensitive(vertex, query);

                    if (nextVertex == null)
                        return null;

                    vertex = nextVertex;
                }

                return vertex;
            }
            
            return null;
        }

        private static IVertex GetQueryOutFirstCaseInsensitive(IVertex baseVertex, string value)
        {
            if (baseVertex == null || value == null)
                return null;

            foreach (IEdge edge in baseVertex.OutEdges)
            {
                if (edge?.To?.Value is string toValue &&
                    string.Equals(toValue, value, StringComparison.OrdinalIgnoreCase))
                    return edge.To;
            }

            return null;
        }

        public static void CreateDirectoryIfNotExist(string baseDirectory, string toBePossiblyCreatedDirectory)
        {
            string fullPath = Path.Combine(baseDirectory, toBePossiblyCreatedDirectory);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public static string AddNew(string fileName)
        {
            string pathPart = GetPathPart(fileName);
            string fileNamePart = GetFileName(fileName);
            string extension = GetExtension(fileName);

            string pre, num;

            GetPreNumFromFileNamePart(fileNamePart, out pre, out num);

            if (num != null)
            {
                int? numParsed = Int32.Parse(num);

                if (numParsed == null)
                    numParsed = 1;
                else
                    numParsed++;

                if (extension == "")
                    return pathPart + pre + "(" + numParsed + ")";
                else
                    return pathPart + pre + "(" + numParsed + ")." + extension;

            }
            else
            {
                if (extension == "")
                    return pathPart + fileNamePart + "(1)";
                else
                    return pathPart + fileNamePart + "(1)." + extension;
            }           
        }

        private static void GetPreNumFromFileNamePart(string fileNamePart, out string pre, out string num)
        {
            Regex rgx = new Regex("(?<PRE>.+)[(](?<NUM>\\d+)[)]");

            pre = null;
            num = null;

            foreach (Match match in rgx.Matches(fileNamePart))
            {
                pre = match.Groups["PRE"].Value;
                num = match.Groups["NUM"].Value;
            }
        }

        public static string GetPathPart(string fileName)
        {
            int slashpos = fileName.LastIndexOf(Path.DirectorySeparatorChar);

            if (slashpos == -1)
                return "";

            return fileName.Substring(0, slashpos+1);
        }

        public static string GetFileNamePart(string fileName)
        {
            int slashpos = fileName.LastIndexOf(Path.DirectorySeparatorChar);

            if (slashpos == -1)
                return fileName;

            return fileName.Substring(slashpos + 1);
        }

        public static string GetExtension(string fileName)
        {
            string fileNamePart = GetFileNamePart(fileName);

            int dotpos = fileNamePart.LastIndexOf('.');

            if (dotpos == -1)
                return "";

            return fileNamePart.Substring(dotpos+1);
        }

        public static string GetFileName(string fileName)
        {
            string fileNamePart = GetFileNamePart(fileName);

            int dotpos = fileNamePart.LastIndexOf('.');

            if (dotpos == -1)
                return fileNamePart;

            return fileNamePart.Substring(0, dotpos);
        }
    }
}
