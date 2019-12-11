using m0.Util;
using m0.ZeroCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace m0.Store.FileSystem
{
    public class FileSystemUtil
    {
        public static string addNew(string fileName)
        {
            string pathPart = getPathPart(fileName);
            string fileNamePart = getFileName(fileName);
            string extension = getExtension(fileName);

            string pre, num;

            getPreNumFromFileNamePart(fileNamePart, out pre, out num);

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

        private static void getPreNumFromFileNamePart(string fileNamePart, out string pre, out string num)
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

        public static string getPathPart(string fileName)
        {
            int slashpos = fileName.LastIndexOf('\\');

            if (slashpos == -1)
                return "";

            return fileName.Substring(0, slashpos+1);
        }

        public static string getFileNamePart(string fileName)
        {
            int slashpos = fileName.LastIndexOf('\\');

            if (slashpos == -1)
                return fileName;

            return fileName.Substring(slashpos + 1);
        }

        public static string getExtension(string fileName)
        {
            string fileNamePart = getFileNamePart(fileName);

            int dotpos = fileNamePart.LastIndexOf('.');

            if (dotpos == -1)
                return "";

            return fileNamePart.Substring(dotpos+1);
        }

        public static string getFileName(string fileName)
        {
            string fileNamePart = getFileNamePart(fileName);

            int dotpos = fileNamePart.LastIndexOf('.');

            if (dotpos == -1)
                return fileNamePart;

            return fileNamePart.Substring(0, dotpos);
        }
    }
}
