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
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return null;

            string fullPath = Path.GetFullPath(path);

            IStore bestStore = FindBestMatchingFileSystemStore(fullPath);

            if (bestStore == null)
                return null;

            IVertex directVertex = bestStore.GetVertexByIdentifier(fullPath);

            if (directVertex != null)
                return directVertex;

            IVertex vertex = bestStore.Root;

            if (vertex == null)
                return null;

            string storeId = bestStore.Identifier.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string pathTrimmed = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string relativePath = pathTrimmed.Length > storeId.Length
                ? pathTrimmed.Substring(storeId.Length)
                : "";

            if (string.IsNullOrEmpty(relativePath))
                return vertex;

            string[] directories = relativePath.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string directory in directories)
            {
                IVertex nextVertex = GraphUtil.GetQueryOutFirst(vertex, null, directory);

                if (nextVertex == null && IsWindows())
                    nextVertex = GetQueryOutFirstCaseInsensitive(vertex, directory);

                if (nextVertex == null)
                    return null;

                vertex = nextVertex;
            }

            return vertex;
        }

        private static IStore FindBestMatchingFileSystemStore(string fullPath)
        {
            string fullPathNormalized = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            StringComparison comparison = IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            IStore bestStore = null;
            int bestLength = -1;

            foreach (IStore store in MinusZero.Instance.Stores)
            {
                if (!(store is FileSystemStore))
                    continue;

                string storeId = store.Identifier.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                bool isMatch = fullPathNormalized.Equals(storeId, comparison)
                    || (fullPathNormalized.StartsWith(storeId, comparison)
                        && fullPathNormalized.Length > storeId.Length
                        && (fullPathNormalized[storeId.Length] == Path.DirectorySeparatorChar
                            || fullPathNormalized[storeId.Length] == Path.AltDirectorySeparatorChar));

                if (storeId.Length == 0 && store.Identifier == "/")
                    isMatch = fullPathNormalized.StartsWith("/");

                if (isMatch && storeId.Length > bestLength)
                {
                    bestStore = store;
                    bestLength = storeId.Length;
                }
            }

            return bestStore;
        }

        private static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
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
