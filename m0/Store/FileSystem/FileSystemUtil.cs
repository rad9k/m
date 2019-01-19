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
            
        }

        public static string getExtension(string fileName)
        {
            int dotpos = fileName.LastIndexOf('.');

            if (dotpos == -1)
                return "";

            int slashpos = fileName.LastIndexOf('\\');

            if (slashpos == -1)
                return fileName.Substring(dotpos+1);

            fileName = fileName.Substring(slashpos);

            dotpos = fileName.LastIndexOf('.');

            if (dotpos == -1)
                return "";

            return fileName.Substring(dotpos);
        }

        public static string getFileName(string fileName)
        {
            return fileName.Substring(0,fileName.LastIndexOf('.'));
        }
    }
}
