using m0.Foundation;
using m0.Graph;
using m0.Store.FileSystem;
using m0.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Bootstrap
{
    public class FileSystemLoader
    {
        static IVertex packageMeta = m0.MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroUML\Package");
        static IVertex isMeta = m0.MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$Is");

        string startPath;

        public FileSystemLoader(string _startPath)
        {
            startPath = _startPath;
        }

        public void Load(IVertex baseVertex)
        {
            Load_Reccursive(baseVertex, startPath);
        }

        void Load_Reccursive(IVertex baseVertex, string directoryPath)
        {
            try
            {
                // Pobierz wszystkie pliki w bieżącym katalogu
                string[] files = Directory.GetFiles(directoryPath, "*.m0t");

                // Uruchom funkcję f dla każdego pliku
                foreach (string file in files)
                {
                    string vertexValue = Path.GetFileNameWithoutExtension(file);
                    IVertex fileVertex = baseVertex.AddVertex(packageMeta, vertexValue);

                    fileVertex.AddEdge(isMeta, packageMeta);
                    
                    ProcessFile(fileVertex, file);
                }

                // Pobierz wszystkie podkatalogi
                string[] subdirectories = Directory.GetDirectories(directoryPath);

                // Rekurencyjnie przetwórz każdy podkatalog
                foreach (string subdirectory in subdirectories)
                {
                    IVertex directoryVertex = baseVertex.AddVertex(packageMeta, Path.GetFileName(subdirectory));

                    directoryVertex.AddEdge(isMeta, packageMeta);

                    Load_Reccursive(directoryVertex, subdirectory);
                }
            }
            catch (Exception e)
            {
                UserInteractionUtil.ShowError("FilesystemLoader", "Error while loading files.");
            }
        }

        void ProcessFile(IVertex baseVertex, string fileName)
        {
            GraphUtil.LoadAndParse(fileName, baseVertex);
        }

    }
}
