using m0.Util;
using m0.ZeroCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            string numberInBrackets = GeneralUtil.GetRegexpEXTRACT(fileNamePart, ".(?<EXTRACT>).");

            if (numberInBrackets != null)
            {

            }else

            if(extension=="")
                return pathPart + fileNamePart + "(" + numberInBrackets + ")";
            else
                return pathPart + fileNamePart + "(" + numberInBrackets + ")" + extension;
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
