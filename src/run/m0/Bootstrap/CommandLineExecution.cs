using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;
using m0.Util;
using m0.ZeroTypes;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Bootstrap
{
    public class CommandLineExecution
    {
        static IVertex root = MinusZero.Instance.root;        

        public static string GetFilePath(string userName)
        {
            string filename = $"home-{userName}.m0j";
            string filePath = Path.Combine(MinusZero.Instance.ApplicationPath, filename);

            return filePath;
        }

        static string GetFileName(string userName)
        {
            return $"home-{userName}.m0j";                        
        }

        public static void RunUser_BeforeInitialisation(string userName)
        {
            string filePath = GetFilePath(userName);

            bool fileDoesExist = File.Exists(filePath);
            bool fileIsEmpty = fileDoesExist && new FileInfo(filePath).Length == 0;

            if (fileDoesExist && fileIsEmpty)
                File.Delete(filePath);
        }

        public static void RunUser(string userName, bool createIfNotExist)
        {
            string filePath = GetFilePath(userName);

            bool fileDoesNotExist = !File.Exists(filePath);
            bool fileIsEmpty = !fileDoesNotExist && new FileInfo(filePath).Length == 0;

            if (createIfNotExist && (fileDoesNotExist || fileIsEmpty))
                User.CreateUser(userName);

            if (!File.Exists(filePath))
            {
                UserInteractionUtil.ShowException("CommandLineExecution", $"User {userName} not found.", ExceptionLevelEnum.Fatal);
                MinusZero.Instance.Fatal();
            }
            

            IVertex startVertex = GraphUtil.GetQueryOutFirst(root, "Start", null);
            IVertex homeFileVertex = GraphUtil.GetQueryOutFirst(startVertex, "File", GetFileName(userName));
            IVertex homeVertex = GraphUtil.GetQueryOutFirst(homeFileVertex, "$Store", null);

            root.AddEdge(root.Get(false, @"System\Meta\Base\Home"), homeVertex);

            IVertex currentUser = GraphUtil.GetQueryOutFirst(homeVertex, "CurrentUser", null);
            IVertex defaultFormalTextLanguage = GraphUtil.GetQueryOutFirst(currentUser, "DefaultFormalTextLanguage", null);

            MinusZero.Instance._DefaultFormalTextLanguage = defaultFormalTextLanguage;
        }

        


    }
}
