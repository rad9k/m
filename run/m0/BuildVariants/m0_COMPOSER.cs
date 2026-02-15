using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace m0.BuildVariants
{
    public class m0_COMPOSER
    {
        public static void RuntimeInitialize()
        {            
            IVertex r = MinusZero.Instance.Root;

            IVertex directory_Vertex = r.Get(false, @"System\Meta\Store\FileSystem\Directory");

            IVertex onNewMusicSpaceStore_Vertex = r.Get(false, @"System\Lib\Music\UserCommands\OnNewMusicSpaceStore");

            directory_Vertex.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\UserCommand"), onNewMusicSpaceStore_Vertex);
        }
    }
}
