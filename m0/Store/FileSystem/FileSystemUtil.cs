using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Store.FileSystem
{
    public class FileSystemUtil
    {
        public static string getExtension(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.'));
        }

        public static string getFileName(string fileName)
        {
            return fileName.Substring(0,fileName.LastIndexOf('.'));
        }
    }
}
