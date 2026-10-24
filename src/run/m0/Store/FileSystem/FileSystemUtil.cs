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
        public const string StartIdentifierPrefix = "Start:/";

        public static bool IsStartIdentifier(string identifier)
        {
            return !string.IsNullOrEmpty(identifier) &&
                identifier.StartsWith("Start:", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCanonicalFileIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return identifier;

            if (IsForeignWindowsAbsolutePath(identifier))
                return identifier.Replace('\\', '/');

            string physicalFileName = GetPhysicalFileName(identifier);
            string applicationPath = GetNormalizedApplicationPath();

            if (applicationPath != null &&
                IsPathInsideOrEqualTo(physicalFileName, applicationPath))
            {
                string relativePath = Path.GetRelativePath(
                    applicationPath,
                    physicalFileName);

                if (relativePath == ".")
                    return StartIdentifierPrefix;

                return StartIdentifierPrefix +
                    relativePath
                        .Replace(Path.DirectorySeparatorChar, '/')
                        .Replace(Path.AltDirectorySeparatorChar, '/')
                        .TrimStart('/');
            }

            return NormalizeFileSystemIdentifier(physicalFileName);
        }

        public static string GetPhysicalFileName(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return identifier;

            if (IsStartIdentifier(identifier))
            {
                string applicationPath = GetNormalizedApplicationPath();

                if (applicationPath == null)
                    throw new InvalidOperationException(
                        "ApplicationPath must be initialized before resolving a Start:/ identifier.");

                string relativePortablePath =
                    identifier.Substring("Start:".Length)
                        .Replace('\\', '/')
                        .TrimStart('/');

                string relativePhysicalPath =
                    relativePortablePath.Replace(
                        '/',
                        Path.DirectorySeparatorChar);

                string physicalFileName = Path.GetFullPath(
                    Path.Combine(
                        applicationPath,
                        relativePhysicalPath));

                if (!IsPathInsideOrEqualTo(
                    physicalFileName,
                    applicationPath))
                {
                    throw new ArgumentException(
                        "A Start:/ identifier cannot point outside ApplicationPath.",
                        nameof(identifier));
                }

                return NormalizeFileSystemIdentifier(
                    physicalFileName);
            }

            if (IsForeignWindowsAbsolutePath(identifier))
                return identifier;

            if (Path.IsPathRooted(identifier))
                return NormalizeFileSystemIdentifier(identifier);

            string basePath = GetNormalizedApplicationPath();

            if (basePath == null)
                return NormalizeFileSystemIdentifier(identifier);

            return NormalizeFileSystemIdentifier(
                Path.Combine(basePath, identifier));
        }

        public static IStore GetFileStore(
            IStoreUniverse storeUniverse,
            string storeTypeName,
            string identifier)
        {
            if (storeUniverse == null)
                throw new ArgumentNullException(nameof(storeUniverse));

            Type storeType = Type.GetType(storeTypeName);

            if (storeType == null ||
                !typeof(IFileSystemIdentifiedStore)
                    .IsAssignableFrom(storeType))
            {
                throw new ArgumentException(
                    "The requested store type is not file-system identified.",
                    nameof(storeTypeName));
            }

            return storeUniverse.GetStore(
                storeTypeName,
                GetCanonicalFileIdentifier(identifier));
        }

        public static bool IsFileSystemIdentifiedStoreType(
            string storeTypeName)
        {
            Type storeType = Type.GetType(storeTypeName);

            return storeType != null &&
                typeof(IFileSystemIdentifiedStore)
                    .IsAssignableFrom(storeType);
        }

        public static bool IsFileSystemStoreType(
            string storeTypeName)
        {
            Type storeType = Type.GetType(storeTypeName);

            return storeType != null &&
                typeof(FileSystemStore).IsAssignableFrom(storeType);
        }

        public static void GetCanonicalStoreReference(
            string storeTypeName,
            string storeIdentifier,
            object vertexIdentifier,
            out string canonicalStoreIdentifier,
            out object canonicalVertexIdentifier)
        {
            canonicalStoreIdentifier = storeIdentifier;
            canonicalVertexIdentifier = vertexIdentifier;

            if (!IsFileSystemIdentifiedStoreType(storeTypeName))
                return;

            if (IsFileSystemStoreType(storeTypeName) &&
                vertexIdentifier is string fileSystemVertexIdentifier)
            {
                string canonicalFileSystemVertexIdentifier =
                    GetCanonicalFileIdentifier(
                        fileSystemVertexIdentifier);

                canonicalVertexIdentifier =
                    canonicalFileSystemVertexIdentifier;

                if (IsStartIdentifier(
                    canonicalFileSystemVertexIdentifier))
                {
                    canonicalStoreIdentifier =
                        StartIdentifierPrefix;
                    return;
                }
            }

            canonicalStoreIdentifier =
                GetCanonicalFileIdentifier(storeIdentifier);
        }

        private static string GetNormalizedApplicationPath()
        {
            string applicationPath =
                MinusZero.Instance?.ApplicationPath;

            if (string.IsNullOrWhiteSpace(applicationPath))
                return null;

            return NormalizeFileSystemIdentifier(applicationPath);
        }

        private static bool IsPathInsideOrEqualTo(
            string path,
            string parentPath)
        {
            string normalizedPath =
                NormalizeFileSystemIdentifier(path);
            string normalizedParentPath =
                NormalizeFileSystemIdentifier(parentPath);

            StringComparison comparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            if (string.Equals(
                normalizedPath,
                normalizedParentPath,
                comparison))
            {
                return true;
            }

            string parentWithSeparator =
                normalizedParentPath.EndsWith(
                    Path.DirectorySeparatorChar)
                    ? normalizedParentPath
                    : normalizedParentPath +
                        Path.DirectorySeparatorChar;

            return normalizedPath.StartsWith(
                parentWithSeparator,
                comparison);
        }

        private static bool IsForeignWindowsAbsolutePath(
            string path)
        {
            return !OperatingSystem.IsWindows() &&
                !string.IsNullOrEmpty(path) &&
                path.Length >= 3 &&
                char.IsLetter(path[0]) &&
                path[1] == ':' &&
                (path[2] == '\\' || path[2] == '/');
        }

        public static string NormalizeFileSystemIdentifier(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                fullPath = path;
            }

            if (IsWindowsDriveRoot(fullPath))
                return fullPath;

            if (fullPath == "/" || fullPath == "\\")
                return fullPath;

            return fullPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        }

        public static bool IsWindowsDriveRoot(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length != 3)
                return false;

            return char.IsLetter(path[0]) &&
                path[1] == ':' &&
                (path[2] == '\\' || path[2] == '/');
        }

        public static IVertex GetDirectoryFromFileSystem(string path)
        {
            string physicalPath = GetPhysicalFileName(path);

            if (string.IsNullOrWhiteSpace(physicalPath) ||
                !Directory.Exists(physicalPath))
                return null;

            string fullPath =
                NormalizeFileSystemIdentifier(physicalPath);

            IStore bestStore = FindBestMatchingFileSystemStore(fullPath);

            if (bestStore == null)
                return null;

            IVertex directVertex = bestStore.GetVertexByIdentifier(fullPath);

            if (directVertex != null)
                return directVertex;

            IVertex vertex = bestStore.Root;

            if (vertex == null)
                return null;

            string storeId = GetPhysicalFileName(
                bestStore.Identifier).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
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

        public static IVertex GetFileSystemVertex(
            string path)
        {
            string physicalPath = GetPhysicalFileName(path);

            if (string.IsNullOrWhiteSpace(physicalPath))
                return null;

            string fullPath =
                NormalizeFileSystemIdentifier(physicalPath);
            IStore bestStore =
                FindBestMatchingFileSystemStore(fullPath);

            return bestStore?.GetVertexByIdentifier(
                GetCanonicalFileIdentifier(fullPath));
        }

        internal static IStore FindBestMatchingFileSystemStore(string fullPath)
        {
            string fullPathNormalized = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            StringComparison comparison = IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            IStore bestStore = null;
            int bestLength = -1;

            foreach (IStore store in MinusZero.Instance.Stores)
            {
                if (!(store is FileSystemStore))
                    continue;

                string physicalStoreIdentifier =
                    GetPhysicalFileName(store.Identifier);
                string storeId = physicalStoreIdentifier.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

                bool isMatch = fullPathNormalized.Equals(storeId, comparison)
                    || (fullPathNormalized.StartsWith(storeId, comparison)
                        && fullPathNormalized.Length > storeId.Length
                        && (fullPathNormalized[storeId.Length] == Path.DirectorySeparatorChar
                            || fullPathNormalized[storeId.Length] == Path.AltDirectorySeparatorChar));

                if (storeId.Length == 0 &&
                    physicalStoreIdentifier == "/")
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
