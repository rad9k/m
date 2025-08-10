using m0.Foundation;
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
